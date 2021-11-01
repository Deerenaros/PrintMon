using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Collections;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace PrintMon
{
    class Program
    {
        static IEnumerable<(string name, string doc, long pages)> FetchSpoolingJobs()
        {

            string searchQuery = "SELECT * FROM Win32_PrintJob";
            ManagementObjectSearcher searchPrintJobs = new ManagementObjectSearcher(searchQuery);
            ManagementObjectCollection prntJobCollection = searchPrintJobs.Get();
            return prntJobCollection.Cast<ManagementObject>()
                .Where((j) =>
                {
                    return Convert.ToString(j.Properties["JobStatus"]?.Value) == "Spooling";
                }).Select((j) =>
                {
                    return (
                        j.Properties["Name"].Value.ToString(),
                        j.Properties["Document"].Value.ToString(),
                        Int64.Parse(j.Properties["PagesPrinted"].Value.ToString())
                    );
                });
        }

        static void Main(string[] _)
        {
            try
            {
                Task.Run(async delegate
                {
                    while (true)
                    {
                        FetchSpoolingJobs().ToList().ForEach((t) =>
                        {
                            Console.WriteLine($"{t.name} of {t.doc} with {t.pages} pages");
                        });

                        await Task.Delay(100);
                    }
                }).Wait();
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Something goes wrong");
            }
        }
    }
}
