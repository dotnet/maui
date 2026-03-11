using System;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// A Button subclass that shadows the base Clicked event with a new one.
/// This tests that EventTrigger source generation correctly picks the
/// most-derived event (matching runtime GetRuntimeEvent behavior).
/// </summary>
public class Maui33591DerivedButton : Button
{
	// Shadow the base Button.Clicked event with a new event
	public new event EventHandler Clicked;

	public int DerivedClickedCount { get; private set; }

	public void FireDerivedClicked()
	{
		DerivedClickedCount++;
		Clicked?.Invoke(this, EventArgs.Empty);
	}
}

public partial class Maui33591_ShadowedEvent : ContentPage
{
	public Maui33591_ShadowedEvent()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		/// <summary>
		/// Verifies that EventTrigger on a derived type with a shadowed event
		/// correctly subscribes to the derived event (not the base event).
		/// All inflators should behave the same way.
		/// </summary>
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void EventTriggerUsesCorrectShadowedEvent(XamlInflator inflator)
		{
			var page = new Maui33591_ShadowedEvent(inflator);

			var trigger = page.TestDerivedButton!.Triggers.OfType<EventTrigger>().Single();
			var action = (Maui33591TestTriggerAction)trigger.Actions[0];

			Assert.Equal("Clicked", trigger.Event);
			Assert.Equal(0, action.InvokeCount);

			// Fire the derived Clicked event (not the base Button.Clicked)
			page.TestDerivedButton.FireDerivedClicked();

			Assert.Equal(1, action.InvokeCount);
		}
	}
}
