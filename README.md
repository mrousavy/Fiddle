# Fiddle
A lightweight code editor and compiler for fiddling with simple code snippets/scripts



# Contributing
1. [Fork](/Fork) Fiddle.
2. Make changes
	* Make bugfixes or other changes
	* Create a new Compiler
		1. Create new classes in `Fiddle.Compilers/Implementation/[LanguageName]/`:
			* `../[LanguageName]Compiler.cs` : `ICompiler`
			* `../[LanguageName]CompileResult.cs` : `ICompileResult`
			* `../[LanguageName]Diagnostic.cs` : `IDiagnostic`
			* `../[LanguageName]ExecuteResult.cs` : `IExecuteResult`
		2. Implement Interface functions and Constructor(s) (Example: `CSharp/CSharpCompiler.cs`)
		3. Add Language Name (filename-friendly) to `Fiddle.Compilers/Host.Language` enum
		4. Add Language Name to `Fiddle.UI/Editor.xaml` in `ComboBox` as `ComboBoxItem`
		5. Add Compiler initialization to `Fiddle.UI/Helper/ChangeLanguage(..)` with name from `ComboBoxItem`
		6. (Optionally) Add file-extension to `Fiddle.UI/Helper/GetFilterForLanguage(..)`
		7. (Optionally) Add Syntax highlighting definition to `Fiddle.UI/Syntax/`