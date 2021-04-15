using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
				layout.label.TextColor = Colors.Blue;
				Assert.That(layout.label.BackgroundColor, Is.EqualTo(Colors.Pink));
			}

		}
	}
}