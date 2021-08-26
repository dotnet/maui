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
		public void ConfigureAppConfigurationConfiguresValues()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Host
				.ConfigureAppConfiguration((_, builder) =>
				{
					builder.AddInMemoryCollection(new Dictionary<string, string>
					{
						{ "key 1", "value 1" },
					});
				});
			var mauiApp = builder.Build();

			var configuration = mauiApp.Services.GetRequiredService<IConfiguration>();

			Assert.Equal("value 1", configuration["key 1"]);
		}

		[Fact]
		public void ConfigureAppConfigurationOverwritesValues()
		{
			var builder = MauiApp.CreateBuilder();
			builder.Host
				.ConfigureAppConfiguration((_, builder) =>
				{
					builder.AddInMemoryCollection(new Dictionary<string, string>
					{
						{ "key 1", "value 1" },
						{ "key 2", "value 2" },
					});
				})
				.ConfigureAppConfiguration((_, builder) =>
				{
					builder.AddInMemoryCollection(new Dictionary<string, string>
					{
						{ "key 1", "value a" },
					});
				});
			var mauiApp = builder.Build();

			var configuration = mauiApp.Services.GetRequiredService<IConfiguration>();

			Assert.Equal("value a", configuration["key 1"]);
			Assert.Equal("value 2", configuration["key 2"]);
		}

		[Fact]
		public void ConfigureServicesCanUseConfig()
		{
			string value = null;

			var builder = MauiApp.CreateBuilder();
			builder.Host
				.ConfigureAppConfiguration((_, builder) =>
				{
					builder.AddInMemoryCollection(new Dictionary<string, string>
					{
						{ "key 1", "value 1" },
					});
				});

			builder.Host
				.ConfigureServices((context, services) =>
				{
					value = context.Configuration["key 1"];
				});
			var mauiApp = builder.Build();

			Assert.Equal("value 1", value);
		}
	}
}