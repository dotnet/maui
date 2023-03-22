#nullable enable
using System;

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// Represents information about the device screen.
	/// </summary>
	public interface IDeviceDisplay
	{
		/// <summary>
		/// Gets or sets if the screen should be kept on.
		/// </summary>
		bool KeepScreenOn { get; set; }

		/// <summary>
		/// Gets the main screen's display info.
		/// </summary>
		DisplayInfo MainDisplayInfo { get; }

		/// <summary>
		/// Occurs when the main display's info changes.
		/// </summary>
		event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged;
	}

	/// <summary>
	/// Main display information event arguments.
	/// </summary>
	public class DisplayInfoChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DisplayInfoChangedEventArgs"/> class.
		/// </summary>
		/// <param name="displayInfo">The display info associated to this event.</param>
		public DisplayInfoChangedEventArgs(DisplayInfo displayInfo) =>
			DisplayInfo = displayInfo;

		/// <summary>
		/// Gets the current display info for the main display associated to this event.
		/// </summary>
		public DisplayInfo DisplayInfo { get; }
	}

	/// <summary>
	/// Represents information about the device screen.
	/// </summary>
	public static class DeviceDisplay
	{
		/// <summary>
		/// Gets or sets if the screen should be kept on.
		/// </summary>
		public static bool KeepScreenOn
		{
			get => Current.KeepScreenOn;
			set => Current.KeepScreenOn = value;
		}

		/// <summary>
		/// Gets the main screen's display info.
		/// </summary>
		public static DisplayInfo MainDisplayInfo => Current.MainDisplayInfo;

		/// <summary>
		/// Occurs when the main display's info changes.
		/// </summary>
		public static event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
		{
			add => Current.MainDisplayInfoChanged += value;
			remove => Current.MainDisplayInfoChanged -= value;
		}

#if WINDOWS
		internal const float BaseLogicalDpi = 96.0f;
#elif ANDROID || TIZEN
		internal const float BaseLogicalDpi = 160.0f;
#endif

		static IDeviceDisplay? currentImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IDeviceDisplay Current =>
			currentImplementation ??= new DeviceDisplayImplementation();

		internal static void SetCurrent(IDeviceDisplay? implementation) =>
			currentImplementation = implementation;
	}

	sealed partial class DeviceDisplayImplementation : DeviceDisplayImplementationBase
	{
	}

	abstract class DeviceDisplayImplementationBase : IDeviceDisplay
	{
		event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChangedInternal;

		DisplayInfo _currentMetrics;

		public DisplayInfo MainDisplayInfo => GetMainDisplayInfo();

		public bool KeepScreenOn
		{
			get => GetKeepScreenOn();
			set => SetKeepScreenOn(value);
		}

		public event EventHandler<DisplayInfoChangedEventArgs> MainDisplayInfoChanged
		{
			add
			{
				if (MainDisplayInfoChangedInternal is null)
				{
					SetCurrent(MainDisplayInfo);
					StartScreenMetricsListeners();
				}
				MainDisplayInfoChangedInternal += value;
			}
			remove
			{
				var wasStopped = MainDisplayInfoChangedInternal is null;
				MainDisplayInfoChangedInternal -= value;
				if (!wasStopped && MainDisplayInfoChangedInternal is null)
					StopScreenMetricsListeners();
			}
		}

		void SetCurrent(DisplayInfo metrics) =>
			_currentMetrics = new DisplayInfo(
				metrics.Width, metrics.Height,
				metrics.Density,
				metrics.Orientation,
				metrics.Rotation,
				metrics.RefreshRate);

		protected void OnMainDisplayInfoChanged(DisplayInfoChangedEventArgs e)
		{
			if (!_currentMetrics.Equals(e.DisplayInfo))
			{
				SetCurrent(e.DisplayInfo);
				MainDisplayInfoChangedInternal?.Invoke(null, e);
			}
		}

		protected void OnMainDisplayInfoChanged()
		{
			var metrics = GetMainDisplayInfo();
			OnMainDisplayInfoChanged(new DisplayInfoChangedEventArgs(metrics));
		}

		protected abstract DisplayInfo GetMainDisplayInfo();

		protected abstract bool GetKeepScreenOn();

		protected abstract void SetKeepScreenOn(bool keepScreenOn);

		protected abstract void StartScreenMetricsListeners();

		protected abstract void StopScreenMetricsListeners();
	}
}
