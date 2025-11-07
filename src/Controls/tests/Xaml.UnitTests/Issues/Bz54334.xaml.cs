using System;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz54334App : Application
{
	bool daymode = true;
	internal Bz54334App(XamlInflator inflator)
	{
		Resources = new ResourceDictionary{
			new Style(typeof(Label)) {
				Setters = {
					new Setter {Property = Label.TextColorProperty, Value=Colors.Blue}
				}
			}
		};
		this.LoadPage(new Bz54334(inflator));
		WeakReferenceMessenger.Default.Register<ContentPage, string>(this, "ChangeTheme", (s, m) =>
		{
			ToggleTheme();
		});
	}

	void ToggleTheme()
	{
		Resources = daymode ? new ResourceDictionary{
			new Style(typeof(Label)) {
				Setters = {
					new Setter {Property = Label.TextColorProperty, Value=Colors.Red}
				}
			}
		} : new ResourceDictionary{
			new Style(typeof(Label)) {
				Setters = {
					new Setter {Property = Label.TextColorProperty, Value=Colors.Blue}
				}
			}
		};
		daymode = !daymode;
	}
}

public partial class Bz54334 : ContentPage
{
	public Bz54334()
	{
		InitializeComponent();
	}


	public class Tests : IDisposable
	{

		public void Dispose()
		{
			Application.Current = null;
		}

		[Fact]
		//		[InlineData(false, Ignore = "This is failing on CI on macOS: https://github.com/dotnet/maui/issues/15054")]
		public void FooBz54334(XamlInflator inflator)
		{
			var app = Application.Current = new Bz54334App(inflator);
			var page = app.MainPage as Bz54334;
			var l0 = page.label;
			var l1 = page.themedLabel;

			Assert.Equal(Colors.Black, l0.TextColor);
			Assert.Equal(Colors.Blue, l1.TextColor);

			WeakReferenceMessenger.Default.Send<ContentPage, string>(page, "ChangeTheme");
			Assert.Equal(Colors.Black, l0.TextColor);
			Assert.Equal(Colors.Red, l1.TextColor);

			WeakReferenceMessenger.Default.Send<ContentPage, string>(page, "ChangeTheme");
			Assert.Equal(Colors.Black, l0.TextColor);
			Assert.Equal(Colors.Blue, l1.TextColor);

		}
	}
}