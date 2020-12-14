using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayTracing
{
    public partial class Form1 : Form
    {
        public List<Shape> scene = new List<Shape>();
        public List<Light> lights = new List<Light>();
        public Color[,] color_pixels;
        public Point[,] pixels;
        public Point focus;
        public Point up_left, up_right, down_left, down_right;
        public int h, w;

        public Form1()
        {
            InitializeComponent();
            focus = new Point();
            up_left = new Point();
            up_right = new Point();
            down_left = new Point();
            down_right = new Point();
            h = pictureBox1.Height;
            w = pictureBox1.Width;
            pictureBox1.Image = new Bitmap(w, h);
        }

        public void build_scene()
        {
            Shape room = Shape.get_cube(10);

            //сторона комнаты
            up_left = room.sides[0].get_point(0);
            up_right = room.sides[0].get_point(1);
            down_right = room.sides[0].get_point(2);
            down_left = room.sides[0].get_point(3);

            Point normal = ShapeSide.norm(room.sides[0]);
            Point center = (up_left + up_right + down_left + down_right) / 4;
            focus = center + normal * 10;

            room.set_pen(new Pen(Color.Gray));
            room.sides[0].drawing_pen = new Pen(Color.Yellow);
            room.sides[3].drawing_pen = new Pen(Color.Red);
            room.sides[2].drawing_pen = new Pen(Color.Green);
            room.sides[1].drawing_pen = new Pen(Color.Blue);
            room.figure_material = new Material(0f, 0, 0.2f, 0.7f);

            Light l1 = new Light(new Point(0f, 2f, 4.9f), new Point(1f, 1f, 1f));
            //Light l2 = new Light(new Point(4.5f, -1f, 4f), new Point(1f, 1f, 1f));
            lights.Add(l1);
            //lights.Add(l2);

            //прозрачная сфера
            ShapeSphere s1 = new ShapeSphere(new Point(-2.5f, 3, -4f), 1f);
            s1.set_pen(new Pen(Color.White));
            s1.figure_material = new Material(0f, 0.9f, 0.1f, 0f, 1.3f);

            //зеркальная сфера
            ShapeSphere s2 = new ShapeSphere(new Point(1.5f, 2, -3.7f), 1.5f);
            s2.set_pen(new Pen(Color.White));
            s2.figure_material = new Material(0.9f, 0f, 0f, 0.1f, 1.5f);

            //куб
            Shape cube = Shape.get_cube(3.0f);
            cube.offset(-2.5f, -1, -3.5f);
            cube.rotate_around(55, "CZ");
            cube.set_pen(new Pen(Color.Purple));
            cube.figure_material = new Material(0f, 0f, 0.3f, 0.7f, 1.5f);
            

            scene.Add(room);
            scene.Add(cube);
            scene.Add(s1);
            scene.Add(s2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            build_scene();
            run_rayTrace();

            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    (pictureBox1.Image as Bitmap).SetPixel(i, j, color_pixels[i, j]);
                }
                pictureBox1.Invalidate();
                progressBar2.PerformStep();
            }

        }

        
        public void run_rayTrace()
        {
            get_pixels();
            for (int i = 0; i < w; ++i)
            {
                for (int j = 0; j < h; ++j)
                {
                    Ray r = new Ray(focus, pixels[i, j]);
                    r.start = new Point(pixels[i, j]);
                    Point clr = RayTrace(r, 10, 1);
                    if (clr.x > 1.0f || clr.y > 1.0f || clr.z > 1.0f)
                        clr = Point.norm(clr);
                    color_pixels[i, j] = Color.FromArgb((int)(255 * clr.x), (int)(255 * clr.y), (int)(255 * clr.z));
                }
                progressBar1.PerformStep();
            }
            
        }

        // получение всех пикселей сцены
        public void get_pixels()
        {
            pixels = new Point[w, h];
            color_pixels = new Color[w, h];
            Point step_up = (up_right - up_left) / (w - 1);
            Point step_down = (down_right - down_left) / (w - 1);

            Point up = new Point(up_left);
            Point down = new Point(down_left);

            for (int i = 0; i < w; ++i)
            {
                Point step_y = (up - down) / (h - 1);
                Point d = new Point(down);
                for (int j = 0; j < h; ++j)
                {
                    pixels[i, j] = d;
                    d += step_y;
                }
                up += step_up;
                down += step_down;
            }
        }

        // видима ли точка пересечения луча с фигурой из источника света
        public bool is_visible(Point light_point, Point hit_point)
        {
            float max_t = (light_point - hit_point).length();     // позиция источника света на луче
            Ray r = new Ray(hit_point, light_point);
            float t;
            Point n;

            foreach (Shape fig in scene)
                if (fig.figure_intersection(r, out t, out n))
                    if (t < max_t && t > Shape.EPS)
                        return false;
             return true;
        }

        public Point RayTrace(Ray r, int iter, float env)
        {
            if (iter <= 0)
                return new Point(0, 0, 0);

            float t = 0;     // позиция точки пересечения луча с фигурой на луче
            Point normal = null;
            Material m = new Material();
            Point res_color = new Point(0, 0, 0);
            bool refract_out_of_figure = false;
            float intersect;
            Point n;

            foreach (Shape fig in scene)
            {
                if (fig.figure_intersection(r, out intersect, out n))
                    if(intersect < t || t == 0)   // нужна ближайшая фигура к точке наблюдения
                    {
                        t = intersect;
                        normal = n;
                        m = new Material(fig.figure_material);
                    }
            }

            if (t == 0)
                return new Point(0, 0, 0);

            if (Point.scalar(r.direction, normal) > 0)
            {
                normal *= -1; 
                refract_out_of_figure = true;
            }

            Point hit_point = r.start + r.direction * t;

            foreach(Light l in lights)
            {
                Point amb = l.color_light * m.ambient;
                amb.x = (amb.x * m.clr.x);
                amb.y = (amb.y * m.clr.y);
                amb.z = (amb.z * m.clr.z);
                res_color += amb;

                // диффузное освещение
                if (is_visible(l.point_light, hit_point))
                    res_color += l.shade(hit_point, normal, m.clr, m.diffuse);
            }

            if(m.reflection > 0)
            {
                Ray reflected_ray = r.reflect(hit_point, normal);
                res_color += m.reflection * RayTrace(reflected_ray, iter - 1, env);
            }

            if(m.refraction > 0)
            {
                float eta = 0;
                if (refract_out_of_figure)
                   eta = m.environment;
                else
                    eta = 1 / m.environment;

                Ray refracted_ray = r.refract(hit_point, normal, eta);
                if(refracted_ray != null)
                    res_color += m.refraction * RayTrace(refracted_ray, iter - 1, m.environment);
            }

            return res_color;
        }
    }
}
