using System;

namespace MinhaApi
{
    public class Utils
    {
        private static readonly Random random = new Random();

        public static void RandomSleep()
        {
            int sleepTime = random.Next(50, 2001);
            Thread.Sleep(sleepTime);
        }

    }
}
