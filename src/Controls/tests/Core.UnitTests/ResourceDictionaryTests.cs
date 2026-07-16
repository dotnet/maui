using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ResourceDictionaryTests : BaseTestFixture
	{
		[Fact]
		public void Add()
		{
			var rd = new ResourceDictionary();
			rd.Add("foo", "bar");
			Assert.Equal("bar", rd["foo"]);
		}

		[Fact]
		public void AddKVP()
		{
			var rd = new ResourceDictionary();
			((ICollection<KeyValuePair<string, object>>)rd).Add(new KeyValuePair<string, object>("foo", "bar"));
			Assert.Equal("bar", rd["foo"]);
		}

		[Fact]
		public void ResourceDictionaryTriggersValueChangedOnAdd()
		{
			var rd = new ResourceDictionary();
			var passed = false;
			((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
			{
				Assert.Single(e.Values);
				var kvp = e.Values.First();
				Assert.Equal("foo", kvp.Key);
				Assert.Equal("FOO", kvp.Value);
				passed = true;
			};
			rd.Add("foo", "FOO");

			if (!passed)
			{
				throw new XunitException("Changing the value in the dictionary did not fire the changed event.");
			}
		}


		[Fact]
		public void ResourceDictionaryTriggersValueChangedOnChange()
		{
			var rd = new ResourceDictionary();
			rd.Add("foo", "FOO");
			var passed = false;
			((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
			{
				Assert.Single(e.Values);
				var kvp = e.Values.First();
				Assert.Equal("foo", kvp.Key);
				Assert.Equal("BAR", kvp.Value);
				passed = true;
			};
			rd["foo"] = "BAR";

			if (!passed)
			{
				throw new XunitException("Changing the value in the dictionary did not fire the changed event.");
			}
		}

		[Fact]
		public void ResourceDictionaryCtor()
		{
			var rd = new ResourceDictionary();
			Assert.Empty(rd);
		}

		[Fact]
		public void ElementMergesParentRDWithCurrent()
		{
			var elt = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "foo", "FOO" },
				}
			};

			elt.Parent = parent;

			object value;
			Assert.True(elt.TryGetResource("foo", out value));
			Assert.Equal("FOO", value);
			Assert.True(elt.TryGetResource("bar", out value));
			Assert.Equal("BAR", value);
		}

		[Fact]
		public void CurrentOverridesParentValues()
		{
			var elt = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "bar","BAZ" },
				}
			};

			var parent = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "foo", "FOO" },
					{ "bar","BAR" },
				}
			};

			elt.Parent = parent;

			object value;
			Assert.True(elt.TryGetResource("foo", out value));
			Assert.Equal("FOO", value);
			Assert.True(elt.TryGetResource("bar", out value));
			Assert.Equal("BAZ", value);
		}

		[Fact]
		public void AddingToParentTriggersValuesChanged()
		{
			var elt = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "foo", "FOO" },
				}
			};

			elt.Parent = parent;

			var passed = false;

			((IElementDefinition)elt).AddResourcesChangedListener((sender, e) =>
			{
				Assert.Single(e.Values);
				var kvp = e.Values.First();
				Assert.Equal("baz", kvp.Key);
				Assert.Equal("BAZ", kvp.Value);
				passed = true;
			});

			parent.Resources["baz"] = "BAZ";

			if (!passed)
			{
				throw new XunitException("Changing the value in the dictionary did not fire the changed event.");
			}
		}

		[Fact]
		public void ResourcesChangedNotRaisedIfKeyExistsInCurrent()
		{
			var elt = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "foo", "FOO" },
				}
			};

			elt.Parent = parent;

			((IElementDefinition)elt).AddResourcesChangedListener((sender, e) => throw new XunitException("Changing the value in the dictionary should not fire the changed event."));
			parent.Resources["bar"] = "BAZ";
		}

		[Fact]
		public void SettingParentTriggersValuesChanged()
		{
			var elt = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement
			{
				Resources = new ResourceDictionary {
					{"foo", "FOO"},
					{"baz", "BAZ"},
					{"bar", "NEWBAR"}
				}
			};

			var passed = false;

			((IElementDefinition)elt).AddResourcesChangedListener((sender, e) =>
			{
				Assert.Equal(2, e.Values.Count());
				Assert.Equal("FOO", e.Values.First(kvp => kvp.Key == "foo").Value);
				Assert.Equal("BAZ", e.Values.First(kvp => kvp.Key == "baz").Value);
				passed = true;
			});
			elt.Parent = parent;

			if (!passed)
			{
				throw new XunitException("Changing the value in the dictionary did not fire the changed event.");
			}
		}

		[Fact]
		public void SettingResourcesTriggersResourcesChanged()
		{
			var elt = new VisualElement();

			var parent = new VisualElement
			{
				Resources = new ResourceDictionary {
					{"bar", "BAR"}
				}
			};

			elt.Parent = parent;

			var passed = false;

			((IElementDefinition)elt).AddResourcesChangedListener((sender, e) =>
			{
				Assert.Equal(3, e.Values.Count());
				passed = true;
			});
			elt.Resources = new ResourceDictionary {
				{"foo", "FOO"},
				{"baz", "BAZ"},
				{"bar", "NEWBAR"}
			};

			if (!passed)
			{
				throw new XunitException("Changing the value in the dictionary did not fire the changed event.");
			}
		}

		[Fact]
		public void DontThrowOnReparenting()
		{
			var elt = new View { Resources = new ResourceDictionary() };
			var parent = new StackLayout();

			parent.Children.Add(elt);
			parent.Children.Remove(elt);
		}

		[Fact]
		public void MultiLevelMerge()
		{
			var elt = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement
			{
				Resources = new ResourceDictionary {
					{ "foo", "FOO" },
				},
				Parent = new VisualElement
				{
					Resources = new ResourceDictionary {
						{"baz", "BAZ"}
					}
				}
			};

			var passed = false;

			((IElementDefinition)elt).AddResourcesChangedListener((sender, e) =>
			{
				Assert.Equal(2, e.Values.Count());
				passed = true;
			});

			elt.Parent = parent;

			if (!passed)
			{
				throw new XunitException("Changing the value in the dictionary did not fire the changed event.");
			}
		}

		[Fact]
		public void FilteredMergedResourcesMatchFullSnapshotForOrdinaryResources()
		{
			var previousApplication = Application.Current;
			try
			{
				var app = new MockApplication
				{
					Resources = new ResourceDictionary {
						{ "app-only", "APP" },
						{ "duplicate", "APP-DUPLICATE" },
					}
				};
				var grandparent = new ContentView
				{
					Resources = new ResourceDictionary {
						{ "grandparent-only", "GRANDPARENT" },
						{ "duplicate", "GRANDPARENT-DUPLICATE" },
					},
					Parent = app,
				};
				var parent = new ContentView
				{
					Resources = new ResourceDictionary {
						new ResourceDictionary {
							{ "merged-only", "MERGED" },
							{ "duplicate", "MERGED-DUPLICATE" },
						},
						{ "parent-only", "PARENT" },
						{ "duplicate", "PARENT-DUPLICATE" },
					},
					Parent = grandparent,
				};

				AssertFilteredResourcesMatchFullSnapshot(parent, "merged-only", "parent-only", "grandparent-only", "app-only", "duplicate", "missing");
			}
			finally
			{
				Application.Current = previousApplication;
			}
		}

		[Fact]
		public void FilteredMergedResourcesMatchFullSnapshotForSystemResources()
		{
			var previousApplication = Application.Current;
			try
			{
				var app = new MockApplication();
				var parent = new ContentView { Parent = app };

				AssertFilteredResourcesMatchFullSnapshot(parent, Device.Styles.BodyStyleKey, Device.Styles.TitleStyleKey);
			}
			finally
			{
				Application.Current = previousApplication;
			}
		}

		[Fact]
		public void FilteredMergedResourcesMatchFullSnapshotForAppThemeResource()
		{
			var previousApplication = Application.Current;
			try
			{
				var app = new MockApplication();
				var parent = new ContentView
				{
					Resources = new ResourceDictionary {
						{ "before-theme", "BEFORE" },
						{ AppThemeBinding.AppThemeResource, AppTheme.Dark },
						{ "after-theme", "AFTER" },
					},
					Parent = app,
				};

				AssertFilteredResourcesMatchFullSnapshot(parent, "before-theme", AppThemeBinding.AppThemeResource, "after-theme");
			}
			finally
			{
				Application.Current = previousApplication;
			}
		}

		[Fact]
		public void FilteredMergedResourcesMatchFullSnapshotForStyleClassResources()
		{
			var previousApplication = Application.Current;
			try
			{
				var appStyle = new Style(typeof(Button)) { Class = "parity", ApplyToDerivedTypes = true };
				var grandparentStyle = new Style(typeof(Button)) { Class = "parity", ApplyToDerivedTypes = true };
				var parentMergedStyle = new Style(typeof(Button)) { Class = "parity", ApplyToDerivedTypes = true };
				var parentLocalStyle = new Style(typeof(Button)) { Class = "parity", ApplyToDerivedTypes = true };
				var app = new MockApplication
				{
					Resources = new ResourceDictionary { appStyle }
				};
				var grandparent = new ContentView
				{
					Resources = new ResourceDictionary { grandparentStyle },
					Parent = app,
				};
				var parent = new ContentView
				{
					Resources = new ResourceDictionary {
						new ResourceDictionary { parentMergedStyle },
						parentLocalStyle,
					},
					Parent = grandparent,
				};
				var key = Style.StyleClassPrefix + "parity";

				AssertFilteredResourcesMatchFullSnapshot(parent, key);
			}
			finally
			{
				Application.Current = previousApplication;
			}
		}

		[Fact]
		public void FilteredMergedResourcesReturnNullWhenNoRequestedKeysMatch()
		{
			var parent = new ContentView
			{
				Resources = new ResourceDictionary {
					{ "available", "AVAILABLE" },
				}
			};

			Assert.Null(((IElementDefinition)parent).GetMergedResourcesForKeys(new[] { "missing", "" }));
		}

		[Fact]
		public void ShowKeyInExceptionIfNotFound()
		{
			var rd = new ResourceDictionary();
			rd.Add("foo", "bar");
			var ex = Assert.Throws<KeyNotFoundException>(() => { var foo = rd["test_invalid_key"]; });
			Assert.Contains("test_invalid_key", ex.Message, StringComparison.InvariantCulture);
		}

		static void AssertFilteredResourcesMatchFullSnapshot(IElementDefinition element, params string[] keys)
		{
			var requestedKeys = new HashSet<string>(keys.Where(key => !string.IsNullOrEmpty(key)), StringComparer.Ordinal);
			var expected = (element.GetMergedResources() ?? Enumerable.Empty<KeyValuePair<string, object>>())
				.Where(resource => requestedKeys.Contains(resource.Key))
				.ToList();
			var actual = (element.GetMergedResourcesForKeys(keys) ?? Enumerable.Empty<KeyValuePair<string, object>>()).ToList();

			Assert.Equal(expected.Select(resource => resource.Key), actual.Select(resource => resource.Key));
			Assert.Equal(expected.Count, actual.Count);

			for (var i = 0; i < expected.Count; i++)
				AssertResourceValueEqual(expected[i].Value, actual[i].Value);
		}

		static void AssertResourceValueEqual(object expected, object actual)
		{
			if (expected is IList<Style> expectedStyles)
			{
				var actualStyles = Assert.IsAssignableFrom<IList<Style>>(actual);
				Assert.Equal(expectedStyles, actualStyles);
				return;
			}

			Assert.Equal(expected, actual);
		}

		class MyRD : ResourceDictionary
		{
			public MyRD()
			{
				CreationCount = CreationCount + 1;
				Add("foo", "Foo");
				Add("bar", "Bar");
			}

			public static int CreationCount { get; set; }
		}

		[Fact]
		public void ThrowOnDuplicateKey()
		{
			var rd0 = new ResourceDictionary();
			rd0.Add("foo", "Foo");
			try
			{
				rd0.Add("foo", "Bar");
			}
			catch (ArgumentException ae)
			{
				Assert.Equal("A resource with the key 'foo' is already present in the ResourceDictionary.", ae.Message);
			}
			catch (Exception ex)
			{
				throw new XunitException(ex.ToString());
			}
		}

		[Fact]
		public void MergedDictionaryResourcesAreFound()
		{
			var rd0 = new ResourceDictionary();
			rd0.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar" } });

			object value;
			Assert.True(rd0.TryGetValue("foo", out value));
			Assert.Equal("bar", value);
		}

		[Fact]
		public void MergedDictionaryResourcesAreFoundLastDictionaryTakesPriority()
		{
			var rd0 = new ResourceDictionary();
			rd0.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar" } });
			rd0.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar1" } });
			rd0.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar2" } });

			object value;
			Assert.True(rd0.TryGetValue("foo", out value));
			Assert.Equal("bar2", value);
		}

		[Fact]
		public void CountDoesNotIncludeMergedDictionaries()
		{
			var rd = new ResourceDictionary {
				{"baz", "Baz"},
				{"qux", "Qux"},
			};
			rd.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar" } });

			Assert.Equal(2, rd.Count);
		}

		[Fact]
		public void ClearMergedDictionaries()
		{
			var rd = new ResourceDictionary {
				{"baz", "Baz"},
				{"qux", "Qux"},
			};
			rd.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar" } });

			Assert.Equal(2, rd.Count);

			rd.MergedDictionaries.Clear();

			Assert.Empty(rd.MergedDictionaries);
		}

		[Fact]
		public void AddingMergedRDTriggersValueChanged()
		{
			var rd = new ResourceDictionary();
			var label = new Label
			{
				Resources = rd
			};
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(label.Text, Label.TextProperty.DefaultValue);

			rd.MergedDictionaries.Add(new ResourceDictionary { { "foo", "Foo" } });
			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		//this is to keep the alignment with resources removed from RD
		public void RemovingMergedRDDoesntTriggersValueChanged()
		{
			var rd = new ResourceDictionary
			{
				MergedDictionaries = {
					new ResourceDictionary {
						{ "foo", "Foo" }
					}
				}
			};
			var label = new Label
			{
				Resources = rd,
			};

			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal("Foo", label.Text);

			rd.MergedDictionaries.Clear();
			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void AddingResourceInMergedRDTriggersValueChanged()
		{
			var rd0 = new ResourceDictionary();
			var rd = new ResourceDictionary
			{
				MergedDictionaries = {
					rd0
				}
			};

			var label = new Label
			{
				Resources = rd,
			};
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(label.Text, Label.TextProperty.DefaultValue);

			rd0.Add("foo", "Foo");
			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void ReimplementedResourceDictionaryInterfaceStillPropagatesValueChanges()
		{
			var merged = new ReimplementedResourceDictionary();
			var resources = new ResourceDictionary
			{
				MergedDictionaries = { merged }
			};
			var label = new Label
			{
				Resources = resources,
			};
			label.SetDynamicResource(Label.TextProperty, "foo");

			merged.Add("foo", "Foo");

			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public async Task RootedMergedResourceDictionaryDoesNotLeakElement()
		{
			// A long-lived / shared ResourceDictionary rooted beyond the element, as in
			// the runtime-theming pattern (a static field, a singleton, or one instance
			// reused across pages via MergedDictionaries.Add). Merging it into a transient
			// element must not keep that element (and its subtree) alive for the lifetime
			// of the shared dictionary.
			var sharedRoot = new ResourceDictionary { { "primary", "#FF0000" } };

			var reference = CreateElementWithMergedDictionaryReference(sharedRoot);

			Assert.False(await reference.WaitForCollect(), "VisualElement should not be alive!");
			GC.KeepAlive(sharedRoot);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateElementWithMergedDictionaryReference(ResourceDictionary sharedRoot)
		{
			var element = new VisualElement();
			element.Resources.MergedDictionaries.Add(sharedRoot);
			return new WeakReference(element);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference CreateReaddedMergedDictionaryProxyReference(ResourceDictionary sharedRoot)
		{
			var resources = new ResourceDictionary();
			resources.MergedDictionaries.Add(new ResourceDictionary());
			resources.MergedDictionaries.Clear();
			resources.MergedDictionaries.Add(sharedRoot);

			var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
			var subscriptionsField = typeof(ResourceDictionary).GetField("_mergedDictionarySubscriptions", flags);
			Assert.NotNull(subscriptionsField);

			var subscriptions = subscriptionsField.GetValue(resources);
			Assert.NotNull(subscriptions);

			var mergedSubscriptionsField = subscriptions.GetType().GetField("_subscriptions", flags);
			Assert.NotNull(mergedSubscriptionsField);

			var mergedSubscriptions = Assert.IsAssignableFrom<System.Collections.IEnumerable>(
				mergedSubscriptionsField.GetValue(subscriptions));
			var subscription = Assert.Single(mergedSubscriptions.Cast<object>());

			return new WeakReference(subscription);
		}

		[Fact]
		public async Task ClearingThenReaddingMergedDictionaryRestoresFinalizerCleanup()
		{
			var sharedRoot = new ResourceDictionary();
			var proxyReference = CreateReaddedMergedDictionaryProxyReference(sharedRoot);

			Assert.False(
				await proxyReference.WaitForCollect(),
				"WeakResourcesChangedProxy should be collected after its owner is dropped.");

			GC.KeepAlive(sharedRoot);
		}

		[Fact]
		public async Task RootedMergedResourceDictionaryStillPropagatesToLiveElementAfterGc()
		{
			// The weak subscription must not sever change propagation for elements that are
			// still alive: after a GC (which could run the subscriptions helper's finalizer if
			// it were ever mistakenly collected while the owner lives), updating the shared
			// merged dictionary must still flow through to a live element.
			var sharedRoot = new ResourceDictionary { { "primary", "#FF0000" } };

			var element = new VisualElement();
			element.Resources.MergedDictionaries.Add(sharedRoot);

			bool changed = false;
			((IResourceDictionary)element.Resources).ValuesChanged += (_, __) => changed = true;

			// Force finalization to run; the live element (and therefore its subscriptions
			// helper) must survive and keep the weak-owner subscriptions wired up.
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			await Task.Yield();

			sharedRoot["secondary"] = "#00FF00";

			Assert.True(changed, "Live element should still receive merged dictionary changes after GC.");
			GC.KeepAlive(element);
		}

		[Fact]
		public void ClearingMergedDictionariesUnsubscribesChildren()
		{
			var removed = new ResourceDictionary();
			var resources = new ResourceDictionary
			{
				MergedDictionaries = { removed }
			};

			bool changed = false;
			((IResourceDictionary)resources).ValuesChanged += (_, __) => changed = true;

			resources.MergedDictionaries.Clear();
			changed = false;

			removed.Add("removed", "value");

			Assert.False(changed);
		}

		[Fact]
		public void RemovingMergedDictionaryOnlyUnsubscribesRemovedChild()
		{
			var removed = new ResourceDictionary();
			var retained = new ResourceDictionary();
			var resources = new ResourceDictionary
			{
				MergedDictionaries = { removed, retained }
			};

			bool changed = false;
			((IResourceDictionary)resources).ValuesChanged += (_, __) => changed = true;

			resources.MergedDictionaries.Remove(removed);
			changed = false;

			removed.Add("removed", "value");
			Assert.False(changed);

			retained.Add("retained", "value");
			Assert.True(changed);
		}

		[Fact]
		public void RemovingDuplicateMergedDictionariesBalancesSubscriptions()
		{
			var shared = new ResourceDictionary();
			var resources = new ResourceDictionary
			{
				MergedDictionaries = { shared, shared }
			};

			int changeCount = 0;
			((IResourceDictionary)resources).ValuesChanged += (_, __) => changeCount++;

			shared.Add("duplicate", "value");
			Assert.Equal(2, changeCount);

			resources.MergedDictionaries.Remove(shared);
			changeCount = 0;

			shared.Add("remaining", "value");
			Assert.Equal(1, changeCount);

			resources.MergedDictionaries.Remove(shared);
			changeCount = 0;

			shared.Add("removed", "value");
			Assert.Equal(0, changeCount);
		}

		[Fact]
		public void ReplacingMergedDictionaryMovesSubscriptionToReplacement()
		{
			var replaced = new ResourceDictionary();
			var retained = new ResourceDictionary();
			var replacement = new ResourceDictionary();
			var resources = new ResourceDictionary
			{
				MergedDictionaries = { replaced, retained }
			};
			var mergedDictionaries = Assert.IsAssignableFrom<IList<ResourceDictionary>>(resources.MergedDictionaries);

			bool changed = false;
			((IResourceDictionary)resources).ValuesChanged += (_, __) => changed = true;

			mergedDictionaries[0] = replacement;
			changed = false;

			replaced.Add("replaced", "value");
			Assert.False(changed);

			retained.Add("retained", "value");
			Assert.True(changed);

			changed = false;
			replacement.Add("replacement", "value");
			Assert.True(changed);
		}

		sealed class ReimplementedResourceDictionary : ResourceDictionary, IResourceDictionary
		{
			event EventHandler<ResourcesChangedEventArgs> IResourceDictionary.ValuesChanged
			{
				add { }
				remove { }
			}
		}
	}
}