using System;

namespace Xamarin.Essentials
{
    [Preserve(AllMembers = true)]
    public readonly struct DisplayInfo : IEquatable<DisplayInfo>
    {
        public DisplayInfo(double width, double height, double density, DisplayOrientation orientation, DisplayRotation rotation)
        {
            Width = width;
            Height = height;
            Density = density;
            Orientation = orientation;
            Rotation = rotation;
        }

        public double Width { get; }

        public double Height { get; }

        public double Density { get; }

        public DisplayOrientation Orientation { get; }

        public DisplayRotation Rotation { get; }

        public static bool operator ==(DisplayInfo left, DisplayInfo right) =>
            left.Equals(right);

        public static bool operator !=(DisplayInfo left, DisplayInfo right) =>
            !left.Equals(right);

        public override bool Equals(object obj) =>
            (obj is DisplayInfo metrics) && Equals(metrics);

        public bool Equals(DisplayInfo other) =>
            Width.Equals(other.Width) &&
            Height.Equals(other.Height) &&
            Density.Equals(other.Density) &&
            Orientation.Equals(other.Orientation) &&
            Rotation.Equals(other.Rotation);

        public override int GetHashCode() =>
            (Height, Width, Density, Orientation, Rotation).GetHashCode();

        public override string ToString() =>
            $"{nameof(Height)}: {Height}, {nameof(Width)}: {Width}, " +
            $"{nameof(Density)}: {Density}, {nameof(Orientation)}: {Orientation}, " +
            $"{nameof(Rotation)}: {Rotation}";
    }
}
