using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh2483Rd : ResourceDictionary
	{
	}

	public class Gh2483Custom : ResourceDictionary
	{
		public Gh2483Custom()
		{
			Add("foo", Color.Orange);
		}
	}

	public partial class Gh2483 : ContentPage
	{
		public Gh2483()
		{
			InitializeComponent();
		}

		public Gh2483(bool useCompiledXaml)
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
			public void DupeKeyRd(bool useCompiledXaml)
			{
				var layout = new Gh2483(useCompiledXaml);
				Assert.Pass();
			}
		}
	}
}
