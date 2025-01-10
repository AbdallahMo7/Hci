using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using WindowsFormsApp1.Components;
using WindowsFormsApp1.Screens;

namespace WindowsFormsApp1
{
    internal class MainAppUI
    {
        // Dimensions
        private int Width, Height;
        
        // Screen and State Management
        private int currentScreen;
        private int timeCounter;
        private bool personDetected;
        private bool isIntersecting;
        private int gestureCounter = 0, maxGestureCounter = 2;
        private int gestureCounter2 = 0, maxGestureCounter2 = 2; // for server message
        private bool emotionAnalysisActivated = false;
        string Emotion = "";

        // Idle Handling
        private int maxIdleWaitTime = 500;

        // YOLO Mobile Detection
        bool ActivateYolo = false,  YOLOActivated = false, StartYOLOTimeOut = false;
        string YOLOMessage = "";
        int YOLOMessageTimeOUT = 20, YOLOMessageTimeOutCounter = 0;

        // AirMouse
        private readonly AirMouse airMouse;

        // Socket Client
        public SocketClient socketClient, YOLOSocketClient;
        private string serverMessage = "";
        public string YOLOServerMessage = "";
        private readonly object lockObject = new object(); // Lock object for thread safety

        // Screens
        public WelcomeScreen welcomeScreen;
        private readonly IdleScreen idleScreen;
        private readonly ContentLoaderScreen contentLoaderScreen;
        public readonly PersonnalRecommendationsScreen personnalRecommendationsScreen;
        VideoPlayer VideoPlayer;

        // Threads
        private Thread airMouseUpdateThread;
        private bool airMouseThreadRunning;

        private Thread proximityThread;

        // Handlers
        public MarkerHandler markerHandler;

        // Constructor
        public MainAppUI(int width, int height, VideoPlayer videoPlayer)
        {
            welcomeScreen = new WelcomeScreen(width, height);
            idleScreen = new IdleScreen(width, height);
            contentLoaderScreen = new ContentLoaderScreen(width, height);
            personnalRecommendationsScreen = new PersonnalRecommendationsScreen(width, height, socketClient);
            contentLoaderScreen.exibitGalleries.videoPlayer = videoPlayer;
            this.VideoPlayer = videoPlayer;
            airMouse = new AirMouse(width, height);
            SetDimensions(width, height);

        }

        ~MainAppUI()
        {
            StopAirMouseUpdateThread();
            
        }

        public void SetDimensions(int width, int height)
        {
            Width = width;
            Height = height;

            welcomeScreen?.SetDimensions(width, height);
            idleScreen?.SetDimensions(width, height);
            contentLoaderScreen?.SetDimensions(width, height);
            personnalRecommendationsScreen?.SetDimensions(width, height);
            airMouse?.SetDimensions(width, height);
        }

        public void Draw(Graphics g)
        {
            switch (currentScreen)
            {
                case 0:
                    idleScreen?.Draw(g);
                    break;
                case 1:
                    welcomeScreen?.Draw(g);
                    break;
                case 2:
                    personnalRecommendationsScreen?.Draw(g);
                    break;
                case 3:
                    contentLoaderScreen?.Draw(g);
                    g.DrawString($"You are {Emotion}", new Font("Arial", 12), Brushes.Black, Width - 150, 10);

                    break;
                default:
                    break;
            }

            airMouse.Draw(g);
            if (YOLOActivated) 
            {
                SizeF messagesize = g.MeasureString(YOLOMessage, new Font("Arial", 28));
                g.DrawString(YOLOMessage, new Font("Arial", 28), Brushes.Red, Width/2 - messagesize.Width / 2, 10);
            }
        }

        // Screen handlers /////////////////////////////////////////////////////////////////
        private void ActvYOLO()
        {
            YOLOSocketClient.SendMessage("$YOLODetection$");
            YOLOActivated = true;
        }
       
