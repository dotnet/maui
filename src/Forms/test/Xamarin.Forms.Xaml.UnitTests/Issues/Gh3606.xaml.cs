using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Gh3606 : ContentPage
	{
		public Gh3606()
		{
			InitializeComponent();
		}

		public Gh3606(bool useCompiledXaml)
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
			public void BindingsWithSourceArentCompiled(bool useCompiledXaml)
			{
				new Gh3606(useCompiledXaml);
			}
		}
	}
}
