using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    public class Point
    {
        public float x, y, z;

        public Point()
        {
            x = 0;
            y = 0;
            z = 0;
        }
        public Point(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public Point(Point p)
        {
            if (p == null)
                return;
            x = p.x;
            y = p.y;
            z = p.z;
        }

        public override string ToString()
        {
            return String.Format("X:{0:f1} Y:{1:f1} Z:{2:f1}", x, y, z);
        }

        public float length()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);

        }

        public static float scalar(Point p1, Point p2)
        {
            return p1.x * p2.x + p1.y * p2.y + p1.z * p2.z;
        }

        public static Point norm(Point p)
        {
            float z = (float)Math.Sqrt((float)(p.x * p.x + p.y * p.y + p.z * p.z));
            if (z == 0)
                return new Point(p);
            return new Point(p.x / z, p.y / z, p.z / z);
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);

        }

        public static Point operator *(Point p1, Point p2)
        {
            return new Point(p1.y * p2.z - p1.z * p2.y, p1.z * p2.x - p1.x * p2.z, p1.x * p2.y - p1.y * p2.x);
        }

        public static Point operator *(float t, Point p1)
        {
            return new Point(p1.x * t, p1.y * t, p1.z * t);
        }


        public static Point operator *(Point p1, float t)
        {
            return new Point(p1.x * t, p1.y * t, p1.z * t);
        }

        public static Point operator -(Point p1, float t)
        {
            return new Point(p1.x - t, p1.y - t, p1.z - t);
        }

        public static Point operator -(float t, Point p1)
        {
            return new Point(t - p1.x, t - p1.y, t - p1.z);
        }

        public static Point operator +(Point p1, float t)
        {
            return new Point(p1.x + t, p1.y + t, p1.z + t);
        }

        public static Point operator +(float t, Point p1)
        {
            return new Point(p1.x + t, p1.y + t, p1.z + t);
        }

        public static Point operator /(Point p1, float t)
        {
            return new Point(p1.x / t, p1.y / t, p1.z / t);
        }

        public static Point operator /(float t, Point p1)
        {
            return new Point(t / p1.x, t / p1.y, t / p1.z);
        }
    }
}
