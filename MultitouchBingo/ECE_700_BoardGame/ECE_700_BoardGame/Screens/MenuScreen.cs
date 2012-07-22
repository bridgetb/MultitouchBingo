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
        PlayButton PlayButton;
        DatabaseHelper DBhelper;
        List<String> Topics;
        List<SettingButton> SettingButtons;
        List<MenuButton> EnabledButtons;
        List<Animation> MovingTopics;

        GameDifficulty Difficulty = GameDifficulty.Easy;

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
            EnabledButtons = new List<MenuButton>();
            MovingTopics = new List<Animation>();
            DBhelper = DatabaseHelper.Instance;

            // Set state to first state where user can choose topics
            ScreenState = State.ChooseTopic;
        }

        public void Draw(GameTime gameTime)
        {
            // TODO: Draw BINGO title!
            // Display all enabled options                    
            foreach (MenuButton b in EnabledButtons)
            {
                b.Draw(SpriteBatch);
            }                    
        }

        public void LoadContent(ContentManager content)
        {
            Content = content;

            #region Topic Buttons
            // Display topics
            DataTable dt = this.DBhelper.queryDBRows("select Topic from Topics");
            
            int y = ScreenHeight / 2 - dt.Rows.Count * 50;

            Texture2D tex;
            Rectangle pos;
            foreach (DataRow row in dt.Rows)
            {
                String topic = row.ItemArray[0].ToString();
                tex = Content.Load<Texture2D>("BingoEnvironment/" + topic);
                pos = new Rectangle(ScreenWidth / 2 - tex.Width / 2, y, tex.Width, tex.Height);
                Rectangle target = new Rectangle(ScreenWidth / 4 - tex.Width / 2, y, tex.Width, tex.Height);
                SettingButtons.Add(new SettingButton(Game, tex, pos, target, "TOPIC", topic));
                y += 100;
            }
            #endregion

            #region Difficulty Buttons
            // Display difficulties
            dt = this.DBhelper.queryDBRows("select Difficulty from DifficultyLevels");
            y = ScreenHeight / 2 - dt.Rows.Count * 50;
            foreach (DataRow row in dt.Rows)
            {
                String diff = row.ItemArray[0].ToString();
                tex = Content.Load<Texture2D>("BingoEnvironment/" + diff);
                pos = new Rectangle(ScreenWidth * 5 / 8, y, tex.Width, tex.Height);
                SettingButton sb = new SettingButton(Game, tex, pos, pos, "DIFFICULTY", diff);
                if (diff.Equals("Easy"))
                {
                    sb.Selected = true;
                }
                SettingButtons.Add(sb);
                y += 100;
            }
            #endregion

            #region Play Button
            tex = Content.Load<Texture2D>("BingoEnvironment/Play");
            pos = new Rectangle(ScreenWidth / 2, y, tex.Width, tex.Height);
            PlayButton = new PlayButton(Game, tex, pos, pos);
            #endregion

            this.SetState(State.ChooseTopic);
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
                State initial = this.ScreenState;
                State after = this.ScreenState;
                foreach (MenuButton b in EnabledButtons)
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
                    if (after == State.ChooseDifficulty)
                    {
                        // Activate animations
                        foreach (MenuButton b in EnabledButtons)
                        {
                            if (b is SettingButton)
                            {
                                b.IsTranslating = true;
                            }
                        }
                    }
                    this.SetState(after);
                }
            }
            foreach (MenuButton b in EnabledButtons)
            {
                b.Update(gameTime);
            }
        }

        public void Update(GameTime gameTime, MouseState ms)
        {
            // Check for settings changed
            State initial = this.ScreenState;
            State after = this.ScreenState;
            foreach (MenuButton b in EnabledButtons)
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
                if (after == State.ChooseDifficulty)
                {
                    // Activate animations
                    foreach (MenuButton b in EnabledButtons)
                    {
                        if (b is SettingButton)
                        {
                            b.IsTranslating = true;
                        }
                    }
                }
                this.SetState(after);
            } 
            foreach (MenuButton b in EnabledButtons)
            {
                b.Update(gameTime);
            }
        }

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
