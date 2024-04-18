using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Battleships
{
    class Program
    {
        //Class to store all global variables 
        public static class GlobalData
        {
            //Initialises all the 2D arrays as global values. However, they are not populated until populateDatabase(); is called.
            //2D array to store each element in the player's 11x11 grid
            public static string[,] playerGrid = new string[11, 11];
            //Enemy 11x11 2D array that is shown to the player in-game
            public static string[,] displayEnemyGrid = new string[11, 11];
            //2D array to store the actual locations of the enemy's ships (not shown to player in-game)
            public static string[,] actualEnemyGrid = new string[11, 11];



            public static Random rand = new Random();



            //Strings to store the statements shown in purple below the grids(i.e. 'Enemy strike missed')
            //Result of the user's last action
            public static string userActionStatusReport = "Nothing to report";
            //Result of the enemy's last action
            public static string enemyActionStatusReport = "Nothing to report";




            //Bool to store surrender value
            public static bool surrender = false;




            //Initialises the starting coordinates for the veteranEnemyStrike searching pattern
            public static int[] structuredCoordinates = new int[2] { 1, 1 };
            public static int[] structuredCoordinatesOffset = new int[2] { 2, 2 };
            //Boolean to signify the start of the second part of the searching pattern
            public static bool offsetStart = false;
            //Boolean to signify the searching pattern has finished
            public static bool structFinished = false;




            //String to store the difficulty level
            public static string aiDifficulty = "Experienced";

            //String to keep track of devMode
            public static string devMode = "Off";

            //String to keep track of darkMode
            public static string darkMode = "On";

            //String to keep track of manual/automatic player fleet generation
            public static string autoGenMode = "Off";

            //String to track game version
            public static string patchNo = "V3.00";
            //Look in '\\strs\dfs\Devs\data\18SWADKKs\Projects\# Complete\Battleships' for patch notes
        };






        //This procedure populates one of the 2D arrays (2D array is chosen in the parameter 'tableIndex' - 1)playerGrid, 2)displayEnemyGrid, 3)actualEnemyGrid)
        //In populating the array, the function basically gives the grid x and y scales in the first row and column, as well as 'blank' elements for everything else
        public static void populateDatabase(int tableIndex)
        {
            //Runs 11 times, for each row in the table
            for (int y = 0; y < 11; y++)
            {
                //Creates a variable used to store the numeical -> alphabetical converted value for the y coordinate
                char correspondingYval = ' ';

                //As long as y is greater than 0 (i.e. the indexed location isn't a column header), add 64 to y and convert to it's ACSII equivalent (e.g. if y=2, you convert 66 to ASCII which gives 'B')
                if (y > 0) { correspondingYval = (char)(y + 64); }


                //Runs 11 times, for each column in the table
                for (int x = 0; x < 11; x++)
                {
                    //If y coordinate is 0 (i.e. if the element is a column header)...
                    if (y == 0)
                    {
                        //Sets the column numbers in each array
                        if (tableIndex == 1) { GlobalData.playerGrid[y, x] = "[" + x + "]"; }
                        else if (tableIndex == 2) { GlobalData.displayEnemyGrid[y, x] = "[" + x + "]"; }
                        else if (tableIndex == 3) { GlobalData.actualEnemyGrid[y, x] = "[" + x + "]"; }
                    }
                    //If x coordinate is 0 (i.e. if the element is a row header)...
                    else if (x == 0)
                    {
                        //Sets the row letters in each array
                        if (tableIndex == 1) { GlobalData.playerGrid[y, x] = "[" + correspondingYval + "]"; }
                        else if (tableIndex == 2) { GlobalData.displayEnemyGrid[y, x] = "[" + correspondingYval + "]"; }
                        else if (tableIndex == 3) { GlobalData.actualEnemyGrid[y, x] = "[" + correspondingYval + "]"; }
                    }
                    //Otherwise, if it is just a regular element, give it the associated 'normal' value
                    else
                    {
                        if (tableIndex == 1) { GlobalData.playerGrid[y, x] = "[~]"; }
                        else if (tableIndex == 2) { GlobalData.displayEnemyGrid[y, x] = "[?]"; }
                        else if (tableIndex == 3) { GlobalData.actualEnemyGrid[y, x] = "[~]"; }
                    }
                    //If the element is the origin square, set it as blank
                    if (x == 0 && y == 0)
                    {
                        if (tableIndex == 1) { GlobalData.playerGrid[y, x] = "   "; }
                        else if (tableIndex == 2) { GlobalData.displayEnemyGrid[y, x] = "   "; }
                        else if (tableIndex == 3) { GlobalData.actualEnemyGrid[y, x] = "   "; }
                    }

                }
            }
        }






        //This procedure generates the enemy's fleet
        public static void generateEnemyFleet()
        {
            /*
            Carrier - 6
            Battleship - 5
            Destroyer - 4
            Submarine - 3
            Patrol Boat - 2
            */

            //Generation checkers for enemy board
            bool enemyFleetGenerated = false;

            bool enemyCarrGenerated = false;
            bool enemyBattGenerated = false;
            bool enemyDestGenerated = false;
            bool enemySubGenerated = false;
            bool enemyPatrGenerated = false;


            //Initialises variable shiplength as 0
            int shipLength = 0;


            //While all the fleet has not yet been fully generated...
            while (enemyFleetGenerated == false)
            {
                //Sets shipLength to the length of the longest ship that has not been generated
                if (enemyCarrGenerated == false) { shipLength = 6; }
                else if (enemyBattGenerated == false) { shipLength = 5; }
                else if (enemyDestGenerated == false) { shipLength = 4; }
                else if (enemySubGenerated == false) { shipLength = 3; }
                else if (enemyPatrGenerated == false) { shipLength = 2; }

                //Initialise collisionImminent as false
                bool collisionImminent = false;

                //X and Y coordinates generated between 1-10
                int xCoordinate = GlobalData.rand.Next(1, 11);
                int yCoordinate = GlobalData.rand.Next(1, 11);
                //Direction generated between as either 1:Vertical, or 2:Horizontal
                int direction = GlobalData.rand.Next(1, 3);

                //PRIMARY COLLISIONS: This section makes sure no ships are generated off the side of the grid
                //If generating ship vertically and y coordinate is closer to the top border than the actual length of ship, set collisionImminent to true
                if (direction == 1 && yCoordinate < shipLength) { collisionImminent = true; }
                //Otherwise, if generating ship horizontally and x coordinate is closer to the right border than the actual length of ship, set collisionImminent to true
                else if (direction == 2 && xCoordinate > 11 - shipLength) { collisionImminent = true; }



                //SECONDARY COLLISIONS: This section makes sure no two ships collide with each other during generation
                //If no collisions have been raised so far (i.e. the ship will hypothetically generate fully on the grid)...
                if (collisionImminent == false)
                {
                    //Creates a copy of the original coordinates to use for checking secondary collisions
                    int yCoordChecker = yCoordinate;
                    int xCoordChecker = xCoordinate;

                    //Repeats from 0 to the length of the ship being generated
                    for (int x = 0; x < shipLength; x++)
                    {
                        //If checker coordinate contains a ship square already, set collisionImminent to true
                        if (GlobalData.actualEnemyGrid[yCoordChecker, xCoordChecker] == "[!]") { collisionImminent = true; }

                        //If checker coordinate (with an offset of 1 in each direction) already contains a ship square, set colission Imminent to true
                        if (yCoordChecker > 1) { if (GlobalData.actualEnemyGrid[yCoordChecker - 1, xCoordChecker] == "[!]") { collisionImminent = true; } }
                        if (yCoordChecker < 10) { if (GlobalData.actualEnemyGrid[yCoordChecker + 1, xCoordChecker] == "[!]") { collisionImminent = true; } }
                        if (xCoordChecker > 1) { if (GlobalData.actualEnemyGrid[yCoordChecker, xCoordChecker - 1] == "[!]") { collisionImminent = true; } }
                        if (xCoordChecker < 10) { if (GlobalData.actualEnemyGrid[yCoordChecker, xCoordChecker + 1] == "[!]") { collisionImminent = true; } }

                        //This section changes the respective checker coordinate depending on the generation direction of the ship
                        //If generating vertically, reduce y coordinate by 1
                        if (direction == 1) { yCoordChecker--; }
                        //If generating horizontally, increase x coordinate by 1
                        else if (direction == 2) { xCoordChecker++; }
                    }
                }



                //If there are no collisions...
                if (collisionImminent == false)
                {
                    //The shipGenerated boolean value corresponding to the length of the generated ship is set to true so it is not generated again and so the next ship can begin generation
                    if (shipLength == 6) { enemyCarrGenerated = true; }
                    if (shipLength == 5) { enemyBattGenerated = true; }
                    if (shipLength == 4) { enemyDestGenerated = true; }
                    if (shipLength == 3) { enemySubGenerated = true; }
                    if (shipLength == 2) { enemyPatrGenerated = true; }

                    //SHIP GENERATION: This section finally generates the ship by changing the relevant values in the player's 2D array
                    //While x is less than shipLength...
                    for (int x = 0; x < shipLength; x++)
                    {
                        //Sets the relevant element in the 2D array to [!]
                        GlobalData.actualEnemyGrid[yCoordinate, xCoordinate] = "[!]";

                        //If generating vertically, reduce y coordinate by 1
                        if (direction == 1) { yCoordinate--; }
                        //If generating horizontally, increase x coordinate by 1
                        else if (direction == 2) { xCoordinate++; }
                    }
                }


                //If every ship in the fleet has been generated, userFleetGenerated is set to true to end the procedure
                if (enemyCarrGenerated == true && enemyBattGenerated == true && enemyDestGenerated == true && enemySubGenerated == true && enemyPatrGenerated == true) { enemyFleetGenerated = true; }
            }
        }

        //This procedure automatically generates the player's fleet
        public static void autoGeneratePlayerFleet()
        {
            /*
            Carrier - 6
            Battleship - 5
            Destroyer - 4
            Submarine - 3
            Patrol Boat - 2
            */

            //Generation checkers for player board
            bool playerFleetGenerated = false;

            bool playerCarrGenerated = false;
            bool playerBattGenerated = false;
            bool playerDestGenerated = false;
            bool playerSubGenerated = false;
            bool playerPatrGenerated = false;


            //Initialises variable shiplength as 0
            int shipLength = 0;


            //While all the fleet has not yet been fully generated...
            while (playerFleetGenerated == false)
            {
                //Sets shipLength to the length of the longest ship that has not been generated
                if (playerCarrGenerated == false) { shipLength = 6; }
                else if (playerBattGenerated == false) { shipLength = 5; }
                else if (playerDestGenerated == false) { shipLength = 4; }
                else if (playerSubGenerated == false) { shipLength = 3; }
                else if (playerPatrGenerated == false) { shipLength = 2; }

                //Initialise collisionImminent as false
                bool collisionImminent = false;

                //X and Y coordinates generated between 1-10
                int xCoordinate = GlobalData.rand.Next(1, 11);
                int yCoordinate = GlobalData.rand.Next(1, 11);
                //Direction generated as either 1:Vertical, or 2:Horizontal
                int direction = GlobalData.rand.Next(1, 3);

                //PRIMARY COLLISIONS: This section makes sure no ships are generated off the side of the grid
                //If generating ship vertically and y coordinate is closer to the top border than the actual length of ship, set collisionImminent to true
                if (direction == 1 && yCoordinate < shipLength) { collisionImminent = true; }
                //Otherwise, if generating ship horizontally and x coordinate is closer to the right border than the actual length of ship, set collisionImminent to true
                else if (direction == 2 && xCoordinate > 11 - shipLength) { collisionImminent = true; }



                //SECONDARY COLLISIONS: This section makes sure no two ships collide with each other during generation, and that the ship will not generate within one square of another
                //If no collisions have been raised so far (i.e. the ship will hypothetically generate fully on the grid)...
                if (collisionImminent == false)
                {
                    //Creates a copy of the original coordinates to use for checking secondary collisions
                    int yCoordChecker = yCoordinate;
                    int xCoordChecker = xCoordinate;

                    //Repeats from 0 to the length of the ship being generated
                    for (int x = 0; x < shipLength; x++)
                    {
                        //If checker coordinate contains a ship square already, set collisionImminent to true
                        if (GlobalData.playerGrid[yCoordChecker, xCoordChecker] == "[+]") { collisionImminent = true; }

                        //If checker coordinate (with an offset of 1 in each direction) already contains a ship square, set colission Imminent to true
                        if (yCoordChecker > 1) { if (GlobalData.playerGrid[yCoordChecker - 1, xCoordChecker] == "[+]") { collisionImminent = true; } }
                        if (yCoordChecker < 10) { if (GlobalData.playerGrid[yCoordChecker + 1, xCoordChecker] == "[+]") { collisionImminent = true; } }
                        if (xCoordChecker > 1) { if (GlobalData.playerGrid[yCoordChecker, xCoordChecker - 1] == "[+]") { collisionImminent = true; } }
                        if (xCoordChecker < 10) { if (GlobalData.playerGrid[yCoordChecker, xCoordChecker + 1] == "[+]") { collisionImminent = true; } }

                        //This section changes the respective checker coordinate depending on the generation direction of the ship
                        //If generating vertically, reduce y coordinate by 1
                        if (direction == 1) { yCoordChecker--; }
                        //If generating horizontally, increase x coordinate by 1
                        else if (direction == 2) { xCoordChecker++; }
                    }
                }



                //If there are no collisions...
                if (collisionImminent == false)
                {
                    //The shipGenerated boolean value corresponding to the length of the generated ship is set to true so it is not generated again and so the next ship can begin generation
                    if (shipLength == 6) { playerCarrGenerated = true; }
                    if (shipLength == 5) { playerBattGenerated = true; }
                    if (shipLength == 4) { playerDestGenerated = true; }
                    if (shipLength == 3) { playerSubGenerated = true; }
                    if (shipLength == 2) { playerPatrGenerated = true; }

                    //SHIP GENERATION: This section finally generates the ship by changing the relevant values in the player's 2D array
                    //While x is less than shipLength...
                    for (int x = 0; x < shipLength; x++)
                    {
                        //Sets the relevant element in the 2D array to [+]
                        GlobalData.playerGrid[yCoordinate, xCoordinate] = "[+]";

                        //If generating vertically, reduce y coordinate by 1
                        if (direction == 1) { yCoordinate--; }
                        //If generating horizontally, increase x coordinate by 1
                        else if (direction == 2) { xCoordinate++; }
                    }
                }


                //If every ship in the fleet has been generated, userFleetGenerated is set to true to end the procedure
                if (playerCarrGenerated == true && playerBattGenerated == true && playerDestGenerated == true && playerSubGenerated == true && playerPatrGenerated == true) { playerFleetGenerated = true; }
            }
        }

        //This procedure allows the player to generate their fleet themselves
        public static bool manualGeneratePlayerFleet()
        {
            /*
            Carrier - 6
            Battleship - 5
            Destroyer - 4
            Submarine - 3
            Patrol Boat - 2
            */

            Console.Clear();

            //Sets the window title
            Console.Title = "Battleships - Preparation Phase";

            //Generation checkers for player board
            bool playerFleetGenerated = false;

            bool playerCarrGenerated = false;
            bool playerBattGenerated = false;
            bool playerDestGenerated = false;
            bool playerSubGenerated = false;
            bool playerPatrGenerated = false;



            //While the player has not yet finished generating their fleet...
            while (!playerFleetGenerated)
            {
                //Sets UI
                Console.Clear();
                displayPlayerGrid();

                resetDefaultFontColour();
                Console.Write("\n\nChoose ship class to generate:\n");
                if (!playerPatrGenerated) { Console.WriteLine("1. Patrol Boat [2 squares]"); }
                if (!playerSubGenerated) { Console.WriteLine("2. Submarine [3 squares]"); }
                if (!playerDestGenerated) { Console.WriteLine("3. Destroyer [4 squares]"); }
                if (!playerBattGenerated) { Console.WriteLine("4. Battleship [5 squares]"); }
                if (!playerCarrGenerated) { Console.WriteLine("5. Carrier [6 squares]"); }

                Console.Write("\nEnter corresponding number: ");

                //Initialises some variables
                bool validInput = false;
                int chosenShipLength = 0;
                //Exception handling for chosenShipLength
                while (!validInput)
                {
                    string input = Console.ReadLine();

                    //If user wants to exit
                    if (input.ToLower() == "surrender" || input.ToLower() == "end" || input.ToLower() == "exit") { return false; }

                    //Takes input and converts to int
                    validInput = int.TryParse(input, out chosenShipLength);
                    //Increases chosenShipLength by 1 to offset the numbering (i.e. Patrol boat is number 1 but takes up 2 squares)
                    chosenShipLength++;

                    //If the input is INVALID...
                    if (!(validInput && chosenShipLength >= 2 && chosenShipLength <= 6) || input.Length != 1)
                    {
                        validInput = false;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("\bError: Invalid input");
                        resetDefaultFontColour();
                        Console.Write("\n\nEnter corresponding number: ");
                    }
                    else if ((chosenShipLength == 2 && playerPatrGenerated) || (chosenShipLength == 3 && playerSubGenerated) || (chosenShipLength == 4 && playerDestGenerated) || (chosenShipLength == 5 && playerBattGenerated) || (chosenShipLength == 6 && playerCarrGenerated))
                    {
                        validInput = false;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("\bError: Ship class already generated");
                        resetDefaultFontColour();
                        Console.Write("\n\nEnter corresponding number: ");
                    }
                }

                Console.Clear();
                displayPlayerGrid();
                resetDefaultFontColour();

                Console.Write("\n\nGenerating ");
                if (chosenShipLength == 2) { Console.Write("Patrol Boat [2 squares]"); }
                else if (chosenShipLength == 3) { Console.Write("Submarine [3 squares]"); }
                else if (chosenShipLength == 4) { Console.Write("Destroyer [4 squares]"); }
                else if (chosenShipLength == 5) { Console.Write("Battleship [5 squares]"); }
                else if (chosenShipLength == 6) { Console.Write("Carrier [6 squares]"); }
                Console.Write("...");


                bool collisionImminent = true;

                while (collisionImminent)
                {
                    collisionImminent = false;
                    //Initialises some variables for taking input
                    bool directionValid = false;
                    char direction = 'X';
                    bool xValid = false;
                    int xVal = 20;
                    bool yValid = false;
                    int yVal = 20;



                    //Direction input exception handling
                    while (!directionValid)
                    {
                        Console.Write("\n\nEnter generation direction(N/E/S/W): ");

                        string rawDirectionInput = Console.ReadLine();

                        //If user wants to exit
                        if (rawDirectionInput.ToLower() == "surrender" || rawDirectionInput.ToLower() == "end" || rawDirectionInput.ToLower() == "exit") { return false; }

                        if (rawDirectionInput.Length == 1)
                        {
                            direction = char.ToLower(Convert.ToChar(rawDirectionInput));

                            if (direction == 'n' || direction == 'e' || direction == 's' || direction == 'w') { directionValid = true; }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("Error: Invalid Input");
                                resetDefaultFontColour();
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("Error: Invalid Input");
                            resetDefaultFontColour();
                        }
                    }
                    //X-Coordinate input exception handling
                    while (!xValid)
                    {
                        Console.Write("\nEnter generation X-Coordinate: ");
                        string rawXval = Console.ReadLine();
                        int.TryParse(rawXval, out xVal);

                        //If user wants to exit...
                        if (rawXval.ToLower() == "surrender" || rawXval.ToLower() == "end" || rawXval.ToLower() == "exit") { return false; }

                        if (direction == 'e')
                        {
                            if (xVal >= 1 && (xVal + chosenShipLength - 1) <= 10) { xValid = true; }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: Invalid input");
                                resetDefaultFontColour();
                            }
                        }
                        else if (direction == 'w')
                        {
                            if (xVal <= 10 && xVal >= chosenShipLength) { xValid = true; }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: Invalid input");
                                resetDefaultFontColour();
                            }
                        }
                        else
                        {
                            if (xVal >= 1 && xVal <= 10) { xValid = true; }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: Invalid input");
                                resetDefaultFontColour();
                            }
                        }
                    }
                    //Y-Coordinate input exception handling
                    while (!yValid)
                    {
                        bool yValidInput = false;
                        while (!yValidInput)
                        {
                            Console.Write("\nEnter generation Y-Coordinate: ");

                            //Taking input for the y-coordinate
                            string YrawInput = Console.ReadLine();

                            //If the length of the input string is equal to one(as it is expected to be)....
                            if (YrawInput.Length == 1)
                            {
                                //Convert the input string to a character and then convert the character to its ASCII equivalent
                                int YasciiRep = (int)Convert.ToChar(YrawInput);

                                //If input is lowercase a-j instead of uppercase, convert to uppercase (using ASCII values)
                                if (Char.IsLower(Convert.ToChar(YrawInput))) { YasciiRep = YasciiRep - 32; }

                                //If the input is an uppercase character between A-J...
                                if (YasciiRep > 64 && YasciiRep < 75)
                                {
                                    //Converts from character to it's relevant integer indexing value (e.g. C converts to 3)
                                    yVal = YasciiRep - 64;
                                    yValidInput = true;
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Error: Invalid input");
                                    resetDefaultFontColour();
                                }
                            }
                            //If user wants to exit...
                            else if (YrawInput.ToLower() == "surrender" || YrawInput.ToLower() == "end" || YrawInput.ToLower() == "exit") { return false; }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: Invalid input");
                                resetDefaultFontColour();
                            }
                        }

                        if (direction == 'n')
                        {
                            if (yVal <= 10 && yVal >= chosenShipLength) { yValid = true; }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: Invalid input");
                                resetDefaultFontColour();
                            }
                        }
                        else if (direction == 's')
                        {
                            if (yVal >= 1 && (yVal + chosenShipLength - 1) <= 10) { yValid = true; }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: Invalid input");
                                resetDefaultFontColour();
                            }
                        }
                        else
                        {
                            if (yVal >= 1 && yVal <= 10) { yValid = true; }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error: Invalid input");
                                resetDefaultFontColour();
                            }
                        }
                    }



                    //SECONDARY COLLISIONS: This section makes sure no two ships collide with each other during generation, and that the ship will not generate within one square of another
                    //Creates a copy of the original coordinates to use for checking secondary collisions
                    int yCoordChecker = yVal;
                    int xCoordChecker = xVal;

                    //Repeats from 0 to the length of the ship being generated
                    for (int x = 0; x < chosenShipLength; x++)
                    {
                        //If checker coordinate contains a ship square already, set collisionImminent to true
                        if (GlobalData.playerGrid[yCoordChecker, xCoordChecker] == "[+]") { collisionImminent = true; }

                        //If checker coordinate (with an offset of 1 in each direction) already contains a ship square, set colission Imminent to true
                        if (yCoordChecker > 1) { if (GlobalData.playerGrid[yCoordChecker - 1, xCoordChecker] == "[+]") { collisionImminent = true; } }
                        if (yCoordChecker < 10) { if (GlobalData.playerGrid[yCoordChecker + 1, xCoordChecker] == "[+]") { collisionImminent = true; } }
                        if (xCoordChecker > 1) { if (GlobalData.playerGrid[yCoordChecker, xCoordChecker - 1] == "[+]") { collisionImminent = true; } }
                        if (xCoordChecker < 10) { if (GlobalData.playerGrid[yCoordChecker, xCoordChecker + 1] == "[+]") { collisionImminent = true; } }

                        //This section changes the respective checker coordinate depending on the generation direction of the ship
                        //If generating northwards, reduce y coordinate by 1
                        if (direction == 'n') { yCoordChecker--; }
                        //If generating eastwards, increase x coordinate by 1
                        else if (direction == 'e') { xCoordChecker++; }
                        //If generating southwards, increase y coordinate by 1
                        if (direction == 's') { yCoordChecker++; }
                        //If generating westwards, reduce x coordinate by 1
                        else if (direction == 'w') { xCoordChecker--; }
                    }



                    //Collision error message
                    if (collisionImminent == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: Ship overlap detected - please enter a new set of coordinates");
                        resetDefaultFontColour();
                    }
                    //If there are no collisions, generate the ship
                    else
                    {
                        //The shipGenerated boolean value corresponding to the length of the generated ship is set to true so it is not generated again and so the next ship can begin generation
                        if (chosenShipLength == 6) { playerCarrGenerated = true; }
                        if (chosenShipLength == 5) { playerBattGenerated = true; }
                        if (chosenShipLength == 4) { playerDestGenerated = true; }
                        if (chosenShipLength == 3) { playerSubGenerated = true; }
                        if (chosenShipLength == 2) { playerPatrGenerated = true; }

                        //SHIP GENERATION: This section finally generates the ship by changing the relevant values in the player's 2D array
                        //While i is less than shipLength...
                        for (int i = 0; i < chosenShipLength; i++)
                        {
                            //Sets the relevant element in the 2D array to [+]
                            GlobalData.playerGrid[yVal, xVal] = "[+]";

                            //GlobalData.playerGrid[yVal, xVal] = "[+]";


                            //If generating northwards, reduce y coordinate by 1
                            if (direction == 'n') { yVal--; }
                            //If generating eastwards, increase x coordinate by 1
                            else if (direction == 'e') { xVal++; }
                            //If generating southwards, increase y coordinate by 1
                            else if (direction == 's') { yVal++; }
                            //If generating westwards, reduce x coordinate by 1
                            else if (direction == 'w') { xVal--; }

                        }
                    }




                }

                if (playerCarrGenerated && playerBattGenerated && playerDestGenerated && playerSubGenerated && playerPatrGenerated) { playerFleetGenerated = true; }
            }
            return true;
        }




        //This procedure prints out the player's grid
        public static void displayPlayerGrid()
        {
            resetDefaultFontColour();
            Console.Write("\n\nYour grid:\n");


            //A for loop (which runs 11 times), within a for loop (that runs 11 times) creates a table of 121 elements, fully displaying one of the three 2D arrays
            //Runs 11 times, once for each row in the 2D array
            for (int x = 0; x < 11; x++)
            {
                Console.Write("\n");
                //Runs 11 times to display every element in the table, plus one extra for displaing the row names on the left
                for (int i = 0; i < 11; i++)
                {
                    //If element is a row or column name, change text colour to yellow
                    if (i == 0 || x == 0)
                    {
                        if (resetDefaultFontColour() == "Dark") { Console.ForegroundColor = ConsoleColor.Yellow; }
                        else { Console.ForegroundColor = ConsoleColor.DarkYellow; }
                    }
                    //Otherwise, if element is a sea square, set text colour to blue
                    else if (GlobalData.playerGrid[x, i] == "[~]") { Console.ForegroundColor = ConsoleColor.Blue; }
                    //Otherwise, if element is a missed enemy strike square, set text colour to dark green
                    else if (GlobalData.playerGrid[x, i] == "[M]") { Console.ForegroundColor = ConsoleColor.Cyan; }
                    //Otherwise, if element is an undamaged ship square, set text colour to dark green
                    else if (GlobalData.playerGrid[x, i] == "[+]") { Console.ForegroundColor = ConsoleColor.DarkGreen; }
                    //Otherwise, if element is a damaged ship square, set text colour to red
                    else if (GlobalData.playerGrid[x, i] == "[!]") { Console.ForegroundColor = ConsoleColor.Red; }
                    //Otherwise, set text colour to magenta(unidentified element)
                    else { Console.ForegroundColor = ConsoleColor.Magenta; }


                    //Write out the specified element in the 2D array
                    Console.Write(GlobalData.playerGrid[x, i]);
                }
            }
        }

        //This procedure prints out the enemy grid
        public static void displayEnemyGrid()
        {
            resetDefaultFontColour();
            Console.Write("Enemy grid:\n");


            //A for loop (which runs 11 times), within a for loop (which runs 11 times) creates a table of 121 elements, fully displaying one of the three 2D arrays
            //Runs 11 times, once for each row in the 2D array
            for (int x = 0; x < 11; x++)
            {
                Console.Write("\n");
                //Runs 11 times to display every element in the table, plus one extra for displaing the row names on the left
                for (int i = 0; i < 11; i++)
                {
                    //This section produces all the relavant colours
                    //If element is a row or column name, change text colour to yellow
                    if (i == 0 || x == 0)
                    {
                        if (resetDefaultFontColour() == "Dark") { Console.ForegroundColor = ConsoleColor.Yellow; }
                        else { Console.ForegroundColor = ConsoleColor.DarkYellow; }
                    }
                    //Otherwise, if element is of unknown status, set text colour to dark gray
                    else if (GlobalData.displayEnemyGrid[x, i] == "[?]") { Console.ForegroundColor = ConsoleColor.DarkGray; }
                    //Otherwise, if element is a sea square, set text colour to blue
                    else if (GlobalData.displayEnemyGrid[x, i] == "[~]") { Console.ForegroundColor = ConsoleColor.Blue; }
                    //Otherwise, if element is a damaged ship square, set text colour to red
                    else if (GlobalData.displayEnemyGrid[x, i] == "[!]") { Console.ForegroundColor = ConsoleColor.Red; }
                    //Otherwise, set text colour to magenta(inidentified element)
                    else { Console.ForegroundColor = ConsoleColor.Magenta; }


                    //Write out the specified element in the 2D array
                    Console.Write(GlobalData.displayEnemyGrid[x, i]);
                }
            }
        }

        //(Dev Tool) This procedure prints out the enemy's actual grid
        public static void displayActualEnemyGrid()
        {
            resetDefaultFontColour();
            Console.Write("\n\nActual Enemy grid:\n");
            //A for loop (which runs 11 times), within a for loop (that runs 11 times) creates a table of 121 elements, fully displaying one of the three 2D arrays
            //Runs 11 times, once for each row in the 2D array
            for (int x = 0; x < 11; x++)
            {
                Console.Write("\n");
                //Runs 11 times to display every element in the table, plus one extra for displaing the row names on the left
                for (int i = 0; i < 11; i++)
                {
                    //If element is a row or column name, change text colour to yellow
                    if (i == 0 || x == 0)
                    {
                        if (resetDefaultFontColour() == "Dark") { Console.ForegroundColor = ConsoleColor.Yellow; }
                        else { Console.ForegroundColor = ConsoleColor.DarkYellow; }
                    }
                    //Otherwise, if element is a sea square, set text colour to blue
                    else if (GlobalData.actualEnemyGrid[x, i] == "[~]") { Console.ForegroundColor = ConsoleColor.Blue; }
                    //Otherwise, if element is an undamaged ship square, set text colour to dark green
                    else if (GlobalData.actualEnemyGrid[x, i] == "[+]") { Console.ForegroundColor = ConsoleColor.DarkGreen; }
                    //Otherwise, if element is a damaged ship square, set text colour to red
                    else if (GlobalData.actualEnemyGrid[x, i] == "[!]") { Console.ForegroundColor = ConsoleColor.Red; }
                    //Otherwise, set text colour to magenta(unidentified element)
                    else { Console.ForegroundColor = ConsoleColor.Magenta; }


                    //Write out the specified element in the 2D array
                    Console.Write(GlobalData.actualEnemyGrid[x, i]);
                }
            }

            Console.Write("\n\n");
        }







        //This procedure takes the player's input, validates it, and acts on the coordinate strike
        public static void takeAndActUserInput()
        {
            //Initialises validInput as false to enable the while loop to run
            bool validInput = false;

            //Creates a new integer for storing the inputted x-coordinate after parsing
            int xVal;


            //While the input is not valid...
            while (validInput == false)
            {
                //validInput is set to true at the beginning of the loop, and then if the input fails a check it is reset to false
                validInput = true;

                //Taking input for the x-coordinate
                resetDefaultFontColour();
                Console.Write("\n\nEnter strike coordinate X-value(integer): ");
                string XrawInput = Console.ReadLine();

                //If the input for the x-coordinate is parsable...
                if (int.TryParse(XrawInput, out xVal))
                {
                    //If the parsed x-coordinate is between 1 and 10 (i.e. it is on the grid)...
                    if (xVal > 0 && xVal < 11)
                    {
                        //The x-coordinate input has now been deemed valid and we can move on to the y coordinate

                        //Taking input for the y-coordinate
                        Console.Write("\nEnter strike coordinate Y-value(letter): ");
                        string YrawInput = Console.ReadLine();

                        //If the length of the input string is equal to one(as it is expected to be)....
                        if (YrawInput.Length == 1)
                        {
                            //Convert the input string to a character and then convert the character to its ASCII equivalent
                            int YasciiRep = (int)Convert.ToChar(YrawInput);

                            //If input is lowercase a-j instead of uppercase, convert to uppercase (using ASCII values)
                            if (Char.IsLower(Convert.ToChar(YrawInput))) { YasciiRep = YasciiRep - 32; }

                            //If the input is an uppercase character between A-J...
                            if (YasciiRep > 64 && YasciiRep < 75)
                            {
                                //Converts from character to it's relevant integer indexing value (e.g. C converts to 3)
                                int yVal = YasciiRep - 64;

                                //If the specified coordinate is one that has not yet been entered...
                                if (GlobalData.displayEnemyGrid[yVal, xVal] == "[?]")
                                {
                                    //If the coordinate in the actual grid is a sea square, change the user status report
                                    if (GlobalData.actualEnemyGrid[yVal, xVal] == "[~]") { GlobalData.userActionStatusReport = "Your last strike did not hit a target"; }
                                    //Otherwise if the coordinate in the actual grid is a ship square, change the user status report
                                    else if (GlobalData.actualEnemyGrid[yVal, xVal] == "[!]") { GlobalData.userActionStatusReport = "Your last strike damaged a target!"; }

                                    //Change the enemy display grid value to the corresponding enemy actual grid value
                                    GlobalData.displayEnemyGrid[yVal, xVal] = GlobalData.actualEnemyGrid[yVal, xVal];
                                }
                                else { validInput = false; }
                            }
                            else { validInput = false; }
                        }
                        else if (YrawInput.ToLower() == "surrender" || YrawInput.ToLower() == "end" || YrawInput.ToLower() == "exit") { GlobalData.surrender = true; }
                        else { validInput = false; }
                    }
                    else { validInput = false; }
                }
                //Otherwise if the user decides to surrender...
                else if (XrawInput.ToLower() == "surrender" || XrawInput.ToLower() == "end" || XrawInput.ToLower() == "exit") { GlobalData.surrender = true; }
                else { validInput = false; }

                //If the input was invalid, show error message
                if (validInput == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    typeWriter("Invalid Input", 10);
                    resetDefaultFontColour();
                }
            }
        }






        //This function searches for strikes that have already been successful, then checks adjacent squares in a clockwise motion to see if they have been fired on yet
        //If it finds a valid coordinate adjacent to a square that has already been struck, it returns the coordinate of the square in an integer array
        public static int[] strikeScanCore()
        {
            int[] returnCoordinates = new int[2];

            //Nested for loops cycle through every element in the player's grid
            for (int y = 1; y < 11; y++)
            {
                for (int x = 1; x < 11; x++)
                {
                    //If the location in question has already has a successful strike...
                    if (GlobalData.playerGrid[y, x] == "[!]")
                    {
                        //Check if the square above has been struck yet...
                        if (y >= 2 && GlobalData.playerGrid[y - 1, x] != "[!]" && GlobalData.playerGrid[y - 1, x] != "[M]")
                        {
                            //If not, return the coordinates of that square
                            returnCoordinates[0] = y - 1;
                            returnCoordinates[1] = x;

                            return returnCoordinates;
                        }
                        //Else check is the square to the right has been struck yet
                        if (x <= 9 && GlobalData.playerGrid[y, x + 1] != "[!]" && GlobalData.playerGrid[y, x + 1] != "[M]")
                        {
                            //If not, return the coordinates of that square
                            returnCoordinates[0] = y;
                            returnCoordinates[1] = x + 1;

                            return returnCoordinates;
                        }
                        //Else check is the square below has been struck yet
                        if (y <= 9 && GlobalData.playerGrid[y + 1, x] != "[!]" && GlobalData.playerGrid[y + 1, x] != "[M]")
                        {
                            //If not, return the coordinates of that square
                            returnCoordinates[0] = y + 1;
                            returnCoordinates[1] = x;

                            return returnCoordinates;
                        }
                        //Else check if the square to the left has been struck yet
                        if (x >= 2 && GlobalData.playerGrid[y, x - 1] != "[!]" && GlobalData.playerGrid[y, x - 1] != "[M]")
                        {
                            //If not, return the coordinates of that square
                            returnCoordinates[0] = y;
                            returnCoordinates[1] = x - 1;

                            return returnCoordinates;
                        }
                    }
                }
            }

            return null;
        }

        //This function allows veteranEnemyStrike to use the linear nature of each battleship to its advantage, as it uses the surroundings to predict locations of other ship squares
        public static int[] strikeScanAdvanced()
        {
            int[] returnCoordinates = new int[2];

            //Intelligent prediction imitates humans and allows the program to understand linear patterns(unlike strikeScanCore which runs a prebaked surroundings search on every hit)
            //Nested for loops cycle through every coordinate in the player's grid
            for (int y = 1; y < 11; y++)
            {
                for (int x = 1; x < 11; x++)
                {
                    //If the select coordinate was a successful strike...
                    if (GlobalData.playerGrid[y, x] == "[!]")
                    {
                        //If the square above the select coordinate was a successful strike, and the square below has not been struck...
                        if (y > 1 && y < 10 && GlobalData.playerGrid[y - 1, x] == "[!]" && GlobalData.playerGrid[y + 1, x] != "[!]" && GlobalData.playerGrid[y + 1, x] != "[M]")
                        {
                            //Then strike the square below
                            returnCoordinates[0] = y + 1;
                            returnCoordinates[1] = x;

                            return returnCoordinates;
                        }
                        //If the square below the select coordinate was a successful strike, and the square above has not been struck
                        if (y > 1 && y < 10 && GlobalData.playerGrid[y + 1, x] == "[!]" && GlobalData.playerGrid[y - 1, x] != "[!]" && GlobalData.playerGrid[y - 1, x] != "[M]")
                        {
                            ///Then strike the square above
                            returnCoordinates[0] = y - 1;
                            returnCoordinates[1] = x;

                            return returnCoordinates;
                        }
                        //If the square to the left of the select coordinate was a successful strike, and the square to the right has not been struck
                        if (x > 1 && x < 10 && GlobalData.playerGrid[y, x - 1] == "[!]" && GlobalData.playerGrid[y, x + 1] != "[!]" && GlobalData.playerGrid[y, x + 1] != "[M]")
                        {
                            //Then strike the square to the right
                            returnCoordinates[0] = y;
                            returnCoordinates[1] = x + 1;

                            return returnCoordinates;
                        }
                        //If the square to the right of the select coordinate was a successful strike, and the square to the left has not been struck
                        if (x > 1 && x < 10 && GlobalData.playerGrid[y, x + 1] == "[!]" && GlobalData.playerGrid[y, x - 1] != "[!]" && GlobalData.playerGrid[y, x - 1] != "[M]")
                        {
                            //Then strike the one to the left
                            returnCoordinates[0] = y;
                            returnCoordinates[1] = x - 1;

                            return returnCoordinates;
                        }
                    }
                }
            }


            //This section fires on squares adjacent to those that have valid hits PROVIDED THAT none of the adjacent squares have valid hits yet
            //Nested for loops cycle through every element in the player's grid
            for (int y = 1; y < 11; y++)
            {
                for (int x = 1; x < 11; x++)
                {
                    //If the location in question has already has a successful strike...
                    if (GlobalData.playerGrid[y, x] == "[!]")
                    {
                        bool valid = true;

                        //If one of the surrounding squares has a valid hit, set valid to false
                        if (y > 1 && GlobalData.playerGrid[y - 1, x] == "[!]") { valid = false; }
                        else { Console.WriteLine("Catch 1"); }
                        if (x < 10 && GlobalData.playerGrid[y, x + 1] == "[!]") { valid = false; }
                        else { Console.WriteLine("Catch 2"); }
                        if (y < 10 && GlobalData.playerGrid[y + 1, x] == "[!]") { valid = false; }
                        else { Console.WriteLine("Catch 3"); }
                        if (x > 1 && GlobalData.playerGrid[y, x - 1] == "[!]") { valid = false; }
                        else { Console.WriteLine("Catch 4"); }

                        //If none of the surrounding square have been hit...
                        if (valid == true)
                        {
                            //...and the square above has not been fired on...
                            if (y > 1 && GlobalData.playerGrid[y - 1, x] != "[M]")
                            {
                                //Then strike the square above
                                returnCoordinates[0] = y - 1;
                                returnCoordinates[1] = x;

                                return returnCoordinates;
                            }
                            //...and the square to the right has not been fired on...
                            else if (x < 10 && GlobalData.playerGrid[y, x + 1] != "[M]")
                            {
                                //Then strike the square to the right
                                returnCoordinates[0] = y;
                                returnCoordinates[1] = x + 1;

                                return returnCoordinates;
                            }
                            //...and the square below has not been fired on...
                            else if (y < 10 && GlobalData.playerGrid[y + 1, x] != "[M]")
                            {
                                //Then strike the square below
                                returnCoordinates[0] = y + 1;
                                returnCoordinates[1] = x;

                                return returnCoordinates;
                            }
                            //...and the square to the left has not been fired on...
                            else if (x > 1 && GlobalData.playerGrid[y, x - 1] != "[M]")
                            {
                                //Then strike the square to the left
                                returnCoordinates[0] = y;
                                returnCoordinates[1] = x - 1;

                                return returnCoordinates;
                            }
                        }
                    }
                }
            }


            //If niehter section returns a result, return null
            return null;

        }






        //This function allows veteranEnemyStrike to search for ships in an efficient pattern instead of randomly choosing coordinates
        //It returns the coordinates of the next square to be struck through an integer array
        public static int[] structuredSearch()
        {
            //Loop runs indefinitely...
            while (true)
            {
                //If structured searching isn't finished..
                if (GlobalData.structFinished == false)
                {
                    //AND If the offset hasn't begun..
                    if (GlobalData.offsetStart == false)
                    {
                        //AND If coordinates are [9, 9] AND the square [9, 9] has been struck, set offsetStart to true
                        if (GlobalData.structuredCoordinates[0] == 9 && GlobalData.structuredCoordinates[1] == 9)
                        {
                            if (GlobalData.playerGrid[9, 9] == "[M]" || GlobalData.playerGrid[9, 9] == "[!]") { GlobalData.offsetStart = true; }
                            //Otherwise return coordinates as per usual
                            else { return GlobalData.structuredCoordinates; }
                        }
                        else
                        {
                            //If the select square has not been struck, return coordinates
                            if (GlobalData.playerGrid[GlobalData.structuredCoordinates[0], GlobalData.structuredCoordinates[1]] != "[M]" && GlobalData.playerGrid[GlobalData.structuredCoordinates[0], GlobalData.structuredCoordinates[1]] != "[!]") { return GlobalData.structuredCoordinates; }
                            //Otherwise, if the select square has been struck...
                            else
                            {
                                //If coordinate x value is 9...
                                if (GlobalData.structuredCoordinates[1] == 9)
                                {
                                    //Set x value equal to 1 and y value equal to y+2
                                    GlobalData.structuredCoordinates[0] = GlobalData.structuredCoordinates[0] + 2;
                                    GlobalData.structuredCoordinates[1] = 1;
                                }
                                //Otherwise increase x value by 2
                                else { GlobalData.structuredCoordinates[1] = GlobalData.structuredCoordinates[1] + 2; }
                            }

                        }
                    }
                    //AND If the offset has begun
                    if (GlobalData.offsetStart == true)
                    {
                        //ANDIf coordinates are [10, 10] AND the square [10, 10] has been struck, set structFinished to true
                        if (GlobalData.structuredCoordinatesOffset[0] == 10 && GlobalData.structuredCoordinatesOffset[1] == 10)
                        {
                            if (GlobalData.playerGrid[10, 10] == "[M]" || GlobalData.playerGrid[10, 10] == "[!]") { GlobalData.structFinished = true; }
                            //Othersiwe return values as normal
                            else { return GlobalData.structuredCoordinatesOffset; }
                        }
                        else
                        {
                            //If the select square has not been struck...
                            if (GlobalData.playerGrid[GlobalData.structuredCoordinatesOffset[0], GlobalData.structuredCoordinatesOffset[1]] != "[M]" && GlobalData.playerGrid[GlobalData.structuredCoordinatesOffset[0], GlobalData.structuredCoordinatesOffset[1]] != "[!]")
                            {
                                return GlobalData.structuredCoordinatesOffset;
                            }
                            //If the select square has been struck...
                            else
                            {
                                //If coordinate x value is 9...
                                if (GlobalData.structuredCoordinatesOffset[1] == 10)
                                {
                                    //Set x value equal to 1 and y value equal to y+2
                                    GlobalData.structuredCoordinatesOffset[0] = GlobalData.structuredCoordinatesOffset[0] + 2;
                                    GlobalData.structuredCoordinatesOffset[1] = 2;
                                }
                                //Otherwise increase x value by 2
                                else { GlobalData.structuredCoordinatesOffset[1] = GlobalData.structuredCoordinatesOffset[1] + 2; }
                            }

                        }
                    }
                }
                //If structured searching is finished
                if (GlobalData.structFinished == true)
                {
                    return null;
                }
            }
        }






        //These functions all act on strikes and return action status messages
        //This function randomly generates a valid strike coordinate with no intelligence
        public static string recruitEnemyStrike()
        {
            //Runs indefinitely until a value is returned (i.e. until a valid coordinate is generated)
            while (true)
            {
                //Set two integers(corresponding to x and y coordinate values) equal to a random numbers between 1 and 10
                int yPos = GlobalData.rand.Next(1, 11);
                int xPos = GlobalData.rand.Next(1, 11);

                //If the coordinate is a sea square (i.e. it hasn't been hit yet)...
                if (GlobalData.playerGrid[yPos, xPos] == "[~]")
                {
                    GlobalData.playerGrid[yPos, xPos] = "[M]";
                    return "No torpedo strike detected.";
                }
                //Otherwise, if the coordinate is an undamaged ship square (i.e. it hasnt been hit yet)...
                else if (GlobalData.playerGrid[yPos, xPos] == "[+]")
                {
                    GlobalData.playerGrid[yPos, xPos] = "[!]";
                    return "Enemy torpedo strike damaged one of your ships!";
                }
            }
        }

        //This function is fairly intelligent, as after a successful strike as it starts searching around the strike for other ship squares
        public static string experiencedEnemyStrike()
        {
            bool strikeValid = false;
            int xPos;
            int yPos;


            while (strikeValid != true)
            {
                if (strikeScanCore() == null)
                {
                    yPos = GlobalData.rand.Next(1, 11);
                    xPos = GlobalData.rand.Next(1, 11);
                }
                else
                {
                    int[] returnedCoordinates = strikeScanCore();

                    yPos = returnedCoordinates[0];
                    xPos = returnedCoordinates[1];
                }

                if (GlobalData.playerGrid[yPos, xPos] == "[~]")
                {
                    GlobalData.playerGrid[yPos, xPos] = "[M]";
                    strikeValid = true;
                    return "Last enemy strike missed.";
                }
                else if (GlobalData.playerGrid[yPos, xPos] == "[+]")
                {
                    GlobalData.playerGrid[yPos, xPos] = "[!]";
                    strikeValid = true;
                    return "Last enemy strike damaged one of your ships!";
                }
            }

            return null;

        }

        //This function is highly intelligent as it is capable of prioritising striking in a linear sequence, and has an effective vessel searching pattern (as opposed to guessing randomly until it scores a hit like recruitEnemyStrike)
        public static string veteranEnemyStrike()
        {
            int xPos = 10;
            int yPos = 10;
            bool strikeValid = false;

            while (!strikeValid)
            {
                //If strikeScanAdvanced returns a value...
                if (strikeScanAdvanced() != null)
                {
                    int[] returnedCoordinates = strikeScanAdvanced();

                    yPos = returnedCoordinates[0];
                    xPos = returnedCoordinates[1];
                }
                //Otherwise if structuredSearch returns a value...
                else if (structuredSearch() != null)
                {
                    //Continue structured search
                    int[] returnedCoordinates1 = structuredSearch();

                    yPos = returnedCoordinates1[0];
                    xPos = returnedCoordinates1[1];
                }
                //Otherwise generate a random number
                else
                {
                    yPos = GlobalData.rand.Next(1, 11);
                    xPos = GlobalData.rand.Next(1, 11);
                }




                if (GlobalData.playerGrid[yPos, xPos] != "[M]" && GlobalData.playerGrid[yPos, xPos] != "[!]")
                {
                    strikeValid = true;
                }
            }


            if (GlobalData.playerGrid[yPos, xPos] == "[~]")
            {
                GlobalData.playerGrid[yPos, xPos] = "[M]";
                return "Last enemy strike missed.";
            }
            else if (GlobalData.playerGrid[yPos, xPos] == "[+]")
            {
                GlobalData.playerGrid[yPos, xPos] = "[!]";
                return "Last enemy strike damaged one of your ships!";
            }
            else { return null; }
        }






        //This function returns the number of successful player strikes on the enemy
        public static int nPlayerStrikesSuccessful()
        {
            //Int to keep tally of no. hits
            int numberOfHits = 0;

            //Nested for loops cycle through every coordinate in the enemy's grid
            for (int y = 1; y < 11; y++)
            {
                for (int x = 1; x < 11; x++)
                {
                    //If it detects a hit square, it adds one to the hit tally
                    if (GlobalData.displayEnemyGrid[y, x] == "[!]") { numberOfHits++; }
                }
            }

            return numberOfHits;
        }

        //This function returns the number of successful enemy strikes on the player
        public static int nEnemyStrikesSuccessful()
        {
            //Int to keep tally of no. hits
            int numberOfHits = 0;

            //Nested for loops cycle through every coordinate in the player's grid
            for (int y = 1; y < 11; y++)
            {
                for (int x = 1; x < 11; x++)
                {
                    //If it detects a hit square, it adds one to the hit tally
                    if (GlobalData.playerGrid[y, x] == "[!]") { numberOfHits++; }
                }
            }

            return numberOfHits;
        }






        //Function to set a random text colour (pretty clunky code, but it works so meh)
        public static void randForegroundColor()
        {
            //Generates random number
            int randomNumber1 = GlobalData.rand.Next(0, 15);
            //Uses random number to choose a colour
            if (randomNumber1 == 0) { Console.ForegroundColor = ConsoleColor.Yellow; }
            if (randomNumber1 == 1) { Console.ForegroundColor = ConsoleColor.Blue; }
            if (randomNumber1 == 2) { Console.ForegroundColor = ConsoleColor.Cyan; }
            if (randomNumber1 == 3) { Console.ForegroundColor = ConsoleColor.DarkBlue; }
            if (randomNumber1 == 4) { Console.ForegroundColor = ConsoleColor.DarkCyan; }
            if (randomNumber1 == 5) { Console.ForegroundColor = ConsoleColor.DarkGray; }
            if (randomNumber1 == 6) { Console.ForegroundColor = ConsoleColor.DarkGreen; }
            if (randomNumber1 == 7) { Console.ForegroundColor = ConsoleColor.DarkMagenta; }
            if (randomNumber1 == 8) { Console.ForegroundColor = ConsoleColor.DarkRed; }
            if (randomNumber1 == 9) { Console.ForegroundColor = ConsoleColor.DarkYellow; }
            if (randomNumber1 == 10) { Console.ForegroundColor = ConsoleColor.Gray; }
            if (randomNumber1 == 11) { Console.ForegroundColor = ConsoleColor.Green; }
            if (randomNumber1 == 12) { Console.ForegroundColor = ConsoleColor.Magenta; }
            if (randomNumber1 == 13) { Console.ForegroundColor = ConsoleColor.Red; }
            if (randomNumber1 == 14) { resetDefaultFontColour(); }

            //Only 15 colours because black would just make the title invisible obviously

        }

        //Procedure imported from one of my other projects - simply adds a slight delay beween each letter being printed to the console to give the game a retro vibe
        public static void typeWriter(string input, [Optional] int? charDelay)
        {
            //If a delay has not been specified, set to 25ms
            if (charDelay.HasValue != true) { charDelay = 25; }

            //Converts input string into a char array
            char[] inputSplice = input.ToCharArray();

            //Variable used to cycle through each letter in the string
            int charLoopNum = 0;

            //Runs for every character in the spliced array
            foreach (char C in inputSplice)
            {
                //Writes each character in the array individually every time the loop restarts
                Console.Write(inputSplice[charLoopNum]);
                //Waits 25 milliseconds
                System.Threading.Thread.Sleep(charDelay.Value);
                charLoopNum++;
            }
        }

        public static string resetDefaultFontColour()
        {
            if (GlobalData.darkMode == "Off")
            {
                Console.ForegroundColor = ConsoleColor.Black;
                return "Light";
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                return "Dark";
            }
        }




        //Not sure exactly what this section does, but it has something to do with disabling window resizing
        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_SIZE = 0xF000;
        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();






        static void Main(string[] args)
        {
            //Not sure exactly what this bit does, but it is part of the code that disables window resizing
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);
            //Stops the user from resizing the window
            if (handle != IntPtr.Zero) { DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND); }


            //Bool to check if the user wants to end the program
            bool terminateProgram = false;

            //While the user decides not to end the program
            while (terminateProgram == false)
            {
                //Initialises variable 'option' to 0
                int option = 0;

                //Clears the console
                Console.Clear();

                //Sets the window title
                Console.Title = "Battleships";

                //Sets the size of the console window
                //Console.SetWindowSize(115, 25);

                //Title
                //Sets the title to a random colour
                randForegroundColor();

                Console.WriteLine(".______        ___   .___________. .___________. __       _______     _______. __    __   __  .______     _______.");
                Console.WriteLine("|   _  \\      /   \\  |           | |           ||  |     |   ____|   /       ||  |  |  | |  | |   _  \\   /       |");
                Console.WriteLine("|  |_)  |    /  ^  \\ `---|  |----` `---|  |----`|  |     |  |__     |   (----`|  |__|  | |  | |  |_)  | |   (----`");
                Console.WriteLine("|   _  <    /  /_\\  \\    |  |          |  |     |  |     |   __|     \\   \\    |   __   | |  | |   ___/   \\   \\    ");
                Console.WriteLine("|  |_)  |  /  _____  \\   |  |          |  |     |  `----.|  |____.----)   |   |  |  |  | |  | |  |   .----)   |   ");
                Console.WriteLine("| ______/ /__/     \\__\\  |__|          |__|     |_______||_______|_______/    |__|  |__| |__| | _|   |_______/    ");

                resetDefaultFontColour();

                //Prints the patch number
                Console.Write("\t\t\t\t\t\t\t\t\t\t\t\t\t    [" + GlobalData.patchNo + "]");

                Console.Write("\n\n\n\n\n\t\t\t\t\t\t1 - Story Mode[UD]" +
                    "\n\t\t\t\t\t\t2 - Quick Match" +
                    "\n\t\t\t\t\t\t3 - Difficulty" +
                    "\n\t\t\t\t\t\t4 - How to Play" +
                    "\n\t\t\t\t\t\t5 - Credits and Info" +
                    "\n\t\t\t\t\t\t6 - Patch Notes" +
                    "\n\t\t\t\t\t\t7 - Settings" +
                    "\n\t\t\t\t\t\t8 - Load Attack Sims[UD]" +
                    "\n\t\t\t\t\t\t9 - Exit game" +
                    "\n\n\t\t\t\t\t\t\t");

                //Snippet from my Hangman code: basically takes input and sets two variables - 1) Option number[int] & 2) Is input valid?[bool]
                int.TryParse(Console.ReadKey().KeyChar.ToString(), out option);


                //9 - End program
                if (option == 9) { terminateProgram = true; }
                //7 - Settings
                else if (option == 7)
                {
                    //Sets size of console window
                    //Console.SetWindowSize(60, 20);
                    //Sets the window title
                    Console.Title = "Battleships - Settings";

                    bool exit = false;

                    while (exit != true)
                    {
                        //Clears the console window
                        Console.Clear();

                        resetDefaultFontColour();

                        //Deals with all toggles
                        string devModeToggle;
                        if (GlobalData.devMode == "Off") { devModeToggle = "on"; }
                        else { devModeToggle = "off"; }

                        string darkModeToggle;
                        if (GlobalData.darkMode == "Off") { darkModeToggle = "on"; }
                        else { darkModeToggle = "off"; }

                        string autoGenModeToggle;
                        if (GlobalData.autoGenMode == "Off") { autoGenModeToggle = "on"; }
                        else { autoGenModeToggle = "off"; }

                        //Displayed text
                        Console.Write("Enter the corresponding number to select command:\n\n\t\t");
                        Console.Write("1 - Turn Developer Mode ");

                        if (devModeToggle == "on") { Console.ForegroundColor = ConsoleColor.Green; }
                        else { Console.ForegroundColor = ConsoleColor.Red; }
                        Console.Write(devModeToggle);
                        resetDefaultFontColour();

                        Console.Write("\n\t\t2 - Turn Dark Mode ");

                        if (darkModeToggle == "on") { Console.ForegroundColor = ConsoleColor.Green; }
                        else { Console.ForegroundColor = ConsoleColor.Red; }
                        Console.Write(darkModeToggle);
                        resetDefaultFontColour();

                        Console.Write("\n\t\t3 - Turn player fleet autogeneration ");

                        if (autoGenModeToggle == "on") { Console.ForegroundColor = ConsoleColor.Green; }
                        else { Console.ForegroundColor = ConsoleColor.Red; }
                        Console.Write(autoGenModeToggle);
                        resetDefaultFontColour();

                        Console.Write("\n\t\t4 - Exit Settings\n\n", 1);

                        bool validSettingInput = false;
                        int choiceNumber = 0;

                        while (validSettingInput == false)
                        {
                            validSettingInput = int.TryParse(Convert.ToString(Console.ReadKey().KeyChar), out choiceNumber);

                            //If the input is an integer between 1 and 3...
                            if (validSettingInput && choiceNumber >= 1 && choiceNumber <= 4)
                            {
                                //Change setting according to input
                                if (choiceNumber == 1)
                                {
                                    if (GlobalData.devMode == "Off") { GlobalData.devMode = "On"; }
                                    else { GlobalData.devMode = "Off"; }
                                }
                                else if (choiceNumber == 2)
                                {
                                    if (GlobalData.darkMode == "Off")
                                    {
                                        GlobalData.darkMode = "On";
                                        Console.BackgroundColor = ConsoleColor.Black;
                                    }
                                    else
                                    {
                                        GlobalData.darkMode = "Off";
                                        Console.BackgroundColor = ConsoleColor.White;
                                    }
                                }
                                else if (choiceNumber == 3)
                                {
                                    if (GlobalData.autoGenMode == "Off") { GlobalData.autoGenMode = "On"; }
                                    else { GlobalData.autoGenMode = "Off"; }
                                }
                                else if (choiceNumber == 4) { exit = true; }
                            }
                            //Otherwise show error message
                            else
                            {
                                validSettingInput = false;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("\b \n Error: Invalid input. Make sure to enter a valid integer.\n\n\t\t\t\t");
                                resetDefaultFontColour();
                            }

                        }
                    }

                }
                //6 - Patch Notes
                else if (option == 6)
                {
                    //Clear the console
                    Console.Clear();
                    //Sets the window title
                    Console.Title = "Battleships - Patch Notes";

                    //Look in '\\strs\dfs\Devs\data\18SWADKKs\Projects\# Complete\Battleships' for patch notes
                    //Prints evertything in a specific text file which details the patch notes
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    typeWriter("PATCH NOTES:\n\n");
                    resetDefaultFontColour();
                    typeWriter(System.IO.File.ReadAllText(@"C:\Users\kshit\OneDrive\Documents\Visual Studio 2022\Projects\C#\Console Applications\Battleships\Patch Notes.txt") + "\n\n\nPress any button to return to menu:", 2);
                    Console.ReadKey();
                }
                //5 - Credits and Info
                else if (option == 5)
                {
                    //Clear the console
                    Console.Clear();
                    //Sets the size of console window
                    //Console.SetWindowSize(98, 25);
                    //Sets the window title
                    Console.Title = "Battleships - Credits and Information";

                    typeWriter("\n\n\n   This program is a computer adaptation of the classic strategy guessing game 'Battleships'.\n\n\n\n\nThe original game dates back to World War 1, thus this fun activity has over 100 years of history.\n\n\n\n\n     This adaptation was developed in 2022-2023 by Kshitij Wadkar as a C# coding project.\n\n\n\n\n\n\t\t\tIt is free to use and spin-off by any individual.\n\n\n\t\t\t\t\t     ", 10);
                    Console.ReadKey();
                }
                //4 - How to Play
                else if (option == 4)
                {
                    //Clear the console
                    Console.Clear();
                    //Sets the window title
                    Console.Title = "Battleships - How to Play";
                    //Sets size of console window
                    //Console.SetWindowSize(150, 40);

                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    typeWriter("OBJECTIVE: Be the first to sink all 5 of your opponent's ships\n\n", 1);

                    resetDefaultFontColour();
                    Console.Write("- When you start the game, you will see two 10x10 grids displayed, with the X-axes labelled in numbers and Y-axes labelled in letters\n" +
                        "- These grids represent two boards and the axes can be used to divide the boards into 100 distinct coordinates\n" +
                        "- The grid at the bottom represents your board\n" +
                        "- You will notice most of your board is covered with blue squiggles[~] - These represent sea squares\n" +
                        "- You will also notice several lines (varying length) of green crosses[+] scattered across the board\n" +
                        "- Each line of green crosses represents a different ship\n" +
                        "- There are 5 ships on your board in total, with lengths between 2 and 6 crosses long\n" +
                        "- The enemy also has an identical board to yours, but with ships in different locations\n" +
                        "- The top board represents your copy of the enemy's board\n" +
                        "- You will notice that it covered in grey question marks, as you do not know where the enemy's ships are(and likewise for the enemy)\n" +
                        "- Your job is to fire torpedos onto the enemy's board (specifying the location using the coordinates) to find and sink every one of their ships\n" +
                        "- A successful hit is shown on the top grid as a red exclamation mark[!]\n" +
                        "- To sink a ship, you must hit it in every square it is present in, not just one\n" +
                        "- However, the enemy is doing the exact same thing as you\n" +
                        "- After every torpedo you fire, the enemy fires one back\n" +
                        "- If the enemy hits one of your ships, it is shown as a red exclamation mark[!]\n" +
                        "- However, if they miss, it is shown as a cyan M[M]\n" +
                        "- Therefore, to win the game you must sink all of the enemy's ships before they sink yours.\n\n\nMARKERS:\n");

                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    typeWriter("[?]", 1);
                    resetDefaultFontColour();
                    typeWriter(" = Unknown\n", 1);

                    Console.ForegroundColor = ConsoleColor.Blue;
                    typeWriter("[~]", 1);
                    resetDefaultFontColour();
                    typeWriter(" = Sea\n", 1);

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    typeWriter("[M]", 1);
                    resetDefaultFontColour();
                    typeWriter(" = Missed Enemy Strike\n", 1);

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    typeWriter("[+]", 1);
                    resetDefaultFontColour();
                    typeWriter(" = Undamaged ship section\n", 1);

                    Console.ForegroundColor = ConsoleColor.Red;
                    typeWriter("[!]", 1);
                    resetDefaultFontColour();
                    typeWriter(" = Hit(Damaged ship section)\n\n\n", 1);

                    typeWriter("NOTES:\n" +
                        "> Typing 'surrender', 'exit', or 'end' when prompted to enter x-coordinates instantly ends the game (in a defeat)\n" +
                        "> DevMode shows the full enemy grid, including ship locations(to be used for debugging)\n" +
                        "> Ships will never generate adjacent to one another\n\n\n", 1);

                    Console.ReadKey();
                }
                //3 - Difficulty
                else if (option == 3)
                {
                    //Clears the console
                    Console.Clear();
                    //Sets size of console window
                    //Console.SetWindowSize(60, 20);
                    //Sets the window title
                    Console.Title = "Battleships - Change Difficulty";

                    typeWriter("Note: Difficulty level only affects Quick Match\n\nCurrent difficulty setting: ", 5);

                    //Changes colour depending on current setting
                    if (GlobalData.aiDifficulty == "Recruit") { Console.ForegroundColor = ConsoleColor.Green; }
                    else if (GlobalData.aiDifficulty == "Experienced") { Console.ForegroundColor = ConsoleColor.DarkYellow; }
                    else if (GlobalData.aiDifficulty == "Veteran") { Console.ForegroundColor = ConsoleColor.Red; }

                    typeWriter(GlobalData.aiDifficulty + "\n", 5);

                    resetDefaultFontColour();
                    typeWriter("Enter the corresponding number to change difficulty:\n", 5);


                    typeWriter("\n\t\t\t1 - ", 5);
                    Console.ForegroundColor = ConsoleColor.Green;
                    typeWriter("Recruit", 5);

                    resetDefaultFontColour();
                    typeWriter("\n\t\t\t2 - ", 5);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    typeWriter("Experienced", 5);

                    resetDefaultFontColour();
                    typeWriter("\n\t\t\t3 - ", 5);
                    Console.ForegroundColor = ConsoleColor.Red;
                    typeWriter("Veteran\n\n\t\t\t\t", 5);
                    resetDefaultFontColour();

                    bool validDifficultyInput = false;
                    int throwawayInt = 0;

                    while (validDifficultyInput == false)
                    {
                        validDifficultyInput = int.TryParse(Convert.ToString(Console.ReadKey().KeyChar), out throwawayInt);

                        //If the input is an integer between 1 and 3...
                        if (validDifficultyInput && throwawayInt > 0 && throwawayInt < 4)
                        {
                            //Set the difficulty according to the input number
                            if (throwawayInt == 1) { GlobalData.aiDifficulty = "Recruit"; }
                            else if (throwawayInt == 2) { GlobalData.aiDifficulty = "Experienced"; }
                            else if (throwawayInt == 3) { GlobalData.aiDifficulty = "Veteran"; }
                        }
                        //Otherwise show error message
                        else
                        {
                            validDifficultyInput = false;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("\b \n Error: Invalid input. Make sure to enter a valid integer.\n\n\t\t\t\t");
                            resetDefaultFontColour();
                        }

                    }
                }
                //2 - Quick Match
                else if (option == 2)
                {
                    //Sets the size of console window
                    //Console.SetWindowSize(80, 40);

                    //Resets all grids
                    populateDatabase(1);
                    populateDatabase(2);
                    populateDatabase(3);

                    bool gameOver = false;

                    //Generates player and enemy's fleets
                    if (GlobalData.autoGenMode == "On") { autoGeneratePlayerFleet(); }
                    else { gameOver = !manualGeneratePlayerFleet(); }

                    if (gameOver == true) { GlobalData.surrender = true; }

                    generateEnemyFleet();

                    //Sets the window title
                    Console.Title = "Battleships - " + GlobalData.aiDifficulty;

                    //Define and start stopwatch
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    //While the game is not over...
                    while (!gameOver)
                    {
                        //Clears console to refresh screen
                        Console.Clear();


                        //If devMode is on, print the enemy's actual grid
                        if (GlobalData.devMode == "On") { displayActualEnemyGrid(); }


                        //Prints out the Enemy Grid
                        displayEnemyGrid();
                        //Prints out the Player Grid
                        displayPlayerGrid();

                        //Status report(action reports and hit totals)
                        resetDefaultFontColour();
                        Console.Write("\n\nSTATUS REPORT:\n");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write("Player Action: " + GlobalData.userActionStatusReport + "\nEnemy Action: " + GlobalData.enemyActionStatusReport);

                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("\nNumber of strikes on enemy vessels: " + nPlayerStrikesSuccessful() + "/20" + "\nNumber of strikes on friendly vessels: " + nEnemyStrikesSuccessful() + "/20");


                        //Takes and acts on user input
                        takeAndActUserInput();

                        //If the total number of player or enemy hits is 20(i.e. all of somebody's ships have been sunk), gameover is set to true
                        if (nPlayerStrikesSuccessful() == 20 || nEnemyStrikesSuccessful() == 20 || GlobalData.surrender == true) { gameOver = true; }
                        //Otherwise if the game is not over...
                        else
                        {
                            //Strike the player depending on the difficulty level
                            if (GlobalData.aiDifficulty == "Recruit") { GlobalData.enemyActionStatusReport = recruitEnemyStrike(); }
                            else if (GlobalData.aiDifficulty == "Experienced") { GlobalData.enemyActionStatusReport = experiencedEnemyStrike(); }
                            else if (GlobalData.aiDifficulty == "Veteran") { GlobalData.enemyActionStatusReport = veteranEnemyStrike(); }
                        }
                        //If the total number of enemy hits is 20, gameOver is set to true (this bit is repeated in case the enemy's last strike won the game)
                        if (nEnemyStrikesSuccessful() == 20) { gameOver = true; }
                    }

                    //Clears the console
                    Console.Clear();

                    //Resets global variables to initial values
                    GlobalData.userActionStatusReport = "Nothing to report";
                    GlobalData.enemyActionStatusReport = "Nothing to report";

                    GlobalData.structuredCoordinates[0] = 1;
                    GlobalData.structuredCoordinates[1] = 1;

                    GlobalData.structuredCoordinatesOffset[0] = 2;
                    GlobalData.structuredCoordinatesOffset[1] = 2;

                    GlobalData.offsetStart = false;
                    GlobalData.structFinished = false;

                    //Stops the Stopwatch
                    stopWatch.Stop();

                    //Sets a TimeSpan variable which records elapsed time in the stopwatch
                    TimeSpan ts = stopWatch.Elapsed;
                    //Formats ts into a string and then outputs to console
                    string elapsedTime = string.Format("{0} minutes, {1:00} seconds", ts.Minutes, ts.Seconds);

                    //If the player won...
                    if (nPlayerStrikesSuccessful() == 20)
                    {
                        //Victory screen
                        //Console.SetWindowSize(80, 30);
                        //Sets the window title
                        Console.Title = "Battleships - " + GlobalData.aiDifficulty + " - Victory";


                        Console.ForegroundColor = ConsoleColor.Green;
                        typeWriter("____    ____  __    ______ .___________.  ______   .______     ____    ____  __  \n" +
                        "\\   \\  /   / |  |  /      ||           | /  __  \\  |   _  \\    \\   \\  /   / |  | \n" +
                        " \\   \\/   /  |  | |  ,----'`---|  |----`|  |  |  | |  |_)  |    \\   \\/   /  |  | \n" +
                        "  \\      /   |  | |  |         |  |     |  |  |  | |      /      \\_    _/   |  | \n" +
                        "   \\    /    |  | |  `----.    |  |     |  `--'  | |  |\\  \\----.   |  |     |__| \n" +
                        "    \\__/     |__|  \\______|    |__|      \\______/  | _| `._____|   |__|     (__) \n\n\n\n", 1);


                        resetDefaultFontColour();
                        displayEnemyGrid();

                        Console.ForegroundColor = ConsoleColor.Green;
                        typeWriter("\n\n\n\n20 confirmed torpedo strikes on OPFOR vessels.\nEnemy fleet destroyed in " + elapsedTime + " - well done.", 1);
                    }
                    //Otherwise if the enemy won...
                    else if (nEnemyStrikesSuccessful() == 20)
                    {
                        //Defeat screen
                        //Console.SetWindowSize(70, 30);
                        //Sets the window title
                        Console.Title = "Battleships - " + GlobalData.aiDifficulty + " - Defeat";


                        Console.ForegroundColor = ConsoleColor.Red;
                        typeWriter(" _______   _______  _______  _______     ___   .___________.         \n" +
                        "|       \\ |   ____||   ____||   ____|   /   \\  |           |         \n" +
                        "|  .--.  ||  |__   |  |__   |  |__     /  ^  \\ `---|  |----`         \n" +
                        "|  |  |  ||   __|  |   __|  |   __|   /  /_\\  \\    |  |              \n" +
                        "|  '--'  ||  |____ |  |     |  |____ /  _____  \\   |  |     __ __ __ \n" +
                        "|_______/ |_______||__|     |_______/__/     \\__\\  |__|    (__|__|__)\n\n\n\n", 1);

                        resetDefaultFontColour();
                        displayPlayerGrid();


                        Console.ForegroundColor = ConsoleColor.Red;
                        typeWriter("\n\n\n\nFriendly forces destroyed in " + elapsedTime + ".\nRegroup and plan your next attack...", 1);
                    }
                    //Otherwise if the player surrendered...
                    else if (GlobalData.surrender == true)
                    {
                        //Sets the console window size
                        //Console.SetWindowSize(70, 30);
                        //Sets the window title
                        Console.Title = "Battleships - " + GlobalData.aiDifficulty + " - Surrender";


                        //Surrender screen
                        GlobalData.surrender = false;

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("\n _______   _______  _______  _______     ___   .___________.         \n" +
                        "|       \\ |   ____||   ____||   ____|   /   \\  |           |         \n" +
                        "|  .--.  ||  |__   |  |__   |  |__     /  ^  \\ `---|  |----`         \n" +
                        "|  |  |  ||   __|  |   __|  |   __|   /  /_\\  \\    |  |              \n" +
                        "|  '--'  ||  |____ |  |     |  |____ /  _____  \\   |  |     __ __ __ \n" +
                        "|_______/ |_______||__|     |_______/__/     \\__\\  |__|    (__|__|__)\n\n\n\n");

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("\n\n\n\nYou surrendered.\n\n\n\n\n");
                    }

                    resetDefaultFontColour();
                    typeWriter("\n\nPress any key to return to menu");
                    Console.ReadKey();
                }
            }
        }
    }
}