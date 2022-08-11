using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class MockBehavior<T> : Behavior<T> where T : BindableObject
	{
		public static int AttachCount { get; set; }
		public bool attached;
		public bool detached;

		protected override void OnAttachedTo(BindableObject bindable)
		{
			base.OnAttachedTo(bindable);
			attached = true;
			AttachCount++;
			AssociatedObject = bindable;
		}

		protected override void OnDetachingFrom(BindableObject bindable)
		{
			AttachCount--;
			detached = true;
			base.OnDetachingFrom(bindable);
			AssociatedObject = null;
		}

		public BindableObject AssociatedObject { get; set; }
	}


	public class BehaviorTest : BaseTestFixture
	{
		[Fact]
		public void AttachAndDetach()
		{
			var behavior = new MockBehavior<MockBindable>();
			var bindable = new MockBindable();

			Assert.False(behavior.attached);
			Assert.False(behavior.detached);
			Assert.Null(behavior.AssociatedObject);

			((IAttachedObject)behavior).AttachTo(bindable);

			Assert.True(behavior.attached);
			Assert.False(behavior.detached);
			Assert.Same(bindable, behavior.AssociatedObject);

			((IAttachedObject)behavior).DetachFrom(bindable);

			Assert.True(behavior.attached);
			Assert.True(behavior.detached);
			Assert.Null(behavior.AssociatedObject);
		}

		[Fact]
		public void AttachToTypeCompatibleWithTargetType()
		{
			var behavior = new MockBehavior<MockBindable>();
			var bindable = new View();

			Assert.Throws<InvalidOperationException>(() => ((IAttachedObject)behavior).AttachTo(bindable));
		}

		[Fact]
		public void BehaviorsInCollectionAreAttachedWhenCollectionIsAttached()
		{
			var behavior = new MockBehavior<MockBindable>();
			var collection = new AttachedCollection<Behavior>();
			var bindable = new MockBindable();
			collection.Add(behavior);
			Assert.Null(behavior.AssociatedObject);

			((IAttachedObject)collection).AttachTo(bindable);
			Assert.Same(bindable, behavior.AssociatedObject);

			((IAttachedObject)collection).DetachFrom(bindable);
			Assert.Null(behavior.AssociatedObject);
		}

		[Fact]
		public void BehaviorsAddedToAttachedCollectionAreAttached()
		{
			var behavior = new MockBehavior<MockBindable>();
			var collection = new AttachedCollection<Behavior>();
			var bindable = new MockBindable();
			((IAttachedObject)collection).AttachTo(bindable);
			Assert.Null(behavior.AssociatedObject);

			collection.Add(behavior);
			Assert.Same(bindable, behavior.AssociatedObject);

			collection.Remove(behavior);
			Assert.Null(behavior.AssociatedObject);
		}

		[Fact]
		public void TestBehaviorsAttachedDP()
		{
			var behavior = new MockBehavior<MockBindable>();
			var bindable = new MockBindable();
			var collection = bindable.Behaviors;
			Assert.Null(behavior.AssociatedObject);

			collection.Add(behavior);
			Assert.Same(bindable, behavior.AssociatedObject);

			collection.Remove(behavior);
			Assert.Null(behavior.AssociatedObject);
		}
	}
}