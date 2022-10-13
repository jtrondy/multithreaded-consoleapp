using System.Diagnostics;

namespace multithreaded_consoleapp
{
    class Program
    {
        static readonly object lockObject = new object();
        static int maxMultiThread = 0;

        static void Main(string[] args)
        {
            int n = 10000000;
            int[] arr = RandomArray(n);
            SingleThread(arr); // utilises a local max variable (not static "maxMultiThread")
            MultiThreadLoop(arr);
            Console.ReadKey();
        }

        // displays the maximum number in an array utilising n threads
        static void MultiThreadLoop(int[] arr)
        {
            Console.WriteLine($"\n-----------Multithread ({Environment.ProcessorCount} threads)-----------");
            var watch = new Stopwatch();
            watch.Start();

            int numOfThreads = Environment.ProcessorCount;
            if (arr.Length < numOfThreads)
            {
                Console.WriteLine($"array not large enough for {numOfThreads} threads, max is {arr.Max()}");
                watch.Stop();
                return;
            }

            var threads = new Thread[numOfThreads];

            // find the sub array size for increment
            int sizeOfArray = (arr.Length / numOfThreads);
            int start = 0;
            int end = sizeOfArray;
            int remaining = arr.Length % numOfThreads;

            for (int i = 0; i < numOfThreads; i++)
            {
                // passing "arr[start..end]" directly to the threads introduces bugs.
                int[] subArr = arr[start..end];

                // https://www.youtube.com/watch?v=rUbmW4qAh8w&t helped with passing data into thread
                threads[i] = new Thread(() => MultiThread(subArr)) { Name = $"thread{i}" };
                threads[i].Start();

                // incrementing indices for subarray
                // last subarray takes remaining elements if array is not equally divisible
                start += sizeOfArray;
                end += (i >= (numOfThreads - 2)) ? (sizeOfArray + remaining) : (sizeOfArray);
            }

            // current thread (main thread) will wait for the threads it joins to finish before continuing
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            watch.Stop();
            Console.WriteLine($"maximum is: {maxMultiThread}");
            Console.WriteLine($"{watch.ElapsedMilliseconds} ms to complete.");
        }

        // multiple threads concurrently find the max of their respective subArr
        static void MultiThread(int[] subArr)
        {
            int localMax = subArr.Max();

            lock (lockObject) // locking the shared resource 
            {
                maxMultiThread = localMax > maxMultiThread ? localMax : maxMultiThread;
            }

            //concurrency and duplicate tests
            //for (int i = 0; i < 100; i++)
            //{
            //    Console.WriteLine($"This is thread #{Thread.CurrentThread.Name} {subArr.Length}");
            //}
            //foreach (int i in subArr)
            //{
            //    Console.WriteLine($"This is thread #{Thread.CurrentThread.Name} {subArr.Length} value: {i}");
            //}
        }

        // displays the maximum number of the array utilising a single thread
        static void SingleThread(int[] arr)
        {
            Console.WriteLine("-----------Singlethread-----------");
            var watch = new Stopwatch();
            watch.Start();
            
            int localMax = arr.Max();
            
            watch.Stop();
            Console.WriteLine($"maximum is: {localMax}");
            Console.WriteLine($"{watch.ElapsedMilliseconds} ms to complete.");
        }

        static int[] RandomArray(int n)
        {
            var arr = new int[n];
            var random = new Random();
            for (int i = 0; i < n; i++)
            {
                arr[i] = random.Next(1000000000); 
            }
            return arr;
        } 
    }
}
