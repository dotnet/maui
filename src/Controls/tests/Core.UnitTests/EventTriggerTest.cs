using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal class MockTriggerAction : TriggerAction<BindableObject>
	{
		public bool Invoked { get; set; }

		protected override void Invoke(BindableObject sender)
		{
			Invoked = true;
		}
	}

	internal class CountingTriggerAction : TriggerAction<BindableObject>
	{
		public int InvokeCount { get; private set; }

		protected override void Invoke(BindableObject sender)
		{
			InvokeCount++;
		}
	}

	internal class MockBindableWithEvent : VisualElement
	{
		public void FireEvent()
		{
			if (MockEvent != null)
				MockEvent(this, EventArgs.Empty);
		}

		public void FireEvent2()
		{
			if (MockEvent2 != null)
				MockEvent2(this, EventArgs.Empty);
		}

		public event EventHandler MockEvent;
		public event EventHandler MockEvent2;
	}

	internal class MockViewWithEvent : View
	{
		public void FireEvent()
		{
			MockEvent?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler MockEvent;
	}


	public class EventTriggerTest : BaseTestFixture
	{
		public EventTriggerTest()
		{
			// Required so Application.Current.Resources is available — this is the
			// condition under which the double-attachment bug manifests.
			ApplicationExtensions.CreateAndSetMockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.ClearCurrent();
			}

			base.Dispose(disposing);
		}
		[Fact]
		public void TestTriggerActionInvoked()
		{
			var bindable = new MockBindableWithEvent();
			var triggeraction = new MockTriggerAction();
			var eventtrigger = new EventTrigger() { Event = "MockEvent", Actions = { triggeraction } };
			var collection = bindable.Triggers;
			collection.Add(eventtrigger);

			Assert.False(triggeraction.Invoked);
			bindable.FireEvent();
			Assert.True(triggeraction.Invoked);
		}

		[Fact]
		public void TestChangeEventOnEventTrigger()
		{
			var bindable = new MockBindableWithEvent();
			var triggeraction = new MockTriggerAction();
			var eventtrigger = new EventTrigger { Event = "MockEvent", Actions = { triggeraction } };
			var collection = bindable.Triggers;
			collection.Add(eventtrigger);

			triggeraction.Invoked = false;
			Assert.False(triggeraction.Invoked);
			bindable.FireEvent();
			Assert.True(triggeraction.Invoked);

			triggeraction.Invoked = false;
			Assert.False(triggeraction.Invoked);
			bindable.FireEvent2();
			Assert.False(triggeraction.Invoked);

			Assert.Throws<InvalidOperationException>(() => eventtrigger.Event = "MockEvent2");
		}

		// Regression test for https://github.com/dotnet/maui/issues/24152
		// EventTrigger in a global (implicit) style should fire exactly once per event,
		// not twice due to double-application in MergedStyle constructor.
		[Fact]
		public void EventTriggerInImplicitStyleFiresOnce()
		{
			var triggerAction = new CountingTriggerAction();
			var implicitStyle = new Style(typeof(MockViewWithEvent))
			{
				Triggers = { new EventTrigger { Event = "MockEvent", Actions = { triggerAction } } }
			};

			// The bug only manifests when the implicit style is in Application.Current.Resources
			// (e.g., defined in App.xaml / Styles.xaml). The TryGetResource fallback resolves
			// the style synchronously during MergedStyle.RegisterImplicitStyles(), causing Apply()
			// to be called once there AND again in the MergedStyle constructor — registering the
			// event handler twice.
			Application.Current.Resources.Add(implicitStyle);

			var view = new MockViewWithEvent();
			view.FireEvent();

			Assert.Equal(1, triggerAction.InvokeCount);
		}
	}
}