using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Compatibility.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.Compatibility.Core.UnitTests
{
	public class HostBuilderAppTests
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void CompatibilityExtensionsWorkUseCompatibility(bool useCompatibility)
		{
			MauiAppBuilderExtensions.ResetCompatibilityCheck();
			bool handlersCalled = false;
			var builder = MauiApp.CreateBuilder().UseMauiApp<ApplicationStub>();

			if (useCompatibility)
#pragma warning disable 0618
				builder = builder.UseMauiCompatibility();
#pragma warning restore 0618

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

		[Fact]
		public void RegisteringHandlerWithAddCompatibilityRendererThrowsException()
		{
			MauiAppBuilderExtensions.ResetCompatibilityCheck();
			bool handlersCalled = false;
			var builder = MauiApp.CreateBuilder().UseMauiApp<ApplicationStub>();

			var mauiApp =
				builder.ConfigureMauiHandlers(collection =>
				{
					handlersCalled = true;

					Assert.Throws<InvalidOperationException>(() =>
							collection.AddCompatibilityRenderer(typeof(ButtonHandlerStub), typeof(Object))
						);
				})
				.Build();

			_ = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.True(handlersCalled);
		}
	}
}
