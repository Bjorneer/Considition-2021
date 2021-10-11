
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
            var generator = new LiveGenerator();
            var (vehicle, packages) = generator.ReadOrGenerateMap();
            Solver solver = new InnerPlacerSolver(packages, vehicle);
            solver.MapGenerator = generator;
            solver.Solve();
            solver.Submit();
        }
    }
}
