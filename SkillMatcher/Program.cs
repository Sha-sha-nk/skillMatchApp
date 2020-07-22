using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace SkillMatcher
{
    class Program
    {
        static void Main(string[] args)
        {   
            var cache = new Cache();
            var storage = new Storage();

            IContentProvider contentProvider = new ContentProvider(cache, storage);
            //IContentProvider contentProvider = new ContentProviderBloom(cache, storage);

            var numContentEntries = 1000000;
            var oneOffFraction = 0.65;

            var loadGenerator = new LoadGenerator(contentProvider);
            loadGenerator.Run(numContentEntries, oneOffFraction);

            cache.PrintStats();

            var backend = new Backend(new UserStore());
            var shashank = backend.AddUser(55.676098F, 12.568337F);
            Console.WriteLine($"Shashank's position: long: {shashank.Longitude}\tlat: {shashank.Latitude}");

            Console.WriteLine($"Populating users...");
            Populate(backend, 100000);

            Console.WriteLine("Nearby users:");
            var nearbyUsers = backend.GetNearbyUsers(
                shashank.Longitude,
                shashank.Latitude,
                0.10F);
            foreach (var user in nearbyUsers)
                Console.WriteLine($"long: {user.Longitude}\tlat: {user.Latitude}\tID: {user.Id}");

            var numUsers = 10000000;
            var numInteractions = 25000000;

            Console.WriteLine("Forest:");
            SimulateLoad(numUsers, numInteractions,
                new InteractionMonitor(new DisjointSetForest<int>()));

            Console.WriteLine("\nHash:");
            SimulateLoad(numUsers, numInteractions,
                new InteractionMonitor(new DisjointSetHash<int>()));

            //var matcher = new KeywordPrefixMatcher();
            //var simulator = new AutocompletionSimulator(matcher);
            //simulator.Menu();

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
        static void Populate(IBackend backend, int numUsers)
        {
            var rnd = new Random(123);
            for (int i = 0; i < numUsers; i++)
            {
                backend.AddUser(Convert.ToSingle(50 + 10 * rnd.NextDouble()),
                    Convert.ToSingle(5 + 10 * rnd.NextDouble()));
            }
        }

        static void SimulateLoad(int numUsers, int numInteractions,
            IInteractionMonitor monitor)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US", false);

            var watch = new Stopwatch();
            var rnd = new Random(123); // Constant seed for repeatable experiments
            Console.Write($"Creating {numUsers:n0} users...\t\t");

            watch.Start();
            for (var i = 0; i < numUsers; i++)
                monitor.RegisterUser(i);
            Console.WriteLine($"done in {watch.Elapsed.TotalSeconds:0.000}");

            Console.Write($"Performing {numInteractions:n0} interactions...\t");
            watch.Restart();
            for (var i = 0; i < numInteractions; i++)
            {
                var user1 = rnd.Next(numUsers);
                var user2 = rnd.Next(numUsers);
                monitor.RegisterInteraction(user1, user2);
            }
            Console.WriteLine($"done in {watch.Elapsed.TotalSeconds:0.000}");

            Console.Write($"Getting all interaction groups...\t");
            watch.Restart();
            var allGroups = monitor.GetAllInteractionGroups();
            Console.WriteLine($"done in {watch.Elapsed.TotalSeconds:0.000}");
            Console.WriteLine("Number of groups: {0:n0}", allGroups.Count());
        }
    }
}
