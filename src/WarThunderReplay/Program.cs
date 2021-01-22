using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace WarThunderReplay
{
    class Program
    {
        static void Main(string[] args)
        {
            var replay = new Replay(args[0]);
            replay.Parse();

            Console.WriteLine("end of program");
        }

        private static void SetConsoleToWriteToFile(string filename)
        {
            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                ostrm = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
                Console.SetOut(writer);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open " + filename + " for writing");
                Console.WriteLine(e.Message);
            }

        }
    }
}
