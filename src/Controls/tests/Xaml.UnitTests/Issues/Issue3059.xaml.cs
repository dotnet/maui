using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue3059 : ContentPage
{
	public Issue3059()
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
		public void BorderWithMultipleChildren_OnlyLastChildIsUsed([Values] XamlInflator inflator)
		{
			// This test verifies the behavior that only the last child is actually used
			// when multiple children are specified in a single-child content property
			var page = new Issue3059(inflator);
			
			Assert.IsNotNull(page.Content);
			Assert.IsInstanceOf<Microsoft.Maui.Controls.Border>(page.Content);
			
			var border = (Microsoft.Maui.Controls.Border)page.Content;
			Assert.IsNotNull(border.Content);
			Assert.IsInstanceOf<Label>(border.Content);
			
			var label = (Label)border.Content;
			// Only the last child ("Second") should be set
			Assert.AreEqual("Second", label.Text);
		}
	}
}
