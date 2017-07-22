#include <iostream>
#include <string>
using namespace std;

#pragma once

extern "C" {
	__declspec(dllexport) string Compile(string sourcecode);
	__declspec(dllexport) string Execute(string assemblylocation);
}

string CompileC(string code);
string ExecuteC(string asmloc);