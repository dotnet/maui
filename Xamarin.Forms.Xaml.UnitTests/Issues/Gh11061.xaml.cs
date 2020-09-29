using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Build.Tasks;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh11061 : ContentPage
	{
		public DateTime MyDateTime { get; set; }

		public Gh11061() => InitializeComponent();
		public Gh11061(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void XamlCBindingOnNonBP([Values(false, true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh11061)));
				else
					Assert.Throws<XamlParseException>(() => new Gh11061(useCompiledXaml) { BindingContext = new { Date = DateTime.Today } });
			}
		}
	}
}
