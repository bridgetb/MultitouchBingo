using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Surface.Core;
using ECE_700_BoardGame.Screens;
using Microsoft.Xna.Framework.Input;

namespace ECE_700_BoardGame.Engine
{
    class PlayButton : Button
    {
        public PlayButton(Game game, Texture2D tex, Rectangle pos, Rectangle target)
            : base(game, tex, pos, target)
        {
            Alpha = 255;
        }

        public override bool OnTouchTapGesture(TouchPoint touch)
        {
            TagData td = touch.Tag;
            //Recognized tag id values
            if (IsPressed(touch) && (td.Value == 0xC0 || td.Value == 8 || td.Value == 9 || td.Value == 0x0B || td.Value == 0x0A))
            {
                if (Game is BingoApp)
                {
                    Screen s = ((BingoApp)Game).GetGameState();
                    if (s is MenuScreen)
                    {
                        ((MenuScreen)s).FinishedSettingOptions();
                    }
                }
                return true;
            }
            return false;
        }

        public override bool OnClickGesture(MouseState mouseState)
        {
            if (IsPressed(mouseState))
            {
                if (Game is BingoApp)
                {
                    Screen s = ((BingoApp)Game).GetGameState();
                    if (s is MenuScreen)
                    {
                        ((MenuScreen)s).FinishedSettingOptions();
                    }
                }
                return true;
            }
            return false;
        }

    }
}
