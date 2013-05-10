/*
 * Copyright (c) Peter Walser and Dean Lunz
 * All rights reserved.
 *
 * Licensed under the Creative Commons Attribution Share-Alike 2.5 Canada
 * license: http://creativecommons.org/licenses/by-sa/2.5/ca/
 */

using System;
using System.Drawing;
using System.IO;

namespace Rednettle.Warp3D
{
    /// <summary>
    /// Summary description for warp_Material.
    /// </summary>
    public class warp_Material
    {
        public int color = 0;
        public int transparency = 0;
        public int reflectivity = 255;
        public int type = 0;
        public warp_Texture texture = null;
        public warp_Texture envmap = null;
        public bool flat = false;
        public bool wireframe = false;
        public bool opaque = true;
        public String texturePath = null;
        public String envmapPath = null;
        public warp_TextureSettings textureSettings = null;
        public warp_TextureSettings envmapSettings = null;
        public double rwx = 500.00;
        public double rwy = 500.00;
        public Point pt = new Point(0, 0);
        public int offset = 0;

        public warp_Material()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public warp_Material(int color)
        {
            setColor(color);
        }

        public warp_Material(warp_Texture t)
        {
            setTexture(t);
            reflectivity = 255;
        }

        public warp_Material(String path)
        {

            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);

            importFromStream(br);
        }

        private void importFromStream(BinaryReader inStream)
        {
            readSettings(inStream);
            readTexture(inStream, true);
            readTexture(inStream, false);
        }

        private void readSettings(BinaryReader inStream)
        {
            setColor(readInt(inStream));
            setTransparency(inStream.ReadByte());
            setReflectivity(inStream.ReadByte());
            setFlat(inStream.ReadBoolean());
        }

        private int readInt(BinaryReader inStream)
        {
            return (inStream.ReadByte() << 24) | (inStream.ReadByte() << 16) | (inStream.ReadByte() << 8) | inStream.ReadByte();
        }

        private string readString(BinaryReader inStream)
        {
            string result = "";
            byte inByte;
            while ((inByte = (byte)inStream.ReadByte()) != 60)
            {
                result += (char)inByte;
            }

            return result;
        }



        private void readTexture(BinaryReader inStream, bool textureId)
        {
            warp_Texture t = null;
            int id = inStream.ReadSByte();
            if (id == 1)
            {
                t = new warp_Texture("c:/source/warp3d/materials/skymap.jpg");

                if (t != null && textureId)
                {
                    texturePath = t.path;
                    textureSettings = null;
                    setTexture(t);
                }
                if (t != null && !textureId)
                {
                    envmapPath = t.path;
                    envmapSettings = null;
                    setEnvmap(t);
                }
            }

            if (id == 2)
            {
                int w = readInt(inStream);
                int h = readInt(inStream);
                int type = inStream.ReadSByte();

                float persistency = readInt(inStream);
                float density = readInt(inStream);

                persistency = .5f;
                density = .5f;

                int samples = inStream.ReadByte();
                int numColors = inStream.ReadByte();
                int[] colors = new int[numColors];
                for (int i = 0; i < numColors; i++)
                {
                    colors[i] = readInt(inStream);

                }
                if (type == 1)
                {
                    t = warp_TextureFactory.PERLIN(w, h, persistency, density, samples, 1024).colorize(warp_Color.makeGradient(colors, 1024));
                }
                if (type == 2)
                {
                    t = warp_TextureFactory.WAVE(w, h, persistency, density, samples, 1024).colorize(warp_Color.makeGradient(colors, 1024));
                }
                if (type == 3)
                {
                    t = warp_TextureFactory.GRAIN(w, h, persistency, density, samples, 20, 1024).colorize(warp_Color.makeGradient(colors, 1024));

                }

                if (textureId)
                {
                    texturePath = null;
                    textureSettings = new warp_TextureSettings(t, w, h, type, persistency, density, samples, colors);
                    setTexture(t);
                }
                else
                {
                    envmapPath = null;
                    envmapSettings = new warp_TextureSettings(t, w, h, type, persistency, density, samples, colors);
                    setEnvmap(t);
                }
            }
        }

        public void setTexture(warp_Texture t)
        {
            texture = t;
            if (texture != null) texture.resize();
        }

        public void setEnvmap(warp_Texture env)
        {
            envmap = env;
            env.resize(256, 256);
        }

        public void setColor(int c)
        {
            color = c;
        }

        public void setTransparency(int factor)
        {
            transparency = warp_Math.crop(factor, 0, 255);
            opaque = (transparency == 0);
        }

        public void setReflectivity(int factor)
        {
            reflectivity = warp_Math.crop(factor, 0, 255);
        }

        public void setFlat(bool flat)
        {
            this.flat = flat;
        }

        public void setWireframe(bool wireframe)
        {
            this.wireframe = wireframe;
        }

        public void setType(int type)
        {
            this.type = type;
        }

        public warp_Texture getTexture()
        {
            return texture;
        }

        public warp_Texture getEnvmap()
        {
            return envmap;
        }

        public int getColor()
        {
            return color;
        }

        public int getTransparency()
        {
            return transparency;
        }

        public int getReflectivity()
        {
            return reflectivity;
        }

        public int getType()
        {
            return type;
        }

        public bool isFlat()
        {
            return flat;
        }

        public bool isWireframe()
        {
            return wireframe;
        }
    }
}