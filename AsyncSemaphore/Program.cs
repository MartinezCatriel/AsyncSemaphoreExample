using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncSemaphore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        public static async Task MainAsync()
        {
            await SemaphoreImplementation.Do();
        }
    }

    public static class SemaphoreImplementation
    {
        public static async Task Do()
        {
            try
            {
                using (var semaphoreSlim = new SemaphoreSlim(2, 2))
                {
                    var stopWatch = Stopwatch.StartNew();
                    var bucketNeeded = 7;
                    var list = await ForParallelImplementationList.GetList(bucketNeeded);
                    Task lastTask = Task.FromResult(true);
                    foreach (var item in list)
                    {
                        await semaphoreSlim.WaitAsync();
                        await WriteToConsole($"Starting task at:{stopWatch.Elapsed}. With {item.DelayTime} delay time.");
                        lastTask = item.DoAsync(semaphoreSlim);
                    }
                    await lastTask;
                    await WriteToConsole($"Total tasks executed {list.Count()}. Total execution time {stopWatch.Elapsed}");
                }
            }
            catch (Exception ex)
            {
                await WriteToConsole("Something went wrong. Try executing the process again.");
            }
        }
        private static async Task WriteToConsole(string toWrite)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(toWrite);
            });
        }
    }

    public static class ForParallelImplementationList
    {
        public static IEnumerable<DoSomethingTaskImplementation> _list = new List<DoSomethingTaskImplementation> {
            new DoSomethingTaskImplementation(){Id=1, DelayTime = 5000 },
            new DoSomethingTaskImplementation(){Id=2, DelayTime = 4000 },
            new DoSomethingTaskImplementation(){Id=3, DelayTime = 3000 },
            new DoSomethingTaskImplementation(){Id=4, DelayTime = 4000 },
            new DoSomethingTaskImplementation(){Id=5, DelayTime = 3000 },
            new DoSomethingTaskImplementation(){Id=6, DelayTime = 3000 },
            new DoSomethingTaskImplementation(){Id=7, DelayTime = 6000 }
        };
        public static async Task<IEnumerable<DoSomethingTaskImplementation>> GetList(int bucketNeeded)
        {
            return await Task.Run(() =>
            {
                var rtn = _list.Take(bucketNeeded);
                return rtn;
            });
        }
    }

    public class DoSomethingTaskImplementation
    {
        public int Id { get; set; }
        public int DelayTime { get; set; }
        public async Task<bool> DoAsync(SemaphoreSlim semaphoreSlim)
        {
            try
            {
                await Task.Delay(this.DelayTime);
            }
            finally
            {
                semaphoreSlim.Release();
            }
            return true;
        }
    }
}
