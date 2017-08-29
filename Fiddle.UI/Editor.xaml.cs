using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Fiddle.Compilers;
using Fiddle.UI.Annotations;
using ICSharpCode.SharpDevelop.Editor;
using MaterialDesignThemes.Wpf;

namespace Fiddle.UI {
    /// <summary>
    ///     Interaction logic for Editor.xaml
    /// </summary>
    public partial class Editor : INotifyPropertyChanged {
        public static RoutedCommand CommandSave = new RoutedCommand(); //Ctrl + S
        public static RoutedCommand CommandCompile = new RoutedCommand(); //F6
        public static RoutedCommand CommandExecute = new RoutedCommand(); //F5
        private ICompiler _compiler; //Compiler instance
        private Timer _dialogTimeout;
        private bool _dropIsOpen; //is Drag & Drop popup open?
        private string _filePath; //path to file - Ctrl + S will save without SaveFileDialog if not null
        private IEnumerable<Language> _languages;
        private bool _needsUpdate; //need to reset textbox underlines & statusmessage?
        private Language _selectedLang;
        private ITextMarkerService _textMarkerService; //underlines

        //constructor
        public Editor() {
            InitializeComponent();
            DataContext = this;
            LoadComboBox();
            LoadTextBox();
            LoadPreferences();
            LoadTextMarkerService();
            LoadHotkeys();
            TextBoxCode.Focus();
        }

        public IEnumerable<Language> Languages {
            get => _languages;
            set {
                _languages = value;
                OnPropertyChanged();
            }
        }

        public Language SelectedLanguage {
            get => _selectedLang;
            set {
                _selectedLang = value;
                OnPropertyChanged();
            }
        }

        public string SourceCode => TextBoxCode.Text;
        public event PropertyChangedEventHandler PropertyChanged; //MVVM Prop changed event

        #region Code

        //Actually compile code
        private async void Compile() {
            if (_compiler == null) {
                await DialogHelper.ShowErrorDialog("Please select a language first!", EditorDialogHost);
                return;
            }

            ClearResultView();
            SetStatus(StatusType.Wait, "Compiling..");

            LockUi();
            try {
                _compiler.SourceCode = SourceCode;
                var result = await _compiler.Compile();

                SetResultView(result);
                if (result.Success) {
                    SetStatus(StatusType.Success, "Compilation successful!", result.Time);
                } else {
                    SetStatus(StatusType.Failure, "Compilation failed!", result.Time);

                    string errors = Helper.ConcatErrors(result.Errors);
                    await DialogHelper.ShowErrorDialog($"Compilation failed!\n{errors}",
                        EditorDialogHost);
                }
            } catch (Exception ex) {
                SetStatus(StatusType.Failure, "Compilation failed!");
                await DialogHelper.ShowErrorDialog($"Could not compile! ({ex.Message})", EditorDialogHost);
            }
            CodeUnderline();

            UnlockUi();
            _needsUpdate = true;
        }

        //Actually execute code
        private async void Execute() {
            if (_compiler == null) {
                await DialogHelper.ShowErrorDialog("Please select a language first!", EditorDialogHost);
                return;
            }

            ClearResultView();
            SetStatus(StatusType.Wait, "Executing..");

            LockUi();
            try {
                _compiler.SourceCode = SourceCode;
                var result = await _compiler.Execute();

                SetResultView(result);
                if (result.Success)
                    SetStatus(StatusType.Success, "Execution successful!", result.CompileResult.Time, result.Time);
                else
                    SetStatus(StatusType.Failure, "Execution failed!", result.CompileResult.Time, result.Time);
            } catch (Exception ex) {
                SetStatus(StatusType.Failure, "Execution failed!");
                await DialogHelper.ShowErrorDialog($"Could not execute! ({ex.Message})", EditorDialogHost);
            }
            CodeUnderline();

            UnlockUi();
            _needsUpdate = true;
        }

