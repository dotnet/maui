using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Simple test TriggerAction that tracks invocation count.
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
		[Test]
		public void EventTriggerHasEventPropertySet([Values] XamlInflator inflator)
		{
			var page = new Maui33591(inflator);

			var trigger = page.TestButton!.Triggers.OfType<EventTrigger>().Single();
			Assert.That(trigger.Event, Is.EqualTo("Clicked"));
			Assert.That(trigger.Actions.Count, Is.EqualTo(1));
		}

		[Test]
		public void EventTriggerFiresWhenEventOccurs([Values] XamlInflator inflator)
		{
			var page = new Maui33591(inflator);

			var trigger = page.TestButton!.Triggers.OfType<EventTrigger>().Single();
			var action = (Maui33591TestTriggerAction)trigger.Actions[0];

			Assert.That(action.InvokeCount, Is.EqualTo(0), "Action should not be invoked yet");

			page.TestButton.SendClicked();

			Assert.That(action.InvokeCount, Is.EqualTo(1), "Action should be invoked once after click");

			page.TestButton.SendClicked();

			Assert.That(action.InvokeCount, Is.EqualTo(2), "Action should be invoked twice after second click");
		}
	}
}
