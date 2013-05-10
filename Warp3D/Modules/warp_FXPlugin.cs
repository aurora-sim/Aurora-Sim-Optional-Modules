/*
 * Copyright (c) Peter Walser and Dean Lunz
 * All rights reserved.
 *
 * Licensed under the Creative Commons Attribution Share-Alike 2.5 Canada
 * license: http://creativecommons.org/licenses/by-sa/2.5/ca/
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Rednettle.Warp3D
{
    public abstract class warp_FXPlugin
    {
        public warp_Scene scene = null;
        public warp_Screen screen = null;

        public warp_FXPlugin(warp_Scene scene)
        {
            this.scene = scene;
            screen = scene.renderPipeline.screen;
        }

        public abstract void apply();
    }
}