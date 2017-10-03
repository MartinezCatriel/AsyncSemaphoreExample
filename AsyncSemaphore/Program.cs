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
        private const int InitialCount = 2;
        private const int MaxCount = 2;

        private static SemaphoreImplementation _sempahoreImplementation = new SemaphoreImplementation(InitialCount, MaxCount);

        public static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        public static async Task MainAsync()
        {
            await _sempahoreImplementation.DoAsync();
        }
    }

    public class SemaphoreImplementation
    {
        public int InitialCount { get; }

        public int MaxCount { get; }

        public SemaphoreImplementation(int initialCount, int maxCount)
        {
            InitialCount = initialCount;
            MaxCount = maxCount;
        }

        public async Task DoAsync()
        {
            try
            {
                using (var semaphoreSlim = new SemaphoreSlim(InitialCount, MaxCount))
                {
                    var stopWatch = Stopwatch.StartNew();
                    var actionsList = GetInitialList(7);
                    var taskList = new List<Task>();

                    foreach (var item in actionsList)
                    {
                        taskList.Add(ExecuteTask(semaphoreSlim, item));
                    }

                    Task.WaitAll(taskList.ToArray());

                    Console.WriteLine($"Total tasks executed {actionsList.Count()}. Total execution time {stopWatch.Elapsed}");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Something went wrong. Try executing the process again.");
            }
        }

        private async Task ExecuteTask(SemaphoreSlim semaphoreSlim, GenerateDelayAsyncItemImplementation item)
        {
            await semaphoreSlim.WaitAsync();
            await item.DoAsync();
            semaphoreSlim.Release();
        }

        private static IEnumerable<GenerateDelayAsyncItemImplementation> GetInitialList(int taskCount)
        {
            var rnd = new Random();
            
            for (var i = 0; i < taskCount; i++)
            {
                yield return new GenerateDelayAsyncItemImplementation { Id = i, DelayTime = 1000 * rnd.Next(1, 10) };
            }
        }
    }

    public class GenerateDelayAsyncItemImplementation
    {
        public int Id { get; internal set; }

        public int DelayTime { get; internal set; }

        public async Task DoAsync()
        {
            Console.WriteLine($"Id: {Id} - Delay: {DelayTime}");
            await Task.Run(async () => await Task.Delay(DelayTime));
        }
    }

    /*
    public static class OverrideReadOVerWriteImplementation
    {
        public static async Task Do()
        {
            var sw = Stopwatch.StartNew();
            var _continuationToken = true;
            do
            {
                var list = await ImplementationList.GetList();
                _continuationToken = ImplementationList._continuationToken > 0;
                var beforeWriteTask = Task.FromResult(true);
                foreach (var item in list)
                {
                    var readed = item.ReadAsync(item.Id);
                    await beforeWriteTask;
                    beforeWriteTask = item.WriteAsync(item.Id, await readed);
                }
                await beforeWriteTask;
                await WriteToConsole($"Last task elapsed time:{sw.Elapsed}. {list.Count()} items proccesed.");
            } while (_continuationToken);
        }
        private static async Task WriteToConsole(string toWrite)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(toWrite);
            });
        }
    }

    public static class ImplementationList
    {
        public static int _continuationToken = 300;
        public static IEnumerable<ReadSaveImplementation> _list;

        public static async Task<IEnumerable<ReadSaveImplementation>> GetList()
        {
            return await Task.Run(() =>
            {
                var rtn = new List<ReadSaveImplementation>();
                for (int i = 0; i < 40; i++)
                {
                    rtn.Add(new ReadSaveImplementation() { Id = i });
                }
                _continuationToken -= 40;
                return rtn;
            });
        }
    }

    public class ReadSaveImplementation
    {
        public int Id { get; set; }

        public async Task<ReadSaveImplementation> ReadAsync(int id)
        {
            await Task.Delay(30);
            //Thread.Sleep(30);
            return this;
        }
        public async Task<bool> WriteAsync(int id, ReadSaveImplementation data)
        {
            await Task.Delay(60);
            //Thread.Sleep(60);
            return true;
        }
    }*/
}
