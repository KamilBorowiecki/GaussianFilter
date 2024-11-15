#include "pch.h"
#include "framework.h"
#include <iostream>
#include <vector>

#define MyFunctions _declspec(dllexport)

extern "C" {
	MyFunctions void calculateFilterCPP(BYTE outData[], BYTE data[], int imWidth, int index,short int filter[], int k)
	{
		// Inicjalizacja wyniku
		int result = 0;
		int filterSize = 2 * k + 1;

		// Przejdü przez kaødy element filtra
		for (int i = -k; i <= k; i++)
		{
			for (int j = -k; j <= k; j++)
			{
				result += data[index + ((i * imWidth + j) * 3)] * filter[(i + k) * filterSize + j + k];
	
			}
		}
		result /= 16;
		
		
		// Ograniczenie wyniku do zakresu 0-255
		if (result <= 0)
			result = 0;
		if (result > 255)
			result = 255;

		outData[index] = (BYTE)result;
	}

}
