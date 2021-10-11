using DotNet.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Generators
{
    public abstract class Generator
    {
        protected Vehicle Vehicle;
        protected List<Package> Packages = new List<Package>();
        protected abstract string Map { get; }
        protected abstract void ReGenerate();
        public virtual SubmitResponse Submit(List<PointPackage> solution)
        { return null; }
        public (Vehicle vehicle, List<Package> packages) ReadOrGenerateMap()
        {
            string path = $"C:\\src\\Considition-2021\\Generators\\SavedMaps\\{Map}.txt"; // SavedMaps folder is excluded from git but will be generated
            if (File.Exists(path))
            {
                string mapContent = File.ReadAllText(path);
                var splitContent = mapContent.Split('\n');
                var vehicleSizeStr = splitContent.First().Split();
                Vehicle = new Vehicle
                {
                    Length = int.Parse(vehicleSizeStr[0]),
                    Width = int.Parse(vehicleSizeStr[1]),
                    Height = int.Parse(vehicleSizeStr[2])
                };
                int idCnt = 0;
                foreach (var packageStr in splitContent.Skip(1))
                {
                    if (string.IsNullOrEmpty(packageStr))
                        continue;
                    var packageSizeStr = packageStr.Split();
                    Packages.Add(new Package
                    {
                        Id = idCnt++,
                        Length = int.Parse(packageSizeStr[0]),
                        Width = int.Parse(packageSizeStr[1]),
                        Height = int.Parse(packageSizeStr[2]),
                        OrderClass = int.Parse(packageSizeStr[3]),
                        WeightClass = int.Parse(packageSizeStr[4])
                    });
                }
            }
            else
            {
                ReGenerate();
                using (StreamWriter writer = new StreamWriter(path, false))
                {
                    writer.WriteLine($"{Vehicle.Length} {Vehicle.Width} {Vehicle.Height}");
                    foreach (var package in Packages)
                    {
                        writer.WriteLine($"{package.Length} {package.Width} {package.Height} {package.OrderClass} {package.WeightClass}");
                    }
                    writer.Close();
                }
            }
            return (Vehicle, Packages);
        }
    }
}
