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
        public int DisksOnPole { get; set; }

        public Pole(int height, int x, int y)
        {
            Height = height;
            X = x;
            Y = y;
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

        Dictionary<Pole, Stack<Disk>> polesDict;
        List<Pole> polesList;

        int diskCount = 5;
        int helpPoleNumber = 1;

        #endregion

        #region Constructors
        public Controller()
        {
            model = new Model(startDiskSize, incDiskWidth, DisksCount, polesCount);
            view = new View();
            polesDict = new Dictionary<Pole, Stack<Disk>>();
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

        // Create new poles at start
        void MakeStartPoles()
        {
            // Get poles coordinates from model
            int[][] allCordinates = model.GetPolsCordinates();

            // Create poles with colculated coordinates
            foreach (var cordXY in allCordinates)
            {
                Pole p = new Pole(DisksCount + 1, cordXY[0], cordXY[1]);
                view.DrawPole(p);
                polesDict[p] = new Stack<Disk>();
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
                polesDict[startPole].Push(disk);
            }

        }

        // Method that move the highest disk from oldPole, and put if to another
        void MoveDisk(Pole oldPole, Pole newPole)
        {
            // remove disk from old pole
            Disk d = polesDict[oldPole].Pop();
            model.RemoveDiskFromPole(oldPole);
            view.ClearDisk(d);
            view.DrawPole(oldPole);     // update empty space where was disk

            // add disk to new pole
            model.AddDiskToPole(newPole, d);
            polesDict[newPole].Push(d);
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

            // Draw poles and disks
            MakeStartPoles();
            MakeStartDisks();

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
        public int[][] GetPolsCordinates()
        {
            // Used get poles coordinates at start
            int[][] res = new int[polesCount][];

            // all this calculation for place poles in middle of the console
            int maxDiskWidth = startDiskSize + incDiskWidth * (DisksCount - 1);
            int freeWidthSpace = Console.WindowWidth - maxDiskWidth*3;

            int maxDiskHeight = DisksCount + 1;  
            int freeHeightSpace = Console.WindowHeight - maxDiskHeight;

            int poleStartX = (freeWidthSpace / 2 + maxDiskWidth / 2) + 1;
            int poleStartY = (freeHeightSpace / 2 + maxDiskHeight) - 3;

            for (int i = 0; i < polesCount; i++)
            {
                res[i] = new int[]{
                                    poleStartX + i * maxDiskWidth,   // x
                                    poleStartY                       // y
                                   };
            }

            // return poles coordinates
            return res;
        }

        // Add disk to pole
        public void AddDiskToPole(Pole p, Disk disk)
        {
            /*
             *  Calculating disks X,Y 
             */
            disk.Y = p.Y + p.Height - p.DisksOnPole - 1;
            disk.X = p.X - disk.Size / 2;
            p.DisksOnPole++;
        }

        // Removing highest disk from pole
        public void RemoveDiskFromPole(Pole p)
        {
            p.DisksOnPole--;
        }

        // Calculate all disks parametres and return list of them
        public List<Disk> GetStartDisks(Pole startPole)
        {
            
            int diskWidth = startDiskSize + incDiskWidth * (DisksCount - 1);
            List<Disk> disksList = new List<Disk>();

            // Create new disks with correct width
            for (int i = 0; i < DisksCount; i++)
            {
                // create disk
                Disk tmp = new Disk(diskWidth, 0, 0);
                disksList.Add(tmp);

                // calculate coordinates
                AddDiskToPole(startPole, tmp);

                // decrease width for next disk
                diskWidth -= incDiskWidth;
            }

            return disksList;
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
            for (int i = 0; i < p.Height - p.DisksOnPole; i++)
            {
                Console.SetCursorPosition(p.X, p.Y + i);
                Console.Write(poleElement);
            }
            Console.ResetColor();
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
            
        }

    }
}
