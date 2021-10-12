using DotNet.models;
using DotNet.Solvers;
using DotNet.Visualisation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet.Solvers
{
    public class InnerPlacerSolver : Solver
    {
        private readonly int MAX_X = 240; // Can be used to force heavys shorter
        public InnerPlacerSolver(List<Package> packages, Vehicle vehicle)
        {
            if (RANDOMIZE)
            {
                packages.ForEach(package =>
                {
                    int h = package.Height;
                    int l = package.Length;
                    int w = package.Width;
                    List<int> sz = new List<int> { h, l, w }.OrderBy(item => Program.Random.Next()).ToList();
                });
            }

            Packages = packages;
            TruckX = vehicle.Length;
            TruckY = vehicle.Width;
            TruckZ = vehicle.Height;
            MAX_X = TruckX; // can decrease perfomance
            Vehicle = vehicle;
        }
        public override List<PointPackage> Solve()
        {
            Console.WriteLine($"Packages: {Packages.Count()}");
            Console.WriteLine($"Heavy: {Packages.Where(item => item.WeightClass == 2).Count()}");
            Console.WriteLine($"Order counts (A,B,C,D,E): {string.Join(", ", Packages.GroupBy(item => item.OrderClass).OrderBy(item => item.Key).Select(item => item.Count().ToString()))}");
            Console.WriteLine($"Heavy counts (A,B,C,D,E): {string.Join(", ", Packages.Where(item => item.WeightClass == 2).GroupBy(item => item.OrderClass).OrderBy(item => item.Key).Select(item => item.Count().ToString()))}");

            var groups = Packages.GroupBy(item => item.OrderClass).OrderByDescending(item => item.Key).Select(item => item.AsEnumerable()).ToList();

            foreach (var group in groups)
            {
                foreach (var heavyPackage in group.Where(item => item.WeightClass == 2))
                {
                    PackHeavy(heavyPackage);
                }
                Pack(group.Where(item => item.WeightClass != 2));
            }
            for (int i = 0; i < 30; i++) // number of times to attempt repacking, each time can open up new spaces.
            {
                Submit(DropFloating(PushIn(PushBack(DeepCopySolution()))));
                Console.WriteLine("Repacking");
                foreach (var group in Packages.GroupBy(item => item.OrderClass).OrderBy(item => item.Key))
                {
                    foreach (var package in group.OrderByDescending(item => item.WeightClass == 2 ? 1 : 0).ThenByDescending(item => Solution.First(sol => sol.Id == item.Id).x5))
                    {
                        Repack(package);
                    }
                }
                Submit(DropFloating(PushIn(PushBack(DeepCopySolution()))));
                Console.WriteLine("Repacking reverse");
                foreach (var group in Packages.GroupBy(item => item.OrderClass).OrderByDescending(item => item.Key))
                {
                    foreach (var package in group.OrderByDescending(item => item.WeightClass == 2 ? 1 : 0).ThenBy(item => Solution.First(sol => sol.Id == item.Id).x1))
                    {
                        RepackReverse(package);
                    }
                }
                PushBack(); // These two may or may not help improve score
                PushIn();
            }
            DropFloating(PushIn(PushBack()));
            return Solution;
        }
        private void Pack(IEnumerable<Package> group)
        {
            if (group.Count() == 0)
                return;
            int minPrevX = Solution.Where(item => item.OrderClass != group.First().OrderClass).Count() == 0 ? 0 : Solution.Where(item => item.OrderClass != group.First().OrderClass).Max(item => item.x5 - 1);
            List<Package> packagesLeft = new List<Package>();

            foreach (var package in group.OrderByDescending(item => item.Length * item.Height * item.Width))
            {
                PointPackage best = null;
                for (int _x = minPrevX; _x >= 0; _x--)
                {
                    foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
                    {
                        if (_x - perm.a < 0)
                            continue;
                        for (int _z = TruckZ - perm.b; _z >= 0; _z--)
                        {
                            for (int _y = 0; _y < TruckY - perm.c; _y++)
                            {
                                if (CanFit(_x - perm.a + 1, _z, _y, perm))
                                {
                                    best = new PointPackage()
                                    {
                                        Id = package.Id,
                                        x1 = _x - perm.a + 1,
                                        x2 = _x - perm.a + 1,
                                        x3 = _x - perm.a + 1,
                                        x4 = _x - perm.a + 1,
                                        x5 = _x + 1,
                                        x6 = _x + 1,
                                        x7 = _x + 1,
                                        x8 = _x + 1,
                                        y1 = _y,
                                        y2 = _y,
                                        y3 = _y,
                                        y4 = _y,
                                        y5 = _y + perm.c,
                                        y6 = _y + perm.c,
                                        y7 = _y + perm.c,
                                        y8 = _y + perm.c,
                                        z1 = _z,
                                        z2 = _z,
                                        z3 = _z,
                                        z4 = _z,
                                        z5 = _z + perm.b,
                                        z6 = _z + perm.b,
                                        z7 = _z + perm.b,
                                        z8 = _z + perm.b,
                                        OrderClass = package.OrderClass,
                                        WeightClass = package.WeightClass
                                    };
                                }
                            }
                        }
                    }
                }
                if (best != null)
                {
                    Console.WriteLine($"Placing package ({Solution.Count() + 1}/{Packages.Count()}) with id: {package.Id}, group: {(char)((int)'A' + package.OrderClass)}, heavy: {package.WeightClass}");
                    Console.WriteLine($"Width: {package.Width}, Length: {package.Length}, Height: {package.Height}");
                    Solution.Add(best);
                }
                else
                {
                    //packagesLeft.Add(package);
                    PackNormal(package);
                }
            }
            foreach (var package in packagesLeft)
            {
                PackNormal(package);
            }
        }
        private void PackNormal(Package package)
        {
            if (package.WeightClass == 2)
            {
                PackHeavy(package);
                return;
            }

            int bestX = int.MaxValue;
            PointPackage best = null;

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
            {
                for (int _x = 0; _x < MAX_X - perm.a; _x++)
                {
                    if (bestX <= _x + perm.a)
                        break;
                    for (int _z = TruckZ - perm.b; _z >= 0; _z--)
                    {
                        if (bestX <= _x + perm.a)
                            break;
                        for (int _y = 0; _y < TruckY - perm.c; _y++)
                        {
                            if (bestX <= _x + perm.a)
                                break;
                            if (CanFit(_x, _z, _y, perm))
                            {
                                bestX = _x + perm.a;
                                best = new PointPackage()
                                {
                                    Id = package.Id,
                                    x1 = _x,
                                    x2 = _x,
                                    x3 = _x,
                                    x4 = _x,
                                    x5 = _x + perm.a,
                                    x6 = _x + perm.a,
                                    x7 = _x + perm.a,
                                    x8 = _x + perm.a,
                                    y1 = _y,
                                    y2 = _y,
                                    y3 = _y,
                                    y4 = _y,
                                    y5 = _y + perm.c,
                                    y6 = _y + perm.c,
                                    y7 = _y + perm.c,
                                    y8 = _y + perm.c,
                                    z1 = _z,
                                    z2 = _z,
                                    z3 = _z,
                                    z4 = _z,
                                    z5 = _z + perm.b,
                                    z6 = _z + perm.b,
                                    z7 = _z + perm.b,
                                    z8 = _z + perm.b,
                                    OrderClass = package.OrderClass,
                                    WeightClass = package.WeightClass
                                };
                            }
                        }
                    }
                }
            }
            if (best == null)
            {
                CsvSaver.Save(new Vehicle
                {
                    Height = TruckZ,
                    Width = TruckY,
                    Length = TruckX
                }, Solution);
                throw new Exception("Could not place package");
            }
            Console.WriteLine($"Placing package ({Solution.Count() + 1}/{Packages.Count()}) with id: {package.Id}, group: {(char)((int)'A' + package.OrderClass)}, heavy: {package.WeightClass}");
            Console.WriteLine($"Width: {package.Width}, Length: {package.Length}, Height: {package.Height}");
            Solution.Add(best);
        }
        private void PackHeavy(Package package)
        {
            int bestX = int.MaxValue;
            PointPackage best = null;

            for (int _z = 0; _z < TruckZ; _z++)
            {
                if (best != null)
                    break;
                foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
                {
                    if (_z + perm.b >= TruckZ)
                        continue;
                    for (int _x = 0; _x < MAX_X - perm.a; _x++)
                    {
                        if (bestX <= _x + perm.a)
                            break;
                        //for (int _y = _truckY - perm.c; _y >= 0; _y--)
                        for (int _y = 0; _y < TruckY - perm.c; _y++)
                        {
                            if (bestX <= _x + perm.a)
                                break;
                            if (CanFit(_x, _z, _y, perm))
                            {
                                bestX = _x + perm.a;
                                best = new PointPackage()
                                {
                                    Id = package.Id,
                                    x1 = _x,
                                    x2 = _x,
                                    x3 = _x,
                                    x4 = _x,
                                    x5 = _x + perm.a,
                                    x6 = _x + perm.a,
                                    x7 = _x + perm.a,
                                    x8 = _x + perm.a,
                                    y1 = _y,
                                    y2 = _y,
                                    y3 = _y,
                                    y4 = _y,
                                    y5 = _y + perm.c,
                                    y6 = _y + perm.c,
                                    y7 = _y + perm.c,
                                    y8 = _y + perm.c,
                                    z1 = _z,
                                    z2 = _z,
                                    z3 = _z,
                                    z4 = _z,
                                    z5 = _z + perm.b,
                                    z6 = _z + perm.b,
                                    z7 = _z + perm.b,
                                    z8 = _z + perm.b,
                                    OrderClass = package.OrderClass,
                                    WeightClass = package.WeightClass
                                };
                            }
                        }
                    }
                }
            }
            if (best == null)
            {
                CsvSaver.Save(new Vehicle
                {
                    Height = TruckZ,
                    Width = TruckY,
                    Length = TruckX
                }, Solution);
                throw new Exception("Could not place package");
            }
            Console.WriteLine($"Placing package ({Solution.Count() + 1}/{Packages.Count()}) with id: {package.Id}, group: {(char)((int)'A' + package.OrderClass)}, heavy: {package.WeightClass}");
            Console.WriteLine($"Width: {package.Width}, Length: {package.Length}, Height: {package.Height}");
            Solution.Add(best);
        }
        private void Repack(Package package)
        {
            int maxX = Solution.Where(item => item.OrderClass == package.OrderClass).Max(item => item.x5);
            //int maxX = package.OrderClass == 0 ? _solution.Max(item => item.x5) : Math.Max(_solution.Where(item => item.OrderClass == package.OrderClass - 1).Min(item => item.x5), _solution.Where(item => item.OrderClass == package.OrderClass).Max(item => item.x5));
            Solution.Remove(Solution.First(item => item.Id == package.Id));
            if (package.WeightClass == 2)
            {
                RepackHeavy(package, maxX);
            }
            else
            {
                RepackNormal(package, maxX);
            }
        }
        private void RepackHeavy(Package package, int maxX)
        {
            int bestX = int.MinValue;
            PointPackage best = null;
            for (int _z = 0; _z < TruckZ; _z++)
            {
                if (best != null)
                    break;
                foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
                {
                    if (perm.b + _z > TruckZ)
                        continue;
                    for (int _x = maxX - perm.a; _x >= 0; _x--)
                    {
                        if (_x <= bestX)
                            break;
                        for (int _y = 0; _y < TruckY - perm.c; _y++)
                        {
                            if (_x <= bestX)
                                break;
                            if (CanFit(_x, _z, _y, perm))
                            {
                                bestX = _x;
                                best = new PointPackage()
                                {
                                    Id = package.Id,
                                    x1 = _x,
                                    x2 = _x,
                                    x3 = _x,
                                    x4 = _x,
                                    x5 = _x + perm.a,
                                    x6 = _x + perm.a,
                                    x7 = _x + perm.a,
                                    x8 = _x + perm.a,
                                    y1 = _y,
                                    y2 = _y,
                                    y3 = _y,
                                    y4 = _y,
                                    y5 = _y + perm.c,
                                    y6 = _y + perm.c,
                                    y7 = _y + perm.c,
                                    y8 = _y + perm.c,
                                    z1 = _z,
                                    z2 = _z,
                                    z3 = _z,
                                    z4 = _z,
                                    z5 = _z + perm.b,
                                    z6 = _z + perm.b,
                                    z7 = _z + perm.b,
                                    z8 = _z + perm.b,
                                    OrderClass = package.OrderClass,
                                    WeightClass = package.WeightClass
                                };
                            }
                        }
                    }
                }
            }
            if (best == null)
            {
                CsvSaver.Save(new Vehicle
                {
                    Height = TruckZ,
                    Width = TruckY,
                    Length = TruckX
                }, Solution);
                throw new Exception("Could not place package");
            }
            Solution.Add(best);
        }
        private void RepackNormal(Package package, int maxX)
        {
            int bestX = int.MinValue;
            PointPackage best = null;

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
            {
                for (int _x = maxX - perm.a; _x >= 0; _x--)
                {
                    if (_x + perm.a <= bestX)
                        break;
                    for (int _z = TruckZ - perm.b; _z >= 0; _z--)
                    {
                        if (_x + perm.a <= bestX)
                            break;
                        //for (int _y = _truckY - perm.c; _y >= 0; _y--)
                        for (int _y = 0; _y < TruckY - perm.c; _y++)
                        {
                            if (_x + perm.a <= bestX)
                                break;
                            if (CanFit(_x, _z, _y, perm))
                            {
                                bestX = _x + perm.a;
                                best = new PointPackage()
                                {
                                    Id = package.Id,
                                    x1 = _x,
                                    x2 = _x,
                                    x3 = _x,
                                    x4 = _x,
                                    x5 = _x + perm.a,
                                    x6 = _x + perm.a,
                                    x7 = _x + perm.a,
                                    x8 = _x + perm.a,
                                    y1 = _y,
                                    y2 = _y,
                                    y3 = _y,
                                    y4 = _y,
                                    y5 = _y + perm.c,
                                    y6 = _y + perm.c,
                                    y7 = _y + perm.c,
                                    y8 = _y + perm.c,
                                    z1 = _z,
                                    z2 = _z,
                                    z3 = _z,
                                    z4 = _z,
                                    z5 = _z + perm.b,
                                    z6 = _z + perm.b,
                                    z7 = _z + perm.b,
                                    z8 = _z + perm.b,
                                    OrderClass = package.OrderClass,
                                    WeightClass = package.WeightClass
                                };
                            }
                        }
                    }
                }
            }
            if (best == null)
            {
                CsvSaver.Save(new Vehicle
                {
                    Height = TruckZ,
                    Width = TruckY,
                    Length = TruckX
                }, Solution);
                throw new Exception("Could not place package");
            }
            Solution.Add(best);
        }
        private void RepackReverse(Package package)
        {
            var pointPackage = Solution.First(item => item.Id == package.Id);
            var prevClassPackages = Solution.Where(item => item.OrderClass < package.OrderClass && (item.x1 < pointPackage.x1 || (item.x1 == pointPackage.x1 && pointPackage.z1 > item.z1)));
            if (prevClassPackages.Count() == 0) prevClassPackages = null;
            int minX = Math.Min(
                Solution.Where(item => item.OrderClass == package.OrderClass).Min(item => item.x1),
                prevClassPackages?.Max(item => item.x1) ?? int.MaxValue);
            Solution.Remove(pointPackage);
            if (package.WeightClass == 2)
            {
                RepackHeavyReverse(package, minX);
            }
            else
            {
                RepackNormalReverse(package, minX);
            }
        }
        private void RepackHeavyReverse(Package package, int minX)
        {
            int bestX = int.MaxValue;
            PointPackage best = null;

            for (int _z = 0; _z < TruckZ; _z++)
            {
                if (best != null)
                    break;
                foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
                {
                    if (_z + perm.b >= TruckZ)
                        continue;
                    for (int _x = minX; _x < MAX_X - perm.a; _x++)
                    {
                        if (bestX <= _x + perm.a)
                            break;
                        //for (int _y = _truckY - perm.c; _y >= 0; _y--)
                        for (int _y = 0; _y < TruckY - perm.c; _y++)
                        {
                            if (bestX <= _x + perm.a)
                                break;
                            if (CanFit(_x, _z, _y, perm))
                            {
                                bestX = _x + perm.a;
                                best = new PointPackage()
                                {
                                    Id = package.Id,
                                    x1 = _x,
                                    x2 = _x,
                                    x3 = _x,
                                    x4 = _x,
                                    x5 = _x + perm.a,
                                    x6 = _x + perm.a,
                                    x7 = _x + perm.a,
                                    x8 = _x + perm.a,
                                    y1 = _y,
                                    y2 = _y,
                                    y3 = _y,
                                    y4 = _y,
                                    y5 = _y + perm.c,
                                    y6 = _y + perm.c,
                                    y7 = _y + perm.c,
                                    y8 = _y + perm.c,
                                    z1 = _z,
                                    z2 = _z,
                                    z3 = _z,
                                    z4 = _z,
                                    z5 = _z + perm.b,
                                    z6 = _z + perm.b,
                                    z7 = _z + perm.b,
                                    z8 = _z + perm.b,
                                    OrderClass = package.OrderClass,
                                    WeightClass = package.WeightClass
                                };
                            }
                        }
                    }
                }
            }
            if (best == null)
            {
                CsvSaver.Save(new Vehicle
                {
                    Height = TruckZ,
                    Width = TruckY,
                    Length = TruckX
                }, Solution);
                throw new Exception("Could not place package");
            }
            Solution.Add(best);
        }
        private void RepackNormalReverse(Package package, int minX)
        {
            int bestX = int.MaxValue;
            PointPackage best = null;

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
            {
                for (int _x = minX; _x < MAX_X - perm.a; _x++)
                {
                    if (bestX <= _x + perm.a)
                        break;
                    for (int _z = TruckZ - perm.b; _z >= 0; _z--)
                    {
                        if (bestX <= _x + perm.a)
                            break;
                        for (int _y = 0; _y < TruckY - perm.c; _y++)
                        {
                            if (bestX <= _x + perm.a)
                                break;
                            if (CanFit(_x, _z, _y, perm))
                            {
                                bestX = _x + perm.a;
                                best = new PointPackage()
                                {
                                    Id = package.Id,
                                    x1 = _x,
                                    x2 = _x,
                                    x3 = _x,
                                    x4 = _x,
                                    x5 = _x + perm.a,
                                    x6 = _x + perm.a,
                                    x7 = _x + perm.a,
                                    x8 = _x + perm.a,
                                    y1 = _y,
                                    y2 = _y,
                                    y3 = _y,
                                    y4 = _y,
                                    y5 = _y + perm.c,
                                    y6 = _y + perm.c,
                                    y7 = _y + perm.c,
                                    y8 = _y + perm.c,
                                    z1 = _z,
                                    z2 = _z,
                                    z3 = _z,
                                    z4 = _z,
                                    z5 = _z + perm.b,
                                    z6 = _z + perm.b,
                                    z7 = _z + perm.b,
                                    z8 = _z + perm.b,
                                    OrderClass = package.OrderClass,
                                    WeightClass = package.WeightClass
                                };
                            }
                        }
                    }
                }
            }
            if (best == null)
            {
                CsvSaver.Save(new Vehicle
                {
                    Height = TruckZ,
                    Width = TruckY,
                    Length = TruckX
                }, Solution);
                throw new Exception("Could not place package");
            }
            Solution.Add(best);
        }
    }
}