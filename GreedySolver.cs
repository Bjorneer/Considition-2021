using System;
using System.Collections.Generic;
using System.Linq;
using DotNet.models;


namespace DotNet
{
    public class  GreedySolver
    {
        private List<Package> _normalPackages;
        private List<Package> _heavyPackages;
        private List<Package> _placedPackages = new();
        
        private readonly List<PointPackage> _solution = new();
        private readonly int _truckX;
        private readonly int _truckY;
        private readonly int _truckZ;
        private int _xp, _yp, _zp;
        private int _lastKnownMaxWidth;
        private int _lastKnownMaxLength;

        public GreedySolver(List<Package> packages, Vehicle vehicle)
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

            _normalPackages = packages.FindAll(p => p.WeightClass != 2);
            _heavyPackages = packages.FindAll(p => p.WeightClass == 2);

            _truckX = vehicle.Length;
            _truckY = vehicle.Width;
            _truckZ = vehicle.Height;
        }

        public List<PointPackage> Solve()
        {
            SortByMaxArea();
            
            while (_placedPackages.Count < _normalPackages.Count + _heavyPackages.Count)
            {
                Package package;
                var nextHeavy = GetNextHeavyPackage();
                var nextNormal = GetNextNormalPackage();

                if (nextHeavy != null & nextNormal != null)
                {
                    if ((!DoesPackageFitZ(nextHeavy) & !DoesPackageFitZ(nextNormal)) |
                        _zp <= _heavyPackages.Select(p => p.Height).Max())
                    {
                        package = nextHeavy;
                    }
                    else
                    {
                        package = nextNormal;
                    }
                }
                else
                {
                    package = nextHeavy ?? nextNormal;
                }
                
                if (DoesPackageFitZ(package))
                {
                    AddPackage(package);
                    _zp += package.Height;
                }
                else if (DoesPackageFitY(package))
                {
                    _yp += _lastKnownMaxWidth;
                    _zp = 0;
                    AddPackage(package);
                    _zp = package.Height;
                    _lastKnownMaxWidth = 0;
                }
                else if (DoesPackageFitX(package))
                {
                    _xp += _lastKnownMaxLength;
                    _yp = 0;
                    _zp = 0;
                    AddPackage(package);
                    _zp = package.Height;
                    _lastKnownMaxLength = 0;
                }
                else
                {
                    Console.WriteLine("SOMETHING WENT TERRIBLY WRONG!! ABORT MISSION");
                    break;
                }
                
                SetMaxX(package);
                SetMaxY(package);
            }

            return _solution;
        }


        private void SetMaxX(Package package)
        {
            if (package.Length > _lastKnownMaxLength)
            {
                _lastKnownMaxLength = package.Length;
            }
        }
        
        private void SetMaxY(Package package)
        {
            if (package.Width > _lastKnownMaxWidth)
            {
                _lastKnownMaxWidth = package.Width;
            }
        }

        private bool DoesPackageFitX(Package package)
        {
            return (_xp + _lastKnownMaxLength + package.Length < _truckX);
        }
        
        private bool DoesPackageFitY(Package package)
        {
            return (_yp + _lastKnownMaxWidth + package.Width < _truckY &
                    _xp + package.Length < _truckX);
        }
        
        private bool DoesPackageFitZ(Package package)
        {
            return (_xp + package.Length < _truckX &
                    _yp + package.Width < _truckY &
                    _zp + package.Height < _truckZ);
        }

        private void SortByMaxArea()
        {
            _heavyPackages = _heavyPackages.OrderByDescending(x => x.Width * x.Length).ToList();
            _normalPackages = _normalPackages.OrderByDescending(x => x.Width * x.Length).ToList();
        }
        
        private Package GetNextHeavyPackage()
        {
            return _heavyPackages.Find(p => !_placedPackages.Contains(p));
        }

        private Package GetNextNormalPackage()
        {
            return _normalPackages.Find(p => !_placedPackages.Contains(p));
        }

        private void AddPackage(Package package)
        {
            var placedPackage = new PointPackage()
            {
                Id = package.Id,
                x1 = _xp,
                x2 = _xp,
                x3 = _xp,
                x4 = _xp,
                x5 = _xp + package.Length,
                x6 = _xp + package.Length,
                x7 = _xp + package.Length,
                x8 = _xp + package.Length,
                y1 = _yp,
                y2 = _yp,
                y3 = _yp,
                y4 = _yp,
                y5 = _yp + package.Width,
                y6 = _yp + package.Width,
                y7 = _yp + package.Width,
                y8 = _yp + package.Width,
                z1 = _zp,
                z2 = _zp,
                z3 = _zp,
                z4 = _zp,
                z5 = _zp + package.Height,
                z6 = _zp + package.Height,
                z7 = _zp + package.Height,
                z8 = _zp + package.Height,
                OrderClass = package.OrderClass,
                WeightClass = package.WeightClass
            };
            
            

            _solution.Add(placedPackage);
            _placedPackages.Add(package);
        }
    }
}