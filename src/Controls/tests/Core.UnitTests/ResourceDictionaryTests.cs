using System;
using System.Collections.Generic;
using System.Linq;
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
		public void ShowKeyInExceptionIfNotFound()
		{
			var rd = new ResourceDictionary();
			rd.Add("foo", "bar");
			var ex = Assert.Throws<KeyNotFoundException>(() => { var foo = rd["test_invalid_key"]; });
			Assert.Contains("test_invalid_key", ex.Message, StringComparison.InvariantCulture);
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
	}
}