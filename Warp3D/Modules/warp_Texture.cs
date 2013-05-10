/*
 * Copyright (c) Peter Walser and Dean Lunz
 * All rights reserved.
 *
 * Licensed under the Creative Commons Attribution Share-Alike 2.5 Canada
 * license: http://creativecommons.org/licenses/by-sa/2.5/ca/
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;

namespace Rednettle.Warp3D
{
    /// <summary>
    /// Summary description for warp_Texture.
    /// </summary>

    public class warp_Texture
    {
        public int width;
        public int height;
        public int bitWidth;
        public int bitHeight;
        public int[] pixel;
        public String path = null;

        public static int ALPHA = unchecked((int)0xFF000000); // alpha mask

        public warp_Texture(int w, int h)
        {
            height = h;
            width = w;
            pixel = new int[w * h];
            cls();
        }

        public warp_Texture(int w, int h, int[] data)
        {
            height = h;
            width = w;
            pixel = new int[width * height];

            System.Array.Copy(data, pixel, pixel.Length);
        }

        public warp_Texture(string path)
        {
            Bitmap map;

            if (path.StartsWith("http"))
            {
                WebRequest webrq = WebRequest.Create(path);
                map = (Bitmap)Bitmap.FromStream(webrq.GetResponse().GetResponseStream());
            }
            else
            {
                map = new Bitmap(path, false);
            }

            loadTexture(map);
        }

        public warp_Texture(Bitmap bitmap)
        {
            loadTexture(bitmap);
        }

        public void resize()
        {
            double log2inv = 1 / Math.Log(2);

            int w = (int)Math.Pow(2, bitWidth = (int)Math.Ceiling((Math.Log(width) * log2inv)));
            int h = (int)Math.Pow(2, bitHeight = (int)Math.Ceiling((Math.Log(height) * log2inv)));

            if (!(w == width && h == height))
                resize(w, h);
        }

        public void resize(int w, int h)
        {
            //System.Console.WriteLine("warp_Texture| resize :"+w+","+h);
            setSize(w, h);
        }

        public void cls()
        {
            warp_Math.clearBuffer(pixel, 0);
        }

        public warp_Texture toAverage()
        {
            for (int i = width * height - 1; i >= 0; i--)
                pixel[i] = warp_Color.getAverage(pixel[i]);

            return this;
        }

        public warp_Texture toGray()
        {
            for (int i = width * height - 1; i >= 0; i--)
                pixel[i] = warp_Color.getGray(pixel[i]);

            return this;
        }

        public warp_Texture valToGray()
        {
            int intensity;
            for (int i = width * height - 1; i >= 0; i--)
            {
                intensity = warp_Math.crop(pixel[i], 0, 255);
                pixel[i] = warp_Color.getColor(intensity, intensity, intensity);
            }

            return this;
        }

        public warp_Texture colorize(int[] pal)
        {
            int range = pal.GetLength(0) - 1;
            for (int i = width * height - 1; i >= 0; i--)
                pixel[i] = pal[warp_Math.crop(pixel[i], 0, range)];

            return this;
        }

        private void loadTexture(Bitmap map)
        {
            width = map.Width;
            height = map.Height;

            pixel = new int[width * height];

            BitmapData bmData = map.LockBits(new Rectangle(0, 0, map.Width, map.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            int scanline = bmData.Stride;

            System.IntPtr Scan0 = bmData.Scan0;
            /*
                          // Declare an array to hold the bytes of the bitmap.
                    // This code is specific to a bitmap with 24 bits per pixels.
                    byte[] bytes = new byte[(map.Width * map.Height) * 3];

                    // Copy the RGB values into the array.
                    System.Runtime.InteropServices.Marshal.Copy (  Scan0, bytes, 0, bytes.Length  );
     
                        int nOffset = bmData.Stride - map.Width * 3;
                    int nPixel = 0;
                    int p =0;
                    for (int i = 0; i < map.Height; i++)
                    {
                        for (int j = 0; j < map.Width; j++)
                        {
                            int blue = bytes[p];
                            int green = bytes[p+1];
                            int red = bytes[p+2];

                            pixel[nPixel++] = ALPHA | red << 16 | green << 8 | blue;
                            p += 3;
                        }

                        p += nOffset;
                    }
            */
            // Copy the RGB values back to the bitmap
            // System.Runtime.InteropServices.Marshal.Copy(bytes, 0, ptr, bytes.Length);

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;


                int nOffset = bmData.Stride - map.Width * 3;
                int nPixel = 0;

                for (int i = 0; i < map.Height; i++)
                {
                    for (int j = 0; j < map.Width; j++)
                    {
                        int blue = p[0];
                        int green = p[1];
                        int red = p[2];

                        pixel[nPixel++] = ALPHA | red << 16 | green << 8 | blue;
                        p += 3;
                    }

                    p += nOffset;
                }
            }

            map.UnlockBits(bmData);
            resize();
        }

        private void setSize(int w, int h)
        {
            int offset = w * h;
            int offset2;
            if (w * h != 0)
            {
                int[] newpixels = new int[w * h];
                for (int j = h - 1; j >= 0; j--)
                {
                    offset -= w;
                    offset2 = (j * height / h) * width;
                    for (int i = w - 1; i >= 0; i--)
                        newpixels[i + offset] = pixel[(i * width / w) + offset2];
                }

                width = w;
                height = h;
                pixel = newpixels;
            }
        }

        private bool inrange(int a, int b, int c)
        {
            return (a >= b) & (a < c);
        }

        public warp_Texture getClone()
        {
            warp_Texture t = new warp_Texture(width, height);
            warp_Math.copyBuffer(pixel, t.pixel);
            return t;
        }
    }
}