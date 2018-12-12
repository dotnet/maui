using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh4553 : ContentPage
	{
		public Gh4553()
		{
			InitializeComponent();
		}
		public Gh4553(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[TestCase(true), TestCase(false)]
			public void ThrowMeaningfulError(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<XamlParseException>(() => MockCompiler.Compile(typeof(Gh4553)));
				else
					Assert.Throws<XamlParseException>(() => new Gh4553(useCompiledXaml));
			}
		}
	}
}
