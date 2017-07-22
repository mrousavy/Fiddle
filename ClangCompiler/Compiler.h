#include <iostream>

#pragma once

char* CompileC(char* code);
char* ExecuteC(char* asmloc);

extern "C" {
	__declspec(dllexport) char* Compile(char* sourcecode);
	__declspec(dllexport) char* Execute(char* assemblylocation);
}