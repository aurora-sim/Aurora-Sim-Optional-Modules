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
using System.Runtime.InteropServices;

namespace Rednettle.Warp3D
{
    /// <summary>
    /// Summary description for warp_Screen.
    /// </summary>
    unsafe public class warp_Screen : IDisposable
    {
        public int width;
        public int height;

        internal int[] pixels;

        private Bitmap image;
        private GCHandle handle;

        public warp_Screen(int w, int h)
        {
            width = w;
            height = h;

            pixels = new int[w * h];

            handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            IntPtr pointer = Marshal.UnsafeAddrOfPinnedArrayElement(pixels, 0);

            image = new Bitmap(w, h, w * 4, PixelFormat.Format32bppPArgb, pointer);
        }

        public void clear(int c)
        {
            warp_Math.clearBuffer(pixels, unchecked((int)0xff000000) | c);
        }

        public void draw(warp_Texture texture, int posx, int posy, int xsize, int ysize)
        {
            draw(width, height, texture, posx, posy, xsize, ysize);
        }

        public void drawBackground(warp_Texture texture, int posx, int posy, int xsize, int ysize)
        {
            draw(width, height, texture, posx, posy, xsize, ysize);
        }

        public Bitmap getImage()
        {
            return image;
        }

        private void draw(int width, int height, warp_Texture texture, int posx, int posy, int xsize, int ysize)
        {
            if (texture == null)
            {
                return;
            }

            int w = xsize;
            int h = ysize;
            int xBase = posx;
            int yBase = posy;
            int tx = texture.width * 255;
            int ty = texture.height * 255;
            int tw = texture.width;
            int dtx = tx / w;
            int dty = ty / h;
            int txBase = warp_Math.crop(-xBase * dtx, 0, 255 * tx);
            int tyBase = warp_Math.crop(-yBase * dty, 0, 255 * ty);
            int xend = warp_Math.crop(xBase + w, 0, width);
            int yend = warp_Math.crop(yBase + h, 0, height);
            int offset1, offset2;
            xBase = warp_Math.crop(xBase, 0, width);
            yBase = warp_Math.crop(yBase, 0, height);

            ty = tyBase;
            for (int j = yBase; j < yend; j++)
            {
                tx = txBase;
                offset1 = j * width;
                offset2 = (ty >> 8) * tw;
                for (int i = xBase; i < xend; i++)
                {
                    pixels[i + offset1] = unchecked((int)0xff000000) | texture.pixel[(tx >> 8) + offset2];
                    tx += dtx;
                }
                ty += dty;
            }
        }

        public void add(warp_Texture texture, int posx, int posy, int xsize, int ysize)
        {
            add(width, height, texture, posx, posy, xsize, ysize);
        }

        private void add(int width, int height, warp_Texture texture, int posx, int posy, int xsize, int ysize)
        {
            if (texture == null)
            {
                return;
            }

            int w = xsize;
            int h = ysize;
            int xBase = posx;
            int yBase = posy;
            int tx = texture.width * 255;
            int ty = texture.height * 255;
            int tw = texture.width;
            int dtx = tx / w;
            int dty = ty / h;
            int txBase = warp_Math.crop(-xBase * dtx, 0, 255 * tx);
            int tyBase = warp_Math.crop(-yBase * dty, 0, 255 * ty);
            int xend = warp_Math.crop(xBase + w, 0, width);
            int yend = warp_Math.crop(yBase + h, 0, height);
            int offset1, offset2;
            xBase = warp_Math.crop(xBase, 0, width);
            yBase = warp_Math.crop(yBase, 0, height);

            ty = tyBase;
            for (int j = yBase; j < yend; j++)
            {
                tx = txBase;
                offset1 = j * width;
                offset2 = (ty >> 8) * tw;
                for (int i = xBase; i < xend; i++)
                {
                    pixels[i + offset1] = unchecked((int)0xff000000) | warp_Color.add(texture.pixel[(tx >> 8) + offset2], pixels[i + offset1]);
                    tx += dtx;
                }
                ty += dty;
            }
        }

        public void Dispose()
        {
            if (image != null)
            {
                handle.Free();
                image.Dispose();
                image = null;
                pixels = null;
            }
        }
    }
}