// GaussianBlurTool.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;

namespace MonoKit.Graphics
{
    public class GaussianBlurTool
    {
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
