using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	internal class MockTriggerAction : TriggerAction<BindableObject>
	{
		public bool Invoked { get; set; }

		protected override void Invoke(BindableObject sender)
		{
			Invoked = true;
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

	[TestFixture]
	public class EventTriggerTest : BaseTestFixture
	{
		[Test]
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

		[Test]
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
	}
}