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
using System.Speech.Recognition;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace VoiceHangmanV2
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        KeyboardState kb, oldkb;
        MouseState mouseState, oldmouseState;
        SpriteFont customfont, titleFont, letterFont;

        SpeechRecognitionEngine recEngine = new SpeechRecognitionEngine();

        int screenWidth, screenHeight;
        Random rand = new Random();

        List<string> possibleWords = new List<string>();
        List<string> commandList = new List<string>();
        string commandListArray;
        char letterToAdd;
        string letterToAddString;
        string randomWord = "";

        int cooldownTimer = 0;
        int testCounter = 0;
        //int indexOfWordHeard = -1;
        int lives = 5;
        int oldlives;

        bool shouldGetNewRandomWord = true;
        bool hasLoadedCommands = false;
        bool hasAddedLetters = false;
        bool doesContainWordHeard = false;
        bool wasFoundInWord = false;
        bool isEntireWordCorrect = false;
        bool hasCompletedWord = false;
        bool isGamePaused = false;

        bool wasLastLetterSaidCorrect = false;

        string wordHeard = "";
        string lastWordHeard = "";
        string lastSingleLetterWord = "";

        string lastLetterAddedBack = "";

        int indexOfLastWordHeard = 0;
        bool hasConfirmedLoss = false;

        bool[] revealedLetters = new bool[16];
        bool[] hasBeenSaid = new bool[26];
        List<string> remainingLetters = new List<string>();




        //StringCollection loadedCommands = Settings1.Default.commands;
        //string[] commandList;


        #region gamestateThings

        enum gameState
        {

            titleScreen, gamePlay, endScreen, Lose, options

        }


        gameState state = gameState.gamePlay;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Sets graphics to 1080p
            this.graphics.PreferredBackBufferWidth = 1200;
            this.graphics.PreferredBackBufferHeight = 650;
            //Shows cursor, is optional
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            screenWidth = GraphicsDevice.Viewport.Width;
            screenHeight = GraphicsDevice.Viewport.Height;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            customfont = Content.Load<SpriteFont>("customfont");
            titleFont = Content.Load<SpriteFont>("titleFont");
            letterFont = Content.Load<SpriteFont>("titleFont");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            kb = Keyboard.GetState();
            mouseState = Mouse.GetState();


            switch (state)
            {

                case gameState.titleScreen:
                    titleScreen();
                    break;
                case gameState.gamePlay:

                    gamePlay(gameTime);
                    break;
                //case gameState.Lose:

                //    Lose();
                //    break;

                //case gameState.endScreen:

                //    endScreen();

                //    break;

                //case gameState.options:

                //    options();

                //    break;

            }
            //changeColor();
            oldkb = kb;
            oldmouseState = mouseState;
            oldlives = lives;

        }

        private void speakToAndFromProgram()
        {
            if (hasAddedLetters == false)
            {
                addLetters();
                addExtraCommands();
                addWords();
                hasAddedLetters = true;
            }


            Choices commands = new Choices();
            commands.Add(commandList.ToArray());
            //testCounter += 1;
            GrammarBuilder gBuilder = new GrammarBuilder();
            gBuilder.Append(commands);
            Grammar grammar = new Grammar(gBuilder);

            recEngine.LoadGrammarAsync(grammar);
            recEngine.SetInputToDefaultAudioDevice();
            recEngine.SpeechRecognized += recEngine_SpeechRecognized;

            /*
            Choices commands = new Choices();
            //commandList = new string[26];

            //loadedCommands.CopyTo(commandList, 0);
            cooldownTimer += 10;
            commands.Add(commandList.ToArray());
            GrammarBuilder gBuilder = new GrammarBuilder();
            gBuilder.Append(commands);
            Grammar grammar = new Grammar(gBuilder);

            recEngine.LoadGrammarAsync(grammar);
            recEngine.LoadGrammar(new DictationGrammar());


            recEngine.SetInputToDefaultAudioDevice();
            //recEngine.SpeechDetected += recEngine_SpeechDetected;

            //recEngine.SpeechRecognitionRejected += recEngine_SpeechRecognitionRejected;
            recEngine.SpeechRecognized += recEngine_SpeechRecognized;

            */

        }

        private void addLetters()
        {
            for (int i = 97; i < 123; i++)
            {
                letterToAdd = (char)i;
                letterToAddString = letterToAdd.ToString();
                commandList.Add(letterToAddString);
                remainingLetters.Add(letterToAddString);

            }

        }

        private void resetForNewWord()
        {
            shouldGetNewRandomWord = true;
            hasConfirmedLoss = false;

            for (int i = 0; i < revealedLetters.Length; i++)
            {
                revealedLetters[i] = false;

            }
            for (int i = 0; i < hasBeenSaid.Length; i++)
            {
                hasBeenSaid[i] = false;


            }

            lives = 5;
            hasCompletedWord = false;

            //commandList.RemoveRange(0, commandList.Count);
            remainingLetters.RemoveRange(0, remainingLetters.Count);

            addLetters();

        }

        private void addWords()
        {
            #region Words
            possibleWords.Add("abruptly");
            possibleWords.Add("absurd");
            possibleWords.Add("abyss");
            possibleWords.Add("affix");
            possibleWords.Add("askew");
            possibleWords.Add("avenue");
            possibleWords.Add("awkward");
            possibleWords.Add("axiom");
            possibleWords.Add("azure");
            possibleWords.Add("bagpipes");
            possibleWords.Add("bandwagon");
            possibleWords.Add("banjo");
            possibleWords.Add("bayou");
            possibleWords.Add("beekeeper");
            possibleWords.Add("bikini");
            possibleWords.Add("blitz");
            possibleWords.Add("blizzard");
            possibleWords.Add("boggle");
            possibleWords.Add("bookworm");
            possibleWords.Add("boxcar");
            possibleWords.Add("boxful");
            possibleWords.Add("buckaroo");
            possibleWords.Add("buffalo");
            possibleWords.Add("buffoon");
            possibleWords.Add("buxom");
            possibleWords.Add("buzzard");
            possibleWords.Add("buzzing");
            possibleWords.Add("buzzwords");
            possibleWords.Add("caliph");
            possibleWords.Add("cobweb");
            possibleWords.Add("cockiness");
            possibleWords.Add("croquet");
            possibleWords.Add("crypt");
            possibleWords.Add("curacao");
            possibleWords.Add("cycle");
            possibleWords.Add("daiquiri");
            possibleWords.Add("dirndl");
            possibleWords.Add("disavow");
            possibleWords.Add("dizzying");
            possibleWords.Add("duplex");
            possibleWords.Add("dwarves");
            possibleWords.Add("embezzle");
            possibleWords.Add("equip");
            possibleWords.Add("espionage");
            possibleWords.Add("euouae");
            possibleWords.Add("exodus");
            possibleWords.Add("faking");
            possibleWords.Add("fishhook");
            possibleWords.Add("fixable");
            possibleWords.Add("fjord");
            possibleWords.Add("flapjack");
            possibleWords.Add("flopping");
            possibleWords.Add("fluffiness");
            possibleWords.Add("flyby");
            possibleWords.Add("foxglove");
            possibleWords.Add("frazzled");
            possibleWords.Add("frizzled");
            possibleWords.Add("fuchsia");
            possibleWords.Add("funny");
            possibleWords.Add("gabby");
            possibleWords.Add("galaxy");
            possibleWords.Add("galvanize");
            possibleWords.Add("gazebo");
            possibleWords.Add("giaour");
            possibleWords.Add("gizmo");
            possibleWords.Add("glowworm");
            possibleWords.Add("glyph");
            possibleWords.Add("gnarly");
            possibleWords.Add("gnostic");
            possibleWords.Add("gossip");
            possibleWords.Add("grogginess");
            possibleWords.Add("haiku");
            possibleWords.Add("haphazard");
            possibleWords.Add("hyphen");
            possibleWords.Add("iatrogenic");
            possibleWords.Add("icebox");
            possibleWords.Add("injury");
            possibleWords.Add("ivory");
            possibleWords.Add("ivy");
            possibleWords.Add("jackpot");
            possibleWords.Add("jaundice");
            possibleWords.Add("jawbreaker");
            possibleWords.Add("jaywalk");
            possibleWords.Add("jazziest");
            possibleWords.Add("jazzy");
            possibleWords.Add("jelly");
            possibleWords.Add("jigsaw");
            possibleWords.Add("jinx");
            possibleWords.Add("jiujitsu");
            possibleWords.Add("jockey");
            possibleWords.Add("jogging");
            possibleWords.Add("joking");
            possibleWords.Add("jovial");
            possibleWords.Add("joyful");
            possibleWords.Add("juicy");
            possibleWords.Add("jukebox");
            possibleWords.Add("jumbo");
            possibleWords.Add("kayak");
            possibleWords.Add("kazoo");
            possibleWords.Add("keyhole");
            possibleWords.Add("khaki");
            possibleWords.Add("kilobyte");
            possibleWords.Add("kiosk");
            possibleWords.Add("kitsch");
            possibleWords.Add("kiwifruit");
            possibleWords.Add("klutz");
            possibleWords.Add("knapsack");
            possibleWords.Add("larynx");
            possibleWords.Add("lengths");
            possibleWords.Add("lucky");
            possibleWords.Add("luxury");
            possibleWords.Add("lymph");
            possibleWords.Add("marquis");
            possibleWords.Add("matrix");
            possibleWords.Add("megahertz");
            possibleWords.Add("microwave");
            possibleWords.Add("mnemonic");
            possibleWords.Add("mystify");
            possibleWords.Add("naphtha");
            possibleWords.Add("nightclub");
            possibleWords.Add("nowadays");
            possibleWords.Add("numbskull");
            possibleWords.Add("nymph");
            possibleWords.Add("onyx");
            //possibleWords.Add("ovary");
            possibleWords.Add("oxidize");
            possibleWords.Add("oxygen");
            possibleWords.Add("pajama");
            possibleWords.Add("peekaboo");
            possibleWords.Add("phlegm");
            possibleWords.Add("pixel");
            possibleWords.Add("pizazz");
            possibleWords.Add("pneumonia");
            possibleWords.Add("polka");
            possibleWords.Add("pshaw");
            possibleWords.Add("psyche");
            possibleWords.Add("puppy");
            possibleWords.Add("puzzling");
            possibleWords.Add("quartz");
            possibleWords.Add("queue");
            possibleWords.Add("quips");
            possibleWords.Add("quixotic");
            possibleWords.Add("quiz");
            possibleWords.Add("quizzes");
            possibleWords.Add("quorum");
            possibleWords.Add("razzmatazz");
            possibleWords.Add("rhubarb");
            possibleWords.Add("rhythm");
            possibleWords.Add("rickshaw");
            possibleWords.Add("schnapps");
            possibleWords.Add("scratch");
            possibleWords.Add("shiv");
            possibleWords.Add("snazzy");
            possibleWords.Add("sphinx");
            possibleWords.Add("spritz");
            possibleWords.Add("squawk");
            possibleWords.Add("staff");
            possibleWords.Add("strength");
            possibleWords.Add("strengths");
            possibleWords.Add("stretch");
            possibleWords.Add("stronghold");
            possibleWords.Add("stymied");
            possibleWords.Add("subway");
            possibleWords.Add("swivel");
            possibleWords.Add("syndrome");
            possibleWords.Add("thriftless");
            possibleWords.Add("thumbscrew");
            possibleWords.Add("topaz");
            possibleWords.Add("transcript");
            possibleWords.Add("transgress");
            possibleWords.Add("transplant");
            possibleWords.Add("triphthong");
            possibleWords.Add("twelfth");
            possibleWords.Add("twelfths");
            possibleWords.Add("unknown");
            possibleWords.Add("unworthy");
            possibleWords.Add("unzip");
            possibleWords.Add("uptown");
            possibleWords.Add("vaporize");
            possibleWords.Add("vixen");
            possibleWords.Add("vodka");
            possibleWords.Add("voodoo");
            possibleWords.Add("vortex");
            possibleWords.Add("voyeurism");
            possibleWords.Add("walkway");
            possibleWords.Add("waltz");
            possibleWords.Add("wave");
            possibleWords.Add("wavy");
            possibleWords.Add("waxy");
            possibleWords.Add("wellspring");
            possibleWords.Add("wheezy");
            possibleWords.Add("whiskey");
            possibleWords.Add("whizzing");
            possibleWords.Add("whomever");
            possibleWords.Add("wimpy");
            possibleWords.Add("witchcraft");
            possibleWords.Add("wizard");
            possibleWords.Add("woozy");
            possibleWords.Add("wristwatch");
            possibleWords.Add("wyvern");
            possibleWords.Add("xylophone");
            possibleWords.Add("yachtsman");
            possibleWords.Add("yippee");
            possibleWords.Add("yoked");
            possibleWords.Add("youthful");
            possibleWords.Add("yummy");
            possibleWords.Add("zephyr");
            possibleWords.Add("zigzag");
            possibleWords.Add("zigzagging");
            possibleWords.Add("zilch");
            possibleWords.Add("zipper");
            possibleWords.Add("zodiac");
            possibleWords.Add("zombie");

            #endregion


        }

        private void addExtraCommands()
        {
            commandList.Add("undo");
            commandList.Add("new word");
            commandList.Add("play again");
            commandList.Add("new game");
            commandList.Add("pause");
            commandList.Add("resume");
            commandList.Add("start");
            commandList.Add("play");
            commandList.Add("confirm");
            commandList.Add("confirm loss");
            commandList.Add("alfa");
            commandList.Add("bravo");
            commandList.Add("beta");
            commandList.Add("charlie");
            commandList.Add("delta");
            commandList.Add("echo");
            commandList.Add("foxtrot");
            commandList.Add("golf");
            commandList.Add("hotel");
            commandList.Add("hostile");
            commandList.Add("india");
            commandList.Add("juliett");
            commandList.Add("kilo");
            commandList.Add("lima");
            commandList.Add("mike");
            commandList.Add("november");
            commandList.Add("oscar");
            commandList.Add("papa");
            commandList.Add("quebec");
            commandList.Add("romeo");
            commandList.Add("sierra");
            commandList.Add("tango");
            commandList.Add("uniform");
            commandList.Add("umbrella");
            commandList.Add("victor");
            commandList.Add("whiskey");
            commandList.Add("xray");
            commandList.Add("yankee");
            commandList.Add("zulu");

        }

        private void checkLetterSaid()
        {

            wasFoundInWord = false;

            if (wordHeard.Length == 1)
            {
                wasLastLetterSaidCorrect = false;
                for (int i = 0; i < randomWord.Length; i++)
                {
                    if (wordHeard.Equals(randomWord.Substring(i, 1)))
                    {
                        revealedLetters[i] = true;
                        wasFoundInWord = true;
                        wasLastLetterSaidCorrect = true;
                    }

                }

                if (wasFoundInWord == false)
                {
                    if (remainingLetters.Contains(wordHeard) && hasCompletedWord == false)
                    {
                        lives -= 1;

                    }

                }

            }

        }

        private void checkEntireWord()
        {
            if (lives > 0)
            {
                isEntireWordCorrect = true;
                for (int i = 0; i < randomWord.Length && isEntireWordCorrect == true; i++)
                {
                    if (revealedLetters[i] != true)
                    {
                        isEntireWordCorrect = false;
                    }

                }

                if (isEntireWordCorrect == true)
                {
                    hasCompletedWord = true;

                }

            }


        }

        //private void removeCorrectLetter()
        //{

        //    for (int i = 0; i < randomWord.Length; i++)
        //    {

        //        if (wordHeard.Equals(randomWord.Substring(i, 1)))
        //        {

        //        }


        //    }



        //}

        void recEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (isGamePaused == false && lives > 0)
            {
                wordHeard = "";
                bool hasFoundWordHeard = false;
                for (int i = 0; i < 26; i++)
                {
                    if(commandList[i].Equals(e.Result.Text.ToLower()))
                    {
                        wordHeard = commandList[i];
                        hasFoundWordHeard = true;
                    }

                }
                if (hasFoundWordHeard == false)
                {
                    for (int i = 36; i < commandList.Count; i++)
                    {
                        if (commandList[i].Equals(e.Result.Text.ToLower()))
                        {
                            wordHeard = commandList[i].Substring(0,1);
                            hasFoundWordHeard = true;
                        }
                    }
                }

                //switch (e.Result.Text.ToLower())
                //{
                //    case "alfa":
                //    case "a":
                //        wordHeard = "a";
                //        break;
                //    case "bravo":
                //    case "beta":
                //    case "b":
                //        wordHeard = "b";
                //        break;
                //    case "charlie":
                //    case "c":
                //        wordHeard = "c";
                //        break;
                //    case "delta":
                //    case "d":
                //        wordHeard = "d";
                //        break;
                //    case "echo":
                //    case "e":
                //        wordHeard = "e";
                //        break;
                //    case "foxtrot":
                //    case "f":
                //        wordHeard = "f";
                //        break;
                //    case "golf":
                //    case "g":
                //        wordHeard = "g";
                //        break;
                //    case "hotel":
                //    case "hostile":
                //    case "h":
                //        wordHeard = "h";
                //        break;
                //    case "india":
                //    case "i":
                //        wordHeard = "i";
                //        break;
                //    case "juliett":
                //    case "j":
                //        wordHeard = "j";
                //        break;
                //    case "kilo":
                //    case "k":
                //        wordHeard = "k";
                //        break;
                //    case "lima":
                //    case "l":
                //        wordHeard = "l";
                //        break;
                //    case "mike":
                //    case "m":
                //        wordHeard = "m";
                //        break;
                //    case "november":
                //    case "n":
                //        wordHeard = "n";
                //        break;
                //    case "oscar":
                //    case "o":
                //        wordHeard = "o";
                //        break;
                //    case "papa":
                //    case "p":
                //        wordHeard = "p";
                //        break;
                //    case "quebec":
                //    case "q":
                //        wordHeard = "q";
                //        break;
                //    case "romeo":
                //    case "r":
                //        wordHeard = "r";
                //        break;
                //    case "sierra":
                //    case "s":
                //        wordHeard = "s";
                //        break;
                //    case "tango":
                //    case "t":
                //        wordHeard = "t";
                //        break;
                //    case "uniform":
                //    case "u":
                //        wordHeard = "u";
                //        break;
                //    case "victor":
                //    case "v":
                //        wordHeard = "v";
                //        break;
                //    case "whiskey":
                //    case "w":
                //        wordHeard = "w";
                //        break;
                //    case "x-ray":
                //    case "x":
                //        wordHeard = "x";
                //        break;
                //    case "yankee":
                //    case "y":
                //        wordHeard = "y";
                //        break;
                //    case "zulu":
                //    case "z":
                //        wordHeard = "z";
                //        break;

                //}

                if (e.Result.Text.Length == 1)
                {
                    lastSingleLetterWord = wordHeard;
                    testCounter++;
                }

                checkLetterSaid();
                checkEntireWord();

                //for (int i = 0; i < 26; i++)
                //{
                //    if (wordHeard == commandList[i])
                //    {
                //        hasBeenSaid[i] = true;
                //        remainingLetters.Remove(commandList[i]);

                //    }

                //}
                if (wordHeard.Length == 1)
                {
                    if (remainingLetters.Contains(wordHeard))
                    {
                        indexOfLastWordHeard = remainingLetters.IndexOf(wordHeard);
                    }
                    remainingLetters.Remove(wordHeard);
                    if (lastWordHeard != wordHeard && wordHeard.Length == 1)
                    {
                        lastWordHeard = wordHeard;
                    }

                }
            }

            switch (e.Result.Text.ToLower())
            {
                case "new word":
                case "new game":


                    resetForNewWord();
                    chooseRandomWord();
                    //synthesizer.Speak("Adding 200 to the cool down timer");

                    commandList.RemoveRange(commandList.Count - 26, 26);

                    break;

                case "play again":
                    if (hasCompletedWord == true)
                    {
                        resetForNewWord();
                        chooseRandomWord();
                    }
                    break;
                case "undo":

                    

                    //if (randomWord.Contains(lastWordHeard) == false && lastWordHeard.Length == 1 && lastWordHeard.Equals(lastSingleLetterWord))
                    //{
                    //    lives += 1;
                    //}
                    if (randomWord.Contains(lastWordHeard) == false && lives < 5)
                    {
                        hasBeenSaid[indexOfLastWordHeard] = false;
                        remainingLetters.Insert(indexOfLastWordHeard, lastSingleLetterWord);
                        lives += 1;
                    }
                    break;
                case "confirm":
                case "confirm loss":
                    if (lives <= 0)
                    {
                        hasConfirmedLoss = true;
                    }
                    break;
                case "pause":
                    isGamePaused = true;
                    break;
                case "play":
                case "resume":
                case "start":
                    isGamePaused = false;
                    break;

            }

        }

        private void titleScreen()
        {

        }

        private void gamePlay(GameTime gameTime)
        {

            if (hasLoadedCommands == false)
            {

                speakToAndFromProgram();
                recEngine.RecognizeAsync(RecognizeMode.Multiple);

                hasLoadedCommands = true;
            }

            chooseRandomWord();


        }

        private void chooseRandomWord()
        {
            if (shouldGetNewRandomWord == true)
            {
                randomWord = possibleWords.ElementAt(rand.Next(possibleWords.Count));
                shouldGetNewRandomWord = false;
            }
        }

        private void drawTitleScreen()
        {


        }

        private void drawLose()
        {

        }

        private void drawEndScreen()
        {

        }
        private void drawOptionsScreen()
        {


        }

        private void drawGamePlay(GameTime gameTime)
        {
            //lastWordHeard = wordHeard;


            spriteBatch.DrawString(customfont, "Word Heard: " + wordHeard, new Vector2(10, 270), Color.Black);
            spriteBatch.DrawString(customfont, "last Word Heard: " + lastWordHeard, new Vector2(10, 320), Color.Black);
            spriteBatch.DrawString(customfont, "last single letter: " + lastSingleLetterWord, new Vector2(10, 370), Color.Black);
            //spriteBatch.DrawString(customfont, "CooldownTimer: " + cooldownTimer, new Vector2(10, 170), Color.Black);
            spriteBatch.DrawString(customfont, "revealedLettersLength: " + revealedLetters.Length, new Vector2(10, 420), Color.Black);
            //spriteBatch.DrawString(customfont, (randomWord.Length / 2 * 25).ToString(), new Vector2(10, 420), Color.Black);
            spriteBatch.DrawString(customfont, "indexofLastwordHeard: " + indexOfLastWordHeard, new Vector2(10, 220), Color.Black);
            spriteBatch.DrawString(customfont, "isGamePaused: " + isGamePaused, new Vector2(10, 600), Color.Black);
            //spriteBatch.DrawString(customfont, "remainingLetterLength: " + remainingLetters.Count, new Vector2(10, 50), Color.Black);

            //for debugging
            //spriteBatch.DrawString(customfont, "The word: " + randomWord, new Vector2(10, 470), Color.Black);
            spriteBatch.DrawString(customfont, "Lives: " + lives, new Vector2(10, 550), Color.Black);
            
            spriteBatch.DrawString(customfont, "  Available Commands\n[use when applicable]: ", new Vector2(screenWidth - 290, 10), Color.Black);
            //commands
            int commandYPos = 60;
            int xPos = 200;
            for (int i = 26; i < 36; i++)
            {
                if (commandList[i].Length != 1)
                {
                    spriteBatch.DrawString(customfont, commandList[i], new Vector2(screenWidth - 220, commandYPos), Color.Black);
                    commandYPos += 22;
                }
            }

            //show all commands
            
            //for (int i = 0; i < commandList.Count; i++)
            //{
            //    //if (commandList[i].Length != 1)
            //    //{
            //    spriteBatch.DrawString(customfont, i + " " + commandList[i], new Vector2(xPos, commandYPos), Color.Black);
            //    commandYPos += 22;
            //    if (commandYPos > 600)
            //    {
            //        commandYPos = 60;
            //        xPos += 130;
            //    }
            //    //}
            //}

            //command list length
            //spriteBatch.DrawString(customfont, commandList.Count.ToString(), new Vector2(screenWidth - 220, 315), Color.Black);

            commandYPos = 40;
            int xPos2 = 750;

            //nato alphabet
            for (int i = 36; i < commandList.Count; i++)
            {
                if (commandList[i].Length != 1)
                {
                    spriteBatch.DrawString(customfont, commandList[i], new Vector2(xPos2, commandYPos), Color.Black);
                    if (i != commandList.Count - 1 && commandList[i + 1].Substring(0, 1).Equals(commandList[i].Substring(0, 1)))
                    {
                        spriteBatch.DrawString(customfont, "|" + commandList[i + 1], new Vector2(xPos2 + (commandList[i].Length * 13), commandYPos), Color.Black);
                        i++;
                    }
                    //spriteBatch.DrawString(customfont, commandList[i], new Vector2(screenWidth - 420, commandYPos), Color.Black);
                    commandYPos += 22;
                }
            }


            //display letters
            int xpos = screenWidth - 250;
            int ypos = 290; ;

            for (int i = 0; i < remainingLetters.Count; i++)
            {
                if (hasBeenSaid[i] == false)
                {
                    spriteBatch.DrawString(letterFont, remainingLetters[i], new Vector2(xpos, ypos), Color.Black);
                }
                xpos += 50;
                if (i % 5 == 0 && i + 1 != 0)
                {
                    ypos += 55;
                    xpos = screenWidth - 250;
                }

            }

            //draw blanks

            int wordX = (screenWidth / 2 - (randomWord.Length * 25)) - 70;
            //int blankX = screenWidth / 2 - randomWord.Length / 2 * 10;
            for (int i = 0; i < randomWord.Length; i++)
            {
                if (revealedLetters[i] == true)
                {

                    spriteBatch.DrawString(titleFont, randomWord.Substring(i, 1) + "  ", new Vector2(wordX, 38), Color.Black);
                    //spriteBatch.DrawString(titleFont, "T" + " ", new Vector2(wordX, 108), Color.Black);

                }
                //else
                //{

                //    spriteBatch.DrawString(titleFont, "F" + " ", new Vector2(wordX, 108), Color.Black);

                //}

                spriteBatch.DrawString(titleFont, "_", new Vector2(wordX, 40), Color.Black);
                //blankX += 50;
                wordX += 50;

            }

            if (hasCompletedWord == true)
            {
                spriteBatch.DrawString(titleFont, "Word Completed!\nSay 'Play Again'\n  To start a\n  new word", new Vector2(screenWidth / 2 - 250, 188), Color.Black);


            }

            if (lives <= 0 && hasConfirmedLoss == false)
            {
                spriteBatch.DrawString(titleFont, "Say 'confirm'\nto confirm\nlast letter", new Vector2(screenWidth / 2 - 250, 188), Color.Black);


            }

            if (lives <= 0 && hasConfirmedLoss == true)
            {
                spriteBatch.DrawString(titleFont, "You Lose!\nThe word was\n  " + randomWord +
                    "\nsay 'new word'\nto start a \nnew word", new Vector2(screenWidth / 2 - 250, 188), Color.Black);

            }



        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightBlue);

            spriteBatch.Begin();

            switch (state)
            {

                case gameState.titleScreen:
                    drawTitleScreen();
                    break;
                case gameState.gamePlay:

                    drawGamePlay(gameTime);
                    break;
                case gameState.Lose:

                    drawLose();
                    break;

                case gameState.endScreen:

                    drawEndScreen();

                    break;

                case gameState.options:

                    drawOptionsScreen();

                    break;

            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
