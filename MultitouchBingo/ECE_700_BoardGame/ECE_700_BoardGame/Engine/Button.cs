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


namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public abstract class Button : DrawableGameComponent
    {
        public Texture2D texture;
        public Rectangle position;
        
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

        protected bool IsPressed(TouchPoint point)
        {
            if (point.CenterX >= this.position.X && point.CenterX <= this.position.X + this.texture.Width)
            {
                if (point.CenterY >= this.position.Y && point.CenterY <= this.position.Y + this.texture.Height)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
