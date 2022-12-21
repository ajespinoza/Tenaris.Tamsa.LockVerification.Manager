using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tenaris.Library.Log;

namespace Tenaris.Tamsa.LockVerification.Manager.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                string command;
                var host = new Host();
                host.Start();

                do
                {
                    command = Console.ReadLine();

                } while (command.ToUpper() != "EXIT");

                host.Stop();
            }
            catch (Exception ex)
            {
                Trace.Exception(ex);
            }
        }
    }
}
