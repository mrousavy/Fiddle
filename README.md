<p align="center">
	<h1 align="center">
		Fiddle
	</h1>
	<p align="center">
		Fiddle is a lightweight tool to <strong>edit</strong>, <strong>compile</strong> and <strong>run</strong> simple <strong>scripts</strong>/<strong>snippets</strong> in any of the <a href="#languages">supported languages</a>.
		<br/>
		⚠️ Fiddle requires .NET Framework v4.6 or above (<a href="https://www.microsoft.com/net/download/framework">download</a>) ⚠️
	</p>
	<p align="center">
		<a href="https://ci.appveyor.com/project/mrousavy/fiddle"><img src="https://ci.appveyor.com/api/projects/status/1g0s56o0v1hdlqxu?svg=true" alt="AppVeyor Build"/></a>
		<a href="https://github.com/mrousavy/Fiddle/releases/latest"><img src="https://img.shields.io/badge/download-fiddle-green.svg" alt="Download"/></a>
		<a href="https://github.com/mrousavy/Fiddle/releases"><img src="https://img.shields.io/github/downloads/mrousavy/Fiddle/total.svg" alt="Total downloads"/></a>
	</p>
</p>

## Languages
- [ ] [**C++**](https://github.com/mrousavy/Fiddle/blob/master/Doc/Cpp.md) 	_(TODO: everything)_
- [x] [**C#**](https://github.com/mrousavy/Fiddle/blob/master/Doc/CSharp.md)
- [x] [**Java**](https://github.com/mrousavy/Fiddle/blob/master/Doc/Java.md) 	_(TODO: Return values)_
- [x] [**LUA**](https://github.com/mrousavy/Fiddle/blob/master/Doc/Lua.md)
- [x] [**Python**](https://github.com/mrousavy/Fiddle/blob/master/Doc/Python.md)
- [x] [**VB**](https://github.com/mrousavy/Fiddle/blob/master/Doc/Vb.md) 	_(TODO: Better Entry point)_

[ToDo's](https://github.com/mrousavy/Fiddle/projects)

## Screenshots
<p align="center">
	<img src="https://github.com/mrousavy/Fiddle/raw/master/Images/Fiddle_slideshow.gif" alt="Fiddle Demo Slideshow"/>
	<a href="https://github.com/mrousavy/Fiddle/tree/master/Images">(See all screenshots)</a>
</p>

## Why?
The purpose of **Fiddle** is to simplify the Task of **quickly creating small code snippets**, like file-rename-scripts.

**Instead of:**
Open Visual Studio **->** _(wait)_ **->** File **->** New **->** New Project **->** .NET **->** Console App **->** _(enter name)_ **->** Create **->** _(wait)_ **->** Open `Program.cs` **->** Write code **->** Build **->** (wait) **->** Start

**Do:**
Open Fiddle **->** Write code **->** Start

## Features
These features apply to [all imported languages](#languages)

* **Rich UI** thanks to the [Material Design in XAML](http://materialdesigninxaml.net/) library
* **Editing code with custom syntax highlighting**
* **Compiling code with result view** including diagnostic messages, error messages and even line markers
* **Executing/Evaluating code/scripts** and viewing results (+ **expanding arrays/collections**)
* **Customizing settings** and **resuming last session** (window position, location, code, etc)
* **Saving code** to file
* **Loading code** from file (via drag and drop)
* **Seamlessly switching** between languages

## Cache
A directory will be created at `%appdata%\Fiddle` containing `Preferences.json` (crash reports will be stored as "`error.txt`").

`Preferences.json` can be edited with the Settings window in Fiddle or by manually changing it via any text editor. 
However, the settings window cannot modify the `imports[]`, `DefaultCode` and window dimensions/cursor position properties, this is not fully implemented. For now you can use `Preferences.json` to manually edit these.

## Build from Source
+ **Visual Studio Way**
    1. Open `Fiddle.sln`
    2. Set build target (**Debug**: development, **Release**: portable releases, **Publish**: InnoSetup installer)
    3. Build Solution/Fiddle.UI (<kbd>Ctrl</kbd> + <kbd>Shift</kbd> + <kbd>B</kbd>)

+ **Command Line**
    1. Run `nuget restore` command in `Fiddle` directory (Requires [NuGet installed](https://www.nuget.org/downloads) and [configured in Environment variables](https://stackoverflow.com/a/21067553))
    2. Run `msbuild Fiddle.sln /t:Build /p:Configuration=Release` or `msbuild Fiddle.sln /t:Build /p:Configuration=Publish` for InnoSetup installer (Requires [MsBuild installed](https://www.microsoft.com/en-us/download/details.aspx?id=48159) and [configured in Environment variables](https://stackoverflow.com/a/12608705))

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
		9. (Optionally) Add **Documentation** (using [this template](https://github.com/mrousavy/Fiddle/blob/master/Doc/Template.md), or [this example](https://github.com/mrousavy/Fiddle/blob/master/Doc/CSharp.md)) to `Doc\[LanguageName].md` and linking it in `Doc\README.md`		
3. **Commit & Push**
4. Create a new **pull request** _(on your fork)_
