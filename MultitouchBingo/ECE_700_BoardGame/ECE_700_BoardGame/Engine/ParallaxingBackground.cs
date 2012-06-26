using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class ParallaxingBackground
    {
        // The image representing the parallaxing background
        Texture2D texture;

        // An array of positions of the parallaxing background
        Vector2[] positions;
        Rectangle[] rectPositions;
        int spacing;

        // The speed which the background is moving
        int speed;
        float floatSpeed;
        float[] totalMovement;

        Boolean intSpeed;

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(ContentManager content, String texturePath, int screenWidth, int speed)
        {
            // Load the background texture we will be using
            texture = content.Load<Texture2D>(texturePath);

            // Set the speed of the background
            this.speed = speed;

            // If we divide the screen with the texture width then we can determine the number of tiles needed.
            // We add 1 to it so that we won't have a gap in the tiling
            positions = new Vector2[screenWidth / texture.Width + 1];

            // Set the initial positions of the parallaxing background
            for (int i = 0; i < positions.Length; i++)
            {
                // We need the tiles to be side by side to create a tiling effect
                positions[i] = new Vector2(i * texture.Width, 0);
            }
            intSpeed = true;
            //base.Initialize();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.
        /// Allows spacing in x other than directly side-by-side.
        /// Note float speed when greater than 1 or less than -1 is rounded to nearest int
        /// </summary>
        public void Initialize(ContentManager content, String texturePath, int screenWidth, int spacing, Rectangle texRect, float speed)
        {
            // Load the background texture we will be using
            texture = content.Load<Texture2D>(texturePath);

            // Set the speed of the background
            this.floatSpeed = speed;
            //this.rectPos = texRect;
            this.spacing = spacing;

            // If we divide the screen with the texture width then we can determine the number of tiles needed.
            // We add 1 to it so that we won't have a gap in the tiling
            int len = screenWidth / spacing;
            if (len < 1)
            {
                len++;
            }
            rectPositions = new Rectangle[len + 1];
            totalMovement = new float[len + 1];
            // Set the initial positions of the parallaxing background
            for (int i = 0; i < rectPositions.Length; i++)
            {
                // We need the tiles to be side by side to create a tiling effect
                rectPositions[i] = new Rectangle(i * spacing, texRect.Y, texRect.Width, texRect.Height);
                totalMovement[i] = 0;
            }
            intSpeed = false;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update()
        {
            if (intSpeed)
            {
                // Update the positions of the background
                for (int i = 0; i < positions.Length; i++)
                {
                    // Update the positions of the screen by adding the speed
                    positions[i].X += speed;

                    // If the speed has the background moving to the left
                    if (speed <= 0)
                    {
                        // Check the texture is out of view then put that texture at the end of the screen
                        if (positions[i].X <= -texture.Width)
                        {
                            positions[i].X = texture.Width * (positions.Length - 1);
                        }
                    }

                    // If the speed has the background moving to the right
                    else
                    {
                        // Check if the texture is out of view then position it to the start of the screen
                        if (positions[i].X >= texture.Width * (positions.Length - 1))
                        {
                            positions[i].X = -texture.Width;
                        }
                    }
                }
            }
            else
            {
                // Update the positions of the background
                for (int i = 0; i < rectPositions.Length; i++)
                {
                    totalMovement[i] += floatSpeed;
                    // Update the positions of the screen by adding the speed
                    if ((totalMovement[i] >= 1) || (totalMovement[i] <= -1))
                    {
                        rectPositions[i].X += (int)Math.Round(totalMovement[i], 0);
                        totalMovement[i] = 0;
                    }

                    // If the speed has the background moving to the left
                    if (floatSpeed <= 0)
                    {
                        // Check the texture is out of view then put that texture at the end of the screen
                        if (rectPositions[i].X <= -spacing)
                        {
                            rectPositions[i].X = spacing * (rectPositions.Length - 1);
                        }
                    }

                    // If the speed has the background moving to the right
                    else
                    {
                        // Check if the texture is out of view then position it to the start of the screen
                        if (rectPositions[i].X >= spacing * (rectPositions.Length - 1))
                        {
                            rectPositions[i].X = -spacing;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draw background
        /// </summary>
        /// 
        public void Draw(SpriteBatch spriteBatch)
        {
            if (intSpeed)
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    spriteBatch.Draw(texture, positions[i], Color.White);
                }
            }
            else
            {
                for (int i = 0; i < rectPositions.Length; i++)
                {
                    spriteBatch.Draw(texture, rectPositions[i], Color.White);
                }
            }
        }
    }
}
