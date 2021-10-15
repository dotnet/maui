using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Controls.DualScreen
{
	public static class HostBuilderExtensions
	{
		public static MauiAppBuilder UseDualScreen(this MauiAppBuilder builder)
		{
#if ANDROID
			builder.ConfigureLifecycleEvents(lc =>
			{
				lc.AddAndroid(android =>
				{
					android.OnConfigurationChanged((activity, configuration) =>
					{

					})
					.OnResume((activity) =>
					{

					})
					.OnCreate((activity, bundle) =>
					{

					});
				});
			});
#endif

			return builder;
		}
	}
}