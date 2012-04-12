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

using ECE_700_BoardGame.Helper;

namespace ECE_700_BoardGame.Layout
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class BackgroundItem
    {
        #region Fields

        private Rectangle Position;
        private Vector2 Origin;

        private Rectangle SourcePosition;

        private float Orient;
        private EnumSettings.ItemOrientation Flip;
        private Texture2D SpriteTexture;

        #endregion

        /// <summary>
        /// Initialize Texture from an image that has only a single Sprite
        /// </summary>
        /// <param name="imagePath">String Path to Texture</param>
        /// <param name="x">On screen TopLeft X Coordinate</param>
        /// <param name="y">On Screen TopLeft Y Coordinate</param>
        /// <param name="itemOrientation">Currently only Vertically Oriented</param>
        /// <param name="content">ContentManager that needs the texture to be loaded</param>
        public BackgroundItem(Texture2D spriteTex, Rectangle position, float itemOrientation)
        {
            this.Position = position;
            this.Orient = itemOrientation;
            SpriteTexture = spriteTex;
            Origin = new Vector2(0, 0);
            this.Flip = EnumSettings.ItemOrientation.DOWN;
        }


        /// <summary>
        /// Initialize Texture from an image that has a set of Sprites in one image file
        /// Currently not implemented in Draw method but if needed will be very quick to add
        /// </summary>
        /// <param name="imagePath">String Path to Texture</param>
        /// <param name="x">On screen TopLeft X Coordinate</param>
        /// <param name="y">On Screen TopLeft Y Coordinate</param>
        /// <param name="itemOrientation">Currently only Vertically Oriented</param>
        /// <param name="content">ContentManager that needs the texture to be loaded</param>
        /// <param name="spriteBounds">Position of the sprite in a sprite sheet</param>
        public BackgroundItem(Texture2D spriteTex, Rectangle position, float itemOrientation, Vector2 origin)
        {
            this.Position = position;
            //this.Position = new Rectangle(0, 0, position.Width, position.Height);
            this.Orient = itemOrientation;
            SpriteTexture = spriteTex;
            this.Origin = origin;
            this.Flip = EnumSettings.ItemOrientation.DOWN;
        }


        /// <summary>
        /// Initialize Texture from an image that has a set of Sprites in one image file
        /// Currently not implemented in Draw method but if needed will be very quick to add
        /// </summary>
        /// <param name="imagePath">String Path to Texture</param>
        /// <param name="x">On screen TopLeft X Coordinate</param>
        /// <param name="y">On Screen TopLeft Y Coordinate</param>
        /// <param name="itemOrientation">Currently only Vertically Oriented</param>
        /// <param name="content">ContentManager that needs the texture to be loaded</param>
        /// <param name="spriteBounds">Position of the sprite in a sprite sheet</param>
        public BackgroundItem(Texture2D spriteTex, Rectangle position, float itemOrientation, Vector2 origin, EnumSettings.ItemOrientation flip)
        {
            this.Position = position;
            this.Orient = itemOrientation;
            SpriteTexture = spriteTex;
            Origin = new Vector2(0, 0);
            this.Flip = flip;
        }

        /// <summary>
        /// Initialize Texture from an image that has a set of Sprites in one image file
        /// Currently not implemented in Draw method but if needed will be very quick to add
        /// </summary>
        /// <param name="imagePath">String Path to Texture</param>
        /// <param name="x">On screen TopLeft X Coordinate</param>
        /// <param name="y">On Screen TopLeft Y Coordinate</param>
        /// <param name="itemOrientation">Currently only Vertically Oriented</param>
        /// <param name="content">ContentManager that needs the texture to be loaded</param>
        /// <param name="spriteBounds">Position of the sprite in a sprite sheet</param>
        public BackgroundItem(Texture2D spriteTex, Rectangle position, float itemOrientation, Vector2 origin, Rectangle spriteBounds)
        {
            this.Position = position;
            this.Orient = itemOrientation;
            SpriteTexture = spriteTex;
            Origin = new Vector2(0, 0);
            SourcePosition = spriteBounds;
            this.Flip = EnumSettings.ItemOrientation.DOWN;
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
            // TODO: Add your update code here
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            switch(Flip){
                case (EnumSettings.ItemOrientation.UP):
                    spriteBatch.Draw(SpriteTexture, Position, null, Color.White, Orient, Origin, SpriteEffects.FlipVertically, 0f);
                    break;
                case (EnumSettings.ItemOrientation.RIGHT):

                    spriteBatch.Draw(SpriteTexture, Position, null, Color.White, Orient, Origin, SpriteEffects.FlipHorizontally, 0f);
                    break;
                default:
                    spriteBatch.Draw(SpriteTexture, Position, null, Color.White, Orient, Origin, SpriteEffects.None, 0f);
                    break;
            }
        }
    }
}
