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
    /// <summary>
    /// Represents the setting buttons for topics and difficulty on the menu screen.
    /// </summary>
    class SettingButton : Button
    {
        public String Setting { get; set; }
        public String Value { get; set; }
        public bool Selected { get; set; }
        int FadeInc = 20;
        double FadeInDelay = .035;
        private Texture2D texAlt;

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

        public SettingButton(Game game, Texture2D tex, Texture2D texAlt, Rectangle pos, Rectangle target, int frames, String setting, String value)
            : base(game, tex, pos, target, frames)
        {
            Setting = setting;
            Value = value;
            Selected = false;
            Alpha = 1;
            this.texAlt = texAlt;
        }

        /// <summary>
        /// Switches button state when tapped.
        /// </summary>
        /// <param name="touch"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Switches state and records settings selected.
        /// </summary>
        public void Toggle()
        {
            if (Game is BingoApp)
            {
                // If an alternate image exists, swap alternate image with currently displaying image
                if (this.texAlt != null)
                {
                    Texture2D old = this.Texture;
                    this.Texture = this.texAlt;
                    this.texAlt = old;
                }
                Selected = !Selected;
                Screen s = ((BingoApp)Game).GetGameState();
                if (s is MenuScreen)
                {
                    if (!Selected)
                    {
                        ((MenuScreen)s).RemoveSetting(Setting, Value);
                    }
                    else
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
                /* // Check box
                Texture2D tex = Game.Content.Load<Texture2D>("daub");
                Rectangle pos = new Rectangle(Position.Left - Position.Height / 2, Position.Bottom - Position.Height, Position.Height, Position.Height);
                spriteBatch.Draw(tex, pos, Color.White);*/
                if (texAlt == null)
                {
                    Texture2D tex = Game.Content.Load<Texture2D>("BingoEnvironment/Highlight");
                    spriteBatch.Draw(tex, this.Position, Color.White);
                }
            }
        }

        /// <summary>
        /// Fades the button in on initialisation.
        /// </summary>
        /// <param name="gametime"></param>
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
}
