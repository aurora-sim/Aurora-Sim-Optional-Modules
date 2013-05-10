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
    /// Summary description for warp_Edge.
    /// </summary>
    /// 
    public class warp_Edge
    {
        warp_Vertex a, b;

        public warp_Edge()
        {
        }

        public warp_Edge(warp_Vertex v1, warp_Vertex v2)
        {
            a = v1;
            b = v2;
        }

        public warp_Vertex start()
        {
            return a;
        }

        public warp_Vertex end()
        {
            return b;
        }
    }
}