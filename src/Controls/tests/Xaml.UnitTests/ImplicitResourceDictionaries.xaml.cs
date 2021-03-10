using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class ImplicitResourceDictionaries : ContentPage
	{
		public ImplicitResourceDictionaries()
		{
			InitializeComponent();
		}

		public ImplicitResourceDictionaries(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ImplicitRDonContentViews(bool useCompiledXaml)
			{
				var layout = new ImplicitResourceDictionaries(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.Purple));
			}
		}
	}
}
