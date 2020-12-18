using System;
using System.IO;

namespace WarThunderReplay
{
    class Program
    {
        static void Main(string[] args)
        {

            // SetConsoleToWriteToFile();


            var replay = new Replay(args[0]);
            replay.Parse();
            Console.WriteLine("end of program");
        }

        private static void SetConsoleToWriteToFile()
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream("consolelog.txt", FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
                Console.SetOut(writer);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open consolelog.txt for writing");
                Console.WriteLine(e.Message);
            }

        }
    }
}
