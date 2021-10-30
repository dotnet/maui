using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class HostBuilderAppTests
	{
		[Test]
		public void UseMauiAppRegistersApp()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var app = (ApplicationStub)mauiApp.Services.GetRequiredService<IApplication>();
			Assert.AreEqual("Default", app.Property);
		}

		[Test]
		public void UseMauiAppRegistersAppWithFactory()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp(services => new ApplicationStub { Property = "Factory" })
				.Build();

			var app = (ApplicationStub)mauiApp.Services.GetRequiredService<IApplication>();
			Assert.AreEqual("Factory", app.Property);
		}

		[Test]
		public void UseMauiAppRegistersSingleton()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var app1 = mauiApp.Services.GetRequiredService<IApplication>();
			var app2 = mauiApp.Services.GetRequiredService<IApplication>();

			Assert.AreEqual(app1, app2);
		}

		[Test]
		public void AddingMemoryStreamBackedConfigurationWorks()
		{
			var builder = MauiApp.CreateBuilder();

			var jsonConfig = @"{ ""foo"": ""bar"" }";
			using var ms = new MemoryStream();
			using var sw = new StreamWriter(ms);
			sw.WriteLine(jsonConfig);
			sw.Flush();

			ms.Position = 0;
			builder.Configuration.AddJsonStream(ms);

			Assert.AreEqual("bar", builder.Configuration["foo"]);

			using var app = builder.Build();

			Assert.AreEqual("bar", app.Configuration["foo"]);
		}

		[Test]
		public void ConfigurationGetDebugViewWorks()
		{
			var builder = MauiApp.CreateBuilder();

			builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
			{
				["foo"] = "bar",
			});

			using var app = builder.Build();

			// Make sure we don't lose "MemoryConfigurationProvider" from GetDebugView() when wrapping the provider.
			Assert.True(((IConfigurationRoot)app.Configuration).GetDebugView().Contains("foo=bar (MemoryConfigurationProvider)"));
		}

		[Test]
		public void ConfigurationProvidersAreLoadedOnceAfterBuild()
		{
			var builder = MauiApp.CreateBuilder();

			var configSource = new TrackingConfigurationSource();
			((IConfigurationBuilder)builder.Configuration).Sources.Add(configSource);

			using var app = builder.Build();

			Assert.AreEqual(1, configSource.ProvidersLoaded);
		}

		[Test]
		public void ConfigurationProvidersAreDisposedWithMauiApp()
		{
			var builder = MauiApp.CreateBuilder();

			var configSource = new TrackingConfigurationSource();
			((IConfigurationBuilder)builder.Configuration).Sources.Add(configSource);

			{
				using var app = builder.Build();

				Assert.AreEqual(0, configSource.ProvidersDisposed);
			}

			Assert.AreEqual(1, configSource.ProvidersDisposed);
		}

		[Test]
		public void ConfigurationProviderTypesArePreserved()
		{
			var builder = MauiApp.CreateBuilder();

			((IConfigurationBuilder)builder.Configuration).Sources.Add(new TrackingConfigurationSource());

			var app = builder.Build();

			Assert.That(((IConfigurationRoot)app.Configuration).Providers.OfType<TrackingConfigurationProvider>(), Has.One.Items);
		}

		[Test]
		public void MauiAppCanObserveSourcesClearedInInnerBuild()
		{
			using var listener = new HostingListener(hostBuilder =>
			{
				hostBuilder.ConfigureHostConfiguration(config =>
				{
					// Clearing here would not remove the app config added via builder.Configuration.
					config.AddInMemoryCollection(new Dictionary<string, string>()
					{
						{ "A", "A" },
					});
				});

				hostBuilder.ConfigureAppConfiguration(config =>
				{
					// This clears both the chained host configuration and chained builder.Configuration.
					config.Sources.Clear();
					config.AddInMemoryCollection(new Dictionary<string, string>()
					{
						{ "B", "B" },
					});
				});
			});

			var builder = MauiApp.CreateBuilder();

			builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>()
			{
				{ "C", "C" },
			});

			using var app = builder.Build();

			Assert.True(string.IsNullOrEmpty(app.Configuration["A"]));
			Assert.True(string.IsNullOrEmpty(app.Configuration["C"]));

			Assert.AreEqual("B", app.Configuration["B"]);

			Assert.AreSame(builder.Configuration, app.Configuration);
		}

		[Test]
		public void MauiAppCanHandleStreamBackedConfigurationAddedInInnerBuild()
		{
			static Stream CreateStreamFromString(string data) => new MemoryStream(Encoding.UTF8.GetBytes(data));

			using var jsonAStream = CreateStreamFromString(@"{ ""A"": ""A"" }");
			using var jsonBStream = CreateStreamFromString(@"{ ""B"": ""B"" }");

			using var listener = new HostingListener(hostBuilder =>
			{
				hostBuilder.ConfigureHostConfiguration(config => config.AddJsonStream(jsonAStream));
				hostBuilder.ConfigureAppConfiguration(config => config.AddJsonStream(jsonBStream));
			});

			var builder = MauiApp.CreateBuilder();
			using var app = builder.Build();

			Assert.AreEqual("A", app.Configuration["A"]);
			Assert.AreEqual("B", app.Configuration["B"]);

			Assert.AreSame(builder.Configuration, app.Configuration);
		}

		[Test]
		public void MauiAppDisposesConfigurationProvidersAddedInInnerBuild()
		{
			var hostConfigSource = new TrackingConfigurationSource();
			var appConfigSource = new TrackingConfigurationSource();

			using var listener = new HostingListener(hostBuilder =>
			{
				hostBuilder.ConfigureHostConfiguration(config => config.Add(hostConfigSource));
				hostBuilder.ConfigureAppConfiguration(config => config.Add(appConfigSource));
			});

			var builder = MauiApp.CreateBuilder();

			{
				using var app = builder.Build();

				Assert.AreEqual(1, hostConfigSource.ProvidersBuilt);
				Assert.AreEqual(1, appConfigSource.ProvidersBuilt);
				Assert.AreEqual(1, hostConfigSource.ProvidersLoaded);
				Assert.AreEqual(1, appConfigSource.ProvidersLoaded);
				Assert.AreEqual(0, hostConfigSource.ProvidersDisposed);
				Assert.AreEqual(0, appConfigSource.ProvidersDisposed);
			}

			Assert.AreEqual(1, hostConfigSource.ProvidersBuilt);
			Assert.AreEqual(1, appConfigSource.ProvidersBuilt);
			Assert.AreEqual(1, hostConfigSource.ProvidersLoaded);
			Assert.AreEqual(1, appConfigSource.ProvidersLoaded);
			Assert.True(hostConfigSource.ProvidersDisposed > 0);
			Assert.True(appConfigSource.ProvidersDisposed > 0);
		}

		[Test]
		public void MauiAppMakesOriginalConfigurationProvidersAddedInBuildAccessable()
		{
			using var listener = new HostingListener(hostBuilder =>
			{
				hostBuilder.ConfigureAppConfiguration(config => config.Add(new TrackingConfigurationSource()));
			});

			var builder = MauiApp.CreateBuilder();
			using var app = builder.Build();

			var wrappedProviders = ((IConfigurationRoot)app.Configuration).Providers.OfType<IEnumerable<IConfigurationProvider>>();
			var unwrappedProviders = wrappedProviders.Select(p =>
			{
				Assert.That(p, Has.One.Items);
				return p.First();
			});

			Assert.That(unwrappedProviders.OfType<TrackingConfigurationProvider>(), Has.One.Items);
		}

		public class TrackingConfigurationSource : IConfigurationSource
		{
			public int ProvidersBuilt { get; set; }
			public int ProvidersLoaded { get; set; }
			public int ProvidersDisposed { get; set; }

			public IConfigurationProvider Build(IConfigurationBuilder builder)
			{
				ProvidersBuilt++;
				return new TrackingConfigurationProvider(this);
			}
		}

		public class TrackingConfigurationProvider : ConfigurationProvider, IDisposable
		{
			private readonly TrackingConfigurationSource _source;

			public TrackingConfigurationProvider(TrackingConfigurationSource source)
			{
				_source = source;
			}

			public override void Load()
			{
				_source.ProvidersLoaded++;
			}

			public void Dispose() => _source.ProvidersDisposed++;
		}

		private sealed class HostingListener : IObserver<DiagnosticListener>, IObserver<KeyValuePair<string, object>>, IDisposable
		{
			private readonly Action<IHostBuilder> _configure;
			private static readonly AsyncLocal<HostingListener> _currentListener = new();
			private readonly IDisposable _subscription0;
			private IDisposable _subscription1;

			public HostingListener(Action<IHostBuilder> configure)
			{
				_configure = configure;

				_subscription0 = DiagnosticListener.AllListeners.Subscribe(this);

				_currentListener.Value = this;
			}

			public void OnCompleted()
			{

			}

			public void OnError(Exception error)
			{

			}

			public void OnNext(DiagnosticListener value)
			{
				if (_currentListener.Value != this)
				{
					// Ignore events that aren't for this listener
					return;
				}

				if (value.Name == "Microsoft.Extensions.Hosting")
				{
					_subscription1 = value.Subscribe(this);
				}
			}

			public void OnNext(KeyValuePair<string, object> value)
			{
				if (value.Key == "HostBuilding")
				{
					_configure?.Invoke((IHostBuilder)value.Value);
				}
			}

			public void Dispose()
			{
				// Undo this here just in case the code unwinds synchronously since that doesn't revert
				// the execution context to the original state. Only async methods do that on exit.
				_currentListener.Value = null;

				_subscription0.Dispose();
				_subscription1?.Dispose();
			}
		}
	}
}
