using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	internal class BlockingTicker : Ticker
	{
		bool _enabled;

		protected override void EnableTimer ()
		{
			_enabled = true;

			while (_enabled) {
				SendSignals (16);
			}
		}

		protected override void DisableTimer ()
		{
			_enabled = false;
		}
	}

	[TestFixture]
	public class MotionTests : BaseTestFixture
	{
		[TestFixtureSetUp]
		public void Init ()
		{
			Device.PlatformServices = new MockPlatformServices ();
			Ticker.Default = new BlockingTicker ();
		}

		[TestFixtureTearDown]
		public void End ()
		{
			Device.PlatformServices = null;
			Ticker.Default = null;
		}

		[Test]
		public void TestLinearTween ()
		{
			var tweener = new Tweener (250);

			double value = 0;
			int updates = 0;
			tweener.ValueUpdated += (sender, args) => {
				Assert.That (tweener.Value, Is.GreaterThanOrEqualTo (value));
				value = tweener.Value;
				updates++;
			};
			tweener.Start ();

			Assert.That (updates, Is.GreaterThanOrEqualTo (10));
		}

		[Test]
		public void ThrowsWithNullCallback ()
		{
			Assert.Throws<ArgumentNullException> (() => new View().Animate ("Test", (Action<double>) null));
		}

		[Test]
		public void ThrowsWithNullTransform ()
		{
			Assert.Throws<ArgumentNullException> (() => new View().Animate<float> ("Test", null, f => { }));
		}

		[Test]
		public void ThrowsWithNullSelf ()
		{
			Assert.Throws<ArgumentNullException> (() => AnimationExtensions.Animate (null, "Foo", d => (float)d, f => { }));
		}

		[Test]
		public void Kinetic ()
		{
			var view = new View ();
			var resultList = new List<Tuple<double, double>> ();
			view.AnimateKinetic (
				name: "Kinetics",
				callback: (distance, velocity) => {
					resultList.Add (new Tuple<double, double> (distance, velocity));
					return true;
				}, 
				velocity: 100, 
				drag: 1);

			Assert.That (resultList, Is.Not.Empty);
			int checkVelo = 100;
			int dragStep = 16;

			foreach (var item in resultList) {
				checkVelo -= dragStep;
				Assert.AreEqual (checkVelo, item.Item2);
				Assert.AreEqual (checkVelo * dragStep, item.Item1);
			}
		}

		[Test]
		public void KineticFinished ()
		{
			var view = new View ();
			bool finished = false;
			view.AnimateKinetic (
				name: "Kinetics",
				callback: (distance, velocity) => true, 
				velocity: 100, 
				drag: 1,
				finished: () => finished = true);

			Assert.True (finished);
		}
	}
}
