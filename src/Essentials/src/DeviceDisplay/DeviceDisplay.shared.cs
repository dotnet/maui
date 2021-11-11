#nullable enable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Essentials
{
	public static partial class DeviceDisplay
	{
		static readonly object locker = new object();
		static IDeviceDisplay currentImplementation;

		static DeviceDisplay()
		{
			currentImplementation = new DeviceDisplayImplementation();
			currentImplementation.MainDisplayInfoChanged += OnMainDisplayInfoChanged;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IDeviceDisplay Current => currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(IDeviceDisplay? implementation)
		{
			lock (locker)
			{
				if (currentImplementation == implementation)
					return;

				var newImplementation = implementation ?? new DeviceDisplayImplementation();

				var oldImplementation = currentImplementation;
				currentImplementation = newImplementation;

				if (oldImplementation is not null)
				{
					oldImplementation.MainDisplayInfoChanged -= OnMainDisplayInfoChanged;

					var wasAlwaysOn = oldImplementation.KeepScreenOn;
					if (wasAlwaysOn)
					{
						oldImplementation.KeepScreenOn = false;
						newImplementation.KeepScreenOn = true;
					}

					var wasRunning = MainDisplayInfoChangedInternal != null;
					if (wasRunning)
					{
						oldImplementation.StopScreenMetricsListeners();
						newImplementation.StartScreenMetricsListeners();
					}
				}

				newImplementation.MainDisplayInfoChanged += OnMainDisplayInfoChanged;

				SetCurrent(newImplementation.GetMainDisplayInfo());
			}
		}

		static event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChangedInternal;

		static DisplayInfo currentMetrics;

		public static bool KeepScreenOn
		{
			get => Current.KeepScreenOn;
			set => Current.KeepScreenOn = value;
		}

		public static DisplayInfo MainDisplayInfo => Current.GetMainDisplayInfo();

		static void SetCurrent(DisplayInfo metrics) =>
			currentMetrics = new DisplayInfo(metrics.Width, metrics.Height, metrics.Density, metrics.Orientation, metrics.Rotation, metrics.RefreshRate);

		public static event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
		{
			add
			{
				var wasRunning = MainDisplayInfoChangedInternal != null;

				MainDisplayInfoChangedInternal += value;

				if (!wasRunning && MainDisplayInfoChangedInternal != null)
				{
					SetCurrent(Current.GetMainDisplayInfo());
					Current.StartScreenMetricsListeners();
				}
			}

			remove
			{
				var wasRunning = MainDisplayInfoChangedInternal != null;

				MainDisplayInfoChangedInternal -= value;

				if (wasRunning && MainDisplayInfoChangedInternal == null)
					Current.StopScreenMetricsListeners();
			}
		}

		static void OnMainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e) =>
			OnMainDisplayInfoChanged(e);

		static void OnMainDisplayInfoChanged(DisplayInfoChangedEventArgs e)
		{
			if (!currentMetrics.Equals(e.DisplayInfo))
			{
				SetCurrent(e.DisplayInfo);
				MainDisplayInfoChangedInternal?.Invoke(null, e);
			}
		}
	}

	public class DisplayInfoChangedEventArgs : EventArgs
	{
		public DisplayInfoChangedEventArgs(DisplayInfo displayInfo) =>
			DisplayInfo = displayInfo;

		public DisplayInfo DisplayInfo { get; }
	}

	public interface IDeviceDisplay
	{
		bool KeepScreenOn { get; set; }

		void StartScreenMetricsListeners();

		void StopScreenMetricsListeners();

		DisplayInfo GetMainDisplayInfo();

		event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged;
	}
}
