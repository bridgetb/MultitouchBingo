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
    class SettingButton : Button
    {
        String Setting;
        String Value;
        Game Game;
        bool Selected = false;

        public SettingButton(Game game, Texture2D tex, Rectangle pos, String setting, String value)
            : base(game, tex, pos)
        {
            Game = game;
            Setting = setting;
            Value = value;
        }

        public bool OnTouchTapGesture(TouchPoint touch)
        {
            if (IsPressed(touch))
            {
                Toggle();
                return true;
            }
            return false;
        }

        public bool OnClickGesture(MouseState mouseState)
        {
            if (IsPressed(mouseState))
            {
                Toggle();
                return true;
            }
            return false;
        }

        protected void Toggle()
        {
            if (Game is BingoApp)
            {
                if (Selected)
                {
                    ((BingoApp)Game).RemoveSetting(Setting, Value);
                }
                else
                {
                    ((BingoApp)Game).AddSetting(Setting, Value);
                }

                Selected = !Selected;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            // Show indication of whether selected
            if (Selected)
            {
                Texture2D tex = Game.Content.Load<Texture2D>("daub");
                Rectangle pos = new Rectangle(position.Left-tex.Width/2, position.Bottom-tex.Height/2, tex.Width, tex.Height);
                spriteBatch.Draw(tex, pos, Color.White);
            }
        }

    }

    class ContinueButton : Button
    {
        public ContinueButton(Game game, Texture2D tex, Rectangle pos)
            : base(game, tex, pos)
        {
            
        }

        public bool OnTouchTapGesture(TouchPoint touch)
        {
            if (IsPressed(touch))
            {
                if (Game is BingoApp)
                    ((BingoApp)Game).FinishedSettingOptions();
                return true;
            }
            return false;
        }

        public bool OnClickGesture(MouseState mouseState)
        {
            if (IsPressed(mouseState))
            {
                if (Game is BingoApp)
                    ((BingoApp)Game).FinishedSettingOptions();
                return true;
            }
            return false;
        }

    }
}
