using System;
using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
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
				defaultIdiom = Device.Idiom;
			}

			[TearDown]
			public void TearDown()
			{
				Device.Idiom = defaultIdiom;
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void OnIdiomXDouble(bool useCompiledXaml)
			{
				Device.Idiom = TargetIdiom.Phone;
				var layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(320));
				Device.Idiom = TargetIdiom.Tablet;
				layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(480));
			}
		}
	}
}