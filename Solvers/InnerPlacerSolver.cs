using DotNet.models;
using DotNet.Solvers;
using DotNet.Visualisation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet.Solvers
{
    public class InnerPlacerSolver : ISolver
    {
        #region Parameters
        private readonly int MAX_X = 240; // Can be used to force heavys shorter
        private const bool RANDOMIZE = false;
        #endregion  

        private List<Package> _packages;
        private List<PointPackage> _solution = new();
        private readonly int _truckX;
        private readonly int _truckY;
        private readonly int _truckZ;

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

            _packages = packages;
            _truckX = vehicle.Length;
            _truckY = vehicle.Width;
            _truckZ = vehicle.Height;
            MAX_X = _truckX; // can decrease perfomance
        }
        public List<PointPackage> Solve()
        {
            Console.WriteLine($"Packages: {_packages.Count()}");
            Console.WriteLine($"Heavy: {_packages.Where(item => item.WeightClass == 2).Count()}");
            Console.WriteLine($"Order counts (A,B,C,D,E): {string.Join(", ", _packages.GroupBy(item => item.OrderClass).OrderBy(item => item.Key).Select(item => item.Count().ToString()))}");
            Console.WriteLine($"Heavy counts (A,B,C,D,E): {string.Join(", ", _packages.Where(item => item.WeightClass == 2).GroupBy(item => item.OrderClass).OrderBy(item => item.Key).Select(item => item.Count().ToString()))}");

            var groups = _packages.GroupBy(item => item.OrderClass).OrderByDescending(item => item.Key).Select(item => item.AsEnumerable()).ToList();

            foreach (var group in groups)
            {
                foreach (var heavyPackage in group.Where(item => item.WeightClass == 2))
                {
                    PackHeavy(heavyPackage);
                }
                Pack(group.Where(item => item.WeightClass != 2));
            }
            //DropFloating(); // Usually does not improve performance
            groups.Reverse();
            Console.WriteLine("Repacking");
            foreach (var group in groups)
            {
                foreach (var package in group.OrderByDescending(item => item.WeightClass == 2 ? 1 : 0).ThenByDescending(item => _solution.First(sol => sol.Id == item.Id).x5))
                {
                    Repack(package);
                }
            }
            DropFloating();
            return _solution;
        }

        private void Pack(IEnumerable<Package> group)
        {
            if (group.Count() == 0)
                return;
            int minPrevX = _solution.Where(item => item.OrderClass != group.First().OrderClass).Count() == 0 ? 0 : _solution.Where(item => item.OrderClass != group.First().OrderClass).Max(item => item.x5 - 1);
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
                        for (int _z = _truckZ - perm.b; _z >= 0; _z--)
                        {
                            for (int _y = 0; _y < _truckY - perm.c; _y++)
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
                    Console.WriteLine($"Placing package ({_solution.Count() + 1}/{_packages.Count()}) with id: {package.Id}, group: {(char)((int)'A' + package.OrderClass)}, heavy: {package.WeightClass}");
                    Console.WriteLine($"Width: {package.Width}, Length: {package.Length}, Height: {package.Height}");
                    _solution.Add(best);
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
        private void DropFloating()
        {
            _solution = _solution.OrderBy(item => item.z1).ToList();
            foreach (var package in _solution)
            {
                while (package.z1 != 0 && CanFit(package.x1, package.z1 - 1, package.y1, new (package.x5 - package.x1, 1, package.y5 - package.y1)))
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
                    for (int _z = _truckZ - perm.b; _z >= 0; _z--)
                    {
                        if (bestX <= _x + perm.a)
                            break;
                        for (int _y = 0; _y < _truckY - perm.c; _y++)
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
                    Height = _truckZ,
                    Width = _truckY,
                    Length = _truckX
                }, _solution);
                throw new Exception("Could not place package");
            }
            Console.WriteLine($"Placing package ({_solution.Count() + 1}/{_packages.Count()}) with id: {package.Id}, group: {(char)((int)'A' + package.OrderClass)}, heavy: {package.WeightClass}");
            Console.WriteLine($"Width: {package.Width}, Length: {package.Length}, Height: {package.Height}");
            _solution.Add(best);
        }
        private void PackHeavy(Package package)
        {
            int bestX = int.MaxValue;
            PointPackage best = null;

            for (int _z = 0; _z < _truckZ; _z++)
            {
                if (best != null)
                    break;
                foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
                {
                    if (_z + perm.b >= _truckZ)
                        break;
                    for (int _x = 0; _x < MAX_X - perm.a; _x++)
                    {
                        if (bestX <= _x + perm.a)
                            break;
                        //for (int _y = _truckY - perm.c; _y >= 0; _y--)
                        for (int _y = 0; _y < _truckY - perm.c; _y++)
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
                    Height = _truckZ,
                    Width = _truckY,
                    Length = _truckX
                }, _solution);
                throw new Exception("Could not place package");
            }
            Console.WriteLine($"Placing package ({_solution.Count() + 1}/{_packages.Count()}) with id: {package.Id}, group: {(char)((int)'A' + package.OrderClass)}, heavy: {package.WeightClass}");
            Console.WriteLine($"Width: {package.Width}, Length: {package.Length}, Height: {package.Height}");
            _solution.Add(best);
        }
        private void Repack(Package package)
        {
            int maxX = _solution.Where(item => item.OrderClass == package.OrderClass).Max(item => item.x5);
            //int maxX = package.OrderClass == 0 ? _solution.Max(item => item.x5) : Math.Max(_solution.Where(item => item.OrderClass == package.OrderClass - 1).Min(item => item.x5), _solution.Where(item => item.OrderClass == package.OrderClass).Max(item => item.x5));
            _solution.Remove(_solution.First(item => item.Id == package.Id));
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
            for (int _z = 0; _z < _truckZ; _z++)
            {
                if (best != null)
                    break;
                foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
                {
                    if (perm.b + _z > _truckZ)
                        break;
                    for (int _x = maxX - perm.a; _x >= 0; _x--)
                    {
                        if (_x <= bestX)
                            break;
                        for (int _y = 0; _y < _truckY - perm.c; _y++)
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
                    Height = _truckZ,
                    Width = _truckY,
                    Length = _truckX
                }, _solution);
                throw new Exception("Could not place package");
            }
            _solution.Add(best);
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
                    for (int _z = _truckZ - perm.b; _z >= 0; _z--)
                    {
                        if (_x <= bestX)
                            break;
                        //for (int _y = _truckY - perm.c; _y >= 0; _y--)
                        for (int _y = 0; _y < _truckY - perm.c; _y++)
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
                    Height = _truckZ,
                    Width = _truckY,
                    Length = _truckX
                }, _solution);
                throw new Exception("Could not place package");
            }
            _solution.Add(best);
        }
        private bool CanFit(int x, int z, int y, (int a, int b, int c) perm)
        {
            if (x < 0 || z < 0 || y < 0 || x + perm.a > _truckX || z + perm.b > _truckZ || y + perm.c > _truckY)
                return false;
            foreach (var package in _solution)
            {
                if ((package.x5 > x && package.x1 < x + perm.a) && (package.y5 > y && package.y1 < y + perm.c) && (package.z5 > z && package.z1 < z + perm.b))
                    return false;
            }
            return true;
        }
        private IEnumerable<(int a, int b, int c)> GetPermutaions(int[] list)
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
        private void Swap(ref int a, ref int b)
        {
            if (a == b) return;

            var temp = a;
            a = b;
            b = temp;
        }
        private int Max(params int[] ints)
        {
            int ma = -1;
            foreach (var item in ints)
                ma = Math.Max(item, ma);
            return ma;
        }
    }
}