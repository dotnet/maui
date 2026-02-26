using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Issue")]
public partial class Maui23989
{
	public Maui23989() => InitializeComponent();

	public class Test : BaseTestFixture
	{
		protected internal override void Setup()
		{
			base.Setup();
			Application.SetCurrentApplication(new MockApplication());
		}

		[Theory]
		[XamlInflatorData]
		internal void ItemDisplayBindingWithoutDataTypeFails(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				XamlExceptionAssert.ThrowsBuildException(12, 13, s => s.Contains("0022", StringComparison.Ordinal), () => MockCompiler.Compile(typeof(Maui23989), treatWarningsAsErrors: true));

			var layout = new Maui23989(inflator);
			//without x:DataType, bindings aren't compiled
			Assert.IsType<Binding>(layout.picker0.ItemDisplayBinding);
			if (inflator == XamlInflator.XamlC || inflator == XamlInflator.SourceGen)
				Assert.IsType<TypedBinding<MockItemViewModel, string>>(layout.picker1.ItemDisplayBinding);
			else
				Assert.IsType<Binding>(layout.picker1.ItemDisplayBinding);

			layout.BindingContext = new MockViewModel
			{
				Items = [
					new() { Title = "item1" },
					new() { Title = "item2" },
					new() { Title = "item3" },
				]
			};

			Assert.Equal("item1", layout.picker0.Items[0]);
			Assert.Equal("item1", layout.picker1.Items[0]);
		}
	}
}