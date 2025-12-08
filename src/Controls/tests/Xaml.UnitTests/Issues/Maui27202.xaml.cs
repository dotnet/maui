using System;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui27202 : ContentPage
{
	public Maui27202() => InitializeComponent();

	public Maui27202(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.Current = new MockApplication();
		}

		[TearDown]
		public void TearDown()
		{
			Application.Current = null;
		}

		[Test]
		public void DerivedStylesInheritVisualStateManager([Values] XamlInflator inflator)
		{
			var page = new Maui27202(inflator);

			// Verify styles are applied
			Assert.That(page.EnabledLabel1.TextColor, Is.EqualTo(Colors.Green));
			
			// Verify VSG exists
			var groups = VisualStateManager.GetVisualStateGroups(page.DisabledLabel1);
			Assert.That(groups, Is.Not.Null);
			Assert.That(groups.Count, Is.GreaterThan(0));
			
			// Check if GoToState succeeds
			var gotoResult = VisualStateManager.GoToState(page.DisabledLabel1, "Disabled");
			
			// Output for debugging
			Console.WriteLine($"GoToState result: {gotoResult}");
			Console.WriteLine($"TextColor after GoToState: {page.DisabledLabel1.TextColor}");
			Console.WriteLine($"Expected: Gray ({Colors.Gray})");
			
			Assert.That(gotoResult, Is.True, "GoToState should succeed");
			Assert.That(page.DisabledLabel1.TextColor, Is.EqualTo(Colors.Gray),
				"VSM Disabled state should override derived style TextColor");
		}
	}
}

// Custom label controls for testing derived styles
public class CustomLabel1 : Label
{
}

public class CustomLabel2 : Label
{
}
