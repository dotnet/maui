using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using System.Linq;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3821 : ContentPage
{
	public Gh3821() => InitializeComponent();

	string _text;
	public string Text
	{
		get => _text;
		set
		{
			_text = value;
			OnPropertyChanged();
		}
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
		public void NoConflictsInNamescopes(XamlInflator inflator)
		{
			var layout = new Gh3821(inflator) { Text = "root" };
			var label0 = (Label)((Gh3821View)((StackLayout)layout.Content).Children[0]).Content;
			Assert.Equal("root", label0.Text);
		}
	}
}