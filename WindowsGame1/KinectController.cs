using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Diagnostics;
using DotNET = System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowsGame1
{
    class KinectController
    {

        // Gesture detection objects. Not unified by an interface because I hate myself.
        WaveGestureDetector waveGestureDetector;
        PressGestureDetector pressGestureDetector;
        ScissorGestureDetector scissorGestureDetector;

        // List of persons in front of the Kinect
        Person[] users;

        Stopwatch stopwatch;
        KinectSensor kinectSensor;

        //THIS IS NOT STAYING
        Flock flock;
        Texture2D kinectRGBVideo;
        GraphicsDeviceManager graphics;

        //THESE NEED TO BE CLEANED UP
        DepthImagePixel[] depthData;
        ColorImageFrame mostRecentFrame;
        Color[] mostRecentColorMap;

        Button tutorialButton;

        private Person ghostUser;

        private const int MAX_NUM_USERS = 6;


        public KinectController(KinectSensor kinectSensor, Stopwatch stopwatch, Flock flock, Texture2D kinectRGBVideo, GraphicsDeviceManager graphics)
        {
            this.stopwatch = stopwatch;
            this.kinectSensor = kinectSensor;
            this.flock = flock;
            this.kinectRGBVideo = kinectRGBVideo;
            this.graphics = graphics;

            users = new Person[MAX_NUM_USERS];

            //waveGestureDetector = new WaveGestureDetector(kinectSensor);
            // Subscribe to the wave gesture detector as an observer.
            //waveGestureDetector.GestureDetected += new System.EventHandler<WaveGestureEventArgs>(this.WaveGestureDetected);
            pressGestureDetector = new PressGestureDetector();
            // Subscribe to the press gesture detector as an observer.
            pressGestureDetector.GestureDetected += new System.EventHandler<PressGestureEventArgs>(this.PressGestureDetected);
            // Subscribe to the scissor gesture detector as an observer
            scissorGestureDetector = new ScissorGestureDetector();
            scissorGestureDetector.GestureDetected += new System.EventHandler<ScissorGestureEventArgs>(this.ScissorGestureDetected);

            // Initialize the found and connected device.
            if (kinectSensor.Status == KinectStatus.Connected)
            {
                InitializeKinect();
            }
        }

        // Method called when notification received from ScissorGestureDetector object
        private void ScissorGestureDetected(Object sender, ScissorGestureEventArgs eventArgs)
        {
            Person gesturer = eventArgs.getUser();

            if (eventArgs.gestureIsOver() == true)
            {
                gesturer.canSpawnBoids = true;
                return;
            }

            Person killer = gesturer;

            DotNET.Point leftHandPos = killer.leftHand.getLocation();
            DotNET.Point rightHandPos = killer.rightHand.getLocation();

            // Produce new location from average of the left and right hands
            double deletionX = (leftHandPos.X + rightHandPos.X) / 2;
            double deletionY = (leftHandPos.Y + leftHandPos.Y) / 2;
            DotNET.Point deletionZoneCenter = new DotNET.Point(deletionX, deletionY);
            double deletionZoneRadius = ((killer.leftHand.getRadius() + killer.rightHand.getRadius()) / 2);
            deletionZoneRadius = deletionZoneRadius * eventArgs.getRadiusScale();
            double deletionZoneRadiusSq = Math.Pow(deletionZoneRadius, 2);

            double distanceSq;

            foreach (Agent agent in flock.GetAgents())
            {
                distanceSq = (Math.Pow((deletionZoneCenter.X - agent.getLocation().X), 2) + Math.Pow((deletionZoneCenter.Y - agent.getLocation().Y), 2));

                if (distanceSq <= deletionZoneRadiusSq)
                {
                    agent.KillAgent();
                }
            }

            gesturer.canSpawnBoids = false;
        }

        // Method called only when notification received from WaveGestureDetector object
        private void WaveGestureDetected(Object sender, WaveGestureEventArgs e)
        {
            stopwatch.Stop();

            int userId = e.getUserID();

            DepthImagePoint leftHandLocation = users[userId].leftHandLocation;
            DepthImagePoint rightHandLocation = users[userId].rightHandLocation;

            int leftHandRadius = users[userId].leftHand.getRadius() + 1;
            int rightHandRadius = users[userId].rightHand.getRadius() +1;
            double leftHandRadiusSq = Math.Pow(leftHandRadius, 2);
            double rightHandRadiusSq = Math.Pow(rightHandRadius, 2);
            double leftDistanceSq;
            double rightDistanceSq;

            foreach (Agent agent in flock.GetAgents())
            {
                // Distance formula
                leftDistanceSq = (Math.Pow((leftHandLocation.X - agent.getLocation().X), 2) + Math.Pow((leftHandLocation.Y - agent.getLocation().Y), 2));
                rightDistanceSq = (Math.Pow((rightHandLocation.X - agent.getLocation().X), 2) + Math.Pow((rightHandLocation.Y - agent.getLocation().Y), 2));

                if (leftDistanceSq <= rightHandRadiusSq)
                {
                    agent.KillAgent();
                }
                else if (rightDistanceSq <= rightHandRadiusSq)
                {
                    agent.KillAgent();
                }
            }

            if (users[userId].lastSpawnedBoidTimestampLeft <= stopwatch.ElapsedMilliseconds)
            {
                users[userId].lastSpawnedBoidTimestampLeft = stopwatch.ElapsedMilliseconds + 1500;
            }

            if (users[userId].lastSpawnedBoidTimestampRight <= stopwatch.ElapsedMilliseconds)
            {
                users[userId].lastSpawnedBoidTimestampRight = stopwatch.ElapsedMilliseconds + 1500;
            }

            stopwatch.Start();

        } // end WaveGestureDetected()

       

        // Method called only when notification received from PressGesture Detector object.
        private void PressGestureDetected(Object sender, PressGestureEventArgs args)
        {
            // Retrieve identifier for which user is spawning boids.
            Person gesturer = args.getUser();

            if (gesturer.canSpawnBoids == false)
            {
                return;
            }

            // Determine which hand is spawning boids.
            bool isLeftHand = args.getLeftHandPressing();
            // Defines where the hand spawning the boids is located;
            // used to determine where to spawn the boids.
            DepthImagePoint depthPoint;

            if (isLeftHand == true) // If they're spawning boids with the lefthand...
            {
                // If the necessary amount of time has elapsed since the user last spawned a boid with the lefthand...
                if ((stopwatch.ElapsedMilliseconds - gesturer.lastSpawnedBoidTimestampLeft) > gesturer.timeBetweenSpawn)
                {
                    depthPoint = this.kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(gesturer.leftHandPosition, this.kinectSensor.DepthStream.Format);

                    // Spawn the new boid where the user's hand is.
                    flock.AddAgent(new DotNET.Point(depthPoint.X, depthPoint.Y), 40, gesturer.color);

                    // Update when last this user spawned a boid with their left hand.
                    gesturer.lastSpawnedBoidTimestampLeft = stopwatch.ElapsedMilliseconds;
                    gesturer.ResetTimeBetweenSpawn();
                }
            }
            else // They're spawning a boid with their right hand.
            {
                // If the necessary amount of time has elapsed since the user last spawned a boid with the righthand...
                if ((stopwatch.ElapsedMilliseconds - gesturer.lastSpawnedBoidTimestampRight) > gesturer.timeBetweenSpawn)
                {
                    depthPoint = this.kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(gesturer.rightHandPosition, this.kinectSensor.DepthStream.Format);

                    // Spawn the new boid where the user's hand is.
                    flock.AddAgent(new DotNET.Point(depthPoint.X, depthPoint.Y), 40, gesturer.color);

                    // Update when last this user spawned a boid with their right hand.
                    gesturer.lastSpawnedBoidTimestampRight = stopwatch.ElapsedMilliseconds;
                    gesturer.ResetTimeBetweenSpawn();
                }
            }
        }

        private void tutorialButtonTriggered(Object sender, EventArgs e)
        {
            int a = 0;
        }

        public bool InitializeKinect()
        {
            // Color stream
            kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectSensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(kinectSensor_ColorFrameReady);

            // Depth stream
            kinectSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            kinectSensor.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(kinectSensor_DepthFrameReady);

            // Skeleton Stream
            kinectSensor.SkeletonStream.Enable(new TransformSmoothParameters()
            {
                // These are default values. Detailed documentation on their significance is scarce.
                Smoothing = 0.5f,
                Correction = 0.5f,
                Prediction = 0.5f,
                JitterRadius = 0.05f,
                MaxDeviationRadius = 0.04f
            });

            kinectSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(kinectSensor_SkeletonFrameReady);

            depthData = new DepthImagePixel[kinectSensor.DepthStream.FramePixelDataLength];

            Hand.setSensor(this.kinectSensor);

            stopwatch.Start(); // ToDo: Stopwatch must be reset every ~ 2.9 million centuries

            try
            {
                kinectSensor.Start();
            }
            catch
            {
                // How embarrasing.
                return false;
            }

            //kinectSensor.ElevationAngle = 15;

            //This API has returned an exception from an HRESULT: 0x8007000D


            //kinectAudioSource = kinectSensor.AudioSource;

            ////RecognizerInfo ri =
            // GetKinectRecognizer();
            //speechEngine = new SpeechRecognitionEngine(ri.Id);

            //stream = kinectAudioSource.Start();
            //speechEngine.SetInputToAudioStream(stream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
            //speechEngine.Recognize();

            //GrammarBuilder grammarBuilder = new GrammarBuilder { Culture = ri.Culture };
            //grammarBuilder.Append("clear");
            //Grammar grammar = new Grammar(grammarBuilder);

            //speechEngine.LoadGrammar(grammar);
            //speechEngine.SpeechRecognized += new System.EventHandler<Microsoft.Speech.Recognition.SpeechRecognizedEventArgs>(SpeechEngineSpeechRecognized);
            //speechEngine.SpeechHypothesized += new System.EventHandler<Microsoft.Speech.Recognition.SpeechHypothesizedEventArgs>(this.SpeechEngineHypothesized);
            //speechEngine.SpeechRecognitionRejected += new System.EventHandler<Microsoft.Speech.Recognition.SpeechRecognitionRejectedEventArgs>(this.SpeechEngineRejected);

            //kinectAudioSource.AutomaticGainControlEnabled = false;

            return true;
        }

        void kinectSensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
                    depthData = new DepthImagePixel[kinectSensor.DepthStream.FramePixelDataLength];
                    depthFrame.CopyDepthImagePixelDataTo(this.depthData);
                }
            }
        }

        void kinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    Skeleton[] skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];

                    int i = 0; // Defines the number of users that we have extracted data from.

                    skeletonFrame.CopySkeletonDataTo(skeletonData);

                    Person currPerson;
                    foreach (Skeleton playerSkeleton in skeletonData)
                    {
                        if (playerSkeleton != null && playerSkeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            // Wrap the Skelton
                            SkeletonWrapper skeletonWrap = new SkeletonWrapper(playerSkeleton);

                            currPerson = users[i];

                            users[i] = updatePerson(skeletonWrap, currPerson, false);

                            i++;
                        } // end if
                    } // end foreach

                    // add the ghosts

                    for (int inactiveUser = (i + 1); inactiveUser < users.Length; inactiveUser++)
                    {
                        users[inactiveUser] = null;
                    }

                    //// Deactivate all unused hands
                    //for (int k = j + 1; j < userHands.Count(); j++)
                    //{
                    //    if (userHands[k] != null)
                    //    {
                    //        userHands[k].NotActive();
                    //    }
                    //}
                } // end if
            } // end using
        } // end method

        private Person updatePerson(SkeletonWrapper skeletonWrap, Person person, bool isGhost)
        {
            // Retrieve hand data.
            Joint rightHand = skeletonWrap.getRightHandJoint();
            Joint leftHand = skeletonWrap.getLeftHandJoint();

            // Store retrieved data.
            if (person == null)
            {
                person = new Person(skeletonWrap, isGhost);
            }
            else
            {
                person.updateSkeletonData(skeletonWrap);
            }

            person.rightHandLocation = this.kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(person.rightHandPosition, this.kinectSensor.DepthStream.Format);
            person.leftHandLocation = this.kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(person.leftHandPosition, this.kinectSensor.DepthStream.Format);

            person.setRightHandRadius(person.rightHandLocation.Depth / 60);
            person.setLeftHandRadius(person.leftHandLocation.Depth / 60);

            if (tutorialButton != null)
            {
                tutorialButton.UpdateHands(users);
            }

            // Update user color.
            updateUserColor(person);

            // Update the gesture detectors of the user's physical state.
            //waveGestureDetector.Update(playerSkeleton, i);
            scissorGestureDetector.update(person);
            pressGestureDetector.Update(person);

            return person;
        }

        void kinectSensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame != null)
                {

                    byte[] pixelsFromFrame = new byte[colorImageFrame.PixelDataLength];

                    colorImageFrame.CopyPixelDataTo(pixelsFromFrame);
                    mostRecentFrame = colorImageFrame;

                    Color[] color = new Color[colorImageFrame.Height * colorImageFrame.Width];
                    kinectRGBVideo = new Texture2D(graphics.GraphicsDevice, colorImageFrame.Width, colorImageFrame.Height);

                    // Go through each pixel and set the bytes correctly
                    // Remember, each pixel gets a Red, Green and Blue
                    int index = 0;
                    for (int y = 0; y < colorImageFrame.Height; y++)
                    {
                        for (int x = 0; x < colorImageFrame.Width; x++, index += 4)
                        {
                            color[y * colorImageFrame.Width + x] = new Color(pixelsFromFrame[index + 2], pixelsFromFrame[index + 1], pixelsFromFrame[index + 0]);
                        }
                    }

                    mostRecentColorMap = color;

                    // Set pixeldata from the ColorImageFrame to a Texture2D
                    kinectRGBVideo.SetData(color);
                }
            }
        }

        public void updateGhost(SkeletonWrapper skel)
        {
            if (skel == null)
            {
                ghostUser = null;
                return;
            }

            if (ghostUser == null)
            {
                ghostUser = new Person(skel, true);
            }

            updatePerson(skel, ghostUser, true);
        }

        public Person[] getUsers()
        {
            if (users != null)
            {
                int i = 0;
                while (users[i] != null)
                {
                    i++;
                }

                int activeGhost = 0;
                if (ghostUser != null)
                {
                    activeGhost++;
                }

                Person[] activeUsers = new Person[i + activeGhost];

                for (int j = 0; j < i; j++)
                {
                    activeUsers[j] = users[j];
                }

                if (activeGhost > 0)
                {
                    activeUsers[activeUsers.Length - 1] = ghostUser;
                }

                return activeUsers;
            }

            return new Person[0];
        }

        public Hand[] getHands()
        {
            Hand[] hands;
            Person[] activeUsers = getUsers();

            if (activeUsers != null)
            {
                hands = new Hand[activeUsers.Length * 2];

                for (int i = 0; i < activeUsers.Length; i++)
                {
                    hands[2 * i] = activeUsers[i].leftHand;
                    hands[(2 * i) + 1] = activeUsers[i].rightHand;

                }

                return hands;
            }

            hands = new Hand[0];

            return hands;
        }

        public int getDepthAtPoint(Vector2 point, int imageWidth)
        {
            int pixel = (int)(point.X * imageWidth + point.Y);

            if (depthData != null)
            {
                return depthData[pixel].Depth;
            }
            return 1000;
        }

        public void addButton(Button newButton)
        {
            tutorialButton = newButton;
            tutorialButton.ButtonTriggered += new System.EventHandler<EventArgs>(this.tutorialButtonTriggered);
        }

        private void updateUserColor(Person person)
        {
            if (mostRecentFrame != null && mostRecentColorMap.Length != 0)
            {
                TorsoData curr = new TorsoData(person.torsoTop, person.torsoBottom);
                if ((curr.torsoTop.X == 0 && curr.torsoTop.Y == 0 && curr.torsoTop.Z == 0) == false)
                {
                    person.color = ColorUtility.ComputeUserColor(person, mostRecentFrame.Width, mostRecentColorMap, kinectSensor);
                }
            }
        }
    }
}
