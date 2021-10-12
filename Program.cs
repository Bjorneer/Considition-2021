
using DotNet.Generators;
using DotNet.Solvers;
using System;

namespace DotNet
{
    public static class Program
    {
        public static Random Random = new Random();

        public static void Main(string[] args)
        {
            var generator = new LiveGenerator("training1");
            //var generator = new EasterGenerator();
            var (vehicle, packages) = generator.ReadOrGenerateMap();
            Solver solver = new GreedyGoodPlaceSolver(packages, vehicle);
            solver.MapGenerator = generator;
            solver.Solve();
            solver.Submit();
        }
    }
}
