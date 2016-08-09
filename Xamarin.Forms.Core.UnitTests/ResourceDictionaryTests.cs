using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ResourceDictionaryTests : BaseTestFixture
	{
		[Test]
		public void Add ()
		{
			var rd = new ResourceDictionary ();
			rd.Add ("foo", "bar");
			Assert.AreEqual ("bar", rd ["foo"]);
		}

		[Test]
		public void AddKVP ()
		{
			var rd = new ResourceDictionary ();
			((ICollection<KeyValuePair<string,object>>)rd).Add (new KeyValuePair<string,object> ("foo", "bar"));
			Assert.AreEqual ("bar", rd ["foo"]);
		}

		[Test]
		public void ResourceDictionaryTriggersValueChangedOnAdd ()
		{
			var rd = new ResourceDictionary ();
			((IResourceDictionary)rd).ValuesChanged += (sender, e) => {
				Assert.AreEqual (1, e.Values.Count());
				var kvp = e.Values.First();
				Assert.AreEqual ("foo", kvp.Key);
				Assert.AreEqual ("FOO", kvp.Value);
				Assert.Pass ();
			};
			rd.Add ("foo", "FOO");
			Assert.Fail ();
		}

		[Test]
		public void ResourceDictionaryTriggersValueChangedOnChange ()
		{
			var rd = new ResourceDictionary ();
			rd.Add ("foo", "FOO");
			((IResourceDictionary)rd).ValuesChanged += (sender, e) => {
				Assert.AreEqual (1, e.Values.Count());
				var kvp = e.Values.First();
				Assert.AreEqual ("foo", kvp.Key);
				Assert.AreEqual ("BAR", kvp.Value);
				Assert.Pass ();
			};
			rd ["foo"] = "BAR";
			Assert.Fail ();
		}

		[Test]
		public void ResourceDictionaryCtor ()
		{
			var rd = new ResourceDictionary ();
			Assert.AreEqual (0, rd.Count());
		}

		[Test]
		public void ElementMergesParentRDWithCurrent ()
		{
			var elt = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "foo", "FOO" },
				}
			};

			elt.Parent = parent;

			object value;
			Assert.True (elt.TryGetResource ("foo", out value));
			Assert.AreEqual ("FOO", value);
			Assert.True (elt.TryGetResource ("bar", out value));
			Assert.AreEqual ("BAR", value);
		}

		[Test]
		public void CurrentOverridesParentValues ()
		{
			var elt = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "bar","BAZ" },
				}
			};

			var parent = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "foo", "FOO" },
					{ "bar","BAR" },
				}
			};

			elt.Parent = parent;

			object value;
			Assert.True (elt.TryGetResource ("foo", out value));
			Assert.AreEqual ("FOO", value);
			Assert.True (elt.TryGetResource ("bar", out value));
			Assert.AreEqual ("BAZ", value);
		}

		[Test]
		public void AddingToParentTriggersValuesChanged ()
		{
			var elt = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "foo", "FOO" },
				}
			};

			elt.Parent = parent;

			((IElement)elt).AddResourcesChangedListener ((sender, e) => {
				Assert.AreEqual (1, e.Values.Count ());
				var kvp = e.Values.First ();
				Assert.AreEqual ("baz", kvp.Key);
				Assert.AreEqual ("BAZ", kvp.Value);
				Assert.Pass ();
			});

			parent.Resources ["baz"] = "BAZ";
			Assert.Fail ();
		}

		[Test]
		public void ResourcesChangedNotRaisedIfKeyExistsInCurrent ()
		{
			var elt = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "foo", "FOO" },
				}
			};

			elt.Parent = parent;

			((IElement)elt).AddResourcesChangedListener ((sender, e) => Assert.Fail ());
			parent.Resources ["bar"] = "BAZ";
			Assert.Pass ();
		}

		[Test]
		public void SettingParentTriggersValuesChanged ()
		{
			var elt = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement {
				Resources = new ResourceDictionary { 
					{"foo", "FOO"},
					{"baz", "BAZ"},
					{"bar", "NEWBAR"}
				}
			};

			((IElement)elt).AddResourcesChangedListener ((sender, e) => {
				Assert.AreEqual (2, e.Values.Count ());
				Assert.AreEqual ("FOO", e.Values.First (kvp => kvp.Key == "foo").Value);
				Assert.AreEqual ("BAZ", e.Values.First (kvp => kvp.Key == "baz").Value);
				Assert.Pass ();
			});
			elt.Parent = parent;
			Assert.Fail ();
		}

		[Test]
		public void SettingResourcesTriggersResourcesChanged ()
		{
			var elt = new VisualElement ();

			var parent = new VisualElement {
				Resources = new ResourceDictionary { 
					{"bar", "BAR"}
				}
			};

			elt.Parent = parent;

			((IElement)elt).AddResourcesChangedListener ((sender, e) => {
				Assert.AreEqual (3, e.Values.Count ());
				Assert.Pass ();
			});
			elt.Resources = new ResourceDictionary { 
				{"foo", "FOO"},
				{"baz", "BAZ"},
				{"bar", "NEWBAR"}
			};
			Assert.Fail();
		}

		[Test]
		public void DontThrowOnReparenting ()
		{
			var elt = new View { Resources = new ResourceDictionary () };
			var parent = new StackLayout ();

			parent.Children.Add (elt);
			Assert.DoesNotThrow (() => parent.Children.Remove (elt));
		}

		[Test]
		public void MultiLevelMerge ()
		{
			var elt = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "bar","BAR" },
				}
			};

			var parent = new VisualElement {
				Resources = new ResourceDictionary { 
					{ "foo", "FOO" },
				},
				Parent = new VisualElement {
					Resources = new ResourceDictionary { 
						{"baz", "BAZ"}
					}
				}
			};

			((IElement)elt).AddResourcesChangedListener ((sender, e) => {
				Assert.AreEqual (2, e.Values.Count ());
				Assert.Pass ();
			});

			elt.Parent = parent;
			Assert.Fail ();
		}

        [Test]
        public void ShowKeyInExceptionIfNotFound()
        {
            var rd = new ResourceDictionary();
            rd.Add("foo", "bar");
            var ex = Assert.Throws<KeyNotFoundException>(() => { var foo = rd["test_invalid_key"]; });
            Assert.That(ex.Message, Is.StringContaining("test_invalid_key"));
        }
    }
}