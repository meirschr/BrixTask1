using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


#region Excercise Detail
//In the supermarket there is 1 queue for multiple cashiers(5 for instance).
//People join the queue when they are finished shopping and wait for their turn until there is a free cashier/checkout desk to process their order.

//Please write a console application that will model this flow.
//The goal is to keep wait time as short as possible for each buyer.


//Order processing time of each cashier is an average of 1 to 5 seconds.
//People join the queue at a rate of 1 per second.

#endregion
namespace Exercise1
{
    // Singleton thread safe queue
    public class SingletonConcurrentQueue<T> : ConcurrentQueue<T>
    {
        private static readonly SingletonConcurrentQueue<T>
                                       _instance = new SingletonConcurrentQueue<T>();

        static SingletonConcurrentQueue() { }
        private SingletonConcurrentQueue() { }

        public static SingletonConcurrentQueue<T> Instance
        {
            get { return _instance; }
        }
    }

    // Cashier class
    public class Cashier
    {
        string name = null;  // a Cashier has a name and a pointer to the queue
        SingletonConcurrentQueue<int> buyerQ = null;
        public Cashier(string _name, SingletonConcurrentQueue<int> _buyerQ)
        {
            name = _name;
            buyerQ = _buyerQ;
        }

        public void CashierWork()
        {
            int buyer;
            Random rnd = new Random(); // contols how long it takes to process a client

            while (true)
            {
                // if available customer
                if (buyerQ.Count > 0)
                {
                    //if got the client to the cashier
                    if (buyerQ.TryDequeue(out buyer))
                    {
                        Console.WriteLine("Cashier {0} is dealing with customer {1}", name, buyer);
                        int waitTime = rnd.Next(1000, 5000); // in milliSeconds - between 1 to 5 seconds
                        // this cashier is processing the client, to the external wirld he is occupied, so wait
                        Thread.Sleep(waitTime);
                        Console.WriteLine("Cashier {0} has finished with customer {1} after {2} milliseconds", name, buyer, waitTime);
                    }
                }
            }

        }
    }

    class Program
    {
        // initialize or get the queue instance
        public static SingletonConcurrentQueue<int> buyers =   SingletonConcurrentQueue<int>.Instance;
             
        static int buyerID = 0; // keep track of next customer number

        static public void AddPersonsToQueue()
        {
            // wait a second to add a new buyer; 
            // this is not exact, and normally I would take a timestamp at the beginning and wait the remainder.
            TimeSpan newCustomerArrival = new TimeSpan(0, 0, 1);

            while (true)
            {
                Console.WriteLine("Customer #{0} added", buyerID.ToString());
                // ad to queue
                buyers.Enqueue(buyerID++);
                //Wait 1 second
                Thread.Sleep(newCustomerArrival);
            }
        }

        static void Main(string[] args)
        {
            // Add Cashiers
            Console.WriteLine("Starting adding Cashiers");
            Cashier A = new Cashier("A", buyers);
            Cashier B = new Cashier("B", buyers);
            Cashier C = new Cashier("C", buyers);
            Cashier D = new Cashier("D", buyers);
            Cashier E = new Cashier("E", buyers);

            // have the running before customers
            Task.Run(() => A.CashierWork());
            Task.Run(() => B.CashierWork());
            Task.Run(() => C.CashierWork());
            Task.Run(() => D.CashierWork());
            Task.Run(() => E.CashierWork());

            Console.WriteLine("Starting adding customers to queue");

            // start a thread fo arriving customers
            Thread AddPersons = new Thread(AddPersonsToQueue);
            if (!(AddPersons.ThreadState.Equals(System.Threading.ThreadState.Running))) // if Thread not running (definitely in a simple program like this)
                AddPersons.Start(); // start thread

            Console.Write("Press <enter> to exit!");
            Console.Read();
        }
        
    }
}
