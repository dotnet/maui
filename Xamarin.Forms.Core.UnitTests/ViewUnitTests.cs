using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

using NUnit.Framework;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Shapes;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ViewUnitTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices(getNativeSizeFunc: (ve, widthConstraint, heightConstraint) =>
			{
				if (widthConstraint < 30)
					return new SizeRequest(new Size(40, 50));
				return new SizeRequest(new Size(20, 100));
			});
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void TestLayout()
		{
			View view = new View();
			view.Layout(new Rectangle(50, 25, 100, 200));

			Assert.AreEqual(view.X, 50);
			Assert.AreEqual(view.Y, 25);
			Assert.AreEqual(view.Width, 100);
			Assert.AreEqual(view.Height, 200);
		}

		[Test]
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

			var result = view.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request;
			Assert.AreEqual(new Size(200, 300), result);
		}

		[Test]
		public void TestSizeChangedEvent()
		{
			View view = new View();

			bool fired = false;
			view.SizeChanged += (sender, e) => fired = true;

			view.Layout(new Rectangle(0, 0, 100, 100));

			Assert.True(fired);
		}

		[Test]
		public void TestOpacityClamping()
		{
			var view = new View();

			view.Opacity = -1;
			Assert.AreEqual(0, view.Opacity);

			view.Opacity = 2;
			Assert.AreEqual(1, view.Opacity);
		}

		[Test]
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

		[Test]
		public void TestOnPlatformiOS()
		{
			var view = new View();

			bool ios = false;
			bool android = false;
			bool winphone = false;

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;

			Device.OnPlatform(
				iOS: () => ios = true,
				Android: () => android = true,
				WinPhone: () => winphone = true);

			Assert.True(ios);
			Assert.False(android);
			Assert.False(winphone);
		}

		[Test]
		public void TestOnPlatformAndroid()
		{
			var view = new View();

			bool ios = false;
			bool android = false;
			bool winphone = false;

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;

			Device.OnPlatform(
				iOS: () => ios = true,
				Android: () => android = true,
				WinPhone: () => winphone = true);

			Assert.False(ios);
			Assert.True(android);
			Assert.False(winphone);
		}

		[Test]
		public void TestOnPlatformDefault()
		{
			var view = new View();

			bool ios = false;
			bool android = false;

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;

			Device.OnPlatform(
				iOS: () => ios = false,
				Default: () => android = true);

			Assert.False(ios);
			Assert.True(android);
		}

		[Test]
		public void TestOnPlatformNoOpWithoutDefault()
		{
			bool any = false;
			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = "Other";

			Device.OnPlatform(
				iOS: () => any = true,
				Android: () => any = true,
				WinPhone: () => any = true);

			Assert.False(any);
		}

		[Test]
		public void TestDefaultOniOS()
		{
			bool defaultExecuted = false;

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;

			Device.OnPlatform(
				Android: () => { },
				WinPhone: () => { },
				Default: () => defaultExecuted = true);

			Assert.True(defaultExecuted);
		}

		[Test]
		public void TestDefaultOnAndroid()
		{
			bool defaultExecuted = false;

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;

			Device.OnPlatform(
				iOS: () => { },
				WinPhone: () => { },
				Default: () => defaultExecuted = true);

			Assert.True(defaultExecuted);
		}

		[Test]
		public void TestDefaultOnOther()
		{
			bool defaultExecuted = false;

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = "Other";

			Device.OnPlatform(
				iOS: () => { },
				Android: () => { },
				WinPhone: () => { },
				Default: () => defaultExecuted = true);

			Assert.True(defaultExecuted);
		}

		[Test]
		public void TestNativeStateConsistent()
		{
			var view = new View { IsPlatformEnabled = true };

			Assert.True(view.IsNativeStateConsistent);

			view.IsNativeStateConsistent = false;

			Assert.False(view.IsNativeStateConsistent);

			bool sizeChanged = false;
			view.MeasureInvalidated += (sender, args) =>
			{
				sizeChanged = true;
			};

			view.IsNativeStateConsistent = true;

			Assert.True(sizeChanged);

			sizeChanged = false;
			view.IsNativeStateConsistent = true;

			Assert.False(sizeChanged);
		}

		[Test]
		public async Task TestFadeTo()
		{
			var view = new View { IsPlatformEnabled = true };
			Ticker.Default = new BlockingTicker();

			await view.FadeTo(0.1);

			Assert.True(Math.Abs(0.1 - view.Opacity) < 0.001);
		}

		[Test]
		public async Task TestTranslateTo()
		{
			var view = new View { IsPlatformEnabled = true };
			Ticker.Default = new BlockingTicker();

			await view.TranslateTo(100, 50);

			Assert.AreEqual(100, view.TranslationX);
			Assert.AreEqual(50, view.TranslationY);
		}

		[Test]
		public async Task ScaleTo()
		{
			var view = new View { IsPlatformEnabled = true };
			Ticker.Default = new BlockingTicker();

			await view.ScaleTo(2);

			Assert.AreEqual(2, view.Scale);
		}

		[Test]
		public void TestNativeSizeChanged()
		{
			var view = new View();

			bool sizeChanged = false;
			view.MeasureInvalidated += (sender, args) => sizeChanged = true;

			((IVisualElementController)view).NativeSizeChanged();

			Assert.True(sizeChanged);
		}

		[Test]
		public async Task TestRotateTo()
		{
			var view = new View { IsPlatformEnabled = true };
			Ticker.Default = new BlockingTicker();

			await view.RotateTo(25);

			Assert.That(view.Rotation, Is.EqualTo(25).Within(0.001));
		}

		[Test]
		public async Task TestRotateYTo()
		{
			var view = new View { IsPlatformEnabled = true };
			Ticker.Default = new BlockingTicker();

			await view.RotateYTo(25);

			Assert.That(view.RotationY, Is.EqualTo(25).Within(0.001));
		}

		[Test]
		public async Task TestRotateXTo()
		{
			var view = new View { IsPlatformEnabled = true };
			Ticker.Default = new BlockingTicker();

			await view.RotateXTo(25);

			Assert.That(view.RotationX, Is.EqualTo(25).Within(0.001));
		}

		[Test]
		public async Task TestRelRotateTo()
		{
			var view = new View { Rotation = 30, IsPlatformEnabled = true };
			Ticker.Default = new BlockingTicker();

			await view.RelRotateTo(20);

			Assert.That(view.Rotation, Is.EqualTo(50).Within(0.001));
		}

		[Test]
		public async Task TestRelScaleTo()
		{
			var view = new View { Scale = 1, IsPlatformEnabled = true };
			Ticker.Default = new BlockingTicker();

			await view.RelScaleTo(1);

			Assert.That(view.Scale, Is.EqualTo(2).Within(0.001));
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

		[Test]
		public void TestDoubleSetParent()
		{
			var view = new ParentSignalView();
			var parent = new NaiveLayout { Children = { view } };

			view.ParentSet = false;
			view.Parent = parent;

			Assert.False(view.ParentSet, "OnParentSet should not be called in the event the parent is already properly set");
		}

		[Test]
		public void TestAncestorAdded()
		{
			var child = new NaiveLayout();
			var view = new NaiveLayout { Children = { child } };

			bool added = false;
			view.DescendantAdded += (sender, arg) => added = true;

			child.Children.Add(new View());

			Assert.True(added, "AncestorAdded must fire when adding a child to an ancestor of a view.");
		}

		[Test]
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

		[Test]
		public void TestOnPlatformGeneric()
		{
			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
			Assert.AreEqual(1, Device.OnPlatform(1, 2, 3));

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
			Assert.AreEqual(2, Device.OnPlatform(1, 2, 3));

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = "Other";
			Assert.AreEqual(1, Device.OnPlatform(1, 2, 3));
		}

		[Test]
		public void TestOnIdiomDefault()
		{
			Device.Idiom = TargetIdiom.Tablet;
			Assert.That((int)(new OnIdiom<int> { Tablet = 12, Default = 42 }), Is.EqualTo(12));
			Device.Idiom = TargetIdiom.Watch;
			Assert.That((int)(new OnIdiom<int> { Tablet = 12, Default = 42 }), Is.EqualTo(42));
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void IsPlatformEnabled()
		{
			var view = new View();

			Assert.False(view.IsPlatformEnabled);

			view.IsPlatformEnabled = true;

			Assert.True(view.IsPlatformEnabled);

			view.IsPlatformEnabled = false;

			Assert.False(view.IsPlatformEnabled);
		}

		[Test]
		public void TestBindingContextChaining()
		{
			View child;
			var group = new NaiveLayout
			{
				Children = { (child = new View()) }
			};

			var context = new object();
			group.BindingContext = context;

			Assert.AreEqual(context, child.BindingContext);
		}



		[Test]
		public void FocusWithoutSubscriber()
		{
			var view = new View();

			Assert.False(view.Focus());
		}

		[Test]
		public void FocusWithSubscriber([Values(true, false)] bool result)
		{
			var view = new View();
			view.FocusChangeRequested += (sender, arg) => arg.Result = result;
			Assert.True(view.Focus() == result);
		}

		[Test]
		public void DoNotSignalWhenAlreadyFocused()
		{
			var view = new View();
			view.SetValueCore(VisualElement.IsFocusedPropertyKey, true);
			bool signaled = false;
			view.FocusChangeRequested += (sender, args) => signaled = true;

			Assert.True(view.Focus(), "View.Focus returned false");
			Assert.False(signaled, "FocusRequested was raised");
		}

		[Test]
		public void UnFocus()
		{
			var view = new View();
			view.SetValueCore(VisualElement.IsFocusedPropertyKey, true);

			var requested = false;
			view.FocusChangeRequested += (sender, args) =>
			{
				requested = !args.Focus;
			};

			view.Unfocus();

			Assert.True(requested);
		}

		[Test]
		public void UnFocusDoesNotFireWhenNotFocused()
		{
			var view = new View();
			view.SetValueCore(VisualElement.IsFocusedPropertyKey, false);

			var requested = false;
			view.FocusChangeRequested += (sender, args) =>
			{
				requested = args.Focus;
			};

			view.Unfocus();

			Assert.False(requested);
		}

		[Test]
		public void TestFocusedEvent()
		{
			var view = new View();

			bool fired = false;
			view.Focused += (sender, args) => fired = true;
			view.SetValueCore(VisualElement.IsFocusedPropertyKey, true);


			Assert.True(fired);
		}

		[Test]
		public void TestUnFocusedEvent()
		{
			var view = new View();
			view.SetValueCore(VisualElement.IsFocusedPropertyKey, true);

			bool fired = false;
			view.Unfocused += (sender, args) => fired = true;
			view.SetValueCore(VisualElement.IsFocusedPropertyKey, false);

			Assert.True(fired);
		}

		[Test]
		public void TestOpenUriAction()
		{
			var uri = new Uri("http://www.xamarin.com/");
			var invoked = false;
			Device.PlatformServices = new MockPlatformServices(openUriAction: u =>
			{
				Assert.AreSame(uri, u);
				invoked = true;
			});

			Device.OpenUri(uri);
			Assert.True(invoked);
		}

		[Test]
		public void OpenUriThrowsWhenNull()
		{
			Device.PlatformServices = null;
			var uri = new Uri("http://www.xamarin.com/");
			Assert.Throws<InvalidOperationException>(() => Device.OpenUri(uri));
		}

		[Test]
		public void MinimumWidthRequest()
		{
			var view = new View();

			bool signaled = false;
			view.MeasureInvalidated += (sender, args) => signaled = true;

			view.MinimumWidthRequest = 10;
			Assert.True(signaled);
			Assert.AreEqual(10, view.MinimumWidthRequest);

			signaled = false;
			view.MinimumWidthRequest = 10;
			Assert.False(signaled);
		}

		[Test]
		public void MinimumHeightRequest()
		{
			var view = new View();

			bool signaled = false;
			view.MeasureInvalidated += (sender, args) => signaled = true;

			view.MinimumHeightRequest = 10;
			Assert.True(signaled);
			Assert.AreEqual(10, view.MinimumHeightRequest);

			signaled = false;
			view.MinimumHeightRequest = 10;
			Assert.False(signaled);
		}

		[Test]
		public void MinimumWidthRequestInSizeRequest()
		{
			var view = new View
			{
				IsPlatformEnabled = true
			};

			view.HeightRequest = 20;
			view.WidthRequest = 200;
			view.MinimumWidthRequest = 100;

			var result = view.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
			Assert.AreEqual(new Size(200, 20), result.Request);
			Assert.AreEqual(new Size(100, 20), result.Minimum);
		}

		[Test]
		public void MinimumHeightRequestInSizeRequest()
		{
			var view = new View
			{
				IsPlatformEnabled = true
			};

			view.HeightRequest = 200;
			view.WidthRequest = 20;
			view.MinimumHeightRequest = 100;

			var result = view.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);
			Assert.AreEqual(new Size(20, 200), result.Request);
			Assert.AreEqual(new Size(20, 100), result.Minimum);
		}

		[Test]
		public void StartTimerSimple()
		{
			Device.PlatformServices = new MockPlatformServices();
			var task = new TaskCompletionSource<bool>();

			Task.Factory.StartNew(() => Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
			{
				task.SetResult(false);
				return false;
			}));

			task.Task.Wait();
			Assert.False(task.Task.Result);
			Device.PlatformServices = null;
		}

		[Test]
		public void StartTimerMultiple()
		{
			Device.PlatformServices = new MockPlatformServices();
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

			task.Task.Wait();
			Assert.AreEqual(2, task.Task.Result);
			Device.PlatformServices = null;
		}

		[Test]
		public void BindingsApplyAfterViewAddedToParentWithContextSet()
		{
			var parent = new NaiveLayout();
			parent.BindingContext = new MockViewModel { Text = "test" };

			var child = new Entry();
			child.SetBinding(Entry.TextProperty, new Binding("Text"));

			parent.Children.Add(child);

			Assert.That(child.BindingContext, Is.SameAs(parent.BindingContext));
			Assert.That(child.Text, Is.EqualTo("test"));
		}

		[Test]
		public void IdIsUnique()
		{
			var view1 = new View();
			var view2 = new View();

			Assert.True(view1.Id != view2.Id);
		}

		[Test]
		public void MockBounds()
		{
			var view = new View();
			view.Layout(new Rectangle(10, 20, 30, 40));

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

			view.MockBounds(new Rectangle(5, 10, 15, 20));

			Assert.AreEqual(new Rectangle(5, 10, 15, 20), view.Bounds);
			Assert.False(changed);

			view.UnmockBounds();

			Assert.AreEqual(new Rectangle(10, 20, 30, 40), view.Bounds);
			Assert.False(changed);
		}

		[Test]
		public void AddGestureRecognizer()
		{
			var view = new View();
			var gestureRecognizer = new TapGestureRecognizer();

			view.GestureRecognizers.Add(gestureRecognizer);

			Assert.True(view.GestureRecognizers.Contains(gestureRecognizer));
		}

		[Test]
		public void AddGestureRecognizerSetsParent()
		{
			var view = new View();
			var gestureRecognizer = new TapGestureRecognizer();

			view.GestureRecognizers.Add(gestureRecognizer);

			Assert.AreEqual(view, gestureRecognizer.Parent);
		}

		[Test]
		public void RemoveGestureRecognizerUnsetsParent()
		{
			var view = new View();
			var gestureRecognizer = new TapGestureRecognizer();

			view.GestureRecognizers.Add(gestureRecognizer);
			view.GestureRecognizers.Remove(gestureRecognizer);

			Assert.Null(gestureRecognizer.Parent);
		}

		[Test]
		public void WidthRequestEffectsGetSizeRequest()
		{
			var view = new View();
			view.IsPlatformEnabled = true;
			view.WidthRequest = 20;
			var request = view.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);

			Assert.AreEqual(new Size(20, 50), request.Request);
		}

		[Test]
		public void HeightRequestEffectsGetSizeRequest()
		{
			Device.PlatformServices = new MockPlatformServices(getNativeSizeFunc: (ve, widthConstraint, heightConstraint) =>
			{
				if (heightConstraint < 30)
					return new SizeRequest(new Size(40, 50));
				return new SizeRequest(new Size(20, 100));
			});

			var view = new View();
			view.IsPlatformEnabled = true;
			view.HeightRequest = 20;
			var request = view.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity);

			Assert.AreEqual(new Size(40, 20), request.Request);
		}

		[Test]
		public void TestClip()
		{
			var view = new View
			{
				HeightRequest = 100,
				WidthRequest = 100,

				Clip = new RectangleGeometry
				{
					Rect = new Rectangle(0, 0, 50, 50)
				}
			};

			Assert.NotNull(view.Clip);
		}

		[Test]
		public void TestRemoveClip()
		{
			var view = new View
			{
				HeightRequest = 100,
				WidthRequest = 100,

				Clip = new RectangleGeometry
				{
					Rect = new Rectangle(0, 0, 50, 50)
				}
			};

			Assert.NotNull(view.Clip);

			view.Clip = null;

			Assert.Null(view.Clip);
		}
	}
}