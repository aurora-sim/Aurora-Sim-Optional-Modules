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
    /// Summary description for warp_Environment.
    /// </summary>
    public class warp_Environment
    {
        public int ambient = 0;
        public int fogcolor = 0;
        public int fogfact = 0;
        public int bgcolor = unchecked((int)0xffffffff);

        public warp_Texture background = null;

        public void setBackground(warp_Texture t)
        {
            background = t;
        }
    }
}