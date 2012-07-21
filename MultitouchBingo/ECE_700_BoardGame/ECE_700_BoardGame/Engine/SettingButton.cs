using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework.Input;
using ECE_700_BoardGame.Screens;

namespace ECE_700_BoardGame.Engine
{
    class SettingButton : Button
    {
        public String Setting { get; set; }
        public String Value { get; set; }
        Game Game;
        public bool Selected {get; set;}

        public SettingButton(Game game, Texture2D tex, Rectangle pos, String setting, String value)
            : base(game, tex, pos)
        {
            Game = game;
            Setting = setting;
            Value = value;
            Selected = false;
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

        public void Toggle()
        {
            if (Game is BingoApp)
            {
                if (Selected)
                {
                    Selected = false;
                    Screen s = ((BingoApp)Game).GetGameState();
                    if (s is MenuScreen)
                    {
                        ((MenuScreen)s).RemoveSetting(Setting, Value);
                    }
                }
                else
                {
                    Selected = true;
                    Screen s = ((BingoApp)Game).GetGameState();
                    if (s is MenuScreen)
                    {
                        ((MenuScreen)s).AddSetting(Setting, Value);
                    }
                }

            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            // Show indication of whether selected
            if (Selected)
            {
                Texture2D tex = Game.Content.Load<Texture2D>("daub");
                Rectangle pos = new Rectangle(position.Left - position.Height / 2, position.Bottom - position.Height, position.Height, position.Height);
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

        public bool OnTouchTapGesture(TouchPoint touch, GameTime gametime)
        {
            if (IsPressed(touch))
            {
                if (Game is BingoApp)
                {
                    Screen s = ((BingoApp)Game).GetGameState();
                    if (s is MenuScreen)
                    {
                        ((MenuScreen)s).FinishedSettingOptions(gametime);
                    }
                }
                return true;
            }
            return false;
        }

        public bool OnClickGesture(MouseState mouseState, GameTime gametime)
        {
            if (IsPressed(mouseState))
            {
                if (Game is BingoApp)
                {
                    Screen s = ((BingoApp)Game).GetGameState();
                    if (s is MenuScreen)
                    {
                        ((MenuScreen)s).FinishedSettingOptions(gametime);
                    }
                }
                return true;
            }
            return false;
        }

    }
}
