using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using System.Diagnostics;


namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public abstract class Button : DrawableGameComponent
    {
        public Texture2D Texture;
        public Rectangle Position;
        public Rectangle Target;
        public Vector2 OriginOffset;
        int XChange = 0;
        int YChange = 0;
        public bool IsTranslating { get; set; }

        protected int Alpha = 255;
        
        public Button(Game game, Texture2D tex, Rectangle pos, Rectangle target) : base(game)
        {
            this.Texture = tex;
            this.Position = pos;
            this.Target = target;
        }
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            if (Target.Equals(Position))
            {
                return;
            }
            int frames = Math.Abs(Target.X - Position.X);
            if (frames == 0 || (frames > Math.Abs(Target.Y - Position.Y) && Math.Abs(Target.Y - Position.Y) != 0))
            {
                frames = Math.Abs(Target.Y - Position.Y) / 100;
            }

            XChange = (Target.X - Position.X) / frames * 15;
            YChange = (Target.Y - Position.Y) / frames * 15;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gametime)
        {
            base.Update(gametime);
        }

        /// <summary>
        /// Rendering of the button
        /// </summary>
        /// <param name="spriteBatch"></param>
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (this.IsTranslating)
            {
                if (Math.Abs((Position.X + XChange) - Target.X) < Math.Abs(Position.X - Target.X))
                {
                    Position.X += XChange;
                }
                else
                {
                    // Gone past target
                    Position.X = Target.X;
                    XChange = 0;
                }
                if (Math.Abs((Position.Y + XChange) - Target.Y) < Math.Abs(Position.Y - Target.Y))
                {
                    Position.Y += YChange;
                }
                else
                {
                    // Gone past target
                    Position.Y = Target.Y;
                    YChange = 0;
                }
            }
            spriteBatch.Draw(Texture, Position, new Color(255, 255, 255, (byte)MathHelper.Clamp(Alpha, 0, 255)));
        }

        public virtual void Draw(SpriteBatch spriteBatch, float orient)
        {
            if (this.IsTranslating)
            {
                if (Math.Abs((Position.X + XChange) - Target.X) < Math.Abs(Position.X - Target.X))
                {
                    Position.X += XChange;
                }
                else
                {
                    // Gone past target
                    Position.X = Target.X;
                    XChange = 0;
                }
                if (Math.Abs((Position.Y + XChange) - Target.Y) < Math.Abs(Position.Y - Target.Y))
                {
                    Position.Y += YChange;
                }
                else
                {
                    // Gone past target
                    Position.Y = Target.Y;
                    YChange = 0;
                }
            }
            spriteBatch.Draw(Texture, Position, null, new Color(255, 255, 255, (byte)MathHelper.Clamp(Alpha, 0, 255)), orient, 
                OriginOffset, SpriteEffects.None, 0f);
        }

        protected virtual bool IsPressed(TouchPoint point)
        {
#if DEBUG
            Debug.WriteLine(point.X.ToString(), "Touch point X");
            Debug.WriteLine(point.Y.ToString(), "Touch point Y");
            Debug.WriteLine(Position.Contains((int)point.X, (int)point.Y).ToString(), "Is Within Item Hit Detection");
#endif
            if (Position.Contains((int)point.X, (int)point.Y))
            {
                return true;
            }
            return false;
        }

        protected virtual bool IsPressed(MouseState clickPoint)
        {
            //Rectangle largerArea = new Rectangle(position.X, position.Y, position.Width + 200, position.Height + 200);            
#if DEBUG
            Debug.WriteLine(Position.Contains((int)clickPoint.X, (int)clickPoint.Y).ToString(), "Is Within Item Hit Detection (CLICK)");
#endif
            if (Position.Contains((int)clickPoint.X, (int)clickPoint.Y))
            {
                return true;
            }
            return false;
        }
    }
}
