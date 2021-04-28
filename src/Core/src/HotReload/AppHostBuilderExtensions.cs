#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.HotReload;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IAppHostBuilder EnableHotReload(this IAppHostBuilder builder, string? ideIp = null, int idePort = 9988)
		{
			builder.ConfigureServices<HotReloadBuilder>(hotReload =>
			{
				hotReload.IdeIp = ideIp;
				hotReload.IdePort = idePort;
			});
			return builder;
		}

		class HotReloadBuilder : IMauiServiceBuilder
		{
			public string? IdeIp { get; set; }

			public int IdePort { get; set; } = 9988;

			public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
			{
			}

			public async void Configure(HostBuilderContext context, IServiceProvider services)
			{
				var handlers = services.GetRequiredService<IMauiHandlersServiceProvider>();
				if (handlers is IHotReloadableHandlersServiceProvider hotReloadable)
					MauiHotReloadHelper.Init(hotReloadable.GetCollection());
				else
					throw new NotSupportedException($"Hot Reload only works with a {nameof(IHotReloadableHandlersServiceProvider)}.");

				Reloadify.Reload.Instance.ReplaceType = (d) =>
				{
					MauiHotReloadHelper.RegisterReplacedView(d.ClassName, d.Type);
				};

				Reloadify.Reload.Instance.FinishedReload = () =>
				{
					MauiHotReloadHelper.TriggerReload();
				};

				await Task.Run(async () =>
				{
					try
					{
						var success = await Reloadify.Reload.Init(IdeIp, IdePort);

						Console.WriteLine($"HotReload Initialize: {success}");
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				});
			}
		}
	}
}