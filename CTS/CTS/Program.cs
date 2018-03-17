using Easy.MessageHub;
using System;
using System.Threading;
using System.Threading.Tasks;
using static CTS.BigOrder;

namespace CTS
{
    public class Person
    {

        public string Id { get; set; }
        public int Score { get; set; }
    }
    public class Order
    {
        public int Score { get; set; }
        private Mutex _mutex = new Mutex();

        public Order()
        {
            Score = 0;

            var hub = MessageHub.Instance;
            var token = hub.Subscribe<Person>(p => Process(p));

            Action<string> action = message => Console.WriteLine($"New Order Message is: {message}");
            var anotherToken = hub.Subscribe(action);
        }

        private void Process(Person p)
        {
            bool haveLock = _mutex.WaitOne();
            try
            {
                Score += p.Score;
                Console.WriteLine($"Order Id is: {p.Id} Score {Score}");
            }
            finally
            {
                if (haveLock)
                    _mutex.ReleaseMutex();

            }
        }

        private void Process(String p)
        {
            Console.WriteLine($"New Order Message is: {p}");
        }
    }

    public class BigOrder
    {
        public int Score { get; set; }

        private Mutex _mutex = new Mutex();

        public BigOrder()
        {
            Score = 0;

            var hub = MessageHub.Instance;
            var token = hub.Subscribe<Person>(p => Process(p));

            Action<string> action = message => Console.WriteLine($"Big Order Message is: {message}");
            var anotherToken = hub.Subscribe(action);
        }

        private void Process(Person p)
        {
            bool haveLock = _mutex.WaitOne();
            try
            {
                Score += p.Score;
                Console.WriteLine($"Big Order Id is: {p.Id} Score {Score}");
            }
            finally
            {
                if (haveLock)
                    _mutex.ReleaseMutex();

            }

        }

        public class NewOrder
        {
            public void PublishPerson()
            {
                var hub = MessageHub.Instance;
                hub.Publish(new Person { Id = "Foo", Score = 1 });
            }

            public void PublishMessage()
            {
                var hub = MessageHub.Instance;
                hub.Publish("An important message");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BigOrder bo = new BigOrder();
            Order o = new Order();
            NewOrder no = new NewOrder();

            for (int i = 0; i < 100; i++)
            {
                Task.Factory.StartNew(() => no.PublishPerson());
                Thread.Sleep(100);
            }

            Console.ReadKey();
        }
    }
}
