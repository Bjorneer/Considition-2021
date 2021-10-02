using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet
{
    public class ConstructiveSolver
    {
        private List<Package> _packages;
        private List<Package> _placedPackages = new();

        private readonly List<PointPackage> _solution = new();
        private bool[,,] _used;
        private readonly int _length;
        private readonly int _width;
        private readonly int _height;

        public ConstructiveSolver(List<Package> packages, Vehicle vehicle)
        {
            
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
            
            _packages = packages;

            _used = new bool[vehicle.Length, vehicle.Length, vehicle.Height];
            _length = vehicle.Length;
            _width = vehicle.Width;
            _height = vehicle.Height;
        }

        public List<PointPackage> Solve()
        {
            List<(int x1, int x2, int y1, int y2, int z1, int z2)> availableBoxes = new List<(int x1, int x2, int y1, int y2, int z1, int z2)>();
            availableBoxes.Add(new (0, _length, 0, _width, 0, _height));

            while (_solution.Count != _packages.Count)
            {
                int boxIndex = GetIndexForMinDistance(availableBoxes.Select(item => Distance(item)).Select(item => Sort(new (item.a, item.b, item.c))).ToList());
                var bestBox = availableBoxes.Select(item => Distance(item)).ToList()[boxIndex];
                (int x1, int x2, int y1, int y2, int z1, int z2) boxToFill = availableBoxes[boxIndex];
                availableBoxes.RemoveAt(boxIndex);
                PointPackage bestPlacedPackage = null;
                int bestVolume = 0;
                foreach (var package in _packages)
                {
                    int height = package.Height;
                    int length = package.Length;
                    int width = package.Width;
                    if (width * height * length > bestVolume && height <= bestBox.a && length <= bestBox.b && width <= bestBox.c) 
                        // Instead of max volume it might be good to go by best fit if 1 or 2 directions fit perfectly
                    {
                        bestPlacedPackage = GetPointPackage(bestBox.corner, package, boxToFill);
                        bestVolume = width * height * length;
                    }
                }
                if (bestPlacedPackage == null)
                    throw new Exception("Unable to place all packages. Increase length");
                _solution.Add(bestPlacedPackage);
                
                SplitBox(bestPlacedPackage, boxToFill);
            }
            LowerFloatingPackages();
            return _solution;
        }

        private IEnumerable<(int x1, int x2, int y1, int y2, int z1, int z2)> SplitBox(PointPackage bestPlacedPackage, (int x1, int x2, int y1, int y2, int z1, int z2) boxToFill)
        {
            return null;
        }

        private void LowerFloatingPackages()
        {
            throw new NotImplementedException();
        }

        private PointPackage GetPointPackage(int corner, Package package, (int x1, int x2, int y1, int y2, int z1, int z2) boxToFill)
        {
            (int x1, int x2, int y1, int y2, int z1, int z2) pointPackage = new (0,0,0,0,0,0);
            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }, 0))
            {

                if (perm.a <= boxToFill.x2 - boxToFill.x1 && perm.b <= boxToFill.y2 - boxToFill.y1 && perm.c <= boxToFill.z2 - boxToFill.z1) 
                    // Instead of max volume it might be good to go by best fit if 1 or 2 directions fit perfectly
                {
                    switch (corner)
                    {
                        case 0:
                            pointPackage.x1 = boxToFill.x1;
                            pointPackage.x2 = perm.a + boxToFill.x2;
                            pointPackage.y1 = boxToFill.y1;
                            pointPackage.y2 = perm.b + boxToFill.y2;
                            pointPackage.z1 = boxToFill.z1;
                            pointPackage.z2 = perm.c + boxToFill.z2;
                            break;
                        case 1:
                            pointPackage.x1 = boxToFill.x1;
                            pointPackage.x2 = perm.a + boxToFill.x2;
                            pointPackage.y1 = boxToFill.y1;
                            pointPackage.y2 = perm.b + boxToFill.y2;
                            pointPackage.z1 = boxToFill.z2 - perm.c;
                            pointPackage.z2 = boxToFill.z1;
                            break;
                        case 2:
                            pointPackage.x1 = boxToFill.x1;
                            pointPackage.x2 = perm.a + boxToFill.x2;
                            pointPackage.y1 = boxToFill.y2 - perm.b;
                            pointPackage.y2 = boxToFill.y2;
                            pointPackage.z1 = boxToFill.z1;
                            pointPackage.z2 = perm.c + boxToFill.z2;
                            break;
                        case 3:
                            pointPackage.x1 = boxToFill.x1;
                            pointPackage.x2 = perm.a + boxToFill.x2;
                            pointPackage.y1 = boxToFill.y2 - perm.b;
                            pointPackage.y2 = boxToFill.y2;
                            pointPackage.z1 = boxToFill.z2 - perm.c;
                            pointPackage.z2 = boxToFill.z1;
                            break;
                        case 4:
                            pointPackage.x1 = boxToFill.x2 - perm.a;
                            pointPackage.x2 = boxToFill.x2;
                            pointPackage.y1 = boxToFill.y1;
                            pointPackage.y2 = perm.b + boxToFill.y2;
                            pointPackage.z1 = boxToFill.z1;
                            pointPackage.z2 = perm.c + boxToFill.z2;
                            break;
                        case 5:
                            pointPackage.x1 = boxToFill.x2 - perm.a;
                            pointPackage.x2 = boxToFill.x2;
                            pointPackage.y1 = boxToFill.y1;
                            pointPackage.y2 = perm.b + boxToFill.y2;
                            pointPackage.z1 = boxToFill.z2 - perm.c;
                            pointPackage.z2 = boxToFill.z1;
                            break;
                        case 6:
                            pointPackage.x1 = boxToFill.x2 - perm.a;
                            pointPackage.x2 = boxToFill.x2;
                            pointPackage.y1 = boxToFill.y2 - perm.b;
                            pointPackage.y2 = boxToFill.y2;
                            pointPackage.z1 = boxToFill.z1;
                            pointPackage.z2 = perm.c + boxToFill.z2;
                            break;
                        case 7:
                            pointPackage.x1 = boxToFill.x2 - perm.a;
                            pointPackage.x2 = boxToFill.x2;
                            pointPackage.y1 = boxToFill.y2 - perm.b;
                            pointPackage.y2 = boxToFill.y2;
                            pointPackage.z1 = boxToFill.z2 - perm.c;
                            pointPackage.z2 = boxToFill.z1;
                            break;
                    }
                }
            }
            return new PointPackage
            {
                Id = package.Id,
                x1 = pointPackage.x1,
                x2 = pointPackage.x1,
                x3 = pointPackage.x1,
                x4 = pointPackage.x1,
                x5 = pointPackage.x2,
                x6 = pointPackage.x2,
                x7 = pointPackage.x2,
                x8 = pointPackage.x2,
                y1 = pointPackage.y1,
                y2 = pointPackage.y1,
                y3 = pointPackage.y1,
                y4 = pointPackage.y1,
                y5 = pointPackage.y2,
                y6 = pointPackage.y2,
                y7 = pointPackage.y2,
                y8 = pointPackage.y2,
                z1 = pointPackage.z1,
                z2 = pointPackage.z1,
                z3 = pointPackage.z1,
                z4 = pointPackage.z1,
                z5 = pointPackage.z2,
                z6 = pointPackage.z2,
                z7 = pointPackage.z2,
                z8 = pointPackage.z2,
                OrderClass = package.OrderClass,
                WeightClass = package.WeightClass
            };
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

        private (int a, int b, int c, int corner) Distance((int x1, int x2, int y1, int y2, int z1, int z2) box)
        {
            var distances = new List<(int a, int b, int c, int corner)>
            {
                new (box.x1, box.y1, box.z1, 0),
                new (box.x1, box.y1, _height - box.z2, 1),
                new (box.x1, _width - box.y2, box.z1, 2),
                new (box.x1, _width - box.y2, _height - box.z2, 3),
                new (_length - box.x2, box.y1, box.z1, 4),
                new (_length - box.x2, box.y1, _height - box.z2, 5),
                new (_length - box.x2, _width - box.y2, box.z1, 6),
                new (_length - box.x2, _width - box.y2, _height - box.z2, 7),
            };
            return distances[GetIndexForMinDistance(distances.Select(item => Sort(new(item.a, item.b, item.c))).ToList())];
        }

        private int GetIndexForMinDistance(List<(int a, int b, int c)> distances)
        {
            int minIdx = 0;
            for (int i = 1; i < distances.Count(); i++)
            {
                var item = distances[i];
                var minItem = distances[minIdx];
                if (item.a < minItem.a || (item.a == minItem.a && item.b < minItem.b) || (item.a == minItem.a && item.b == minItem.b && item.c < minItem.c))
                    minIdx = i;
            }
            return minIdx;
        }

        private (int a, int b, int c) Sort((int a, int b, int c) distance)
        {
            if (distance.a <= distance.b && distance.a <= distance.c)
                return (distance.a, Math.Min(distance.c, distance.b), Math.Max(distance.c, distance.b));
            else if (distance.b <= distance.c && distance.b <= distance.c)
                return (distance.b, Math.Min(distance.a, distance.c), Math.Max(distance.a, distance.c));
            else
                return (distance.c, Math.Min(distance.a, distance.b), Math.Max(distance.a, distance.b));
        }
    }
}
