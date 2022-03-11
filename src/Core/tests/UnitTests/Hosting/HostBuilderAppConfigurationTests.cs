using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.UnitTests.Hosting
{
	[Category(TestCategory.Core, TestCategory.Hosting)]
	public class HostBuilderAppConfigurationTests
	{
		[Fact]
		public void CanConfigureAppConfiguration()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.Configuration
				.AddInMemoryCollection(new Dictionary<string, string>
				{
					{ "key 1", "value 1" },
				});
			var mauiApp = builder.Build();

			var configuration = mauiApp.Services.GetRequiredService<IConfiguration>();

			Assert.Equal("value 1", configuration["key 1"]);
		}

		[Fact]
		public void AppConfigurationOverwritesValues()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.Configuration
				.AddInMemoryCollection(new Dictionary<string, string>
				{
					{ "key 1", "value 1" },
					{ "key 2", "value 2" },
				});

			builder
				.Configuration
				.AddInMemoryCollection(new Dictionary<string, string>
				{
					{ "key 1", "value a" },
				});

			var mauiApp = builder.Build();

			var configuration = mauiApp.Services.GetRequiredService<IConfiguration>();

			Assert.Equal("value a", configuration["key 1"]);
			Assert.Equal("value 2", configuration["key 2"]);
		}

		[Fact]
		public void ConfigureServicesCanUseConfig()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.Configuration
				.AddInMemoryCollection(new Dictionary<string, string>
				{
					{ "key 1", "value 1" },
				});

			Assert.Equal("value 1", builder.Configuration["key 1"]);
		}
	}
}