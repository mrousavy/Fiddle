# Java (`Language.Java`)

**Java** has been implemented by using the `javac.exe` from commandline (with `Process.Start()`) - Can be improved by using an API (e.g: create `JNI Interface` which will call a Java compiler in Java ([JavaCompiler](https://docs.oracle.com/javase/7/docs/api/javax/tools/JavaCompiler.html)))

[Implementation](https://github.com/mrousavy/Fiddle/tree/master/Fiddle.Compilers/Implementation/Java) / [Compiler](https://github.com/mrousavy/Fiddle/blob/master/Fiddle.Compilers/Implementation/Java/JavaCompiler.cs)

## About
You can write small snippets, but be aware: **Fiddle** will automatically put them into a main method if none found.

So:
```java
System.out.println("Hello world!");
```
Will be compiled as:
```java
public class FiddleClass {
    public static void main(String[] args) {
        System.out.println("Hello world!");
    }
}
```

## Completeness

- [x] Syntax highlighting
- [x] Compilation
- [x] Execution
- [x] Console ouput
- [ ] Return values
- [x] Diagnostics (Line number and red underline does not work)
- [x] Errors (Line number and red underline does not work)

[Example code](https://github.com/mrousavy/Fiddle/blob/master/Fiddle.Compilers/Implementation/Java/JavaDemo.java) | [Projects](https://github.com/mrousavy/Fiddle/projects)

## Globals
You cannot use any Globals in your code so far.

## Properties
- `string jdkPath`: Path to Java Development Kit. If empty, **Fiddle** will **automatically search** the JDK in Program files or [Environment variables](https://www.java.com/en/download/help/path.xml)
