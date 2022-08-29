using System;
using System.Collections.Generic;
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