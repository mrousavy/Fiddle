<p align="center">
	<h1 align="center">
		Fiddle
	</h1>
	<p align="center">
		Fiddle is a lightweight tool to <strong>edit</strong>, <strong>compile</strong> and <strong>run</strong> simple <strong>scripts</strong> in any of the <a href="#languages">supported languages</a>.
	</p>
	<p align="center">
		<a href="https://github.com/mrousavy/Fiddle/releases/latest"><img src="https://img.shields.io/badge/download-fiddle-green.svg" alt="Download"/></a>
	</p>
</p>

## Languages
- [ ] **C++** 	_(TODO: everything)_
- [x] **C#**
- [x] **Java** 	_(TODO: Return values)_
- [x] **LUA**
- [x] **Python**
- [x] **VB** 	_(TODO: Better Entry point)_

[Detailed documentation about implemented languages](https://github.com/mrousavy/Fiddle/tree/master/Doc)

[Other ToDo's](https://github.com/mrousavy/Fiddle/projects)

## Screenshots
<p align="center">
	<img src="https://github.com/mrousavy/Fiddle/raw/master/Images/Fiddle_slideshow.gif" alt="Fiddle Demo Slideshow"/>
</p>

## Cache
A directory will get created at `%appdata%\Fiddle` containing `Preferences.json` and when a crash was reported `error.txt`.

`Preferences.json` can get edited with the Settings window in Fiddle or by manually changing it via any text editor. The settings window cannot modify the `imports[]` property, this is not fully implemented.

## Contributing
1. Fork **Fiddle** and **clone the fork**.
2. Make changes
	* Make **bugfixes** or **other changes**
	
	   **.. or ..**
	* Add a **new Compiler**
		1. Create new **classes** in `Fiddle.Compilers\Implementation\[LanguageName]\`:
			* `..\[LanguageName]Compiler.cs` : `ICompiler`
			* `..\[LanguageName]CompileResult.cs` : `ICompileResult`
			* `..\[LanguageName]Diagnostic.cs` : `IDiagnostic`
			* `..\[LanguageName]ExecuteResult.cs` : `IExecuteResult`
		2. **Implement Interface** functions and Constructor(s) _(Example: `CSharp\CSharpCompiler.cs`)_
		3. Add **Language Name** (filename-friendly) to `Fiddle.Compilers\Host.Language` enum
		4. Add **Language Name** to `Fiddle.UI\Editor.xaml` in `ComboBox` as `ComboBoxItem`
		5. Add **Compiler initialization** to `Fiddle.UI\Helper.ChangeLanguage(..)` with name from `ComboBoxItem`
		6. Add Language name to every other function that handles language names hardcoded in `Fiddle.UI\Helper.cs`
		7. (Optionally) Add **file-extension** to `Fiddle.UI\Helper.GetFilterForLanguage(..)`
		8. (Optionally) Add **Syntax highlighting definition** to `Fiddle.UI\Syntax\[LanguageName].xshd`
		9. (Optionally) Add **Documentation** (using [this template](https://github.com/mrousavy/Fiddle/blob/master/Doc/CSharp.md)) to `Doc\[LanguageName].md` and linking it in `Doc\README.md`		
3. **Commit & Push**
4. Create a new **pull request** _(on your fork)_
