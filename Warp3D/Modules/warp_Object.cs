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
    /// Summary description for warp_Object.
    /// </summary>
    public class warp_Object : warp_CoreObject
    {
        public Object userData;	// Can be freely used
        public String user; 		// Can be freely used

        public List<warp_Vertex> vertexData;
        public List<warp_Triangle> triangleData;

        public int id;  // This object's index
        public String name;  // This object's name
        public bool visible; // Visibility tag
        public bool projected;
        public warp_Scene parent;
        private bool dirty;  // Flag for dirty handling

        public warp_Vertex[] fastvertex;
        public warp_Triangle[] fasttriangle;

        public int vertices;
        public int triangles;

        public warp_Material material;
        public int index;
        public int offset;
        public int split;

        public warp_Object()
        {
            vertexData = new List<warp_Vertex>();
            triangleData = new List<warp_Triangle>();

            name = String.Empty;
            dirty = true;
            visible = true;
        }

        public warp_Object(int vertexCount, int triangleCount)
        {
            vertexData = new List<warp_Vertex>(vertexCount);
            triangleData = new List<warp_Triangle>(triangleCount);

            name = String.Empty;
            dirty = true;
            visible = true;
        }

        public warp_Vertex vertex(int id)
        {
            return vertexData[id];
        }

        public warp_Triangle triangle(int id)
        {
            return triangleData[id];
        }

        public void addVertex(warp_Vector pos)
        {
            addVertex(new warp_Vertex(pos));
        }

        public void addVertex(warp_Vector pos, warp_Vector norm)
        {
            addVertex(new warp_Vertex(pos, norm));
        }

        public void addVertex(warp_Vector pos, float u, float v)
        {
            addVertex(new warp_Vertex(pos, u, v));
        }

        public void addVertex(warp_Vector pos, warp_Vector norm, float u, float v)
        {
            addVertex(new warp_Vertex(pos, norm, u, v));
        }

        public void addVertex(warp_Vertex newVertex)
        {
            newVertex.parent = this;
            vertexData.Add(newVertex);
            dirty = true;
        }

        public void addTriangle(warp_Triangle newTriangle)
        {
            newTriangle.parent = this;
            triangleData.Add(newTriangle);
            dirty = true;
        }

        public void addTriangle(int v1, int v2, int v3)
        {
            addTriangle(vertex(v1), vertex(v2), vertex(v3));
        }

        public void removeVertex(warp_Vertex v)
        {
            vertexData.Remove(v);
        }

        public void removeTriangle(warp_Triangle t)
        {
            triangleData.Remove(t);
        }

        public void removeVertexAt(int pos)
        {
            vertexData.RemoveAt(pos);
        }

        public void removeTriangleAt(int pos)
        {
            triangleData.RemoveAt(pos);
        }

        public void setMaterial(warp_Material m)
        {
            material = m;
        }

        public void rebuild()
        {
            if (!dirty) return;
            dirty = false;

            // Generate faster structure for vertices
            vertices = vertexData.Count;
            fastvertex = new warp_Vertex[vertices];

            //List<warp_Vertex>.Enumerator enumerator = vertexData.GetEnumerator();
            int j = 0;
            for (int i = vertices - 1; i >= 0; i--)
            {
                fastvertex[i] = vertexData[j++];
                fastvertex[i].id = i;
            }

            // Generate faster structure for triangles
            triangles = triangleData.Count;
            fasttriangle = new warp_Triangle[triangles];

            //List<warp_Triangle>.Enumerator enumerator2 = triangleData.GetEnumerator();
            j = 0;
            for (int i = triangles - 1; i >= 0; i--)
            {
                fasttriangle[i] = triangleData[j++];
                fasttriangle[i].id = i;
            }

            if (parent.autoCalcNormals)
            {
                for (int i = vertices - 1; i >= 0; i--)
                {
                    fastvertex[i].resetNeighbors();
                }

                warp_Triangle tri;
                for (int i = triangles - 1; i >= 0; i--)
                {
                    tri = fasttriangle[i];
                    tri.p1.registerNeighbor(tri);
                    tri.p2.registerNeighbor(tri);
                    tri.p3.registerNeighbor(tri);
                }
            }

            regenerate();
        }

        public void addTriangle(warp_Vertex a, warp_Vertex b, warp_Vertex c)
        {
            addTriangle(new warp_Triangle(a, b, c));
        }

        public void regenerate()
        // Regenerates the triangle and vertex normals
        {
            for (int i = 0; i < triangles; i++)
                fasttriangle[i].regenerateNormal();

            if (parent.autoCalcNormals)
            {
                for (int i = 0; i < vertices; i++)
                    fastvertex[i].regenerateNormal();
            }
        }

        public void remapUV(int w, int h, float sx, float sy)
        {
            rebuild();
            for (int j = 0, p = 0; j < h; j++)
            {
                float v = ((float)j / (float)(h - 1)) * sy;
                for (int i = 0; i < w; i++)
                {
                    float u = ((float)i / (float)(w - 1)) * sx;
                    fastvertex[p++].setUV(u, v);
                }
            }
        }

        /*
        public void tilt(float fact)
        {
            rebuild();
            for (int i=0;i<vertices;i++)
                fastvertex[i].pos=warp_Vector.add(fastvertex[i].pos,warp_Vector.random(fact));

            regenerate();
        }
        */

        public warp_Vector minimum()
        {
            if (vertices == 0) return new warp_Vector();
            float minX = fastvertex[0].pos.x;
            float minY = fastvertex[0].pos.y;
            float minZ = fastvertex[0].pos.z;
            for (int i = 1; i < vertices; i++)
            {
                if (fastvertex[i].pos.x < minX) minX = fastvertex[i].pos.x;
                if (fastvertex[i].pos.y < minY) minY = fastvertex[i].pos.y;
                if (fastvertex[i].pos.z < minZ) minZ = fastvertex[i].pos.z;
            }

            return new warp_Vector(minX, minY, minZ);
        }

        public warp_Vector maximum()
        {
            if (vertices == 0) return new warp_Vector();
            float maxX = fastvertex[0].pos.x;
            float maxY = fastvertex[0].pos.y;
            float maxZ = fastvertex[0].pos.z;
            for (int i = 1; i < vertices; i++)
            {
                if (fastvertex[i].pos.x > maxX) maxX = fastvertex[i].pos.x;
                if (fastvertex[i].pos.y > maxY) maxY = fastvertex[i].pos.y;
                if (fastvertex[i].pos.z > maxZ) maxZ = fastvertex[i].pos.z;
            }
            return new warp_Vector(maxX, maxY, maxZ);
        }


        public void detach()
        // Centers the object in its coordinate system
        // The offset from origin to object center will be transfered to the matrix,
        // so your object actually does not move.
        // Usefull if you want prepare objects for self rotation.
        {
            warp_Vector center = getCenter();

            for (int i = 0; i < vertices; i++)
            {
                fastvertex[i].pos.x -= center.x;
                fastvertex[i].pos.y -= center.y;
                fastvertex[i].pos.z -= center.z;
            }

            shift(center);
        }

        public warp_Vector getCenter()
        // Returns the center of this object
        {
            warp_Vector max = maximum();
            warp_Vector min = minimum();

            return new warp_Vector((max.x + min.x) / 2, (max.y + min.y) / 2, (max.z + min.z) / 2);
        }

        public warp_Vector getDimension()
        // Returns the x,y,z - Dimension of this object
        {
            warp_Vector max = maximum();
            warp_Vector min = minimum();

            return new warp_Vector(max.x - min.x, max.y - min.y, max.z - min.z);
        }

        public void matrixMeltdown()
        // Applies the transformations in the matrix to all vertices
        // and resets the matrix to untransformed.
        {
            rebuild();
            for (int i = vertices - 1; i >= 0; i--)
                fastvertex[i].pos = fastvertex[i].pos.transform(matrix);

            regenerate();
            matrix.reset();
            normalmatrix.reset();
        }

        //public warp_Object getClone()
        //{
        //    warp_Object obj=new warp_Object();
        //    rebuild();
        //    for(int i=0;i<vertices;i++) obj.addVertex(fastvertex[i].getClone());
        //    for(int i=0;i<triangles;i++) obj.addTriangle(fasttriangle[i].getClone());
        //    obj.name=name+" [cloned]";
        //    obj.material=material;
        //    obj.matrix=matrix.getClone();
        //    obj.normalmatrix=normalmatrix.getClone();
        //    obj.rebuild();
        //    return obj;
        //}
        /*
                public void removeDuplicateVertices()
                {
                    rebuild();
                    Vector edgesToCollapse=new Vector();
                    for (int i=0;i<vertices;i++)
                        for (int j=i+1;j<vertices;j++)
                            if (vertex[i].equals(vertex[j],0.0001f))
                                edgesToCollapse.addElement(new warp_Edge(vertex[i],vertex[j]));


                    Enumeration enum=edgesToCollapse.elements();
                    while(enum.hasMoreElements()) 
                    {
                        edgeCollapse((warp_Edge)enum.nextElement());
                    }

                    removeDegeneratedTriangles();
                }

                public void removeDegeneratedTriangles()
                {
                    rebuild();
                    for (int i=0;i<triangles;i++)
                        if (triangle[i].degenerated()) removeTriangleAt(i);

                    dirty=true;
                    rebuild();
                }

                private void edgeCollapse(warp_Edge edge)
                // Collapses the edge [u,v] by replacing v by u
                {
                    warp_Vertex u=edge.start();
                    warp_Vertex v=edge.end();
                    if (!vertexData.contains(u)) return;
                    if (!vertexData.contains(v)) return;
                    rebuild();
                    warp_Triangle tri;
                    for (int i=0; i<triangles; i++)
                    {
                        tri=triangle(i);
                        if (tri.p1==v) tri.p1=u;
                        if (tri.p2==v) tri.p2=u;
                        if (tri.p3==v) tri.p3=u;
                    }
                    vertexData.removeElement(v);
                }
                */

    }
}