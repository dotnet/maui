using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh6361 : ContentPage
	{
		public Gh6361() => InitializeComponent();
		public Gh6361(bool useCompiledXaml)
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
				Xamarin.Forms.Internals.Registrar.RegisterAll(new Type[0]);
			}

			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void CSSBorderRadiusDoesNotFail([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh6361(useCompiledXaml);
			}
		}
	}
}
