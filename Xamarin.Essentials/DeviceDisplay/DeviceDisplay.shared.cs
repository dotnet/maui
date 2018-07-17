using System;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static event EventHandler<ScreenMetricsChangedEventArgs> ScreenMetricsChangedInternal;

        public static ScreenMetrics ScreenMetrics => GetScreenMetrics();

        public static event EventHandler<ScreenMetricsChangedEventArgs> ScreenMetricsChanged
        {
            add
            {
                var wasRunning = ScreenMetricsChangedInternal != null;

                ScreenMetricsChangedInternal += value;

                if (!wasRunning && ScreenMetricsChangedInternal != null)
                    StartScreenMetricsListeners();
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
            => ScreenMetricsChangedInternal?.Invoke(null, e);
    }

    public class ScreenMetricsChangedEventArgs : EventArgs
    {
        public ScreenMetricsChangedEventArgs(ScreenMetrics metrics)
        {
            Metrics = metrics;
        }

        public ScreenMetrics Metrics { get; }
    }

    [Preserve(AllMembers = true)]
    public struct ScreenMetrics
    {
        internal ScreenMetrics(double width, double height, double density, ScreenOrientation orientation, ScreenRotation rotation)
        {
            Width = width;
            Height = height;
            Density = density;
            Orientation = orientation;
            Rotation = rotation;
        }

        public double Width { get; set; }

        public double Height { get; set; }

        public double Density { get; set; }

        public ScreenOrientation Orientation { get; set; }

        public ScreenRotation Rotation { get; set; }
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
