using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// This test verifies that VisualStateGroupList with multiple VisualState children
// does NOT trigger XC0067 warning, as this is a valid collection pattern
public partial class Maui3059VisualStates : ContentPage
{
	public Maui3059VisualStates()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
		}

		[TearDown]
		public void TearDown()
		{
			Application.SetCurrentApplication(null);
		}

		[Test]
		public void VisualStateGroupList_MultipleChildren_NoWarning()
		{
			// This test verifies that VisualStateGroupList with multiple VisualState children
			// compiles without warnings (this is valid XAML for collection types)
			var page = new Maui3059VisualStates();
			Assert.IsNotNull(page.Content);
			Assert.IsInstanceOf<Grid>(page.Content);
		}
	}
}