        private void ProcessYOLOMessage()
        {
            lock (lockObject)
            {
                if (this.YOLOServerMessage.StartsWith("$YOLODetection$"))
                {
                    string[] data = this.YOLOServerMessage.Split('$');
                    if (data.Length == 4) 
                    {
                        if (data[2]== "HoldingPhone")
                        {
                            YOLOMessage = "Put the Phone Away to Enjoy the Experience";
                        }
                    }
                    SetYOLOServerMessage("");
                }
                else
                {
                    YOLOMessage = "";
                }
            }
        }
        public void ScreenTransitionHandler() // Also a timer
        {
            if (currentScreen == 0) 
            {
                if (YOLOActivated)
                {
                    ActivateYolo = false;
                }
            }
            else
            {
                if (!YOLOActivated) 
                {
                    if(StartYOLOTimeOut)
                    {
                        if(YOLOMessageTimeOutCounter >= YOLOMessageTimeOUT)
                            ActivateYolo = true;
                        else
                            YOLOMessageTimeOutCounter++;
                    }
                    else 
                    {
                        StartYOLOTimeOut = true;
                        YOLOMessageTimeOutCounter = 0;
                    }
                }
            }
            if (ActivateYolo) 
            {
                ActivateYolo = false;
                ActvYOLO();
            }
            if (YOLOActivated)
            {
                ProcessYOLOMessage();
            }
            if (timeCounter % 100 == 0)
            {
                if (welcomeScreen != null) 
                {
                    if(welcomeScreen.sessionTracker != null)
                    {
                        welcomeScreen.sessionTracker.SaveUserData();
                    }
                }
            }
            if (currentScreen != 0) // Not idle screen
            {
                CheckInactivity();
            }
            if (contentLoaderScreen != null && welcomeScreen != null) 
            {
                if (contentLoaderScreen.changed && welcomeScreen.sessionTracker != null)
                {
                    contentLoaderScreen.changed = false;
                    bool found = false;
                    for (int i = 0; i < welcomeScreen.sessionTracker.VisitedExibits.Count; i++)
                    {
                        string exbit = welcomeScreen.sessionTracker.VisitedExibits[i];
                        if (exbit == contentLoaderScreen.currentGallerieTitle)
                        {
                            welcomeScreen.sessionTracker.currentExibitIndex = i;
                            found = true; break;
                        }
                    }
                    if (!found)
                    {
                        welcomeScreen.sessionTracker.VisitedExibits.Add(contentLoaderScreen.currentGallerieTitle);
                        welcomeScreen.sessionTracker.VisitedExibitsTime.Add(1);
                        welcomeScreen.sessionTracker.currentExibitIndex = welcomeScreen.sessionTracker.VisitedExibitsTime.Count - 1;
                    }
                }
                if(welcomeScreen.sessionTracker != null)
                {
                    if(welcomeScreen.sessionTracker.currentExibitIndex >=0)
                        welcomeScreen.sessionTracker.VisitedExibitsTime[welcomeScreen.sessionTracker.currentExibitIndex]++;
                }
            }

            switch (currentScreen)
            {
                case 0:
                    IdleScreenHandler();
                    break;
                case 1:
                    WelcomeScreenHandler();
                    break;
                case 2:
                    PersonalRecommendationsScreenHandler();
                    break;
                case 3:
                    ExhibitContentScreenHandler();
                    break;
            }

            timeCounter = (timeCounter + 1) % 9999999; // Prevent overflow
        }

        private void IdleScreenHandler()
        {
            StopAirMouseUpdateThread();
            if (timeCounter % 5 == 0)
            {
                StartProximityDetectionThread();
            }

            if (personDetected)
            {
                TransitionToWelcomeScreen();
            }
        }

