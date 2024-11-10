#include "pch.h"
#include "framework.h"
#include <iostream>
#include <vector>

#define MyFunctions _declspec(dllexport)

extern "C" {
    MyFunctions void calculateFilterCPP(BYTE* ptr, int row, int width, int height, int k, double* filter) {
        int stride = ((width * 3 + 3) / 4) * 4;
        std::vector<BYTE> result(width * 3);

        int kernelSize = 2 * k + 1;

        // Przetwarzanie pikseli
        for (int x = k; x < width - k; x++) {
            double blueSum = 0, greenSum = 0, redSum = 0;

            for (int ky = -k; ky <= k; ky++) {
                for (int kx = -k; kx <= k; kx++) {
                    int neighborX = x + kx;
                    int neighborY = row + ky;

                    if (neighborY >= 0 && neighborY < height) {
                        BYTE* neighborPixel = ptr + (neighborY * stride) + (neighborX * 3);

                        int neighborBlue = neighborPixel[0];
                        int neighborGreen = neighborPixel[1];
                        int neighborRed = neighborPixel[2];

                        // Odczyt wartoœci z p³askiej tablicy
                        double kernelValue = filter[(ky + k) * kernelSize + (kx + k)];
                        blueSum += neighborBlue * kernelValue;
                        greenSum += neighborGreen * kernelValue;
                        redSum += neighborRed * kernelValue;
                    }
                }
            }

            // Rêczne ograniczenie wartoœci
            int resultIndex = x * 3;
            int blue = int(blueSum);
            int green = int(greenSum);
            int red = int(redSum);

            if (blue <= 0) blue = 0;
            if (blue > 255) blue = 255;
            if (green <= 0) green = 0;
            if (green > 255) green = 255;
            if (red <= 0) red = 0;
            if (red > 255) red = 255;

            result[resultIndex] = BYTE(blue);
            result[resultIndex + 1] = BYTE(green);
            result[resultIndex + 2] = BYTE(red);
        }
        BYTE* rowPtr = ptr + (row * stride);
        for (int i = 0; i < width * 3; i++) {
            rowPtr[i] = result[i];
        }
    }
}
