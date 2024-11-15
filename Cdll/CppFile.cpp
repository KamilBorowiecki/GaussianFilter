#include "pch.h"
#include "framework.h"
#include <iostream>
#include <vector>

#define MyFunctions _declspec(dllexport)

extern "C" {
	MyFunctions void calculateFilterCPP(BYTE outData[], BYTE data[], int imWidth, int index, short int filter[])
	{
		// Inicjalizacja wyniku
		short int result = 0;

		// Przejdü przez kaødy element filtra
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				result += data[index + ((i * imWidth + j) * 3)] * filter[(i + 1) * 3 + j + 1];	
			}
		}
		result = result / 16;
			
		// Ograniczenie wyniku do zakresu 0-255
		if (result <= 0)
			result = 0;
		if (result > 255)
			result = 255;

		outData[index] = (BYTE)result;
	}

}
