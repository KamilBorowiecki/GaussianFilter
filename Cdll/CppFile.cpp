
#include "pch.h"
#include "framework.h"
#include <iostream>

#define MyFunctions _declspec(dllexport)

extern "C" {
	MyFunctions void calculateFilterCPP(BYTE* ptr, BYTE valueToAdd)
	{
		*ptr += valueToAdd;
	}
}
