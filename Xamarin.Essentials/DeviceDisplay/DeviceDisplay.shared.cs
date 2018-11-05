using System;

namespace Xamarin.Essentials
{
    public static partial class DeviceDisplay
    {
        static event EventHandler<MainDisplayInfoChangedEventArgs> MainDisplayInfoChangedInternal;

        static DisplayInfo currentMetrics;

        public static bool KeepScreenOn
        {
            get => PlatformKeepScreenOn;
            set => PlatformKeepScreenOn = value;
        }

        public static DisplayInfo MainDisplayInfo => GetMainDisplayInfo();

        static void SetCurrent(DisplayInfo metrics) =>
            currentMetrics = new DisplayInfo(metrics.Width, metrics.Height, metrics.Density, metrics.Orientation, metrics.Rotation);

        public static event EventHandler<MainDisplayInfoChangedEventArgs> MainDisplayInfoChanged
        {
            add
            {
                var wasRunning = MainDisplayInfoChangedInternal != null;

                MainDisplayInfoChangedInternal += value;

                if (!wasRunning && MainDisplayInfoChangedInternal != null)
                {
                    SetCurrent(GetMainDisplayInfo());
                    StartScreenMetricsListeners();
                }
            }

            remove
            {
                var wasRunning = MainDisplayInfoChangedInternal != null;

                MainDisplayInfoChangedInternal -= value;

                if (wasRunning && MainDisplayInfoChangedInternal == null)
                    StopScreenMetricsListeners();
            }
        }

        static void OnMainDisplayInfoChanged(DisplayInfo metrics)
            => OnMainDisplayInfoChanged(new MainDisplayInfoChangedEventArgs(metrics));

        static void OnMainDisplayInfoChanged(MainDisplayInfoChangedEventArgs e)
        {
            if (!currentMetrics.Equals(e.MainDisplayInfo))
            {
                SetCurrent(e.MainDisplayInfo);
                MainDisplayInfoChangedInternal?.Invoke(null, e);
            }
        }
    }

    public class MainDisplayInfoChangedEventArgs : EventArgs
    {
        public MainDisplayInfoChangedEventArgs(DisplayInfo mainDisplayInfo) =>
            MainDisplayInfo = mainDisplayInfo;

        public DisplayInfo MainDisplayInfo { get; }
    }
}
