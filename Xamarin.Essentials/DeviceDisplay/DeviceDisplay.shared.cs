using System;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static event EventHandler<ScreenMetricsChangedEventArgs> ScreenMetricsChangedInternal;

        static ScreenMetrics currentMetrics;

        public static ScreenMetrics ScreenMetrics => GetScreenMetrics();

        static void SetCurrent(ScreenMetrics metrics) =>
            currentMetrics = new ScreenMetrics(metrics.Width, metrics.Height, metrics.Density, metrics.Orientation, metrics.Rotation);

        public static event EventHandler<ScreenMetricsChangedEventArgs> ScreenMetricsChanged
        {
            add
            {
                var wasRunning = ScreenMetricsChangedInternal != null;

                ScreenMetricsChangedInternal += value;

                if (!wasRunning && ScreenMetricsChangedInternal != null)
                {
                    SetCurrent(GetScreenMetrics());
                    StartScreenMetricsListeners();
                }
            }

            remove
            {
                var wasRunning = ScreenMetricsChangedInternal != null;

                ScreenMetricsChangedInternal -= value;

                if (wasRunning && ScreenMetricsChangedInternal == null)
                    StopScreenMetricsListeners();
            }
        }

        static void OnScreenMetricsChanged(ScreenMetrics metrics)
            => OnScreenMetricsChanged(new ScreenMetricsChangedEventArgs(metrics));

        static void OnScreenMetricsChanged(ScreenMetricsChangedEventArgs e)
        {
            if (!currentMetrics.Equals(e.Metrics))
            {
                SetCurrent(e.Metrics);
                ScreenMetricsChangedInternal?.Invoke(null, e);
            }
        }
    }

    public class ScreenMetricsChangedEventArgs : EventArgs
    {
        public ScreenMetricsChangedEventArgs(ScreenMetrics metrics) =>
            Metrics = metrics;

        public ScreenMetrics Metrics { get; }
    }

    [Preserve(AllMembers = true)]
    public readonly struct ScreenMetrics : IEquatable<ScreenMetrics>
    {
        internal ScreenMetrics(double width, double height, double density, ScreenOrientation orientation, ScreenRotation rotation)
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

        public ScreenOrientation Orientation { get; }

        public ScreenRotation Rotation { get; }

        public static bool operator ==(ScreenMetrics left, ScreenMetrics right) =>
            Equals(left, right);

        public static bool operator !=(ScreenMetrics left, ScreenMetrics right) =>
            !Equals(left, right);

        public override bool Equals(object obj) =>
            (obj is ScreenMetrics metrics) && Equals(metrics);

        public bool Equals(ScreenMetrics other) =>
            Width.Equals(other.Width) &&
            Height.Equals(other.Height) &&
            Density.Equals(other.Density) &&
            Orientation.Equals(other.Orientation) &&
            Rotation.Equals(other.Rotation);

        public override int GetHashCode() =>
            (Height, Width, Density, Orientation, Rotation).GetHashCode();

        public override string ToString() =>
            $"Height: {Height}, Width: {Width}, Density: {Density}, Orientation: {Orientation}, Rotation: {Rotation}";
    }

    public enum ScreenOrientation
    {
        Unknown,
        Portrait,
        Landscape
    }

    public enum ScreenRotation
    {
        Rotation0,
        Rotation90,
        Rotation180,
        Rotation270
    }
}
