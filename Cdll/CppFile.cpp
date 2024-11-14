#include "pch.h"
#include "framework.h"
#include <iostream>
#include <vector>

#define MyFunctions _declspec(dllexport)

extern "C" {
	MyFunctions void calculateFilterCPP(BYTE outData[], BYTE data[], int imWidth, int index, int filter[], int k)
	{
		// Inicjalizacja wyniku
		double result = 0;
		int filterSize = 2 * k + 1;

		// Przejdü przez kaødy element filtra
		for (int i = -k; i <= k; i++)
		{
			for (int j = -k; j <= k; j++)
			{
				int filterValue = filter[(i + k) * filterSize + j + k];
				result += data[index + ((i * imWidth + j) * 3)] * (filterValue);
	
			}
		}

		
		
		// Ograniczenie wyniku do zakresu 0-255
		if (result <= 0)
			result = 0;
		if (result > 255)
			result = 255;

		outData[index] = (BYTE)result;
	}

}
