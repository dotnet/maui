#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Base class for factories that create elements for registered routes.</summary>
	public abstract class RouteFactory
	{
		/// <summary>Gets or creates the element for this route.</summary>
		/// <returns>The element associated with the route.</returns>
		public abstract Element GetOrCreate();

		/// <summary>Gets or creates the element for this route using dependency injection.</summary>
		/// <param name="services">The service provider for dependency injection.</param>
		/// <returns>The element associated with the route.</returns>
		public abstract Element GetOrCreate(IServiceProvider services);
	}
}
