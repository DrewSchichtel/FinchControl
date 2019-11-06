using FinchAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace FinchControl
{
    // **************************************************
    // Title: Finch Control
    // Description:
    // Author: Drew Schichtel
    // Dated Created: 9/30/2019
    // Last Modified: 11/06/2019
    // **************************************************

    class Program
    {
        public enum Command
        {
            NONE,
            MOVEFORWARD,
            MOVEBACKWARD,
            STOPMOTORS,
            WAIT,
            TURNRIGHT,
            TURNLEFT,
            LEDRED,
            LEDGREEN,
            LEDBLUE,
            LEDWHITE,
            LEDOFF,
            GETTEMP,
            GETLIGHT,
            DONE
        }

        static void Main(string[] args)
        {
            SetTheme();
            DisplayWelcomeScreen();
            DisplayMenu();
            DisplayClosingScreen();
        }

        #region Modules

        /// <summary>
        /// module: Talent Show
        /// </summary>
        static void TalentShow(Finch finchRobot)
        {
            string userResponse;
            DisplayScreenHeader("Talent Show");

            Console.WriteLine("The Finch robot will now show its talent.");
            DisplayContinuePrompt();

            for (int i = 0; i < 255; i++)
            {
                finchRobot.setLED(0, i, 0);
                finchRobot.noteOn(i * 100);
                finchRobot.setMotors(i, i);
            }

            finchRobot.setLED(0, 255, 0);
            finchRobot.setMotors(-100, -100);
            finchRobot.wait(1000);
            RobotStopAll(finchRobot);
            finchRobot.wait(1000);

            finchRobot.setLED(0, 0, 255);
            finchRobot.setMotors(100, 100);
            finchRobot.wait(1000);
            RobotStopAll(finchRobot);
            finchRobot.wait(1000);

            finchRobot.setLED(255, 50, 255);
            finchRobot.setMotors(-255, -255);
            finchRobot.wait(1000);
            RobotStopAll(finchRobot);
            finchRobot.wait(1000);



            RobotStopAll(finchRobot);
            finchRobot.wait(1000);

            finchRobot.setMotors(50, 255);
            finchRobot.wait(500);
            finchRobot.setMotors(-50, -255);
            finchRobot.wait(500);

            finchRobot.setMotors(255, 50);
            finchRobot.wait(500);
            finchRobot.setMotors(-255, -50);
            finchRobot.wait(500);
            RobotStopAll(finchRobot);

            Console.WriteLine();
            do
            {
                Console.WriteLine("Would you like to hear a song?");
                userResponse = Console.ReadLine().ToLower();
                if (userResponse == "yes")
                {
                    Console.WriteLine();
                    Console.WriteLine("That is too bad because I can't find anything on the internet that breaks songs down to simple frequencies");
                    Console.WriteLine("Also, I have no talent so I can't make a song, here is a bunch of beeps though.");
                    PlaySong(finchRobot);
                }

                else if (userResponse == "no")
                {
                    Console.WriteLine("Wow, you upset the Finch.");
                }

                else
                {
                    Console.WriteLine($"I don't understand what {userResponse} means, please answer yes or no");
                    userResponse = "retry";
                }
            } while (userResponse == "retry");

            Console.WriteLine();
            Console.WriteLine("That's all folks.");
            DisplayContinuePrompt();
        }

        /// <summary>
        /// module: User Programming
        /// </summary>
        static void UserProgramming(Finch finchRobot)
        {
            string menuChoice;
            bool quitApplication = false;
            (int motorSpeed, int ledBrightness, int waitSeconds) commandParameters;
            commandParameters.motorSpeed = 0;
            commandParameters.ledBrightness = 0;
            commandParameters.waitSeconds = 0;
            List<Command> commands = new List<Command>();

            do
            {
                DisplayScreenHeader("User Programming");

                Console.WriteLine("a) Set Command Parameters");
                Console.WriteLine("b) Add Commands");
                Console.WriteLine("c) View Commands");
                Console.WriteLine("d) Execute Commands");
                Console.WriteLine("e) Write Commands to Data File");
                Console.WriteLine("f) Read Commands From Data File");
                Console.WriteLine("q) Return to Main Menu");
                Console.Write("Enter Choice:");
                menuChoice = Console.ReadLine().ToLower();

                switch (menuChoice)
                {
                    case "a":
                        commandParameters = DisplayGetCommandParameters();
                        break;

                    case "b":
                        DisplayGetFinchCommands(commands);
                        break;

                    case "c":
                        DisplayFinchCommands(commands);
                        break;

                    case "d":
                        DisplayExecuteFinchCommands(finchRobot, commands, commandParameters);
                        break;

                    case "e":
                        DisplayWriteUserProgrammingData(commands);
                        break;

                    case "f":
                        commands = DisplayReadUserProgrammingData();
                        break;

                    case "q":
                        quitApplication = true;
                        break;
                    default:
                        Console.WriteLine();
                        Console.WriteLine("Please provide a proper menu choice.");
                        DisplayContinuePrompt();
                        break;
                }
            } while (!quitApplication);

        }

        /// <summary>
        /// module: Alarm System
        /// </summary>
        static void AlarmSystem(Finch finchRobot)
        {
            string alarmType;
            int maxSeconds;
            double upperThreshold;
            double lowerThreshold;
            bool thresholdExceeded;

            DisplayScreenHeader("Alarm System");

            alarmType = DisplayGetAlarmType();
            maxSeconds = DisplayGetMaxSeconds();
            DisplayGetThreshhold(finchRobot, alarmType, out lowerThreshold, out upperThreshold);
            Console.WriteLine();
            Console.WriteLine("Once you continue the alarm will be set");
            DisplayContinuePrompt();
            finchRobot.setLED(0, 255, 0);
            thresholdExceeded = MonitorLevels(finchRobot, lowerThreshold, upperThreshold, maxSeconds, alarmType);

            if (thresholdExceeded)
            {
                if (alarmType == "light")
                {
                    Console.WriteLine("Maximum or minimum light level exceeded");

                }
                else
                {
                    Console.WriteLine("Maximum or minimum temperature level exceeded");
                }

                finchRobot.setLED(255, 0, 0);
                for (int i = 0; i < 5; i++)
                {
                    finchRobot.noteOn(5000);
                    finchRobot.wait(500);
                    finchRobot.noteOff();
                    finchRobot.wait(500);
                }

            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Maximum time exceeded");
            }


            DisplayContinuePrompt();
            finchRobot.noteOff();
            finchRobot.setLED(0, 0, 0);
        }

        /// <summary>
        /// module: Data Recorder
        /// </summary>
        static void DataRecorder(Finch finchRobot)
        {

            double dataPointFrequency;
            int numberOfDataPoints;
            string userResponse;
            DisplayScreenHeader("Data Recorder");

            Console.WriteLine("To record temperature or light level data with the finch, you will first need to specify how often \nbetween recordings you would like in seconds, and the number of recordings.");
            Console.WriteLine("You will then be asked if you would like to record temperature or light, \nsimply type which you would like to record then press enter.");
            Console.WriteLine();
            DisplayContinuePrompt();

            DisplayScreenHeader("Data Recorder");
            dataPointFrequency = DisplayGetDataPointFrequency();
            numberOfDataPoints = DisplayGetNumberOfDataPoints();

            do
            {
                Console.WriteLine("Would you like to record temperature or light?");
                userResponse = Console.ReadLine().ToLower();
                if (userResponse == "light")
                {
                    int[] light = new int[numberOfDataPoints];

                    DisplayGetLightData(finchRobot, numberOfDataPoints, dataPointFrequency, light);

                    DisplayLightData(light);
                }
                else if (userResponse == "temperature")
                {
                    double[] temperatures = new double[numberOfDataPoints];

                    DisplayGetTempData(finchRobot, numberOfDataPoints, dataPointFrequency, temperatures);

                    DisplayTempData(temperatures);
                }
                else
                {
                    Console.WriteLine("I don't understand, please enter temperature or light when prompted.");
                    userResponse = "retry";
                    DisplayContinuePrompt();
                }

            } while (userResponse == "retry");


            DisplayContinuePrompt();
        }

        /// <summary>
        /// Display Main Menu
        /// </summary>
        static void DisplayMenu()
        {
            //
            // instantiate a Finch object
            //
            Finch finchRobot = new Finch();

            bool finchRobotConnected = false;
            bool quitApplication = false;
            string menuChoice;

            do
            {
                DisplayScreenHeader("Main Menu");

                Console.WriteLine("a) Connect Finch Robot");
                Console.WriteLine("b) Talent Show");
                Console.WriteLine("c) Data Recorder");
                Console.WriteLine("d) Alarm System");
                Console.WriteLine("e) User Programming");
                Console.WriteLine("f) Disconnect Finch Robot");
                Console.WriteLine("g) Set Theme");
                Console.WriteLine("q) Quit");
                Console.Write("Enter Choice:");
                menuChoice = Console.ReadLine().ToLower();

                switch (menuChoice)
                {
                    case "a":
                        finchRobotConnected = DisplayConnectFinchRobot(finchRobot);
                        break;
                    case "b":
                        if (finchRobotConnected) TalentShow(finchRobot);
                        else DisplayConnectionIssueInformation();
                        break;
                    case "c":
                        if (finchRobotConnected) DataRecorder(finchRobot);
                        else DisplayConnectionIssueInformation();
                        break;
                    case "d":
                        if (finchRobotConnected) AlarmSystem(finchRobot);
                        else DisplayConnectionIssueInformation();
                        break;
                    case "e":
                        if (finchRobotConnected) UserProgramming(finchRobot);
                        else DisplayConnectionIssueInformation();
                        break;
                    case "f":
                        DisplayDisconnectFinchRobot(finchRobot);
                        break;
                    case "g":
                        ThemeConfig();
                        break;
                    case "q":
                        finchRobot.disConnect();
                        quitApplication = true;
                        break;
                    default:
                        Console.WriteLine();
                        Console.WriteLine("Please provide a proper menu choice.");
                        DisplayContinuePrompt();
                        break;
                }
            } while (!quitApplication);

        }

        /// <summary>
        /// display disconnecting from the Finch robot
        /// </summary>
        static void DisplayDisconnectFinchRobot(Finch finchRobot)
        {
            DisplayScreenHeader("Disconnect the Finch Robot");

            Console.WriteLine("The Finch robot is about to be disconnected.");
            DisplayContinuePrompt();

            finchRobot.disConnect();
            Console.WriteLine();
            Console.WriteLine("The Finch robot is now disconnected.");

            DisplayContinuePrompt();
        }

        /// <summary>
        /// displayed when a module is called and the Finch robot is not connected
        /// </summary>
        static void DisplayConnectionIssueInformation()
        {
            DisplayScreenHeader("Connection Information");
            Console.WriteLine("The Finch robot is not connected. Please confirm that the USB cables are fully connected and choose \"a\" from the menu to connect the Finch robot.");
            DisplayContinuePrompt();
        }

        /// <summary>
        /// connect the Finch robot to the application
        /// </summary>
        static bool DisplayConnectFinchRobot(Finch finchRobot)
        {
            const int MAX_ATTEMPTS = 3;
            int attempts = 0;
            bool finchRobotConnected;

            DisplayScreenHeader("Connect Finch Robot");

            Console.WriteLine("\tConnecting to the Finch robot. Be sure the USB cord is plugged into both the robot and the computer.");
            Console.WriteLine();
            DisplayContinuePrompt();

            //
            // loop until the Finch robot is connected or the maximum number of attempts is exceeded
            //
            do
            {
                //
                // increment attempt counter
                //
                attempts++;

                finchRobotConnected = finchRobot.connect();

                if (!finchRobotConnected)
                {
                    Console.WriteLine();
                    Console.WriteLine("\tUnable to connect to the Finch robot. Please confirm all USB cords are plugged in.");
                    Console.WriteLine();
                    DisplayContinuePrompt();
                }
            } while (!finchRobot.connect() && attempts < MAX_ATTEMPTS);

            //
            // notify the user if the maximum attempts is exceeded
            //
            if (finchRobotConnected)
            {
                Console.WriteLine();
                Console.WriteLine("\tFinch robot is now connected.");
                Console.WriteLine();
                finchRobot.setLED(0, 255, 0); // set nose to green
                finchRobot.wait(3000);
                finchRobot.setLED(0, 0, 0);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("\tUnable to connect to the Finch robot. Please check connection and try again.");
                Console.WriteLine();
            }

            DisplayContinuePrompt();

            return finchRobotConnected;
        }

        /// <summary>
        /// display welcome screen
        /// </summary>
        static void DisplayWelcomeScreen()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("\t\tFinch Control");
            Console.WriteLine();

            DisplayContinuePrompt();
        }

        /// <summary>
        /// display closing screen
        /// </summary>
        static void DisplayClosingScreen()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("\t\tThank you for using Finch Control!");
            Console.WriteLine();

            DisplayContinuePrompt();
        }

        #endregion

        #region UserProgrammingHelpers

        /// <summary>
        /// Get command parameters from user
        /// </summary>
        static (int motorSpeed, int ledBrightness, int waitSeconds) DisplayGetCommandParameters()
        {
            bool ifNumber;
            (int motorSpeed, int ledBrightness, int waitSeconds) commandParameters;
            commandParameters.motorSpeed = 0;
            commandParameters.ledBrightness = 0;
            commandParameters.waitSeconds = 0;

            Console.Write("Enter Motor Speed[1-255]: ");

            do
            {
                ifNumber = int.TryParse(Console.ReadLine(), out commandParameters.motorSpeed);

                if (!ifNumber)
                {
                    Console.WriteLine();
                    Console.Write("Please enter a valid number: ");
                }
                else if (commandParameters.motorSpeed > 255)
                {
                    commandParameters.motorSpeed = 255;
                }
                else if (commandParameters.motorSpeed <= 0)
                {
                    Console.Write("Enter a Positive Number: ");
                    ifNumber = false;
                }
            } while (!ifNumber);

            Console.WriteLine();
            Console.Write("Enter LED Brightness[1-255]: ");

            commandParameters.ledBrightness = int.Parse(Console.ReadLine());
            do
            {
                ifNumber = int.TryParse(Console.ReadLine(), out commandParameters.ledBrightness);

                if (!ifNumber)
                {
                    Console.WriteLine();
                    Console.Write("Please enter a valid number: ");
                }
                else if (commandParameters.ledBrightness > 255)
                {
                    commandParameters.ledBrightness = 255;
                }
                else if (commandParameters.ledBrightness <= 0)
                {
                    Console.WriteLine();
                    Console.Write("Enter a Positive Number: ");
                    ifNumber = false;
                }
            } while (!ifNumber);

            Console.WriteLine("Enter length of wait command in seconds");
            Console.Write("Wait: ");

            do
            {
                ifNumber = int.TryParse(Console.ReadLine(), out commandParameters.waitSeconds);

                if (!ifNumber)
                {
                    Console.WriteLine();
                    Console.Write("Please enter a valid number:");
                }
                else if (commandParameters.waitSeconds <= 0)
                {
                    Console.WriteLine();
                    Console.Write("Enter a Positive Number:");
                    ifNumber = false;
                }
            } while (!ifNumber);

            Console.WriteLine($"Motor Speed : {commandParameters.motorSpeed}");
            Console.WriteLine($"LED Brightness : {commandParameters.ledBrightness}");
            Console.WriteLine($"Wait Time : {commandParameters.waitSeconds}");
            DisplayContinuePrompt();

            return commandParameters;
        }

        /// <summary>
        /// Get commands from user
        /// </summary>
        static void DisplayGetFinchCommands(List<Command> commands)
        {
            Command command = Command.NONE;
            string userResponse;
            bool valid;
            Console.WriteLine("From here you will be able to add commands for the Finch to execute.");
            Console.WriteLine("After you continue, enter the commands from the list one at a time. Enter Done when you have entered the commands.");
            Console.WriteLine("*The motors or lights will stay on unless overwritten or stopped.");
            DisplayContinuePrompt();
            DisplayScreenHeader("Finch Robot Commands");
            foreach (Command value in Enum.GetValues(typeof(Command)))
            {
                if (value != Command.NONE)
                {
                    if (value != Command.DONE)
                    {
                        Console.Write($"{value},");
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine("Enter 'Done' after entering desired commands");
            Console.WriteLine();
            while (command != Command.DONE)
            {
                Console.Write("Enter Command:");
                userResponse = Console.ReadLine().ToUpper();
                valid = Enum.TryParse(userResponse, out command);
                if (!valid)
                {
                    Console.WriteLine("Command Not Reconized, Make Sure To Enter Correctly");
                    command = Command.NONE;
                }
                if (command != Command.NONE)
                {
                    commands.Add(command);
                    Console.WriteLine($"{command} Added");
                }

            }

            Console.WriteLine("Continue to View Commands");
            DisplayContinuePrompt();
            DisplayFinchCommands(commands);
        }

        /// <summary>
        /// Display entered commands
        /// </summary>
        static void DisplayFinchCommands(List<Command> commands)
        {
            DisplayScreenHeader("Finch Robot Commands");

            foreach (Command command in commands)
            {
                Console.WriteLine(command);
            }

            DisplayContinuePrompt();
        }

        static List<Command> DisplayReadUserProgrammingData()
        {
            string dataPath = @"Data\Data.txt";
            List<Command> commands = new List<Command>();
            string[] commandArray;

            DisplayScreenHeader("Load Commands from Data File");

            Console.WriteLine();
            Console.WriteLine("Ready to load the commands from the data file.");
            DisplayContinuePrompt();

            //
            // create a list of Commmand
            //
            Command command;
            commandArray = File.ReadAllLines(dataPath);
            foreach (string commandString in commandArray)
            {
                Enum.TryParse(commandString, out command);

                commands.Add(command);
            }

            Console.WriteLine();
            Console.WriteLine("Commands loaded succesfully");
            DisplayContinuePrompt();

            return commands;
        }

        static void DisplayWriteUserProgrammingData(List<Command> commands)
        {
            string dataPath = @"Data\Data.txt";
            List<string> commandList = new List<string>();
            DisplayScreenHeader("Save Commands to the Data File");

            Console.WriteLine("Ready to save the commands to the data file.");
            DisplayContinuePrompt();
            //
            // creates list of command strings
            //

            foreach (Command command in commands)
            {
                commandList.Add(command.ToString());
            }
            File.WriteAllLines(dataPath, commandList);

            Console.WriteLine();
            Console.WriteLine("Commands have been saved to the data file...");
            Console.WriteLine();


            DisplayContinuePrompt();
        }

        /// <summary>
        /// Executes finch commands
        /// </summary>
        static void DisplayExecuteFinchCommands(
            Finch finchRobot,
            List<Command> commands,
            (int motorSpeed, int ledBrightness, int waitSeconds) CommandParameters)
        {
            int motorSpeed = CommandParameters.motorSpeed;
            int ledBrightness = CommandParameters.ledBrightness;
            int waitSeconds = CommandParameters.waitSeconds;

            DisplayScreenHeader("Execute Finch Commands");
            Console.WriteLine("When you continue the finch will execute the set commands. Make sure area is clear.");
            DisplayContinuePrompt();

            foreach (Command command in commands)
            {
                switch (command)
                {
                    case Command.NONE:
                        break;

                    case Command.MOVEFORWARD:
                        finchRobot.setMotors(motorSpeed, motorSpeed);
                        break;

                    case Command.MOVEBACKWARD:
                        finchRobot.setMotors(-motorSpeed, -motorSpeed);
                        break;

                    case Command.STOPMOTORS:
                        finchRobot.setMotors(0, 0);
                        break;

                    case Command.WAIT:
                        finchRobot.wait(waitSeconds * 1000);
                        break;

                    case Command.TURNRIGHT:
                        finchRobot.setMotors(motorSpeed / 2, motorSpeed);
                        break;

                    case Command.TURNLEFT:
                        finchRobot.setMotors(motorSpeed, motorSpeed / 2);
                        break;

                    case Command.LEDRED:
                        finchRobot.setLED(ledBrightness, 0, 0);
                        break;

                    case Command.LEDGREEN:
                        finchRobot.setLED(0, ledBrightness, 0);
                        break;

                    case Command.LEDBLUE:
                        finchRobot.setLED(0, 0, ledBrightness);
                        break;

                    case Command.LEDWHITE:
                        finchRobot.setLED(ledBrightness, ledBrightness, ledBrightness);
                        break;

                    case Command.LEDOFF:
                        finchRobot.setLED(0, 0, 0);
                        break;

                    case Command.GETTEMP:
                        Console.WriteLine($"Temperature : {ConvertCelciusToFahrenheit(finchRobot.getTemperature())}");
                        break;

                    case Command.GETLIGHT:
                        Console.WriteLine($"Light Level : {finchRobot.getLeftLightSensor()} / 255");
                        break;

                    case Command.DONE:
                        finchRobot.setMotors(0, 0);
                        finchRobot.setLED(0, 0, 0);
                        Console.WriteLine("Finch is finshed with commands");
                        break;

                    default:
                        break;
                }

            }

            DisplayContinuePrompt();
        }
        #endregion

        #region AlarmSystemHelpers

        /// <summary>
        /// Gets alarm type from user
        /// </summary>
        static string DisplayGetAlarmType()
        {
            string alarmType;
            Console.WriteLine("Enter Alarm Type [Light or Temperature]:");

            do
            {
                alarmType = Console.ReadLine().ToLower();
                switch (alarmType)
                {
                    case "light":
                        Console.WriteLine("Light alarm selected.");
                        break;
                    case "temperature":
                        Console.WriteLine("Temperature alarm selected");
                        break;
                    default:
                        alarmType = "retry";
                        Console.WriteLine("Please enter light or temperature");
                        break;
                }
            } while (alarmType == "retry");

            DisplayContinuePrompt();
            return alarmType;
        }

        /// <summary>
        /// Gets length of time to monitor from user
        /// </summary>
        static int DisplayGetMaxSeconds()
        {
            int maxSeconds;
            bool ifNumber;
            Console.WriteLine("How long would you like to keep the alarm activated in seconds?");
            do
            {
                ifNumber = int.TryParse(Console.ReadLine(), out maxSeconds);

                if (!ifNumber)
                {
                    Console.WriteLine();
                    Console.Write("Please enter a number:");
                }

            } while (!ifNumber);

            DisplayContinuePrompt();
            return maxSeconds;
        }

        /// <summary>
        /// Gets upper and lower thresholds from user
        /// </summary>
        static void DisplayGetThreshhold(Finch finchRobot, string alarmType, out double lowerThreshold, out double upperThreshold)
        {
            bool ifNumber;
            lowerThreshold = 0;
            upperThreshold = 0;
            DisplayScreenHeader("Threshold Value");


            switch (alarmType)
            {
                case "light":
                    Console.Write($"Current Light Level : {finchRobot.getLeftLightSensor()}");
                    Console.WriteLine();
                    Console.WriteLine("Enter maximum light level[0-255]");
                    do
                    {
                        ifNumber = double.TryParse(Console.ReadLine(), out upperThreshold);
                        if (!ifNumber)
                        {
                            Console.WriteLine();
                            Console.Write("Please enter a number:");
                        }
                        else if (upperThreshold > 255)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Maximum light level can't be higher than 255, please enter a valid number:");
                            ifNumber = false;
                        }
                        else if (upperThreshold < finchRobot.getLeftLightSensor())
                        {
                            Console.WriteLine();
                            Console.WriteLine("Maximum light level cannot be below ambient, please enter a valid number:");
                            ifNumber = false;
                        }

                    } while (!ifNumber);

                    Console.WriteLine();
                    Console.WriteLine("Enter minimum light level[0-255]");
                    do
                    {
                        ifNumber = double.TryParse(Console.ReadLine(), out lowerThreshold);
                        if (!ifNumber)
                        {
                            Console.WriteLine();
                            Console.Write("Please enter a number:");
                        }
                        else if (lowerThreshold < 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Minimum light level can't be less than 0, please enter a valid number:");
                            ifNumber = false;
                        }
                        else if (lowerThreshold > finchRobot.getLeftLightSensor())
                        {
                            Console.WriteLine();
                            Console.WriteLine("Minimum light level cannot be above ambient, please enter a valid number:");
                            ifNumber = false;
                        }

                    } while (!ifNumber);

                    break;

                case "temperature":
                    Console.Write($"Current Temperature : {ConvertCelciusToFahrenheit(finchRobot.getTemperature())}");
                    Console.WriteLine();
                    Console.WriteLine("Enter maximum temperature in degrees Fahrenheit");
                    do
                    {
                        ifNumber = double.TryParse(Console.ReadLine(), out upperThreshold);
                        if (!ifNumber)
                        {
                            Console.WriteLine();
                            Console.Write("Please enter a number:");
                        }
                        else if (upperThreshold < finchRobot.getLeftLightSensor())
                        {
                            Console.WriteLine();
                            Console.WriteLine("Maximum temperature cannot be below ambient, please enter a valid number:");
                            ifNumber = false;
                        }

                    } while (!ifNumber);

                    Console.WriteLine();
                    Console.WriteLine("Enter minimum temperature in degrees Fahrenheit");
                    do
                    {
                        ifNumber = double.TryParse(Console.ReadLine(), out lowerThreshold);
                        if (!ifNumber)
                        {
                            Console.WriteLine();
                            Console.Write("Please enter a number:");
                        }
                        else if (lowerThreshold > finchRobot.getLeftLightSensor())
                        {
                            Console.WriteLine();
                            Console.WriteLine("Minimum temperature cannot be above ambient, please enter a valid number:");
                            ifNumber = false;
                        }

                    } while (!ifNumber);

                    break;


                default:
                    throw new FormatException();
                    //break;
            }

            DisplayContinuePrompt();
        }

        /// <summary>
        /// Monitors temperature or light level
        /// </summary>
        static bool MonitorLevels(Finch finchRobot, double lowerThreshold, double upperThreshold, int maxSeconds, string alarmType)
        {
            bool thresholdExceeded = false;
            int currentLevel;
            double currentTemp;
            double seconds = 0;
            switch (alarmType)
            {
                case "light":
                    while (!thresholdExceeded && seconds <= maxSeconds)
                    {
                        currentLevel = finchRobot.getLeftLightSensor();

                        DisplayScreenHeader("Monitoring Light Levels...");
                        Console.WriteLine();
                        Console.WriteLine($"Maximum light level: {upperThreshold}");
                        Console.WriteLine($"Minimum light level: {lowerThreshold}");
                        Console.WriteLine($"Current light level: {currentLevel}");

                        if (currentLevel > upperThreshold)
                        {
                            thresholdExceeded = true;
                        }
                        else if (currentLevel < lowerThreshold)
                        {
                            thresholdExceeded = true;
                        }

                        finchRobot.wait(500);
                        seconds += .5;
                    }
                    break;

                case "temperature":
                    while (!thresholdExceeded && seconds <= maxSeconds)
                    {
                        currentTemp = finchRobot.getTemperature();

                        DisplayScreenHeader("Monitoring Temperature Levels...");
                        Console.WriteLine();
                        Console.WriteLine($"Maximum temperature: {ConvertCelciusToFahrenheit(upperThreshold)}");
                        Console.WriteLine($"Minimum light level: {ConvertCelciusToFahrenheit(lowerThreshold)}");
                        Console.WriteLine($"Current light level: {ConvertCelciusToFahrenheit(currentTemp)}");
                        if (currentTemp > upperThreshold)
                        {
                            thresholdExceeded = true;
                        }
                        else if (currentTemp < lowerThreshold)
                        {
                            thresholdExceeded = true;
                        }

                        finchRobot.wait(500);
                        seconds += .5;
                    }
                    break;

                default:
                    break;
            }

            return thresholdExceeded;
        }


        #endregion

        #region DataRecorderHelpers
        /// <summary>
        /// Displays Temperature Data
        /// </summary>
        static void DisplayTempData(double[] temperatures)
        {
            DisplayScreenHeader("Temperatures");

            for (int i = 0; i < temperatures.Length; i++)
            {
                Console.WriteLine($"Temperature {i + 1}: {temperatures[i]} degrees Fahrenheit.");

            }

            DisplayContinuePrompt();
        }

        /// <summary>
        /// Collects Temperature Data
        /// </summary>
        static void DisplayGetTempData(Finch finchRobot, int numberOfDataPoints, double dataPointFrequency, double[] temperatures)
        {
            DisplayScreenHeader("Getting Temperatures");
            Console.WriteLine();
            Console.WriteLine("After you continue, the Finch will begin recording data. Once the recording has finished you can continue to view the collected data.");
            Console.WriteLine();
            DisplayContinuePrompt();


            for (int i = 0; i < numberOfDataPoints; i++)
            {
                temperatures[i] = ConvertCelciusToFahrenheit(finchRobot.getTemperature());
                int milliseconds = Convert.ToInt32(dataPointFrequency * 1000);
                finchRobot.wait(milliseconds);

                Console.WriteLine($"Temperature Reading {i + 1} Complete...");
            }
            Console.WriteLine();
            Console.WriteLine("Data recording has finished.");

            DisplayContinuePrompt();
        }

        /// <summary>
        /// Converts Finch Temp Recording into Fahrenheit
        static double ConvertCelciusToFahrenheit(double celciusTemp)
        {
            double fahrenheitTemp;

            fahrenheitTemp = celciusTemp * 9 / 5 + 32;

            return fahrenheitTemp;
        }

        /// <summary>
        /// Displays Light Sensor Data
        /// </summary>
        static void DisplayLightData(int[] light)
        {
            DisplayScreenHeader("Light Levels");

            for (int i = 0; i < light.Length; i++)
            {
                Console.WriteLine($"Light Level {i + 1}: {light[i]}");
            }

            DisplayContinuePrompt();
        }

        /// <summary>
        /// Collects Light Sensor Data
        /// </summary>
        static void DisplayGetLightData(Finch finchRobot, int numberOfDataPoints, double dataPointFrequency, int[] light)
        {
            DisplayScreenHeader("Getting Light Levels");
            Console.WriteLine();
            Console.WriteLine("After you continue, the Finch will begin recording data. Once the recording has finished you can continue to view the collected data.");
            Console.WriteLine();
            DisplayContinuePrompt();

            for (int i = 0; i < numberOfDataPoints; i++)
            {
                light[i] = (finchRobot.getLeftLightSensor() + finchRobot.getRightLightSensor() / 2);
                int milliseconds = Convert.ToInt32(dataPointFrequency * 1000);
                finchRobot.wait(milliseconds);

                Console.WriteLine($"Light Level Reading {i + 1} Complete...");
            }

            Console.WriteLine();
            Console.WriteLine("Data recording has finished.");

            DisplayContinuePrompt();
        }

        /// <summary>
        /// Prompts User for Number of Data Points to Record
        /// </summary>
        static int DisplayGetNumberOfDataPoints()
        {
            bool ifNumber;
            int numberOfDataPoints;
            Console.WriteLine();
            Console.Write("Enter the number of data points: ");
            do
            {
                ifNumber = int.TryParse(Console.ReadLine(), out numberOfDataPoints);

                if (!ifNumber)
                {
                    Console.WriteLine();
                    Console.Write("Please enter a number:");
                }

            } while (!ifNumber);


            return numberOfDataPoints;

        }

        /// <summary>
        /// Prompts User for Frequency of Data Point Recordings
        /// </summary>
        static double DisplayGetDataPointFrequency()
        {
            double dataPointFrequency;
            bool ifNumber;
            Console.Write("Enter frequency of data recordings in seconds: ");
            do
            {
                ifNumber = double.TryParse(Console.ReadLine(), out dataPointFrequency);

                if (!ifNumber)
                {
                    Console.WriteLine();
                    Console.Write("Please enter a number:");
                }

            } while (!ifNumber);

            return dataPointFrequency;
        }

        #endregion

        #region TalentShowHelpers
        static void PlaySong(Finch finchRobot)
        {
            for (int i = 0; i < 3; i++)
            {
                PlayTone(finchRobot, 165, 500);
                PauseTone(finchRobot, 10);
                PlayTone(finchRobot, 165, 20);
                PauseTone(finchRobot, 20);
                PlayTone(finchRobot, 220, 20);
                PauseTone(finchRobot, 30);
                PlayTone(finchRobot, 165, 20);
                PauseTone(finchRobot, 30);
                PlayTone(finchRobot, 140, 40);
                PauseTone(finchRobot, 30);
                PlayTone(finchRobot, 120, 600);
                PauseTone(finchRobot, 10);
                PlayTone(finchRobot, 101, 600);
                PauseTone(finchRobot, 10);
            }
            for (int i = 0; i < 2; i++)
            {
                PlayTone(finchRobot, 165 + 400, 500);
                PauseTone(finchRobot, 10);
                PlayTone(finchRobot, 165 + 400, 20);
                PauseTone(finchRobot, 20);
                PlayTone(finchRobot, 220 + 400, 20);
                PauseTone(finchRobot, 30);
                PlayTone(finchRobot, 165 + 400, 20);
                PauseTone(finchRobot, 30);
                PlayTone(finchRobot, 140 + 400, 40);
                PauseTone(finchRobot, 30);
                PlayTone(finchRobot, 120 + 400, 600);
                PauseTone(finchRobot, 10);
                PlayTone(finchRobot, 101 + 400, 600);
                PauseTone(finchRobot, 10);
            }

            PlayTone(finchRobot, 165, 500);
            PauseTone(finchRobot, 10);
            PlayTone(finchRobot, 165, 20);
            PauseTone(finchRobot, 20);
            PlayTone(finchRobot, 220, 20);
            PauseTone(finchRobot, 30);
            PlayTone(finchRobot, 165, 20);
            PauseTone(finchRobot, 30);
            PlayTone(finchRobot, 140, 40);
            PauseTone(finchRobot, 30);
            PlayTone(finchRobot, 120, 600);
            PauseTone(finchRobot, 10);
            PlayTone(finchRobot, 101, 600);
            PauseTone(finchRobot, 10);

            RobotStopAll(finchRobot);
        }

        static void PauseTone(Finch finchRobot, int time)
        {
            finchRobot.noteOff();
            finchRobot.wait(time);
        }

        static void PlayTone(Finch finchRobot, int hz, int time)
        {
            finchRobot.noteOn(hz);
            finchRobot.wait(time);
        }

        static void RobotStopAll(Finch finchRobot)
        {
            finchRobot.setMotors(0, 0);
            finchRobot.setLED(0, 0, 0);
            finchRobot.noteOff();
        }
        #endregion

        #region HELPER METHODS

        /// <summary>
        /// allows user to set theme
        /// </summary>
        static void ThemeConfig()
        {
            string dataPath = @"Data\Theme.txt";
            string foregroundColorString;
            string backgroundColorString;
            bool colorValid;
            ConsoleColor foregroundColor;
            ConsoleColor backgroundColor;

            DisplayScreenHeader("Theme Selection");
            Console.WriteLine();
            do
            {
                Console.WriteLine("Please enter desired foreground color:");
                foregroundColorString = Console.ReadLine();
                colorValid = Enum.TryParse(foregroundColorString, out foregroundColor);
                if (colorValid)
                {
                    File.WriteAllText(dataPath, foregroundColorString);
                }
                else
                {
                    Console.WriteLine($"{foregroundColorString} is not a valid color");
                }

            } while (!colorValid);

            colorValid = false;
            File.AppendAllText(dataPath, ",");

            do
            {
                Console.WriteLine("Please enter desired background color:");
                backgroundColorString = Console.ReadLine();
                colorValid = Enum.TryParse(backgroundColorString, out backgroundColor);
                if (colorValid)
                {
                    File.AppendAllText(dataPath, backgroundColorString);
                }
                else
                {
                    Console.WriteLine($"{foregroundColorString} is not a valid color");
                }

            } while (!colorValid);


            SetTheme();
            Console.Clear();           
            Console.WriteLine("Theme has been set.");
            DisplayContinuePrompt();

        }

        /// <summary>
        /// sets theme from data file
        /// </summary>
        static void SetTheme()
        {
            string dataPath = @"Data\Theme.txt";
            string foregroundColorString;
            string backgroundColorString;
            ConsoleColor foreground;
            ConsoleColor background;
            string theme;
            string[] themeArray;

            theme = File.ReadAllText(dataPath);
            themeArray = theme.Split(',');
            foregroundColorString = themeArray[0];
            backgroundColorString = themeArray[1];
            Enum.TryParse(foregroundColorString, out foreground);
            Enum.TryParse(backgroundColorString, out background);
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;

        }

        /// <summary>
        /// display continue prompt
        /// </summary>
        static void DisplayContinuePrompt()
        {
            Console.WriteLine();
            Console.WriteLine("\t\tPress any key to continue.");
            Console.ReadKey();
        }

        /// <summary>
        /// display screen header
        /// </summary>
        static void DisplayScreenHeader(string headerText)
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("\t\t" + headerText);
            Console.WriteLine();
        }

        #endregion

    }
}