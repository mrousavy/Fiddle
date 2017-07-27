# LUA (`Language.Lua`)

**LUA** has been implemented with the [NLua engine](https://github.com/NLua/NLua).

[Implementation](https://github.com/mrousavy/Fiddle/tree/master/Fiddle.Compilers/Implementation/LUA) / [Compiler](https://github.com/mrousavy/Fiddle/blob/master/Fiddle.Compilers/Implementation/LUA/LuaCompiler.cs)

## Completeness

- [x] Syntax highlighting
- [x] Compilation _(LUA is interpreted -> no compilation needed)_
- [x] Execution
- [x] Console ouput
- [x] Return values
- [x] Diagnostics
- [x] Errors

[Example code](https://github.com/mrousavy/Fiddle/blob/master/Fiddle.Compilers/Implementation/LUA/LuaDemo.lua) | [Projects](https://github.com/mrousavy/Fiddle/projects)

## Globals
You can use the following **Globals/Variables** inside your code:

* `Random` (**object**, .NET `System.Random`, [msdn](https://msdn.microsoft.com/en-us/library/system.random(v=vs.110).aspx)): **Create random numbers** with [`Random:Next(..)`](https://msdn.microsoft.com/en-us/library/system.random.next(v=vs.110).aspx)

* `Console` (**object**, .NET `System.IO.StringWriter`, [msdn](https://msdn.microsoft.com/en-us/library/system.io.stringwriter(v=vs.110).aspx)): **Write to Console** with [`Console:WriteLine(string)`](https://msdn.microsoft.com/en-us/library/system.console.writeline(v=vs.110).aspx) or [`Console:Write(string)`](https://msdn.microsoft.com/en-us/library/system.console.write(v=vs.110).aspx)

* `CurrentThread` (**object**, .NET `System.Threading.Thread`, [msdn](https://msdn.microsoft.com/en-us/library/system.threading.thread(v=vs.110).aspx)): The thread this got initialized on (**mostly UI Thread**), can be used to access all properties or functions from a [`System.Threading.Thread`](https://msdn.microsoft.com/en-us/library/system.threading.thread(v=vs.110).aspx).

* `Editor` (**object**, Fiddle `Fiddle.UI.Editor` (from `System.Windows.Window`, [msdn](https://msdn.microsoft.com/en-us/library/system.windows.window(v=vs.110).aspx))): Fiddle's Editor window, can be used to access all [`UIElements`](https://msdn.microsoft.com/en-us/library/system.windows.uielement(v=vs.110).aspx), properties, public functions or functions derived from `System.Windows.Window`. ([Editor XAML code](https://github.com/mrousavy/Fiddle/blob/master/Fiddle.UI/Editor.xaml), [Editor source code](https://github.com/mrousavy/Fiddle/blob/master/Fiddle.UI/Editor.xaml.cs))

* `App` (**object**, .NET `System.Windows.Application`, [msdn](https://msdn.microsoft.com/en-us/library/system.windows.application(v=vs.110).aspx)): Fiddle's calling `Application`/`App`, can be used to access all properties or functions derived from [`System.Windows.Application`](https://msdn.microsoft.com/en-us/library/system.windows.application(v=vs.110).aspx))

* `RunUi(Action)` (**function**, `void Invoke(Action)`, [declaration](https://github.com/mrousavy/Fiddle/blob/master/Fiddle.UI/FiddleGlobals.cs#L42)): A function with an anonymous function or [`Action`](https://msdn.microsoft.com/en-us/library/018hxwa8(v=vs.110).aspx) as a parameter **to execute code on the UI Thread**. This is **required for getting/setting properties or calling functions from `Editor` or `App`** because those are **not thread safe**.

## Properties
/
