using System;

namespace ScribaService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // Default port stands for SCRIBA (omitting first number to fit in size)
            var port = 27422;

            Console.WriteLine(
                "Scriba server. Version 0.1.\nLicensed under: http://creativecommons.org/licenses/by-nc-sa/4.0/\nVicenç Gascó (vicenc@idako.us)\n");

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
                Console.Error.WriteLine("Scriba server could not start.\nError: " + ex);
            }
        }
    }
}