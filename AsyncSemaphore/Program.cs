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

	//This class does not need to be static, create a local static instance instead
    public static class SemaphoreImplementation
    {
		//Rename to DoAsync
        public static async Task Do()
        {
            try
            {
                using (var semaphoreSlim = new SemaphoreSlim(2, 2))
                {
                    var stopWatch = Stopwatch.StartNew();
					//Dont hardcode, use constants self explanatory instead
                    var bucketNeeded = 7;
					//Remove that class and use a method for initilization
                    var list = await ForParallelImplementationList.GetList(bucketNeeded);
					//Use var instead
                    Task lastTask = Task.FromResult(true);
                    foreach (var item in list)
                    {
                        await semaphoreSlim.WaitAsync();
						//Use Console.Write instead, no need for async here
                        await WriteToConsole($"Starting task at:{stopWatch.Elapsed}. With {item.DelayTime} delay time.");
                        lastTask = item.DoAsync(semaphoreSlim);
                    }
					
					//Think about this implementation:
					// var listOfTask = new List<Task>();
					// foreach (var item in list)
					// {
					// 	 listOfTask.Add(DoYourMagicHere(semaphoreSlim, item));
					// }
                    //await Task.WaitAll(listOfTask);
					
                    await WriteToConsole($"Total tasks executed {list.Count()}. Total execution time {stopWatch.Elapsed}");
                }
            }
            catch (Exception ex)
            {
                await WriteToConsole("Something went wrong. Try executing the process again.");
            }
        }
		//Use space in between methods
		//Useless method
        private static async Task WriteToConsole(string toWrite)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(toWrite);
            });
        }
		
		private static Task DoYourMagicHere(SemaphoreSlim semaphoreSlim, DoSomethingTaskImplementation item)
		{
			await semaphoreSlim.WaitAsync();
			await Console.WriteAsync($"Starting task at:{stopWatch.Elapsed}. With {item.DelayTime} delay time.");
			item.DoAsync(semaphoreSlim);
			semaphoreSlim.Release();
		}
    }

	//This clsas is useless
    public static class ForParallelImplementationList
    {
		//What if I want to take more than 7?
        public static IEnumerable<DoSomethingTaskImplementation> _list = new List<DoSomethingTaskImplementation> {
            new DoSomethingTaskImplementation(){Id=1, DelayTime = 5000 },
            new DoSomethingTaskImplementation(){Id=2, DelayTime = 4000 },
            new DoSomethingTaskImplementation(){Id=3, DelayTime = 3000 },
            new DoSomethingTaskImplementation(){Id=4, DelayTime = 4000 },
            new DoSomethingTaskImplementation(){Id=5, DelayTime = 3000 },
            new DoSomethingTaskImplementation(){Id=6, DelayTime = 3000 },
            new DoSomethingTaskImplementation(){Id=7, DelayTime = 6000 }
        };
		//Leave space in between methods
        public static async Task<IEnumerable<DoSomethingTaskImplementation>> GetList(int bucketNeeded)
        {
            return await Task.Run(() =>
            {
                var rtn = _list.Take(bucketNeeded);
                return rtn;
            });
        }
    }

	//Rename acordingly to what it is intended for
    public class DoSomethingTaskImplementation
    {
        public int Id { get; set; } //Dont need to expose setter
        public int DelayTime { get; set; } //Dont need to expose setter
		//Space in between
		//Dont need to return a type
        public async Task<bool> DoAsync(SemaphoreSlim semaphoreSlim)
        {
			//For the sake of abstraction this method should not have to know anything about semaphores
			//and also this will produce side effects.
            try
            {
                await Task.Delay(this.DelayTime);
            }
            finally
            {
                semaphoreSlim.Release();
            }
			//Space in between
            return true;
        }
    }
}
