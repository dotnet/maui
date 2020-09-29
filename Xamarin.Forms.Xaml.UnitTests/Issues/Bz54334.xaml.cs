using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Bz54334App : Application
	{
		bool daymode = true;
		public Bz54334App(bool useCompiledXaml)
		{
			Resources = new ResourceDictionary{
				new Style(typeof(Label)) {
					Setters = {
						new Setter {Property = Label.TextColorProperty, Value=Color.Blue}
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
						new Setter {Property = Label.TextColorProperty, Value=Color.Red}
					}
				}
			} : new ResourceDictionary{
				new Style(typeof(Label)) {
					Setters = {
						new Setter {Property = Label.TextColorProperty, Value=Color.Blue}
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

				Assert.That(l0.TextColor, Is.EqualTo(Color.Black));
				Assert.That(l1.TextColor, Is.EqualTo(Color.Blue));

				MessagingCenter.Send<ContentPage>(page, "ChangeTheme");
				Assert.That(l0.TextColor, Is.EqualTo(Color.Black));
				Assert.That(l1.TextColor, Is.EqualTo(Color.Red));

				MessagingCenter.Send<ContentPage>(page, "ChangeTheme");
				Assert.That(l0.TextColor, Is.EqualTo(Color.Black));
				Assert.That(l1.TextColor, Is.EqualTo(Color.Blue));

			}
		}
	}
}