        private void WelcomeScreenHandler()
        {
            if(!airMouseThreadRunning && welcomeScreen.loggedIn)
            {
                socketClient?.SendMessage("$AirMouse$");
                StartAirMouseUpdateThread();
            }
            if (!welcomeScreen.loggedIn && !welcomeScreen.registering)
            {
                welcomeScreen.Start();
                lock (lockObject)
                {
                    if (this.serverMessage.StartsWith("$FaceIdentification$Login$"))
                    {
                        welcomeScreen.HandleLogin(serverMessage);
                    }
                    else
                    {
                        welcomeScreen.loggingin = false;
                    }
                    this.serverMessage = "";
                }
            }
            else if (welcomeScreen.registering)
            {
                lock (lockObject)
                {
                    if (this.serverMessage.StartsWith("$FaceIdentification$Register$"))
                    {
                        welcomeScreen.HandleRegester(serverMessage);
                    }
                }
            }
            if (welcomeScreen.loggedIn)
            {
                if (airMouse.IsIntersecting() && !isIntersecting)
                {
                    isIntersecting = true;
                    welcomeScreen?.optionsPannel.Hit(new Point(airMouse.Xp, airMouse.Yp), airMouse.radius);
                    if (welcomeScreen?.optionsPannel.selectedOption == true)
                    {
                        HandleWelcomeScreenSelection();
                    }
                }
                else if (!airMouse.IsIntersecting())
                {
                    isIntersecting = false;
                }

                if (markerHandler != null)
                {
                    if (!markerHandler.started)
                    {
                        markerHandler.StartMarkerRecognition();
                    }
                    else if (markerHandler.started)
                    {
                        lock (lockObject)
                        {
                            if (!string.IsNullOrEmpty(serverMessage) && serverMessage.StartsWith("$MarkerRecognition$"))
                            {
                                Dictionary<string, object> markerData = markerHandler.ProcessMarkerData(serverMessage);
                                if (markerData.TryGetValue("Object ID", out object objectId))
                                {
                                    Console.WriteLine($"Object ID: {objectId}");
                                }

                                if (markerData.TryGetValue("Position", out object position))
                                {
                                    Console.WriteLine($"Position: {position}");
                                }

                                if (markerData.TryGetValue("Orientation", out object orientation))
                                {
                                    Console.WriteLine($"Orientation: {orientation}");
                                }

                                if (objectId is double id && orientation is double orient)
                                {
                                    if (id >= 0 && id <= 3)
                                    {
                                        welcomeScreen.optionsPannel.selectedIndex = (int)id;
                                        welcomeScreen.optionsPannel.selectedOption = true;
                                        HandleWelcomeScreenSelection();
                                        markerHandler.StopMarkerRecognition();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ExhibitContentScreenHandler()
        {
            if(!emotionAnalysisActivated)
            {
                socketClient.SendMessage("$EmotionAnalysis$");
                emotionAnalysisActivated = true;
            }
            if (emotionAnalysisActivated) 
            {
                lock (lockObject)
                {
                    if (!string.IsNullOrEmpty(serverMessage) && serverMessage.StartsWith("$EmotionAnalysis$"))
                    {
                        string[] data = serverMessage.Split('$');
                        if (data.Length > 3)
                        {
                            Emotion = data[2];
                        }
                    }
                }
            }
            if (airMouse.IsIntersecting() && !isIntersecting)
            {
                isIntersecting = true;
                if (contentLoaderScreen?.OptionsPannelActivated == true)
                {
                    contentLoaderScreen?.optionsPannel.Hit(new Point(airMouse.Xp, airMouse.Yp), airMouse.radius);
                }
                HandleContentScreenSelection();
            }
            else if (!airMouse.IsIntersecting())
            {
                isIntersecting = false;
            }
        }
        private void PersonalRecommendationsScreenHandler()
        {
            if (airMouse.IsIntersecting() && !isIntersecting)
            {
                isIntersecting = true;
                personnalRecommendationsScreen?.optionsPannel.Hit(new Point(airMouse.Xp, airMouse.Yp), airMouse.radius);
                if (personnalRecommendationsScreen?.optionsPannel.selectedOption == true)
                {
                    HandlePersonalRecommendationsScreenSelection();
                }
                if (personnalRecommendationsScreen.profile || personnalRecommendationsScreen.recomm)
                {
                    if (personnalRecommendationsScreen.IsCircleIntersectingRectangle(new Point(airMouse.Xp, airMouse.Yp), airMouse.radius))
                    {
                        personnalRecommendationsScreen.recomm = false;
                        personnalRecommendationsScreen.profile = false;
                    }
                }
            }
            else if (!airMouse.IsIntersecting())
            {
                isIntersecting = false;
            }

            if(personnalRecommendationsScreen.recomm)
            {
                if (personnalRecommendationsScreen.activateGestureRecognition)
                {
                    socketClient.SendMessage("$AirMouseStop$");
                    personnalRecommendationsScreen.activateGestureRecognition = false;
                    socketClient.SendMessage("$GestureRecognition$");
                }
                HandleGestureServerMessage();
            }
        }
        // Screen handlers End /////////////////////////////////////////////////////////////////

        // Components handlers ////////////////////////////////////////////////////////////
        private void HandleGestureServerMessage()
        {
            lock (lockObject)
            {
                if (!string.IsNullOrEmpty(serverMessage) && serverMessage.StartsWith("$GestureRecognition$"))
                {
                    Console.WriteLine(serverMessage);
                    if (gestureCounter > maxGestureCounter)
                    {
                        string[] data = serverMessage.Split('$');
                        if (data.Length > 3)
                        {
                            socketClient.SendMessage("$AirMouse$");


                            if (data[2] == "ok")
                            {
                                string maxExbit = personnalRecommendationsScreen.maxExibit;
                                if (maxExbit != "")
                                {
                                    // "Ancient Egypt", "Space Exploration", "Modern Art"
                                    currentScreen = 3; // Exhibit Content Screen
                                    if (maxExbit == "Ancient Egypt")
                                    {
                                        contentLoaderScreen.optionsPannel.selectedIndex = 0;
                                        contentLoaderScreen.optionsPannel.selectedOption = true;
                                    }
                                    else if (maxExbit == "Space Exploration")
                                    {
                                        contentLoaderScreen.optionsPannel.selectedIndex = 1;
                                        contentLoaderScreen.optionsPannel.selectedOption = true;
                                    }
                                    else if (maxExbit == "Modern Art")
                                    {
                                        contentLoaderScreen.optionsPannel.selectedIndex = 2;
                                        contentLoaderScreen.optionsPannel.selectedOption = true;
                                    }
                                    else
                                    {
                                        currentScreen = 2; // Personal Recomm Secreen
                                    }
                                }
                                else
                                {
                                    gestureCounter = 0;
                                }
                            }
                        }
                        gestureCounter2 = 0;
                        gestureCounter = 0;
                        personnalRecommendationsScreen.recomm = false;
                        personnalRecommendationsScreen.profile = false;
                    }
                    else
                    {
                        gestureCounter++;

                    }
                }
                else
                {
                    if(gestureCounter2 < maxGestureCounter2)
                    {
                        socketClient.SendMessage("$GestureRecognition$");
                    }
                    gestureCounter2++;
                }
            }
        }
        private void HandleAirMouseUpdates()
        {
            lock (lockObject)
            {
                if (!string.IsNullOrEmpty(serverMessage) && serverMessage.StartsWith("$AirMouse$"))
                {
                    airMouse.ProcessServerMessage(serverMessage, timeCounter);
                    serverMessage = string.Empty;
                }
            }
        }

        private void CheckInactivity()
        {
            if (timeCounter - airMouse.updateTime > maxIdleWaitTime)
            {
                StartProximityDetectionThread();

                if (!personDetected)
                {
                    if (welcomeScreen.sessionTracker != null)
                    {
                        welcomeScreen.sessionTracker.SaveUserData();
                        welcomeScreen.END();
                    }
                    
                    TransitionToIdleScreen();
                }
            }
        }

        private void StartProximityDetectionThread()
        {
            if (proximityThread == null || !proximityThread.IsAlive)
            {
                proximityThread = new Thread(DetectProximity);
                proximityThread.Start();
            }
        }

        private void DetectProximity()
        {
            if (socketClient == null) return;

            socketClient.SendMessage("$ProximityDetector$");
            
            lock (lockObject)
            {
                personDetected = serverMessage == "$ProximityDetector$Ok$";
            }
        }

        public void SetServerMessage(string serverMessage)
        {
            lock (lockObject)
            {
                this.serverMessage = serverMessage;
            }
        }
        public void SetYOLOServerMessage(string serverMessage)
        {
            lock (lockObject)
            {
                this.YOLOServerMessage = serverMessage;
            }
        }

        private void TransitionToIdleScreen()
        {
            currentScreen = 0;
            timeCounter = 0;
        }

        private void TransitionToWelcomeScreen()
        {
            currentScreen = 1;
            personDetected = false;
        }

        private void HandleWelcomeScreenSelection()
        {
            if (welcomeScreen?.optionsPannel.selectedIndex == 0)
            {
                currentScreen = 3; // Exhibit Content Screen
                
            }
            else if (welcomeScreen?.optionsPannel.selectedIndex == 1)
            {
                currentScreen = 2; // Personal Recomm Secreen
            }
            else if (welcomeScreen?.optionsPannel.selectedIndex == welcomeScreen.optionsPannel.Options.Count - 1)
            {
                if(welcomeScreen.sessionTracker!=null)
                {
                    welcomeScreen.sessionTracker.currentExibitIndex = -1;
                    welcomeScreen.sessionTracker.SaveUserData();
                    welcomeScreen.END();
                }

                TransitionToIdleScreen();
            }

            welcomeScreen.optionsPannel.selectedOption = false;
        }

        private void HandleContentScreenSelection()
        {
            // Choose exhibit
            if (contentLoaderScreen?.optionsPannel.selectedIndex == contentLoaderScreen.optionsPannel.Options.Count - 1 && contentLoaderScreen?.optionsPannel.selectedOption == true)
            {
                currentScreen = 1; // Welcome Screen
                contentLoaderScreen.VideoImageTextOptionsActivated = false;
                contentLoaderScreen.optionsPannel.selectedOption = false;
                welcomeScreen.sessionTracker.currentExibitIndex = -1;
            }
            else
            {
                contentLoaderScreen.HandleMenuSelection(new Point(airMouse.Xp, airMouse.Yp), airMouse.radius);
            }
        }

        private void HandlePersonalRecommendationsScreenSelection()
        {
            if (personnalRecommendationsScreen?.optionsPannel.selectedIndex == personnalRecommendationsScreen.optionsPannel.Options.Count - 1 && personnalRecommendationsScreen?.optionsPannel.selectedOption == true)
            {
                currentScreen = 1; // Welcome Screen
                personnalRecommendationsScreen.optionsPannel.selectedOption = false;
                personnalRecommendationsScreen.profile = false;
                personnalRecommendationsScreen.recomm = false;
            }
            else
            {
                if(welcomeScreen.sessionTracker != null)
                    personnalRecommendationsScreen.HandleMenuSelection(welcomeScreen.sessionTracker);
            }
            
        }
        // Thread management //////////////////////////////////////////////////////////////

        public void StartAirMouseUpdateThread()
        {
            airMouseThreadRunning = true;
            airMouseUpdateThread = new Thread(() =>
            {
                while (airMouseThreadRunning)
                {
                    HandleAirMouseUpdates();
                    Thread.Sleep(50); // Prevent excessive CPU usage
                }
            })
            {
                IsBackground = true // Ensure thread doesn't block application shutdown
            };
            airMouseUpdateThread.Start();
        }

        public void StopAirMouseUpdateThread()
        {
            airMouseThreadRunning = false;
            airMouseUpdateThread?.Join(); // Wait for thread to finish
            //socketClient.SendMessage("")
        }
    }
}