using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace PrintMon
{
    class Program
    {
        static readonly HashSet<long> discoveredJobs = new HashSet<long>();

        static readonly string STARTING_STATUS = "Spooling";

        static long CalcJobHash(ManagementObject job)
        {
            return $"{job.Properties["Name"]} {job.Properties["StartTime"]}".GetHashCode();
        }

        static IEnumerable<(string name, string doc, long pages)> FetchSpoolingJobs()
        {

            string searchQuery = "SELECT * FROM Win32_PrintJob";
            ManagementObjectSearcher searchPrintJobs = new ManagementObjectSearcher(searchQuery);
            ManagementObjectCollection prntJobCollection = searchPrintJobs.Get();
            foreach (ManagementObject job in prntJobCollection) {
                if(!discoveredJobs.Contains(CalcJobHash(job)) &&
                   Convert.ToString(job.Properties["JobStatus"]?.Value).Contains(STARTING_STATUS))
                {
                    discoveredJobs.Add(CalcJobHash(job));
                    yield return (
                        job.Properties["Name"].Value.ToString(),
                        job.Properties["Document"].Value.ToString(),
                        Int64.Parse(job.Properties["TotalPages"].Value.ToString())
                    );
                } else {
                    discoveredJobs.Remove(CalcJobHash(job));
                }
            }
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
                            Console.WriteLine($"{t.name} of '{t.doc}' with {t.pages} pages");
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
