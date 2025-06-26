using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz27863 : ContentPage
	{
		public Bz27863()
		{
			InitializeComponent();
		}

		public Bz27863(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Tests
		{
			// Constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			// IDisposable public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			[InlineData(false)]
			public void DataTemplateInResourceDictionaries(bool useCompiledXaml)
			{
				var layout = new Bz27863(useCompiledXaml);
				var listview = layout.Resources["listview"] as ListView;
				Assert.NotNull(listview.ItemTemplate);
				var template = listview.ItemTemplate;
				var cell = template.CreateContent() as ViewCell;
				cell.BindingContext = "Foo";
				Assert.Equal("ooF", ((Label)((Compatibility.StackLayout)cell.View).Children[0]).Text);
			}
		}
	}
}