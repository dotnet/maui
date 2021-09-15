#nullable enable
using System;

namespace Microsoft.Maui.Essentials
{
	public static partial class DeviceDisplay
	{
		static readonly Lazy<IDeviceDisplay> PlatformDeviceDisplay = new(() => new PlatformDeviceDisplay());
		static IDeviceDisplay? CurrentDeviceDisplay;

		public static void SetCurrent(IDeviceDisplay? current) => CurrentDeviceDisplay = current;

		public static IDeviceDisplay Current => CurrentDeviceDisplay ?? PlatformDeviceDisplay.Value;

		public static bool KeepScreenOn
		{
			get => Current.KeepScreenOn;
			set => Current.KeepScreenOn = value;
		}

		public static DisplayInfo MainDisplayInfo => Current.MainDisplayInfo;

		public static event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
		{
			add => Current.MainDisplayInfoChanged += value;
			remove => Current.MainDisplayInfoChanged -= value;
		}
	}

	partial class PlatformDeviceDisplay : IDeviceDisplay
	{
		event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChangedInternal;

		DisplayInfo _currentMetrics;

		public bool KeepScreenOn
		{
			get => PlatformKeepScreenOn;
			set => PlatformKeepScreenOn = value;
		}

		public DisplayInfo MainDisplayInfo => GetMainDisplayInfo();

		void SetCurrent(DisplayInfo metrics) =>
			_currentMetrics = new DisplayInfo(metrics.Width, metrics.Height, metrics.Density, metrics.Orientation, metrics.Rotation, metrics.RefreshRate);

		public event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChanged
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

		void OnMainDisplayInfoChanged(DisplayInfo metrics)
			=> OnMainDisplayInfoChanged(new DisplayInfoChangedEventArgs(metrics));

		void OnMainDisplayInfoChanged(DisplayInfoChangedEventArgs e)
		{
			if (!_currentMetrics.Equals(e.DisplayInfo))
			{
				SetCurrent(e.DisplayInfo);
				MainDisplayInfoChangedInternal?.Invoke(null, e);
			}
		}
	}

	public interface IDeviceDisplay
	{
		bool KeepScreenOn { get; set; }

		DisplayInfo MainDisplayInfo { get; }

		event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged;
	}

	public class DisplayInfoChangedEventArgs : EventArgs
	{
		public DisplayInfoChangedEventArgs(DisplayInfo displayInfo) =>
			DisplayInfo = displayInfo;

		public DisplayInfo DisplayInfo { get; }
	}
}