        //Underline Errors red and warnings/infos Yellow in code
        private void CodeUnderline() {
            ResetUnderline();
            if (_compiler?.CompileResult?.Diagnostics == null || !_compiler.CompileResult.Diagnostics.Any())
                return;

            foreach (var diagnostic in _compiler.CompileResult.Diagnostics)
                try {
                    if (diagnostic.LineFrom < 1 || diagnostic.LineTo < 1 || diagnostic.CharFrom < 1 ||
                        diagnostic.CharTo < 1)
                        continue; //invalid diagnostic

                    //LineFrom/LineTo/CharFrom/CharTo -1 because it's 1-based and Lines[] expects an Index
                    int startOffset = TextBoxCode.Document.Lines[diagnostic.LineFrom - 1].Offset + diagnostic.CharFrom -
                                      1;
                    int endOffset = TextBoxCode.Document.Lines[diagnostic.LineTo - 1].Offset + diagnostic.CharTo - 1;
                    int length = endOffset - startOffset;

                    var marker = _textMarkerService.Create(startOffset, length);
                    marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
                    marker.MarkerColor = diagnostic.Severity == Severity.Error ? Colors.Red : Colors.Yellow;
                } catch {
                    // could not underline, out of bounds, etc
                }
        }

        //Remove all Red/Yellow underlinings in code
        private void ResetUnderline() {
            try {
                _textMarkerService?.RemoveAll(m => true);
            } catch {
                //could not reset underline
            }
        }

        //Select a different programming language (drop down)
        private async void ComboBoxLanguageSelected(object sender, SelectionChangedEventArgs e) {
            LockUi();
            ResetUnderline();

            try {
                _compiler?.Dispose();
                _compiler = Helper.NewCompiler(SelectedLanguage, SourceCode, this);
                if (App.Preferences.CacheType.HasFlag(CacheType.Language)) //only save if cache type saves language
                    App.Preferences.SelectedLanguage = SelectedLanguage;
                TextBoxCode.SyntaxHighlighting = Helper.LoadXshd(SelectedLanguage);

                Title = $"Fiddle - {SelectedLanguage.GetDescription()}";
                _filePath = null;
            } catch (Exception ex) {
                await DialogHelper.ShowErrorDialog($"Could not load {SelectedLanguage} compiler! ({ex.Message})",
                    EditorDialogHost);
                //Revert changes
                SelectedLanguage = App.Preferences.SelectedLanguage;
                _compiler?.Dispose();
                _compiler = Helper.NewCompiler(SelectedLanguage, SourceCode, this);
                TextBoxCode.SyntaxHighlighting = Helper.LoadXshd(SelectedLanguage);
            }
            UnlockUi();
        }

        //Text Changed
        private void TextBoxCode_DocumentChanged(object sender, EventArgs e) {
            if (!_needsUpdate) return;

            ResetStatus();
            ResetUnderline();

            _needsUpdate = false;
        }

        #endregion

        #region Helper

        //Freeze every control
        private void LockUi() {
            foreach (UIElement element in EditorGrid.Children) element.IsEnabled = false;
            Cursor = Cursors.Wait;
        }

        //Unfreeze every control
        private void UnlockUi() {
            foreach (UIElement element in EditorGrid.Children) element.IsEnabled = true;
            Cursor = Cursors.Arrow;
            TextBoxCode.Focus();
        }

        //Set Result View content
        private void SetResultView(ICompileResult result) {
            ClearResultView();
            IEnumerable<Inline> runs = Helper.BuildRuns(result);
            TextBlockResults.Inlines.AddRange(runs);
        }

        //Set Result View content
        private void SetResultView(IExecuteResult result) {
            ClearResultView();
            IEnumerable<Inline> runs = Helper.BuildRuns(result);
            TextBlockResults.Inlines.AddRange(runs);
        }

        //Clear result view
        private void ClearResultView() {
            TextBlockResults.Text = "";
        }

