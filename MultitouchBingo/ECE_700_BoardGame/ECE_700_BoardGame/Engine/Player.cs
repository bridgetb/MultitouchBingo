using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;

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
        GameDifficulty DifficultyLevel {get; set;}

        #endregion

        public Player(List<BingoTile> playerTiles, int playerID, GameDifficulty difficulty){
            this.PlayerTiles = playerTiles;
            AnsweredTiles = new bool[playerTiles.Count];
            this.PlayerID = playerID;
            DifficultyLevel = difficulty;
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
            for (int i = 0; i < AnsweredTiles.Length; i++)
            {
                if (!AnsweredTiles[i])
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Rendering of the Player scoring data that are placed in each players section
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {

        }
    }
}
