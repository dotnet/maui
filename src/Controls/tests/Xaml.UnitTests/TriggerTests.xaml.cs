using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TriggerTests : ContentPage
{
	public TriggerTests() => InitializeComponent();

	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void ValueIsConverted(XamlInflator inflator)
		{
			var layout = new TriggerTests(inflator);
			Entry entry = layout.entry;
			Assert.NotNull(entry);

			var triggers = entry.Triggers;
			Assert.NotEmpty(triggers);
			var pwTrigger = triggers[0] as Trigger;
			Assert.Equal(Entry.IsPasswordProperty, pwTrigger.Property);
			Assert.Equal(true, pwTrigger.Value);
		}

		[Theory]
		[Values]
		public void ValueIsConvertedWithPropertyCondition(XamlInflator inflator)
		{
			var layout = new TriggerTests(inflator);
			Entry entry = layout.entry1;
			Assert.NotNull(entry);

			var triggers = entry.Triggers;
			Assert.NotEmpty(triggers);
			var pwTrigger = triggers[0] as MultiTrigger;
			var pwCondition = pwTrigger.Conditions[0] as PropertyCondition;
			Assert.Equal(Entry.IsPasswordProperty, pwCondition.Property);
			Assert.Equal(true, pwCondition.Value);
		}
	}
}