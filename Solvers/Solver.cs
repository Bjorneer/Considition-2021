using DotNet.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Solvers
{
    public abstract class Solver
    {
        protected const bool RANDOMIZE = true;
        public abstract List<PointPackage> Solve();
        protected List<Package> Packages;
        protected List<PointPackage> Solution = new();
        protected int TruckX;
        protected int TruckY;
        protected int TruckZ;

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
    }
}
