using DotNet.Generators;
using DotNet.models;
using DotNet.Visualization;
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
        protected const bool RANDOMIZE = true; // Should be true for best solution
        public abstract List<PointPackage> Solve();
        public Generator MapGenerator;
        protected List<Package> Packages;
        protected List<PointPackage> Solution = new();
        protected int TruckX;
        protected int TruckY;
        protected int TruckZ;
        protected Vehicle Vehicle;

        protected bool CanFit(int x, int z, int y, (int a, int b, int c) perm, List<PointPackage> solution = null)
        {
            solution ??= Solution;
            if (x < 0 || z < 0 || y < 0 || x + perm.a > TruckX || z + perm.b > TruckZ || y + perm.c > TruckY)
                return false;
            foreach (var package in solution)
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
            CsvSaver.Save(Vehicle, solution ?? Solution);
            var submitSolution = MapGenerator.Submit(solution ?? Solution);
            if (submitSolution != null && submitSolution.Link != "visualizer.py")
            {
                Console.WriteLine("Your GameId is: " + submitSolution.GameId);
                Console.WriteLine("Your score is: " + submitSolution.Score);
                Console.WriteLine("Link to visualisation" + submitSolution.Link);
            }
            if (_bestSubmission == null || _bestSubmission.Score < submitSolution.Score)
            {
                Console.WriteLine("Saving solution for local visualasation.");
                CsvSaver.Save(Vehicle, solution ?? Solution);
                _bestSubmission = submitSolution;
            }
        }
        protected List<PointPackage> DeepCopySolution(List<PointPackage> solution = null)
        {
            var solutionToCopy = solution ?? Solution;
            var newSolution = new List<PointPackage>();
            foreach (var item in solutionToCopy)
            {
                newSolution.Add(new PointPackage
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
            }
            return newSolution.ToList();
        }
        protected List<PointPackage> DropFloating(List<PointPackage> solution = null)
        {
            solution ??= Solution;
            solution = solution.OrderBy(item => item.z1).ToList();
            foreach (var package in solution)
            {
                while (package.z1 != 0 && CanFit(package.x1, package.z1 - 1, package.y1, new(package.x5 - package.x1, 1, package.y5 - package.y1), solution))
                {
                    package.z1 = package.z1 - 1;
                    package.z2 = package.z1;
                    package.z3 = package.z1;
                    package.z4 = package.z1;
                    package.z5 = package.z5 - 1;
                    package.z6 = package.z5;
                    package.z7 = package.z5;
                    package.z8 = package.z5;
                }
            }
            return solution;
        }
        protected List<PointPackage> PushBack(List<PointPackage> solution = null)
        {
            solution = solution ?? Solution;
            solution = solution.OrderBy(item => item.x1).ToList();
            foreach (var package in solution)
            {
                while (package.x1 != 0 && CanFit(package.x1 - 1, package.z1, package.y1, new(1, package.z5 - package.z1, package.y5 - package.y1), solution))
                {
                    package.x1 = package.x1 - 1;
                    package.x2 = package.x1;
                    package.x3 = package.x1;
                    package.x4 = package.x1;
                    package.x5 = package.x5 - 1;
                    package.x6 = package.x5;
                    package.x7 = package.x5;
                    package.x8 = package.x5;
                }
            }
            return solution;
        }
        protected List<PointPackage> PushIn(List<PointPackage> solution = null)
        {
            solution = solution ?? Solution;
            solution = solution.OrderBy(item => item.y1).ToList();
            foreach (var package in solution)
            {
                while (package.y1 != 0 && CanFit(package.x1, package.z1, package.y1 - 1, new(package.x5 - package.x1, package.z5 - package.z1, 1), solution))
                {
                    package.y1 = package.y1 - 1;
                    package.y2 = package.y1;
                    package.y3 = package.y1;
                    package.y4 = package.y1;
                    package.y5 = package.y5 - 1;
                    package.y6 = package.y5;
                    package.y7 = package.y5;
                    package.y8 = package.y5;
                }
            }
            return solution;
        }
    }
}
