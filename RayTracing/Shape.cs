using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{

    public class Shape
    {
        public static float EPS = 0.0001f;
        public List<Point> points = new List<Point>(); // точки 
        public List<ShapeSide> sides = new List<ShapeSide>();        // стороны
        public Material figure_material;
        public Shape() { }

        //-------------------------------------------------------------------------------------------------------
        // redo for new members
        public Shape(Shape f)
        {
            foreach (Point p in f.points)
                points.Add(new Point(p));

            foreach (ShapeSide s in f.sides)
            {
                sides.Add(new ShapeSide(s));
                sides.Last().host = this;
            }
        }

        //-------------------------------------------------------------------------------------------------------
        public bool ray_intersects_triangle(Ray r, Point p0, Point p1, Point p2, out float intersect)
        {
            intersect = -1;

            Point edge1 = p1 - p0;
            Point edge2 = p2 - p0;
            Point h = r.direction * edge2;
            float a = Point.scalar(edge1, h);

            if (a > -EPS && a < EPS)
                return false;       // This ray is parallel to this triangle.

            float f = 1.0f / a;
            Point s = r.start - p0;
            float u = f * Point.scalar(s, h);

            if (u < 0 || u > 1)
                return false;

            Point q = s * edge1;
            float v = f * Point.scalar(r.direction, q);

            if (v < 0 || u + v > 1)
                return false;
            // At this stage we can compute t to find out where the intersection point is on the line.
            float t = f * Point.scalar(edge2, q);
            if (t > EPS)
            {
                intersect = t;
                return true;
            }
            else      // This means that there is a line intersection but not a ray intersection.
                return false;
        }

        //-------------------------------------------------------------------------------------------------------
        // пересечение луча с фигурой
        public virtual bool figure_intersection(Ray r, out float intersect, out Point normal)
        {
            intersect = 0;
            normal = null;
            ShapeSide sd = null;


            foreach (ShapeSide s in sides)
            {
                if (s.points.Count == 3)
                {
                    float t;
                    if (ray_intersects_triangle(r, s.get_point(0), s.get_point(1), s.get_point(2), out t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        sd = s;
                    }
                }
                else if (s.points.Count == 4)
                {
                    float t;
                    if (ray_intersects_triangle(r, s.get_point(0), s.get_point(1), s.get_point(3), out t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        sd = s;
                    }
                    else if (ray_intersects_triangle(r, s.get_point(1), s.get_point(2), s.get_point(3), out t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        sd = s;
                    }
                }
            }

            if (intersect != 0)
            {
                normal = ShapeSide.norm(sd);
                figure_material.clr = new Point(sd.drawing_pen.Color.R / 255f, sd.drawing_pen.Color.G / 255f, sd.drawing_pen.Color.B / 255f);
                return true;
            }

            return false;
        }



        
        /// ------------------------ГОТОВЫЕ ФИГУРЫ-----------------------------
        

        static public Shape get_cube(float sz)
        {
            Shape res = new Shape();
            res.points.Add(new Point(sz / 2, sz / 2, sz / 2)); // 0 
            res.points.Add(new Point(-sz / 2, sz / 2, sz / 2)); // 1
            res.points.Add(new Point(-sz / 2, sz / 2, -sz / 2)); // 2
            res.points.Add(new Point(sz / 2, sz / 2, -sz / 2)); //3

            res.points.Add(new Point(sz / 2, -sz / 2, sz / 2)); // 4
            res.points.Add(new Point(-sz / 2, -sz / 2, sz / 2)); //5
            res.points.Add(new Point(-sz / 2, -sz / 2, -sz / 2)); // 6
            res.points.Add(new Point(sz / 2, -sz / 2, -sz / 2)); // 7

            ShapeSide s = new ShapeSide(res);
            s.points.AddRange(new int[] { 3, 2, 1, 0 });
            res.sides.Add(s);

            s = new ShapeSide(res);
            s.points.AddRange(new int[] { 4, 5, 6, 7 });
            res.sides.Add(s);

            s = new ShapeSide(res);
            s.points.AddRange(new int[] { 2, 6, 5, 1 });
            res.sides.Add(s);

            s = new ShapeSide(res);
            s.points.AddRange(new int[] { 0, 4, 7, 3 });
            res.sides.Add(s);

            s = new ShapeSide(res);
            s.points.AddRange(new int[] { 1, 5, 4, 0 });
            res.sides.Add(s);

            s = new ShapeSide(res);
            s.points.AddRange(new int[] { 2, 3, 7, 6 });
            res.sides.Add(s);

            return res;
        }


        ///
        /// ---------------------------------------------------------------------------------------
        ///















        
        //----------------------------- МЕТОДЫ ПРЕОБРАЗОВАНИЙ--------------------------------
       

        public float[,] get_matrix()
        {
            var res = new float[points.Count, 4];
            for (int i = 0; i < points.Count; i++)
            {
                res[i, 0] = points[i].x;
                res[i, 1] = points[i].y;
                res[i, 2] = points[i].z;
                res[i, 3] = 1;
            }
            return res;
        }

        public void apply_matrix(float[,] matrix)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].x = matrix[i, 0] / matrix[i, 3];
                points[i].y = matrix[i, 1] / matrix[i, 3];
                points[i].z = matrix[i, 2] / matrix[i, 3];

            }
        }

        private Point get_center()
        {
            Point res = new Point(0, 0, 0);
            foreach (Point p in points)
            {
                res.x += p.x;
                res.y += p.y;
                res.z += p.z;

            }
            res.x /= points.Count();
            res.y /= points.Count();
            res.z /= points.Count();
            return res;
        }

        
        // ----------------------------- АФФИННЫЕ ПРЕОБРАЗОВАНИЯ --------------------------------

        public void rotate_around_rad(float rangle, string type)
        {
            float[,] mt = get_matrix();
            Point center = get_center();
            switch (type)
            {
               
                case "CZ":
                    mt = apply_offset(mt, -center.x, -center.y, -center.z);
                    mt = apply_rotation_Z(mt, rangle);
                    mt = apply_offset(mt, center.x, center.y, center.z);
                    break;
                default:
                    break;
            }
            apply_matrix(mt);
        }

        public void rotate_around(float angle, string type)
        {
            rotate_around_rad(angle * (float)Math.PI / 180, type);
        }

        public void offset(float xs, float ys, float zs)
        {
            apply_matrix(apply_offset(get_matrix(), xs, ys, zs));
        }

        public void set_pen(Pen dw)
        {
            foreach (ShapeSide s in sides)
                s.drawing_pen = dw;

        }

        ///
        /// ----------------------------- STATIC BACKEND FOR TRANSFROMS --------------------------------
        ///

      
        private static float[,] multiply_matrix(float[,] m1, float[,] m2)
        {
            float[,] res = new float[m1.GetLength(0), m2.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); i++)
            {
                for (int j = 0; j < m2.GetLength(1); j++)
                {
                    for (int k = 0; k < m2.GetLength(0); k++)
                    {
                        res[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }
            return res;
        }

        private static float[,] apply_offset(float[,] transform_matrix, float offset_x, float offset_y, float offset_z)
        {
            float[,] translationMatrix = new float[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { offset_x, offset_y, offset_z, 1 } };
            return multiply_matrix(transform_matrix, translationMatrix);
        }

        private static float[,] apply_rotation_Z(float[,] transform_matrix, float angle)
        {
            float[,] rotationMatrix = new float[,] { { (float)Math.Cos(angle), (float)Math.Sin(angle), 0, 0 }, { -(float)Math.Sin(angle), (float)Math.Cos(angle), 0, 0 },
                { 0, 0, 1, 0 }, { 0, 0, 0, 1} };
            return multiply_matrix(transform_matrix, rotationMatrix);
        }
    }
    
}
