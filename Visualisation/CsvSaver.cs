using DotNet.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNet.Visualisation
{
    public class CsvSaver
    {
        public static void Save(Vehicle vehicle, List<PointPackage> packages)
        {
            using (StreamWriter writer = new StreamWriter("C:\\src\\Considition-2021\\Visualisation\\visualization.txt", false))
            {
                writer.WriteLine("length,width,height,x,y,z");
                writer.WriteLine($"{vehicle.Length},{vehicle.Width},{vehicle.Height},0,0,0");
                writer.WriteLine($"{packages.Max(item => item.x5)},{packages.Max(item => item.y5)},{packages.Max(item => item.z5)},0,0,0");
                foreach (var package in packages)
                {
                    writer.WriteLine($"{package.x5 - package.x1},{package.y5 - package.y1},{package.z5 - package.z1},{package.x1},{package.y1},{package.z1}");
                }
                writer.Close();
            }
        }
    }
}
