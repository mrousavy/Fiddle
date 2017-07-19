#include <iostream>

#pragma once
class Compiler
{
public:
	Compiler();
	~Compiler();
	string Compile(string code);
	string Execute();
};

