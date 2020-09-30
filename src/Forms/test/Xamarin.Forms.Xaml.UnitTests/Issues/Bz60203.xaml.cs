using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Bz60203 : ContentPage
	{
		public Bz60203()
		{
			InitializeComponent();
		}

		public Bz60203(bool useCompiledXaml)
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
				Device.PlatformServices = null;
			}

			[TestCase(true), TestCase(false)]
			public void CanCompileMultiTriggersWithDifferentConditions(bool useCompiledXaml)
			{
				var layout = new Bz60203(useCompiledXaml);
				Assert.That(layout.label.BackgroundColor, Is.EqualTo(BackgroundColorProperty.DefaultValue));
				layout.BindingContext = new { Text = "Foo" };
				layout.label.TextColor = Color.Blue;
				Assert.That(layout.label.BackgroundColor, Is.EqualTo(Color.Pink));
			}

		}
	}
}