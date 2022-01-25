using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// IPlatformApplication.
	/// Hosts the platform application.
	/// </summary>
	public interface IPlatformApplication
	{
		/// <summary>
		/// Gets the current IPlatformApplication.
		/// </summary>
		public static IPlatformApplication? Current { get; }

		/// <summary>
		/// Gets the Service Provider.
		/// <see cref="IServiceProvider"/>.
		/// </summary>
		public IServiceProvider Services { get; }

		/// <summary>
		/// Gets the Application.
		/// <see cref="IApplication"/>.
		/// </summary>
		public IApplication Application { get; }
	}
}
