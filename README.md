# Fiddle
Fiddle is a lightweight tool to **edit**, **compile** and **run** simple **scripts** in any of the [supported languages](#languages).

## Settings
- [ ] Settings Window

_(For now you can edit the settings in `%appdata%\Fiddle\Preferences.json`. [Documentation (Preferences class)](https://github.com/mrousavy/Fiddle/blob/master/Fiddle.UI/Preferences.cs))_

## Screenshots

## Languages
- [ ] **C++** 	_(TODO: everything)_
- [x] **C#**
- [ ] **Java** 	_(TODO: Return values)_
- [x] **LUA**
- [x] **Python**
- [x] **VB** 	_(TODO: Better Entry point)_

## Contributing
1. [Fork **Fiddle**](http://github.com/mrousavy/Fiddle/Fork) and **clone the fork**.
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
		6. (Optionally) Add **file-extension** to `Fiddle.UI\Helper.GetFilterForLanguage(..)`
		7. (Optionally) Add **Syntax highlighting definition** to `Fiddle.UI\Syntax\[LanguageName].xshd`		
3. **Commit & Push**
4. Create a new **pull request** _(on your fork)_
