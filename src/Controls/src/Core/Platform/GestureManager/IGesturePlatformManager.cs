using System;

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Interface for platform-specific gesture management.
	/// </summary>
	// TODO: Make this public in a future release once a usable extensibility contract
	// (with real members and a public/replaceable implementation) is designed. See #33364.
	internal interface IGesturePlatformManager : IDisposable
	{
	}
}