        //Actually save code file
        private void Save() {
            LockUi();
            try {
                if (string.IsNullOrWhiteSpace(_filePath))
                    _filePath = Helper.SaveFile(SourceCode, _compiler.Language);
                else
                    File.WriteAllText(_filePath, SourceCode);

                SetStatus(StatusType.Success, "File saved!");
            } catch (Exception ex) {
                SetStatus(StatusType.Failure, $"Could not save file! ({ex.Message})");
            }
            UnlockUi();
        }

        //Set the status bar's status message/icon
        private void SetStatus(StatusType type, string message = "Ready", long compileTime = -1, long execTime = -1) {
            switch (type) {
                case StatusType.Failure:
                    IconStatus.Kind = PackIconKind.CloseCircleOutline;
                    break;
                case StatusType.Success:
                    IconStatus.Kind = PackIconKind.CheckCircleOutline;
                    break;
                case StatusType.Wait:
                    IconStatus.Kind = PackIconKind.Sync;
                    break;
                default:
                    IconStatus.Kind = PackIconKind.CodeBraces;
                    break;
            }
            TextBlockStatusMessage.Text = message;
            if (compileTime > 0)
                TextBlockCompileTime.Text = $"C: {compileTime}ms";
            if (execTime > 0)
                TextBlockExecuteTime.Text = $"X: {execTime}ms";
        }

        //Reset the status bar's status message/icon
        private void ResetStatus() {
            SetStatus(StatusType.Idle);
        }

        private enum StatusType {
            Idle,
            Success,
            Failure,
            Wait
        }

        #endregion

        #region Events

        //Save the file to disk
        private void ButtonSave(object sender, RoutedEventArgs e) {
            _filePath = null; //Set to null again so save button opens file picker
            Save();
        }

        //Open Settings
        private void ButtonSettings(object sender, RoutedEventArgs e) {
            LockUi();
            var settings = new Settings {Owner = this};
            settings.ShowDialog();
            if (_compiler != null)
                _compiler = Helper.NewCompiler(_compiler.Language, SourceCode, this);
            UnlockUi();
        }

        //Compile Button click
        private void ButtonCompile(object sender, RoutedEventArgs e) {
            Compile();
        }

        //Execute button click
        private void ButtonExecute(object sender, RoutedEventArgs e) {
            Execute();
        }

        //Show results view raw button click
        private void ButtonShowRaw(object sender, RoutedEventArgs e) {
            var window = new RawText(TextBlockResults.Text) {Owner = this};
            window.ShowDialog();
        }

        //Show Raw Button (Mouse over)
        private void TextBlockResultsMEnter(object sender, MouseEventArgs e) {
            if (!string.IsNullOrWhiteSpace(TextBlockResults.Text)) {
                RawBtn.Visibility = Visibility.Visible;
                RawBtn.Animate(OpacityProperty, 0, 1, 200);
            }
        }

        //Hide Raw Button (Mouse leave)
        private async void TextBlockResultsMLeave(object sender, MouseEventArgs e) {
            if (!string.IsNullOrWhiteSpace(TextBlockResults.Text)) {
                await RawBtn.AnimateAsync(OpacityProperty, 1, 0, 200);
                RawBtn.Visibility = Visibility.Collapsed;
            }
        }


        //Window loaded event
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            ResetStatus();
            PopupCardScaleTransform.CenterX = PopupCard.ActualWidth / 2;
            PopupCardScaleTransform.CenterY = PopupCard.ActualHeight / 2;
        }

        //Window closes event (save preferences)
        private void Window_Closing(object sender, CancelEventArgs e) {
            SetStatus(StatusType.Wait, "Closing..");
            if (App.Preferences.CacheUserSettings) {
                var type = App.Preferences.CacheType;
                if (type == 0)
                    return;
                if (type.HasFlag(CacheType.WindowSize)) {
                    App.Preferences.WindowWidth = Width;
                    App.Preferences.WindowHeight = Height;
                }
                if (type.HasFlag(CacheType.WindowPos)) {
                    App.Preferences.WindowLeft = Left;
                    App.Preferences.WindowTop = Top;
                }
                if (type.HasFlag(CacheType.WindowState)) App.Preferences.WindowState = WindowState;
                if (type.HasFlag(CacheType.ResultsViewSize))
                    App.Preferences.ResultsViewSize = GridCodeResults.ColumnDefinitions[2].Width.Value;
                if (type.HasFlag(CacheType.SourceCode)) App.Preferences.SourceCode = SourceCode;
                if (type.HasFlag(CacheType.CursorPos)) App.Preferences.CursorOffset = TextBoxCode.TextArea.Caret.Offset;
            }
        }

