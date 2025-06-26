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
		class Tests
		{
			// Constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// IDisposable public void TearDown() => DispatcherProvider.SetCurrent(null);

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
				Assert.Equal(1, sl.Resources.Count);
				var label = sl.Children[0] as Label;
				Assert.Equal(LayoutOptions.Center, label.HorizontalOptions);
			}
		}
	}
}