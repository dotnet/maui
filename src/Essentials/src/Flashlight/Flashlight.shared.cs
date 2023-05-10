#nullable enable
using System.Threading.Tasks;

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// The Flashlight API has the ability to turn on or off the device's camera flash to turn it into a flashlight.
	/// </summary>
	public interface IFlashlight
	{
		/// <summary>
		/// Checks if the flashlight is available and can be turned on or off.
		/// </summary>
		/// <returns><see langword="true"/> when the flashlight is available, or <see langword="false"/> when not</returns>
		Task<bool> IsSupportedAsync();

		/// <summary>
		/// Turns the camera flashlight on.
		/// </summary>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task TurnOnAsync();

		/// <summary>
		/// Turns the camera flashlight off.
		/// </summary>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		Task TurnOffAsync();
	}

	/// <summary>
	/// The Flashlight API has the ability to turn on or off the device's camera flash to turn it into a flashlight.
	/// </summary>
	public static partial class Flashlight
	{
		/// <summary>
		/// Checks if the flashlight is available and can be turned on or off.
		/// </summary>
		/// <returns><see langword="true"/> when the flashlight is available, or <see langword="false"/> when not</returns>
		public static Task<bool> IsSupportedAsync() => Default.IsSupportedAsync();

		/// <summary>
		/// Turns the camera flashlight on.
		/// </summary>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task TurnOnAsync() =>
			Default.TurnOnAsync();

		/// <summary>
		/// Turns the camera flashlight off.
		/// </summary>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		public static Task TurnOffAsync() =>
			Default.TurnOffAsync();

		static IFlashlight? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IFlashlight Default =>
			defaultImplementation ??= new FlashlightImplementation();

		internal static void SetDefault(IFlashlight? implementation) =>
			defaultImplementation = implementation;
	}
}
