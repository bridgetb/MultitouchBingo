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
    class SettingButton : MenuButton
    {
        public String Setting { get; set; }
        public String Value { get; set; }
        public bool Selected { get; set; }
        int FadeInc = 20;
        double FadeInDelay = .035;

        public SettingButton(Game game, Texture2D tex, Rectangle pos, Rectangle target, String setting, String value)
            : base(game, tex, pos, target)
        {
            Setting = setting;
            Value = value;
            Selected = false;
            Alpha = 1;
        }

        public SettingButton(Game game, Texture2D tex, Rectangle pos, Rectangle target, int frames, String setting, String value)
            : base(game, tex, pos, target, frames)
        {
            Setting = setting;
            Value = value;
            Selected = false;
            Alpha = 1;
        }

        public override bool OnTouchTapGesture(TouchPoint touch)
        {
            if (IsPressed(touch))
            {
                Toggle();
                return true;
            }
            return false;
        }

        public override bool OnClickGesture(MouseState mouseState)
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
                Rectangle pos = new Rectangle(Position.Left - Position.Height / 2, Position.Bottom - Position.Height, Position.Height, Position.Height);
                spriteBatch.Draw(tex, pos, Color.White);
            }
        }

        public override void Update(GameTime gametime)
        {
            FadeInDelay -= gametime.ElapsedGameTime.TotalSeconds;
            if (FadeInDelay <= 0)
            {
                FadeInDelay = .035;
                Alpha += FadeInc;
                if (Alpha >= 255)
                {
                    FadeInc = 0;
                }
            }
            base.Update(gametime);
        }
    }

    class PlayButton : MenuButton
    {
        public PlayButton(Game game, Texture2D tex, Rectangle pos, Rectangle target)
            : base(game, tex, pos, target)
        {
            Alpha = 255;
        }

        public override bool OnTouchTapGesture(TouchPoint touch)
        {
            if (IsPressed(touch))
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
