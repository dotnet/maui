using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Simple test TriggerAction for use in Maui33591 tests.
/// </summary>
public class Maui33591TestTriggerAction : TriggerAction<VisualElement>
{
	public int InvokeCount { get; private set; }

	protected override void Invoke(VisualElement sender)
	{
		InvokeCount++;
	}
}

public partial class Maui33591 : ContentPage
{
	public Maui33591()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		/// <summary>
		/// Test that XAML page creates elements correctly across all inflators.
		/// </summary>
		[Test]
		public void PageCreatesSuccessfully([Values] XamlInflator inflator)
		{
			var page = new Maui33591(inflator);
			Assert.That(page, Is.Not.Null);
			Assert.That(page.Title, Is.EqualTo("EventTrigger AOT-Safe Test"));
		}

		/// <summary>
		/// Test that all named controls are created.
		/// </summary>
		[Test]
		public void ControlsAreCreated([Values] XamlInflator inflator)
		{
			var page = new Maui33591(inflator);
			
			Assert.That(page.ClickButton, Is.Not.Null);
			Assert.That(page.ClickButton!.Text, Is.EqualTo("Click Me"));
			Assert.That(page.ClickButton.BackgroundColor, Is.EqualTo(Colors.Blue));

			Assert.That(page.TextEntry, Is.Not.Null);
			Assert.That(page.TextEntry!.Placeholder, Is.EqualTo("Type something"));

			Assert.That(page.TestSwitch, Is.Not.Null);
			Assert.That(page.TestSwitch!.IsToggled, Is.False);

			Assert.That(page.StatusLabel, Is.Not.Null);
			Assert.That(page.StatusLabel!.Text, Is.EqualTo("Ready"));
		}

		/// <summary>
		/// Test that EventTriggers are created with correct Event property for all inflators.
		/// This validates backward compatibility - the Event property should be set regardless
		/// of whether the inflator uses reflection or AOT-safe factory methods.
		/// </summary>
		[Test]
		public void EventTriggersHaveEventPropertySet([Values] XamlInflator inflator)
		{
			var page = new Maui33591(inflator);

			// Button with Clicked EventTrigger
			var buttonTrigger = page.ClickButton!.Triggers.OfType<EventTrigger>().Single();
			Assert.That(buttonTrigger.Event, Is.EqualTo("Clicked"), "Button EventTrigger.Event should be 'Clicked'");
			Assert.That(buttonTrigger.Actions.Count, Is.EqualTo(1), "Button EventTrigger should have 1 action");

			// Entry with TextChanged EventTrigger
			var entryTrigger = page.TextEntry!.Triggers.OfType<EventTrigger>().Single();
			Assert.That(entryTrigger.Event, Is.EqualTo("TextChanged"), "Entry EventTrigger.Event should be 'TextChanged'");
			Assert.That(entryTrigger.Actions.Count, Is.EqualTo(1), "Entry EventTrigger should have 1 action");

			// Switch with Toggled EventTrigger
			var switchTrigger = page.TestSwitch!.Triggers.OfType<EventTrigger>().Single();
			Assert.That(switchTrigger.Event, Is.EqualTo("Toggled"), "Switch EventTrigger.Event should be 'Toggled'");
			Assert.That(switchTrigger.Actions.Count, Is.EqualTo(1), "Switch EventTrigger should have 1 action");
		}
	}
}
