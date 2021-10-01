using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet
{
    public class Percent95Solver
    {
        private List<Package> _packages;

        private readonly List<PointPackage> _solution = new();
        private readonly int _truckX;
        private readonly int _truckY;
        private readonly int _truckZ;
        private bool[,,] _used;

        public Percent95Solver(List<Package> packages, Vehicle vehicle)
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
            var groups = _packages.GroupBy(item => 0).OrderByDescending(item => item.Key).Select(item => item.AsEnumerable()).ToList();
            int grpidx = 0;

            Queue<(int x, int y, int z)> queue = new Queue<(int x, int y, int z)>();
            for (int _x = 0; _x < 140; _x++)
            {
                for (int _z = 0; _z < _truckZ; _z++)
                {
                    for (int _y = 0; _y < _truckY; _y++)
                    {
                        queue.Enqueue((_x, _y, _z));
                    }
                }
            }

            foreach (var group in groups)
            {
                var packages = group.OrderByDescending(item => item.Length * item.Height * item.Width).ThenByDescending(item => Max(item.Width, item.Height, item.Length));

                foreach (var package in packages)
                {
                    queue = Pack(package, queue);
                }
                ++grpidx;
            }
            Console.WriteLine(_solution.Select(item => item.x8).Max());
            return _solution;
        }

        private Queue<(int x, int y, int z)> Pack(Package package, Queue<(int x, int y, int z)> queue)
        {
            int bestArea = 0;
            PointPackage best = null;
            bool finished = false;

            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }, 0))
            {
                if (finished == true)
                    break;
                foreach (var value in queue)
                {
                    if (finished)
                        break;
                    var first = value;
                    int _x = first.x;
                    int _y = first.y;
                    int _z = first.z;
                    if (CanFit(_x, _z, _y, perm, out int area))
                    {
                        if (area > bestArea)
                        {
                            bestArea = area;
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
                            if (area >= (package.Width * package.Height + package.Height * package.Length + package.Width * package.Length))
                                finished = true;
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
            Queue<(int x, int y, int z)> newQueue = new Queue<(int x, int y, int z)>();
            while(queue.Count() > 0)
            {
                var first = queue.Dequeue();
                if (!_used[first.x, first.z, first.y])
                    newQueue.Enqueue(first);
            }
            return newQueue;
        }

        private bool CanFit(int x, int z, int y, (int a, int b, int c) perm, out int area)
        {
            area = 0;
            if (x + perm.a >= _truckX || z + perm.b >= _truckZ || y + perm.c >= _truckY)
                return false;
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
            bool floating = true;
            for (int i = 0; i < perm.a; i++)
            {
                for (int j = 0; j < perm.b; j++)
                {
                    if (y == 0 || _used[i + x, j + z, y - 1])
                        area++;
                    if (y + perm.c == _truckY - 1 || _used[i + x, j + z, y + perm.c + 1])
                        area++;
                }
            }

            for (int i = 0; i < perm.c; i++)
            {
                for (int j = 0; j < perm.b; j++)
                {
                    if (x == 0 || _used[x - 1, j + z, y + i])
                        area++;
                    if (x + perm.a == _truckX - 1 || _used[x + perm.a + 1, j + z, y + j])
                        area++;
                }
            }

            for (int i = 0; i < perm.a; i++)
            {
                for (int j = 0; j < perm.c; j++)
                {
                    if (z == 0 || _used[i + x, z - 1, y + j])
                    {
                        area++;
                        floating = false;
                    }
                    if (z + perm.b == _truckZ - 1 || _used[i + x, z + perm.b + 1, y + j])
                        area++;
                }
            }
            return !floating;
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