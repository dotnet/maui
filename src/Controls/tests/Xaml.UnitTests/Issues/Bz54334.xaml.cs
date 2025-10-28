using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Bz54334App : Application
	{
		bool daymode = true;
		public Bz54334App(bool useCompiledXaml)
		{
			Resources = new ResourceDictionary{
				new Style(typeof(Label)) {
					Setters = {
						new Setter {Property = Label.TextColorProperty, Value=Colors.Blue}
					}
				}
			};
			this.LoadPage(new Bz54334(useCompiledXaml));
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
		public Bz54334(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
			public void TearDown()
			{
				Application.Current = null;
			}

			[InlineData(true)]
			[InlineData(false, Ignore = "This is failing on CI on macOS: https://github.com/dotnet/maui/issues/15054")]
			public void FooBz54334(bool useCompiledXaml)
			{
				var app = Application.Current = new Bz54334App(useCompiledXaml);
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
}