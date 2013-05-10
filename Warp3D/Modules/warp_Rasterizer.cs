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
    /// Summary description for warp_Rasterizer.
    /// </summary>
    public class warp_Rasterizer
    {
        private const int F = 0;   	// FLAT
        private const int W = 1;	// WIREFRAME
        private const int P = 2;  	// PHONG
        private const int E = 4;  	// ENVMAP
        private const int T = 8; 	// TEXTURED

        private bool materialLoaded;
        private bool lightmapLoaded;
        public bool ready;

        // Current material settings
        private int color;
        private int currentColor;
        private int transparency;
        private int reflectivity;
        //private int refraction;
        private warp_Texture texture;
        private int[] envmap;
        private int[] diffuse;
        private int[] specular;
        //private short[] refractionMap;
        private int tw;
        private int th;
        private int tbitW;
        private int tbitH;

        // Rasterizer hints
        private int mode;
        private int shaded;

        warp_Vertex p1, p2, p3, tempVertex;

        private int
        bkgrd, c, s, lutID, //lutID is position in LUT (diffuse,envmap,specular)
        x1, x2, x3, x4, y1, y2, y3, z1, z2, z3, z4,
        x, y, z, dx, dy, dz, offset, pos, temp,
        xL, xR, xBase, zBase, xMax, dxL, dxR, dzBase,

        nx1, nx2, nx3, nx4, ny1, ny2, ny3, ny4,
        nxBase, nyBase,
        dnx, dny, nx, ny,
        dnxBase, dnyBase;

        private float
        tx1, tx2, tx3, tx4, ty1, ty2, ty3, ty4,
        txBase, tyBase,
        tx, ty, sw,
        dtxBase, dtyBase,
        dtx, dty,
        sw1, sw2, sw3, sw4, swBase, dsw, dswBase;

        warp_Screen screen;
        int[] zBuffer;
        int[] idBuffer;
        int width, height;
        bool useIdBuffer;
        //int zFar=0xffffff;
        int currentId;

        // Constructor
        public warp_Rasterizer(warp_RenderPipeline pipeline)
        {
            shaded = P | E | T;

            rebuildReferences(pipeline);
            loadLightmap(pipeline.lightmap);
        }

        // References
        public void rebuildReferences(warp_RenderPipeline pipeline)
        {
            screen = pipeline.screen;
            zBuffer = pipeline.zBuffer;
            idBuffer = pipeline.idBuffer;
            width = screen.width;
            height = screen.height;
            useIdBuffer = pipeline.useId;
        }

        // Lightmap loader
        public void loadLightmap(warp_Lightmap lm)
        {
            if (lm == null) return;
            diffuse = lm.diffuse;
            specular = lm.specular;
            lightmapLoaded = true;
            ready = lightmapLoaded && materialLoaded;
        }

        // Material loader
        public void loadMaterial(warp_Material material)
        {
            color = material.color;
            transparency = material.transparency;
            reflectivity = material.reflectivity;
            texture = material.texture;
            if (material.envmap != null) envmap = material.envmap.pixel;
            else envmap = null;

            if (texture != null)
            {
                tw = texture.width - 1;
                th = texture.height - 1;
                tbitW = texture.bitWidth;
                tbitH = texture.bitHeight;
            }

            mode = 0;
            if (!material.flat) mode |= P;
            if (envmap != null) mode |= E;
            if (texture != null) mode |= T;
            if (material.wireframe) mode |= W;
            materialLoaded = true;
            ready = lightmapLoaded && materialLoaded;
        }

        public void render(warp_Triangle tri)
        {
            if (!ready) { return; }
            if (tri.parent == null) { return; }
            if ((mode & W) != 0)
            {
                drawWireframe(tri, color);
                if ((mode & W) == 0) { return; }
            }

            p1 = tri.p1;
            p2 = tri.p2;
            p3 = tri.p3;

            if (p1.y > p2.y) { tempVertex = p1; p1 = p2; p2 = tempVertex; }
            if (p2.y > p3.y) { tempVertex = p2; p2 = p3; p3 = tempVertex; }
            if (p1.y > p2.y) { tempVertex = p1; p1 = p2; p2 = tempVertex; }

            if (p1.y >= height) { return; }
            if (p3.y < 0) { return; }
            if (p1.y == p3.y) { return; }

            if (mode == F)
            {
                lutID = (int)(tri.n2.x * 127 + 127) + ((int)(tri.n2.y * 127 + 127) << 8);
                c = warp_Color.multiply(color, diffuse[lutID]);
                s = warp_Color.scale(specular[lutID], reflectivity);
                currentColor = warp_Color.add(c, s);
            }

            currentId = (tri.parent.id << 16) | tri.id;

            x1 = p1.x << 8;
            x2 = p2.x << 8;
            x3 = p3.x << 8;
            y1 = p1.y;
            y2 = p2.y;
            y3 = p3.y;

            x4 = x1 + (x3 - x1) * (y2 - y1) / (y3 - y1);
            x1 <<= 8;
            x2 <<= 8;
            x3 <<= 8;
            x4 <<= 8;

            z1 = p1.z;
            z2 = p2.z;
            z3 = p3.z;
            nx1 = p1.nx << 16;
            nx2 = p2.nx << 16;
            nx3 = p3.nx << 16;
            ny1 = p1.ny << 16;
            ny2 = p2.ny << 16;
            ny3 = p3.ny << 16;

            sw1 = 1.0f / p1.sw;
            sw2 = 1.0f / p2.sw;
            sw3 = 1.0f / p3.sw;

            tx1 = p1.tx * sw1;
            tx2 = p2.tx * sw2;
            tx3 = p3.tx * sw3;
            ty1 = p1.ty * sw1;
            ty2 = p2.ty * sw2;
            ty3 = p3.ty * sw3;

            dx = (x4 - x2) >> 16;
            if (dx == 0)
            {
                return;
            }

            temp = 256 * (y2 - y1) / (y3 - y1);

            z4 = z1 + ((z3 - z1) >> 8) * temp;
            nx4 = nx1 + ((nx3 - nx1) >> 8) * temp;
            ny4 = ny1 + ((ny3 - ny1) >> 8) * temp;

            float tf = (float)(y2 - y1) / (float)(y3 - y1);

            tx4 = tx1 + ((tx3 - tx1) * tf);
            ty4 = ty1 + ((ty3 - ty1) * tf);

            sw4 = sw1 + ((sw3 - sw1) * tf);

            dz = (z4 - z2) / dx;
            dnx = (nx4 - nx2) / dx;
            dny = (ny4 - ny2) / dx;
            dtx = (tx4 - tx2) / dx;
            dty = (ty4 - ty2) / dx;
            dsw = (sw4 - sw2) / dx;

            if (dx < 0)
            {
                temp = x2;
                x2 = x4;
                x4 = temp;
                z2 = z4;
                tx2 = tx4;
                ty2 = ty4;
                sw2 = sw4;
                nx2 = nx4;
                ny2 = ny4;
            }
            if (y2 >= 0)
            {
                dy = y2 - y1;
                if (dy != 0)
                {
                    dxL = (x2 - x1) / dy;
                    dxR = (x4 - x1) / dy;
                    dzBase = (z2 - z1) / dy;
                    dnxBase = (nx2 - nx1) / dy;
                    dnyBase = (ny2 - ny1) / dy;
                    dtxBase = (tx2 - tx1) / dy;
                    dtyBase = (ty2 - ty1) / dy;
                    dswBase = (sw2 - sw1) / dy;
                }

                xBase = x1;
                xMax = x1;
                zBase = z1;
                nxBase = nx1;
                nyBase = ny1;
                txBase = tx1;
                tyBase = ty1;
                swBase = sw1;

                if (y1 < 0)
                {
                    xBase -= y1 * dxL;
                    xMax -= y1 * dxR;
                    zBase -= y1 * dzBase;
                    nxBase -= y1 * dnxBase;
                    nyBase -= y1 * dnyBase;
                    txBase -= y1 * dtxBase;
                    tyBase -= y1 * dtyBase;
                    swBase -= y1 * dswBase;
                    y1 = 0;
                }

                y2 = (y2 < height) ? y2 : height;
                offset = y1 * width;
                for (y = y1; y < y2; y++)
                {
                    renderLine();
                }
            }

            if (y2 < height)
            {
                dy = y3 - y2;
                if (dy != 0)
                {
                    dxL = (x3 - x2) / dy;
                    dxR = (x3 - x4) / dy;
                    dzBase = (z3 - z2) / dy;
                    dnxBase = (nx3 - nx2) / dy;
                    dnyBase = (ny3 - ny2) / dy;
                    dtxBase = (tx3 - tx2) / dy;
                    dtyBase = (ty3 - ty2) / dy;
                    dswBase = (sw3 - sw2) / dy;
                }

                xBase = x2;
                xMax = x4;
                zBase = z2;
                nxBase = nx2;
                nyBase = ny2;
                txBase = tx2;
                tyBase = ty2;
                swBase = sw2;

                if (y2 < 0)
                {
                    xBase -= y2 * dxL;
                    xMax -= y2 * dxR;
                    zBase -= y2 * dzBase;
                    nxBase -= y2 * dnxBase;
                    nyBase -= y2 * dnyBase;
                    txBase -= y2 * dtxBase;
                    tyBase -= y2 * dtyBase;
                    swBase -= y2 * dswBase;
                    y2 = 0;
                }

                y3 = (y3 < height) ? y3 : height;
                offset = y2 * width;

                for (y = y2; y < y3; y++)
                {
                    renderLine();
                }
            }
        }

        private void renderLine()
        {
            xL = xBase >> 16;
            xR = xMax >> 16;
            z = zBase;
            nx = nxBase;
            ny = nyBase;
            tx = txBase;
            ty = tyBase;
            sw = swBase;

            if (xL < 0)
            {
                z -= xL * dz;
                nx -= xL * dnx;
                ny -= xL * dny;
                tx -= xL * dtx;
                ty -= xL * dty;
                sw -= xL * dsw;
                xL = 0;
            }
            xR = (xR < width) ? xR : width;

            if (mode == F) renderLineF();
            else if ((mode & shaded) == P) renderLineP();
            else if ((mode & shaded) == E) renderLineE();
            else if ((mode & shaded) == T) renderLineT();
            else if ((mode & shaded) == (P | E)) renderLinePE();
            else if ((mode & shaded) == (P | T)) renderLinePT();
            else if ((mode & shaded) == (P | E | T)) renderLinePET();

            offset += width;
            xBase += dxL;
            xMax += dxR;
            zBase += dzBase;
            nxBase += dnxBase;
            nyBase += dnyBase;
            txBase += dtxBase;
            tyBase += dtyBase;
            swBase += dswBase;
        }

        // Fast scanline rendering
        private void renderLineF()
        {
            for (x = xL; x < xR; x++)
            {
                pos = x + offset;
                if (z < zBuffer[pos])
                {
                    bkgrd = screen.pixels[pos];
                    c = warp_Color.transparency(bkgrd, currentColor, transparency);
                    screen.pixels[pos] = c;
                    zBuffer[pos] = z;
                    if (useIdBuffer) idBuffer[pos] = currentId;
                }
                z += dz;
            }

        }

        private void renderLineP()
        {
            for (x = xL; x < xR; x++)
            {
                pos = x + offset;
                if (z < zBuffer[pos])
                {
                    lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);
                    bkgrd = screen.pixels[pos];
                    c = warp_Color.multiply(color, diffuse[lutID]);
                    s = specular[lutID];
                    s = warp_Color.scale(s, reflectivity);
                    c = warp_Color.transparency(bkgrd, c, transparency);
                    c = warp_Color.add(c, s);
                    screen.pixels[pos] = c;
                    zBuffer[pos] = z;

                    if (useIdBuffer) idBuffer[pos] = currentId;
                }
                z += dz;
                nx += dnx;
                ny += dny;
            }

        }

        private void renderLineE()
        {
            for (x = xL; x < xR; x++)
            {
                pos = x + offset;
                if (z < zBuffer[pos])
                {
                    lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);
                    bkgrd = screen.pixels[pos];
                    s = warp_Color.add(specular[lutID], envmap[lutID]);
                    s = warp_Color.scale(s, reflectivity);
                    c = warp_Color.transparency(bkgrd, s, transparency);
                    screen.pixels[pos] = c;
                    zBuffer[pos] = z;

                    if (useIdBuffer) idBuffer[pos] = currentId;
                }
                z += dz;
                nx += dnx;
                ny += dny;
            }
        }

        private void renderLineT()
        {
            for (x = xL; x < xR; x++)
            {
                pos = x + offset;
                if (z < zBuffer[pos])
                {
                    bkgrd = screen.pixels[pos];
                    c = texture.pixel[(((int)(tx / sw)) & tw) + ((((int)(ty / sw)) & th) << tbitW)];
                    c = warp_Color.transparency(bkgrd, c, transparency);
                    screen.pixels[pos] = c;
                    zBuffer[pos] = z;

                    if (useIdBuffer) idBuffer[pos] = currentId;
                }
                z += dz;
                tx += dtx;
                ty += dty;
                sw += dsw;
            }
        }

        private void renderLinePE()
        {
            for (x = xL; x < xR; x++)
            {
                pos = x + offset;
                if (z < zBuffer[pos])
                {
                    lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);

                    bkgrd = screen.pixels[pos];
                    c = warp_Color.multiply(color, diffuse[lutID]);
                    s = warp_Color.add(specular[lutID], envmap[lutID]);
                    s = warp_Color.scale(s, reflectivity);
                    c = warp_Color.transparency(bkgrd, c, transparency);
                    c = warp_Color.add(c, s);
                    screen.pixels[pos] = c;
                    zBuffer[pos] = z;

                    if (useIdBuffer) idBuffer[pos] = currentId;
                }
                z += dz;
                nx += dnx;
                ny += dny;
            }
        }

        private void renderLinePT()
        {
            for (x = xL; x < xR; x++)
            {
                pos = x + offset;
                if (z < zBuffer[pos])
                {
                    lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);

                    bkgrd = screen.pixels[pos];
                    c = texture.pixel[(((int)(tx / sw)) & tw) + ((((int)(ty / sw)) & th) << tbitW)];
                    c = warp_Color.multiply(c, diffuse[lutID]);
                    s = warp_Color.scale(specular[lutID], reflectivity);
                    c = warp_Color.transparency(bkgrd, c, transparency);
                    c = warp_Color.add(c, s);
                    screen.pixels[pos] = c;
                    zBuffer[pos] = z;

                    if (useIdBuffer) idBuffer[pos] = currentId;
                }

                z += dz;
                nx += dnx;
                ny += dny;
                tx += dtx;
                ty += dty;
                sw += dsw;
            }
        }

        private void renderLinePET()
        {
            for (x = xL; x < xR; x++)
            {
                pos = x + offset;
                if (z < zBuffer[pos])
                {
                    lutID = ((nx >> 16) & 255) + (((ny >> 16) & 255) << 8);
                    bkgrd = screen.pixels[pos];
                    c = texture.pixel[(((int)(tx / sw)) & tw) + ((((int)(ty / sw)) & th) << tbitW)];
                    c = warp_Color.multiply(c, diffuse[lutID]);
                    s = warp_Color.add(specular[lutID], envmap[lutID]);
                    s = warp_Color.scale(s, reflectivity);
                    c = warp_Color.transparency(bkgrd, c, transparency);
                    c = warp_Color.add(c, s);

                    screen.pixels[pos] = c;
                    zBuffer[pos] = z;
                    if (useIdBuffer) idBuffer[pos] = currentId;
                }
                z += dz;
                nx += dnx;
                ny += dny;
                tx += dtx;
                ty += dty;
                sw += dsw;
            }
        }

        private void drawWireframe(warp_Triangle tri, int defaultcolor)
        {
            drawLine(tri.p1, tri.p2, defaultcolor);
            drawLine(tri.p2, tri.p3, defaultcolor);
            drawLine(tri.p3, tri.p1, defaultcolor);
        }

        private void drawLine(warp_Vertex a, warp_Vertex b, int color)
        {
            warp_Vertex temp;
            if ((a.clipcode & b.clipcode) != 0) return;

            dx = (int)Math.Abs(a.x - b.x);
            dy = (int)Math.Abs(a.y - b.y);
            dz = 0;

            if (dx > dy)
            {
                if (a.x > b.x) { temp = a; a = b; b = temp; }
                if (dx > 0)
                {
                    dz = (b.z - a.z) / dx;
                    dy = ((b.y - a.y) << 16) / dx;
                }
                z = a.z;
                y = a.y << 16;
                for (x = a.x; x <= b.x; x++)
                {
                    y2 = y >> 16;
                    if (warp_Math.inrange(x, 0, width - 1) && warp_Math.inrange(y2, 0, height - 1))
                    {
                        offset = y2 * width;
                        if (z < zBuffer[x + offset])
                        {
                            {
                                screen.pixels[x + offset] = color;
                                zBuffer[x + offset] = z;
                            }
                        }
                        if (useIdBuffer) idBuffer[x + offset] = currentId;
                    }
                    z += dz; y += dy;
                }
            }
            else
            {
                if (a.y > b.y) { temp = a; a = b; b = temp; }
                if (dy > 0)
                {
                    dz = (b.z - a.z) / dy;
                    dx = ((b.x - a.x) << 16) / dy;
                }
                z = a.z;
                x = a.x << 16;
                for (y = a.y; y <= b.y; y++)
                {
                    x2 = x >> 16;
                    if (warp_Math.inrange(x2, 0, width - 1) && warp_Math.inrange(y, 0, height - 1))
                    {
                        offset = y * width;
                        if (z < zBuffer[x2 + offset])
                        {
                            {
                                screen.pixels[x2 + offset] = color;
                                zBuffer[x2 + offset] = z;
                            }
                        }
                        if (useIdBuffer) idBuffer[x2 + offset] = currentId;
                    }
                    z += dz; x += dx;
                }
            }
        }
    }
}