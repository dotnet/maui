using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh2508FlashingTriggerAction : TriggerAction<VisualElement>
{
	public uint Duration { get; set; } = 1000;

	protected override void Invoke(VisualElement sender)
	{
		new Animation {
				{ 0.0, 0.5, new Animation(v => sender.Opacity = v)},
				{0.5, 1.0, new Animation(v => sender.Opacity = 1.0 - v) },
			}.Commit(sender, "FlashingAnimation", 16U, Duration, Easing.Linear, null, () => true);
	}
}

public partial class Gh2508 : ContentPage
{
	public Gh2508() => InitializeComponent();

	[TestFixture]
	class Tests
	{

		[Test]
		public void UintProperties([Values] XamlInflator inflator)
		{
			var layout = new Gh2508(inflator);
			Assert.That(((layout.entry.Triggers[0] as Trigger).EnterActions[0] as Gh2508FlashingTriggerAction).Duration, Is.EqualTo(2000));
		}
	}
}
