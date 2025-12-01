// IntExtension.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.


using System;

namespace MonoKit.Core.Extensions
{
    public static class IntExtension
    {
        public static string ToRoman(this int value)
        {
            if (value < 1 || value > 3999)
                return "?";

            var romanNumerals = new[]
            {
                new { Value = 1000, Numeral = "M" },
                new { Value = 900, Numeral = "CM" },
                new { Value = 500, Numeral = "D" },
                new { Value = 400, Numeral = "CD" },
                new { Value = 100, Numeral = "C" },
                new { Value = 90, Numeral = "XC" },
                new { Value = 50, Numeral = "L" },
                new { Value = 40, Numeral = "XL" },
                new { Value = 10, Numeral = "X" },
                new { Value = 9, Numeral = "IX" },
                new { Value = 5, Numeral = "V" },
                new { Value = 4, Numeral = "IV" },
                new { Value = 1, Numeral = "I" }
            };

            var result = new System.Text.StringBuilder();

            foreach (var item in romanNumerals)
            {
                while (value >= item.Value)
                {
                    result.Append(item.Numeral);
                    value -= item.Value;
                }
            }

            return result.ToString();
        }
    }
}
