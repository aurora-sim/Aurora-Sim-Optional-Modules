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
    /// Summary description for warp_TextureSettings.
    /// </summary>
    /// 
    public class warp_TextureSettings
    {
        public warp_Texture texture;
        public int width;
        public int height;
        public int type;
        public float persistency;
        public float density;
        public int samples;
        public int numColors;
        public int[] colors;

        public warp_TextureSettings(warp_Texture tex, int w, int h, int t, float p, float d, int s, int[] c)
        {
            texture = tex;
            width = w;
            height = h;
            type = t;
            persistency = p;
            density = d;
            samples = s;
            colors = c;
        }
    }
}