using System;
using System.Collections.Generic;

using System.Maui;

using NUnit.Framework;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public class Bz27968Page : ContentPage
	{
	}

	public partial class Bz27968 : Bz27968Page
	{
		public Bz27968 ()
		{
			InitializeComponent ();
		}

		public Bz27968 (bool useCompiledXaml)
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
			[TestCase(false)]
			public void BaseClassIdentifiersAreValidForResources (bool useCompiledXaml)
			{
				var layout = new Bz27968 (useCompiledXaml);
				Assert.That (layout.Resources ["listView"], Is.TypeOf<ListView> ());
			}
		}
	}
}
