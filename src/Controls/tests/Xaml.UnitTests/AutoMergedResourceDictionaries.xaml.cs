using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class AutoMergedResourceDictionaries : ContentPage
{
	public AutoMergedResourceDictionaries()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => Application.Current = new MockApplication();
		[TearDown] public void TearDown() => Application.Current = null;

		[Test]
		public void AutoMergedRd([Values] XamlInflator inflator)
		{
			var layout = new AutoMergedResourceDictionaries(inflator);
			Assert.That(layout.label.TextColor, Is.EqualTo(Colors.Purple));
			Assert.That(layout.label.BackgroundColor, Is.EqualTo(Color.FromArgb("#FF96F3")));
		}
	}
}
