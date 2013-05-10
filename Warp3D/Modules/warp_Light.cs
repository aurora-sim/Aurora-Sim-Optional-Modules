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
    /// Summary description for warp_Light.
    /// </summary>
    public class warp_Light
    {
        // F I E L D S

        public warp_Vector v; //Light direction
        public warp_Vector v2; //projected Light direction
        public int diffuse;
        public int specular;
        public int highlightSheen;
        public int highlightSpread;

        warp_Matrix matrix2;

        public warp_Light(warp_Vector direction)
        {
            v = direction;
            v.normalize();
        }

        public warp_Light(warp_Vector direction, int diffuse)
        {
            v = direction;
            v.normalize();
            this.diffuse = diffuse;
        }

        public warp_Light(warp_Vector direction, int color, int highlightSheen, int highlightSpread)
        {
            v = direction;
            v.normalize();
            this.diffuse = color;
            this.specular = color;
            this.highlightSheen = highlightSheen;
            this.highlightSpread = highlightSpread;
        }

        public warp_Light(warp_Vector direction, int diffuse, int specular, int highlightSheen, int highlightSpread)
        {
            v = direction;
            v.normalize();
            this.diffuse = diffuse;
            this.specular = specular;
            this.highlightSheen = highlightSheen;
            this.highlightSpread = highlightSpread;
        }

        public void project(warp_Matrix m)
        {
            matrix2 = m.getClone();
            matrix2.transform(m);
            v2 = v.transform(matrix2);
        }
    }
}