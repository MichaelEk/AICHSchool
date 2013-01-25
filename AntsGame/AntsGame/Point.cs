using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntsGame
{
    public class Point
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
            Width = int.MaxValue;
            Height = int.MaxValue;
        }

        public Point(int x, int y, int width, int height)
        {
            X = GetMod(x, width);
            Y = GetMod(y, height);
            Width = width;
            Height = height;
        }

        private int GetMod(int x, int modX)
        {
            if(x < 0)
            {
                x += (-x/modX*modX + modX);
            }
            return x%modX;
        }

        public static int Distance2(Point point1, Point point2)
        {
            var height = Math.Min(point1.Height, point2.Height);
            var width = Math.Min(point1.Width, point2.Width);

            var diffX = Math.Abs(point1.X - point2.X);
            var diffY = Math.Abs(point1.Y - point2.Y);
            var wx = Math.Min(diffX, width - diffX);
            var wy = Math.Min(diffY, height - diffY);
            return wx*wx + wy*wy;
        }

        public double Norm2()
        {
            return X*X + Y*Y;
        }

        public static Point operator +(Point point1, Point point2)
        {
            var height = Math.Min(point1.Height, point2.Height);
            var width = Math.Min(point1.Width, point2.Width);

            return new Point((point1.X + point2.X + width) % width,
                             (point1.Y + point2.Y + height)%height,
                             width, height);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point))
                return false;
            var point = (Point) obj;
            return X == point.X && Y == point.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }
    }
}
