
#include "pch.h"
#include "framework.h"
#include <iostream>

#define MyFunctions _declspec(dllexport)

extern "C" {
    MyFunctions void calculateFilterCPP(BYTE* data, int width, int height) {
        int channels = 3; // dla 24-bitowego RGB
        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                // Oblicz wskaŸnik do piksela (ka¿dy piksel to 3 bajty: R, G, B)
                unsigned char* pixel = data + (y * width + x) * channels;

                // Przyk³adowa modyfikacja: odwróæ kolor
                pixel[0] = 255 - pixel[0]; // R
                pixel[1] = 255 - pixel[1]; // G
                pixel[2] = 255 - pixel[2]; // B
            }
        }
    }
}
