using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Triggers
{
	/// <summary>
	/// Unit tests for EventTrigger and EventTrigger.Create factory methods.
	/// Validates core functionality, lifecycle management, and AOT-safe behavior.
	/// </summary>
	public class EventTriggerTests
	{
#pragma warning disable CS0618 // Type or member is obsolete (we're testing reflection-based EventTrigger)
		/// <summary>
		/// Validates that EventTrigger can be instantiated and initialized.
		/// </summary>
		[Fact]
		public void EventTrigger_CreatesSuccessfully()
		{
			var trigger = new EventTrigger();
			
			Assert.NotNull(trigger);
			Assert.NotNull(trigger.Actions);
			Assert.Empty(trigger.Actions);
		}

		/// <summary>
		/// Validates that Event property can be set and retrieved.
		/// </summary>
		[Fact]
		public void EventTrigger_EventProperty_CanBeSetAndRetrieved()
		{
			var trigger = new EventTrigger();
			const string eventName = "Clicked";
			trigger.Event = eventName;
			
			Assert.Equal(eventName, trigger.Event);
		}
#pragma warning restore CS0618

		/// <summary>
		/// Validates that EventTrigger.Create can be created with lambdas.
		/// </summary>
		[Fact]
		public void EventTrigger_Create_CanBeCreatedWithLambdas()
		{
			var trigger = EventTrigger.Create<Button>(
				static (b, h) => b.Clicked += h,
				static (b, h) => b.Clicked -= h);
			
			Assert.NotNull(trigger);
			Assert.NotNull(trigger.Actions);
		}

		/// <summary>
		/// Validates that EventTrigger.Create properly binds to event.
		/// </summary>
		[Fact]
		public void EventTrigger_Create_BindsToEventSuccessfully()
		{
			var callCount = 0;
			var button = new Button { Text = "Test" };
			
			var trigger = EventTrigger.Create<Button>(
				static (b, h) => b.Clicked += h,
				static (b, h) => b.Clicked -= h);
			
			// Create a test action
			var action = new CountingTriggerAction(() => callCount++);
			trigger.Actions.Add(action);
			
			// Attach to button
			((TriggerBase)trigger).OnAttachedTo(button);
			
			// Simulate button click
			button.SendClicked();
			
			Assert.True(callCount > 0, "Action should be invoked on button click");
		}

		/// <summary>
		/// Validates that event handlers are properly removed when detached.
		/// </summary>
		[Fact]
		public void EventTrigger_Create_RemovesHandlerWhenDetached()
		{
			var callCount = 0;
			var button = new Button { Text = "Test" };
			
			var trigger = EventTrigger.Create<Button>(
				static (b, h) => b.Clicked += h,
				static (b, h) => b.Clicked -= h);
			
			var action = new CountingTriggerAction(() => callCount++);
			trigger.Actions.Add(action);
			
			// Attach, click, detach, click
			((TriggerBase)trigger).OnAttachedTo(button);
			button.SendClicked();
			var countAfterFirstClick = callCount;
			
			((TriggerBase)trigger).OnDetachingFrom(button);
			button.SendClicked();
			var countAfterSecondClick = callCount;
			
			Assert.True(countAfterFirstClick > 0, "Should invoke after attach");
			Assert.Equal(countAfterFirstClick, countAfterSecondClick);
		}

		/// <summary>
		/// Validates that EventTrigger.Create with EventHandler&lt;T&gt; works correctly.
		/// </summary>
		[Fact]
		public void EventTrigger_CreateWithEventArgs_BindsToEventSuccessfully()
		{
			var callCount = 0;
			var entry = new Entry { Text = "Test" };
			
			var trigger = EventTrigger.Create<Entry, TextChangedEventArgs>(
				static (e, h) => e.TextChanged += h,
				static (e, h) => e.TextChanged -= h);
			
			var action = new CountingTriggerAction(() => callCount++);
			trigger.Actions.Add(action);
			
			// Attach to entry
			((TriggerBase)trigger).OnAttachedTo(entry);
			
			// Simulate text change
			entry.Text = "Changed";
			
			Assert.True(callCount > 0, "Action should be invoked on text change");
		}

		/// <summary>
		/// Validates multiple attachments and detachments work correctly.
		/// </summary>
		[Fact]
		public void EventTrigger_Create_MultipleAttachmentsAndDetachments()
		{
			var callCount = 0;
			var button1 = new Button { Text = "Button1" };
			var button2 = new Button { Text = "Button2" };
			
			var trigger = EventTrigger.Create<Button>(
				static (b, h) => b.Clicked += h,
				static (b, h) => b.Clicked -= h);
			
			var action = new CountingTriggerAction(() => callCount++);
			trigger.Actions.Add(action);
			
			// Attach to both buttons
			((TriggerBase)trigger).OnAttachedTo(button1);
			((TriggerBase)trigger).OnAttachedTo(button2);
			
			// Click both - should invoke twice
			button1.SendClicked();
			button2.SendClicked();
			Assert.Equal(2, callCount);
			
			// Detach from first button
			((TriggerBase)trigger).OnDetachingFrom(button1);
			
			// Click both - only second should invoke
			callCount = 0;
			button1.SendClicked();
			button2.SendClicked();
			Assert.Equal(1, callCount);
		}

		private class CountingTriggerAction : TriggerAction<BindableObject>
		{
			private readonly Action _callback;

			public CountingTriggerAction(Action callback)
			{
				_callback = callback;
			}

			protected override void Invoke(BindableObject sender)
			{
				_callback?.Invoke();
			}
		}
	}
}
