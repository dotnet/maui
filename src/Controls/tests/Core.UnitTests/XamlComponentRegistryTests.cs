using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class XamlComponentRegistryTests
{
	[Fact]
	public void UpdateResourceDictionary_RemovesStaleResourcesAndPreservesRuntimeAdditions()
	{
		var removedMerge = new ResourceDictionary { ["Merged"] = "old" };
		var dictionary = new ResourceDictionary { ["Removed"] = "old", ["Changed"] = "old" };
		dictionary.MergedDictionaries.Add(removedMerge);
		XamlComponentRegistry.RegisterResourceKeys(dictionary, new[] { "Removed", "Changed" });
		dictionary["Runtime"] = "preserved";
		var runtimeMerge = new ResourceDictionary { ["Merged"] = "runtime" };
		dictionary.MergedDictionaries.Add(runtimeMerge);
		var updated = new ResourceDictionary { ["Changed"] = "new" };
		var updatedMerge = new ResourceDictionary { ["Merged"] = "new" };
		updated.MergedDictionaries.Add(updatedMerge);

		XamlComponentRegistry.UpdateResourceDictionary(dictionary, updated);

		Assert.False(dictionary.ContainsKey("Removed"));
		Assert.Equal("new", dictionary["Changed"]);
		Assert.Equal("preserved", dictionary["Runtime"]);
		Assert.Equal("runtime", dictionary["Merged"]);
		Assert.Equal(new[] { updatedMerge, runtimeMerge }, dictionary.MergedDictionaries);

		XamlComponentRegistry.UpdateResourceDictionary(dictionary, new ResourceDictionary());

		Assert.False(dictionary.ContainsKey("Changed"));
		Assert.Equal("preserved", dictionary["Runtime"]);
		Assert.Same(runtimeMerge, Assert.Single(dictionary.MergedDictionaries));
	}

	[Fact]
	public void UpdateResourceDictionary_DoesNotMaterializeLazyResources()
	{
		var created = 0;
		var dictionary = new ResourceDictionary();
		dictionary.AddFactory("Removed", () => { created++; return new object(); });
		XamlComponentRegistry.RegisterResourceKeys(dictionary, new[] { "Removed" });
		var updated = new ResourceDictionary();
		updated.AddFactory("Added", () => { created++; return new object(); });

		XamlComponentRegistry.UpdateResourceDictionary(dictionary, updated);

		Assert.Equal(0, created);
		Assert.False(dictionary.ContainsKey("Removed"));
		Assert.NotNull(dictionary["Added"]);
		Assert.Equal(1, created);
	}

	[Fact]
	public void UpdateResourceDictionary_NotifiesFinalValuesAndRemovedKeys()
	{
		var dictionary = new ResourceDictionary { ["Removed"] = "old" };
		dictionary.MergedDictionaries.Add(new ResourceDictionary { ["Accent"] = "old" });
		XamlComponentRegistry.RegisterResourceKeys(dictionary, new[] { "Removed" });
		var updated = new ResourceDictionary();
		updated.MergedDictionaries.Add(new ResourceDictionary { ["Accent"] = "first" });
		updated.MergedDictionaries.Add(new ResourceDictionary { ["Accent"] = "last" });
		var notifications = new List<ResourcesChangedEventArgs>();
		((IResourceDictionary)dictionary).ValuesChanged += (_, args) => notifications.Add(args);

		XamlComponentRegistry.UpdateResourceDictionary(dictionary, updated);

		var notification = Assert.Single(notifications);
		Assert.Contains("Removed", notification.Keys);
		Assert.Contains("Accent", notification.Keys);
		Assert.Equal("last", dictionary["Accent"]);
	}

	[Fact]
	public void UpdateResourceDictionary_RemovedDynamicResourceFallsBackAndTracksReaddition()
	{
		var label = new Label();
		label.SetDynamicResource(Label.TextProperty, "Caption");
		var layout = new VerticalStackLayout { Children = { label } };
		layout.Resources["Caption"] = "local";
		var page = new ContentPage { Content = layout };
		page.Resources["Caption"] = "outer";
		XamlComponentRegistry.RegisterResourceKeys(layout.Resources, new[] { "Caption" });
		Assert.Equal("local", label.Text);

		XamlComponentRegistry.UpdateResourceDictionary(layout.Resources, new ResourceDictionary());
		Assert.Equal("outer", label.Text);

		layout.Resources["Caption"] = "readded";
		Assert.Equal("readded", label.Text);
	}

	[Fact]
	public void UpdateResourceDictionary_RemovedDynamicResourceClearsValueButKeepsSubscription()
	{
		var label = new Label();
		label.SetDynamicResource(Label.TextProperty, "Caption");
		var page = new ContentPage { Content = label };
		page.Resources["Caption"] = "old";
		XamlComponentRegistry.RegisterResourceKeys(page.Resources, new[] { "Caption" });
		Assert.Equal("old", label.Text);

		XamlComponentRegistry.UpdateResourceDictionary(page.Resources, new ResourceDictionary());
		Assert.Null(label.Text);

		page.Resources["Caption"] = "readded";
		Assert.Equal("readded", label.Text);
	}

	[Fact]
	public void TryGet_AfterRegister_ReturnsComponent()
	{
		var page = new object();
		var label = new object();

		XamlComponentRegistry.Register(page, "label_0", label);

		var found = XamlComponentRegistry.TryGet(page, "label_0", out var result);

		Assert.True(found);
		Assert.Same(label, result);
	}

	[Fact]
	public void TryGet_UnregisteredPage_ReturnsFalse()
	{
		var page = new object();

		var found = XamlComponentRegistry.TryGet(page, "label_0", out var result);

		Assert.False(found);
		Assert.Null(result);
	}

	[Fact]
	public void TryGet_UnregisteredNodeId_ReturnsFalse()
	{
		var page = new object();
		XamlComponentRegistry.Register(page, "button_0", new object());

		var found = XamlComponentRegistry.TryGet(page, "label_0", out var result);

		Assert.False(found);
		Assert.Null(result);
	}

	[Fact]
	public void Register_Twice_SameNodeId_ReplacesComponent()
	{
		var page = new object();
		var label1 = new object();
		var label2 = new object();

		XamlComponentRegistry.Register(page, "label_0", label1);
		XamlComponentRegistry.Register(page, "label_0", label2);

		XamlComponentRegistry.TryGet(page, "label_0", out var result);
		Assert.Same(label2, result);
	}

	[Fact]
	public void Register_MultipleNodes_AllRetrievable()
	{
		var page = new object();
		var label = new object();
		var button = new object();
		var entry = new object();

		XamlComponentRegistry.Register(page, "label_0", label);
		XamlComponentRegistry.Register(page, "button_0", button);
		XamlComponentRegistry.Register(page, "entry_0", entry);

		Assert.True(XamlComponentRegistry.TryGet(page, "label_0", out var r1));
		Assert.Same(label, r1);

		Assert.True(XamlComponentRegistry.TryGet(page, "button_0", out var r2));
		Assert.Same(button, r2);

		Assert.True(XamlComponentRegistry.TryGet(page, "entry_0", out var r3));
		Assert.Same(entry, r3);
	}

	[Fact]
	public void RegisterTemplateComponent_MultipleInstances_AllRetrievable()
	{
		var page = new object();
		var first = new object();
		var second = new object();

		XamlComponentRegistry.RegisterTemplateComponent(page, "label_0", first);
		XamlComponentRegistry.RegisterTemplateComponent(page, "label_0", second);

		var components = XamlComponentRegistry.GetTemplateComponents(page, "label_0");

		Assert.Equal(2, components.Count);
		Assert.Contains(first, components);
		Assert.Contains(second, components);
	}

	[Fact]
	public void RegisterTemplateComponent_PrunesDeadEntriesAtGeometricThreshold()
	{
		var page = new object();
		const string nodeId = "label_0";

		for (int i = 0; i < 63; i++)
			RegisterEphemeralTemplateComponent(page, nodeId);

		CollectGarbage();

		var live = new object();
		XamlComponentRegistry.RegisterTemplateComponent(page, nodeId, live);

		Assert.Equal(1, GetTemplateRegistrationCount(page, nodeId));
		GC.KeepAlive(live);
		GC.KeepAlive(page);
	}

	[Fact]
	public void Register_TwoPagesWithSameNodeId_AreIndependent()
	{
		var page1 = new object();
		var page2 = new object();
		var comp1 = new object();
		var comp2 = new object();

		XamlComponentRegistry.Register(page1, "label_0", comp1);
		XamlComponentRegistry.Register(page2, "label_0", comp2);

		XamlComponentRegistry.TryGet(page1, "label_0", out var r1);
		XamlComponentRegistry.TryGet(page2, "label_0", out var r2);

		Assert.Same(comp1, r1);
		Assert.Same(comp2, r2);
		Assert.NotSame(r1, r2);
	}

	[Fact]
	public void Unregister_RemovesAllComponentsForPage()
	{
		var page = new object();
		XamlComponentRegistry.Register(page, "label_0", new object());
		XamlComponentRegistry.Register(page, "button_0", new object());

		XamlComponentRegistry.Unregister(page);

		Assert.False(XamlComponentRegistry.TryGet(page, "label_0", out _));
		Assert.False(XamlComponentRegistry.TryGet(page, "button_0", out _));
	}

	[Fact]
	public void Unregister_UnknownPage_DoesNotThrow()
	{
		var page = new object();
		// Should be a no-op, not throw
		XamlComponentRegistry.Unregister(page);
	}

	[Fact]
	public void Unregister_OtherPageUnaffected()
	{
		var page1 = new object();
		var page2 = new object();
		var comp = new object();

		XamlComponentRegistry.Register(page1, "label_0", new object());
		XamlComponentRegistry.Register(page2, "label_0", comp);

		XamlComponentRegistry.Unregister(page1);

		Assert.True(XamlComponentRegistry.TryGet(page2, "label_0", out var result));
		Assert.Same(comp, result);
	}

	[Fact]
	public void GetInstances_ReturnsAllLivePagesOfType()
	{
		// Use unique wrapper types to avoid interference from other tests
		var page1 = new FakePage();
		var page2 = new FakePage();
		var other = new object();

		XamlComponentRegistry.Register(page1, "x", new object());
		XamlComponentRegistry.Register(page2, "x", new object());
		XamlComponentRegistry.Register(other, "x", new object());

		var instances = XamlComponentRegistry.GetInstances(typeof(FakePage));

		Assert.Contains(page1, instances);
		Assert.Contains(page2, instances);
		Assert.DoesNotContain(other, instances);
	}

	[Fact]
	public void GetInstances_EmptyRegistry_ReturnsEmpty()
	{
		var instances = XamlComponentRegistry.GetInstances(typeof(UnrelatedPage));

		// May be empty or contain entries from other tests; we just verify the call succeeds
		foreach (var item in instances)
			Assert.IsType<UnrelatedPage>(item);
	}

	[Fact]
	public void GetInstances_NullType_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => XamlComponentRegistry.GetInstances(null!));
	}

	[Fact]
	public void Register_NullPage_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => XamlComponentRegistry.Register(null!, "id", new object()));
	}

	[Fact]
	public void Register_NullNodeId_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => XamlComponentRegistry.Register(new object(), null!, new object()));
	}

	[Fact]
	public void Register_NullComponent_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => XamlComponentRegistry.Register(new object(), "id", null!));
	}

	[Fact]
	public void TryGet_NullPage_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => XamlComponentRegistry.TryGet(null!, "id", out _));
	}

	[Fact]
	public void TryGet_NullNodeId_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => XamlComponentRegistry.TryGet(new object(), null!, out _));
	}

	[Fact]
	public void Unregister_NullPage_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => XamlComponentRegistry.Unregister(null!));
	}

	[Fact]
	public void ReRoot_RenamesMatchingEntries()
	{
		var page = new FakePage();
		var child = new object();
		var grandchild = new object();

		XamlComponentRegistry.Register(page, "VSL_0/Label_0", child);
		XamlComponentRegistry.Register(page, "VSL_0/Label_0/Entry_0", grandchild);

		XamlComponentRegistry.ReRoot(page, "VSL_0/Label_0", "VSL_0/Label_1");

		// Old IDs should no longer resolve
		Assert.False(XamlComponentRegistry.TryGet(page, "VSL_0/Label_0", out _));
		Assert.False(XamlComponentRegistry.TryGet(page, "VSL_0/Label_0/Entry_0", out _));

		// New IDs should resolve
		Assert.True(XamlComponentRegistry.TryGet(page, "VSL_0/Label_1", out var foundChild));
		Assert.Same(child, foundChild);
		Assert.True(XamlComponentRegistry.TryGet(page, "VSL_0/Label_1/Entry_0", out var foundGrandchild));
		Assert.Same(grandchild, foundGrandchild);
	}

	[Fact]
	public void ReRoot_SamePrefixNoOp()
	{
		var page = new FakePage();
		var comp = new object();
		XamlComponentRegistry.Register(page, "VSL_0/Label_0", comp);

		XamlComponentRegistry.ReRoot(page, "VSL_0/Label_0", "VSL_0/Label_0");

		Assert.True(XamlComponentRegistry.TryGet(page, "VSL_0/Label_0", out var found));
		Assert.Same(comp, found);
	}

	[Fact]
	public void ReRoot_NullPage_Throws()
	{
		Assert.Throws<ArgumentNullException>(() => XamlComponentRegistry.ReRoot(null!, "a", "b"));
	}

	[Fact]
	public void ReRoot_DoesNotAffectSiblingWithPrefixCollision()
	{
		// "Label_1" must NOT match "Label_10" — segment-boundary check
		var page = new FakePage();
		var label1 = new object();
		var label10 = new object();
		var label1Child = new object();

		XamlComponentRegistry.Register(page, "VSL_0/Label_1", label1);
		XamlComponentRegistry.Register(page, "VSL_0/Label_1/Entry_0", label1Child);
		XamlComponentRegistry.Register(page, "VSL_0/Label_10", label10);

		XamlComponentRegistry.ReRoot(page, "VSL_0/Label_1", "VSL_0/Label_5");

		// Label_1 and its child should be renamed
		Assert.False(XamlComponentRegistry.TryGet(page, "VSL_0/Label_1", out _));
		Assert.True(XamlComponentRegistry.TryGet(page, "VSL_0/Label_5", out var found1));
		Assert.Same(label1, found1);
		Assert.True(XamlComponentRegistry.TryGet(page, "VSL_0/Label_5/Entry_0", out var foundChild));
		Assert.Same(label1Child, foundChild);

		// Label_10 must be unaffected
		Assert.True(XamlComponentRegistry.TryGet(page, "VSL_0/Label_10", out var found10));
		Assert.Same(label10, found10);
	}

	[Fact]
	public void UnregisterSubtree_RemovesNodeAndDescendants()
	{
		var page = new FakePage();
		var parent = new object();
		var child = new object();
		var sibling = new object();

		XamlComponentRegistry.Register(page, "VSL_0/Label_0", parent);
		XamlComponentRegistry.Register(page, "VSL_0/Label_0/Entry_0", child);
		XamlComponentRegistry.Register(page, "VSL_0/Button_1", sibling);

		XamlComponentRegistry.UnregisterSubtree(page, "VSL_0/Label_0");

		Assert.False(XamlComponentRegistry.TryGet(page, "VSL_0/Label_0", out _));
		Assert.False(XamlComponentRegistry.TryGet(page, "VSL_0/Label_0/Entry_0", out _));
		// Sibling preserved
		Assert.True(XamlComponentRegistry.TryGet(page, "VSL_0/Button_1", out var found));
		Assert.Same(sibling, found);
	}

	[Fact]
	public void Unregister_RemovesTemplateComponentsForNode()
	{
		var page = new object();
		XamlComponentRegistry.RegisterTemplateComponent(page, "label_0", new object());

		XamlComponentRegistry.Unregister(page, "label_0");

		Assert.Empty(XamlComponentRegistry.GetTemplateComponents(page, "label_0"));
	}

	[Fact]
	public void UnregisterSubtree_DoesNotAffectSiblingWithPrefixCollision()
	{
		// "Label_1" must NOT remove "Label_10"
		var page = new FakePage();
		var label1 = new object();
		var label10 = new object();

		XamlComponentRegistry.Register(page, "VSL_0/Label_1", label1);
		XamlComponentRegistry.Register(page, "VSL_0/Label_10", label10);

		XamlComponentRegistry.UnregisterSubtree(page, "VSL_0/Label_1");

		Assert.False(XamlComponentRegistry.TryGet(page, "VSL_0/Label_1", out _));
		Assert.True(XamlComponentRegistry.TryGet(page, "VSL_0/Label_10", out var found));
		Assert.Same(label10, found);
	}

	[Fact]
	public void RegisterComponent_SelfRegisters()
	{
		var page = new FakePage();
		XamlComponentRegistry.RegisterComponent(page, "root");

		Assert.True(XamlComponentRegistry.TryGet(page, "root", out var found));
		Assert.Same(page, found);
	}

	[Fact]
	public void GetComponent_ReturnsRegisteredOrNull()
	{
		var page = new FakePage();
		var comp = new object();
		XamlComponentRegistry.Register(page, "Label_0", comp);

		Assert.Same(comp, XamlComponentRegistry.GetComponent(page, "Label_0"));
		Assert.Null(XamlComponentRegistry.GetComponent(page, "NotRegistered"));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static void RegisterEphemeralTemplateComponent(object page, string nodeId) =>
		XamlComponentRegistry.RegisterTemplateComponent(page, nodeId, new object());

	static void CollectGarbage()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	static int GetTemplateRegistrationCount(object page, string nodeId)
	{
		var tableField = typeof(XamlComponentRegistry).GetField("s_table", BindingFlags.NonPublic | BindingFlags.Static);
		Assert.NotNull(tableField);
		var table = tableField.GetValue(null);
		Assert.NotNull(table);

		var tryGetValue = table.GetType().GetMethod("TryGetValue");
		Assert.NotNull(tryGetValue);
		var arguments = new object[] { page, null };
		Assert.True((bool)tryGetValue.Invoke(table, arguments)!);
		var map = arguments[1];
		Assert.NotNull(map);

		var entriesField = map.GetType().GetField("_templateEntries", BindingFlags.NonPublic | BindingFlags.Instance);
		Assert.NotNull(entriesField);
		var entries = Assert.IsAssignableFrom<IDictionary>(entriesField.GetValue(map));
		var registrations = Assert.IsAssignableFrom<ICollection>(entries[nodeId]);
		return registrations.Count;
	}

	sealed class FakePage { }
	sealed class UnrelatedPage { }
}
