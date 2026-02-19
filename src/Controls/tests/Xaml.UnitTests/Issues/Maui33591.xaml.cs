using System.Linq;
using Xunit;

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

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void EventTriggerHasEventPropertySet(XamlInflator inflator)
		{
			var page = new Maui33591(inflator);

			var trigger = page.TestButton!.Triggers.OfType<EventTrigger>().Single();
			Assert.Equal("Clicked", trigger.Event);
			Assert.Single(trigger.Actions);
		}

		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void EventTriggerFiresWhenEventOccurs(XamlInflator inflator)
		{
			var page = new Maui33591(inflator);

			var trigger = page.TestButton!.Triggers.OfType<EventTrigger>().Single();
			var action = (Maui33591TestTriggerAction)trigger.Actions[0];

			Assert.Equal(0, action.InvokeCount);

			page.TestButton.SendClicked();

			Assert.Equal(1, action.InvokeCount);

			page.TestButton.SendClicked();

			Assert.Equal(2, action.InvokeCount);
		}
	}
}
