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
			var host = new AppHostBuilder()
				.ConfigureAppConfiguration((_, builder) =>
				{
					builder.AddInMemoryCollection(new Dictionary<string, string>
					{
						{ "key 1", "value 1" },
					});
				})
				.Build();

			var configuration = host.Services.GetRequiredService<IConfiguration>();

			Assert.Equal("value 1", configuration["key 1"]);
		}

		[Fact]
		public void ConfigureAppConfigurationOverwritesValues()
		{
			var host = new AppHostBuilder()
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
				})
				.Build();

			var configuration = host.Services.GetRequiredService<IConfiguration>();

			Assert.Equal("value a", configuration["key 1"]);
			Assert.Equal("value 2", configuration["key 2"]);
		}

		[Fact]
		public void ConfigureServicesCanUseConfig()
		{
			string value = null;

			var host = new AppHostBuilder()
				.ConfigureAppConfiguration((_, builder) =>
				{
					builder.AddInMemoryCollection(new Dictionary<string, string>
					{
						{ "key 1", "value 1" },
					});
				})
				.ConfigureServices((context, services) =>
				{
					value = context.Configuration["key 1"];
				})
				.Build();

			Assert.Equal("value 1", value);
		}
	}
}