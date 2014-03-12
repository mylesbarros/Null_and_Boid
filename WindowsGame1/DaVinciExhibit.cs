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
using DotNET = System.Windows;

using Microsoft.Kinect;
using kinectToolkit = Microsoft.Kinect.Toolkit.Controls;
using System.Collections.ObjectModel;
using Microsoft.Kinect.Toolkit.Interaction;
using System.Diagnostics;

using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.IO;
using System.Threading;
using System.Text;

namespace WindowsGame1
{

    public class DaVinciExhibit : Microsoft.Xna.Framework.Game
    {  

        public const int gameWidth = 1024;
        public const int gameHeight = 768;

        public const int dataWidth = 640;
        public  const int dataHeight = 480;

        public static double xScale = gameWidth / (double)(dataWidth);
        public static double yScale = gameHeight / (double)(dataHeight);

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Texture2D rightHandSprite;
        Texture2D leftHandSprite;
        Texture2D tutorialButtonSprite;

        SpriteFont font1;

        Boolean tutorialModeOn;
        Tutorial currentTutorial;

        BasicEffect basicEffect;
        VertexPositionColor[] verticies;
        VertexBuffer vertexBuffer;

        Matrix world = Matrix.CreateTranslation(0, 0, 0);
        Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 1), Vector3.Zero, Vector3.Up);
        //Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), (float)gameWidth / (float)gameHeight, .01f, 100f);
        Matrix projection = Matrix.CreateOrthographicOffCenter(0, (float)gameWidth, (float)gameHeight, 0, 1f, 1000f);

        Button tutorialButton;

        Texture2D kinectRGBVideo; 

        KinectSensor kinectSensor;

        Flock flock;
        
        // Used to keep track of boid spawning.
        Stopwatch stopwatch;

        KinectAudioSource kinectAudioSource;
        SpeechRecognitionEngine speechEngine;
        Stream stream;

        KinectController kinectController;

        // Minimum time that must have passed since the last agent was spawned
        // before we allow another to be spawned by the same user/hand.

        public DaVinciExhibit()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = gameWidth;
            graphics.PreferredBackBufferHeight = gameHeight;

            this.graphics.IsFullScreen = true;

            graphics.ApplyChanges();

            stopwatch = new Stopwatch();

            flock = new Flock(50, dataWidth, dataHeight, 50);
            tutorialModeOn = true;
            currentTutorial = new GuideTutorial(this);

            DiscoverKinectSensor();

            kinectController = new KinectController(kinectSensor, stopwatch, flock, kinectRGBVideo, graphics);
        }

        void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            if (this.kinectSensor == e.Sensor)
            {
                // If the user tripped over one of the cables...
                if (e.Status == KinectStatus.Disconnected ||
                    e.Status == KinectStatus.NotPowered)
                {
                    this.kinectSensor = null;
                    this.DiscoverKinectSensor();
                }
            }
        }

        private bool InitializeKinect()
        {
            return kinectController.InitializeKinect();
        }

        private void SpeechEngineSpeechRecognized(Object sender, SpeechRecognizedEventArgs args)
        {
            if (args.Result.Confidence > 0.82 && args.Result.Text == "clear")
            {
                flock.ClearAgents();
            }
        }

        private void SpeechEngineHypothesized(Object sender, SpeechHypothesizedEventArgs args)
        {
            // We don't care.
        }

        private void SpeechEngineRejected(Object sender, SpeechRecognitionRejectedEventArgs args)
        {
            // We don't care.
        }

        private static void GetKinectRecognizer()
        {
            foreach (RecognizerInfo ri in SpeechRecognitionEngine.InstalledRecognizers())
            {
                Debug.WriteLine(String.Format("Id={0}, Name={1}, Description={2}, Culture={3}", ri.Id, ri.Name, ri.Description, ri.Culture));
                foreach (string key in ri.AdditionalInfo.Keys)
                {
                    Debug.WriteLine(string.Format("{0} = {1}", key, ri.AdditionalInfo[key]));
                }
                Debug.WriteLine("-");
            }
        }

        private void DiscoverKinectSensor()
        {
            foreach (KinectSensor sensor in KinectSensor.KinectSensors)
            {
                if (sensor.Status == KinectStatus.Connected)
                {
                    // Found one, set our sensor to this
                    kinectSensor = sensor;
                    break; // We absolutely refuse to support multiple sensors.
                }
            }

            if (this.kinectSensor == null)
            {
                return;
            }

            // Can be modified to give discrete failure messages to the user.
            // Or surprise them with a little puzzle, like we do now, as it's more fun that way.
            switch (kinectSensor.Status)
            {
                case KinectStatus.Connected:
                    {
                        break;
                    }
                case KinectStatus.Disconnected:
                    {
                        break;
                    }
                case KinectStatus.NotPowered:
                    {
                        break;
                    }
                default:
                    {
                        break;
                    }
            }


        }

        protected override void Initialize()
        {
            KinectSensor.KinectSensors.StatusChanged += new EventHandler<StatusChangedEventArgs>(KinectSensors_StatusChanged);
            DiscoverKinectSensor();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Content.RootDirectory = "Content1";

            font1 = Content.Load<SpriteFont>("ArialFont");

            rightHandSprite = Content.Load<Texture2D>("rHand");
            leftHandSprite = Content.Load<Texture2D>("lHand");

            tutorialButtonSprite = Content.Load<Texture2D>("tutorialImage");

            DotNET.Point tutorialButtonLocation = new DotNET.Point(gameWidth - tutorialButtonSprite.Width, 0 + tutorialButtonSprite.Height);
            tutorialButton = new Button(tutorialButtonSprite, tutorialButtonLocation, 2400);

            tutorialButton.ButtonTriggered += button_ButtonTriggered;

            kinectRGBVideo = new Texture2D(GraphicsDevice, 640, 480);

            basicEffect = new BasicEffect(GraphicsDevice);

            kinectController.addButton(tutorialButton);

            //verticies = new VertexPositionColor[3];

            //verticies[0].Position = new Vector3(-.5f, -.5f, 0f);
            //verticies[0].Color = Color.Red;
            //verticies[1].Position = new Vector3(0f, .5f, 0f);
            //verticies[1].Color = Color.Green;
            //verticies[2].Position = new Vector3(.5f, -.5f, 0f);
            //verticies[2].Color = Color.Yellow;

            //vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);
            //vertexBuffer.SetData<VertexPositionColor>(verticies);
        }

        protected override void UnloadContent()
        {
            kinectSensor.Stop();
            kinectSensor.Dispose();
        }

        public void setTutorialState(Tutorial tutorial)
        {
            this.currentTutorial = tutorial;
        }

        public void button_ButtonTriggered(object sender, System.EventArgs e)
        {
            if (tutorialModeOn == false)
            {
                currentTutorial.start();
                tutorialModeOn = true;
            }
            else
            {
                currentTutorial.stop();
                tutorialModeOn = false;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            List<Flockable> passiveAgents = new List<Flockable>();
            Hand[] userHands = kinectController.getHands();

            for (int i = 0; i < userHands.Count(); i++)
            {
                if (userHands[i] != null && userHands[i].IsActive() == true)
                {
                    passiveAgents.Add(userHands[i]);
                }
            }

            if (tutorialButton != null)
            {
                tutorialButton.Update();
            }

            if (tutorialModeOn == true)
            {
                currentTutorial.update(gameTime.ElapsedGameTime.TotalMilliseconds);
            }

            base.Update(gameTime);
            flock.updateAgents(gameTime.ElapsedGameTime.TotalSeconds, passiveAgents);
        }

        protected override void Draw(GameTime gameTime)
        {
            //DrawSprite(gameTime);

            DrawPrimitive(gameTime);
        }

        public void DrawPrimitive(GameTime gameTime)
        {
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;

            MakeTriangles();

            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, flock.numAgents() + 2);
            }

            spriteBatch.Begin();

            //DrawHandCircles();
            DrawHandSprite();
            DrawText();

            if (tutorialButton != null)
            {
                DrawButtonSprite(tutorialButton);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawText()
        {
            StringBuilder builder = new StringBuilder();

            //Hand[] hands = kinectController.getHands();

            //for (int i = 0; i < hands.Count(); i++)
            //{
            //    if (hands[i] != null && hands[i].IsActive() == true)
            //    {
            //        builder.Clear();
            //        builder.Append("Hand at: (");
            //        builder.Append(hands[i].getLocation().X * xScale);
            //        builder.Append(", ");
            //        builder.Append(hands[i].getLocation().Y * yScale);
            //        builder.Append(")");
            //        spriteBatch.DrawString(font1, builder.ToString(), new Vector2(10, 10 + (20 * i)), Color.Black);
            //    }
            //}

            if (tutorialModeOn)
            {
                //String tutorialText = "This is a Tutorial for What the Flock?";
                //String tutorialText2 = "to exit the tutorial, hold your hand over the information icon";

                //spriteBatch.DrawString(font1, tutorialText, new Vector2(10, 10), Color.Black);
                //spriteBatch.DrawString(font1, tutorialText2, new Vector2(10, 30), Color.Black);

                spriteBatch.DrawString(font1, currentTutorial.getDrawText(), new Vector2(10, 10), Color.Black);
            }

        }

        public void MakeTriangles()
        {
            int n = flock.numAgents();
            int i = 6;
            verticies = new VertexPositionColor[(n * 3) + 6];
            float agentX, agentY;
            float tx1 = 10;
            float ty1 = 0;
            float tx2 = -6;
            float ty2 = -6;
            float tx3 = -6;
            float ty3 = 6;
            float theta;

            verticies[0] = new VertexPositionColor(new Vector3(0f, 0f, 0f), Color.Gray);
            verticies[1] = new VertexPositionColor(new Vector3(0f, (float) gameHeight, 0f), Color.White);
            verticies[2] = new VertexPositionColor(new Vector3((float) gameWidth, 0f, 0f), Color.Gray);
            verticies[3] = new VertexPositionColor(new Vector3(0f, (float)gameHeight, 0f), Color.White);
            verticies[4] = new VertexPositionColor(new Vector3((float) gameWidth, 0f, 0f), Color.Gray);
            verticies[5] = new VertexPositionColor(new Vector3((float)gameWidth, (float)gameHeight, 0f), Color.White);

            foreach (Agent agent in flock.GetAgents())
            {
                agentX = (float)agent.getLocation().X;
                agentY = (float)agent.getLocation().Y;
                theta = (float)Math.Atan2((float)(agent.getHeading().Y), ((float)agent.getHeading().X));

                verticies[i] = new VertexPositionColor(
                    new Vector3((float)((tx1 * Math.Cos(theta)) - (ty1 * Math.Sin(theta)) + agent.getLocation().X * xScale), 
                        (float)((ty1 * Math.Cos(theta)) + (tx1 * Math.Sin(theta)) + agent.getLocation().Y * yScale), -10f), agent.color);

                verticies[i + 1] = new VertexPositionColor(
                    new Vector3((float)((tx2 * Math.Cos(theta)) - (ty2 * Math.Sin(theta)) + agent.getLocation().X * xScale),
                        (float)((ty2 * Math.Cos(theta)) + (tx2 * Math.Sin(theta)) + agent.getLocation().Y * yScale), -10f), agent.color);

                verticies[i + 2] = new VertexPositionColor(
                    new Vector3((float)((tx3 * Math.Cos(theta)) - (ty3 * Math.Sin(theta)) + agent.getLocation().X * xScale),
                        (float)((ty3 * Math.Cos(theta)) + (tx3 * Math.Sin(theta)) + agent.getLocation().Y * yScale), -10f), agent.color);

                i += 3;
            }

            vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), verticies.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPositionColor>(verticies);
        }

        private const float HAND_SCALE_DIVISOR = 18f;
        public void DrawHandSprite()
        {
            DepthImagePoint rightHandDepthPoint, leftHandDepthPoint;
            Person[] users = kinectController.getUsers();

            // For each of the users...
            for (int i = 0; i < users.Length; i++)
            {
                // collect the position of their hand locations
                rightHandDepthPoint = users[i].getRightHandLocation();
                leftHandDepthPoint = users[i].getLeftHandLocation();

                // Define the location of the user's hands using vectors based on the location of their hands and accounting for the size of image
                Vector2 vR = new Vector2((float)((rightHandDepthPoint.X - (rightHandSprite.Width / 2)) * xScale), (float)((rightHandDepthPoint.Y - (rightHandSprite.Height - 2)) * yScale));
                Vector2 vL = new Vector2((float)((leftHandDepthPoint.X - (leftHandSprite.Width / 2)) * xScale), (float)((leftHandDepthPoint.Y - (leftHandSprite.Height / 2)) * yScale));


                // Draw the sprites over the users hand locations
                spriteBatch.Draw(rightHandSprite, vR, null, Color.White, 0f, Vector2.Zero, users[i].rightHand.getRadius() / HAND_SCALE_DIVISOR, SpriteEffects.None, 0f);
                spriteBatch.Draw(leftHandSprite, vL, null, Color.White, 0f, Vector2.Zero, users[i].leftHand.getRadius() / HAND_SCALE_DIVISOR, SpriteEffects.None, 0f);
            }

        }

        private void DrawButtonSprite(Button b)
        {
            Texture2D buttonSprite = b.getSprite();
            DotNET.Point buttonLocation = b.getLocation();

            Vector2 buttonVector = new Vector2((float) (buttonLocation.X - (buttonSprite.Width / 2)), (float)(buttonLocation.Y - (buttonSprite.Height / 2)));

            spriteBatch.Draw(buttonSprite, buttonVector, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        public void DrawSprite(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();

            // Draw the raw RGB video feed on a 640x480 blank background.
            //spriteBatch.Draw(kinectRGBVideo, new Rectangle(0, 0, 640, 480), Color.White);

            //DrawHandCircles();
            DrawHandSprite();
            
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
