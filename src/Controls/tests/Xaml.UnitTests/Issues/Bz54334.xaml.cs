using System;
using System.Collections.Generic;
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
			MainPage = new Bz54334(useCompiledXaml);
			MessagingCenter.Subscribe<ContentPage>(this, "ChangeTheme", (s) =>
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
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Application.Current = null;
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void Foo(bool useCompiledXaml)
			{
				var app = Application.Current = new Bz54334App(useCompiledXaml);
				var page = app.MainPage as Bz54334;
				var l0 = page.label;
				var l1 = page.themedLabel;

				Assert.That(l0.TextColor, Is.EqualTo(Colors.Black));
				Assert.That(l1.TextColor, Is.EqualTo(Colors.Blue));

				MessagingCenter.Send<ContentPage>(page, "ChangeTheme");
				Assert.That(l0.TextColor, Is.EqualTo(Colors.Black));
				Assert.That(l1.TextColor, Is.EqualTo(Colors.Red));

				MessagingCenter.Send<ContentPage>(page, "ChangeTheme");
				Assert.That(l0.TextColor, Is.EqualTo(Colors.Black));
				Assert.That(l1.TextColor, Is.EqualTo(Colors.Blue));

			}
		}
	}
}