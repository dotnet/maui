using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Maps.Handlers;

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IMauiHandlersCollection AddMauiMapsControlsHandlers(this IMauiHandlersCollection handlersCollection)
		{
			handlersCollection.AddHandler<Map, MapHandler>();
			return handlersCollection;
		}
	}
}
