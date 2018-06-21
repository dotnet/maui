using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;


using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh3082 : ContentPage
	{
		public Gh3082()
		{
			InitializeComponent();
		}

		public Gh3082(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		async Task OnClicked(object sender, EventArgs e)
		{
			await Task.Delay(1000);
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

			[TestCase(false), TestCase(true)]
			public void ThrowsOnWrongEventHandlerSignature(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<XamlParseException>(()=>MockCompiler.Compile(typeof(Gh3082)));
				else
					Assert.Throws<XamlParseException>(()=>new Gh3082(false));
			}
		}
	}
}
