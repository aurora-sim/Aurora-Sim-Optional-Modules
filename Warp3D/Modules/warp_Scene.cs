/*
 * Copyright (c) Peter Walser and Dean Lunz
 * All rights reserved.
 *
 * Licensed under the Creative Commons Attribution Share-Alike 2.5 Canada
 * license: http://creativecommons.org/licenses/by-sa/2.5/ca/
 */

using System;
using System.Collections;
using System.Drawing;
using System.Timers;

namespace Rednettle.Warp3D
{
    /// <summary>
    /// Summary description for warp_Scene.
    /// </summary>
    public class warp_Scene : warp_CoreObject
    {
        public static String version = "1.0.0";
        public static String release = "0010";

        public warp_RenderPipeline renderPipeline;

        public warp_Environment environment = new warp_Environment();
        public warp_Camera defaultCamera = warp_Camera.FRONT();

        public warp_Object[] wobject;
        public warp_Light[] light;

        public int objects = 0;
        public int lights = 0;

        public bool autoCalcNormals = true;

        private bool objectsNeedRebuild = true;
        private bool lightsNeedRebuild = true;

        protected bool preparedForRendering = false;

        public warp_Vector normalizedOffset;
        public float normalizedScale = 1f;

        public Hashtable objectData = new Hashtable();
        public Hashtable lightData = new Hashtable();
        public Hashtable materialData = new Hashtable();
        public Hashtable cameraData = new Hashtable();

        public warp_Scene(int width, int height)
        {
            renderPipeline = new warp_RenderPipeline(this, width, height);
        }

        public void removeAllObjects()
        {
            objectData = new Hashtable();
            objectsNeedRebuild = true;
            rebuild();
        }

        public void rebuild()
        {
            if (objectsNeedRebuild)
            {
                objectsNeedRebuild = false;

                objects = objectData.Count;
                wobject = new warp_Object[objects];
                IDictionaryEnumerator enumerator = objectData.GetEnumerator();

                for (int i = objects - 1; i >= 0; i--)
                {
                    enumerator.MoveNext();
                    wobject[i] = (warp_Object)enumerator.Value;

                    wobject[i].id = i;
                    wobject[i].rebuild();
                }
            }

            if (lightsNeedRebuild)
            {
                lightsNeedRebuild = false;
                lights = lightData.Count;
                light = new warp_Light[lights];
                IDictionaryEnumerator enumerator = lightData.GetEnumerator();
                for (int i = lights - 1; i >= 0; i--)
                {
                    enumerator.MoveNext();
                    light[i] = (warp_Light)enumerator.Value;

                }
            }
        }

        public warp_Object sceneobject(String key)
        {
            return (warp_Object)objectData[key];
        }

        public warp_Material material(String key)
        {
            return (warp_Material)materialData[key];
        }

        public warp_Camera camera(String key)
        {
            return (warp_Camera)cameraData[key];
        }

        public void addObject(String key, warp_Object obj)
        {
            obj.name = key;
            objectData.Add(key, obj);
            obj.parent = this;
            objectsNeedRebuild = true;
        }

        public void removeObject(String key)
        {
            objectData.Remove(key);
            objectsNeedRebuild = true;
            preparedForRendering = false;
        }

        public void addMaterial(String key, warp_Material m)
        {
            materialData.Add(key, m);
        }

        public void removeMaterial(String key)
        {
            materialData.Remove(key);
        }

        public void addCamera(String key, warp_Camera c)
        {
            cameraData.Add(key, c);
        }

        public void removeCamera(String key)
        {
            cameraData.Remove(key);
        }

        public void addLight(String key, warp_Light l)
        {
            lightData.Add(key, l);
            lightsNeedRebuild = true;
        }

        public void removeLight(String key)
        {
            lightData.Remove(key);
            lightsNeedRebuild = true;
            preparedForRendering = false;
        }

        public void prepareForRendering()
        {
            if (preparedForRendering) return;
            preparedForRendering = true;

            //System.Console.WriteLine("warp_Scene| prepareForRendering : Preparing for realtime rendering ...");
            rebuild();
            renderPipeline.buildLightMap();
            printSceneInfo();
        }

        public void printSceneInfo()
        {
            //System.Console.WriteLine("warp_Scene| Objects   : "+objects);
            //System.Console.WriteLine("warp_Scene| Vertices  : "+countVertices());
            //System.Console.WriteLine("warp_Scene| Triangles : "+countTriangles());
        }

        public void render()
        {
            renderPipeline.render(this.defaultCamera);
        }

        public Bitmap getImage()
        {
            return renderPipeline.screen.getImage();
        }

        public void setBackgroundColor(int bgcolor)
        {
            environment.bgcolor = bgcolor;
        }

        public void setBackground(warp_Texture t)
        {
            environment.setBackground(t);
        }

        public void setAmbient(int ambientcolor)
        {
            environment.ambient = ambientcolor;
        }

        public int countVertices()
        {
            int counter = 0;
            for (int i = 0; i < objects; i++) counter += wobject[i].vertices;
            return counter;
        }

        public int countTriangles()
        {
            int counter = 0;
            for (int i = 0; i < objects; i++) counter += wobject[i].triangles;
            return counter;
        }

        public void useIdBuffer(bool buffer)
        {
            renderPipeline.useIdBuffer(buffer);
        }

        public warp_Triangle identifyTriangleAt(int xpos, int ypos)
        {
            if (!renderPipeline.useId)
            {
                return null;
            }
            if (xpos < 0 || xpos >= renderPipeline.screen.width)
            {
                return null;
            }
            if (ypos < 0 || ypos >= renderPipeline.screen.height)
            {
                return null;
            }

            int pos = xpos + renderPipeline.screen.width * ypos;
            int idCode = renderPipeline.idBuffer[pos];
            if (idCode < 0)
            {
                return null;
            }
            return wobject[idCode >> 16].fasttriangle[idCode & 0xFFFF];
        }

        public warp_Object identifyObjectAt(int xpos, int ypos)
        {
            warp_Triangle tri = identifyTriangleAt(xpos, ypos);
            if (tri == null)
            {
                return null;
            }
            return tri.parent;
        }
    }
}