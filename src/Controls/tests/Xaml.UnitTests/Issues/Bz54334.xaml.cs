using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

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
		}

		[TestFixture]
		class Tests
		{
			[TearDown]
			public void TearDown()
			{
				Application.Current = null;
			}

			[TestCase(true)]
			[TestCase(false, Ignore = "This is failing on CI on macOS: https://github.com/dotnet/maui/issues/15054")]
			public void FooBz54334(bool useCompiledXaml)
			{
				var app = Application.Current = new Bz54334App(useCompiledXaml);
				var page = app.MainPage as Bz54334;
				var l0 = page.label;
				var l1 = page.themedLabel;

				Assert.That(l0.TextColor, Is.EqualTo(Colors.Black));
				Assert.That(l1.TextColor, Is.EqualTo(Colors.Blue));

				WeakReferenceMessenger.Default.Send<ContentPage, string>(page, "ChangeTheme");
				Assert.That(l0.TextColor, Is.EqualTo(Colors.Black));
				Assert.That(l1.TextColor, Is.EqualTo(Colors.Red));

				WeakReferenceMessenger.Default.Send<ContentPage, string>(page, "ChangeTheme");
				Assert.That(l0.TextColor, Is.EqualTo(Colors.Black));
				Assert.That(l1.TextColor, Is.EqualTo(Colors.Blue));

			}
		}
	}
}