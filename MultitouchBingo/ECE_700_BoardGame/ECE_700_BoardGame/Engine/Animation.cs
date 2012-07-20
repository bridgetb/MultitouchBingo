using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ECE_700_BoardGame.Engine
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Animation //: Microsoft.Xna.Framework.GameComponent
    {
        // The image representing the collection of images used for animation
        Texture2D spriteStrip;

        // The scale used to display the sprite strip
        float scale;

        // Offset for rotation
        Vector2 offset;

        // The time since we last updated the frame
        int elapsedTime;

        // The time we display a frame until the next one
        int frameTime;

        // The number of frames that the animation contains
        int frameCount;

        // The index of the current frame we are displaying
        int currentFrame;

        // The color of the frame we will be displaying
        Color color;

        // The area of the image strip we want to display
        Rectangle sourceRect = new Rectangle();

        // The area where we want to display the image strip in the game
        Rectangle destinationRect = new Rectangle();

        // Width of a given frame
        public int FrameWidth;

        // Height of a given frame
        public int FrameHeight;

        // The state of the Animation
        public bool Active;

        // Determines if the animation will keep playing or deactivate after one run
        public bool Looping;

        // Set if last frame persists
        bool hold;
        bool holdReady;

        // Width of a given frame
        public Vector2 Position;

        public Animation()//Game game)
        //: base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize(Texture2D texture, Vector2 position,
                                int frameWidth, int frameHeight, int frameCount,
                                int frameTime, Color color, float scale, bool looping, bool hold)
        {
            // Keep a local copy of the values passed in
            this.color = color;
            this.FrameWidth = frameWidth;
            this.FrameHeight = frameHeight;
            this.frameCount = frameCount;
            this.frameTime = frameTime;
            this.scale = scale;
            this.hold = hold;

            Looping = looping;
            Position = position;
            spriteStrip = texture;

            // Set the time to zero
            elapsedTime = 0;
            currentFrame = 0;

            this.offset = new Vector2(frameWidth, frameHeight);

            // Set the Animation to active by default
            Active = true;
            this.holdReady = false;
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // Do not update the game if we are not active
            if (Active == false)
                return;

            if (holdReady)
                return;

            // Update the elapsed time
            elapsedTime += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            // If the elapsed time is larger than the frame time
            // we need to switch frames
            if (elapsedTime > frameTime)
            {
                // Move to the next frame
                currentFrame++;

                // If the currentFrame is equal to the frameCount reset currentFrame to zero
                if (currentFrame == frameCount)
                {
                    if (hold)
                    {
                        holdReady = true;
                        return;
                    }

                    currentFrame = 0;
                    // If we are not looping deactivate the animation
                    if (Looping == false)
                        Active = false;
                }

                // Reset the elapsed time to zero
                elapsedTime = 0;
            }

            // Grab the correct frame in the image strip by multiplying the currentFrame index by the frame width
            sourceRect = new Rectangle(currentFrame * FrameWidth, 0, FrameWidth, FrameHeight);

            // Set destination
            destinationRect = new Rectangle((int)Position.X,
                                            (int)Position.Y,
                                            (int)(FrameWidth * scale),
                                            (int)(FrameHeight * scale));

            //destinationRect = new Rectangle((int)Position.X - (int)(FrameWidth * scale) / 2,
            //                                (int)Position.Y - (int)(FrameHeight * scale) / 2,
            //                                (int)(FrameWidth * scale),
            //                                (int)(FrameHeight * scale));
        }


        public void Draw(SpriteBatch spriteBatch, bool rotated)
        {
            // Only draw the animation when we are active
            if (Active)
            {
                if (rotated)
                {
                    spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color, (float)Math.PI, offset, SpriteEffects.None, 0f);
                    //spriteBatch.Draw(AnsweredSprite, position, null, Color.White, TileOrient, ansSpriteOffset, SpriteEffects.None, 0f);
                }
                else
                {
                    spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, color);
                }
            }
        }
    }
}
