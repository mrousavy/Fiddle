#include "stdafx.h"
#include <Windows.h>
#include <iostream>
#include <string.h>



char* CompileC(char* code) {
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

char* ExecuteC(char* asmloc) {
	// Path to the executable
	//string outputPath = "cppfiddle";

	////TODO: Run cppfiddle.exe
	////..

	//string output; //TODO: Read console output of cppfiddle.exe
	return "executed";
}


//Interface wrapper
extern "C" {
	__declspec(dllexport) char* Compile(char* sourcecode) {
		return "test";
		return CompileC(sourcecode);
	}
	__declspec(dllexport) char* Execute(char* assemblylocation) {
		return "test";
		return ExecuteC(assemblylocation);
	}
}
