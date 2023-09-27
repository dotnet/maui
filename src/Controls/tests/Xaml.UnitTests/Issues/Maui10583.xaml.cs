using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Microsoft.Maui.Dispatching;
using NUnit.Framework;
using System.Collections.Generic;

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

	public enum Maui10583Enum {
		John, Paul, George, Ringo
	}

	public partial class Maui10583 : ContentPage
	{
		public Maui10583() => InitializeComponent();
		public Maui10583(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

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
			public void GenericMarkupExtensions([Values(false, true)] bool useCompiledXaml)
			{
				if (true)
				{
					MockCompiler.Compile(typeof(Maui10583), out var methodDefinition);

				}
				var page = new Maui10583(useCompiledXaml);

				Assert.That(page.lv.ItemsSource, Is.Not.Null);
				var items = page.lv.ItemsSource as Maui10583Enum[];
				Assert.That(items[1], Is.EqualTo(Maui10583Enum.Paul));
				
		
			}
		}
	}
}
