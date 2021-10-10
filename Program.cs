
using DotNet.Solvers;
using DotNet.Visualisation;
using System;
using System.Linq;
using System.Text.Json;

namespace DotNet
{
    public static class Program
    {
        public static Random Random = new Random();

        private const string ApiKey = "510c78d2-d786-41aa-b327-d6902d965217";  // TODO: Enter your API key
        public const string Map = "training1";     // TODO: Enter your desired map
        public static readonly GameLayer GameLayer = new(ApiKey);

        public static void Main(string[] args)
        {
            var gameInformation = GameLayer.NewGame(Map);
            Solver solver = new InnerPlacerSolver(gameInformation.Dimensions, gameInformation.Vehicle);
            var solution = solver.Solve();
            solver.Submit();
        }
    }
}
