using System;
using System.Threading;

namespace SurfsUpServer
{
    class Program
    {
        private static bool isRunning = false;
        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            server.start(3, 12345);
            isRunning = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
        }

        private static void MainThread()
        {
            Console.WriteLine("Main thread has started");
            DateTime nextLoop = DateTime.Now;

            while(isRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    gameLogic.Update();

                    nextLoop = nextLoop.AddMilliseconds(constants.MS_PER_TICK);

                    if(nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
