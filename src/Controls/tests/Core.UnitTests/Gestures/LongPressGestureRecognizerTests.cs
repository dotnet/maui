#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class LongPressGestureRecognizerTests : BaseTestFixture
	{
		[Fact]
		public void Constructor_InitializesDefaultValues()
		{
			var longPress = new LongPressGestureRecognizer();

			Assert.Null(longPress.Command);
			Assert.Null(longPress.CommandParameter);
			Assert.Equal(500, longPress.MinimumPressDuration);
			Assert.Equal(1, longPress.NumberOfTouchesRequired);
			Assert.Equal(10.0, longPress.AllowableMovement);
			Assert.Equal(GestureStatus.Canceled, longPress.State);
		}

		[Fact]
		public void Command_CanBeSet()
		{
			var longPress = new LongPressGestureRecognizer();
			var command = new Command(() => { });

			longPress.Command = command;

			Assert.Equal(command, longPress.Command);
		}

		[Fact]
		public void CommandParameter_CanBeSet()
		{
			var longPress = new LongPressGestureRecognizer();
			var parameter = "TestParameter";

			longPress.CommandParameter = parameter;

			Assert.Equal(parameter, longPress.CommandParameter);
		}

		[Fact]
		public void MinimumPressDuration_CanBeChanged()
		{
			var longPress = new LongPressGestureRecognizer();

			longPress.MinimumPressDuration = 1000;

			Assert.Equal(1000, longPress.MinimumPressDuration);
		}

		[Fact]
		public void NumberOfTouchesRequired_CanBeChanged()
		{
			var longPress = new LongPressGestureRecognizer();

			longPress.NumberOfTouchesRequired = 2;

			Assert.Equal(2, longPress.NumberOfTouchesRequired);
		}

		[Fact]
		public void AllowableMovement_CanBeChanged()
		{
			var longPress = new LongPressGestureRecognizer();

			longPress.AllowableMovement = 20.0;

			Assert.Equal(20.0, longPress.AllowableMovement);
		}

		[Fact]
		public void SendLongPressed_InvokesCommand()
		{
			var view = new View();
			var longPress = new LongPressGestureRecognizer();
			bool commandInvoked = false;

			longPress.Command = new Command(() => commandInvoked = true);

			longPress.SendLongPressed(view);

			Assert.True(commandInvoked);
		}

		[Fact]
		public void SendLongPressed_PassesParameter()
		{
			var view = new View();
			var longPress = new LongPressGestureRecognizer();
			longPress.CommandParameter = "Hello";

			object result = null;
			longPress.Command = new Command(o => result = o);

			longPress.SendLongPressed(view);

			Assert.Equal("Hello", result);
		}

		[Fact]
		public void SendLongPressed_RaisesEvent()
		{
			var view = new View();
			var longPress = new LongPressGestureRecognizer();
			bool eventRaised = false;
			LongPressedEventArgs eventArgs = null;

			longPress.LongPressed += (sender, args) =>
			{
				eventRaised = true;
				eventArgs = args;
			};

			var position = new Point(10, 20);
			Func<IElement?, Point?> getPosition = (_) => position;
			longPress.SendLongPressed(view, getPosition);

			Assert.True(eventRaised);
			Assert.NotNull(eventArgs);
			Assert.Equal(position, eventArgs!.GetPosition(null));
		}

		[Fact]
		public void SendLongPressed_PassesParameterInEventArgs()
		{
			var view = new View();
			var longPress = new LongPressGestureRecognizer();
			longPress.CommandParameter = "EventParameter";

			LongPressedEventArgs eventArgs = null;
			longPress.LongPressed += (sender, args) => eventArgs = args;

			longPress.SendLongPressed(view);

			Assert.NotNull(eventArgs);
			Assert.Equal("EventParameter", eventArgs!.Parameter);
		}

		[Fact]
		public void SendLongPressing_UpdatesState()
		{
			var view = new View();
			var longPress = new LongPressGestureRecognizer();

			longPress.SendLongPressing(view, GestureStatus.Started, null);
			Assert.Equal(GestureStatus.Started, longPress.State);

			longPress.SendLongPressing(view, GestureStatus.Running, null);
			Assert.Equal(GestureStatus.Running, longPress.State);

			longPress.SendLongPressing(view, GestureStatus.Completed, null);
			Assert.Equal(GestureStatus.Completed, longPress.State);

			longPress.SendLongPressing(view, GestureStatus.Canceled, null);
			Assert.Equal(GestureStatus.Canceled, longPress.State);
		}

		[Fact]
		public void SendLongPressing_RaisesEvent()
		{
			var view = new View();
			var longPress = new LongPressGestureRecognizer();
			bool eventRaised = false;
			LongPressingEventArgs eventArgs = null;

			longPress.LongPressing += (sender, args) =>
			{
				eventRaised = true;
				eventArgs = args;
			};

			var position = new Point(15, 25);
			Func<IElement?, Point?> getPosition = (_) => position;
			longPress.SendLongPressing(view, GestureStatus.Started, getPosition);

			Assert.True(eventRaised);
			Assert.NotNull(eventArgs);
			Assert.Equal(GestureStatus.Started, eventArgs!.Status);
			Assert.Equal(position, eventArgs.GetPosition(null));
		}

		[Fact]
		public void PropertyChanged_RaisesCorrectly()
		{
			var longPress = new LongPressGestureRecognizer();
			bool propertyChanged = false;
			string changedPropertyName = null;

			longPress.PropertyChanged += (sender, args) =>
			{
				propertyChanged = true;
				changedPropertyName = args.PropertyName;
			};

			longPress.MinimumPressDuration = 750;

			Assert.True(propertyChanged);
			Assert.Equal(nameof(LongPressGestureRecognizer.MinimumPressDuration), changedPropertyName);
		}

		[Fact]
		public void Command_DoesNotExecuteWhenCanExecuteIsFalse()
		{
			var view = new View();
			var longPress = new LongPressGestureRecognizer();
			bool commandInvoked = false;

			longPress.Command = new Command(() => commandInvoked = true, () => false);

			longPress.SendLongPressed(view);

			Assert.False(commandInvoked);
		}

		[Fact]
		public void LongPressedEventArgs_ConstructorSetsProperties()
		{
			var parameter = "Test";

			var eventArgs = new LongPressedEventArgs(parameter);

			Assert.Equal(parameter, eventArgs.Parameter);
			Assert.Null(eventArgs.GetPosition(null)); // No position function set in public constructor
		}

		[Fact]
		public void LongPressingEventArgs_ConstructorSetsProperties()
		{
			var status = GestureStatus.Running;

			var eventArgs = new LongPressingEventArgs(status);

			Assert.Equal(status, eventArgs.Status);
			Assert.Null(eventArgs.GetPosition(null)); // No position function set in public constructor
		}

		[Fact]
		public void State_IsOneWayToSource()
		{
			var longPress = new LongPressGestureRecognizer();
			
			// State should only be set internally via SendLongPressing
			// Verify it has OneWayToSource binding mode by checking the property
			var stateProperty = LongPressGestureRecognizer.StateProperty;
			Assert.Equal(BindingMode.OneWayToSource, stateProperty.DefaultBindingMode);
		}
	}
}
