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
    /// Summary description for warp_Triangle.
    /// </summary>
    public class warp_Triangle
    {
        public warp_Object parent;  // the object which obtains this triangle
        public bool visible = true;  //visibility tag for clipping
        public bool outOfFrustrum;  //visibility tag for frustrum clipping

        public warp_Vertex p1;  // first  vertex
        public warp_Vertex p2;  // second vertex
        public warp_Vertex p3;  // third  vertex

        public warp_Vector n;  // Normal vector of flat triangle
        public warp_Vector n2; // Projected Normal vector

        //private int minx,maxx,miny,maxy; // for clipping
        public float dist;

        public int id;

        public warp_Triangle(warp_Vertex a, warp_Vertex b, warp_Vertex c)
        {
            p1 = a;
            p2 = b;
            p3 = c;
        }

        public void clipFrustrum(int w, int h)
        {
            if (parent.material == null) { visible = false; return; }
            outOfFrustrum = (p1.clipcode & p2.clipcode & p3.clipcode) != 0;
            if (outOfFrustrum) { visible = false; return; }
            if (n2.z > 0.5) { visible = true; return; }

            warp_Vector triangleCenter = new warp_Vector(
                (p1.pos2.x + p2.pos2.x + p3.pos2.x),
                (p1.pos2.y + p2.pos2.y + p3.pos2.y),
                (p1.pos2.z + p2.pos2.z + p3.pos2.z));

            visible = warp_Vector.angle(triangleCenter, n2) > 0;

        }

        public void project(warp_Matrix normalProjection)
        {
            n2 = n.transform(normalProjection);
            dist = getDist();
        }

        public void regenerateNormal()
        {
            n = warp_Vector.getNormal(p1.pos, p2.pos, p3.pos);
        }

        public warp_Vector getWeightedNormal()
        {
            return warp_Vector.vectorProduct(p1.pos, p2.pos, p3.pos);
        }

        public warp_Vertex getMedium()
        {
            float cx = (p1.pos.x + p2.pos.x + p3.pos.x) / 3;
            float cy = (p1.pos.y + p2.pos.y + p3.pos.y) / 3;
            float cz = (p1.pos.z + p2.pos.z + p3.pos.z) / 3;
            float cu = (p1.u + p2.u + p3.u) / 3;
            float cv = (p1.v + p2.v + p3.v) / 3;
            return new warp_Vertex(new warp_Vector(cx, cy, cz), cu, cv);
        }

        public warp_Vector getCenter()
        {
            float cx = (p1.pos.x + p2.pos.x + p3.pos.x) / 3;
            float cy = (p1.pos.y + p2.pos.y + p3.pos.y) / 3;
            float cz = (p1.pos.z + p2.pos.z + p3.pos.z) / 3;
            return new warp_Vector(cx, cy, cz);
        }

        public float getDist()
        {
            return p1.z + p2.z + p3.z;
        }

        public bool degenerated()
        {
            return p1.equals(p2) || p2.equals(p3) || p3.equals(p1);
        }

        public warp_Triangle getClone()
        {
            return new warp_Triangle(p1, p2, p3);
        }
    }
}