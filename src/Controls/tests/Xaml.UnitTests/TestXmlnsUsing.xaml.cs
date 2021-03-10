using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class TestXmlnsUsing : ContentPage
	{
		public TestXmlnsUsing()
		{
			InitializeComponent();
		}

		public TestXmlnsUsing(bool useCompiledXaml)
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
				Application.Current = null;
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void SupportUsingXmlns(bool useCompiledXaml)
			{
				var page = new TestXmlnsUsing(useCompiledXaml);
				Assert.That(page.Content, Is.Not.Null);
				Assert.That(page.Content, Is.TypeOf<CustomXamlView>());
			}
		}
	}
}
