using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.SharpDevelop.Editor;

namespace ICSharpCode.AvalonEdit.AddIn {
    /// <summary>
    ///     Handles the text markers for a code editor.
    /// </summary>
    public sealed class TextMarkerService : DocumentColorizingTransformer, IBackgroundRenderer, ITextMarkerService,
        ITextViewConnect {
        private readonly TextDocument _document;
        private readonly TextSegmentCollection<TextMarker> _markers;

        public TextMarkerService(TextDocument document) {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _markers = new TextSegmentCollection<TextMarker>(document);
        }

        #region DocumentColorizingTransformer

        protected override void ColorizeLine(DocumentLine line) {
            if (_markers == null)
                return;
            int lineStart = line.Offset;
            int lineEnd = lineStart + line.Length;
            foreach (var marker in _markers.FindOverlappingSegments(lineStart, line.Length)) {
                Brush foregroundBrush = null;
                if (marker.ForegroundColor != null) {
                    foregroundBrush = new SolidColorBrush(marker.ForegroundColor.Value);
                    foregroundBrush.Freeze();
                }
                ChangeLinePart(
                    Math.Max(marker.StartOffset, lineStart),
                    Math.Min(marker.EndOffset, lineEnd),
                    element => {
                        if (foregroundBrush != null) element.TextRunProperties.SetForegroundBrush(foregroundBrush);
                        var tf = element.TextRunProperties.Typeface;
                        element.TextRunProperties.SetTypeface(new Typeface(
                            tf.FontFamily,
                            marker.FontStyle ?? tf.Style,
                            marker.FontWeight ?? tf.Weight,
                            tf.Stretch
                        ));
                    }
                );
            }
        }

        #endregion

        #region ITextMarkerService

        public ITextMarker Create(int startOffset, int length) {
            if (_markers == null)
                throw new InvalidOperationException("Cannot create a marker when not attached to a document");

            int textLength = _document.TextLength;
            if (startOffset < 0 || startOffset > textLength)
                throw new ArgumentOutOfRangeException(nameof(startOffset), startOffset,
                    "Value must be between 0 and " + textLength);
            if (length < 0 || startOffset + length > textLength)
                throw new ArgumentOutOfRangeException(nameof(length), length,
                    "length must not be negative and startOffset+length must not be after the end of the document");

            var m = new TextMarker(this, startOffset, length);
            _markers.Add(m);
            // no need to mark segment for redraw: the text marker is invisible until a property is set
            return m;
        }

        public IEnumerable<ITextMarker> GetMarkersAtOffset(int offset) {
            return _markers == null ? Enumerable.Empty<ITextMarker>() : _markers.FindSegmentsContaining(offset);
        }

        public IEnumerable<ITextMarker> TextMarkers => _markers ?? Enumerable.Empty<ITextMarker>();

        public void RemoveAll(Predicate<ITextMarker> predicate) {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));
            if (_markers == null) return;
            foreach (var m in _markers.ToArray())
                if (predicate(m))
                    Remove(m);
        }

        public void Remove(ITextMarker marker) {
            if (marker == null)
                throw new ArgumentNullException(nameof(marker));
            var m = marker as TextMarker;
            if (_markers == null || !_markers.Remove(m)) return;
            Redraw(m);
            m?.OnDeleted();
        }

        /// <summary>
        ///     Redraws the specified text segment.
        /// </summary>
        internal void Redraw(ISegment segment) {
            foreach (var view in _textViews) view.Redraw(segment);
            RedrawRequested?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RedrawRequested;

        #endregion

        #region IBackgroundRenderer

        public KnownLayer Layer => KnownLayer.Selection;

        public void Draw(TextView textView, DrawingContext drawingContext) {
            if (textView == null)
                throw new ArgumentNullException(nameof(textView));
            if (drawingContext == null)
                throw new ArgumentNullException(nameof(drawingContext));
            if (_markers == null || !textView.VisualLinesValid)
                return;
            ReadOnlyCollection<VisualLine> visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
                return;
            int viewStart = visualLines.First().FirstDocumentLine.Offset;
            int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
            foreach (var marker in _markers.FindOverlappingSegments(viewStart, viewEnd - viewStart)) {
                if (marker.BackgroundColor != null) {
                    var geoBuilder = new BackgroundGeometryBuilder {
                        AlignToWholePixels = true,
                        CornerRadius = 3
                    };
                    geoBuilder.AddSegment(textView, marker);
                    var geometry = geoBuilder.CreateGeometry();
                    if (geometry != null) {
                        var color = marker.BackgroundColor.Value;
                        var brush = new SolidColorBrush(color);
                        brush.Freeze();
                        drawingContext.DrawGeometry(brush, null, geometry);
                    }
                }
                var underlineMarkerTypes = TextMarkerTypes.SquigglyUnderline |
                                           TextMarkerTypes.NormalUnderline |
                                           TextMarkerTypes.DottedUnderline;
                if ((marker.MarkerTypes & underlineMarkerTypes) != 0)
                    foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker)) {
                        var startPoint = r.BottomLeft;
                        var endPoint = r.BottomRight;

                        Brush usedBrush = new SolidColorBrush(marker.MarkerColor);
                        usedBrush.Freeze();
                        if ((marker.MarkerTypes & TextMarkerTypes.SquigglyUnderline) != 0) {
                            double offset = 2.5;

                            int count = Math.Max((int) ((endPoint.X - startPoint.X) / offset) + 1, 4);

                            var geometry = new StreamGeometry();

                            using (var ctx = geometry.Open()) {
                                ctx.BeginFigure(startPoint, false, false);
                                ctx.PolyLineTo(CreatePoints(startPoint, endPoint, offset, count).ToArray(), true,
                                    false);
                            }

                            geometry.Freeze();

                            var usedPen = new Pen(usedBrush, 1);
                            usedPen.Freeze();
                            drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                        }
                        if ((marker.MarkerTypes & TextMarkerTypes.NormalUnderline) != 0) {
                            var usedPen = new Pen(usedBrush, 1);
                            usedPen.Freeze();
                            drawingContext.DrawLine(usedPen, startPoint, endPoint);
                        }
                        if ((marker.MarkerTypes & TextMarkerTypes.DottedUnderline) == 0) continue;
                        {
                            var usedPen = new Pen(usedBrush, 1) {DashStyle = DashStyles.Dot};
                            usedPen.Freeze();
                            drawingContext.DrawLine(usedPen, startPoint, endPoint);
                        }
                    }
            }
        }

        private static IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count) {
            for (int i = 0; i < count; i++)
                yield return new Point(start.X + i * offset, start.Y - ((i + 1) % 2 == 0 ? offset : 0));
        }

        #endregion

        #region ITextViewConnect

        private readonly List<TextView> _textViews = new List<TextView>();

        void ITextViewConnect.AddToTextView(TextView textView) {
            if (textView == null || _textViews.Contains(textView)) return;
            Debug.Assert(textView.Document == _document);
            _textViews.Add(textView);
        }

        void ITextViewConnect.RemoveFromTextView(TextView textView) {
            if (textView != null) {
                Debug.Assert(textView.Document == _document);
                _textViews.Remove(textView);
            }
        }

        #endregion
    }

    public sealed class TextMarker : TextSegment, ITextMarker {
        private readonly TextMarkerService _service;

        private Color? _backgroundColor;

        private FontStyle? _fontStyle;

        private FontWeight? _fontWeight;

        private Color? _foregroundColor;

        private Color _markerColor;

        private TextMarkerTypes _markerTypes;

        public TextMarker(TextMarkerService service, int startOffset, int length) {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            StartOffset = startOffset;
            Length = length;
            _markerTypes = TextMarkerTypes.None;
        }

        public event EventHandler Deleted;

        public bool IsDeleted => !IsConnectedToCollection;

        public void Delete() {
            _service.Remove(this);
        }

        public Color? BackgroundColor {
            get => _backgroundColor;
            set {
                if (_backgroundColor != value) {
                    _backgroundColor = value;
                    Redraw();
                }
            }
        }

        public Color? ForegroundColor {
            get => _foregroundColor;
            set {
                if (_foregroundColor != value) {
                    _foregroundColor = value;
                    Redraw();
                }
            }
        }

        public FontWeight? FontWeight {
            get => _fontWeight;
            set {
                if (_fontWeight != value) {
                    _fontWeight = value;
                    Redraw();
                }
            }
        }

        public FontStyle? FontStyle {
            get => _fontStyle;
            set {
                if (_fontStyle != value) {
                    _fontStyle = value;
                    Redraw();
                }
            }
        }

        public object Tag { get; set; }

        public TextMarkerTypes MarkerTypes {
            get => _markerTypes;
            set {
                if (_markerTypes == value) return;
                _markerTypes = value;
                Redraw();
            }
        }

        public Color MarkerColor {
            get => _markerColor;
            set {
                if (_markerColor == value) return;
                _markerColor = value;
                Redraw();
            }
        }

        public object ToolTip { get; set; }

        internal void OnDeleted() {
            Deleted?.Invoke(this, EventArgs.Empty);
        }

        private void Redraw() {
            _service.Redraw(this);
        }
    }
}