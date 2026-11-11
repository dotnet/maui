#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Tests.ExternalGestureBackend;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Verifies that a platform backend living in a third-party assembly (one that is not on
	/// <c>Microsoft.Maui.Controls</c>'s <c>InternalsVisibleTo</c> list) can drive
	/// <see cref="LongPressGestureRecognizer"/> end to end, exactly like the public tap and pointer
	/// dispatch APIs allow today.
	/// </summary>
	/// <remarks>
	/// <see cref="FakeLongPressGestureBackend"/> is compiled into
	/// <c>Microsoft.Maui.Controls.Tests.ExternalGestureBackend</c>. The mere fact that it builds proves
	/// the dispatch contract is reachable from outside the framework; demoting the <c>Send*</c> methods
	/// back to <see langword="internal"/> breaks that project's compilation.
	/// </remarks>
	public class LongPressGestureRecognizerExternalBackendTests : BaseTestFixture
	{
		static (View View, LongPressGestureRecognizer Recognizer, FakeLongPressGestureBackend Backend) CreateBackend()
		{
			var view = new View();
			var recognizer = new LongPressGestureRecognizer();
			view.GestureRecognizers.Add(recognizer);

			return (view, recognizer, new FakeLongPressGestureBackend(view));
		}

		[Fact]
		public void DispatchMethodsArePubliclyAccessible()
		{
			var type = typeof(LongPressGestureRecognizer);

			foreach (var name in new[] { nameof(LongPressGestureRecognizer.SendLongPressed), nameof(LongPressGestureRecognizer.SendLongPressing) })
			{
				var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance);

				Assert.NotNull(method);
				Assert.True(method!.IsPublic);

				// Infrastructure API: reachable by backends but hidden from IntelliSense, matching
				// TapGestureRecognizer.SendTapped and PointerGestureRecognizer.Send*.
				var browsable = method.GetCustomAttribute<EditorBrowsableAttribute>();
				Assert.NotNull(browsable);
				Assert.Equal(EditorBrowsableState.Never, browsable!.State);
			}
		}

		[Fact]
		public void ExternalBackendRaisesFullGestureLifecycle()
		{
			var (_, recognizer, backend) = CreateBackend();
			var states = new List<GestureStatus>();
			var pressedCount = 0;

			recognizer.LongPressing += (_, args) => states.Add(args.Status);
			recognizer.LongPressed += (_, _) => pressedCount++;

			backend.RaisePressStarted(new Point(5, 5));
			Assert.Equal(GestureStatus.Started, recognizer.State);

			backend.RaisePressMoved(new Point(6, 6));
			Assert.Equal(GestureStatus.Running, recognizer.State);

			backend.RaisePressCompleted();

			Assert.Equal(new[] { GestureStatus.Started, GestureStatus.Running, GestureStatus.Completed }, states);
			Assert.Equal(1, pressedCount);
			Assert.Equal(GestureStatus.Completed, recognizer.State);
		}

		[Fact]
		public void ExternalBackendRaisesLongPressedBeforeCompleted()
		{
			var (_, recognizer, backend) = CreateBackend();

			var sequence = new List<string>();
			recognizer.LongPressed += (_, _) => sequence.Add("pressed");
			recognizer.LongPressing += (_, args) => sequence.Add(args.Status.ToString());

			backend.RaisePressStarted(Point.Zero);
			backend.RaisePressCompleted();

			Assert.Equal(new[] { "Started", "pressed", "Completed" }, sequence);
		}

		[Fact]
		public void ExternalBackendCancelDoesNotRaiseLongPressedOrCommand()
		{
			var (_, recognizer, backend) = CreateBackend();

			var commandInvoked = false;
			var pressedRaised = false;
			recognizer.Command = new Command(() => commandInvoked = true);
			recognizer.LongPressed += (_, _) => pressedRaised = true;

			backend.RaisePressStarted(Point.Zero);
			backend.RaisePressCanceled();

			Assert.False(pressedRaised);
			Assert.False(commandInvoked);
			Assert.Equal(GestureStatus.Canceled, recognizer.State);
		}

		[Fact]
		public void ExternalBackendCancelsWhenMovementExceedsAllowableMovement()
		{
			var (_, recognizer, backend) = CreateBackend();
			recognizer.AllowableMovement = 5;

			var states = new List<GestureStatus>();
			recognizer.LongPressing += (_, args) => states.Add(args.Status);

			backend.RaisePressStarted(Point.Zero);
			backend.RaisePressMoved(new Point(100, 100));

			Assert.Equal(new[] { GestureStatus.Started, GestureStatus.Canceled }, states);
			Assert.Empty(backend.ActiveRecognizers);
		}

		[Fact]
		public void ExternalBackendExecutesCommandWithParameter()
		{
			var (_, recognizer, backend) = CreateBackend();

			object? received = null;
			recognizer.CommandParameter = "tizen";
			recognizer.Command = new Command(parameter => received = parameter);

			backend.RaisePressStarted(Point.Zero);
			backend.RaisePressCompleted();

			Assert.Equal("tizen", received);
		}

		[Fact]
		public void ExternalBackendRespectsCanExecute()
		{
			var (_, recognizer, backend) = CreateBackend();

			var commandInvoked = false;
			recognizer.Command = new Command(() => commandInvoked = true, () => false);

			backend.RaisePressStarted(Point.Zero);
			backend.RaisePressCompleted();

			Assert.False(commandInvoked);
		}

		[Fact]
		public void ExternalBackendPassesSenderAndPositionCallback()
		{
			var (view, recognizer, backend) = CreateBackend();
			var relativeElement = new View();

			object? pressedSender = null;
			Point? positionInView = null;
			Point? positionRelativeToOther = null;

			recognizer.LongPressed += (sender, args) =>
			{
				pressedSender = sender;
				positionInView = args.GetPosition(view);
				positionRelativeToOther = args.GetPosition(relativeElement);
			};

			backend.RaisePressStarted(new Point(12, 34));
			backend.RaisePressCompleted();

			Assert.Same(view, pressedSender);
			Assert.Equal(new Point(12, 34), positionInView);
			Assert.Equal(
				new Point(12 + FakeLongPressGestureBackend.RelativeElementOffset.X, 34 + FakeLongPressGestureBackend.RelativeElementOffset.Y),
				positionRelativeToOther);
		}

		[Fact]
		public void ExternalBackendOnlyDispatchesToRecognizersMatchingTouchCount()
		{
			var view = new View();
			var oneTouch = new LongPressGestureRecognizer { NumberOfTouchesRequired = 1 };
			var twoTouch = new LongPressGestureRecognizer { NumberOfTouchesRequired = 2 };
			view.GestureRecognizers.Add(oneTouch);
			view.GestureRecognizers.Add(twoTouch);

			var oneTouchPressed = false;
			var twoTouchPressed = false;
			oneTouch.LongPressed += (_, _) => oneTouchPressed = true;
			twoTouch.LongPressed += (_, _) => twoTouchPressed = true;

			var backend = new FakeLongPressGestureBackend(view) { TouchCount = 2 };
			backend.RaisePressStarted(Point.Zero);
			backend.RaisePressCompleted();

			Assert.False(oneTouchPressed);
			Assert.True(twoTouchPressed);
		}

		[Fact]
		public void ExternalBackendDispatchesToEveryMatchingRecognizerOnTheView()
		{
			var view = new View();
			var recognizers = Enumerable.Range(0, 3).Select(_ => new LongPressGestureRecognizer()).ToList();
			foreach (var recognizer in recognizers)
				view.GestureRecognizers.Add(recognizer);

			var backend = new FakeLongPressGestureBackend(view);
			backend.RaisePressStarted(Point.Zero);
			backend.RaisePressCompleted();

			Assert.All(recognizers, recognizer => Assert.Equal(GestureStatus.Completed, recognizer.State));
		}
	}
}
