using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleMiniApp
{
    class Matrix
    {
        public int MaxLineLenght
        {
            get { return maxLineLength; }
            set { maxLineLength = value; }
        }
        public int MinLineLenght
        {
            get { return minLineLenght; }
            set { minLineLenght = value; }
        }

        public int MaxLineSpeed
        {
            get { return maxLineSpeed; }
            set { maxLineSpeed = value; }
        }
        public int MinLineSpeed
        {
            get { return minLineSpeed; }
            set { minLineSpeed = value; }
        }


        #region Vars
        const ConsoleColor white = ConsoleColor.White;
        const ConsoleColor greenLight = ConsoleColor.Green;
        const ConsoleColor greenDark = ConsoleColor.DarkGreen;
        const string letters = "!@#$%^&*()_+-=~ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        static int lettersLenght = letters.Length;
        static object key = new object();

        static int maxLineLength = 7;
        static int minLineLenght = 2;
        static int minLineSpeed = 20;
        static int maxLineSpeed = 700;

        // max height == console height
        static int maxHeight;

        // max width == console width
        static int maxWidth;

        // currentCol - number of columt that paint faling line
        int currentCol;

        #endregion

        #region Constructors
        static Matrix()
        {
            Console.CursorVisible = false;
            maxHeight = Console.WindowHeight - 1;
            maxWidth = Console.WindowWidth - 1;
        }
        public Matrix()
        {
        }

        Matrix(int column)
        {
            currentCol = column;
        }
        #endregion

        void TailFalingLine(ref int headElement, ref int lastElement, ref int charIndex, int falLineLength)
        {
            /*  TailFalingLine - calculate chars number that need to be draw upward head char(need find lastElement)
             *  Calling every time when head char moving down
             *  
             *  At start:
             *  headtElement == headPosition(Y axis)
             *  lastElement == 0(unknown yet)
             *  charIndex == 0(zero chars was placed)
             *  
             *  =============================================================================================
             *  
             *  Condition check can we draw ALL count of chars(==falLineLenght) or no
             *  
             *   We have 3 situation:
             *  
             *   1) If length from 0(min console position) to head position BIGGER(or equal) than faling line length
             *       Example: head char index 5, and falling line lenght is 2, than we CAN draw all chars
             *                lastElement need up for 1
             *       
             *   2) If length from 0(min console position) to head position LOWER than faling line length
             *       Example: head char index 1(second row), and falling line lenght is 3, than we CAN'T draw all 3 chars
             *                lastElement == 0
             *   
             *   3) If head position bigger than max console height, so we need cut first chars
             *  
             */
            if ((headElement - falLineLength + 1) > 0)
            {
                // 1) situation
                if (headElement <= maxHeight)
                {
                    lastElement++;
                }
                // 3) situation
                else
                {
                    /*currentElement - maxHeight == how many chars we need cut(<X>)
                     * As we know first char is falling line WHITE, second - LIGHTGREEN, than - DARKGREEN
                     * So <X> using to display correct colors by changing charIndex
                     * No matter if charIndex >= 2, all this chars colored same
                     */
                    charIndex = (headElement - maxHeight) <= 1 ? 1 : 2;
                    lastElement++;
                    headElement = maxHeight; // starting draw from max posible position
                }
            }
            // 2) situation
            else
            {
                lastElement = 0;
            }
        }

        ConsoleColor GetCurrentColor(int charIndex)
        {
            /*
             *  This method return in what color need be painter char by his index
             *  index == 0(head char) -> white
             *  index == 1(second char) -> greenLigth
             *  index >= 2(all others) -> darkGreen
             */
            switch (charIndex)
            {
                case 0:
                    {
                        return white;
                    }
                case 1:
                    {
                        return greenLight;
                    }
                default:
                    {
                        return greenDark;
                    }
            }
        }

        char PickRandomChar(List<char> usedChars)
        {
            /*
             * This method pick unique(not in usedChars) char
             */
            char curChar;

            // prevent dead lock
            if (usedChars.Count == lettersLenght)
            {
                return letters[(new Random()).Next(lettersLenght)];
            }

            while (true)
            {
                curChar = letters[(new Random()).Next(lettersLenght)];
                if (!usedChars.Contains(curChar))
                {
                    usedChars.Add(curChar);
                    return curChar;
                }
            }
        }

        void DrawChar(ConsoleColor curColor, char curChar, int drawPosition)
        {
            /*  
             *  This method lock console and draw faling line
             */

            lock (key)
            {
                Console.ForegroundColor = curColor;
                //if (currentCol >= Console.WindowWidth)
                //{
                //    Thread.CurrentThread.Abort();
                //}

                // Draw chars
                Console.SetCursorPosition(currentCol, drawPosition);
                Console.Write(curChar);
            }
        }

        void ClearLastChar(int lastCharPosition)
        {
            // Claer last char
            // Check if we can clean previos element
            
            if (lastCharPosition != -1)
            {
                lock (key)
                {
                    Console.SetCursorPosition(currentCol, lastCharPosition);
                    Console.Write(" ");
                }
            }
        }

        int GetClearPosition(int lastElement, int headElement)
        {
            /*
             *  Return position(Y axis) of element than need to be deleted.
             *  if headPosition >= maxHeight => need delete (last - 1) element
             *  else delete last element
             */

            // No need delete
            if(lastElement == 0)
            {
                return -1;
            }
            return lastElement - 1;
        }

        void FalingLine()
        {

            #region Local vars

            // position(Y axis) of last element
            int lastElement;

            // falLineLength - chars number of faling line
            int falLineLength;

            // falLineSpeed - how fast falling line moving
            int falLineSpeed;

            // usedChars - list of chars that already exist in falling line 
            // prevents char dublicates in falling line
            List<char> usedChars;

            // curChar - corrent char that choose for draw at the end of cicle, in lock section
            char curChar;

            // curColor - current char color that choose for implement at the end of cicle, in lock section
            ConsoleColor curColor;

            #endregion

            #region Main loop
            while (true)
            {
                Thread.Sleep(new Random().Next(10, 30));
                // generate faling line speed and length at every cicle
                falLineSpeed = new Random().Next(minLineSpeed, maxLineSpeed);
                falLineLength = new Random().Next(minLineLenght, maxLineLength);
                
                lastElement = 0; 

                // headPosition is "head" position(Y axis) of falling line(position of first char)
                for (int headPosition = 0; headPosition < maxHeight + falLineLength + 1; headPosition++)
                {
                    int currentElement = headPosition;
                    int charIndex = 0;
                    usedChars = new List<char>();

                    // calculate currentElement, lastElement, and change charIndex(if needed)
                    TailFalingLine(ref currentElement, ref lastElement, ref charIndex, falLineLength);

                    // draw faling line from headPosition to lastElement
                    for (; currentElement >= lastElement; currentElement--)
                    {
                        curColor = GetCurrentColor(charIndex);
                        curChar = PickRandomChar(usedChars);

                        DrawChar(curColor, curChar, currentElement);

                        charIndex++;
                    }

                    // clear previos char if needed
                    int clearPosition = GetClearPosition(lastElement, headPosition);
                    ClearLastChar(clearPosition);

                    // timeout
                    Thread.Sleep(falLineSpeed);
                }
            }
            #endregion

        }


        public void Start()
        {
            for (int i = 0; i < maxWidth; i++)
            {
                new Thread(new Matrix(i).FalingLine).Start();

                // delay
                Thread.Sleep(new Random().Next(15, 50));
            }

            
        }
    }
}
