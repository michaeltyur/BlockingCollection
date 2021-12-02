using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace BlockingCollection
{
    class Program
    {
        static void Main(string[] args)
        {
            Random random = new Random();
            ProducerWorker producer1 = new ProducerWorker(random);
            ProducerWorker producer2 = new ProducerWorker(random);
            ProducerWorker producer3 = new ProducerWorker(random);
            producer1.StartWork();
            producer2.StartWork();
            producer3.StartWork();
            Consumer consumer = new Consumer();
            ConsoleKeyInfo key = Console.ReadKey(true);

            while (Console.ReadKey(true).Key == ConsoleKey.Escape || Console.ReadKey(true).Key == ConsoleKey.Enter)
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    producer1.StopWork();
                    producer2.StopWork();
                    producer3.StopWork();
                    consumer.StopWork();
                }
                else
                {
                    producer1.StartWork();
                    producer2.StartWork();
                    producer3.StartWork();
                    consumer.StartWork();
                }
            }
        }

    }
    public class ProducerWorker
    {
        private Random _random;
        private Thread _thread;
        public ProducerWorker(Random random)
        {
            _random = random;

        }
        public void StartWork()
        {
            _thread = new Thread(new ThreadStart(() =>
            {
                while (_thread.ThreadState != ThreadState.Aborted)
                {
                    int number = RunWorkWithDelay();
                    BlockingCollectionInstance.Instance.AddData(number);
                }
            }));
            _thread.Start();
        }
        public void StopWork()
        {
            _thread.Abort();
        }
        private int RunWorkWithDelay()
        {
            int delay = GetRandomNumber(1, 3);
            Thread.Sleep(TimeSpan.FromSeconds(delay));
            return GetRandomNumber(0, 100);
        }
        private int GetRandomNumber(int start, int end)
        {
            return _random.Next(start, end);
        }

    }

    public class BlockingCollectionInstance
    {
        private BlockingCollection<int> blockingCollection;

        private static BlockingCollectionInstance instance = null;
        public static BlockingCollectionInstance Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BlockingCollectionInstance();
                }
                return instance;
            }
        }
        private BlockingCollectionInstance()
        {
            blockingCollection = new BlockingCollection<int>();
        }
        public void AddData(int data)
        {
            blockingCollection.Add(data);
        }
        public IEnumerable<int> FetchData()
        {
            foreach (int item in blockingCollection.GetConsumingEnumerable())
            {
                yield return item;
            }
        }
        public void StopWork()
        {
            blockingCollection.CompleteAdding();
            instance = null;
        }

    }

    public class Consumer
    {
        private Random _random;
        private Thread _thread;
        ConsoleColor[] _colors = (ConsoleColor[])ConsoleColor.GetValues(typeof(ConsoleColor));
        public Consumer()
        {
            _random = new Random();
            _colors = _colors.Where(x => x != ConsoleColor.Black).ToArray();
            StartWork();
        }
        public void StartWork()
        {
            Console.WriteLine();
            Console.WriteLine("For stop press escape");
            _thread = new Thread(new ThreadStart(() =>
            {
                while (_thread.ThreadState != ThreadState.Aborted)
                {
                    var data = BlockingCollectionInstance.Instance.FetchData();
                    Console.ForegroundColor = _colors[GetRandomNumber(0, _colors.Length - 1)];
                    Console.Write(data.First() + ", ");
                }
            }));
            _thread.Start();
        }
        public void StopWork()
        {
            _thread.Abort();
            BlockingCollectionInstance.Instance.StopWork();
            Console.WriteLine();
            Console.WriteLine("Stopped");
            Console.WriteLine("For start press enter");
        }
        private int GetRandomNumber(int start, int end)
        {
            return _random.Next(start, end);
        }
    }
}
