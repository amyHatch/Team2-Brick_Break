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


namespace WindowsGame4
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    /// 



    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        SpriteFont font, font2;

        Texture2D ball;
        Texture2D bar;
        Texture2D block;
        Texture2D background;

        Vector2 ballPos;
        Vector2 barPos;
        Vector2[] blocksPos;


        Vector2 ballVel;
        Vector2 barVel;

        Random rdm;
        int height, width;

        SoundEffect sound, blockhit;
        SoundEffectInstance soundEngineInstance, soundEngineInstance2;

        int score = 0;
        int hits = 0;  //number of blocks hit.

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            rdm = new Random();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            background = Content.Load<Texture2D>("background_brickbreak");
            font = Content.Load<SpriteFont>("SpriteFont1"); //for score
            font2 = Content.Load<SpriteFont>("SpriteFont2"); //for game over
            spriteBatch = new SpriteBatch(GraphicsDevice);

            width = graphics.GraphicsDevice.Viewport.Width;
            height = graphics.GraphicsDevice.Viewport.Height;


            ball = Content.Load<Texture2D>("ball");
            bar = Content.Load<Texture2D>("paddle"); //change to paddle

            //ballPos = new Vector2(width/2 + rdm.Next()%100 - 50, rdm.Next() % (height/2));
            ballPos = new Vector2(width / 2, height / 2);
            barPos = new Vector2(width / 2 - bar.Width, height - bar.Height);

            ballVel = new Vector2(150.0f, 150.0f);
            barVel = new Vector2(220.0f, 220.0f);

            block = Content.Load<Texture2D>("brick");
            blocksPos = new Vector2[35]; //creates 40 blocks

            int i = 0; //used for number of blocks
            int k = height - 50; //used for blocks height
            while (i < blocksPos.Length) //positions blocks
            {
                //blocksPos[i] = new Vector2((float)(rdm.NextDouble() * width - block.Width), (float)(rdm.NextDouble() * height / 2));
                for (int j = 0; j < width / (block.Width * 2); j++) //j is number of blocks in row
                {
                    if (i < blocksPos.Length)
                    {
                        blocksPos[i] = new Vector2((width - ((2 * j) + 2) * block.Width), (height - k));
                        //blocksPos[i] = new Vector2((width - block.Width), (height / 4));
                        i++;
                    }
                }
                k = k - block.Height * 2;
            }

            blockhit = Content.Load<SoundEffect>("chimes");
            sound = Content.Load<SoundEffect>("kong");
            soundEngineInstance = sound.CreateInstance();
            soundEngineInstance.Volume = 0.5f;
            soundEngineInstance.IsLooped = true;
            soundEngineInstance.Play();


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

            ballPos += ballVel * (float)gameTime.ElapsedGameTime.TotalSeconds;

            detectBallWallCollision();
            detectBallBarCollision();
            detectBallBrickCollision();
            processKeyboard(gameTime);

            base.Update(gameTime);
        }

        void detectBallWallCollision()
        {
            if (ballPos.X < 0)
            {
                ballVel.X *= -1;
                ballPos.X = 0;
            }
            if (ballPos.Y < 0)
            {
                ballVel.Y *= -1;
                ballPos.Y = 0;
            }
            if (ballPos.X > width - ball.Width)
            {
                ballVel.X *= -1;
                ballPos.X = width - ball.Width;
            }
            if (ballPos.Y > height - ball.Height)
            {
                ballVel.Y *= -1;
                ballPos.Y = height - ball.Height;
                score -= 10; //ball hits floor
            }
        }

        void detectBallBarCollision()
        {
            Vector3 ballMin = new Vector3(ballPos.X, ballPos.Y, 0);
            Vector3 ballMax = new Vector3(ballPos.X + ball.Width, ballPos.Y + ball.Height, 0);

            Vector3 barMin = new Vector3(barPos.X, barPos.Y, 0);
            Vector3 barMax = new Vector3(barPos.X + bar.Width, barPos.Y + bar.Height, 0);

            BoundingBox bb1, bb2;

            bb1.Min = ballMin;
            bb1.Max = ballMax;

            bb2.Min = barMin;
            bb2.Max = barMax;

            if (bb1.Intersects(bb2))
            {
                ballVel.Y *= -1;
                ballPos.Y = barPos.Y - ball.Height;
            }
        }

        void detectBallBrickCollision()
        {
            Vector3 ballMin = new Vector3(ballPos.X, ballPos.Y, 0);
            Vector3 ballMax = new Vector3(ballPos.X + ball.Width, ballPos.Y + ball.Height, 0);
            BoundingBox bb1 = new BoundingBox(ballMin, ballMax);

            Vector2 nearestPnt = ballPos;
            Vector3 brickMin, brickMax;

            for (int i = 0; i < blocksPos.Length; i++)
            {
                if (blocksPos[i] != Vector2.Zero)
                {
                    brickMin = new Vector3(blocksPos[i].X, blocksPos[i].Y, 0);
                    brickMax = new Vector3(blocksPos[i].X + block.Width, blocksPos[i].Y + block.Height, 0);

                    if (bb1.Intersects(new BoundingBox(brickMin, brickMax)))
                    {
                        //what should we do? change x or y velocity?

                        //checking to see if it's to left of brick
                        if (ballPos.X < blocksPos[i].X && Math.Abs(blocksPos[i].Y - ballPos.Y) <= ball.Width / 2)
                        {
                            ballVel.X *= -1;
                            ballPos.X = blocksPos[i].X - ball.Width;
                        }
                        //check to see if it's too the right
                        else if (ballPos.X + ball.Width > blocksPos[i].X + block.Width && Math.Abs(blocksPos[i].Y - ballPos.Y) <= ball.Width / 2)
                        {
                            ballVel.X *= -1;
                            ballPos.X = blocksPos[i].X + block.Width;
                        }
                        else
                        {
                            ballVel.Y *= -1;
                            if (ballPos.Y < blocksPos[i].Y)
                            {
                                ballPos.Y = blocksPos[i].Y - ball.Height + 1;
                            }
                            else
                            {
                                ballPos.Y = blocksPos[i].Y + block.Height - 1;
                            }
                        }
                        blocksPos[i] = Vector2.Zero;
                        score += 5; //ball hits brick
                        hits += 1;

                        soundEngineInstance2 = blockhit.CreateInstance();
                        soundEngineInstance2.Volume = 1;
                        soundEngineInstance2.Play();

                    }
                }
            }

        }
        void processKeyboard(GameTime gameTime)
        {
            KeyboardState s = Keyboard.GetState();

            if (s.IsKeyDown(Keys.Left) && barPos.X > 0)
            {
                barPos.X -= barVel.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (s.IsKeyDown(Keys.Right) && barPos.X + bar.Width < width)
            {
                barPos.X += barVel.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here


            spriteBatch.Begin();
            Rectangle screenRectangle = new Rectangle(0, 0, width, height);
            spriteBatch.Draw(background, screenRectangle, Color.White); //background
            spriteBatch.Draw(ball, ballPos, Color.White);
            spriteBatch.Draw(bar, barPos, Color.White);
            spriteBatch.DrawString(font, "Score:" + score, new Vector2(10, 10), Color.White);//score
            for (int i = 0; i < blocksPos.Length; i++)
            {
                if (blocksPos[i] != Vector2.Zero)
                {
                    spriteBatch.Draw(block, blocksPos[i], Color.White);
                    //spriteBatch.Draw(block2, blocksPos[i+1], Color.White);
                }
            }
            if (hits == blocksPos.Length)
            {
                spriteBatch.DrawString(font2, "Game Over", new Vector2(300, 200), Color.White);
                ballVel = Vector2.Zero;
                barVel = Vector2.Zero;
                soundEngineInstance.Stop();
            }
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
