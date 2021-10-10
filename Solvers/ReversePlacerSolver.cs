﻿using DotNet.models;
using DotNet.Solvers;
using DotNet.Visualisation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet.Solvers
{
    public class ReversePlacerSolver : Solver
    {
        private const int MAX_X = 127;

        public ReversePlacerSolver(List<Package> packages, Vehicle vehicle)
        {
            Packages = packages;
            TruckX = vehicle.Length;
            TruckY = vehicle.Width;
            TruckZ = vehicle.Height;
        }
        private readonly int[] placeOrder = new int[] { 2, 4, 6, 5, 3, 1 };
        public override List<PointPackage> Solve()
        {
            Console.WriteLine("Heavy packages: " + Packages.Where(item => item.WeightClass == 2).Count());
            var groups = Packages.GroupBy(item => item.WeightClass == 2 ? 1 : 0).OrderByDescending(item => item.Key).Select(item => item.AsEnumerable()).ToList();
            //var groups = _packages.GroupBy(item => item.OrderClass).OrderByDescending(item => item.Key).Select(item => item.AsEnumerable()).ToList();
            int grpidx = 0;
            foreach (var group in groups)
            {
                var packages = group.OrderBy(item => placeOrder[item.OrderClass]).ThenByDescending(item => item.Height * item.Width * item.Length).ThenByDescending(item => Max(item.Width, item.Height, item.Length)).ThenByDescending(item => item.WeightClass);
                //var packages = group.OrderByDescending(item => Max(item.Width, item.Height, item.Length)).ThenByDescending(item => item.WeightClass);
                //var packages = group.OrderBy(item => item.OrderClass).ThenBy(item => item.WeightClass).ThenByDescending(item => Max(item.Width, item.Height, item.Length));

                foreach (var package in packages)
                {
                    Console.WriteLine($"Placing package ({Solution.Count() + 1}/{Packages.Count()}) with id: {package.Id}, group: {(char)((int)'A' + package.OrderClass)}, heavy: {package.WeightClass}");
                    Console.WriteLine($"Width: {package.Width}, Length: {package.Length}, Height: {package.Height}");
                    Pack(package);
                }
                ++grpidx;
            }
            Console.WriteLine("Repacking");
            foreach (var group in Packages.GroupBy(item => item.OrderClass).OrderBy(item => item.Key))
            {
                foreach (var package in group.OrderByDescending(item => item.WeightClass == 2 ? 1 : 0).ThenByDescending(item => Solution.First(sol => sol.Id == item.Id).x5))
                {
                    Repack(package);
                }
            }
            DropFloating();
            return Solution;
        }
        private void DropFloating()
        {
            Solution = Solution.OrderBy(item => item.z1).ToList();
            foreach (var package in Solution)
            {
                while (package.z1 != 0 && CanFit(package.x1, package.z1 - 1, package.y1, new(package.x5 - package.x1, 1, package.y5 - package.y1)))
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
        }
        private void Pack(Package package)
        {
            if (placeOrder[package.OrderClass] % 2 == 0)
            {
                PackReverese(package);
                return;
            }
            if (package.WeightClass == 2)
            {
                PackHeavy(package);
                return;
            }

            int bestX = int.MaxValue;
            PointPackage best = null;

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
            {
                for (int _x = 0; _x < TruckX - perm.a; _x++)
                {
                    if (bestX <= _x + perm.a)
                        break;
                    for (int _z = 0; _z < TruckZ - perm.b; _z++)
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
        private void PackHeavy(Package package)
        {
            int bestX = int.MaxValue;
            PointPackage best = null;

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
            {
                for (int _z = 0; _z < TruckZ - perm.b; _z++)
                {
                    if (best != null)
                        break;
                    for (int _x = 0; _x < MAX_X - perm.a; _x++)
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
        private void PackReverese(Package package)
        {
            if (package.WeightClass == 2)
            {
                PackHeavyReverse(package);
                return;
            }

            int bestX = int.MinValue;
            PointPackage best = null;

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
            {
                for (int _x = MAX_X - perm.a - 1; _x >= 0; _x--)
                {
                    if (_x <= bestX)
                        break;
                    for (int _z = 0; _z < TruckZ - perm.b; _z++)
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
        private void PackHeavyReverse(Package package)
        {
            int bestX = int.MinValue;
            PointPackage best = null;


            for (int _z = 0; _z < TruckZ; _z++)
            {
                if (best != null)
                    break;
                foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
                {
                    if (perm.b + _z >= TruckZ)
                        break;
                    for (int _x = MAX_X - perm.a - 1; _x >= 0; _x--)
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
                        break;
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
                    if (_x <= bestX)
                        break;
                    for (int _z = 0; _z < TruckZ - perm.b; _z++)
                    {
                        if (_x <= bestX)
                            break;
                        //for (int _y = _truckY - perm.c; _y >= 0; _y--)
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
    }
}