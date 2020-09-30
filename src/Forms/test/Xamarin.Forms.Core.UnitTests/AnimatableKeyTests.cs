using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class AnimatableKeyTests
	{
		class FakeAnimatable : IAnimatable
		{
			public void BatchBegin()
			{

			}

			public void BatchCommit()
			{

			}
		}

		[Test]
		public void KeysWithDifferentHandlesAreNotEqual()
		{
			var animatable = new FakeAnimatable();

			var key1 = new AnimatableKey(animatable, "handle1");
			var key2 = new AnimatableKey(animatable, "handle2");

			Assert.AreNotEqual(key1, key2);
		}

		[Test]
		public void KeysWithDifferentAnimatablesAreNotEqual()
		{
			var animatable1 = new FakeAnimatable();
			var animatable2 = new FakeAnimatable();

			var key1 = new AnimatableKey(animatable1, "handle");
			var key2 = new AnimatableKey(animatable2, "handle");

			Assert.AreNotEqual(key1, key2);
		}

		[Test]
		public void KeysWithSameAnimatableAndHandleAreEqual()
		{
			var animatable = new FakeAnimatable();

			var key1 = new AnimatableKey(animatable, "handle");
			var key2 = new AnimatableKey(animatable, "handle");

			Assert.AreEqual(key1, key2);
		}

		[Test]
		public void ThrowsWhenKeysWithSameAnimatableAdded()
		{
			var animatable = new FakeAnimatable();

			var key1 = new AnimatableKey(animatable, "handle");
			var key2 = new AnimatableKey(animatable, "handle");

			var dict = new Dictionary<AnimatableKey, object> { { key1, new object() } };

			Assert.Throws<ArgumentException>(() =>
			{
				var closureKey1 = key1;
				var closureKey2 = key1;
				var closureAnimatable = animatable;

				dict.Add(key2, new object());
			});
		}
	}
}