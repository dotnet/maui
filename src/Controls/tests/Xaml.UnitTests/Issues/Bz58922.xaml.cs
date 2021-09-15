using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz58922 : ContentPage
	{
		public Bz58922()
		{
			InitializeComponent();
		}

		public Bz58922(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			TargetIdiom defaultIdiom;
			[SetUp]
			public void SetUp()
			{
				defaultIdiom = DeviceInfo.Idiom;
			}

			[TearDown]
			public void TearDown()
			{
				DeviceInfo.Idiom = defaultIdiom;
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void OnIdiomXDouble(bool useCompiledXaml)
			{
				DeviceInfo.Idiom = DeviceIdiom.Phone;
				var layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(320));
				DeviceInfo.Idiom = DeviceIdiom.Tablet;
				layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(480));
			}
		}
	}
}