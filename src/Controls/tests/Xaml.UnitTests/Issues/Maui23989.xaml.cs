using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23989
{
	public Maui23989() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void ItemDisplayBindingWithoutDataTypeFails([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(12, 13, s => s.Contains("0022", StringComparison.Ordinal)), () => MockCompiler.Compile(typeof(Maui23989), treatWarningsAsErrors: true));

			var layout = new Maui23989(inflator);
			//without x:DataType, bindings aren't compiled
			Assert.That(layout.picker0.ItemDisplayBinding, Is.TypeOf<Binding>());
			if (inflator == XamlInflator.XamlC || inflator == XamlInflator.SourceGen)
				Assert.That(layout.picker1.ItemDisplayBinding, Is.TypeOf<TypedBinding<MockItemViewModel, string>>());
			else
				Assert.That(layout.picker1.ItemDisplayBinding, Is.TypeOf<Binding>());

			layout.BindingContext = new MockViewModel
			{
				Items = [
					new() { Title = "item1" },
					new() { Title = "item2" },
					new() { Title = "item3" },
				]
			};

			Assert.That(layout.picker0.Items[0], Is.EqualTo("item1"));
			Assert.That(layout.picker1.Items[0], Is.EqualTo("item1"));
		}
	}
}