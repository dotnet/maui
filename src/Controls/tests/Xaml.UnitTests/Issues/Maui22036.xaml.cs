using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22036
{
	public Maui22036() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public async Task StyleWithTriggerLeak([Values] XamlInflator inflator)
		{
			var style = new Style(typeof(ContentPage));
			var trigger = new EventTrigger { Event = nameof(Appearing) };
			trigger.Actions.Add(new EmptyTriggerAction());
			style.Triggers.Add(trigger);

			Application.Current.Resources.Add(style);

			var pagewr = new WeakReference(new Maui22036(inflator));
			await Task.Delay(10);
			GC.Collect();
			Assert.IsNull(pagewr.Target, "Page leaked");
		}
	}

	class EmptyTriggerAction : TriggerAction<ContentPage>
	{
		protected override void Invoke(ContentPage sender)
		{
		}
	}
}
