
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using DotNet.models;
using DotNet.Responses;

namespace DotNet
{
    public static class Program
    {
        private const string ApiKey = "510c78d2-d786-41aa-b327-d6902d965217";  // TODO: Enter your API key
        // The different map names can be found on considition.com/rules
        private const string Map = "training1";     // TODO: Enter your desired map
        private static readonly GameLayer GameLayer = new(ApiKey);
        
        public static void Main(string[] args)
        {
            var gameInformation = GameLayer.NewGame(Map);
            GreedyGoodPlaceSolver greedySolver = new GreedyGoodPlaceSolver(gameInformation.Dimensions, gameInformation.Vehicle);
            var solution = greedySolver.Solve();
            var submitSolution = GameLayer.Submit(JsonSerializer.Serialize(solution), Map);
            Console.WriteLine("Your GameId is: " + submitSolution.GameId);
            Console.WriteLine("Your score is: " + submitSolution.Score);
            /*
            var alg = new EB_AFIT();
            var result = alg.Run(new Container(0, gameInformation.Vehicle.Height, gameInformation.Vehicle.Length, gameInformation.Vehicle.Width), gameInformation.Dimensions.Select(item => new Item(item.Id, item.Width, item.Height, item.Length, 1)).ToList());
            var algResult = 
result.PackedItems.Select(item => new PointPackage
            {
                Id = item.ID,
                x1 = (int)item.CoordX,
                x2 = (int)item.CoordX,
                x3 = (int)item.CoordX,
                x4 = (int)item.CoordX,
                x5 = (int)item.CoordX + (int)item.CoordX,
                x6 = (int)item.CoordX + (int)item.CoordX,
                x7 = (int)item.CoordX + (int)item.CoordX,
                x8 = (int)item.CoordX + (int)item.CoordX,
                y1 = (int)item.CoordY,
                y2 = (int)item.CoordY,
                y3 = (int)item.CoordY,
                y4 = (int)item.CoordY,
                y5 = (int)item.CoordY + (int)item.CoordY,
                y6 = (int)item.CoordY + (int)item.CoordY,
                y7 = (int)item.CoordY + (int)item.CoordY,
                y8 = (int)item.CoordY + (int)item.CoordY,
                z1 = (int)item.CoordZ,
                z2 = (int)item.CoordZ,
                z3 = (int)item.CoordZ,
                z4 = (int)item.CoordZ,
                z5 = (int)item.CoordZ + (int)item.CoordZ,
                z6 = (int)item.CoordZ + (int)item.CoordZ,
                z7 = (int)item.CoordZ + (int)item.CoordZ,
                z8 = (int)item.CoordZ + (int)item.CoordZ,
            }).ToList();
            */
            //submitSolution = GameLayer.Submit(JsonSerializer.Serialize(algResult.ToList()), Map);
            Console.WriteLine("Link to visualisation" + submitSolution.Link);
        }
    }
}
