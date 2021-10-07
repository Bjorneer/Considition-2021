
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using DotNet.models;
using DotNet.Responses;
using DotNet.Solvers;
using DotNet.Visualisation;

namespace DotNet
{
    public static class Program
    {
        public static Random Random = new Random();

        private const string ApiKey = "510c78d2-d786-41aa-b327-d6902d965217";  // TODO: Enter your API key
        // The different map names can be found on considition.com/rules
        private const string Map = "training2";     // TODO: Enter your desired map
        private static readonly GameLayer GameLayer = new(ApiKey);
        
        public static void Main(string[] args)
        {
            var gameInformation = GameLayer.NewGame(Map);
            ISolver greedySolver = new Corner4Solver(gameInformation.Dimensions, gameInformation.Vehicle);
            var solution = greedySolver.Solve();
            var submitSolution = GameLayer.Submit(JsonSerializer.Serialize(solution), Map);
            Console.WriteLine("Your GameId is: " + submitSolution.GameId);
            Console.WriteLine("Your score is: " + submitSolution.Score);
            Console.WriteLine("Link to visualisation" + submitSolution.Link);
            Console.WriteLine("MaxLength: " + solution.Select(item => item.x5).Max());
            CsvSaver.Save(gameInformation.Vehicle, solution);
        }
    }
}
