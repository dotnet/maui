using System;
using System.Collections.Generic;

using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;
using Xamarin.Forms;
namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh2549 : ContentPage
	{
		public Gh2549()
		{
			InitializeComponent();
		}

		public Gh2549(bool useCompiledXaml)
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

			[TestCase(true)]
			public void ErrorOnUnknownXmlnsForDataType(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<XamlParseException>(() => MockCompiler.Compile(typeof(Gh2549)));
			}
		}
	}


}
