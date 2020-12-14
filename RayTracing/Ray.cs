using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    public class Ray
    {
        public Point start, direction;

        public Ray(Point st, Point end)
        {
            start = new Point(st);
            direction = Point.norm(end - st);
        }

        public Ray() { }

        public Ray(Ray r)
        {
            start = r.start;
            direction = r.direction;
        }

        public Ray reflect(Point hit_point, Point normal)
        {
            Point reflect_dir = direction - 2 * normal * Point.scalar(direction, normal);
            return new Ray(hit_point, hit_point + reflect_dir);
        }

        public Ray refract(Point hit_point, Point normal, float eta)
        {
            Ray res_ray = new Ray();
            float sclr = Point.scalar(normal, direction);

            float k = 1 - eta * eta * (1 - sclr * sclr);

            if (k >= 0)
            {
                float cos_theta = (float)Math.Sqrt(k);
                res_ray.start = new Point(hit_point);
                res_ray.direction = Point.norm(eta * direction - (cos_theta + eta * sclr) * normal);
                return res_ray;
            }
            else
                return null;
        }
    }
}
