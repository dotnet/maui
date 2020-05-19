using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Bz43733Rd : ResourceDictionary
	{
		public Bz43733Rd()
		{
			Add("SharedText", "Foo");
		}
	}

	public partial class Bz43733 : ContentPage
	{
		public Bz43733()
		{
			InitializeComponent();
		}

		public Bz43733(bool useCompiledXaml)
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
			public void ThrowOnMissingDictionary(bool useCompiledXaml)
			{
				Application.Current = new MockApplication {
					Resources = new ResourceDictionary {
#pragma warning disable 618
						MergedWith = typeof(Bz43733Rd),
#pragma warning restore 618
					}
				};
				var p = new Bz43733(useCompiledXaml);
				Assert.AreEqual("Foo", p.label.Text);
			}
		}
	}
}
