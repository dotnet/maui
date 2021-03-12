using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Encapsulates the Application and its available services.
	/// </summary>
	public interface IApplication : IApplicationLifetime
	{
		/// <summary>
		/// Gets a collection of application-scoped services.
		/// </summary>
		IServiceProvider? Services { get; }
	}
}