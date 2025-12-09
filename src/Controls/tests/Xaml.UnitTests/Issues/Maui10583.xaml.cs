using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[AcceptEmptyServiceProvider]
public class Maui10583EnumValuesExtension<T> : IMarkupExtension<T[]> where T : struct, Enum
{
	public T[] ProvideValue(IServiceProvider serviceProvider) => Enum.GetValues<T>();

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}

public enum Maui10583Enum
{
	John, Paul, George, Ringo
}

public partial class Maui10583 : ContentPage
{
	public Maui10583() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		}

		public void Dispose()
		{
			DispatcherProvider.SetCurrent(null);
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void GenericMarkupExtensions(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui10583));
			}
			var page = new Maui10583(inflator);
			Assert.NotNull(page.lv.ItemsSource);
			var items = page.lv.ItemsSource as Maui10583Enum[];
			Assert.Equal(Maui10583Enum.Paul, items[1]);


		}
	}
}
