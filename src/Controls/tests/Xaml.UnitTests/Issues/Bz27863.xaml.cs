using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz27863 : ContentPage
{
	public Bz27863()
	{
		InitializeComponent();
	}


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void DataTemplateInResourceDictionaries(XamlInflator inflator)
		{
			var layout = new Bz27863(inflator);
			var listview = layout.Resources["listview"] as ListView;
			Assert.NotNull(listview.ItemTemplate);
			var template = listview.ItemTemplate;
			var cell = template.CreateContent() as ViewCell;
			cell.BindingContext = "Foo";
			Assert.Equal("ooF", ((Label)((Compatibility.StackLayout)cell.View).Children[0]).Text);
		}
	}
}