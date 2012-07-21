using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ECE_700_BoardGame.Engine;
using System.Data;
using ECE_700_BoardGame.Helper;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace ECE_700_BoardGame.Screens
{
    public class MenuScreen : Screen
    {
        Game Game;
        int ScreenHeight;
        int ScreenWidth;
        SpriteBatch SpriteBatch;
        ContentManager Content;
        ContinueButton PlayButton;
        DatabaseHelper DBhelper;
        List<String> Topics;
        List<SettingButton> SettingButtons;

        GameDifficulty Difficulty = GameDifficulty.Easy;

        ReadOnlyTouchPointCollection TouchesPrevState;

        public MenuScreen(Game game, SpriteBatch spriteBatch, int screenHeight, int screenWidth)
        {
            this.Game = game;
            this.SpriteBatch = spriteBatch;
            ScreenHeight = screenHeight;
            ScreenWidth = screenWidth;
            Topics = new List<String>();
            SettingButtons = new List<SettingButton>();
            DBhelper = DatabaseHelper.Instance;
        }

        public void Draw(GameTime gameTime)
        {
            // Display topic options
            int y = ScreenHeight / 2 - SettingButtons.Count * 50;
            String categories = "Categories:";
            SpriteFont font = Content.Load<SpriteFont>("Comic");
            String difficulty = "Difficulty levels:";
            SpriteBatch.DrawString(font, categories, new Vector2(ScreenWidth / 8, y - 100), Color.Black,
                0, new Vector2(0, 0), 1, SpriteEffects.None, 0);
            SpriteBatch.DrawString(font, difficulty, new Vector2(ScreenWidth * 5 / 8, y - 100), Color.Black,
                0, new Vector2(0, 0), 1, SpriteEffects.None, 0);

            foreach (SettingButton b in SettingButtons)
            {
                b.Draw(SpriteBatch);
            }

            // OK button
            PlayButton.Draw(SpriteBatch);
        }

        public void LoadContent(ContentManager content)
        {
            Content = content;

            #region Game Settings
            // Display topics
            DataTable dt = this.DBhelper.queryDBRows("select Topic from Topics");
            
            int y = ScreenHeight / 2 - dt.Rows.Count * 50;

            Texture2D tex;
            Rectangle pos;
            foreach (DataRow row in dt.Rows)
            {
                String topic = row.ItemArray[0].ToString();
                tex = Content.Load<Texture2D>("BingoEnvironment/" + topic);
                pos = new Rectangle(ScreenWidth / 8, y, tex.Width, tex.Height);
                SettingButtons.Add(new SettingButton(Game, tex, pos, "TOPIC", topic));
                y += 100;
            }
            // Display difficulties
            dt = this.DBhelper.queryDBRows("select Difficulty from DifficultyLevels");
            y = ScreenHeight / 2 - dt.Rows.Count * 50;
            foreach (DataRow row in dt.Rows)
            {
                String diff = row.ItemArray[0].ToString();
                tex = Content.Load<Texture2D>("BingoEnvironment/" + diff);
                pos = new Rectangle(ScreenWidth * 5 / 8, y, tex.Width, tex.Height);
                SettingButton sb = new SettingButton(Game, tex, pos, "DIFFICULTY", diff);
                if (diff.Equals("Easy"))
                {
                    sb.Selected = true;
                }
                SettingButtons.Add(sb);
                y += 100;
            }

            tex = Content.Load<Texture2D>("BingoEnvironment/Play");
            pos = new Rectangle(ScreenWidth / 2, y, tex.Width, tex.Height);
            PlayButton = new ContinueButton(Game, tex, pos);
            #endregion
        }

        public void Update(GameTime gameTime, ReadOnlyTouchPointCollection touches)
        {
            if (TouchesPrevState == null)
            {
                TouchesPrevState = touches;
            }
                
            foreach (TouchPoint touch in touches)
            {
                var result = from oldtouch in TouchesPrevState
                             from newtouch in touches
                             where Helper.Geometry.Contains(newtouch.Bounds, oldtouch.X, oldtouch.Y) &&
                             newtouch.Id == oldtouch.Id
                             select oldtouch;

                var sameTouch = result.FirstOrDefault();
                if (sameTouch != null)
                {
                    continue;
                }
                // Check for settings changed
                foreach (SettingButton b in this.SettingButtons)
                {
                    b.OnTouchTapGesture(touch);
                }
                PlayButton.OnTouchTapGesture(touch, gameTime);
            }
        }

        public void Update(GameTime gameTime, MouseState ms)
        {
            // Check for settings changed
            foreach (SettingButton b in this.SettingButtons)
            {
                b.OnClickGesture(ms);
            }
            PlayButton.OnClickGesture(ms, gameTime);
        }

        #region Options Setting
        public void AddSetting(String setting, String value)
        {
            switch (setting)
            {
                case "TOPIC":
                    if (!Topics.Contains(value))
                        this.Topics.Add(value);
                    break;
                case "DIFFICULTY":
                    // Ensure that only one level is selected at any point in time
                    var result = from b in SettingButtons
                                 where b.Setting.Equals(setting)
                                 select b;
                    foreach (SettingButton b in result)
                    {
                        // If button is not the selected difficulty and has been selected, deselect
                        if (!b.Value.Equals(value))
                        {
                            b.Selected = false;
                        }
                        else
                        {
                            b.Selected = true; // Otherwise "undo" press
                        }
                    }
                    if (value.Equals("Hard"))
                    {
                        this.Difficulty = GameDifficulty.Hard;
                    }
                    else
                    {
                        this.Difficulty = GameDifficulty.Easy;
                    }
                    break;
                default:
                    return;
            }
        }

        public void RemoveSetting(String setting, String value)
        {
            switch (setting)
            {
                case "TOPIC":
                    this.Topics.Remove(value);
                    break;
                case "DIFFICULTY":
                    // Ensure that only one level is selected at any point in time
                    var result = from b in SettingButtons
                                 where b.Setting.Equals(setting)
                                 select b;
                    foreach (SettingButton b in result)
                    {
                        // If button is not the selected difficulty and has been selected, deselect
                        if (!b.Value.Equals(value))
                        {
                            b.Selected = false;
                        }
                        else
                        {
                            b.Selected = true; // Otherwise "undo" press
                        }
                    }
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        /// Sets the difficulty level of questions/answers - default is easy
        /// </summary>
        /// <param name="difficulty"></param>
        public void SetDifficulty(int difficulty)
        {
            switch (difficulty)
            {
                case (1):
                    Difficulty = GameDifficulty.Easy;
                    break;
                case (2):
                    Difficulty = GameDifficulty.Hard;
                    break;
                default:
                    Difficulty = GameDifficulty.Easy;
                    break;
            }
        }

        public void FinishedSettingOptions(GameTime gameTime)
        {
            if (Game is BingoApp)
            {
                ((BingoApp)Game).StartGame(Topics, Difficulty);
            }
        }

        #endregion
    }
}
