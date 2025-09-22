using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		}

		[TearDown]
		public void TearDown()
		{
			DispatcherProvider.SetCurrent(null);
			AppInfo.SetCurrent(null);
		}

		[Test]
		public void GenericMarkupExtensions([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui10583));
			}
			var page = new Maui10583(inflator);
			Assert.That(page.lv.ItemsSource, Is.Not.Null);
			var items = page.lv.ItemsSource as Maui10583Enum[];
			Assert.That(items[1], Is.EqualTo(Maui10583Enum.Paul));


		}
	}
}
