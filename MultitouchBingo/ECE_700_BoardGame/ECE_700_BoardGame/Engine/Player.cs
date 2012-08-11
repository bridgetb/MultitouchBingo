using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace ECE_700_BoardGame.Engine
{
    class Player
    {
        #region Field Members

        List<BingoTile> PlayerTiles;
        
        bool[] AnsweredTiles;
        bool HasWon;
        bool previousAnsCorrect;

        int Score;
        int PlayerID;
        SpriteFont WinnerMessage;
        Texture2D Highlight;
        GameDifficulty DifficultyLevel {get; set;}

        #endregion

        public Player(List<BingoTile> playerTiles, int playerID, Game game)
        {
           
            this.PlayerTiles = playerTiles;
            AnsweredTiles = new bool[playerTiles.Count];
            this.PlayerID = playerID;
            WinnerMessage = game.Content.Load<SpriteFont>("Comic");
            Highlight = game.Content.Load<Texture2D>("BingoEnvironment/Highlight");
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public void Initialize()
        {
            Score = 0;
            HasWon = false;
            previousAnsCorrect = false;

            for (int i = 0; i < AnsweredTiles.Length; i++)
            {
                AnsweredTiles[i] = false;
            }
        }

        public void tileAnswered(bool ansCorrect, int tileNum)
        {
            if(ansCorrect){
                AnsweredTiles[tileNum] = true;

                if(previousAnsCorrect){
                    Score += 20;
                }
                else{
                    Score += 10;
                }
                previousAnsCorrect = true;

                //TODO check if won
                if (Bingo())
                {
                    HasWon = true;

#if DEBUG
                    Debug.WriteLine("Player " + PlayerID.ToString() + " wins!", "BINGO");
#endif
                }
            }
            else{

            }
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update()
        {

        }

        //TODO: Check correctly for all winning conditions for the game
        bool Bingo()
        {
            bool victory1 = true, victory2 = true, victory3 = true, victory4 = true;
            int boardWidthHeight = (int)Math.Sqrt(AnsweredTiles.Length);

            //Check for horizontal victory
            for (int r = 0; r < boardWidthHeight; r++)
            {
                victory1 = true;
                for (int c = 0; c < boardWidthHeight; c++)
                {
                    if (!AnsweredTiles[(boardWidthHeight * r) + c])
                    {
                        victory1 = false;
                        break;
                    }
                }
                if (victory1)
                {
                    for (int i = r * boardWidthHeight; i < (r + 1) * boardWidthHeight; i++)
                    {
                        PlayerTiles[i].SetWinningRow(Highlight);
                    }
                }
            }


            //Check for vertical victory
            for (int r = 0; r < boardWidthHeight; r++)
            {
                victory2 = true;
                for (int c = 0; c < boardWidthHeight; c++)
                {
                    if (!AnsweredTiles[(boardWidthHeight * c) + r])
                    {
                        victory2 = false;
                    }
                }
                if (victory2)
                {
                    for (int i = r; i < r + boardWidthHeight * boardWidthHeight; i += boardWidthHeight)
                    {
                        PlayerTiles[i].SetWinningRow(Highlight);
                    }
                }
            }

            //Check for diagonal victory
            for (int c = 0; c < boardWidthHeight; c++)
            {
                if (!AnsweredTiles[(boardWidthHeight * c) + c])
                {
                    victory3 = false;
                }
            }
            if (victory3)
            {
                for (int c = 0; c < boardWidthHeight; c++)
                {
                    PlayerTiles[(boardWidthHeight * c) + c].SetWinningRow(Highlight);
                }
            }

            for (int c = 0; c < boardWidthHeight; c++)
            {
                if (!AnsweredTiles[(boardWidthHeight * c) + (boardWidthHeight - (c + 1))])
                {
                    victory4 = false;
                }
            }
            if (victory4)
            {
                for (int c = 0; c < boardWidthHeight; c++)
                {
                    PlayerTiles[(boardWidthHeight * c) + (boardWidthHeight - (c + 1))].SetWinningRow(Highlight);
                }
            }
            return (victory1 || victory2 || victory3 || victory4);
        }

        /// <summary>
        /// Rendering of the Player scoring data that are placed in each players section
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            if (HasWon)
            {
                float rotation = 0;
                Vector2 pos = new Vector2(graphicsDevice.Viewport.Width * 0.1f, graphicsDevice.Viewport.Height * 0.4f);
                if (PlayerID < 2)
                {
                    rotation = (float)Math.PI;
                }
                else
                {
                    pos.Y += graphicsDevice.Viewport.Height * 0.2f;
                }


                switch (PlayerID)
                {
                    case 0:
                        pos.X += graphicsDevice.Viewport.Width * 0.2f;
                        break;
                    case 1:
                        pos.X += graphicsDevice.Viewport.Width * 0.8f;
                        break;
                    case 3:
                        pos.X += graphicsDevice.Viewport.Width * 0.6f;
                        break;
                }
                
               
                //TODO possibly change scaling
                float scale = 2f;

#if DEBUG
                scale = 1.2f;
#endif

                //spriteBatch.DrawString(WinnerMessage, "BINGO!!!", pos, Color.White, rotation, new Vector2(0, 0), scale, SpriteEffects.None, 0);
            }
        }
    }
}
