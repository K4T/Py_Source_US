using System;
using System.Runtime.InteropServices;

namespace Py_Game.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Point3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Point3D operator -(Point3D PosA, Point3D PosB)
        {
            Point3D result = new Point3D()
            {
                X = PosA.X - PosB.X,
                Y = PosA.Y - PosB.Y,
                Z = PosA.Z - PosB.Z,
            };
            return result;
        }
        public static Point3D operator +(Point3D PosA, Point3D PosB)
        {
            Point3D result = new Point3D()
            {
                X = PosA.X + PosB.X,
                Y = PosA.Y + PosB.Y,
                Z = PosA.Z + PosB.Z,
            };
            return result;
        }

        public float Distance(Point3D PlayerPos)
        {
            return (this - PlayerPos).Length();
        }

        public float Length()
        {
            return Convert.ToSingle(Math.Sqrt(X * X + Y * Y));
        }

        public float HoleDistance(Point3D PosB)
        {
            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            //::  This function converts decimal degrees to radians             :::
            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            double Deg2rad(double deg)
            {
                return deg * Math.PI / 180.0;
            }

            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            //::  This function converts radians to decimal degrees             :::
            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            double Rad2deg(double rad)
            {
                return rad / Math.PI * 180.0;
            }

            if ((X == PosB.X) && (Z == PosB.Z))
            {
                return 0;
            }
            else
            {
                double theta = X - PosB.X;
                double dist = Math.Sin(Deg2rad(X)) * Math.Sin(Deg2rad(PosB.Z)) + Math.Cos(Rad2deg(X)) * Math.Cos(Deg2rad(PosB.Z)) * Math.Cos(Deg2rad(theta));
                dist = Math.Acos(dist);
                dist = Rad2deg(dist);
                dist = dist * 60 * 1.1515;
                //if (unit == 'K')
                //{
                //    dist *= 1.609344;
                //}
                //else if (unit == 'N')
                //{
                //    dist = dist * 0.8684;
                //}
                return Convert.ToSingle(dist);
            }
        }
    }
}
