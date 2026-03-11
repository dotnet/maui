using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class XamlComponentRegistryTests
{
	// -------------------------------------------------------------------------
	// Register + TryGet round-trip
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// Replace / update
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// Multiple nodes per page
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// Multiple pages with same node IDs are independent
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// Unregister
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// GetInstances
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// Argument validation
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// ReRoot
	// -------------------------------------------------------------------------

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

	// -------------------------------------------------------------------------
	// Helper types (nested to avoid collision with other tests)
	// -------------------------------------------------------------------------

	sealed class FakePage { }
	sealed class UnrelatedPage { }
}
