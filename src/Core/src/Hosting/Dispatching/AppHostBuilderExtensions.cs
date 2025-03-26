using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureDispatching(this MauiAppBuilder builder)
		{
			// register the DispatcherProvider as a singleton for the entire app
			builder.Services.TryAddSingleton<IDispatcherProvider>(svc =>
				// the DispatcherProvider might have already been initialized, so ensure that we are grabbing the
				// Current and putting it in the DI container.
				DispatcherProvider.Current);

			// register a fallback dispatcher when the service provider does not support keyed services
			builder.Services.TryAddSingleton<ApplicationDispatcher>((svc) => new ApplicationDispatcher(GetDispatcher(svc, false)));
			// register the initializer so we can init the dispatcher in the app thread for the app
			builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IMauiInitializeService, ApplicationDispatcherInitializer>());

			// register the Dispatcher as a scoped service as there may be different dispatchers per window
			builder.Services.TryAddScoped<IDispatcher>((svc) => GetDispatcher(svc, true));
			// register the initializer so we can init the dispatcher in the window thread for that window
			builder.Services.TryAddEnumerable(ServiceDescriptor.Scoped<IMauiInitializeScopedService, DispatcherInitializer>());

			return builder;
		}

		public static MauiAppBuilder ConfigureEnvironmentVariables(this MauiAppBuilder builder)
		{
			IDictionary environmentVariables = Environment.GetEnvironmentVariables();

#if !NETSTANDARD
			// For Android we read the environment variables from a text file that is written to the device/emulator
			if (OperatingSystem.IsAndroid())
			{
				var envVarLines = System.IO.File.ReadAllLines("/data/local/tmp/ide-launchenv.txt");

				environmentVariables = envVarLines
					.Select(line => line.Split('=', 2))
					.ToDictionary(parts => parts[0], parts => parts[1]);
			}
#endif

			string devTunnelId = environmentVariables["DEVTUNNEL_ID"]?.ToString() ?? string.Empty;

			var variablesToInclude = new HashSet<string>
			{
				"ASPNETCORE_ENVIRONMENT",
				"ASPNETCORE_URLS",
				"DOTNET_ENVIRONMENT",
				"DOTNET_LAUNCH_PROFILE",
				"DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION"
			};

			var prefixesToRemove = new List<string>
			{
				"ASPNETCORE_",
				"DOTNET_",
			};

			List<KeyValuePair<string, string?>> settings = [];
			foreach (object variableNameObject in environmentVariables.Keys)
			{
				string variableName = (string)variableNameObject;
				if (variablesToInclude.Contains(variableName) || variableName.StartsWith("OTEL_")
					|| variableName.StartsWith("LOGGING__CONSOLE") || variableName.StartsWith("services__"))
				{
					string value = (string)environmentVariables[variableName]!;

					// Normalize the key, matching the logic here:
					// https://github.dev/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Configuration.EnvironmentVariables/src/EnvironmentVariablesConfigurationProvider.cs
#if NETSTANDARD2_0
					variableName = variableName.Replace("__", ":");
#else
					variableName = variableName.Replace("__", ":", StringComparison.OrdinalIgnoreCase);
#endif

					// For defined prefixes, add the variable with the prefix removed, matching the logic
					// in EnvironmentVariablesConfigurationProvider.cs. Also add the variable with the
					// prefix intact, which matches the normal HostApplicationBuilder behavior, where
					// there's an EnvironmentVariablesConfigurationProvider added with and another one
					// without the prefix set.
					foreach (var prefix in prefixesToRemove)
					{
						if (variableName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
						{
							settings.Add(new KeyValuePair<string, string?>(variableName, value));
							variableName = variableName.Substring(prefix.Length);
							break;
						}
					}

					if (!string.IsNullOrEmpty(devTunnelId))
					{
						value = ReplaceLocalhost(value, devTunnelId);
					}
					settings.Add(new KeyValuePair<string, string?>(variableName, value));
				}
			}

			builder.Configuration.AddInMemoryCollection(settings);

			return builder;
		}

		static string ReplaceLocalhost(string uri, string devTunnelId)
		{
			// source format is `http[s]://localhost:[port]`
			// tunnel format is `http[s]://exciting-tunnel-[port].devtunnels.ms`

			var replacement = Regex.Replace(
				uri,
				@"://localhost\:(\d+)(.*)",
				$"://{devTunnelId}-$1.devtunnels.ms$2",
				RegexOptions.Compiled);

			return replacement;
		}

		internal static IDispatcher GetRequiredApplicationDispatcher(this IServiceProvider provider)
		{
			if (provider is IKeyedServiceProvider keyed)
			{
				var dispatcher = keyed.GetKeyedService<IDispatcher>(typeof(IApplication));
				if (dispatcher is not null)
				{
					return dispatcher;
				}
			}

			return provider.GetRequiredService<ApplicationDispatcher>().Dispatcher;
		}

		internal static IDispatcher? GetOptionalApplicationDispatcher(this IServiceProvider provider)
		{
			if (provider is IKeyedServiceProvider keyed)
			{
				var dispatcher = keyed.GetKeyedService<IDispatcher>(typeof(IApplication));
				if (dispatcher is not null)
				{
					return dispatcher;
				}
			}

			return provider.GetService<ApplicationDispatcher>()?.Dispatcher;
		}

		static IDispatcher GetDispatcher(IServiceProvider services, bool fallBackToApplicationDispatcher)
		{
			var provider = services.GetRequiredService<IDispatcherProvider>();
			if (DispatcherProvider.SetCurrent(provider))
			{
				services.CreateLogger<Dispatcher>()?.LogWarning("Replaced an existing DispatcherProvider with one from the service provider.");
			}

			var result = Dispatcher.GetForCurrentThread();

			if (fallBackToApplicationDispatcher && result is null)
				result = services.GetRequiredService<ApplicationDispatcher>().Dispatcher;

			return result!;
		}

		class ApplicationDispatcherInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
				_ = services.GetOptionalApplicationDispatcher();
			}
		}

		class DispatcherInitializer : IMauiInitializeScopedService
		{
			public void Initialize(IServiceProvider services)
			{
				_ = services.GetRequiredService<IDispatcher>();
			}
		}
	}
}
