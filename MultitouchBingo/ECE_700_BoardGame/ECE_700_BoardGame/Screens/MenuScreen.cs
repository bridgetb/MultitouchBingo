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
    /// <summary>
    /// Represents the menu screen.
    /// </summary>
    public class MenuScreen : Screen
    {
        Game Game;
        int ScreenHeight;
        int ScreenWidth;
        SpriteBatch SpriteBatch;
        ContentManager Content;
        PlayButton PlayButton;
        DatabaseHelper DBhelper;
        List<String> Topics;
        List<SettingButton> SettingButtons;
        List<Button> EnabledButtons;
        List<Animation> MovingTopics;

        GameDifficulty Difficulty = GameDifficulty.Easy;
        Texture2D BingoTitle;

        ReadOnlyTouchPointCollection TouchesPrevState;

        State ScreenState;

        public enum State { ChooseTopic, ChooseDifficulty, ConfirmOptions }

        public MenuScreen(Game game, SpriteBatch spriteBatch, int screenHeight, int screenWidth)
        {
            this.Game = game;
            this.SpriteBatch = spriteBatch;
            ScreenHeight = screenHeight;
            ScreenWidth = screenWidth;
            Topics = new List<String>();
            SettingButtons = new List<SettingButton>();
            EnabledButtons = new List<Button>();
            MovingTopics = new List<Animation>();
            DBhelper = DatabaseHelper.Instance;

            // Set state to first state where user can choose topics
            ScreenState = State.ChooseTopic;

        }

        public void Draw(GameTime gameTime)
        {
            // Draw BINGO title!
            SpriteBatch.Draw(BingoTitle, new Rectangle(ScreenWidth / 2 - BingoTitle.Width / 2, ScreenHeight / 10, 
                BingoTitle.Width, BingoTitle.Height), Color.White);
            // Display all enabled options                    
            foreach (Button b in EnabledButtons)
            {
                b.Draw(SpriteBatch);
            }                    
        }

        public void LoadContent(ContentManager content)
        {
            Content = content;

            BingoTitle = Content.Load<Texture2D>("BingoEnvironment/BingoTitle");

            #region Topic Buttons
            // Display topics
            DataTable dt = this.DBhelper.queryDBRows("select Topic from Topics");

            int x = (ScreenWidth / (dt.Rows.Count + 1)) - ScreenWidth/10;
            int xSpacing = x + ScreenWidth/5;
            int y = (ScreenHeight / 5)*2;

            Texture2D tex;
            Texture2D texAlt;
            Rectangle pos;
            int frames = ScreenWidth / 100; ;
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                String topic = row.ItemArray[0].ToString();
                tex = Content.Load<Texture2D>("BingoEnvironment/" + topic);
                texAlt = Content.Load<Texture2D>("BingoEnvironment/" + topic + "Active");
                pos = new Rectangle(x - ScreenWidth / 8, y, ScreenWidth/4, ScreenHeight/4);
#if DEBUG
                System.Diagnostics.Debug.WriteLine("width " + tex.Width.ToString() + "; height " + tex.Height.ToString());
#endif
                Rectangle target = new Rectangle(ScreenWidth / 4 - ScreenWidth / 8, y + (i - 1) * ScreenHeight / 4, ScreenWidth / 4, ScreenHeight / 4);
                SettingButtons.Add(new SettingButton(Game, tex, texAlt, pos, target, frames, "TOPIC", topic));
                x += xSpacing;
                i++;
            }
            #endregion

            #region Difficulty Buttons
            // Display difficulties
            dt = this.DBhelper.queryDBRows("select Difficulty from DifficultyLevels");
            y = ScreenHeight / 3;
            x = ScreenWidth * 5 / 8;
            foreach (DataRow row in dt.Rows)
            {
                String diff = row.ItemArray[0].ToString();
                tex = Content.Load<Texture2D>("BingoEnvironment/" + diff);
                x -= tex.Width / 2;
                pos = new Rectangle(x, ScreenHeight / 2, tex.Width, tex.Height);
                Rectangle target = new Rectangle(ScreenWidth * 3 / 4 - tex.Width / 2, y, tex.Width, tex.Height);
                SettingButton sb = new SettingButton(Game, tex, pos, target, frames, "DIFFICULTY", diff);
                if (diff.Equals("Easy"))
                {
                    sb.Selected = true;
                }
                SettingButtons.Add(sb);
                y += (int)(tex.Height * 1.5);
                x += tex.Width / 2 + ScreenWidth / 4;
            }
            #endregion

            #region Play Button
            tex = Content.Load<Texture2D>("BingoEnvironment/Play");
            pos = new Rectangle(ScreenWidth * 3 / 4 - tex.Width / 2, ScreenHeight * 3 / 4, tex.Width, tex.Height);
            PlayButton = new PlayButton(Game, tex, pos, pos);
            #endregion

            this.SetState(State.ChooseTopic);
        }

        /// <summary>
        /// Update for touches.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="touches"></param>
        public void Update(GameTime gameTime, ReadOnlyTouchPointCollection touches)
        {
            if (TouchesPrevState == null)
            {
                TouchesPrevState = touches;
            }
                
            foreach (TouchPoint touch in touches)
            {
                var result = from oldtouch in TouchesPrevState
                             where Helper.Geometry.Contains(touch.Bounds, oldtouch.X, oldtouch.Y) &&
                             touch.Id == oldtouch.Id
                             select oldtouch;

                var sameTouch = result.FirstOrDefault();
                if (sameTouch != null)
                {
                    continue;
                }
                // Check for settings changed
                State initial = this.ScreenState;
                State after = this.ScreenState;
                foreach (Button b in EnabledButtons)
                {
                    if (b.OnTouchTapGesture(touch)) // selected >=1 topic
                    {
                        if (b is SettingButton && ((SettingButton)b).Setting.Equals("TOPIC"))
                            after = State.ChooseDifficulty;
                        else if (b is SettingButton && ((SettingButton)b).Setting.Equals("DIFFICULTY"))
                            after = State.ConfirmOptions;
                    }
                }
                if (EnabledButtons.Contains(PlayButton))
                    PlayButton.OnTouchTapGesture(touch);
                if (after > initial)
                {
                    // Activate animations
                    foreach (Button b in EnabledButtons)
                    {
                        if (b is SettingButton)
                        {
                            b.IsTranslating = true;
                        }
                    }
                    this.SetState(after);
                }
            }
            TouchesPrevState = touches;
            foreach (Button b in EnabledButtons)
            {
                b.Update(gameTime);
            }
        }

        /// <summary>
        /// Update method for mouse clicks (debug mode)
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="ms"></param>
        public void Update(GameTime gameTime, MouseState ms)
        {
            // Check for settings changed
            State initial = this.ScreenState;
            State after = this.ScreenState;
            foreach (Button b in EnabledButtons)
            {
                if (b.OnClickGesture(ms)) // selected >=1 topic
                {
                    if (b is SettingButton && ((SettingButton)b).Setting.Equals("TOPIC"))
                        after = State.ChooseDifficulty;
                    else if (b is SettingButton && ((SettingButton)b).Setting.Equals("DIFFICULTY"))
                        after = State.ConfirmOptions;
                }
            }
            if (EnabledButtons.Contains(PlayButton))
                PlayButton.OnClickGesture(ms);
            if (after > initial)
            {
                // Activate animations
                foreach (Button b in EnabledButtons)
                {
                    if (b is SettingButton)
                    {
                        b.IsTranslating = true;
                    }
                }
                this.SetState(after);
            }
            foreach (Button b in EnabledButtons)
            {
                b.Update(gameTime);
            }
        }

        /// <summary>
        /// Sets the state of the menu screen (i.e. determines which setting buttons to show).
        /// </summary>
        /// <param name="screenState"></param>
        public void SetState(State screenState)
        {
            this.ScreenState = screenState;
            switch (screenState)
            {
                case State.ChooseTopic:
                    var e = from newButton in SettingButtons
                            where newButton.Setting.Equals("TOPIC")
                            && newButton is SettingButton
                            select newButton;
                    foreach (SettingButton b in e)
                    {
                        b.Initialize();
                    }
                    EnabledButtons.AddRange(e);
                    break;
                case State.ChooseDifficulty:
                    e = from newButton in SettingButtons
                            where newButton.Setting.Equals("DIFFICULTY")
                            && newButton is SettingButton
                            select newButton;
                    foreach (SettingButton b in e)
                    {
                        b.Initialize();
                    }
                    EnabledButtons.AddRange(e);
                    break;
                case State.ConfirmOptions:
                    EnabledButtons.Add(PlayButton);
                    break;
                default:
                    break;
            }
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

        /// <summary>
        /// Called when the play button is activated.
        /// </summary>
        public void FinishedSettingOptions()
        {
            if (Game is BingoApp)
            {
                ((BingoApp)Game).StartGame(Topics, Difficulty);
            }
        }

        #endregion
    }
}
