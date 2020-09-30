using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Issue2016 : ContentPage
	{
		public Issue2016()
		{
			InitializeComponent();
		}

		public Issue2016(bool useCompiledXaml)
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

			[TestCase(false)]
			[TestCase(true)]
			public void TestSwitches(bool useCompiledXaml)
			{
				var page = new Issue2016(useCompiledXaml);
				Assert.AreEqual(false, page.a0.IsToggled);
				Assert.AreEqual(false, page.b0.IsToggled);
				Assert.AreEqual(false, page.s0.IsToggled);
				Assert.AreEqual(false, page.t0.IsToggled);

				page.a0.IsToggled = true;
				page.b0.IsToggled = true;

				Assert.AreEqual(true, page.s0.IsToggled);
				Assert.AreEqual(true, page.t0.IsToggled);
			}
		}
	}
}