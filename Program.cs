
using DotNet.Generators;
using DotNet.Solvers;
using System;

namespace DotNet
{
    public static class Program
    {
        public static Random Random = new Random();
        public static bool GENERATE_VISUAL_FILE = false;
        public static bool READ_MAP_FROM_FILE = false;

        public static void Main(string[] args)
        {
            Console.Write("Map: ");
            string map = Console.ReadLine();
            var generator = new LiveGenerator(map);
            Console.WriteLine("Starting");
            //var generator = new CyberMondayGenerator();
            var (vehicle, packages) = generator.ReadOrGenerateMap();
            Solver solver = new FullInnerPlacerSolver(packages, vehicle);
            solver.MapGenerator = generator;
            solver.Solve();
            solver.Submit();
        }
    }
}
