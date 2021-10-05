using DotNet.models;
using DotNet.Visualisation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet
{
    public class BfsLocatorSolver
    {
        #region Parameters
        private const int MAX_X = 127;
        #endregion  


        private List<Package> _packages;
        private List<Package> _placedPackages = new();

        private readonly List<PointPackage> _solution = new();
        private bool[,,] _used;
        private bool[,,] _visited;
        private readonly int _truckX;
        private readonly int _truckY;
        private readonly int _truckZ;

        public BfsLocatorSolver(List<Package> packages, Vehicle vehicle)
        {
            _packages = packages;
            _visited = new bool[vehicle.Length, vehicle.Height, vehicle.Length];
            _used = new bool[vehicle.Length, vehicle.Height, vehicle.Length];
            _truckX = vehicle.Length;
            _truckY = vehicle.Width;
            _truckZ = vehicle.Height;
        }

        private readonly int[] placeOrder = new int[] { 2, 4, 6, 5, 3, 1 };

        public List<PointPackage> Solve()
        {
            Queue<(int x, int y, int z)> bfsQueue = new Queue<(int x, int y, int z)>();
            bfsQueue.Enqueue(new(0, 0, 0));
            bfsQueue.Enqueue(new(0, _truckY - 1, 0));
            _visited[0, 0, 0] = true;
            _visited[0, _truckY - 1, 0] = true;

            while(bfsQueue.Count() != 0)
            {
                var location = bfsQueue.Dequeue();

                if (location.x != MAX_X - 1 && _visited[location.x + 1, location.y, location.z])
                {
                    bfsQueue.Enqueue(new(location.x + 1, location.y, location.z));
                    _visited[location.x + 1, location.y, location.z] = true;
                    continue;
                }
                if (location.y != _truckY - 1 && _visited[location.x, location.y + 1, location.z])
                {
                    bfsQueue.Enqueue(new(location.x, location.y + 1, location.z));
                    _visited[location.x, location.y + 1, location.z] = true;
                    continue;
                }
                if (location.z != _truckZ - 1 && _visited[location.x, location.y, location.z + 1])
                {
                    bfsQueue.Enqueue(new(location.x, location.y, location.z + 1));
                    _visited[location.x, location.y, location.z + 1] = true;
                    continue;
                }
                /*
                if (CanFitFromY0())
                {

                }
                else if (CanFitFromYMax())
                {

                }
                */
            }

            return _solution;
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
                for (int _x = 0; _x < _truckX - perm.a; _x++)
                {
                    if (bestX <= _x + perm.a)
                        break;
                    for (int _z = 0; _z < _truckZ - perm.b; _z++)
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
            _solution.Add(best);
            for (int i = best.x1; i <= best.x5 - 1; i++)
            {
                for (int j = best.z1; j <= best.z5 - 1; j++)
                {
                    for (int k = best.y1; k <= best.y5 - 1; k++)
                    {
                        _used[i, j, k] = true;
                    }
                }
            }
        }
        private void PackHeavy(Package package)
        {
            int bestX = int.MaxValue;
            PointPackage best = null;

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
            {
                for (int _z = 0; _z < _truckZ - perm.b; _z++)
                {
                    if (best != null)
                        break;
                    for (int _x = 0; _x < MAX_X - perm.a; _x++)
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
            _solution.Add(best);
            for (int i = best.x1; i <= best.x5 - 1; i++)
            {
                for (int j = best.z1; j <= best.z5 - 1; j++)
                {
                    for (int k = best.y1; k <= best.y5 - 1; k++)
                    {
                        _used[i, j, k] = true;
                    }
                }
            }
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
                    for (int _z = 0; _z < _truckZ - perm.b; _z++)
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
            for (int i = best.x1; i <= best.x5 - 1; i++)
            {
                for (int j = best.z1; j <= best.z5 - 1; j++)
                {
                    for (int k = best.y1; k <= best.y5 - 1; k++)
                    {
                        _used[i, j, k] = true;
                    }
                }
            }
        }
        private void PackHeavyReverse(Package package)
        {
            int bestX = int.MinValue;
            PointPackage best = null;


            for (int _z = 0; _z < _truckZ; _z++)
            {
                if (best != null)
                    break;
                foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }))
                {
                    if (perm.b + _z >= _truckZ)
                        break;
                    for (int _x = MAX_X - perm.a - 1; _x >= 0; _x--)
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
            for (int i = best.x1; i <= best.x5 - 1; i++)
            {
                for (int j = best.z1; j <= best.z5 - 1; j++)
                {
                    for (int k = best.y1; k <= best.y5 - 1; k++)
                    {
                        _used[i, j, k] = true;
                    }
                }
            }
        }

        private bool CanFit(int x, int z, int y, (int a, int b, int c) perm)
        {
            for (int i = 0; i < perm.a; i++)
            {
                for (int j = 0; j < perm.b; j++)
                {
                    for (int k = 0; k < perm.c; k++)
                    {
                        if (_used[x + i, z + j, k + y])
                            return false;
                    }
                }
            }
            return true;
        }

        private IEnumerable<(int a, int b, int c)> GetPermutaions(int[] list)
        {
            yield return new(list[0], list[1], list[2]);
            yield return new(list[0], list[2], list[1]);
            yield return new(list[1], list[0], list[2]);
            yield return new(list[1], list[2], list[0]);
            yield return new(list[2], list[1], list[0]);
            yield return new(list[2], list[0], list[1]);
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