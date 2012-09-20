using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

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

        Microsoft.Xna.Framework.Media.Song BingoSound;

        #endregion

        public Player(List<BingoTile> playerTiles, int playerID, Game game)
        {
           
            this.PlayerTiles = playerTiles;
            AnsweredTiles = new bool[playerTiles.Count];
            this.PlayerID = playerID;
            WinnerMessage = game.Content.Load<SpriteFont>("Comic");
            Highlight = game.Content.Load<Texture2D>("BingoEnvironment/Highlight");
            BingoSound = game.Content.Load<Microsoft.Xna.Framework.Media.Song>("BingoEnvironment/BingoSound");
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

        //Store in array the set of correctly answered tiles for each player (1 array per player)
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

                if (Bingo())
                {
                    MediaPlayer.Play(BingoSound);
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
        public void Update()
        {

        }

        bool Bingo()
        {
            bool victoryHoriz = false, victoryVert = false, victoryDiag = false, temp;
            bool highlighted = false;
            int boardWidthHeight = (int)Math.Sqrt(AnsweredTiles.Length);

            //Check for horizontal victory
            for (int r = 0; r < boardWidthHeight; r++)
            {
                temp = true;
                for (int c = 0; c < boardWidthHeight; c++)
                {
                    if (!AnsweredTiles[(boardWidthHeight * r) + c])
                    {
                        temp = false;
                        break;
                    }
                }
                if (temp)
                {
                    for (int i = r * boardWidthHeight; i < (r + 1) * boardWidthHeight; i++)
                    {
                        // Check if this is a recent victory
                        if (!PlayerTiles[i].SetWinningRow(Highlight))
                        {
                            temp = false;
                        }
                        else
                        {
                            highlighted = true;
                        }
                    }
                }
                victoryHoriz = (victoryHoriz || temp);
            }

            //Check for vertical victory
            for (int r = 0; r < boardWidthHeight; r++)
            {
                temp = true;
                for (int c = 0; c < boardWidthHeight; c++)
                {
                    if (!AnsweredTiles[(boardWidthHeight * c) + r])
                    {
                        temp = false;
                    }
                }
                if (temp)
                {
                    for (int i = r; i < r + boardWidthHeight * boardWidthHeight; i += boardWidthHeight)
                    {
                        // Check if this is a recent victory
                        if (!PlayerTiles[i].SetWinningRow(Highlight))
                        {
                            temp = false;
                        }
                        else
                        {
                            highlighted = true;
                        }
                    }
                }
                victoryVert = (victoryVert || temp);
            }

            //Check for diagonal victory
            temp = true;
            for (int c = 0; c < boardWidthHeight; c++)
            {
                if (!AnsweredTiles[(boardWidthHeight * c) + c])
                {
                    temp = false;
                }
            }
            if (temp)
            {
                for (int c = 0; c < boardWidthHeight; c++)
                {
                    if (!PlayerTiles[(boardWidthHeight * c) + c].SetWinningRow(Highlight))
                    {
                        temp = false;
                    }
                    else
                    {
                        highlighted = true;
                    }
                }
            }
            victoryDiag = (victoryDiag || temp);
            temp = true;
            for (int c = 0; c < boardWidthHeight; c++)
            {
                if (!AnsweredTiles[(boardWidthHeight * c) + (boardWidthHeight - (c + 1))])
                {
                    temp = false;
                }
            }
            if (temp)
            {
                for (int c = 0; c < boardWidthHeight; c++)
                {
                    if (!PlayerTiles[(boardWidthHeight * c) + (boardWidthHeight - (c + 1))].SetWinningRow(Highlight))
                    {
                        temp = false;
                    }
                    else
                    {
                        highlighted = true;
                    }
                }
            }
            victoryDiag = (victoryDiag || temp);
            return highlighted;
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
                
                float scale = 2f;

#if DEBUG
                scale = 1.2f;
#endif
            }
        }
    }
}
