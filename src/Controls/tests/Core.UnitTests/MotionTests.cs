using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class MotionTests : BaseTestFixture
	{
		[Fact]
		public void TestLinearTween()
		{
			var animations = new TestAnimationManager();
			var tweener = new Tweener(250, animations);

			double value = 0;
			int updates = 0;
			tweener.ValueUpdated += (sender, args) =>
			{
				Assert.True(tweener.Value >= (value));
				value = tweener.Value;
				updates++;
			};
			tweener.Start();

			Assert.True(updates >= (10));
		}

		[Fact]
		public void ZeroLengthTweenerFinishesOnFirstFrameWithoutNaN()
		{
			var tweener = new Tweener(0, new ZeroFirstFrameAnimationManager());
			var values = new List<double>();
			var finished = false;

			tweener.ValueUpdated += (sender, args) => values.Add(tweener.Value);
			tweener.Finished += (sender, args) => finished = true;
			tweener.Start();

			Assert.Equal(new[] { 1.0 }, values);
			Assert.True(finished);
		}

		[Theory]
		[InlineData(nameof(VisualElement.ScaleX))]
		[InlineData(nameof(VisualElement.ScaleY))]
		[InlineData(nameof(VisualElement.Scale))]
		public async Task ZeroLengthScaleAnimationNeverWritesNaN(string property)
		{
			var box = new BoxView();
			box.Handler = new AnimationReadyHandler<BlockingTicker>(new ZeroFirstFrameAnimationManager());
			Func<double> read = property switch
			{
				nameof(VisualElement.ScaleX) => () => box.ScaleX,
				nameof(VisualElement.ScaleY) => () => box.ScaleY,
				_ => () => box.Scale,
			};
			var written = new List<double>();
			box.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == property)
					written.Add(read());
			};

			var wasCanceled = await (property switch
			{
				nameof(VisualElement.ScaleX) => box.ScaleXToAsync(2, 0),
				nameof(VisualElement.ScaleY) => box.ScaleYToAsync(2, 0),
				_ => box.ScaleToAsync(2, 0),
			});

			Assert.False(wasCanceled);
			Assert.DoesNotContain(written, double.IsNaN);
			Assert.Equal(2d, written[^1]);
		}

		class ZeroFirstFrameAnimationManager : AnimationManager
		{
			bool _firstFrame = true;

			public ZeroFirstFrameAnimationManager()
				: base(new BlockingTicker())
			{
			}

			internal override double AdjustSpeed(double elapsedMilliseconds)
			{
				if (_firstFrame)
				{
					_firstFrame = false;
					return 0;
				}

				return 16;
			}
		}

		[Fact]
		public void ThrowsWithNullCallback()
		{
			Assert.Throws<ArgumentNullException>(() => new View().Animate("Test", (Action<double>)null));
		}

		[Fact]
		public void ThrowsWithNullTransform()
		{
			Assert.Throws<ArgumentNullException>(() => new View().Animate<float>("Test", null, f => { }));
		}

		[Fact]
		public void ThrowsWithNullSelf()
		{
			Assert.Throws<ArgumentNullException>(() => AnimationExtensions.Animate(null, "Foo", d => (float)d, f => { }));
		}

		[Fact]
		public void Kinetic()
		{
			var view = AnimationReadyHandler.Prepare(new View());
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

			Assert.NotEmpty(resultList);
			int checkVelo = 100;
			int dragStep = 16;

			foreach (var item in resultList)
			{
				checkVelo -= dragStep;
				Assert.Equal(checkVelo, item.Item2);
				Assert.Equal(checkVelo * dragStep, item.Item1);
			}
		}

		[Fact]
		public void KineticFinished()
		{
			var view = AnimationReadyHandler.Prepare(new View());
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
}