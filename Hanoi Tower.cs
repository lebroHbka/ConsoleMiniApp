using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleMiniApp
{
    // MVC Realisation

    class Disk
    {
        public int Size;
        public int X { get; set; }
        public int Y { get; set; }

        public Disk(int size, int x, int y)
        {
            Size = size;
            X = x;
            Y = y;
        }
    }
    class Pole
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public int DisksCount
        {
            get { return disksOnPole.Count; }
        }
        Stack<Disk> disksOnPole;

        public Pole(int height, int x, int y)
        {
            Height = height;
            X = x;
            Y = y;
            disksOnPole = new Stack<Disk>();
        }

        public void AddDisk(Disk d)
        {
            disksOnPole.Push(d);
        }

        public Disk PopDisk()
        {
            return disksOnPole.Pop();
        }

    }

    class PoleIndexException : Exception
    {
        public PoleIndexException(string msg)
            :base(msg)
        {
        }
    }
    class DisksCountException : Exception
    {
        public DisksCountException(string msg)
            : base(msg)
        {
        }
    }
    class ConsoleSizeException : Exception
    {
        public ConsoleSizeException(string msg)
            : base(msg)
        {
        }
    }


    // Make all actions(calculate nothing)
    class Controller
    {
        public int DisksCount
        {
            get { return diskCount; }
            set
            {
                diskCount = value;
                model.DisksCount = value;
            }
        }
        public int StartPoleNumber { get; set; } = 0;
        public int EndPoleNumber { get; set; } = 2;

        #region Vars
        const int polesCount = 3;
        const int startDiskSize = 3;
        const int incDiskWidth = 4;
        const int maxDisks = 10;
        const int moveSpeed = 1000;

        Model model;
        View view;

        List<Pole> polesList;

        int diskCount = 5;
        int helpPoleNumber = 1;

        #endregion

        #region Constructors
        public Controller()
        {
            model = new Model(startDiskSize, incDiskWidth, DisksCount, polesCount);
            view = new View();
            polesList = new List<Pole>();
        }

        public Controller(int disksCount)
            :this()
        {
            DisksCount = disksCount;
        }

        public Controller(int startPole, int finishPole)
            :this()
        {
            StartPoleNumber = startPole;
            EndPoleNumber = finishPole;
        }

        public Controller(int disksCount, int startPole, int finishPole)
            :this(startPole, finishPole)
        {
            DisksCount = disksCount;
        }
        #endregion

        #region Enums
        enum Coordinates
        {
            x = 0,
            y = 1
        }
        #endregion
        
        // Create new poles at start
        void MakeStartPoles()
        {
            // Get poles coordinates from model, and than create poles
            foreach (var cord in model.GetPolsCordinates())
            {
                Pole p = new Pole(DisksCount + 1, 
                                  cord[(int)Coordinates.x], cord[(int)Coordinates.y]);
                view.DrawPole(p);
                polesList.Add(p);
            }
        }

        // Create start disks and put them in start pole
        void MakeStartDisks()
        {
            Pole startPole = polesList[StartPoleNumber];
            
            // add disks in start pole and draw them
            foreach (var disk in model.GetStartDisks(startPole))
            {
                view.DrawDisk(disk);
                startPole.AddDisk(disk);
            }
        }

        // Method that move the highest disk from oldPole, and put if to another
        void MoveDisk(Pole oldPole, Pole newPole)
        {
            // remove disk from old pole
            Disk d = oldPole.PopDisk();
            view.ClearDisk(d);
            view.DrawPole(oldPole);     // update empty space where was disk

            // add disk to new pole
            model.CalculateCoordinates(newPole, d);
            newPole.AddDisk(d);
            view.DrawDisk(d);
        }

        // Main algorithm
        void HanoiAlgorithm(int diskCount, Pole start, Pole end, Pole help)
        {
            if(diskCount >= 1)
            {
                HanoiAlgorithm(diskCount - 1, start, help, end);
                Thread.Sleep(moveSpeed);
                MoveDisk(start, end);
                HanoiAlgorithm(diskCount - 1, help, end, start);
            }
        }

        // Method that calculate helpPole index, if user change settings
        public int GetHelpPoleNumber()
        {
            /* 
             * This method calculate wich pole is free
             * Each pole can get 1 unique index, so we can get total sum and subtract indexs of 
             * startPole and endPole
             * Example:
             *      0 + 1 + 2 = 3 (total sum of poles index)
             *      2 - startPole
             *      0 - endPole
             *      3 - (2+0) = 1 is free pole
             */
            return 3 - (StartPoleNumber + EndPoleNumber);
        }



        // Start algorithm
        public void StartHanoi()
        {
            if((StartPoleNumber == EndPoleNumber) ||
                (StartPoleNumber < 0) || (StartPoleNumber >= polesCount) ||
                (EndPoleNumber < 0) || (EndPoleNumber >= polesCount))
            {
                throw new PoleIndexException("Incorect pole(start/end) index");
            }
            if ((DisksCount <= 0) || (DisksCount > maxDisks))
            {
                throw new DisksCountException("Incorect disk count");
            }

            try
            {
                // Draw poles and disks
                MakeStartPoles();
                MakeStartDisks();
            }
            catch (ConsoleSizeException)
            {
                throw;
            }

            // calculate helpPole Number in case user change settings
            helpPoleNumber = GetHelpPoleNumber();

            // Start
            HanoiAlgorithm(DisksCount, polesList[StartPoleNumber], polesList[EndPoleNumber], polesList[helpPoleNumber]);

        }
    }

    // Make all calculation
    class Model 
    {
        #region Vars

        int startDiskSize;
        int incDiskWidth;
        public int DisksCount { get; set; }
        int polesCount;

        #endregion

        // Constructor
        public Model(int startDiskSize, int incDiskWidth, int disksCount, int polesCount)
        {
            this.startDiskSize = startDiskSize;
            this.incDiskWidth = incDiskWidth;
            this.DisksCount = disksCount;
            this.polesCount = polesCount;
        }

        // Method return start poles coordinates
        public IEnumerable<int[]> GetPolsCordinates()
        {
            // all this calculation for place poles in middle of the console
            int maxDiskWidth = startDiskSize + incDiskWidth * (DisksCount - 1);
            int freeWidthSpace = Console.WindowWidth - maxDiskWidth*3;

            int maxDiskHeight = DisksCount + 1;  
            int freeHeightSpace = Console.WindowHeight - maxDiskHeight;

            int poleStartX = (freeWidthSpace / 2 + maxDiskWidth / 2) + 1;
            int poleStartY = (freeHeightSpace / 2 + maxDiskHeight) - 3;

            if((freeWidthSpace <= 0) || (freeHeightSpace <= 0))
            {
                throw new ConsoleSizeException("Window size is to small");
            }

            for (int i = 0; i < polesCount; i++)
            {
                int[] poleCoordinat = {
                                         poleStartX + i * maxDiskWidth,   // x
                                         poleStartY                       // y
                                      };

                // return pole coordinates
                yield return poleCoordinat;
            }
        }

        //  Calculat and apply disk X,Y 
        public void CalculateCoordinates(Pole p, Disk disk)
        {
            disk.Y = p.Y + p.Height - p.DisksCount - 1;
            disk.X = p.X - disk.Size / 2;
        }

        // Create new disk, calculate parameters and return 
        public IEnumerable<Disk> GetStartDisks(Pole startPole)
        {
            // calculate start disk width
            int diskWidth = startDiskSize + incDiskWidth * (DisksCount - 1);

            // Create new disks with correct width
            for (int i = 0; i < DisksCount; i++)
            {
                // create new disk
                Disk d = new Disk(diskWidth, 0, 0);

                // calculate and apply new coordinates
                CalculateCoordinates(startPole, d);

                // decrease width for next disk
                diskWidth -= incDiskWidth;

                // return disk
                yield return d;
            }
        }
    }

    // Console drawing
    class View
    {
        public const char diskElement = ' ';
        public const string poleElement = "|";

        public ConsoleColor diskColor = ConsoleColor.DarkMagenta;
        public ConsoleColor poleColor = ConsoleColor.Gray;


        // Draw pole
        public void DrawPole(Pole p)
        {
            // Draw poles level that have no disk (p.Height - p.DisksOnPole)
            Console.ForegroundColor = poleColor;
            for (int i = 0; i < p.Height - p.DisksCount; i++)
            {
                Console.SetCursorPosition(p.X, p.Y + i);
                Console.Write(poleElement);
            }
            Console.ResetColor();
        }

        // Clear pole
        public void ClearPole(Pole p)
        {
            for (int i = 0; i < p.Height; i++)
            {
                Console.SetCursorPosition(p.X, p.Y + i);
                Console.Write(' ');
            }
        }

        // Draw disk
        public void DrawDisk(Disk d)
        {
            Console.SetCursorPosition(d.X, d.Y);
            Console.BackgroundColor = diskColor;
            Console.Write(new String(diskElement, d.Size));
            Console.ResetColor();
        }

        // Clear disk space
        public void ClearDisk(Disk d)
        {
            Console.SetCursorPosition(d.X, d.Y);
            Console.Write(new String(' ', d.Size));
        }
    }
    

    class Hanoi_Tower
    {
        Controller controller;

        #region Constructors
        static Hanoi_Tower()
        {
            Console.CursorVisible = false;
        }
        
        public Hanoi_Tower()
        {
            controller = new Controller();
        }

        public Hanoi_Tower(int disksCount)
        {
            controller = new Controller(disksCount);
        }

        public Hanoi_Tower(int startPole, int finishPole)
        {
            controller = new Controller(startPole, finishPole);
        }

        public Hanoi_Tower(int disksCount, int startPole, int finishPole)
        {
            controller = new Controller(disksCount, startPole, finishPole);
        }
        #endregion
        void EndTittle()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(Console.WindowWidth / 2 - 5, 2);
            Console.WriteLine("-----------");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 5, 3);
            Console.WriteLine("    END    ");
            Console.SetCursorPosition(Console.WindowWidth / 2 - 5, 4);
            Console.WriteLine("-----------");
            Console.ForegroundColor = ConsoleColor.Black;
        }

        public void Start()
        {
            try
            {
                controller.StartHanoi();
                EndTittle();
            }
            catch(PoleIndexException e)
            {
                Console.WriteLine(e.Message);
            }
            catch(DisksCountException e)
            {
                Console.WriteLine(e.Message);
            }
            catch(ConsoleSizeException e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

    }
}
