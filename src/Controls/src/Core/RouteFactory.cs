using System;

namespace Microsoft.Maui.Controls
{
	public abstract class RouteFactory
	{
		public abstract Element GetOrCreate();
	}
}