using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal class BlockingTicker : Ticker
	{
		bool _enabled;

		public override void Start()
		{
			_enabled = true;

			while (_enabled)
			{
				Fire?.Invoke();
				Task.Delay(16).Wait();
			}
		}
		public override void Stop()
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

		}
		public override async void Start()
		{
			_enabled = true;

			while (_enabled)
			{
				Fire?.Invoke();
				await Task.Delay(16);
			}
		}
		public override void Stop()
		{
			_enabled = false;
		}
	}

	[TestFixture]
	public class MotionTests : BaseTestFixture
	{
		HandlerWithAnimationContextStub animationContext;

		[OneTimeSetUp]
		public void Init()
		{
			Device.PlatformServices = new MockPlatformServices();
			animationContext = new HandlerWithAnimationContextStub();
		}

		[OneTimeTearDown]
		public void End()
		{
			Device.PlatformServices = null;
			animationContext = null;
		}

		[Test]
		public void TestLinearTween()
		{
			var tweener = new Tweener(250, animationContext.AnimationManager);

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
			var view = new View()
			{
				Handler = animationContext,
			};
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
			var view = new View
			{
				Handler = animationContext,
			};
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
		HandlerWithAnimationContextStub animationContext;

		[OneTimeSetUp]
		public void Init()
		{
			Device.PlatformServices = new MockPlatformServices();
			animationContext = new HandlerWithAnimationContextStub(new TestAnimationManager(new AsyncTicker()));
		}

		[OneTimeTearDown]
		public void End()
		{
			Device.PlatformServices = null;
			animationContext = null;
		}

		async Task DisableTicker()
		{
			await Task.Delay(32);
			((AsyncTicker)animationContext.AnimationManager.Ticker).SetEnabled(false);
		}

		async Task EnableTicker()
		{
			await Task.Delay(32);
			((AsyncTicker)animationContext.AnimationManager.Ticker).SetEnabled(true);
		}

		async Task SwapFadeViews(View view1, View view2)
		{
			await view1.FadeTo(0, 1000);
			await view2.FadeTo(1, 1000);
		}

		[Test, Timeout(3000)]
		public async Task DisablingTickerFinishesAnimationInProgress()
		{
			var view = new View { Opacity = 1, Handler = animationContext };

			await Task.WhenAll(view.FadeTo(0, 2000), DisableTicker());

			Assert.That(view.Opacity, Is.EqualTo(0));
		}

		[Test, Timeout(3000)]
		public async Task DisablingTickerFinishesAllAnimationsInChain()
		{
			var view1 = new View { Opacity = 1, Handler = animationContext };
			var view2 = new View { Opacity = 0, Handler = animationContext };

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
			var view = new View { Opacity = 0, Handler = animationContext };

			await Task.WhenAll(RepeatFade(view), DisableTicker());

			Assert.That(view.Opacity, Is.EqualTo(1));
		}

		[Test]
		public async Task NewAnimationsFinishImmediatelyWhenTickerDisabled()
		{
			var view = new View { Opacity = 1, Handler = animationContext };

			await DisableTicker();

			await view.RotateYTo(200);

			Assert.That(view.RotationY, Is.EqualTo(200));
		}

		[Test]
		public async Task AnimationExtensionsReturnTrueIfAnimationsDisabled()
		{
			await DisableTicker();

			var label = new Label { Text = "Foo", Handler = animationContext };
			var result = await label.ScaleTo(2, 500);

			Assert.That(result, Is.True);
		}

		[Test, Timeout(2000)]
		public async Task CanExitAnimationLoopIfAnimationsDisabled()
		{
			await DisableTicker();

			var run = true;
			var label = new Label { Text = "Foo", Handler = animationContext };

			while (run)
			{
				await label.ScaleTo(2, 500);
				run = !(await label.ScaleTo(0.5, 500));
			}
		}
	}
}
