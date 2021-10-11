using DotNet.Generators;
using DotNet.models;
using DotNet.Visualisation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotNet.Solvers
{
    public abstract class Solver
    {
        protected const bool RANDOMIZE = true;
        public abstract List<PointPackage> Solve();
        public Generator MapGenerator;
        protected List<Package> Packages;
        protected List<PointPackage> Solution = new();
        protected int TruckX;
        protected int TruckY;
        protected int TruckZ;
        protected Vehicle Vehicle;

        protected bool CanFit(int x, int z, int y, (int a, int b, int c) perm)
        {
            if (x < 0 || z < 0 || y < 0 || x + perm.a > TruckX || z + perm.b > TruckZ || y + perm.c > TruckY)
                return false;
            foreach (var package in Solution)
            {
                if ((package.x5 > x && package.x1 < x + perm.a) && (package.y5 > y && package.y1 < y + perm.c) && (package.z5 > z && package.z1 < z + perm.b))
                    return false;
            }
            return true;
        }
        protected IEnumerable<(int a, int b, int c)> GetPermutaions(int[] list)
        {
            var perms = GetPerms(list);
            if (RANDOMIZE)
                perms = perms.OrderBy(item => Program.Random.Next());
            return perms;
            IEnumerable<(int a, int b, int c)> GetPerms(int[] list)
            {

                yield return new(list[0], list[1], list[2]);
                yield return new(list[0], list[2], list[1]);
                yield return new(list[1], list[0], list[2]);
                yield return new(list[1], list[2], list[0]);
                yield return new(list[2], list[1], list[0]);
                yield return new(list[2], list[0], list[1]);

            }
        }
        protected void Swap(ref int a, ref int b)
        {
            if (a == b) return;

            var temp = a;
            a = b;
            b = temp;
        }
        protected int Max(params int[] ints)
        {
            int ma = -1;
            foreach (var item in ints)
                ma = Math.Max(item, ma);
            return ma;
        }
        private SubmitResponse _bestSubmission = null;
        public void Submit(List<PointPackage> solution = null)
        {
            var submitSolution = MapGenerator.Submit(solution ?? Solution);
            if (submitSolution != null && submitSolution.Link != "visualizer.py")
            {
                Console.WriteLine("Your GameId is: " + submitSolution.GameId);
                Console.WriteLine("Your score is: " + submitSolution.Score);
                Console.WriteLine("Link to visualisation" + submitSolution.Link);
                if (_bestSubmission == null || _bestSubmission.Score < submitSolution.Score)
                {
                    Console.WriteLine("Saving solution");
                    CsvSaver.Save(Vehicle, solution ?? Solution);
                    _bestSubmission = submitSolution;
                }
            }
            else
            {
                Console.WriteLine("Saving solution");
                CsvSaver.Save(Vehicle, solution ?? Solution);
            }
        }
        protected List<PointPackage> DeepCopySolution(List<PointPackage> solution = null)
        {
            var solutionToCopy = solution ?? Solution;
            var newSolution = solutionToCopy.Select(item => new PointPackage
            {
                Id = item.Id,
                OrderClass = item.OrderClass,
                WeightClass = item.WeightClass,
                x1 = item.x1,
                x2 = item.x2,
                x3 = item.x3,
                x4 = item.x4,
                x5 = item.x5,
                x6 = item.x6,
                x7 = item.x7,
                x8 = item.x8,
                y1 = item.y1,
                y2 = item.y2,
                y3 = item.y3,
                y4 = item.y4,
                y5 = item.y5,
                y6 = item.y6,
                y7 = item.y7,
                y8 = item.y8,
                z1 = item.z1,
                z2 = item.z2,
                z3 = item.z3,
                z4 = item.z4,
                z5 = item.z5,
                z6 = item.z6,
                z7 = item.z7,
                z8 = item.z8,
            });
            return newSolution.ToList();
        }
    }
}
