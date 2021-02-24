using System;
using System.Collections.Generic;
using NUnit.Framework;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Pr3384 : ContentPage
	{
		public Pr3384()
		{
			InitializeComponent();
		}

		public Pr3384(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices { RuntimePlatform = Device.iOS };
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void RecyclingStrategyIsHandled(bool useCompiledXaml)
			{
				var p = new Pr3384(useCompiledXaml);
				Assert.AreEqual(ListViewCachingStrategy.RecycleElement, p.listView.CachingStrategy);
			}
		}
	}
}