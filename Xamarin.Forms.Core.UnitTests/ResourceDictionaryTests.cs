using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ResourceDictionaryTests : BaseTestFixture
	{
		[Test]
		public void Add()
		{
			var rd = new ResourceDictionary();
			rd.Add("foo", "bar");
			Assert.AreEqual("bar", rd["foo"]);
		}

		[Test]
		public void AddKVP()
		{
			var rd = new ResourceDictionary();
			((ICollection<KeyValuePair<string, object>>)rd).Add(new KeyValuePair<string, object>("foo", "bar"));
			Assert.AreEqual("bar", rd["foo"]);
		}

		[Test]
		public void ResourceDictionaryTriggersValueChangedOnAdd()
		{
			var rd = new ResourceDictionary();
			((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
			{
				Assert.AreEqual(1, e.Values.Count());
				var kvp = e.Values.First();
				Assert.AreEqual("foo", kvp.Key);
				Assert.AreEqual("FOO", kvp.Value);
				Assert.Pass();
			};
			rd.Add("foo", "FOO");
			Assert.Fail();
		}

		[Test]
		public void ResourceDictionaryTriggersValueChangedOnChange()
		{
			var rd = new ResourceDictionary();
			rd.Add("foo", "FOO");
			((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
			{
				Assert.AreEqual(1, e.Values.Count());
				var kvp = e.Values.First();
				Assert.AreEqual("foo", kvp.Key);
				Assert.AreEqual("BAR", kvp.Value);
				Assert.Pass();
			};
			rd["foo"] = "BAR";
			Assert.Fail();
		}

		[Test]
		public void ResourceDictionaryCtor()
		{
			var rd = new ResourceDictionary();
			Assert.AreEqual(0, rd.Count());
		}

		[Test]
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
			Assert.AreEqual("FOO", value);
			Assert.True(elt.TryGetResource("bar", out value));
			Assert.AreEqual("BAR", value);
		}

		[Test]
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
			Assert.AreEqual("FOO", value);
			Assert.True(elt.TryGetResource("bar", out value));
			Assert.AreEqual("BAZ", value);
		}

		[Test]
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

			((IElement)elt).AddResourcesChangedListener((sender, e) =>
			{
				Assert.AreEqual(1, e.Values.Count());
				var kvp = e.Values.First();
				Assert.AreEqual("baz", kvp.Key);
				Assert.AreEqual("BAZ", kvp.Value);
				Assert.Pass();
			});

			parent.Resources["baz"] = "BAZ";
			Assert.Fail();
		}

		[Test]
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

			((IElement)elt).AddResourcesChangedListener((sender, e) => Assert.Fail());
			parent.Resources["bar"] = "BAZ";
			Assert.Pass();
		}

		[Test]
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

			((IElement)elt).AddResourcesChangedListener((sender, e) =>
			{
				Assert.AreEqual(2, e.Values.Count());
				Assert.AreEqual("FOO", e.Values.First(kvp => kvp.Key == "foo").Value);
				Assert.AreEqual("BAZ", e.Values.First(kvp => kvp.Key == "baz").Value);
				Assert.Pass();
			});
			elt.Parent = parent;
			Assert.Fail();
		}

		[Test]
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

			((IElement)elt).AddResourcesChangedListener((sender, e) =>
			{
				Assert.AreEqual(3, e.Values.Count());
				Assert.Pass();
			});
			elt.Resources = new ResourceDictionary {
				{"foo", "FOO"},
				{"baz", "BAZ"},
				{"bar", "NEWBAR"}
			};
			Assert.Fail();
		}

		[Test]
		public void DontThrowOnReparenting()
		{
			var elt = new View { Resources = new ResourceDictionary() };
			var parent = new StackLayout();

			parent.Children.Add(elt);
			Assert.DoesNotThrow(() => parent.Children.Remove(elt));
		}

		[Test]
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

			((IElement)elt).AddResourcesChangedListener((sender, e) =>
			{
				Assert.AreEqual(2, e.Values.Count());
				Assert.Pass();
			});

			elt.Parent = parent;
			Assert.Fail();
		}

		[Test]
		public void ShowKeyInExceptionIfNotFound()
		{
			var rd = new ResourceDictionary();
			rd.Add("foo", "bar");
			var ex = Assert.Throws<KeyNotFoundException>(() => { var foo = rd["test_invalid_key"]; });
			Assert.That(ex.Message, Does.Contain("test_invalid_key"));
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

		[Test]
		public void MergedWithFailsToMergeAnythingButRDs()
		{
			var rd = new ResourceDictionary();
			Assert.DoesNotThrow(() => rd.MergedWith = typeof(MyRD));
			Assert.Throws<ArgumentException>(() => rd.MergedWith = typeof(ContentPage));
		}

		[Test]
		public void MergedResourcesAreFound()
		{
			var rd0 = new ResourceDictionary();
			rd0.MergedWith = typeof(MyRD);

			object _;
			Assert.True(rd0.TryGetValue("foo", out _));
			Assert.AreEqual("Foo", _);
		}

		[Test]
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
				Assert.AreEqual("A resource with the key 'foo' is already present in the ResourceDictionary.", ae.Message);
				Assert.Pass();
			}
			Assert.Fail();
		}

		[Test]
		public void ContainsReturnsValuesForMergedRD()
		{
			var rd = new ResourceDictionary {
				{"baz", "BAZ"},
				{"qux", "QUX"},
			};
			rd.MergedWith = typeof(MyRD);

			Assert.That(rd.Contains(new KeyValuePair<string, object>("foo", "Foo")), Is.True);
		}

		[Test]
		public void CountDoesNotIncludeMerged()
		{
			var rd = new ResourceDictionary {
				{"baz", "Baz"},
				{"qux", "Qux"},
			};
			rd.MergedWith = typeof(MyRD);

			Assert.That(rd.Count, Is.EqualTo(2));
		}

		[Test]
		public void IndexerLookupInMerged()
		{
			var rd = new ResourceDictionary {
				{"baz", "BAZ"},
				{"qux", "QUX"},
			};
			rd.MergedWith = typeof(MyRD);

			Assert.That(() => rd["foo"], Throws.Nothing);
			Assert.That(rd["foo"], Is.EqualTo("Foo"));
		}

		[Test]
		public void TryGetValueLookupInMerged()
		{
			var rd = new ResourceDictionary {
				{"baz", "BAZ"},
				{"qux", "QUX"},
			};
			rd.MergedWith = typeof(MyRD);

			object _;
			Assert.That(rd.TryGetValue("foo", out _), Is.True);
			Assert.That(rd.TryGetValue("baz", out _), Is.True);
		}

		[Test]
		public void MergedDictionaryResourcesAreFound()
		{
			var rd0 = new ResourceDictionary();
			rd0.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar" } });

			object value;
			Assert.True(rd0.TryGetValue("foo", out value));
			Assert.AreEqual("bar", value);
		}

		[Test]
		public void MergedDictionaryResourcesAreFoundLastDictionaryTakesPriority()
		{
			var rd0 = new ResourceDictionary();
			rd0.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar" } });
			rd0.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar1" } });
			rd0.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar2" } });

			object value;
			Assert.True(rd0.TryGetValue("foo", out value));
			Assert.AreEqual("bar2", value);
		}

		[Test]
		public void CountDoesNotIncludeMergedDictionaries()
		{
			var rd = new ResourceDictionary {
				{"baz", "Baz"},
				{"qux", "Qux"},
			};
			rd.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar" } });

			Assert.That(rd.Count, Is.EqualTo(2));
		}

		[Test]
		public void ClearMergedDictionaries()
		{
			var rd = new ResourceDictionary {
				{"baz", "Baz"},
				{"qux", "Qux"},
			};
			rd.MergedDictionaries.Add(new ResourceDictionary() { { "foo", "bar" } });

			Assert.That(rd.Count, Is.EqualTo(2));

			rd.MergedDictionaries.Clear();

			Assert.That(rd.MergedDictionaries.Count, Is.EqualTo(0));
		}

		[Test]
		public void AddingMergedRDTriggersValueChanged()
		{
			var rd = new ResourceDictionary();
			var label = new Label
			{
				Resources = rd
			};
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.That(label.Text, Is.EqualTo(Label.TextProperty.DefaultValue));

			rd.MergedDictionaries.Add(new ResourceDictionary { { "foo", "Foo" } });
			Assert.That(label.Text, Is.EqualTo("Foo"));
		}

		[Test]
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
			Assert.That(label.Text, Is.EqualTo("Foo"));

			rd.MergedDictionaries.Clear();
			Assert.That(label.Text, Is.EqualTo("Foo"));
		}

		[Test]
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
			Assert.That(label.Text, Is.EqualTo(Label.TextProperty.DefaultValue));

			rd0.Add("foo", "Foo");
			Assert.That(label.Text, Is.EqualTo("Foo"));
		}
	}
}