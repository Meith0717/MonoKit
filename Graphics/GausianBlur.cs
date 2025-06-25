// GaussianBlur.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Threading.Tasks;

namespace GameEngine.Graphics
{
    internal class GaussianBlur
    {
        private static float[] GaussianBlurMap(float[] map, int size, int radius, float sigma)
        {
            int area = size * size;
            var temp = new float[area];
            var result = new float[area];

            // Generate 1D Gaussian kernel
            float[] kernel = new float[radius * 2 + 1];
            float sum = 0;
            for (int i = -radius; i <= radius; i++)
            {
                float weight = (float)Math.Exp(-(i * i) / (2 * sigma * sigma));
                kernel[i + radius] = weight;
                sum += weight;
            }

            // Normalize the kernel
            for (int i = 0; i < kernel.Length; i++)
            {
                kernel[i] /= sum;
            }

            // Horizontal pass
            Parallel.For(0, size, y =>
            {
                for (int x = 0; x < size; x++)
                {
                    float blurred = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int sampleX = Math.Clamp(x + k, 0, size - 1);
                        blurred += map[y * size + sampleX] * kernel[k + radius];
                    }
                    temp[y * size + x] = blurred;
                }
            });

            // Vertical pass
            Parallel.For(0, size, x =>
            {
                for (int y = 0; y < size; y++)
                {
                    float blurred = 0;
                    for (int k = -radius; k <= radius; k++)
                    {
                        int sampleY = Math.Clamp(y + k, 0, size - 1);
                        blurred += temp[sampleY * size + x] * kernel[k + radius];
                    }
                    result[y * size + x] = blurred;
                }
            });

            return result;
        }

        public static float[] GetGaussianKernel1D(int radius, double sigma)
        {
            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be non-negative.");
            if (sigma <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma must be positive.");

            int length = radius * 2 + 1;
            var kernel = new float[length];

            double twoSigmaSq = 2.0 * sigma * sigma;
            double sum = 0.0;

            // Compute weights
            for (int x = -radius; x <= radius; x++)
            {
                double w = Math.Exp(-(x * x) / twoSigmaSq);
                kernel[x + radius] = (float)w;
                sum += w;
            }

            // Normalize
            float invSum = (float)(1.0 / sum);
            for (int i = 0; i < length; i++)
                kernel[i] *= invSum;

            return kernel;
        }


    }
}
