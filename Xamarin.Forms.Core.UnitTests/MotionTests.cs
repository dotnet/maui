using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	internal class BlockingTicker : Ticker
	{
		bool _enabled;

		protected override void EnableTimer()
		{
			_enabled = true;

			while (_enabled)
			{
				SendSignals(16);
			}
		}

		protected override void DisableTimer()
		{
			_enabled = false;
		}
	}

	internal class AsyncTicker : Ticker
	{
		bool _systemEnabled = true;
		public override bool SystemEnabled => _systemEnabled;

		bool _enabled;

		public void SetEnabled(bool enabled)
		{
			_systemEnabled = enabled;

			_enabled = enabled;

			OnSystemEnabledChanged();
		}

		protected override async void EnableTimer()
		{
			_enabled = true;

			while (_enabled)
			{
				SendSignals(16);
				await Task.Delay(16);
			}
		}

		protected override void DisableTimer()
		{
			_enabled = false;
		}
	}

	[TestFixture]
	public class MotionTests : BaseTestFixture
	{
		[OneTimeSetUp]
		public void Init()
		{
			Device.PlatformServices = new MockPlatformServices();
			Ticker.Default = new BlockingTicker();
		}

		[OneTimeTearDown]
		public void End()
		{
			Device.PlatformServices = null;
			Ticker.Default = null;
		}

		[Test]
		public void TestLinearTween()
		{
			var tweener = new Tweener(250);

			double value = 0;
			int updates = 0;
			tweener.ValueUpdated += (sender, args) =>
			{
				Assert.That(tweener.Value, Is.GreaterThanOrEqualTo(value));
				value = tweener.Value;
				updates++;
			};
			tweener.Start();

			Assert.That(updates, Is.GreaterThanOrEqualTo(10));
		}

		[Test]
		public void ThrowsWithNullCallback()
		{
			Assert.Throws<ArgumentNullException>(() => new View().Animate("Test", (Action<double>)null));
		}

		[Test]
		public void ThrowsWithNullTransform()
		{
			Assert.Throws<ArgumentNullException>(() => new View().Animate<float>("Test", null, f => { }));
		}

		[Test]
		public void ThrowsWithNullSelf()
		{
			Assert.Throws<ArgumentNullException>(() => AnimationExtensions.Animate(null, "Foo", d => (float)d, f => { }));
		}

		[Test]
		public void Kinetic()
		{
			var view = new View();
			var resultList = new List<Tuple<double, double>>();
			view.AnimateKinetic(
				name: "Kinetics",
				callback: (distance, velocity) =>
				{
					resultList.Add(new Tuple<double, double>(distance, velocity));
					return true;
				},
				velocity: 100,
				drag: 1);

			Assert.That(resultList, Is.Not.Empty);
			int checkVelo = 100;
			int dragStep = 16;

			foreach (var item in resultList)
			{
				checkVelo -= dragStep;
				Assert.AreEqual(checkVelo, item.Item2);
				Assert.AreEqual(checkVelo * dragStep, item.Item1);
			}
		}

		[Test]
		public void KineticFinished()
		{
			var view = new View();
			bool finished = false;
			view.AnimateKinetic(
				name: "Kinetics",
				callback: (distance, velocity) => true,
				velocity: 100,
				drag: 1,
				finished: () => finished = true);

			Assert.True(finished);
		}
	}

	[TestFixture]
	public class TickerSystemEnabledTests
	{
		[OneTimeSetUp]
		public void Init()
		{
			Device.PlatformServices = new MockPlatformServices();
			Ticker.Default = new AsyncTicker();
		}

		[OneTimeTearDown]
		public void End()
		{
			Device.PlatformServices = null;
			Ticker.Default = null;
		}

		static async Task DisableTicker()
		{
			await Task.Delay(32);
			((AsyncTicker)Ticker.Default).SetEnabled(false);
		}

		static async Task EnableTicker()
		{
			await Task.Delay(32);
			((AsyncTicker)Ticker.Default).SetEnabled(true);
		}

		async Task SwapFadeViews(View view1, View view2)
		{
			await view1.FadeTo(0, 1000);
			await view2.FadeTo(1, 1000);
		}

		[Test, Timeout(3000)]
		public async Task DisablingTickerFinishesAnimationInProgress()
		{
			var view = new View { Opacity = 1 };

			await Task.WhenAll(view.FadeTo(0, 2000), DisableTicker());

			Assert.That(view.Opacity, Is.EqualTo(0));
		}

		[Test, Timeout(3000)]
		public async Task DisablingTickerFinishesAllAnimationsInChain()
		{
			var view1 = new View { Opacity = 1 };
			var view2 = new View { Opacity = 0 };

			await Task.WhenAll(SwapFadeViews(view1, view2), DisableTicker());

			Assert.That(view1.Opacity, Is.EqualTo(0));

		}

		static Task<bool> RepeatFade(View view)
		{
			var tcs = new TaskCompletionSource<bool>();
			var fadeIn = new Animation(d => { view.Opacity = d; }, 0, 1);
			var i = 0;

			fadeIn.Commit(view, "fadeIn", length: 2000, repeat: () => ++i < 2, finished: (d, b) =>
			{
				tcs.SetResult(b);
			});

			return tcs.Task;
		}

		[Test, Timeout(3000)]
		public async Task DisablingTickerPreventsAnimationFromRepeating()
		{
			var view = new View { Opacity = 0 };

			await Task.WhenAll(RepeatFade(view), DisableTicker());

			Assert.That(view.Opacity, Is.EqualTo(1));
		}

		[Test]
		public async Task NewAnimationsFinishImmediatelyWhenTickerDisabled()
		{
			var view = new View { Opacity = 1 };

			await DisableTicker();

			await view.RotateYTo(200);

			Assert.That(view.RotationY, Is.EqualTo(200));
		}

		[Test]
		public async Task AnimationExtensionsReturnTrueIfAnimationsDisabled()
		{
			await DisableTicker();

			var label = new Label { Text = "Foo" };
			var result = await label.ScaleTo(2, 500);

			Assert.That(result, Is.True);
		}

		[Test, Timeout(2000)]
		public async Task CanExitAnimationLoopIfAnimationsDisabled()
		{
			await DisableTicker();

			var run = true;
			var label = new Label { Text = "Foo" };

			while (run)
			{
				await label.ScaleTo(2, 500);
				run = !(await label.ScaleTo(0.5, 500));
			}
		}

		[Test]
		public async Task CanCheckThatAnimationsAreEnabled()
		{
			await EnableTicker();
			Assert.That(Animation.IsEnabled, Is.True);

			await DisableTicker();
			Assert.That(Animation.IsEnabled, Is.False);
		}
	}
}