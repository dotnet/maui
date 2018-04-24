using System;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static event ScreenMetricsChanagedEventHandler ScreenMetricsChanagedInternal;

        public static ScreenMetrics ScreenMetrics => GetScreenMetrics();

        public static event ScreenMetricsChanagedEventHandler ScreenMetricsChanaged
        {
            add
            {
                var wasRunning = ScreenMetricsChanagedInternal != null;

                ScreenMetricsChanagedInternal += value;

                if (!wasRunning && ScreenMetricsChanagedInternal != null)
                    StartScreenMetricsListeners();
            }

            remove
            {
                var wasRunning = ScreenMetricsChanagedInternal != null;

                ScreenMetricsChanagedInternal -= value;

                if (wasRunning && ScreenMetricsChanagedInternal == null)
                    StopScreenMetricsListeners();
            }
        }

        static void OnScreenMetricsChanaged(ScreenMetrics metrics)
            => OnScreenMetricsChanaged(new ScreenMetricsChanagedEventArgs(metrics));

        static void OnScreenMetricsChanaged(ScreenMetricsChanagedEventArgs e)
            => ScreenMetricsChanagedInternal?.Invoke(e);
    }

    public delegate void ScreenMetricsChanagedEventHandler(ScreenMetricsChanagedEventArgs e);

    public class ScreenMetricsChanagedEventArgs : EventArgs
    {
        public ScreenMetricsChanagedEventArgs(ScreenMetrics metrics)
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
