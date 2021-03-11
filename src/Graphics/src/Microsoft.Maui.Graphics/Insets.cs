using System;
using System.Globalization;

namespace Microsoft.Maui.Graphics
{
    public class Insets
    {
        private double _bottom;
        private double _left;
        private double _right;
        private double _top;

        public Insets(double top, double left, double bottom, double right)
        {
            _top = top;
            _left = left;
            _bottom = bottom;
            _right = right;
        }

        public Insets(Insets insets) : this(insets.Top, insets.Left, insets.Bottom, insets.Right)
        {
        }

        public double Top
        {
            get => _top;
            set => _top = value;
        }

        public double Left
        {
            get => _left;
            set => _left = value;
        }

        public double Bottom
        {
            get => _bottom;
            set => _bottom = value;
        }

        public double Right
        {
            get => _right;
            set => _right = value;
        }

        /// <summary>
        /// The sum of the left and right insets.
        /// </summary>
        public double Horizontal => _left + _right;

        /// <summary>
        /// The sum of the top and bottom insets.
        /// </summary>
        public double Vertical => _top + _bottom;

        public bool AllValuesAreEqualTo(double value)
        {
            return Math.Abs(_top - value) < Geometry.Epsilon && Math.Abs(_left - value) < Geometry.Epsilon && Math.Abs(_right - value) < Geometry.Epsilon &&
                   Math.Abs(_bottom - value) < Geometry.Epsilon;
        }

        public override bool Equals(object obj)
        {
            if (obj is Insets vCompareTo)
            {
                return Math.Abs(vCompareTo.Top - Top) < Geometry.Epsilon && Math.Abs(vCompareTo.Left - Left) < Geometry.Epsilon && Math.Abs(vCompareTo.Bottom - Bottom) < Geometry.Epsilon &&
                       Math.Abs(vCompareTo.Right - Right) < Geometry.Epsilon;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return (int) _top ^ (int) _left + (int) _bottom ^ (int) _right;
        }

        public string ToParsableString()
        {
            return _top.ToString(CultureInfo.InvariantCulture) + "," + _left.ToString(CultureInfo.InvariantCulture) + "," + _bottom.ToString(CultureInfo.InvariantCulture) + "," +
                   _right.ToString(CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return $"[Insets: Top={_top}, Left={_left}, Bottom={_bottom}, Right={_right}]";
        }

        public static Insets Parse(string value)
        {
            try
            {
                var values = value.Split(',');
                double top = double.Parse(values[0], CultureInfo.InvariantCulture);
                double left = double.Parse(values[1], CultureInfo.InvariantCulture);
                double bottom = double.Parse(values[2], CultureInfo.InvariantCulture);
                double right = double.Parse(values[3], CultureInfo.InvariantCulture);
                return new Insets(top, left, bottom, right);
            }
            catch (Exception exc)
            {
#if DEBUG
                Logger.Debug(exc);
#endif
                return new Insets(0, 0, 0, 0);
            }
        }
    }
}