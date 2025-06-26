using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23989
{
	public Maui23989()
	{
		InitializeComponent();
	}

	public Maui23989(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Test
	{
		// Constructor
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		public void ItemDisplayBindingWithoutDataTypeFails([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.Throws(new BuildExceptionConstraint(12, 13, s => s.Contains("0022", StringComparison.Ordinal)), () => MockCompiler.Compile(typeof(Maui23989), treatWarningsAsErrors: true));

			var layout = new Maui23989(useCompiledXaml);
			//without x:DataType, bindings aren't compiled
			Assert.That(layout.picker0.ItemDisplayBinding, Is.TypeOf<Binding>());
			if (useCompiledXaml)
				Assert.That(layout.picker1.ItemDisplayBinding, Is.TypeOf<TypedBinding<MockItemViewModel, string>>());
			else
				Assert.That(layout.picker1.ItemDisplayBinding, Is.TypeOf<Binding>());

			layout.BindingContext = new MockViewModel
			{
				Items = new List<MockItemViewModel> {
					new MockItemViewModel { Title = "item1" },
					new MockItemViewModel { Title = "item2" },
					new MockItemViewModel { Title = "item3" },
				}.ToArray()
			};

			Assert.Equal("item1", layout.picker0.Items[0]);
			Assert.Equal("item1", layout.picker1.Items[0]);

		}
	}
}
