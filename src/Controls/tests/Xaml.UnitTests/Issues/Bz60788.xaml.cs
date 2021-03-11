using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz60788 : ContentPage
	{
		public Bz60788()
		{
			InitializeComponent();
		}

		public Bz60788(bool useCompiledXaml)
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
			public void KeyedRDWithImplicitStyles(bool useCompiledXaml)
			{
				var layout = new Bz60788(useCompiledXaml);
				Assert.That(layout.Resources.Count, Is.EqualTo(2));
				Assert.That(((ResourceDictionary)layout.Resources["RedTextBlueBackground"]).Count, Is.EqualTo(3));
				Assert.That(((ResourceDictionary)layout.Resources["BlueTextRedBackground"]).Count, Is.EqualTo(3));
			}
		}
	}
}