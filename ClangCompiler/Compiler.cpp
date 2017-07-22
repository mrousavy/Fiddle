#include "stdafx.h"
#include <iostream>

using namespace std;


//Interface wrapper
extern "C" __declspec(dllexport) string Compile(string sourcecode) {
	return Compile(sourcecode);
}
//Interface wrapper
extern "C" __declspec(dllexport) string Execute(string assemblylocation) {
	return ExecuteC(assemblylocation);
}




string CompileC(string code) {
	//TODO: Use %TEMP% for .c and .exe instead of CurrentWorkingDirectory

	//TODO: Compile cppfiddle.c to cppfiddle.exe
	//..

	//string inputPath = "cppfiddle.c";

	//// Path to the executable
	//string outputPath = "cppfiddle";

	//// Path to clang (e.g. /usr/local/bin/clang)
	//llvm::sys::Path clangPath = llvm::sys::Program::FindProgramByName("clang");

	//// Arguments to pass to the clang driver:
	////    clang cppfiddle.c -lcurl -v
	//vector<const char *> args;
	//args.push_back(clangPath.c_str());
	//args.push_back(inputPath.c_str());
	//args.push_back("-l");
	//args.push_back("curl");

	//string output; //TODO: Read console output of compilation
	return "compiled";
}

string ExecuteC(string asmloc) {
	// Path to the executable
	//string outputPath = "cppfiddle";

	////TODO: Run cppfiddle.exe
	////..

	//string output; //TODO: Read console output of cppfiddle.exe
	return "executed";
}