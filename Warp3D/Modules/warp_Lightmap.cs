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
    /// Summary description for warp_Lightmap.
    /// </summary>
    public class warp_Lightmap
    {
        public int[] diffuse = new int[65536];
        public int[] specular = new int[65536];
        private float[] sphere = new float[65536];
        private warp_Light[] light;
        private int lights;
        private int ambient;
        //private int temp, overflow, color, r, g, b;
        private int pos;

        public warp_Lightmap(warp_Scene scene)
        {
            scene.rebuild();

            light = scene.light;
            lights = scene.lights;
            ambient = scene.environment.ambient;

            buildSphereMap();
            rebuildLightmap();
        }

        private void buildSphereMap()
        {
            float fnx, fny, fnz;
            int pos;
            for (int ny = -128; ny < 128; ny++)
            {
                fny = (float)ny / 128;
                for (int nx = -128; nx < 128; nx++)
                {
                    pos = nx + 128 + ((ny + 128) << 8);
                    fnx = (float)nx / 128;
                    fnz = (float)(1 - Math.Sqrt(fnx * fnx + fny * fny));
                    sphere[pos] = (fnz > 0) ? fnz : 0;
                }
            }
        }

        public void rebuildLightmap()
        {
            //System.Console.WriteLine(">> Rebuilding Light Map  ...  [" + lights + " light sources]");
            warp_Vector l;
            float fnx, fny, phongfact, sheen, spread;
            int diffuse, specular, cos, dr, dg, db, sr, sg, sb;
            for (int ny = -128; ny < 128; ny++)
            {
                fny = (float)ny / 128;
                for (int nx = -128; nx < 128; nx++)
                {
                    pos = nx + 128 + ((ny + 128) << 8);
                    fnx = (float)nx / 128;
                    sr = sg = sb = 0;

                    dr = warp_Color.getRed(ambient);
                    dg = warp_Color.getGreen(ambient);
                    db = warp_Color.getBlue(ambient);

                    for (int i = 0; i < lights; i++)
                    {
                        l = light[i].v;
                        diffuse = light[i].diffuse;
                        specular = light[i].specular;
                        sheen = (float)light[i].highlightSheen / 255f;
                        spread = (float)light[i].highlightSpread / 4096;
                        spread = (spread < 0.01f) ? 0.01f : spread;
                        cos = (int)(255 * warp_Vector.angle(light[i].v, new warp_Vector(fnx, fny, sphere[pos])));
                        cos = (cos > 0) ? cos : 0;
                        dr += (warp_Color.getRed(diffuse) * cos) >> 8;
                        dg += (warp_Color.getGreen(diffuse) * cos) >> 8;
                        db += (warp_Color.getBlue(diffuse) * cos) >> 8;
                        phongfact = sheen * (float)Math.Pow((float)cos / 255f, 1 / spread);
                        sr += (int)((float)warp_Color.getRed(specular) * phongfact);
                        sg += (int)((float)warp_Color.getGreen(specular) * phongfact);
                        sb += (int)((float)warp_Color.getBlue(specular) * phongfact);
                    }
                    this.diffuse[pos] = warp_Color.getCropColor(dr, dg, db);
                    this.specular[pos] = warp_Color.getCropColor(sr, sg, sb);
                }
            }
        }
    }
}