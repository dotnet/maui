using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	// A ResourceDictionary assigned to VisualElement.Resources is owned by the app and is routinely
	// shared -- merged into several elements, or held in App.Resources for the lifetime of the app.
	// A non-weak ValuesChanged subscription therefore roots every element it is assigned to.
	public class ResourcesWeakEventTests : BaseTestFixture
	{
		// https://github.com/dotnet/maui/issues/36389
		[Fact]
		public async Task SharedResourceDictionaryDoesNotRootElement()
		{
			var shared = new ResourceDictionary { { "accent", Colors.Red } };

			var reference = CreateElement(shared);

			Assert.False(await reference.WaitForCollect(), "VisualElement should not be alive!");
			GC.KeepAlive(shared);
		}

		// The same dictionary assigned to several elements must not root any of them.
		[Fact]
		public async Task ResourceDictionaryReusedAcrossElementsDoesNotRootThem()
		{
			var shared = new ResourceDictionary { { "accent", Colors.Red } };

			var first = CreateElement(shared);
			var second = CreateElement(shared);

			Assert.False(await first.WaitForCollect(), "First VisualElement should not be alive!");
			Assert.False(await second.WaitForCollect(), "Second VisualElement should not be alive!");
			GC.KeepAlive(shared);
		}

		// Guards that the weak subscription still delivers: DynamicResource resolution runs through
		// ValuesChanged, so this would break if the subscription were merely dropped.
		[Fact]
		public void DynamicResourceStillUpdatesWhenResourceChanges()
		{
			var resources = new ResourceDictionary();
			var label = new Label { Resources = resources };
			label.SetDynamicResource(Label.TextProperty, "greeting");

			resources["greeting"] = "hello";

			Assert.Equal("hello", label.Text);
			GC.KeepAlive(label);
		}

		[Fact]
		public void DynamicResourceStillUpdatesOnSubsequentChanges()
		{
			var resources = new ResourceDictionary { { "greeting", "hello" } };
			var label = new Label { Resources = resources };
			label.SetDynamicResource(Label.TextProperty, "greeting");

			Assert.Equal("hello", label.Text);

			resources["greeting"] = "goodbye";

			Assert.Equal("goodbye", label.Text);
			GC.KeepAlive(label);
		}

		// Replacing the dictionary must detach from the old one.
		[Fact]
		public void ReplacingResourcesDetachesFromOldDictionary()
		{
			var original = new ResourceDictionary { { "greeting", "hello" } };
			var label = new Label { Resources = original };
			label.SetDynamicResource(Label.TextProperty, "greeting");

			label.Resources = new ResourceDictionary { { "greeting", "replaced" } };
			Assert.Equal("replaced", label.Text);

			original["greeting"] = "ignored";

			Assert.Equal("replaced", label.Text);
			GC.KeepAlive(label);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateElement(ResourceDictionary resources) =>
			new WeakReference(new Label { Resources = resources });
	}
}
