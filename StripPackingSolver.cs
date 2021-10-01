using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNet
{
    public class StripPackingSolver
    {
        /*
         * https://www.win.tue.nl/~nikhil/pubs/3dstrip-28.pdf
         * http://www.optimization-online.org/DB_FILE/2012/04/3429.pdf
         * http://www.inf.u-szeged.hu/bpseminar/EklavyaSharmaPresentation.pdf
         * https://github.com/Mxbonn/strip-packing/blob/47008729ccc7e68305ca81458ec06985d15e3406/spp/ph.py#L133
         * https://www.scielo.br/j/gp/a/KQXpLtqqPKPmgJ4gVqLHt4m/?lang=en&format=pdf
         * https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.418.7210&rep=rep1&type=pdf
         * https://www.scielo.br/j/pope/a/Mp4Pd8CS86hTKMLKnGhQDsp/?lang=en
         */
        private List<Package> _packages;
        private List<Package> _placedPackages = new();

        private readonly List<PointPackage> _solution = new();
        private bool[,,] _used;
        private readonly int _truckX;
        private readonly int _truckY;
        private readonly int _truckZ;

        public StripPackingSolver(List<Package> packages, Vehicle vehicle)
        {
            /*
            packages.ForEach(package =>
            {
                int h = package.Height;
                int l = package.Length;
                int w = package.Width;
                List<int> sz = new List<int> { h, l, w };
                sz.Sort();
                package.Height = sz[0];
                package.Length = sz[1];
                package.Width = sz[2];
            });
            */
            _packages = packages;

            _used = new bool[vehicle.Length, vehicle.Height, vehicle.Length];
            _truckX = vehicle.Length;
            _truckY = vehicle.Width;
            _truckZ = vehicle.Height;
        }

        public List<PointPackage> Solve()
        {
            //var groups = _packages.GroupBy(item => item.WeightClass == 2  ? 10 : item.OrderClass).OrderByDescending(item => item.Key).Select(item => item.AsEnumerable()).ToList();
            var groups = _packages.GroupBy(item => 0).OrderByDescending(item => item.Key).Select(item => item.AsEnumerable()).ToList();
            int grpidx = 0;
            foreach (var group in groups)
            {
                //var packages = group.OrderByDescending(item => item.OrderClass).ThenByDescending(item => Max(item.Width, item.Height, item.Length)).ThenByDescending(item => item.WeightClass);
                var packages = group.OrderByDescending(item => Max(item.Width, item.Height, item.Length)).ThenByDescending(item => item.WeightClass);

                foreach (var package in packages)
                {
                    Pack(package);
                }
                ++grpidx;
            }
            Console.WriteLine(_solution.Select(item => item.x8).Max());
            return _solution;
        }

        private void Pack(Package package)
        {
            /*
            if (package.WeightClass == 2)
            {
                PackHeavy(package);
                return;
            }
            */
            int bestX = int.MaxValue;
            PointPackage best = null;

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }, 0))
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
                throw new Exception("Could not place package");
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

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }, 0))
            {
                for (int _z = 0; _z < _truckZ - perm.b; _z++)
                {
                    if (best != null)
                        break;
                    for (int _x = 0; _x < _truckX - perm.a; _x++)
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
                throw new Exception("Could not place package");
            _solution.Add(best);
            for (int i = best.x1; i <= best.x5; i++)
            {
                for (int j = best.z1; j <= best.z5 - 1; j++)
                {
                    for (int k = best.y1; k <= best.y5; k++)
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

        private IEnumerable<(int a, int b, int c)> GetPermutaions(int[] list, int k)
        {
            if (k == 3)
            {
                yield return (list[0], list[1], list[2]);
            }
            else
                for (int i = k; i <= 2; i++)
                {
                    Swap(ref list[k], ref list[i]);
                    var perms = GetPermutaions(list, k + 1);
                    foreach (var perm in perms)
                        yield return perm;
                    Swap(ref list[k], ref list[i]);
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