        //Ctrl + S Command (Save)
        private void CtrlS(object sender, ExecutedRoutedEventArgs e) {
            Save();
        }

        //F6 Command (Compile)
        private void F6(object sender, ExecutedRoutedEventArgs e) {
            Compile();
        }

        //F5 Command (Execute)
        private void F5(object sender, ExecutedRoutedEventArgs e) {
            Execute();
        }

        //Show "Drop your files here" popup
        private async void OpenDropPopup() {
            if (_dropIsOpen) return;
            _dropIsOpen = true;

            EditorGrid.IsHitTestVisible = false;
            var brighten = EditorGrid.AnimateAsync(OpacityProperty, Opacity, 0.4, 100);
            var popupx = PopupCardScaleTransform.AnimateAsync(ScaleTransform.ScaleXProperty, 0, 1, 150);
            var popupy = PopupCardScaleTransform.AnimateAsync(ScaleTransform.ScaleYProperty, 0, 1, 150);

            await Task.WhenAll(brighten, popupx, popupy);
            PopupCard.BringIntoView();
        }

        //Hide "Drop your files here" popup
        private async void CloseDropPopup() {
            if (!_dropIsOpen) return;
            _dropIsOpen = false;
            _dialogTimeout?.Dispose();

            EditorGrid.IsHitTestVisible = true;
            var dim = EditorGrid.AnimateAsync(OpacityProperty, Opacity, 1, 100);
            var popupx =
                PopupCardScaleTransform.AnimateAsync(ScaleTransform.ScaleXProperty, PopupCardScaleTransform.ScaleX, 0,
                    150);
            var popupy =
                PopupCardScaleTransform.AnimateAsync(ScaleTransform.ScaleYProperty, PopupCardScaleTransform.ScaleY, 0,
                    150);

            await Task.WhenAll(dim, popupx, popupy);
            TextBoxCode.Focus();
        }

        //Trigger file drag enter event
        private void OnWindowDragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                _dialogTimeout?.Dispose();
                _dialogTimeout = new Timer(TimeoutDialogClose, null, 2500, Timeout.Infinite);
                OpenDropPopup();
            }
        }

        //Trigger file drag leave event
        private void OnWindowDragLeave(object sender, DragEventArgs e) {
            CloseDropPopup();
        }

        //Trigger file drag drop event
        private async void OnDragDrop(object sender, DragEventArgs e) {
            Cursor = Cursors.Wait;
            try {
                if (_compiler == null) throw new Exception("Please select a language first!");
                _compiler = await Helper.LoadDragDrop(e, this, _compiler);
                _filePath = (e.Data.GetData(DataFormats.FileDrop) as string[])?[0]; //set path for Ctrl S
                CloseDropPopup();
            } catch (Exception ex) {
                //some unknown error
                await DialogHelper.ShowErrorDialog($"Could not load file! ({ex.Message})", EditorDialogHost);
            }
            Cursor = Cursors.Arrow;
        }

        //Automatically close after Timer interval if DragLeave didn't fire
        private void TimeoutDialogClose(object state) {
            Dispatcher.Invoke(() => {
                try {
                    var mpos = PointFromScreen(Helper.GetMousePosition());
                    if (mpos.X < 0 || mpos.X > Width || mpos.Y < 0 || mpos.Y > Height)
                        CloseDropPopup();
                } catch {
                    //visual may not be there anymore, etc
                }
            });
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}