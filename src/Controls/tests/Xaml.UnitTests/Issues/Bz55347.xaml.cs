using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz55347 : ContentPage
	{
		public Bz55347()
		{
		}

		public Bz55347(bool useCompiledXaml)
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
				Application.Current = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void PaddingThicknessResource(bool useCompiledXaml)
			{
				Application.Current = new MockApplication
				{
					Resources = new ResourceDictionary {
						{"Padding", new Thickness(8)}
					}
				};
				var layout = new Bz55347(useCompiledXaml);
			}
		}
	}
}