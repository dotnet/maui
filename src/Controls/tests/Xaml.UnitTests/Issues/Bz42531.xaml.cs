using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz42531 : ContentPage
	{
		public Bz42531()
		{
			InitializeComponent();
		}

		public Bz42531(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void RDInDataTemplates(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Bz42531));
				var p = new Bz42531(useCompiledXaml);
				ListView lv = p.lv;
				var template = lv.ItemTemplate;
				var cell = template.CreateContent(null, lv) as ViewCell;
				var sl = cell.View as StackLayout;
				Assert.Single(sl.Resources);
				var label = sl.Children[0] as Label;
				Assert.Equal(LayoutOptions.Center, label.HorizontalOptions);
			}
		}
	}
}