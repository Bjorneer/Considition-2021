﻿using DotNet.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Generators
{
    public abstract class Generator
    {
        protected Vehicle Vehicle;
        protected List<Package> Packages = new List<Package>();
        protected abstract string Map { get; }
        protected abstract void ReGenerate();
        private int OrderScore(List<PointPackage> solution)
        {
            var submissionOrder = solution.OrderByDescending(item => item.x1).ThenByDescending(item => item.z1).ThenBy(item => item.OrderClass).ToList(); 
            // Third sort argument is missing from docs, Also sorting by x1 doesnt quite make alot of sense but thats whats used
            var perfectOrder = solution.OrderBy(item => item.OrderClass).ToList();
            int orderScore = 0;
            for (int i = 0; i < perfectOrder.Count; i++)
            {
                var diff = Math.Abs(perfectOrder[i].OrderClass - submissionOrder[i].OrderClass);
                switch (diff)
                {
                    case 0:
                        orderScore += 20;
                        break;
                    case 1:
                        orderScore += 10;
                        break;
                    case 2:
                        orderScore += 2;
                        break;
                    case 3:
                        orderScore -= 10;
                        break;
                    case 4:
                        orderScore -= 20;
                        break;
                }
            }
            return orderScore;
        }
        public virtual SubmitResponse Submit(List<PointPackage> solution)
        {
            // Score calc assumes 1000 weight
            Console.WriteLine("Submission for non live map created");
            Console.WriteLine($"Order score: {OrderScore(solution)} / {solution.Count * 20}");
            Console.WriteLine($"Weight score: 1000 (non calculated)");
            Console.WriteLine($"Packing efficency: {Math.Round(1 + (Packages.Sum(item => item.Width * item.Height * item.Length) / (double)(solution.Max(item => item.x5) * solution.Max(item => item.y5) * solution.Max(item => item.z5))), 2) }");
            Console.WriteLine($"Length score: {10 * (Vehicle.Length - solution.Max(item => item.x5) )} / {Vehicle.Length * 10}");
            Console.WriteLine($"Total score: {(int)((1000 + OrderScore(solution) + 10 * (Vehicle.Length - solution.Max(item => item.x5))) * (1 + (Packages.Sum(item => item.Width * item.Height * item.Length) / (double)(solution.Max(item => item.x5) * solution.Max(item => item.y5) * solution.Max(item => item.z5))))) }");
            return new SubmitResponse()
            {
                Link = "visualizer.py",
                GameId = Guid.NewGuid().ToString(),
                valid = true,
                Score = (int)((1000 + OrderScore(solution) + 10 * (Vehicle.Length - solution.Max(item => item.x5))) * (1 + (Packages.Sum(item => item.Width * item.Height * item.Length) / (double)(solution.Max(item => item.x5) * solution.Max(item => item.y5) * solution.Max(item => item.z5)))))
            };
        }
        public (Vehicle vehicle, List<Package> packages) ReadOrGenerateMap()
        {
            string path = $"C:\\src\\Considition-2021\\Generators\\SavedMaps\\{Map}.txt"; // SavedMaps folder is excluded from git but will be generated
            if (File.Exists(path))
            {
                string mapContent = File.ReadAllText(path);
                var splitContent = mapContent.Split('\n');
                var vehicleSizeStr = splitContent.First().Split();
                Vehicle = new Vehicle
                {
                    Length = int.Parse(vehicleSizeStr[0]),
                    Width = int.Parse(vehicleSizeStr[1]),
                    Height = int.Parse(vehicleSizeStr[2])
                };
                int idCnt = 0;
                foreach (var packageStr in splitContent.Skip(1))
                {
                    if (string.IsNullOrEmpty(packageStr))
                        continue;
                    var packageSizeStr = packageStr.Split();
                    Packages.Add(new Package
                    {
                        Id = idCnt++,
                        Length = int.Parse(packageSizeStr[0]),
                        Width = int.Parse(packageSizeStr[1]),
                        Height = int.Parse(packageSizeStr[2]),
                        OrderClass = int.Parse(packageSizeStr[3]),
                        WeightClass = int.Parse(packageSizeStr[4])
                    });
                }
            }
            else
            {
                ReGenerate();
                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    writer.WriteLine($"{Vehicle.Length} {Vehicle.Width} {Vehicle.Height}");
                    foreach (var package in Packages)
                    {
                        writer.WriteLine($"{package.Length} {package.Width} {package.Height} {package.OrderClass} {package.WeightClass}");
                    }
                    writer.Close();
                }
            }
            return (Vehicle, Packages);
        }
    }
}
