using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[AcceptEmptyServiceProvider]
	public class Maui10583EnumValuesExtension<T> : IMarkupExtension<T[]> where T : struct, Enum
	{
		public T[] ProvideValue(IServiceProvider serviceProvider)
		{
			return Enum.GetValues<T>();
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return ProvideValue(serviceProvider);
		}
	}

	public enum Maui10583Enum
	{
		John, Paul, George, Ringo
	}

	public partial class Maui10583 : ContentPage
	{
		public Maui10583() => InitializeComponent();
		public Maui10583(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
			public void Setup()
			{
				AppInfo.SetCurrent(new MockAppInfo());
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			}

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
			public void TearDown()
			{
				DispatcherProvider.SetCurrent(null);
				AppInfo.SetCurrent(null);
			}

			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				if (true)
				{
					MockCompiler.Compile(typeof(Maui10583), out var methodDefinition);

				}
				var page = new Maui10583(useCompiledXaml);

				Assert.NotNull(page.lv.ItemsSource);
				var items = page.lv.ItemsSource as Maui10583Enum[];
				Assert.Equal(Maui10583Enum.Paul, items[1]);


			}
		}
	}
}
