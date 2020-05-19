using System;
using System.Collections.Generic;

using NUnit.Framework;

using System.Maui;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public partial class DynamicResource : ContentPage
	{
		public DynamicResource ()
		{
			InitializeComponent ();
		}

		public DynamicResource (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
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

			[TestCase (false), TestCase (true)]
			public void TestDynamicResources (bool useCompiledXaml)
			{
				var layout = new DynamicResource (useCompiledXaml);
				var label = layout.label0;

				Assert.Null (label.Text);

				layout.Resources = new ResourceDictionary { 
					{"FooBar", "FOOBAR"},
				};
				Assert.AreEqual ("FOOBAR", label.Text);
			}
		}
	}
}