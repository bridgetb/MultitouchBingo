﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework.Input;

namespace ECE_700_BoardGame.Engine
{
    public abstract class MenuButton : Button
    {
        public MenuButton(Game game, Texture2D tex, Rectangle pos, Rectangle target) 
            : base(game, tex, pos, target) { }
        public MenuButton(Game game, Texture2D tex, Rectangle pos, Rectangle target, int frames) 
            : base(game, tex, pos, target, frames) { }

        public virtual bool OnTouchTapGesture(TouchPoint touch)
        {
            if (IsPressed(touch))
            {
                return true;
            }
            return false;
        }

        public virtual bool OnClickGesture(MouseState mouseState)
        {
            if (IsPressed(mouseState))
            {
                return true;
            }
            return false;
        }
    }
}