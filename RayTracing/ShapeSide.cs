using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    public class ShapeSide
    {
        public Shape host = null;
        public List<int> points = new List<int>();
        public Pen drawing_pen = new Pen(Color.Black);
        public Point Normal;

        public ShapeSide(Shape h = null)
        {
            host = h;
        }
        public ShapeSide(ShapeSide s)
        {
            points = new List<int>(s.points);
            host = s.host;
            drawing_pen = s.drawing_pen.Clone() as Pen;
            Normal = new Point(s.Normal);
        }
        public Point get_point(int ind)
        {
            if (host != null)
                return host.points[points[ind]];
            return null;
        }

        public static Point norm(ShapeSide S)
        {
            if (S.points.Count() < 3)
                return new Point(0, 0, 0);
            Point U = S.get_point(1) - S.get_point(0);
            Point V = S.get_point(S.points.Count - 1) - S.get_point(0);
            Point normal = U * V;
            return Point.norm(normal);
        }
    }
}
