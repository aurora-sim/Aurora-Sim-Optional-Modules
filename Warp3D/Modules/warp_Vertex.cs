/*
 * Copyright (c) Peter Walser and Dean Lunz
 * All rights reserved.
 *
 * Licensed under the Creative Commons Attribution Share-Alike 2.5 Canada
 * license: http://creativecommons.org/licenses/by-sa/2.5/ca/
 */

using System;
using System.Collections.Generic;

namespace Rednettle.Warp3D
{
    /// <summary>
    /// Summary description for warp_Vertex.
    /// </summary>
    public class warp_Vertex
    {
        public warp_Vector pos;   //(x,y,z) Coordinate of vertex
        public warp_Vector n;   //Normal Vector at vertex
        public float u; // Texture x-coordinate (relative)
        public float v; // Texture y-coordinate (relative)

        internal warp_Object parent;

        internal warp_Vector pos2;  //Transformed vertex coordinate
        internal warp_Vector n2;  //Transformed normal vector (camera space)

        internal int x;  //Projected x coordinate
        internal int y;  //Projected y coordinate
        internal int z;  //Projected z coordinate for z-Buffer

        internal int nx; // Normal x-coordinate for envmapping
        internal int ny; // Normal y-coordinate for envmapping
        internal int tx; // Texture x-coordinate (absolute)
        internal int ty; // Texture y-coordinate (absolute)

        internal float sw = 1.0f;

        internal int clipcode;
        internal int id; // Vertex index

        internal List<warp_Triangle> neighbor;

        public warp_Vertex()
        {
        }

        public warp_Vertex(warp_Vector ppos)
        {
            pos = ppos;
        }

        public warp_Vertex(warp_Vector ppos, warp_Vector norm)
        {
            pos = ppos;
            n = norm;
        }

        public warp_Vertex(warp_Vector ppos, float u, float v)
        {
            pos = ppos;
            this.u = u;
            this.v = v;
        }

        public warp_Vertex(warp_Vector ppos, warp_Vector norm, float u, float v)
        {
            pos = ppos;
            n = norm;
            this.u = u;
            this.v = v;
        }

        public void project(warp_Matrix vertexProjection, warp_Matrix normalProjection, warp_Camera camera)
        // Projects this vertex into camera space
        {
            pos2 = pos.transform(vertexProjection);
            n2 = n.transform(normalProjection);

            float fact;
            if (camera.isOrthographic)
            {
                x = (int)(pos2.x * (camera.screenscale / camera.orthoViewWidth) + (camera.screenwidth >> 1));
                y = (int)(-pos2.y * (camera.screenscale / camera.orthoViewHeight) + (camera.screenheight >> 1));
            }
            else
            {
                fact = camera.screenscale / camera.fovfact / ((pos2.z > 0.1) ? pos2.z : 0.1f);
                x = (int)(pos2.x * fact + (camera.screenwidth >> 1));
                y = (int)(-pos2.y * fact + (camera.screenheight >> 1));
            }

            z = (int)(65536f * pos2.z);
            sw = -(pos2.z);
            nx = (int)(n2.x * 127 + 127);
            ny = (int)(n2.y * 127 + 127);
            if (parent.material == null) return;
            if (parent.material.texture == null) return;
            tx = (int)((float)parent.material.texture.width * u);
            ty = (int)((float)parent.material.texture.height * v);
        }

        public void setUV(float u, float v)
        {
            this.u = u;
            this.v = v;
        }

        public void clipFrustrum(int w, int h)
        {
            // View plane clipping
            clipcode = 0;
            if (x < 0) clipcode |= 1;
            if (x >= w) clipcode |= 2;
            if (y < 0) clipcode |= 4;
            if (y >= h) clipcode |= 8;
            if (pos2.z < 0) clipcode |= 16;
        }

        public void registerNeighbor(warp_Triangle triangle)
        {
            if (!neighbor.Contains(triangle)) neighbor.Add(triangle);
        }

        public void resetNeighbors()
        {
            if (neighbor != null)
                neighbor.Clear();
            else
                neighbor = new List<warp_Triangle>();
        }

        public void regenerateNormal()
        {
            warp_Vector wn;
            float nx = 0f;
            float ny = 0f;
            float nz = 0f;

            for (int i = 0; i < neighbor.Count; i++)
            {
                wn = neighbor[i].getWeightedNormal();
                nx += wn.x;
                ny += wn.y;
                nz += wn.z;
            }

            n = new warp_Vector(nx, ny, nz).normalize();
        }

        public void scaleTextureCoordinates(float fx, float fy)
        {
            u *= fx;
            v *= fy;
        }

        public void moveTextureCoordinates(float fx, float fy)
        {
            u += fx;
            v += fy;
        }

        public warp_Vertex getClone()
        {
            warp_Vertex newVertex = new warp_Vertex();
            newVertex.pos = pos;
            newVertex.n = n;
            newVertex.u = u;
            newVertex.v = v;

            return newVertex;
        }

        public bool equals(warp_Vertex v)
        {
            return ((pos.x == v.pos.x) && (pos.y == v.pos.y) && (pos.z == v.pos.z));
        }

        public bool equals(warp_Vertex v, float tolerance)
        {
            return Math.Abs(warp_Vector.sub(pos, v.pos).length()) < tolerance;
        }
    }
}
