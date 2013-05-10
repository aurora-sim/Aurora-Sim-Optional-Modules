/*
 * Copyright (c) Peter Walser and Dean Lunz
 * All rights reserved.
 *
 * Licensed under the Creative Commons Attribution Share-Alike 2.5 Canada
 * license: http://creativecommons.org/licenses/by-sa/2.5/ca/
 */

using System;
using System.Collections;

namespace Rednettle.Warp3D
{
    /// <summary>
    /// Summary description for warp_RenderPipeline.
    /// </summary>
    public class warp_RenderPipeline : IDisposable
    {
        public warp_Screen screen;
        warp_Scene scene;
        public warp_Lightmap lightmap;

        //private bool resizingRequested = false;
        //private int requestedWidth;
        //private int requestedHeight;
        //private double splitOffset;
        public bool useId = false;

        warp_Rasterizer rasterizer;

        ArrayList opaqueQueue = new ArrayList();
        ArrayList transparentQueue = new ArrayList();

        int zFar = 0xffffff;

        public int[] zBuffer;
        public int[] idBuffer;

        public warp_RenderPipeline(warp_Scene scene, int w, int h)
        {
            this.scene = scene;

            screen = new warp_Screen(w, h);
            zBuffer = new int[screen.width * screen.height];
            rasterizer = new warp_Rasterizer(this);
        }

        public void buildLightMap()
        {
            if (lightmap == null)
            {
                lightmap = new warp_Lightmap(scene);
            }
            else
            {
                lightmap.rebuildLightmap();
            }

            rasterizer.loadLightmap(lightmap);
        }

        public void useIdBuffer(bool useId)
        {
            this.useId = useId;
            if (useId)
            {
                idBuffer = new int[screen.width * screen.height];
            }
            else
            {
                idBuffer = null;
            }
        }

        public void render(warp_Camera cam)
        {
            rasterizer.rebuildReferences(this);

            warp_Math.clearBuffer(zBuffer, zFar);
            //System.Array.Copy(screen.zBuffer,0,zBuffer,0,zBuffer.Length);

            if (useId)
            {
                warp_Math.clearBuffer(idBuffer, -1);
            }
            if (scene.environment.background != null)
            {
                screen.drawBackground(scene.environment.background, 0, 0, screen.width, screen.height);
            }
            else
            {
                screen.clear(scene.environment.bgcolor);
            }

            cam.setScreensize(screen.width, screen.height);
            scene.prepareForRendering();
            emptyQueues();

            // Project
            warp_Matrix m = warp_Matrix.multiply(cam.getMatrix(), scene.matrix);
            warp_Matrix nm = warp_Matrix.multiply(cam.getNormalMatrix(), scene.normalmatrix);
            warp_Matrix vertexProjection, normalProjection;
            warp_Object obj;
            warp_Triangle t;
            warp_Vertex v;
            int w = screen.width;
            int h = screen.height;
            for (int id = scene.objects - 1; id >= 0; id--)
            {
                obj = scene.wobject[id];
                if (obj.visible)
                {
                    vertexProjection = obj.matrix.getClone();
                    normalProjection = obj.normalmatrix.getClone();
                    vertexProjection.transform(m);
                    normalProjection.transform(nm);

                    for (int i = obj.vertices - 1; i >= 0; i--)
                    {
                        v = obj.fastvertex[i];
                        v.project(vertexProjection, normalProjection, cam);
                        v.clipFrustrum(w, h);
                    }
                    for (int i = obj.triangles - 1; i >= 0; i--)
                    {
                        t = obj.fasttriangle[i];
                        t.project(normalProjection);
                        t.clipFrustrum(w, h);
                        enqueueTriangle(t);
                    }
                }
            }

            //screen.lockImage();

            warp_Triangle[] tri;
            tri = getOpaqueQueue();
            if (tri != null)
            {
                for (int i = tri.GetLength(0) - 1; i >= 0; i--)
                {
                    rasterizer.loadMaterial(tri[i].parent.material);
                    rasterizer.render(tri[i]);
                }
            }

            tri = getTransparentQueue();
            if (tri != null)
            {
                for (int i = 0; i < tri.GetLength(0); i++)
                {
                    rasterizer.loadMaterial(tri[i].parent.material);
                    rasterizer.render(tri[i]);
                }
            }

            //screen.unlockImage();
        }

        private void performResizing()
        {
            //resizingRequested = false;
            //screen.resize(requestedWidth, requestedHeight);

            zBuffer = new int[screen.width * screen.height];
            if (useId)
            {
                idBuffer = new int[screen.width * screen.height];
            }
        }

        // Triangle sorting
        private void emptyQueues()
        {
            opaqueQueue.Clear();
            transparentQueue.Clear();
        }

        private void enqueueTriangle(warp_Triangle tri)
        {
            if (tri.parent.material == null)
            {
                return;
            }
            if (tri.visible == false)
            {
                return;
            }
            if ((tri.parent.material.transparency == 255) &&
                (tri.parent.material.reflectivity == 0))
            {
                return;
            }

            if (tri.parent.material.transparency > 0)
            {
                transparentQueue.Add(tri);
            }
            else
            {
                opaqueQueue.Add(tri);
            }
        }

        private warp_Triangle[] getOpaqueQueue()
        {
            if (opaqueQueue.Count == 0) return null;

            IEnumerator enumerator = opaqueQueue.GetEnumerator();
            warp_Triangle[] tri = new warp_Triangle[opaqueQueue.Count];

            int id = 0;
            while (enumerator.MoveNext())
                tri[id++] = (warp_Triangle)enumerator.Current;

            return sortTriangles(tri, 0, tri.GetLength(0) - 1);
        }

        private warp_Triangle[] getTransparentQueue()
        {
            if (transparentQueue.Count == 0) return null;
            IEnumerator enumerator = transparentQueue.GetEnumerator();
            warp_Triangle[] tri = new warp_Triangle[transparentQueue.Count];

            int id = 0;
            while (enumerator.MoveNext())
                tri[id++] = (warp_Triangle)enumerator.Current;

            return sortTriangles(tri, 0, tri.GetLength(0) - 1);
        }

        private warp_Triangle[] sortTriangles(warp_Triangle[] tri, int L, int R)
        {
            //FIX: Added Index bounds checking. (Was causing random exceptions) - Created by: X
            if (L < 0) L = 0;
            if (L > tri.Length) L = tri.Length;
            if (R < 0) R = 0;
            if (R > tri.Length) R = tri.Length;
            // - Created by: X

            float m = (tri[L].dist + tri[R].dist) * 0.5f;
            int i = L;
            int j = R;
            warp_Triangle temp;

            do
            {
                while (tri[i].dist > m) i++;
                while (tri[j].dist < m) j--;

                if (i <= j)
                {
                    temp = tri[i];
                    tri[i] = tri[j];
                    tri[j] = temp;
                    i++;
                    j--;
                }
            }
            while (j >= i);

            if (L < j) sortTriangles(tri, L, j);
            if (R > i) sortTriangles(tri, i, R);

            return tri;
        }

        public void Dispose()
        {

        }
    }
}