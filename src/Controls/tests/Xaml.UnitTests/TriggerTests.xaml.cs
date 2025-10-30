using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TriggerTests : ContentPage
{
	public TriggerTests() => InitializeComponent();

	class Tests
	{
		[Test]
		public void ValueIsConverted([Values] XamlInflator inflator)
		{
			var layout = new TriggerTests(inflator);
			Entry entry = layout.entry;
			Assert.NotNull(entry);

			var triggers = entry.Triggers;
			Assert.IsNotEmpty(triggers);
			var pwTrigger = triggers[0] as Trigger;
			Assert.AreEqual(Entry.IsPasswordProperty, pwTrigger.Property);
			Assert.AreEqual(true, pwTrigger.Value);
		}

		[Test]
		public void ValueIsConvertedWithPropertyCondition([Values] XamlInflator inflator)
		{
			var layout = new TriggerTests(inflator);
			Entry entry = layout.entry1;
			Assert.NotNull(entry);

			var triggers = entry.Triggers;
			Assert.IsNotEmpty(triggers);
			var pwTrigger = triggers[0] as MultiTrigger;
			var pwCondition = pwTrigger.Conditions[0] as PropertyCondition;
			Assert.AreEqual(Entry.IsPasswordProperty, pwCondition.Property);
			Assert.AreEqual(true, pwCondition.Value);
		}
	}
}