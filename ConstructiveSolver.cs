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
                PointPackage bestPlacedPackage = null;
                int bestVolume = 0;
                foreach (var package in _packages)
                {
                    int height = package.Height;
                    int length = package.Length;
                    int width = package.Width;
                    if (width * height * length > bestVolume && height <= bestBox.a && length <= bestBox.b && width <= bestBox.c)
                    {
                        bestPlacedPackage = GetPointPackage(bestBox.corner, package, boxToFill);
                        bestVolume = width * height * length;
                    }
                }
                if (bestPlacedPackage == null)
                    throw new Exception("Unable to place all packages. Increase length");
                _solution.Add(bestPlacedPackage);

            }
            return _solution;
        }

        private PointPackage GetPointPackage(int corner, Package package, (int x1, int x2, int y1, int y2, int z1, int z2) boxToFill)
        {
            foreach (var perm in GetPermutaions(new int[] { package.Width, package.Height, package.Length }, 0))
            {
            }
            return null;
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
