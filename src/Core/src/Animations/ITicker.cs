using System;

namespace Microsoft.Maui.Animations
{
	/// <summary>
	/// A ticker makes sure that animations get triggered to advance and progress through their different stages.
	/// </summary>
	public interface ITicker
	{
		/// <summary>
		/// Specifies whether this ticker is currently running.
		/// </summary>
		bool IsRunning { get; }

		/// <summary>
		/// Specifies whether this ticker is system enabled.
		/// </summary>
		/// <remarks>If <see langword="false"/>, it might be disabled by the device energy savings for instance.</remarks>
		bool SystemEnabled { get; }

		/// <summary>
		/// Maximum frames per second this ticker can handle.
		/// </summary>
		int MaxFps { get; set; }

		/// <summary>
		/// The <see cref="Action"/> that is triggered when this ticker interval has been reached.
		/// </summary>
		Action? Fire { get; set; }

		/// <summary>
		/// Starts running this ticker.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops this ticker from running.
		/// </summary>
		void Stop();
	}
}