using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue3059b : ContentPage
{
	public Issue3059b()
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
		public void PropertiesSetMultipleTimes_OnlyLastValueIsUsed([Values] XamlInflator inflator)
		{
			// This test verifies that when any property is set multiple times,
			// only the last value is used (not just Content property)
			var page = new Issue3059b(inflator);
			
			Assert.IsNotNull(page.Content);
			var stack = (VerticalStackLayout)page.Content;
			
			// Label.Text set twice - should use "Second"
			var label = (Label)stack.Children[0];
			Assert.AreEqual("Second", label.Text);
			
			// Entry.Placeholder set twice - should use "Second"
			var entry = (Entry)stack.Children[1];
			Assert.AreEqual("Second", entry.Placeholder);
		}
	}
}
