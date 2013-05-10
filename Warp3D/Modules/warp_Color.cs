/*
 * Copyright (c) Peter Walser and Dean Lunz
 * All rights reserved.
 *
 * Licensed under the Creative Commons Attribution Share-Alike 2.5 Canada
 * license: http://creativecommons.org/licenses/by-sa/2.5/ca/
 */

using System;

namespace Rednettle.Warp3D
{
    /// <summary>
    /// Summary description for warp_Color.
    /// </summary>
    public static class warp_Color
    {
        public const int ALPHA = unchecked((int)0xFF000000); // alpha mask
        public const int RED = unchecked((int)0xFF0000);  // red mask
        public const int GREEN = unchecked((int)0xFF00);  // green mask
        public const int BLUE = unchecked((int)0xFF);  // blue mask
        public const int MASK7Bit = unchecked((int)0xFEFEFF);  // mask for additive/subtractive shading
        public const int MASK6Bit = unchecked((int)0xFCFCFC);  // mask for additive/subtractive shading
        public const int RGB = unchecked((int)0xFFFFFF);  // rgb mask

        public static int random(int color, int delta)
        {
            Random rnd = new Random();

            int r = (color >> 16) & 255;
            int g = (color >> 8) & 255;
            int b = color & 255;

            r += (int)(rnd.NextDouble() * (float)delta);
            g += (int)(rnd.NextDouble() * (float)delta);
            b += (int)(rnd.NextDouble() * (float)delta);

            return getCropColor(r, g, b);
        }

        public static int random()
        {
            Random rnd = new Random();
            return (int)(rnd.NextDouble() * 16777216);
        }

        public static int getRed(int c)
        {
            return (c & RED) >> 16;
        }

        public static int getGreen(int c)
        {
            return (c & GREEN) >> 8;
        }

        public static int getBlue(int c)
        {
            return c & BLUE;
        }

        public static int[] makeGradient(int[] colors, int size)
        {
            int[] pal = new int[size];
            int c1, c2, pos1, pos2, range;
            int r, g, b, r1, g1, b1, r2, g2, b2, dr, dg, db;
            if (colors.GetLength(0) == 1)
            {
                c1 = colors[0];
                for (int i = 0; i < size; i++)
                {
                    pal[i] = c1;
                }
                return pal;
            }

            for (int c = 0; c < colors.GetLength(0) - 1; c++)
            {
                c1 = colors[c];
                c2 = colors[c + 1];
                pos1 = size * c / (colors.GetLength(0) - 1);
                pos2 = size * (c + 1) / (colors.GetLength(0) - 1);
                range = pos2 - pos1;
                r1 = warp_Color.getRed(c1) << 16;
                g1 = warp_Color.getGreen(c1) << 16;
                b1 = warp_Color.getBlue(c1) << 16;
                r2 = warp_Color.getRed(c2) << 16;
                g2 = warp_Color.getGreen(c2) << 16;
                b2 = warp_Color.getBlue(c2) << 16;
                dr = (r2 - r1) / range;
                dg = (g2 - g1) / range;
                db = (b2 - b1) / range;
                r = r1;
                g = g1;
                b = b1;
                for (int i = pos1; i < pos2; i++)
                {
                    pal[i] = getColor(r >> 16, g >> 16, b >> 16);
                    r += dr;
                    g += dg;
                    b += db;
                }
            }

            return pal;
        }

        public static int add(int color1, int color2)
        {
            int pixel = (color1 & MASK7Bit) + (color2 & MASK7Bit);
            int overflow = pixel & 0x1010100;
            overflow = overflow - (overflow >> 8);

            return ALPHA | overflow | pixel;
        }

        public static int sub(int color1, int color2)
        // Substracts color2 from color1
        {
            int pixel = (color1 & MASK7Bit) + (~color2 & MASK7Bit);
            int overflow = ~pixel & 0x1010100;
            overflow = overflow - (overflow >> 8);
            return ALPHA | (~overflow & pixel);
        }

        public static int subneg(int color1, int color2)
        // Substracts the negative of color2 from color1
        {
            int pixel = (color1 & MASK7Bit) + (color2 & MASK7Bit);
            int overflow = ~pixel & 0x1010100;
            overflow = overflow - (overflow >> 8);
            return ALPHA | (~overflow & pixel);
        }

        public static int inv(int color)
        // returns the inverse of the given color
        {
            return ALPHA | (~color);
        }

        public static int mix(int color1, int color2)
        // Returns the averidge color from 2 colors
        {
            return ALPHA | (((color1 & MASK7Bit) >> 1) + ((color2 & MASK7Bit) >> 1));
        }

        public static int scale(int color, int factor)
        {
            if (factor == 0)
            {
                return 0;
            }
            if (factor == 255)
            {
                return color;
            }
            if (factor == 127)
            {
                return (color & 0xFEFEFE) >> 1;
            }

            int r = (((color >> 16) & 255) * factor) >> 8;
            int g = (((color >> 8) & 255) * factor) >> 8;
            int b = ((color & 255) * factor) >> 8;
            return ALPHA | (r << 16) | (g << 8) | b;
        }

        public static int multiply(int color1, int color2)
        {
            if ((color1 & RGB) == 0)
            {
                return 0;
            }
            if ((color2 & RGB) == 0)
            {
                return 0;
            }
            int r = (((color1 >> 16) & 255) * ((color2 >> 16) & 255)) >> 8;
            int g = (((color1 >> 8) & 255) * ((color2 >> 8) & 255)) >> 8;
            int b = ((color1 & 255) * (color2 & 255)) >> 8;
            return ALPHA | (r << 16) | (g << 8) | b;
        }

        public static int transparency(int bkgrd, int color, int alpha)
        // alpha=0 : opaque , alpha=255: full transparent
        {
            if (alpha == 0)
            {
                return color;
            }
            if (alpha == 255)
            {
                return bkgrd;
            }
            if (alpha == 127)
            {
                return mix(bkgrd, color);
            }

            int r = (alpha * (((bkgrd >> 16) & 255) - ((color >> 16) & 255)) >> 8) +
                ((color >> 16) & 255);
            int g = (alpha * (((bkgrd >> 8) & 255) - ((color >> 8) & 255)) >> 8) +
                ((color >> 8) & 255);
            int b = (alpha * ((bkgrd & 255) - (color & 255)) >> 8) + (color & 255);

            return ALPHA | (r << 16) | (g << 8) | b;

        }

        public static int getCropColor(int r, int g, int b)
        {
            return ALPHA | (warp_Math.crop(r, 0, 255) << 16) | (warp_Math.crop(g, 0, 255) << 8) | warp_Math.crop(b, 0, 255);
        }


        public static int getColor(int r, int g, int b)
        {
            return ALPHA | (r << 16) | (g << 8) | b;
        }

        public static int getGray(int color)
        {
            int r = ((color & RED) >> 16);
            int g = ((color & GREEN) >> 8);
            int b = (color & BLUE);
            int Y = (r * 3 + g * 6 + b) / 10;
            return ALPHA | (Y << 16) | (Y << 8) | Y;
        }

        public static int getAverage(int color)
        {
            return (((color & RED) >> 16) + ((color & GREEN) >> 8) + (color & BLUE)) / 3;
        }
    }
}