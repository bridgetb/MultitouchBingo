using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework.Input;

namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// Button to be instantiated for the menu screen.
    /// </summary>
    public abstract class MenuButton : Button
    {
        public MenuButton(Game game, Texture2D tex, Rectangle pos, Rectangle target) 
            : base(game, tex, pos, target) { }
        public MenuButton(Game game, Texture2D tex, Rectangle pos, Rectangle target, int frames) 
            : base(game, tex, pos, target, frames) { }
    }
}
