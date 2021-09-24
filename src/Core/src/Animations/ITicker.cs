using System;

namespace Microsoft.Maui.Animations
{
	public interface ITicker
	{
		bool IsRunning { get; }

		bool SystemEnabled { get; }

		int MaxFps { get; set; }

		Action? Fire { get; set; }

		void Start();

		void Stop();
	}
}