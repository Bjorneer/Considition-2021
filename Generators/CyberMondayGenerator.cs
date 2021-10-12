using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Generators
{
    public class CyberMondayGenerator : Generator
    {
        protected override string Map => "cyber";

        private int GetRandomPackageSize(int mean, int std)
        {
            var u = Program.Random.NextDouble();
            var v = Program.Random.NextDouble();
            return (int)Math.Max(Math.Min(Math.Round(Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v) * std + mean), 70), 5);
        }

        protected override void ReGenerate()
        {
            int length = 250;
            int height = 250;
            int widht = 200;
            double[] weightProbabilities = new double[] { 0.4, 0.5, 1 };
            double[] orderProbabilities = new double[] { 0.2, 0.4, 0.6, 0.8, 1 };
            double aprxPackVolumeOfTotal = 0.4;

            int currentVolume = 0;
            int idCnt = 0;
            while(currentVolume < length * height * widht * aprxPackVolumeOfTotal)
            {
                double weightSel = Program.Random.NextDouble();
                double orderSel = Program.Random.NextDouble();
                Packages.Add(new models.Package
                {
                    Id = idCnt++,
                    WeightClass = weightProbabilities.Select((v, i) => new { dist = v, index = i }).First(item => item.dist >= weightSel).index,
                    OrderClass = orderProbabilities.Select((v, i) => new { dist = v, index = i }).First(item => item.dist >= orderSel).index,
                    Height = GetRandomPackageSize(50, 20),
                    Length = GetRandomPackageSize(40, 20),
                    Width  = GetRandomPackageSize(30, 20),
                });
                currentVolume += Packages.Last().Width * Packages.Last().Length * Packages.Last().Height;
            }
            Vehicle = new models.Vehicle
            {
                Length = length,
                Height = height,
                Width = widht
            };
        }
    }
}
