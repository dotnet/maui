using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Compatibility.Hosting;
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
			Assert.True(((IConfigurationRoot)app.Configuration).GetDebugView().Contains("foo=bar (MemoryConfigurationProvider)", StringComparison.Ordinal));
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

		[TestCase(true)]
		[TestCase(false)]
		public void CompatibilityExtensionsWorkUseCompatibility(bool useCompatibility)
		{
			MauiAppBuilderExtensions.ResetCompatibilityCheck();
			bool handlersCalled = false;
			var builder = MauiApp.CreateBuilder().UseMauiApp<ApplicationStub>();

			if (useCompatibility)
				builder = builder.UseMauiCompatibility();

			var mauiApp =
				builder.ConfigureMauiHandlers(collection =>
				{
					handlersCalled = true;

					if (useCompatibility)
					{
						collection.AddCompatibilityRenderer(typeof(Object), typeof(Object));
					}
					else
					{
						Assert.Throws<InvalidOperationException>(() =>
							collection.AddCompatibilityRenderer(typeof(Object), typeof(Object))
						);
					}
				})
				.Build();

			_ = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.True(handlersCalled);
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
	}
}
