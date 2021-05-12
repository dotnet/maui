using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Controls
{
	class RedServiceBuilder : IMauiServiceBuilder
	{
		static IMauiHandlersCollection handlersCollection;
		static Dictionary<Type, Type> pendingHandlers = new();

		public static void TryAddHandler<TType, TTypeRender>()
			where TType : IFrameworkElement
			where TTypeRender : IViewHandler
		{
			if (handlersCollection == null)
				pendingHandlers[typeof(TType)] = typeof(TTypeRender);
			else
				handlersCollection.TryAddHandler<TType, TTypeRender>();
		}

		void IMauiServiceBuilder.ConfigureServices(HostBuilderContext context, IServiceCollection services)
		{
			// no-op
		}

		void IMauiServiceBuilder.Configure(HostBuilderContext context, IServiceProvider services)
		{
			handlersCollection ??= services.GetRequiredService<IMauiHandlersServiceProvider>().GetCollection();

			if (pendingHandlers.Count > 0)
			{
				handlersCollection.TryAddHandlers(pendingHandlers);
				pendingHandlers.Clear();
			}
		}
	}
}