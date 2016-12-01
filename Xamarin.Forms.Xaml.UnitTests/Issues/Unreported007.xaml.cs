using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Unreported007 : ContentPage
	{
		public Unreported007()
		{
			InitializeComponent();
		}
		public Unreported007(bool useCompiledXaml)
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
			public void ConstraintsAreEvaluatedWithOnPlatform(bool useCompiledXaml)
			{
				Device.OS = TargetPlatform.iOS;
				var page = new Unreported007(useCompiledXaml);
				Assert.That(RelativeLayout.GetXConstraint(page.label), Is.TypeOf<Constraint>());
				Assert.AreEqual(3, RelativeLayout.GetXConstraint(page.label).Compute(null));
			}
		}
	}
}
