using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
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

		#region Lazy ResourceDictionary (AddFactory) Tests

		[Fact]
		public void AddFactory_SharedTrue_InvokesFactoryOnce()
		{
			var rd = new ResourceDictionary();
			var invokeCount = 0;

			rd.AddFactory("test", () =>
			{
				invokeCount++;
				return "TestValue";
			}, shared: true);

			// First access
			var value1 = rd["test"];
			Assert.Equal("TestValue", value1);
			Assert.Equal(1, invokeCount);

			// Second access - should use cached value
			var value2 = rd["test"];
			Assert.Equal("TestValue", value2);
			Assert.Equal(1, invokeCount); // Still 1, factory not invoked again
		}

		[Fact]
		public void AddFactory_SharedFalse_InvokesFactoryEachTime()
		{
			var rd = new ResourceDictionary();
			var invokeCount = 0;

			rd.AddFactory("test", () =>
			{
				invokeCount++;
				return $"Value{invokeCount}";
			}, shared: false);

			// First access
			var value1 = rd["test"];
			Assert.Equal("Value1", value1);
			Assert.Equal(1, invokeCount);

			// Second access - should invoke factory again
			var value2 = rd["test"];
			Assert.Equal("Value2", value2);
			Assert.Equal(2, invokeCount);

			// Third access
			var value3 = rd["test"];
			Assert.Equal("Value3", value3);
			Assert.Equal(3, invokeCount);
		}

		[Fact]
		public void AddFactory_TryGetValue_ResolvesFactory()
		{
			var rd = new ResourceDictionary();
			rd.AddFactory("test", () => "FactoryValue", shared: true);

			Assert.True(rd.TryGetValue("test", out var value));
			Assert.Equal("FactoryValue", value);
		}

		[Fact]
		public void AddFactory_ContainsKey_ReturnsTrue()
		{
			var rd = new ResourceDictionary();
			rd.AddFactory("test", () => "Value", shared: true);

			Assert.True(rd.ContainsKey("test"));
		}

		[Fact]
		public void AddFactory_DuplicateKey_Throws()
		{
			var rd = new ResourceDictionary();
			rd.AddFactory("test", () => "Value1", shared: true);

			var ex = Assert.Throws<ArgumentException>(() =>
				rd.AddFactory("test", () => "Value2", shared: true));
			Assert.Contains("test", ex.Message, StringComparison.InvariantCulture);
		}

		[Fact]
		public void AddFactory_ImplicitStyle_UsesTargetTypeAsKey()
		{
			var rd = new ResourceDictionary();
			rd.AddFactory(typeof(Label), () => new Style(typeof(Label))
			{
				Setters = { new Setter { Property = Label.TextColorProperty, Value = Colors.Red } }
			}, shared: true);

			// Should be accessible via type's full name
			Assert.True(rd.TryGetValue(typeof(Label).FullName, out var value));
			Assert.IsType<Style>(value);
			var style = (Style)value;
			Assert.Equal(typeof(Label), style.TargetType);
		}

		[Fact]
		public void AddFactory_Values_ResolvesLazyResources()
		{
			var rd = new ResourceDictionary();
			rd.AddFactory("lazy", () => "LazyValue", shared: true);
			rd.Add("regular", "RegularValue");

			var values = rd.Values.ToList();
			Assert.Contains("LazyValue", values);
			Assert.Contains("RegularValue", values);
		}

		[Fact]
		public void AddFactory_Enumerate_ResolvesLazyResources()
		{
			var rd = new ResourceDictionary();
			rd.AddFactory("lazy", () => "LazyValue", shared: true);
			rd.Add("regular", "RegularValue");

			var dict = rd.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			Assert.Equal("LazyValue", dict["lazy"]);
			Assert.Equal("RegularValue", dict["regular"]);
		}

		[Fact]
		public void AddFactory_DoesNotInvokeUntilAccessed()
		{
			var rd = new ResourceDictionary();
			var invoked = false;

			rd.AddFactory("test", () =>
			{
				invoked = true;
				return "Value";
			}, shared: true);

			// Factory should not be invoked yet
			Assert.False(invoked);

			// Now access it
			_ = rd["test"];
			Assert.True(invoked);
		}

		[Fact]
		public void AddFactory_SharedFalse_CreatesNewInstances()
		{
			var rd = new ResourceDictionary();
			rd.AddFactory("shape", () => new BoxView(), shared: false);

			var shape1 = rd["shape"];
			var shape2 = rd["shape"];

			Assert.NotSame(shape1, shape2);
		}

		[Fact]
		public void AddFactory_SharedTrue_ReturnsSameInstance()
		{
			var rd = new ResourceDictionary();
			rd.AddFactory("shape", () => new BoxView(), shared: true);

			var shape1 = rd["shape"];
			var shape2 = rd["shape"];

			Assert.Same(shape1, shape2);
		}

		[Fact]
		public void AddFactory_ValuesChanged_FiresWithLazyResourceWrapper_KnownIssue()
		{
			// This test documents a known issue: AddFactory fires ValuesChanged with the
			// internal LazyResource wrapper instead of the resolved value.
			// This can break DynamicResource bindings that are set up before the resource is added.
			// 
			// Future fix: Either resolve before firing, or have Element.OnResourcesChanged 
			// handle LazyResource resolution.

			var rd = new ResourceDictionary();
			object eventValue = null;

			((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
			{
				eventValue = e.Values.First().Value;
			};

			rd.AddFactory("myColor", () => Colors.Red, shared: true);

			// Current behavior: eventValue is a LazyResource wrapper, not Colors.Red
			// This is a known issue - the value is NOT resolved before firing the event
			Assert.NotNull(eventValue);
			
			// This assertion documents the bug - eventValue should be Colors.Red but isn't
			Assert.NotEqual(Colors.Red, eventValue);
			
			// The value IS correct when accessed directly
			Assert.Equal(Colors.Red, rd["myColor"]);
		}

		[Fact]
		public void AddFactory_DynamicResource_ReceivesLazyWrapper_KnownIssue()
		{
			// This test documents a known issue: When a DynamicResource binding exists
			// and a lazy resource is added later, the element receives the LazyResource
			// wrapper instead of the resolved value.

			var rd = new ResourceDictionary();
			var label = new Label();
			
			// Set up the page hierarchy so DynamicResource can find resources
			var page = new ContentPage { Content = label };
			page.Resources = rd;

			// Set up DynamicResource binding BEFORE the resource exists
			label.SetDynamicResource(Label.TextColorProperty, "myColor");

			// Add the resource via factory - this fires ValuesChanged
			rd.AddFactory("myColor", () => Colors.Blue, shared: true);

			// Known issue: The label's TextColor is NOT set to Colors.Blue
			// because OnResourcesChanged received the LazyResource wrapper
			// 
			// When fixed, this should be: Assert.Equal(Colors.Blue, label.TextColor);
			Assert.NotEqual(Colors.Blue, label.TextColor);
		}

		#endregion
	}
}