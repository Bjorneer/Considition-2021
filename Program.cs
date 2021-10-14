
using DotNet.Generators;
using DotNet.Solvers;
using System;

namespace DotNet
{
    public static class Program
    {
        public static Random Random = new Random();
        public static bool GENERATE_VISUAL_FILE = false; // Set to true for for generating visualiazation, before use set filepath in CsvSaver
        public static bool READ_MAP_FROM_FILE = false; // Set to true to save map to file for reuse, before use set filepath in Generator

        public static void Main(string[] args)
        {
            /* Live api generator */
            Console.Write("Map: ");
            string map = Console.ReadLine();
            var generator = new LiveGenerator(map);
            Console.WriteLine("Starting");
            /**/
            //var generator = new CyberMondayGenerator(); // Custom generator
            var (vehicle, packages) = generator.ReadOrGenerateMap();
            Solver solver = new InnerPlacerSolver(packages, vehicle); // Solver
            solver.MapGenerator = generator;
            solver.Solve();
            solver.Submit();
        }
    }
}
