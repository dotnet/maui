using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// Every mutation path on VisualStateGroupList keeps its StatesChanged subscriptions in step
	// except the indexer, which replaced the element without touching either subscription.
	public class VisualStateGroupListIndexerTests : BaseTestFixture
	{
		// The subscription runs group -> list (VisualStateGroup.StatesChanged holds the list's
		// ValidateAndNotify), so a displaced group the app still holds keeps the whole list -- and the
		// element that owns it -- alive.
		// https://github.com/dotnet/maui/issues/37171
		[Fact]
		public async Task DisplacedGroupDoesNotRootTheList()
		{
			var displaced = CreateGroup("First");

			var reference = ReplaceAndTrackList(displaced);

			Assert.False(await reference.WaitForCollect(), "VisualStateGroupList should not be alive!");
			GC.KeepAlive(displaced);
		}

		[Fact]
		public void ReplacingByIndexSubscribesTheIncomingGroup()
		{
			var list = new VisualStateGroupList { CreateGroup("First") };
			var incoming = CreateGroup("Second");

			list[0] = incoming;

			// A subscribed group propagates its state changes; validation rejects a duplicate state
			// name, which only happens if the list is actually listening to the incoming group.
			incoming.States.Add(new VisualState { Name = "Normal" });

			Assert.Throws<InvalidOperationException>(() => incoming.States.Add(new VisualState { Name = "Normal" }));
			GC.KeepAlive(list);
		}

		[Fact]
		public void ReplacingByIndexUnsubscribesTheDisplacedGroup()
		{
			var displaced = CreateGroup("First");
			var list = new VisualStateGroupList { displaced };

			list[0] = CreateGroup("Second");

			// The displaced group is no longer validated by the list, so a duplicate name is allowed.
			displaced.States.Add(new VisualState { Name = "Normal" });
			displaced.States.Add(new VisualState { Name = "Normal" });

			Assert.Equal(3, displaced.States.Count);
			GC.KeepAlive(list);
		}

		[Fact]
		public void ReplacingWithTheSameInstanceIsANoOp()
		{
			var group = CreateGroup("First");
			var list = new VisualStateGroupList { group };

			list[0] = group;

			Assert.Same(group, list[0]);
			Assert.Single(list);
			GC.KeepAlive(list);
		}

		static VisualStateGroup CreateGroup(string name)
		{
			var group = new VisualStateGroup { Name = name };
			group.States.Add(new VisualState { Name = name + "State" });
			return group;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference ReplaceAndTrackList(VisualStateGroup displaced)
		{
			var list = new VisualStateGroupList { displaced };
			list[0] = CreateGroup("Second");
			return new WeakReference(list);
		}
	}
}
