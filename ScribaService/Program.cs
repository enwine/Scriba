using System;

namespace ScribaService
{
    class Program
    {
        static void Main(string[] args)
        {
            // Default port stands for SCRIBA (omitting first number to fit in size)
            int port = 27422;
            if (args.Length > 0)
            {
                int.TryParse(args[0], out port);
            }
            try
            {
                Server.Gate.OpenPort(port);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Scriba server could not start.\nError: " + ex.ToString());
            }
        }
    }
}
