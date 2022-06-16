using System;

namespace AquaCoaster.Utilities
{
    public struct Point
    {
        public Int32 X { get; set; }
        public Int32 Y { get; set; }

        // Constructor
        public Point(Int32 x, Int32 y)
        {
            this.X = x;
            this.Y = y;
        }

        public Point(System.Drawing.Point point) : this(point.X, point.Y) { }

        // Opeartors

        public static bool operator ==(Point left, Point right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        // Methods

        public Point Offset(Int32 x, Int32 y)
        {
            return new Point(this.X + x, this.Y + y);
        }

        public System.Drawing.Point ToPoint()
        {
            return new System.Drawing.Point(X, Y);
        }

        public static Point Parse(String s)
        {
            String[] tokens = s.Split(",");
            return new Point(Int32.Parse(tokens[0]), Int32.Parse(tokens[1]));
        }

        // Overriden methods

        public override Boolean Equals(object obj)
        {
            if (!(obj is Point))
                return false;

            Point p = (Point)obj;
            return p.X == this.X && p.Y == this.Y;
        }

        public override Int32 GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override String ToString()
        {
            return X + "," + Y;
        }
    }
}
