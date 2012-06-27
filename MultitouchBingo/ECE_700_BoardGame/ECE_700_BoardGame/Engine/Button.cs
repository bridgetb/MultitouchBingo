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
        public Texture2D texture;
        public Rectangle position;
        public Vector2 originOffset;
        
        public Button(Game game, Texture2D tex, Rectangle pos) : base(game)
        {
            this.texture = tex;
            this.position = pos;
        }
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            // TODO: Add your initialization code here

        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // 
        }

        /// <summary>
        /// Rendering of the button
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, position, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, float orient)
        {
            spriteBatch.Draw(texture, position, null, Color.White, orient, originOffset, SpriteEffects.None, 0f);
        }

        protected bool IsPressed(TouchPoint point)
        {
#if DEBUG
            Debug.WriteLine(point.X.ToString(), "Touch point X");
            Debug.WriteLine(point.Y.ToString(), "Touch point Y");
            Debug.WriteLine(position.Contains((int)point.X, (int)point.Y).ToString(), "Is Within Item Hit Detection");
#endif
            if( position.Contains((int)point.X, (int)point.Y) ){
                return true;
            }
            return false;
        }

        protected bool IsPressed(MouseState clickPoint)
        {
#if DEBUG
            Debug.WriteLine(position.Contains((int)clickPoint.X, (int)clickPoint.Y).ToString(), "Is Within Item Hit Detection (CLICK)");
#endif
            if (position.Contains((int)clickPoint.X, (int)clickPoint.Y))
            {
                return true;
            }
            return false;
        }
    }
}
