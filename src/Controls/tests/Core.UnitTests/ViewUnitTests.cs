using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ViewUnitTests : BaseTestFixture
	{
		MockDeviceInfo mockDeviceInfo;


		public ViewUnitTests()
		{

			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (ve, widthConstraint, heightConstraint) =>
			{
				if (widthConstraint < 30)
					return new SizeRequest(new Size(40, 50));
				return new SizeRequest(new Size(20, 100));
			};
		}

		[Fact]
		public void TestLayout()
		{
			View view = new View();
			view.Layout(new Rect(50, 25, 100, 200));

			Assert.Equal(50, view.X);
			Assert.Equal(25, view.Y);
			Assert.Equal(100, view.Width);
			Assert.Equal(200, view.Height);
		}

		[Fact]
		public void TestPreferredSize()
		{
			View view = new View
			{
				IsPlatformEnabled = true,
			};

			bool fired = false;
			view.MeasureInvalidated += (sender, e) => fired = true;

			view.WidthRequest = 200;
			view.HeightRequest = 300;

			Assert.True(fired);

			var result = view.Measure(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.Equal(new Size(200, 300), result);
		}

		[Fact]
		public void TestSizeChangedEvent()
		{
			View view = new View();

			bool fired = false;
			view.SizeChanged += (sender, e) => fired = true;

			view.Layout(new Rect(0, 0, 100, 100));

			Assert.True(fired);
		}

		[Fact]
		public void TestOpacityClamping()
		{
			var view = new View();

			view.Opacity = -1;
			Assert.Equal(0, view.Opacity);

			view.Opacity = 2;
			Assert.Equal(1, view.Opacity);
		}

		[Fact]
		public void TestMeasureInvalidatedFiredOnVisibilityChanged()
		{
			var view = new View { IsVisible = false };
			bool signaled = false;
			view.MeasureInvalidated += (sender, e) =>
			{
				signaled = true;
			};
			view.IsVisible = true;
			Assert.True(signaled);
		}

		[Fact]
		public void TestNativeStateConsistent()
		{
			var view = new View { IsPlatformEnabled = true };

			Assert.True(view.IsPlatformStateConsistent);

			view.IsPlatformStateConsistent = false;

			Assert.False(view.IsPlatformStateConsistent);

			bool sizeChanged = false;
			view.MeasureInvalidated += (sender, args) =>
			{
				sizeChanged = true;
			};

			view.IsPlatformStateConsistent = true;

			Assert.True(sizeChanged);

			sizeChanged = false;
			view.IsPlatformStateConsistent = true;

			Assert.False(sizeChanged);
		}

		[Fact]
		public async Task TestFadeTo()
		{
			var view = AnimationReadyHandler.Prepare(new View());

			await view.FadeTo(0.1);

			Assert.True(Math.Abs(0.1 - view.Opacity) < 0.001);
		}

		[Fact]
		public async Task TestTranslateTo()
		{
			var view = AnimationReadyHandler.Prepare(new View());

			await view.TranslateTo(100, 50);

			Assert.Equal(100, view.TranslationX);
			Assert.Equal(50, view.TranslationY);
		}

		[Fact]
		public async Task ScaleTo()
		{
			var view = AnimationReadyHandler.Prepare(new View());

			await view.ScaleTo(2);

			Assert.Equal(2, view.Scale);
		}

		[Fact]
		public void TestPlatformSizeChanged()
		{
			var view = new View();

			bool sizeChanged = false;
			view.MeasureInvalidated += (sender, args) => sizeChanged = true;

			((IVisualElementController)view).PlatformSizeChanged();

			Assert.True(sizeChanged);
		}

		[Fact]
		public async Task TestRotateTo()
		{
			var view = AnimationReadyHandler.Prepare(new View());

			await view.RotateTo(25);

			AssertEqualWithTolerance(view.Rotation, 25, 0.001);
		}

		[Fact]
		public async Task TestRotateYTo()
		{
			var view = AnimationReadyHandler.Prepare(new View());

			await view.RotateYTo(25);

			AssertEqualWithTolerance(view.RotationY, 25, 0.001);
		}

		[Fact]
		public async Task TestRotateXTo()
		{
			var view = AnimationReadyHandler.Prepare(new View());

			await view.RotateXTo(25);

			AssertEqualWithTolerance(view.RotationX, 25, 0.001);
		}

		[Fact]
		public async Task TestRelRotateTo()
		{
			var view = AnimationReadyHandler.Prepare(new View { Rotation = 30 });

			await view.RelRotateTo(20);

			AssertEqualWithTolerance(view.Rotation, 50, 0.001);
		}

		[Fact]
		public async Task TestRelScaleTo()
		{
			var view = AnimationReadyHandler.Prepare(new View { Scale = 1 });

			await view.RelScaleTo(1);

			AssertEqualWithTolerance(view.Scale, 2, 0.001);
		}

		class ParentSignalView : View
		{
			public bool ParentSet { get; set; }

			protected override void OnParentSet()
			{
				ParentSet = true;
				base.OnParentSet();
			}
		}

		[Fact]
		public void TestDoubleSetParent()
		{
			var view = new ParentSignalView();
			var parent = new NaiveLayout { Children = { view } };

			view.ParentSet = false;
			view.Parent = parent;

			Assert.False(view.ParentSet, "OnParentSet should not be called in the event the parent is already properly set");
		}

		[Fact]
		public void TestAncestorAdded()
		{
			var child = new NaiveLayout();
			var view = new NaiveLayout { Children = { child } };

			bool added = false;
			view.DescendantAdded += (sender, arg) => added = true;

			child.Children.Add(new View());

			Assert.True(added, "AncestorAdded must fire when adding a child to an ancestor of a view.");
		}

		[Fact]
		public void TestAncestorRemoved()
		{
			var ancestor = new View();
			var child = new NaiveLayout { Children = { ancestor } };
			var view = new NaiveLayout { Children = { child } };

			bool removed = false;
			view.DescendantRemoved += (sender, arg) => removed = true;

			child.Children.Remove(ancestor);
			Assert.True(removed, "AncestorRemoved must fire when removing a child from an ancestor of a view.");
		}

		[Fact]
		public void TestOnIdiomDefault()
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Tablet;
			Assert.Equal(12, (int)(new OnIdiom<int> { Tablet = 12, Default = 42 }));
			mockDeviceInfo.Idiom = DeviceIdiom.Watch;
			Assert.Equal(42, (int)(new OnIdiom<int> { Tablet = 12, Default = 42 }));
		}

		[Fact]
		public void TestBatching()
		{
			var view = new View();

			bool committed = false;
			view.BatchCommitted += (sender, arg) => committed = true;

			view.BatchBegin();

			Assert.True(view.Batched);

			view.BatchBegin();

			Assert.True(view.Batched);

			view.BatchCommit();

			Assert.True(view.Batched);
			Assert.False(committed);

			view.BatchCommit();

			Assert.False(view.Batched);
			Assert.True(committed);
		}

		[Fact]
		public void TestBatchRegularCase()
		{
			var view = new View();
			var committed = false;
			view.BatchCommitted += (sender, arg) => committed = true;
			using (view.Batch())
			{
				Assert.True(view.Batched);
			}
			Assert.False(view.Batched);
			Assert.True(committed);
		}

		[Fact]
		public void TestBatchWhenExceptionThrown()
		{
			var view = new View();
			var committed = false;
			view.BatchCommitted += (sender, arg) => committed = true;
			var exceptionThrown = false;
			try
			{
				using (view.Batch())
				{
					Assert.True(view.Batched);
					throw new Exception();
				}
			}
			catch
			{
				exceptionThrown = true;
			}
			Assert.True(exceptionThrown);
			Assert.False(view.Batched);
			Assert.True(committed);
		}

		[Fact]
		public void IsPlatformEnabled()
		{
			var view = new View();

			Assert.False(view.IsPlatformEnabled);

			view.IsPlatformEnabled = true;

			Assert.True(view.IsPlatformEnabled);

			view.IsPlatformEnabled = false;

			Assert.False(view.IsPlatformEnabled);
		}

		[Fact]
		public void TestBindingContextChaining()
		{
			View child;
			var group = new NaiveLayout
			{
				Children = { (child = new View()) }
			};

			var context = new object();
			group.BindingContext = context;

			Assert.Equal(context, child.BindingContext);
		}



		[Fact]
		public void FocusWithoutSubscriber()
		{
			var view = new View();

			Assert.False(view.Focus());
		}

		[Theory, InlineData(true), InlineData(false)]
		public void FocusWithSubscriber(bool result)
		{
			var view = new View();
			view.FocusChangeRequested += (sender, arg) => arg.Result = result;
			Assert.True(view.Focus() == result);
		}

		[Fact]
		public void DoNotSignalWhenAlreadyFocused()
		{
			var view = new View();
			view.SetValue(VisualElement.IsFocusedPropertyKey, true, specificity: SetterSpecificity.FromHandler);
			bool signaled = false;
			view.FocusChangeRequested += (sender, args) => signaled = true;

			Assert.True(view.Focus(), "View.Focus returned false");
			Assert.False(signaled, "FocusRequested was raised");
		}

		[Fact]
		public void UnFocus()
		{
			var view = new View();
			view.SetValue(VisualElement.IsFocusedPropertyKey, true, specificity: SetterSpecificity.FromHandler);

			var requested = false;
			view.FocusChangeRequested += (sender, args) =>
			{
				requested = !args.Focus;
			};

			view.Unfocus();

			Assert.True(requested);
		}

		[Fact]
		public void UnFocusDoesNotFireWhenNotFocused()
		{
			var view = new View();
			view.SetValue(VisualElement.IsFocusedPropertyKey, false, specificity: SetterSpecificity.FromHandler);

			var requested = false;
			view.FocusChangeRequested += (sender, args) =>
			{
				requested = args.Focus;
			};

			view.Unfocus();

			Assert.False(requested);
		}

		[Fact]
		public void TestFocusedEvent()
		{
			var view = new View();

			bool fired = false;
			view.Focused += (sender, args) => fired = true;
			view.SetValue(VisualElement.IsFocusedPropertyKey, true, specificity: SetterSpecificity.FromHandler);


			Assert.True(fired);
		}

		[Fact]
		public void TestUnFocusedEvent()
		{
			var view = new View();
			view.SetValue(VisualElement.IsFocusedPropertyKey, true, specificity: SetterSpecificity.FromHandler);

			bool fired = false;
			view.Unfocused += (sender, args) => fired = true;
			view.SetValue(VisualElement.IsFocusedPropertyKey, false, specificity: SetterSpecificity.FromHandler);

			Assert.True(fired);
		}

		[Fact]
		public void MinimumWidthRequest()
		{
			var view = new View();

			bool signaled = false;
			view.MeasureInvalidated += (sender, args) => signaled = true;

			view.MinimumWidthRequest = 10;
			Assert.True(signaled);
			Assert.Equal(10, view.MinimumWidthRequest);

			signaled = false;
			view.MinimumWidthRequest = 10;
			Assert.False(signaled);
		}

		[Fact]
		public void MinimumHeightRequest()
		{
			var view = new View();

			bool signaled = false;
			view.MeasureInvalidated += (sender, args) => signaled = true;

			view.MinimumHeightRequest = 10;
			Assert.True(signaled);
			Assert.Equal(10, view.MinimumHeightRequest);

			signaled = false;
			view.MinimumHeightRequest = 10;
			Assert.False(signaled);
		}

		[Fact]
		public void MinimumWidthRequestInSizeRequest()
		{
			var view = new View
			{
				IsPlatformEnabled = true
			};

			view.HeightRequest = 20;
			view.WidthRequest = 200;
			view.MinimumWidthRequest = 100;

			var result = view.Measure(double.PositiveInfinity, double.PositiveInfinity);
			Assert.Equal(new Size(200, 20), result.Request);
			Assert.Equal(new Size(100, 20), result.Minimum);
		}

		[Fact]
		public void MinimumHeightRequestInSizeRequest()
		{
			var view = new View
			{
				IsPlatformEnabled = true
			};

			view.HeightRequest = 200;
			view.WidthRequest = 20;
			view.MinimumHeightRequest = 100;

			var result = view.Measure(double.PositiveInfinity, double.PositiveInfinity);
			Assert.Equal(new Size(20, 200), result.Request);
			Assert.Equal(new Size(20, 100), result.Minimum);
		}

		[Fact]
		public async Task StartTimerSimple()
		{
			var task = new TaskCompletionSource<bool>();

			Task.Factory.StartNew(() => Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
			{
				task.SetResult(false);
				return false;
			}));

			await task.Task;
			Assert.False(task.Task.Result);
		}

		[Fact]
		public async Task StartTimerMultiple()
		{
			var task = new TaskCompletionSource<int>();

			int steps = 0;
			Task.Factory.StartNew(() => Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
			{
				steps++;
				if (steps < 2)
					return true;
				task.SetResult(steps);
				return false;
			}));

			await task.Task;
			Assert.Equal(2, task.Task.Result);
		}

		[Fact]
		public void BindingsApplyAfterViewAddedToParentWithContextSet()
		{
			var parent = new NaiveLayout();
			parent.BindingContext = new MockViewModel { Text = "test" };

			var child = new Entry();
			child.SetBinding(Entry.TextProperty, new Binding("Text"));

			parent.Children.Add(child);

			Assert.Same(child.BindingContext, parent.BindingContext);
			Assert.Equal("test", child.Text);
		}

		[Fact]
		public void IdIsUnique()
		{
			var view1 = new View();
			var view2 = new View();

			Assert.True(view1.Id != view2.Id);
		}

		[Fact]
		public void MockBounds()
		{
			var view = new View();
			view.Layout(new Rect(10, 20, 30, 40));

			bool changed = false;
			view.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == View.XProperty.PropertyName ||
					args.PropertyName == View.YProperty.PropertyName ||
					args.PropertyName == View.WidthProperty.PropertyName ||
					args.PropertyName == View.HeightProperty.PropertyName)
					changed = true;
			};

			view.SizeChanged += (sender, args) => changed = true;

			view.MockBounds(new Rect(5, 10, 15, 20));

			Assert.Equal(new Rect(5, 10, 15, 20), view.Bounds);
			Assert.False(changed);

			view.UnmockBounds();

			Assert.Equal(new Rect(10, 20, 30, 40), view.Bounds);
			Assert.False(changed);
		}

		[Fact]
		public void AddGestureRecognizer()
		{
			var view = new View();
			var gestureRecognizer = new TapGestureRecognizer();

			view.GestureRecognizers.Add(gestureRecognizer);

			Assert.True(view.GestureRecognizers.Contains(gestureRecognizer));
		}

		[Fact]
		public void AddGestureRecognizerSetsParent()
		{
			var view = new View();
			var gestureRecognizer = new TapGestureRecognizer();

			view.GestureRecognizers.Add(gestureRecognizer);

			Assert.Equal(view, gestureRecognizer.Parent);
		}

		[Fact]
		public void RemoveGestureRecognizerUnsetsParent()
		{
			var view = new View();
			var gestureRecognizer = new TapGestureRecognizer();

			view.GestureRecognizers.Add(gestureRecognizer);
			view.GestureRecognizers.Remove(gestureRecognizer);

			Assert.Null(gestureRecognizer.Parent);
		}

		[Fact]
		public void WidthRequestEffectsGetSizeRequest()
		{
			var view = new View();
			view.IsPlatformEnabled = true;
			view.WidthRequest = 20;
			var request = view.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(new Size(20, 50), request.Request);
		}

		[Fact]
		public void HeightRequestEffectsGetSizeRequest()
		{
			MockPlatformSizeService.Current.GetPlatformSizeFunc = (ve, widthConstraint, heightConstraint) =>
			{
				if (heightConstraint < 30)
					return new SizeRequest(new Size(40, 50));
				return new SizeRequest(new Size(20, 100));
			};

			var view = new View();
			view.IsPlatformEnabled = true;
			view.HeightRequest = 20;
			var request = view.Measure(double.PositiveInfinity, double.PositiveInfinity);

			Assert.Equal(new Size(40, 20), request.Request);
		}

		[Fact]
		public void TestClip()
		{
			var view = new View
			{
				HeightRequest = 100,
				WidthRequest = 100,

				Clip = new RectangleGeometry
				{
					Rect = new Rect(0, 0, 50, 50)
				}
			};

			Assert.NotNull(view.Clip);
		}

		[Fact]
		public void TestRemoveClip()
		{
			var view = new View
			{
				HeightRequest = 100,
				WidthRequest = 100,

				Clip = new RectangleGeometry
				{
					Rect = new Rect(0, 0, 50, 50)
				}
			};

			Assert.NotNull(view.Clip);

			view.Clip = null;

			Assert.Null(view.Clip);
		}

		[Fact]
		public void AssigningElementHandlerThrowsException()
		{
			Maui.IElement view = new View();
			Assert.Throws<InvalidOperationException>(() => view.Handler = new ElementHandlerStub());
		}

		static void AssertEqualWithTolerance(double a, double b, double tolerance)
		{
			var diff = Math.Abs(a - b);
			Assert.True(diff <= tolerance);
		}
	}
}