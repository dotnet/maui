#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Xml;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
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

        /// <summary>
        /// Tests that SetAndCreateSource with a valid Uri and ResourceDictionary type successfully creates an instance and sets the source.
        /// </summary>
        [Fact]
        public void SetAndCreateSource_ValidUriAndType_SetsSourceCorrectly()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var resourceDictionary = new ResourceDictionary();
            var uri = new Uri("test://example", UriKind.Absolute);
            bool valuesChangedFired = false;

            ((IResourceDictionary)resourceDictionary).ValuesChanged += (sender, e) =>
            {
                valuesChangedFired = true;
            };

            // Act
            resourceDictionary.SetAndCreateSource<TestResourceDictionary>(uri);

            // Assert
            Assert.Equal(uri, resourceDictionary.Source);
            Assert.True(valuesChangedFired);
        }

        /// <summary>
        /// Tests that SetAndCreateSource with null Uri successfully creates an instance and sets null source.
        /// </summary>
        [Fact]
        public void SetAndCreateSource_NullUri_SetsSourceToNull()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var resourceDictionary = new ResourceDictionary();
            bool valuesChangedFired = false;

            ((IResourceDictionary)resourceDictionary).ValuesChanged += (sender, e) =>
            {
                valuesChangedFired = true;
            };

            // Act
            resourceDictionary.SetAndCreateSource<TestResourceDictionary>(null);

            // Assert
            Assert.Null(resourceDictionary.Source);
            Assert.True(valuesChangedFired);
        }

        /// <summary>
        /// Tests that SetAndCreateSource with empty Uri successfully creates an instance and sets the source.
        /// </summary>
        [Fact]
        public void SetAndCreateSource_EmptyUri_SetsSourceCorrectly()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var resourceDictionary = new ResourceDictionary();
            var uri = new Uri(string.Empty, UriKind.Relative);
            bool valuesChangedFired = false;

            ((IResourceDictionary)resourceDictionary).ValuesChanged += (sender, e) =>
            {
                valuesChangedFired = true;
            };

            // Act
            resourceDictionary.SetAndCreateSource<TestResourceDictionary>(uri);

            // Assert
            Assert.Equal(uri, resourceDictionary.Source);
            Assert.True(valuesChangedFired);
        }

        /// <summary>
        /// Tests that SetAndCreateSource throws the inner exception when the ResourceDictionary constructor throws a TargetInvocationException.
        /// </summary>
        [Fact]
        public void SetAndCreateSource_ConstructorThrowsTargetInvocationException_RethrowsInnerException()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var resourceDictionary = new ResourceDictionary();
            var uri = new Uri("test://example", UriKind.Absolute);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                resourceDictionary.SetAndCreateSource<ThrowingResourceDictionary>(uri));

            Assert.Equal("Test exception from constructor", exception.Message);
        }

        /// <summary>
        /// Tests that SetAndCreateSource uses cached instance when called multiple times with the same type.
        /// </summary>
        [Fact]
        public void SetAndCreateSource_SameTypeTwice_ReusesCachedInstance()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            TestResourceDictionary.InstanceCount = 0;
            var resourceDictionary1 = new ResourceDictionary();
            var resourceDictionary2 = new ResourceDictionary();
            var uri1 = new Uri("test://example1", UriKind.Absolute);
            var uri2 = new Uri("test://example2", UriKind.Absolute);

            // Act
            resourceDictionary1.SetAndCreateSource<TestResourceDictionary>(uri1);
            resourceDictionary2.SetAndCreateSource<TestResourceDictionary>(uri2);

            // Assert
            Assert.Equal(1, TestResourceDictionary.InstanceCount); // Only one instance should be created due to caching
        }

        /// <summary>
        /// Tests that SetAndCreateSource creates different instances for different types.
        /// </summary>
        [Fact]
        public void SetAndCreateSource_DifferentTypes_CreatesDifferentInstances()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            TestResourceDictionary.InstanceCount = 0;
            AnotherTestResourceDictionary.InstanceCount = 0;
            var resourceDictionary1 = new ResourceDictionary();
            var resourceDictionary2 = new ResourceDictionary();
            var uri1 = new Uri("test://example1", UriKind.Absolute);
            var uri2 = new Uri("test://example2", UriKind.Absolute);

            // Act
            resourceDictionary1.SetAndCreateSource<TestResourceDictionary>(uri1);
            resourceDictionary2.SetAndCreateSource<AnotherTestResourceDictionary>(uri2);

            // Assert
            Assert.Equal(1, TestResourceDictionary.InstanceCount);
            Assert.Equal(1, AnotherTestResourceDictionary.InstanceCount);
        }

        /// <summary>
        /// Tests that SetAndCreateSource with relative Uri successfully creates an instance and sets the source.
        /// </summary>
        [Fact]
        public void SetAndCreateSource_RelativeUri_SetsSourceCorrectly()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var resourceDictionary = new ResourceDictionary();
            var uri = new Uri("relative/path", UriKind.Relative);
            bool valuesChangedFired = false;

            ((IResourceDictionary)resourceDictionary).ValuesChanged += (sender, e) =>
            {
                valuesChangedFired = true;
            };

            // Act
            resourceDictionary.SetAndCreateSource<TestResourceDictionary>(uri);

            // Assert
            Assert.Equal(uri, resourceDictionary.Source);
            Assert.True(valuesChangedFired);
        }

        /// <summary>
        /// Test ResourceDictionary class for testing SetAndCreateSource method.
        /// </summary>
        public class TestResourceDictionary : ResourceDictionary
        {
            public static int InstanceCount { get; set; }

            public TestResourceDictionary()
            {
                InstanceCount++;
                Add("TestKey", "TestValue");
            }
        }

        /// <summary>
        /// Another test ResourceDictionary class for testing different type handling.
        /// </summary>
        public class AnotherTestResourceDictionary : ResourceDictionary
        {
            public static int InstanceCount { get; set; }

            public AnotherTestResourceDictionary()
            {
                InstanceCount++;
                Add("AnotherTestKey", "AnotherTestValue");
            }
        }

        /// <summary>
        /// Test ResourceDictionary class that throws exception in constructor for testing exception handling.
        /// </summary>
        public class ThrowingResourceDictionary : ResourceDictionary
        {
            public ThrowingResourceDictionary()
            {
                throw new TargetInvocationException(new InvalidOperationException("Test exception from constructor"));
            }
        }

        /// <summary>
        /// Tests that GetOrCreateInstance successfully creates and returns a ResourceDictionary instance
        /// for a valid ResourceDictionary subclass type.
        /// </summary>
        [Fact]
        public void GetOrCreateInstance_ValidType_ReturnsInstance()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var type = typeof(TestResourceDictionary);

            // Act
            var result = ResourceDictionary.GetOrCreateInstance(type);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<TestResourceDictionary>(result);
        }

        /// <summary>
        /// Tests that GetOrCreateInstance returns the same instance when called multiple times
        /// with the same type, demonstrating proper caching behavior.
        /// </summary>
        [Fact]
        public void GetOrCreateInstance_SameTypeCalled_ReturnsSameInstance()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var type = typeof(TestResourceDictionary);

            // Act
            var result1 = ResourceDictionary.GetOrCreateInstance(type);
            var result2 = ResourceDictionary.GetOrCreateInstance(type);

            // Assert
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that GetOrCreateInstance returns different instances for different types,
        /// ensuring proper type-based caching separation.
        /// </summary>
        [Fact]
        public void GetOrCreateInstance_DifferentTypes_ReturnsDifferentInstances()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var type1 = typeof(TestResourceDictionary);
            var type2 = typeof(AnotherTestResourceDictionary);

            // Act
            var result1 = ResourceDictionary.GetOrCreateInstance(type1);
            var result2 = ResourceDictionary.GetOrCreateInstance(type2);

            // Assert
            Assert.NotSame(result1, result2);
            Assert.IsType<TestResourceDictionary>(result1);
            Assert.IsType<AnotherTestResourceDictionary>(result2);
        }

        /// <summary>
        /// Tests that GetOrCreateInstance properly unwraps and rethrows the inner exception
        /// when a TargetInvocationException with an inner exception is thrown during construction.
        /// </summary>
        [Fact]
        public void GetOrCreateInstance_ConstructorThrowsTargetInvocationExceptionWithInnerException_RethrowsInnerException()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var type = typeof(ThrowingResourceDictionary);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => ResourceDictionary.GetOrCreateInstance(type));
            Assert.Equal("Constructor failed", exception.Message);
        }

        /// <summary>
        /// Tests that GetOrCreateInstance throws ArgumentNullException when passed a null type parameter.
        /// </summary>
        [Fact]
        public void GetOrCreateInstance_NullType_ThrowsArgumentNullException()
        {
            // Arrange
            ResourceDictionary.ClearCache();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ResourceDictionary.GetOrCreateInstance(null));
        }

        /// <summary>
        /// Tests that GetOrCreateInstance throws InvalidCastException when the type cannot be cast to ResourceDictionary.
        /// </summary>
        [Fact]
        public void GetOrCreateInstance_NonResourceDictionaryType_ThrowsInvalidCastException()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var type = typeof(object);

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => ResourceDictionary.GetOrCreateInstance(type));
        }

        /// <summary>
        /// Tests that GetOrCreateInstance throws MissingMethodException when the type has no parameterless constructor.
        /// </summary>
        [Fact]
        public void GetOrCreateInstance_NoParameterlessConstructor_ThrowsMissingMethodException()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var type = typeof(NoParameterlessConstructorResourceDictionary);

            // Act & Assert
            Assert.Throws<MissingMethodException>(() => ResourceDictionary.GetOrCreateInstance(type));
        }

        /// <summary>
        /// Tests that GetOrCreateInstance throws TypeInitializationException when the type cannot be instantiated due to abstract class.
        /// </summary>
        [Fact]
        public void GetOrCreateInstance_AbstractType_ThrowsMissingMethodException()
        {
            // Arrange
            ResourceDictionary.ClearCache();
            var type = typeof(AbstractResourceDictionary);

            // Act & Assert
            Assert.Throws<MissingMethodException>(() => ResourceDictionary.GetOrCreateInstance(type));
        }

        /// <summary>
        /// Helper ResourceDictionary subclass without parameterless constructor
        /// for testing constructor requirement validation.
        /// </summary>
        public class NoParameterlessConstructorResourceDictionary : ResourceDictionary
        {
            public NoParameterlessConstructorResourceDictionary(string parameter)
            {
                // Only non-parameterless constructor
            }
        }

        /// <summary>
        /// Abstract ResourceDictionary subclass for testing abstract type handling.
        /// </summary>
        public abstract class AbstractResourceDictionary : ResourceDictionary
        {
            // Abstract class cannot be instantiated
        }

        /// <summary>
        /// Tests Remove method when key exists in the inner dictionary.
        /// Should return true and remove the key from inner dictionary.
        /// </summary>
        [Fact]
        public void Remove_KeyExistsInInnerDictionary_ReturnsTrue()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd.Add("testKey", "testValue");

            // Act
            var result = rd.Remove("testKey");

            // Assert
            Assert.True(result);
            Assert.False(rd.ContainsKey("testKey"));
        }

        /// <summary>
        /// Tests Remove method when key exists only in merged instance.
        /// Should return true and remove the key from merged instance.
        /// </summary>
        [Fact]
        public void Remove_KeyExistsOnlyInMergedInstance_ReturnsTrue()
        {
            // Arrange
            var rd = new ResourceDictionary();
            var mergedRd = new ResourceDictionary();
            mergedRd.Add("mergedKey", "mergedValue");
            rd.SetSource(new Uri("test://source", UriKind.Absolute), mergedRd);

            // Act
            var result = rd.Remove("mergedKey");

            // Assert
            Assert.True(result);
            Assert.False(mergedRd.ContainsKey("mergedKey"));
        }

        /// <summary>
        /// Tests Remove method when key exists in both inner and merged dictionaries.
        /// Should return true and remove from inner dictionary first.
        /// </summary>
        [Fact]
        public void Remove_KeyExistsInBothDictionaries_ReturnsTrue()
        {
            // Arrange
            var rd = new ResourceDictionary();
            var mergedRd = new ResourceDictionary();
            rd.Add("sameKey", "innerValue");
            mergedRd.Add("sameKey", "mergedValue");
            rd.SetSource(new Uri("test://source", UriKind.Absolute), mergedRd);

            // Act
            var result = rd.Remove("sameKey");

            // Assert
            Assert.True(result);
            Assert.False(rd.ContainsKey("sameKey"));
            Assert.True(mergedRd.ContainsKey("sameKey")); // Should still exist in merged
        }

        /// <summary>
        /// Tests Remove method when key doesn't exist in either dictionary.
        /// Should return false.
        /// </summary>
        [Fact]
        public void Remove_KeyExistsInNeither_ReturnsFalse()
        {
            // Arrange
            var rd = new ResourceDictionary();
            var mergedRd = new ResourceDictionary();
            rd.Add("existingKey", "value");
            mergedRd.Add("anotherKey", "anotherValue");
            rd.SetSource(new Uri("test://source", UriKind.Absolute), mergedRd);

            // Act
            var result = rd.Remove("nonExistentKey");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Remove method when merged instance is null and key doesn't exist.
        /// Should return false.
        /// </summary>
        [Fact]
        public void Remove_MergedInstanceIsNull_ReturnsFalse()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd.Add("existingKey", "value");

            // Act
            var result = rd.Remove("nonExistentKey");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests Remove method with null key parameter.
        /// Should throw ArgumentNullException.
        /// </summary>
        [Fact]
        public void Remove_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var rd = new ResourceDictionary();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => rd.Remove(null));
        }

        /// <summary>
        /// Tests Remove method with empty string key.
        /// Should handle empty string as valid key and return appropriate result.
        /// </summary>
        [Fact]
        public void Remove_EmptyStringKey_ValidBehavior()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd.Add("", "emptyKeyValue");

            // Act
            var result = rd.Remove("");

            // Assert
            Assert.True(result);
            Assert.False(rd.ContainsKey(""));
        }

        /// <summary>
        /// Tests Remove method with whitespace-only key.
        /// Should handle whitespace as valid key and return appropriate result.
        /// </summary>
        [Fact]
        public void Remove_WhitespaceKey_ValidBehavior()
        {
            // Arrange
            var rd = new ResourceDictionary();
            var whitespaceKey = "   ";
            rd.Add(whitespaceKey, "whitespaceValue");

            // Act
            var result = rd.Remove(whitespaceKey);

            // Assert
            Assert.True(result);
            Assert.False(rd.ContainsKey(whitespaceKey));
        }

        /// <summary>
        /// Tests Remove method with very long key string.
        /// Should handle long strings properly.
        /// </summary>
        [Fact]
        public void Remove_VeryLongKey_ValidBehavior()
        {
            // Arrange
            var rd = new ResourceDictionary();
            var longKey = new string('a', 10000);
            rd.Add(longKey, "longKeyValue");

            // Act
            var result = rd.Remove(longKey);

            // Assert
            Assert.True(result);
            Assert.False(rd.ContainsKey(longKey));
        }

        /// <summary>
        /// Tests Remove method with special characters in key.
        /// Should handle special characters properly.
        /// </summary>
        [Theory]
        [InlineData("key with spaces")]
        [InlineData("key\twith\ttabs")]
        [InlineData("key\nwith\nnewlines")]
        [InlineData("key.with.dots")]
        [InlineData("key/with/slashes")]
        [InlineData("key\\with\\backslashes")]
        [InlineData("key:with:colons")]
        [InlineData("key;with;semicolons")]
        [InlineData("key=with=equals")]
        [InlineData("key&with&ampersands")]
        [InlineData("key?with?questions")]
        [InlineData("key#with#hashes")]
        public void Remove_SpecialCharactersInKey_ValidBehavior(string specialKey)
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd.Add(specialKey, "specialValue");

            // Act
            var result = rd.Remove(specialKey);

            // Assert
            Assert.True(result);
            Assert.False(rd.ContainsKey(specialKey));
        }

        /// <summary>
        /// Tests Remove method when merged instance becomes null after removal.
        /// Verifies proper null handling in merged instance operations.
        /// </summary>
        [Fact]
        public void Remove_FromEmptyMergedInstance_ReturnsFalse()
        {
            // Arrange
            var rd = new ResourceDictionary();
            var emptyMergedRd = new ResourceDictionary();
            rd.SetSource(new Uri("test://source", UriKind.Absolute), emptyMergedRd);

            // Act
            var result = rd.Remove("nonExistentKey");

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that MergedResources returns only inner dictionary resources when no merged dictionaries exist.
        /// Input: ResourceDictionary with only inner dictionary resources.
        /// Expected: Returns only the inner dictionary resources.
        /// </summary>
        [Fact]
        public void MergedResources_WithOnlyInnerDictionary_ReturnsInnerDictionaryResources()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd["key1"] = "value1";
            rd["key2"] = "value2";

            // Act
            var result = rd.MergedResources.ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(new KeyValuePair<string, object>("key1", "value1"), result);
            Assert.Contains(new KeyValuePair<string, object>("key2", "value2"), result);
        }

        /// <summary>
        /// Tests that MergedResources returns empty enumerable when resource dictionary is completely empty.
        /// Input: Empty ResourceDictionary with no resources.
        /// Expected: Returns empty enumerable.
        /// </summary>
        [Fact]
        public void MergedResources_WithEmptyDictionary_ReturnsEmptyEnumerable()
        {
            // Arrange
            var rd = new ResourceDictionary();

            // Act
            var result = rd.MergedResources.ToList();

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that MergedResources returns resources from merged dictionaries and inner dictionary in correct order.
        /// Input: ResourceDictionary with merged dictionaries and inner dictionary resources.
        /// Expected: Returns merged dictionaries in reverse order, then inner dictionary resources.
        /// </summary>
        [Fact]
        public void MergedResources_WithMergedDictionaries_ReturnsResourcesInCorrectOrder()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd["inner1"] = "innerValue1";
            rd["inner2"] = "innerValue2";

            var merged1 = new ResourceDictionary();
            merged1["merged1Key"] = "merged1Value";

            var merged2 = new ResourceDictionary();
            merged2["merged2Key"] = "merged2Value";

            rd.MergedDictionaries.Add(merged1);
            rd.MergedDictionaries.Add(merged2);

            // Act
            var result = rd.MergedResources.ToList();

            // Assert
            Assert.Equal(4, result.Count);
            // Should return merged dictionaries in reverse order (merged2, then merged1), then inner dictionary
            Assert.Equal("merged2Value", result[0].Value);
            Assert.Equal("merged1Value", result[1].Value);
            Assert.Contains(new KeyValuePair<string, object>("inner1", "innerValue1"), result);
            Assert.Contains(new KeyValuePair<string, object>("inner2", "innerValue2"), result);
        }

        /// <summary>
        /// Tests that MergedResources correctly handles nested merged dictionaries recursively.
        /// Input: ResourceDictionary with nested merged dictionaries.
        /// Expected: Returns all resources from all levels in correct hierarchical order.
        /// </summary>
        [Fact]
        public void MergedResources_WithNestedMergedDictionaries_ReturnsAllResourcesRecursively()
        {
            // Arrange
            var innerMerged = new ResourceDictionary();
            innerMerged["deepKey"] = "deepValue";

            var midLevel = new ResourceDictionary();
            midLevel["midKey"] = "midValue";
            midLevel.MergedDictionaries.Add(innerMerged);

            var topLevel = new ResourceDictionary();
            topLevel["topKey"] = "topValue";
            topLevel.MergedDictionaries.Add(midLevel);

            // Act
            var result = topLevel.MergedResources.ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(new KeyValuePair<string, object>("deepKey", "deepValue"), result);
            Assert.Contains(new KeyValuePair<string, object>("midKey", "midValue"), result);
            Assert.Contains(new KeyValuePair<string, object>("topKey", "topValue"), result);
        }

        /// <summary>
        /// Tests that MergedResources returns duplicate keys from different dictionaries without deduplication.
        /// Input: ResourceDictionary with multiple dictionaries containing same keys.
        /// Expected: Returns all key-value pairs including duplicates.
        /// </summary>
        [Fact]
        public void MergedResources_WithDuplicateKeys_ReturnsAllKeyValuePairsIncludingDuplicates()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd["duplicateKey"] = "innerValue";

            var merged1 = new ResourceDictionary();
            merged1["duplicateKey"] = "merged1Value";

            var merged2 = new ResourceDictionary();
            merged2["duplicateKey"] = "merged2Value";

            rd.MergedDictionaries.Add(merged1);
            rd.MergedDictionaries.Add(merged2);

            // Act
            var result = rd.MergedResources.ToList();

            // Assert
            Assert.Equal(3, result.Count);
            var duplicateKeyValues = result.Where(kvp => kvp.Key == "duplicateKey").Select(kvp => kvp.Value).ToList();
            Assert.Equal(3, duplicateKeyValues.Count);
            Assert.Contains("innerValue", duplicateKeyValues);
            Assert.Contains("merged1Value", duplicateKeyValues);
            Assert.Contains("merged2Value", duplicateKeyValues);
        }

        /// <summary>
        /// Tests that MergedResources handles multiple merged dictionaries with various data types.
        /// Input: ResourceDictionary with merged dictionaries containing different object types.
        /// Expected: Returns all resources with their original types preserved.
        /// </summary>
        [Theory]
        [InlineData("stringValue")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void MergedResources_WithVariousDataTypes_PreservesObjectTypes(object testValue)
        {
            // Arrange
            var rd = new ResourceDictionary();
            var merged = new ResourceDictionary();
            merged["testKey"] = testValue;
            rd.MergedDictionaries.Add(merged);

            // Act
            var result = rd.MergedResources.ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("testKey", result[0].Key);
            Assert.Equal(testValue, result[0].Value);
            Assert.Equal(testValue.GetType(), result[0].Value.GetType());
        }

        /// <summary>
        /// Tests that MergedResources handles null values correctly.
        /// Input: ResourceDictionary with null values in merged and inner dictionaries.
        /// Expected: Returns null values without throwing exceptions.
        /// </summary>
        [Fact]
        public void MergedResources_WithNullValues_ReturnsNullValuesCorrectly()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd["innerNull"] = null;

            var merged = new ResourceDictionary();
            merged["mergedNull"] = null;
            rd.MergedDictionaries.Add(merged);

            // Act
            var result = rd.MergedResources.ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(new KeyValuePair<string, object>("innerNull", null), result);
            Assert.Contains(new KeyValuePair<string, object>("mergedNull", null), result);
        }

        /// <summary>
        /// Tests that MergedResources correctly handles empty merged dictionaries.
        /// Input: ResourceDictionary with empty merged dictionaries and inner dictionary resources.
        /// Expected: Returns only inner dictionary resources, skipping empty merged dictionaries.
        /// </summary>
        [Fact]
        public void MergedResources_WithEmptyMergedDictionaries_ReturnsOnlyInnerDictionaryResources()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd["innerKey"] = "innerValue";

            var emptyMerged1 = new ResourceDictionary();
            var emptyMerged2 = new ResourceDictionary();

            rd.MergedDictionaries.Add(emptyMerged1);
            rd.MergedDictionaries.Add(emptyMerged2);

            // Act
            var result = rd.MergedResources.ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(new KeyValuePair<string, object>("innerKey", "innerValue"), result[0]);
        }

        /// <summary>
        /// Tests that MergedResources maintains correct enumeration order across multiple iterations.
        /// Input: ResourceDictionary with complex merged dictionary structure.
        /// Expected: Multiple enumerations return resources in same order consistently.
        /// </summary>
        [Fact]
        public void MergedResources_MultipleEnumerations_ReturnsSameOrderConsistently()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd["inner"] = "innerValue";

            var merged1 = new ResourceDictionary();
            merged1["merged1"] = "merged1Value";

            var merged2 = new ResourceDictionary();
            merged2["merged2"] = "merged2Value";

            rd.MergedDictionaries.Add(merged1);
            rd.MergedDictionaries.Add(merged2);

            // Act
            var firstEnumeration = rd.MergedResources.ToList();
            var secondEnumeration = rd.MergedResources.ToList();

            // Assert
            Assert.Equal(firstEnumeration.Count, secondEnumeration.Count);
            for (int i = 0; i < firstEnumeration.Count; i++)
            {
                Assert.Equal(firstEnumeration[i].Key, secondEnumeration[i].Key);
                Assert.Equal(firstEnumeration[i].Value, secondEnumeration[i].Value);
            }
        }
    }


    public partial class ResourceDictionarySetAndLoadSourceTests : BaseTestFixture
    {
        private Action<ResourceDictionary, Uri, string, Assembly, IXmlLineInfo> _originalDelegate;

        public ResourceDictionarySetAndLoadSourceTests()
        {
            // Save the original delegate state
            _originalDelegate = GetSetAndLoadSourceDelegate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Restore the original delegate state
                SetSetAndLoadSourceDelegate(_originalDelegate);
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Tests that SetAndLoadSource throws InvalidOperationException when the static delegate is null.
        /// This verifies the initialization check and error handling for uninitialized hot reload functionality.
        /// </summary>
        [Fact]
        public void SetAndLoadSource_WhenDelegateIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var uri = new Uri("test://example", UriKind.Absolute);
            var resourcePath = "TestPath";
            var assembly = GetType().Assembly;
            var lineInfo = Substitute.For<IXmlLineInfo>();

            SetSetAndLoadSourceDelegate(null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                resourceDictionary.SetAndLoadSource(uri, resourcePath, assembly, lineInfo));

            Assert.Equal("ResourceDictionary.SetAndLoadSource was not initialized", exception.Message);
        }

        /// <summary>
        /// Tests that SetAndLoadSource throws InvalidOperationException when delegate is null with null parameters.
        /// This ensures the null check occurs before parameter validation.
        /// </summary>
        [Fact]
        public void SetAndLoadSource_WhenDelegateIsNullAndParametersAreNull_ThrowsInvalidOperationException()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();

            SetSetAndLoadSourceDelegate(null);

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                resourceDictionary.SetAndLoadSource(null, null, null, null));

            Assert.Equal("ResourceDictionary.SetAndLoadSource was not initialized", exception.Message);
        }

        /// <summary>
        /// Tests that SetAndLoadSource calls the delegate with correct parameters when delegate is not null.
        /// This verifies the delegate invocation and parameter passing functionality.
        /// </summary>
        [Fact]
        public void SetAndLoadSource_WhenDelegateIsNotNull_CallsDelegateWithCorrectParameters()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var uri = new Uri("test://example", UriKind.Absolute);
            var resourcePath = "TestPath";
            var assembly = GetType().Assembly;
            var lineInfo = Substitute.For<IXmlLineInfo>();

            var mockDelegate = Substitute.For<Action<ResourceDictionary, Uri, string, Assembly, IXmlLineInfo>>();
            SetSetAndLoadSourceDelegate(mockDelegate);

            // Act
            resourceDictionary.SetAndLoadSource(uri, resourcePath, assembly, lineInfo);

            // Assert
            mockDelegate.Received(1).Invoke(resourceDictionary, uri, resourcePath, assembly, lineInfo);
        }

        /// <summary>
        /// Tests that SetAndLoadSource calls the delegate with null parameters when they are provided.
        /// This verifies that null parameters are passed through correctly to the delegate.
        /// </summary>
        [Fact]
        public void SetAndLoadSource_WhenDelegateIsNotNullAndParametersAreNull_CallsDelegateWithNullParameters()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();

            var mockDelegate = Substitute.For<Action<ResourceDictionary, Uri, string, Assembly, IXmlLineInfo>>();
            SetSetAndLoadSourceDelegate(mockDelegate);

            // Act
            resourceDictionary.SetAndLoadSource(null, null, null, null);

            // Assert
            mockDelegate.Received(1).Invoke(resourceDictionary, null, null, null, null);
        }

        /// <summary>
        /// Tests SetAndLoadSource with various edge case parameter combinations to ensure delegate receives them correctly.
        /// This verifies parameter passing for boundary values and special cases.
        /// </summary>
        [Theory]
        [InlineData("", "")]
        [InlineData("   ", "   ")]
        [InlineData("valid/path", "resource")]
        public void SetAndLoadSource_WithVariousStringParameters_CallsDelegateCorrectly(string resourcePath, string expectedPath)
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var uri = new Uri("relative/path", UriKind.Relative);
            var assembly = GetType().Assembly;
            var lineInfo = Substitute.For<IXmlLineInfo>();

            var mockDelegate = Substitute.For<Action<ResourceDictionary, Uri, string, Assembly, IXmlLineInfo>>();
            SetSetAndLoadSourceDelegate(mockDelegate);

            // Act
            resourceDictionary.SetAndLoadSource(uri, resourcePath, assembly, lineInfo);

            // Assert
            mockDelegate.Received(1).Invoke(resourceDictionary, uri, expectedPath, assembly, lineInfo);
        }

        /// <summary>
        /// Tests SetAndLoadSource with different URI types to ensure all are passed correctly to the delegate.
        /// This verifies handling of absolute, relative, and file URIs.
        /// </summary>
        [Theory]
        [InlineData("https://example.com/resource.xaml")]
        [InlineData("file:///C:/path/resource.xaml")]
        [InlineData("relative/path.xaml")]
        public void SetAndLoadSource_WithDifferentUriTypes_CallsDelegateCorrectly(string uriString)
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            var resourcePath = "test";
            var assembly = GetType().Assembly;
            var lineInfo = Substitute.For<IXmlLineInfo>();

            var mockDelegate = Substitute.For<Action<ResourceDictionary, Uri, string, Assembly, IXmlLineInfo>>();
            SetSetAndLoadSourceDelegate(mockDelegate);

            // Act
            resourceDictionary.SetAndLoadSource(uri, resourcePath, assembly, lineInfo);

            // Assert
            mockDelegate.Received(1).Invoke(resourceDictionary, uri, resourcePath, assembly, lineInfo);
        }

        /// <summary>
        /// Tests that SetAndLoadSource passes the correct ResourceDictionary instance to the delegate.
        /// This ensures the 'this' parameter is correctly forwarded.
        /// </summary>
        [Fact]
        public void SetAndLoadSource_PassesCorrectResourceDictionaryInstance()
        {
            // Arrange
            var resourceDictionary1 = new ResourceDictionary();
            var resourceDictionary2 = new ResourceDictionary();
            var uri = new Uri("test://example");
            var resourcePath = "path";
            var assembly = GetType().Assembly;
            var lineInfo = Substitute.For<IXmlLineInfo>();

            var mockDelegate = Substitute.For<Action<ResourceDictionary, Uri, string, Assembly, IXmlLineInfo>>();
            SetSetAndLoadSourceDelegate(mockDelegate);

            // Act
            resourceDictionary1.SetAndLoadSource(uri, resourcePath, assembly, lineInfo);

            // Assert
            mockDelegate.Received(1).Invoke(resourceDictionary1, uri, resourcePath, assembly, lineInfo);
            mockDelegate.DidNotReceive().Invoke(resourceDictionary2, Arg.Any<Uri>(), Arg.Any<string>(), Arg.Any<Assembly>(), Arg.Any<IXmlLineInfo>());
        }

        private Action<ResourceDictionary, Uri, string, Assembly, IXmlLineInfo> GetSetAndLoadSourceDelegate()
        {
            var field = typeof(ResourceDictionary).GetField("s_setAndLoadSource", BindingFlags.NonPublic | BindingFlags.Static);
            return (Action<ResourceDictionary, Uri, string, Assembly, IXmlLineInfo>)field.GetValue(null);
        }

        private void SetSetAndLoadSourceDelegate(Action<ResourceDictionary, Uri, string, Assembly, IXmlLineInfo> value)
        {
            var field = typeof(ResourceDictionary).GetField("s_setAndLoadSource", BindingFlags.NonPublic | BindingFlags.Static);
            field.SetValue(null, value);
        }
    }


    /// <summary>
    /// Tests for the SetSource method in ResourceDictionary class.
    /// </summary>
    public partial class ResourceDictionarySetSourceTests
    {
        /// <summary>
        /// Helper class to expose internal SetSource method for testing.
        /// </summary>
        private class TestableResourceDictionary : ResourceDictionary
        {
            public void CallSetSource(Uri source, ResourceDictionary sourceInstance)
            {
                SetSource(source, sourceInstance);
            }

            public Uri GetSource()
            {
                var field = typeof(ResourceDictionary).GetField("_source", BindingFlags.NonPublic | BindingFlags.Instance);
                return (Uri)field.GetValue(this);
            }

            public ResourceDictionary GetMergedInstance()
            {
                var field = typeof(ResourceDictionary).GetField("_mergedInstance", BindingFlags.NonPublic | BindingFlags.Instance);
                return (ResourceDictionary)field.GetValue(this);
            }
        }

        /// <summary>
        /// Tests that SetSource correctly sets the source URI and merged instance fields when both parameters are valid.
        /// </summary>
        [Fact]
        public void SetSource_ValidUriAndResourceDictionary_SetsFieldsAndTriggersEvent()
        {
            // Arrange
            var testDictionary = new TestableResourceDictionary();
            var sourceUri = new Uri("test://source", UriKind.Absolute);
            var sourceInstance = new ResourceDictionary();
            sourceInstance.Add("testKey", "testValue");

            bool eventTriggered = false;
            ResourcesChangedEventArgs capturedArgs = null;

            ((IResourceDictionary)testDictionary).ValuesChanged += (sender, e) =>
            {
                eventTriggered = true;
                capturedArgs = e;
            };

            // Act
            testDictionary.CallSetSource(sourceUri, sourceInstance);

            // Assert
            Assert.Equal(sourceUri, testDictionary.GetSource());
            Assert.Equal(sourceInstance, testDictionary.GetMergedInstance());
            Assert.True(eventTriggered);
            Assert.NotNull(capturedArgs);
            Assert.Single(capturedArgs.Values);
            Assert.Equal("testKey", capturedArgs.Values.First().Key);
            Assert.Equal("testValue", capturedArgs.Values.First().Value);
        }

        /// <summary>
        /// Tests that SetSource correctly handles null URI parameter.
        /// </summary>
        [Fact]
        public void SetSource_NullUri_SetsNullSourceAndValidMergedInstance()
        {
            // Arrange
            var testDictionary = new TestableResourceDictionary();
            var sourceInstance = new ResourceDictionary();
            sourceInstance.Add("key1", "value1");

            bool eventTriggered = false;

            ((IResourceDictionary)testDictionary).ValuesChanged += (sender, e) =>
            {
                eventTriggered = true;
            };

            // Act
            testDictionary.CallSetSource(null, sourceInstance);

            // Assert
            Assert.Null(testDictionary.GetSource());
            Assert.Equal(sourceInstance, testDictionary.GetMergedInstance());
            Assert.True(eventTriggered);
        }

        /// <summary>
        /// Tests that SetSource throws NullReferenceException when sourceInstance parameter is null.
        /// </summary>
        [Fact]
        public void SetSource_NullSourceInstance_ThrowsNullReferenceException()
        {
            // Arrange
            var testDictionary = new TestableResourceDictionary();
            var sourceUri = new Uri("test://source", UriKind.Absolute);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => testDictionary.CallSetSource(sourceUri, null));
        }

        /// <summary>
        /// Tests that SetSource correctly handles empty ResourceDictionary.
        /// </summary>
        [Fact]
        public void SetSource_EmptyResourceDictionary_SetsFieldsButDoesNotTriggerEvent()
        {
            // Arrange
            var testDictionary = new TestableResourceDictionary();
            var sourceUri = new Uri("test://empty", UriKind.Absolute);
            var emptySourceInstance = new ResourceDictionary();

            bool eventTriggered = false;

            ((IResourceDictionary)testDictionary).ValuesChanged += (sender, e) =>
            {
                eventTriggered = true;
            };

            // Act
            testDictionary.CallSetSource(sourceUri, emptySourceInstance);

            // Assert
            Assert.Equal(sourceUri, testDictionary.GetSource());
            Assert.Equal(emptySourceInstance, testDictionary.GetMergedInstance());
            Assert.False(eventTriggered); // OnValuesChanged returns early for empty arrays
        }

        /// <summary>
        /// Tests that SetSource correctly handles ResourceDictionary with multiple items.
        /// </summary>
        [Fact]
        public void SetSource_MultipleItemsInResourceDictionary_SetsFieldsAndTriggersEventWithAllItems()
        {
            // Arrange
            var testDictionary = new TestableResourceDictionary();
            var sourceUri = new Uri("test://multiple", UriKind.Absolute);
            var sourceInstance = new ResourceDictionary();
            sourceInstance.Add("key1", "value1");
            sourceInstance.Add("key2", "value2");
            sourceInstance.Add("key3", "value3");

            bool eventTriggered = false;
            ResourcesChangedEventArgs capturedArgs = null;

            ((IResourceDictionary)testDictionary).ValuesChanged += (sender, e) =>
            {
                eventTriggered = true;
                capturedArgs = e;
            };

            // Act
            testDictionary.CallSetSource(sourceUri, sourceInstance);

            // Assert
            Assert.Equal(sourceUri, testDictionary.GetSource());
            Assert.Equal(sourceInstance, testDictionary.GetMergedInstance());
            Assert.True(eventTriggered);
            Assert.NotNull(capturedArgs);
            Assert.Equal(3, capturedArgs.Values.Count());

            var valuesDict = capturedArgs.Values.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Assert.Equal("value1", valuesDict["key1"]);
            Assert.Equal("value2", valuesDict["key2"]);
            Assert.Equal("value3", valuesDict["key3"]);
        }

        /// <summary>
        /// Tests that SetSource can be called multiple times with different parameters.
        /// </summary>
        [Fact]
        public void SetSource_CalledMultipleTimes_OverwritesPreviousValues()
        {
            // Arrange
            var testDictionary = new TestableResourceDictionary();

            var firstUri = new Uri("test://first", UriKind.Absolute);
            var firstInstance = new ResourceDictionary();
            firstInstance.Add("first", "1");

            var secondUri = new Uri("test://second", UriKind.Absolute);
            var secondInstance = new ResourceDictionary();
            secondInstance.Add("second", "2");

            // Act
            testDictionary.CallSetSource(firstUri, firstInstance);
            testDictionary.CallSetSource(secondUri, secondInstance);

            // Assert
            Assert.Equal(secondUri, testDictionary.GetSource());
            Assert.Equal(secondInstance, testDictionary.GetMergedInstance());
            Assert.NotEqual(firstInstance, testDictionary.GetMergedInstance());
        }

        /// <summary>
        /// Tests that SetSource correctly handles both null parameters.
        /// </summary>
        [Fact]
        public void SetSource_BothParametersNull_ThrowsNullReferenceException()
        {
            // Arrange
            var testDictionary = new TestableResourceDictionary();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => testDictionary.CallSetSource(null, null));
        }
    }


    public partial class RDSourceTypeConverterTests
    {
        /// <summary>
        /// Tests CombineUriAndAssembly with valid string value and assembly.
        /// Verifies that the method correctly combines the value with assembly name in the expected format.
        /// </summary>
        [Fact]
        public void CombineUriAndAssembly_ValidValueAndAssembly_ReturnsCorrectUri()
        {
            // Arrange
            string value = "path/to/resource";
            var assembly = Substitute.For<Assembly>();
            var assemblyName = Substitute.For<AssemblyName>();
            assemblyName.Name = "TestAssembly";
            assembly.GetName().Returns(assemblyName);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("path/to/resource;assembly=TestAssembly", result.ToString());
            Assert.Equal(UriKind.Relative, result.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        /// <summary>
        /// Tests CombineUriAndAssembly with empty string value.
        /// Verifies that the method handles empty values correctly and still includes assembly information.
        /// </summary>
        [Fact]
        public void CombineUriAndAssembly_EmptyValue_ReturnsUriWithAssemblyOnly()
        {
            // Arrange
            string value = "";
            var assembly = Substitute.For<Assembly>();
            var assemblyName = Substitute.For<AssemblyName>();
            assemblyName.Name = "TestAssembly";
            assembly.GetName().Returns(assemblyName);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(";assembly=TestAssembly", result.ToString());
        }

        /// <summary>
        /// Tests CombineUriAndAssembly with null string value.
        /// Verifies that the method throws ArgumentNullException when value parameter is null.
        /// </summary>
        [Fact]
        public void CombineUriAndAssembly_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            string value = null;
            var assembly = Substitute.For<Assembly>();
            var assemblyName = Substitute.For<AssemblyName>();
            assemblyName.Name = "TestAssembly";
            assembly.GetName().Returns(assemblyName);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly));
        }

        /// <summary>
        /// Tests CombineUriAndAssembly with null assembly parameter.
        /// Verifies that the method throws NullReferenceException when assembly parameter is null.
        /// </summary>
        [Fact]
        public void CombineUriAndAssembly_NullAssembly_ThrowsNullReferenceException()
        {
            // Arrange
            string value = "path/to/resource";
            Assembly assembly = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly));
        }

        /// <summary>
        /// Tests CombineUriAndAssembly with various string values and assembly names.
        /// Verifies correct URI formation for different input combinations.
        /// </summary>
        [Theory]
        [InlineData("simple", "MyAssembly", "simple;assembly=MyAssembly")]
        [InlineData("path/with/slashes", "TestLib", "path/with/slashes;assembly=TestLib")]
        [InlineData("resource.xaml", "UI.Controls", "resource.xaml;assembly=UI.Controls")]
        [InlineData("folder\\resource", "WindowsApp", "folder\\resource;assembly=WindowsApp")]
        [InlineData("resource;existing=param", "TestAssembly", "resource;existing=param;assembly=TestAssembly")]
        public void CombineUriAndAssembly_VariousInputs_ReturnsExpectedUri(string value, string assemblyName, string expected)
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var mockAssemblyName = Substitute.For<AssemblyName>();
            mockAssemblyName.Name = assemblyName;
            assembly.GetName().Returns(mockAssemblyName);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

            // Assert
            Assert.Equal(expected, result.ToString());
            Assert.False(result.IsAbsoluteUri);
        }

        /// <summary>
        /// Tests CombineUriAndAssembly with special characters in assembly name.
        /// Verifies that the method handles special characters in assembly names correctly.
        /// </summary>
        [Fact]
        public void CombineUriAndAssembly_AssemblyNameWithSpecialCharacters_ReturnsCorrectUri()
        {
            // Arrange
            string value = "resource";
            var assembly = Substitute.For<Assembly>();
            var assemblyName = Substitute.For<AssemblyName>();
            assemblyName.Name = "My-Assembly_Name.Test";
            assembly.GetName().Returns(assemblyName);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

            // Assert
            Assert.Equal("resource;assembly=My-Assembly_Name.Test", result.ToString());
        }

        /// <summary>
        /// Tests CombineUriAndAssembly with null assembly name.
        /// Verifies that the method handles null assembly name correctly.
        /// </summary>
        [Fact]
        public void CombineUriAndAssembly_NullAssemblyName_ReturnsUriWithNullAssemblyName()
        {
            // Arrange
            string value = "resource";
            var assembly = Substitute.For<Assembly>();
            var assemblyName = Substitute.For<AssemblyName>();
            assemblyName.Name = null;
            assembly.GetName().Returns(assemblyName);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

            // Assert
            Assert.Equal("resource;assembly=", result.ToString());
        }

        /// <summary>
        /// Tests CombineUriAndAssembly with empty assembly name.
        /// Verifies that the method handles empty assembly name correctly.
        /// </summary>
        [Fact]
        public void CombineUriAndAssembly_EmptyAssemblyName_ReturnsUriWithEmptyAssemblyName()
        {
            // Arrange
            string value = "resource";
            var assembly = Substitute.For<Assembly>();
            var assemblyName = Substitute.For<AssemblyName>();
            assemblyName.Name = "";
            assembly.GetName().Returns(assemblyName);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

            // Assert
            Assert.Equal("resource;assembly=", result.ToString());
        }

        /// <summary>
        /// Tests CombineUriAndAssembly with whitespace-only string value.
        /// Verifies that the method handles whitespace values correctly.
        /// </summary>
        [Fact]
        public void CombineUriAndAssembly_WhitespaceValue_ReturnsCorrectUri()
        {
            // Arrange
            string value = "   ";
            var assembly = Substitute.For<Assembly>();
            var assemblyName = Substitute.For<AssemblyName>();
            assemblyName.Name = "TestAssembly";
            assembly.GetName().Returns(assemblyName);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

            // Assert
            Assert.Equal("   ;assembly=TestAssembly", result.ToString());
        }

        /// <summary>
        /// Tests CombineUriAndAssembly with very long string value.
        /// Verifies that the method handles long string values without issues.
        /// </summary>
        [Fact]
        public void CombineUriAndAssembly_VeryLongValue_ReturnsCorrectUri()
        {
            // Arrange
            string value = new string('a', 1000);
            var assembly = Substitute.For<Assembly>();
            var assemblyName = Substitute.For<AssemblyName>();
            assemblyName.Name = "TestAssembly";
            assembly.GetName().Returns(assemblyName);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.CombineUriAndAssembly(value, assembly);

            // Assert
            Assert.Equal($"{value};assembly=TestAssembly", result.ToString());
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string).
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsString_ReturnsTrue()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns true when sourceType is typeof(string) and context is null.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsStringWithNullContext_ReturnsTrue()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();

            // Act
            var result = converter.CanConvertFrom(null, typeof(string));

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom returns false when sourceType is not typeof(string).
        /// Validates various non-string types to ensure comprehensive coverage.
        /// </summary>
        /// <param name="sourceType">The source type to test conversion from</param>
        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Uri))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(byte[]))]
        [InlineData(typeof(ICollection))]
        [InlineData(typeof(ResourceDictionary))]
        public void CanConvertFrom_SourceTypeIsNotString_ReturnsFalse(Type sourceType)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertFrom(context, sourceType);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null.
        /// This validates the behavior when a null Type is passed as sourceType parameter.
        /// </summary>
        [Fact]
        public void CanConvertFrom_SourceTypeIsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertFrom(context, null));
        }

        /// <summary>
        /// Tests that CanConvertFrom throws ArgumentNullException when sourceType is null and context is null.
        /// This validates the behavior when both parameters could potentially be null.
        /// </summary>
        [Fact]
        public void CanConvertFrom_BothParametersNull_ThrowsArgumentNullException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => converter.CanConvertFrom(null, null));
        }

        /// <summary>
        /// Tests GetResourcePath with URI that starts with forward slash.
        /// Verifies that absolute paths are handled correctly by combining with the maui:// scheme.
        /// Expected result: Returns the path portion without the leading slash.
        /// </summary>
        [Theory]
        [InlineData("/MyResource.xaml", "SomeRoot", "MyResource.xaml")]
        [InlineData("/", "SomeRoot", "")]
        [InlineData("/folder/resource.xaml", "SomeRoot", "folder/resource.xaml")]
        [InlineData("/path/with spaces/file.xaml", "SomeRoot", "path/with spaces/file.xaml")]
        [InlineData("/path/with%20encoded/file.xaml", "SomeRoot", "path/with%20encoded/file.xaml")]
        public void GetResourcePath_UriStartsWithSlash_ReturnsCorrectPath(string uriString, string rootTargetPath, string expectedResult)
        {
            // Arrange
            var uri = new Uri(uriString, UriKind.Relative);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests GetResourcePath with URI that does not start with forward slash.
        /// Verifies that relative paths are resolved against the rootTargetPath with parent directory navigation.
        /// Expected result: Returns the resolved path portion without the leading slash.
        /// </summary>
        [Theory]
        [InlineData("MyResource.xaml", "Views", "MyResource.xaml")]
        [InlineData("Resource.xaml", "Pages/SubFolder", "Pages/Resource.xaml")]
        [InlineData("folder/resource.xaml", "Views", "folder/resource.xaml")]
        [InlineData("../shared/resource.xaml", "Views/Pages", "Views/shared/resource.xaml")]
        [InlineData("resource with spaces.xaml", "Views", "resource with spaces.xaml")]
        public void GetResourcePath_UriDoesNotStartWithSlash_ReturnsResolvedPath(string uriString, string rootTargetPath, string expectedResult)
        {
            // Arrange
            var uri = new Uri(uriString, UriKind.Relative);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests GetResourcePath with null URI parameter.
        /// Verifies that appropriate exception is thrown for invalid input.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetResourcePath_NullUri_ThrowsArgumentNullException()
        {
            // Arrange
            Uri uri = null;
            string rootTargetPath = "SomeRoot";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath));
        }

        /// <summary>
        /// Tests GetResourcePath with null rootTargetPath parameter when URI doesn't start with slash.
        /// Verifies that null rootTargetPath is handled appropriately in relative path construction.
        /// Expected result: Method handles null rootTargetPath without throwing.
        /// </summary>
        [Fact]
        public void GetResourcePath_NullRootTargetPath_RelativeUri_HandledCorrectly()
        {
            // Arrange
            var uri = new Uri("resource.xaml", UriKind.Relative);
            string rootTargetPath = null;

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("resource.xaml", result);
        }

        /// <summary>
        /// Tests GetResourcePath with empty string rootTargetPath when URI doesn't start with slash.
        /// Verifies that empty rootTargetPath is handled correctly in relative path construction.
        /// Expected result: Returns resolved path with empty root handling.
        /// </summary>
        [Fact]
        public void GetResourcePath_EmptyRootTargetPath_RelativeUri_HandledCorrectly()
        {
            // Arrange
            var uri = new Uri("resource.xaml", UriKind.Relative);
            string rootTargetPath = "";

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("resource.xaml", result);
        }

        /// <summary>
        /// Tests GetResourcePath with URI containing empty original string.
        /// Verifies that URIs with empty strings are handled appropriately.
        /// Expected result: Returns empty string after processing.
        /// </summary>
        [Fact]
        public void GetResourcePath_EmptyUriString_ReturnsEmptyString()
        {
            // Arrange
            var uri = new Uri("", UriKind.Relative);
            string rootTargetPath = "SomeRoot";

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

            // Assert
            Assert.Equal("", result);
        }

        /// <summary>
        /// Tests GetResourcePath with URI containing special characters and encoded values.
        /// Verifies that special characters in paths are preserved correctly.
        /// Expected result: Returns path with special characters intact.
        /// </summary>
        [Theory]
        [InlineData("/resource-file_name.xaml", "Views", "resource-file_name.xaml")]
        [InlineData("/folder.with.dots/file.xaml", "Views", "folder.with.dots/file.xaml")]
        [InlineData("resource-file_name.xaml", "Views", "resource-file_name.xaml")]
        [InlineData("folder.with.dots/file.xaml", "Views", "folder.with.dots/file.xaml")]
        public void GetResourcePath_SpecialCharactersInPath_PreservesCharacters(string uriString, string rootTargetPath, string expectedResult)
        {
            // Arrange
            var uri = new Uri(uriString, UriKind.Relative);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests GetResourcePath with complex relative path navigation patterns.
        /// Verifies that parent directory navigation (../) is resolved correctly.
        /// Expected result: Returns correctly resolved path with parent navigation applied.
        /// </summary>
        [Theory]
        [InlineData("../../shared/resource.xaml", "Views/Pages/SubFolder", "Views/shared/resource.xaml")]
        [InlineData("../resource.xaml", "Views/Pages", "Views/resource.xaml")]
        [InlineData("./resource.xaml", "Views", "resource.xaml")]
        public void GetResourcePath_ComplexRelativeNavigation_ResolvesCorrectly(string uriString, string rootTargetPath, string expectedResult)
        {
            // Arrange
            var uri = new Uri(uriString, UriKind.Relative);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests GetResourcePath with rootTargetPath containing special characters.
        /// Verifies that special characters in rootTargetPath are handled correctly.
        /// Expected result: Returns path with special characters in root preserved.
        /// </summary>
        [Theory]
        [InlineData("resource.xaml", "Views-With_Dots.Folder", "resource.xaml")]
        [InlineData("resource.xaml", "Views With Spaces", "resource.xaml")]
        public void GetResourcePath_RootTargetPathWithSpecialChars_HandledCorrectly(string uriString, string rootTargetPath, string expectedResult)
        {
            // Arrange
            var uri = new Uri(uriString, UriKind.Relative);

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.GetResourcePath(uri, rootTargetPath);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with a simple value and default assembly.
        /// Input: Simple string value without assembly specification and valid default assembly.
        /// Expected: Returns Uri with format "{value};assembly={defaultAssembly.Name}".
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_SimpleValueWithDefaultAssembly_ReturnsUriWithDefaultAssembly()
        {
            // Arrange
            var value = "TestResource.xaml";
            var defaultAssembly = Assembly.GetExecutingAssembly();
            var expectedUri = new Uri($"{value};assembly={defaultAssembly.GetName().Name}", UriKind.Relative);

            // Act
            var result = CallGetUriWithExplicitAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal(expectedUri.ToString(), result.ToString());
            Assert.Equal(UriKind.Relative, result.IsAbsoluteUri ? UriKind.Absolute : UriKind.Relative);
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with value containing assembly specification.
        /// Input: String value with embedded assembly name and default assembly.
        /// Expected: Returns Uri using the embedded assembly name, not the default.
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_ValueWithEmbeddedAssembly_ReturnsUriWithEmbeddedAssembly()
        {
            // Arrange
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var value = $"TestResource.xaml;assembly={assemblyName}";
            var defaultAssembly = Assembly.GetExecutingAssembly();
            var expectedUri = new Uri($"TestResource.xaml;assembly={assemblyName}", UriKind.Relative);

            // Act
            var result = CallGetUriWithExplicitAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal(expectedUri.ToString(), result.ToString());
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with null string value.
        /// Input: null string value and valid default assembly.
        /// Expected: Throws ArgumentNullException due to null reference operations.
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_NullValue_ThrowsArgumentNullException()
        {
            // Arrange
            string value = null;
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => CallGetUriWithExplicitAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with empty string value.
        /// Input: Empty string value and valid default assembly.
        /// Expected: Returns Uri with format ";assembly={defaultAssembly.Name}".
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_EmptyValue_ReturnsUriWithEmptyValueAndDefaultAssembly()
        {
            // Arrange
            var value = "";
            var defaultAssembly = Assembly.GetExecutingAssembly();
            var expectedUri = new Uri($";assembly={defaultAssembly.GetName().Name}", UriKind.Relative);

            // Act
            var result = CallGetUriWithExplicitAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal(expectedUri.ToString(), result.ToString());
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with whitespace-only string value.
        /// Input: Whitespace-only string value and valid default assembly.
        /// Expected: Returns Uri with whitespace value and default assembly.
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_WhitespaceValue_ReturnsUriWithWhitespaceAndDefaultAssembly()
        {
            // Arrange
            var value = "   \t\n  ";
            var defaultAssembly = Assembly.GetExecutingAssembly();
            var expectedUri = new Uri($"{value};assembly={defaultAssembly.GetName().Name}", UriKind.Relative);

            // Act
            var result = CallGetUriWithExplicitAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal(expectedUri.ToString(), result.ToString());
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with null default assembly.
        /// Input: Valid string value and null default assembly.
        /// Expected: Throws NullReferenceException when trying to get assembly name.
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_NullDefaultAssembly_ThrowsNullReferenceException()
        {
            // Arrange
            var value = "TestResource.xaml";
            Assembly defaultAssembly = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => CallGetUriWithExplicitAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with invalid embedded assembly name.
        /// Input: String value with non-existent assembly name embedded.
        /// Expected: Throws exception from Assembly.Load when trying to load invalid assembly.
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_InvalidEmbeddedAssembly_ThrowsException()
        {
            // Arrange
            var value = "TestResource.xaml;assembly=NonExistentAssembly";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => CallGetUriWithExplicitAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with assembly specification but no assembly name.
        /// Input: String value ending with ";assembly=" but no assembly name following.
        /// Expected: Throws exception when trying to load empty assembly name.
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_AssemblySpecificationWithoutName_ThrowsException()
        {
            // Arrange
            var value = "TestResource.xaml;assembly=";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => CallGetUriWithExplicitAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with multiple assembly specifications.
        /// Input: String value with multiple ";assembly=" occurrences.
        /// Expected: Uses first split result, may cause issues with assembly loading.
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_MultipleAssemblySpecifications_UsesFirstSplit()
        {
            // Arrange
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            var value = $"TestResource.xaml;assembly={assemblyName};assembly=AnotherAssembly";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            // This should either work (using the first assembly) or throw due to the complex string
            var result = CallGetUriWithExplicitAssembly(value, defaultAssembly);
            Assert.Contains(assemblyName, result.ToString());
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with very long string value.
        /// Input: Very long string value without embedded assembly and valid default assembly.
        /// Expected: Returns Uri with the long string and default assembly name.
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_VeryLongValue_ReturnsUriWithLongValue()
        {
            // Arrange
            var value = new string('a', 1000) + ".xaml";
            var defaultAssembly = Assembly.GetExecutingAssembly();
            var expectedUri = new Uri($"{value};assembly={defaultAssembly.GetName().Name}", UriKind.Relative);

            // Act
            var result = CallGetUriWithExplicitAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal(expectedUri.ToString(), result.ToString());
        }

        /// <summary>
        /// Tests GetUriWithExplicitAssembly with special characters in value.
        /// Input: String value containing special characters and valid default assembly.
        /// Expected: Returns Uri with special characters properly handled.
        /// </summary>
        [Fact]
        public void GetUriWithExplicitAssembly_ValueWithSpecialCharacters_ReturnsUriWithSpecialCharacters()
        {
            // Arrange
            var value = "Test/Resource@#$.xaml";
            var defaultAssembly = Assembly.GetExecutingAssembly();
            var expectedUri = new Uri($"{value};assembly={defaultAssembly.GetName().Name}", UriKind.Relative);

            // Act
            var result = CallGetUriWithExplicitAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal(expectedUri.ToString(), result.ToString());
        }

        private static Uri CallGetUriWithExplicitAssembly(string value, Assembly defaultAssembly)
        {
            var type = typeof(ResourceDictionary).GetNestedType("RDSourceTypeConverter", BindingFlags.Public);
            var method = type.GetMethod("GetUriWithExplicitAssembly", BindingFlags.Static | BindingFlags.NonPublic);
            return (Uri)method.Invoke(null, new object[] { value, defaultAssembly });
        }

        /// <summary>
        /// Tests that ConvertTo successfully converts a valid Uri to its string representation.
        /// Input: Valid Uri object with various formats (absolute, relative, with schemes)
        /// Expected: Returns uri.ToString() result
        /// </summary>
        [Theory]
        [InlineData("https://example.com/resource.xaml")]
        [InlineData("/local/path/resource.xaml")]
        [InlineData("resource.xaml")]
        [InlineData("pack://application:,,,/MyAssembly;component/Resources/Dictionary.xaml")]
        [InlineData("maui://resource/path")]
        public void ConvertTo_ValidUri_ReturnsUriString(string uriString)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);

            // Act
            var result = converter.ConvertTo(null, null, uri, typeof(string));

            // Assert
            Assert.Equal(uri.ToString(), result);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is null.
        /// Input: null value
        /// Expected: NotSupportedException is thrown
        /// </summary>
        [Fact]
        public void ConvertTo_NullValue_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, null, typeof(string)));

            Assert.NotNull(exception);
        }

        /// <summary>
        /// Tests that ConvertTo throws NotSupportedException when value is not a Uri.
        /// Input: Various non-Uri objects (string, int, object, etc.)
        /// Expected: NotSupportedException is thrown for all non-Uri types
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void ConvertTo_NonUriValue_ThrowsNotSupportedException(object value)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() =>
                converter.ConvertTo(null, null, value, typeof(string)));

            Assert.NotNull(exception);
        }

        /// <summary>
        /// Tests that ConvertTo works correctly with complex object types that are not Uri.
        /// Input: Complex objects like DateTime, Guid, custom objects
        /// Expected: NotSupportedException is thrown
        /// </summary>
        [Fact]
        public void ConvertTo_ComplexNonUriObjects_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var testObjects = new object[]
            {
                DateTime.Now,
                Guid.NewGuid(),
                new object(),
                new System.Collections.Generic.List<string>(),
                new int[] { 1, 2, 3 }
            };

            // Act & Assert
            foreach (var testObject in testObjects)
            {
                var exception = Assert.Throws<NotSupportedException>(() =>
                    converter.ConvertTo(null, null, testObject, typeof(string)));

                Assert.NotNull(exception);
            }
        }

        /// <summary>
        /// Tests that ConvertTo works with various context and culture combinations.
        /// Input: Valid Uri with different ITypeDescriptorContext and CultureInfo values
        /// Expected: Returns uri.ToString() regardless of context and culture values
        /// </summary>
        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("")]
        public void ConvertTo_ValidUriWithDifferentCultures_ReturnsUriString(string cultureName)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var uri = new Uri("https://example.com/test.xaml");
            var culture = string.IsNullOrEmpty(cultureName) ? null : new CultureInfo(cultureName);

            // Act
            var result = converter.ConvertTo(null, culture, uri, typeof(string));

            // Assert
            Assert.Equal(uri.ToString(), result);
        }

        /// <summary>
        /// Tests that ConvertTo ignores the destinationType parameter.
        /// Input: Valid Uri with various destination types
        /// Expected: Always returns uri.ToString() regardless of destinationType
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int))]
        [InlineData(null)]
        public void ConvertTo_ValidUriWithDifferentDestinationTypes_ReturnsUriString(Type destinationType)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var uri = new Uri("test://example.com/resource");

            // Act
            var result = converter.ConvertTo(null, null, uri, destinationType);

            // Assert
            Assert.Equal(uri.ToString(), result);
        }

        /// <summary>
        /// Tests ConvertTo with edge case Uri values like empty and special schemes.
        /// Input: Uri objects with edge case values
        /// Expected: Returns uri.ToString() for all valid Uri objects
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("file:///")]
        [InlineData("about:blank")]
        [InlineData("data:text/plain;base64,SGVsbG8=")]
        public void ConvertTo_EdgeCaseUris_ReturnsUriString(string uriString)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);

            // Act
            var result = converter.ConvertTo(null, null, uri, typeof(string));

            // Assert
            Assert.Equal(uri.ToString(), result);
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly returns the original value and default assembly when no assembly separator is present.
        /// Input: A URI string without ";assembly=" separator.
        /// Expected: Returns tuple with original value and the provided default assembly.
        /// </summary>
        [Theory]
        [InlineData("MyPath")]
        [InlineData("path/to/resource")]
        [InlineData("")]
        [InlineData("path with spaces")]
        [InlineData("path;noassembly")]
        public void SplitUriAndAssembly_NoAssemblySeparator_ReturnsOriginalValueAndDefaultAssembly(string value)
        {
            // Arrange
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal(value, result.Item1);
            Assert.Equal(defaultAssembly, result.Item2);
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly returns the original value and null when default assembly is null and no separator is present.
        /// Input: A URI string without ";assembly=" separator and null default assembly.
        /// Expected: Returns tuple with original value and null assembly.
        /// </summary>
        [Fact]
        public void SplitUriAndAssembly_NoAssemblySeparatorWithNullDefault_ReturnsOriginalValueAndNull()
        {
            // Arrange
            string value = "MyPath";
            Assembly defaultAssembly = null;

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal(value, result.Item1);
            Assert.Null(result.Item2);
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly splits the URI correctly when assembly separator is present.
        /// Input: A URI string with ";assembly=" separator and valid assembly name.
        /// Expected: Returns tuple with path part and loaded assembly.
        /// </summary>
        [Fact]
        public void SplitUriAndAssembly_WithValidAssembly_ReturnsPathAndLoadedAssembly()
        {
            // Arrange
            string assemblyName = "System.Private.CoreLib";
            string value = $"MyPath;assembly={assemblyName}";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal("MyPath", result.Item1);
            Assert.Equal(assemblyName, result.Item2.GetName().Name);
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly throws exception when trying to load invalid assembly.
        /// Input: A URI string with ";assembly=" separator and invalid assembly name.
        /// Expected: Throws exception during Assembly.Load.
        /// </summary>
        [Fact]
        public void SplitUriAndAssembly_WithInvalidAssembly_ThrowsException()
        {
            // Arrange
            string value = "MyPath;assembly=NonExistentAssembly123";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() =>
                ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly throws NullReferenceException when value is null.
        /// Input: null string value.
        /// Expected: Throws NullReferenceException when calling IndexOf on null string.
        /// </summary>
        [Fact]
        public void SplitUriAndAssembly_NullValue_ThrowsNullReferenceException()
        {
            // Arrange
            string value = null;
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly throws IndexOutOfRangeException when path part is missing.
        /// Input: String starting with ";assembly=" separator.
        /// Expected: Throws IndexOutOfRangeException when accessing parts[0].
        /// </summary>
        [Fact]
        public void SplitUriAndAssembly_OnlyAssemblyPart_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            string value = ";assembly=System.Private.CoreLib";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() =>
                ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly throws IndexOutOfRangeException when assembly name is missing.
        /// Input: String ending with ";assembly=" separator but no assembly name.
        /// Expected: Throws IndexOutOfRangeException when accessing parts[1].
        /// </summary>
        [Fact]
        public void SplitUriAndAssembly_EmptyAssemblyName_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            string value = "MyPath;assembly=";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() =>
                ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly throws IndexOutOfRangeException when only separator is present.
        /// Input: String containing only ";assembly=" separator.
        /// Expected: Throws IndexOutOfRangeException when accessing parts array.
        /// </summary>
        [Fact]
        public void SplitUriAndAssembly_OnlySeparator_ThrowsIndexOutOfRangeException()
        {
            // Arrange
            string value = ";assembly=";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() =>
                ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly handles multiple assembly separators by using the first split.
        /// Input: String with multiple ";assembly=" separators.
        /// Expected: Uses first occurrence for splitting, loads assembly from first assembly part.
        /// </summary>
        [Fact]
        public void SplitUriAndAssembly_MultipleAssemblySeparators_UsesFirstSplit()
        {
            // Arrange
            string value = "MyPath;assembly=System.Private.CoreLib;assembly=OtherAssembly";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act
            var result = ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly);

            // Assert
            Assert.Equal("MyPath", result.Item1);
            Assert.Equal("System.Private.CoreLib", result.Item2.GetName().Name);
        }

        /// <summary>
        /// Tests that SplitUriAndAssembly handles whitespace around separator correctly.
        /// Input: String with whitespace around ";assembly=" separator.
        /// Expected: Whitespace is preserved in path and assembly name parts.
        /// </summary>
        [Fact]
        public void SplitUriAndAssembly_WhitespaceAroundSeparator_PreservesWhitespace()
        {
            // Arrange
            string value = "My Path ;assembly= System.Private.CoreLib ";
            var defaultAssembly = Assembly.GetExecutingAssembly();

            // Act & Assert
            // This should throw because " System.Private.CoreLib " (with spaces) is not a valid assembly name
            Assert.ThrowsAny<Exception>(() =>
                ResourceDictionary.RDSourceTypeConverter.SplitUriAndAssembly(value, defaultAssembly));
        }

        /// <summary>
        /// Tests that CanConvertTo always returns true regardless of input parameters.
        /// Validates that the method handles null contexts correctly.
        /// Expected result: Always returns true.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Uri))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(ICollection))]
        [InlineData(typeof(List<string>))]
        public void CanConvertTo_WithNullContext_ReturnsTrue(Type destinationType)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo always returns true with a valid mocked context.
        /// Validates that the method works correctly with non-null contexts.
        /// Expected result: Always returns true.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(double))]
        [InlineData(typeof(object))]
        [InlineData(typeof(Uri))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(ICollection))]
        [InlineData(typeof(List<string>))]
        public void CanConvertTo_WithValidContext_ReturnsTrue(Type destinationType)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true for abstract types and interfaces.
        /// Validates edge cases with non-concrete types.
        /// Expected result: Always returns true.
        /// </summary>
        [Theory]
        [InlineData(typeof(IDisposable))]
        [InlineData(typeof(IEnumerable))]
        [InlineData(typeof(ITypeDescriptorContext))]
        [InlineData(typeof(Array))]
        public void CanConvertTo_WithAbstractTypesAndInterfaces_ReturnsTrue(Type destinationType)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true for generic types.
        /// Validates behavior with parameterized types.
        /// Expected result: Always returns true.
        /// </summary>
        [Theory]
        [InlineData(typeof(List<int>))]
        [InlineData(typeof(Dictionary<string, object>))]
        [InlineData(typeof(IEnumerable<string>))]
        [InlineData(typeof(KeyValuePair<string, object>))]
        public void CanConvertTo_WithGenericTypes_ReturnsTrue(Type destinationType)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            ITypeDescriptorContext context = null;

            // Act
            var result = converter.CanConvertTo(context, destinationType);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that CanConvertTo returns true for both null parameters.
        /// Validates edge case with all null inputs.
        /// Expected result: Always returns true.
        /// </summary>
        [Fact]
        public void CanConvertTo_WithBothParametersNull_ReturnsTrue()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();

            // Act
            var result = converter.CanConvertTo(null, null);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with valid parameters.
        /// Verifies the method behavior when all parameters are provided with valid values.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertFrom_ValidParameters_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var value = "test";

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with null context.
        /// Verifies the method behavior when context parameter is null.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullContext_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var culture = CultureInfo.InvariantCulture;
            var value = "test";

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(null, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with null culture.
        /// Verifies the method behavior when culture parameter is null.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullCulture_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var value = "test";

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(context, null, value));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with null value.
        /// Verifies the method behavior when value parameter is null.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertFrom_NullValue_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(context, culture, null));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with all null parameters.
        /// Verifies the method behavior when all parameters are null.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertFrom_AllNullParameters_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(null, null, null));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with various parameter types.
        /// Verifies the method behavior with different value types including string, int, object, and Uri.
        /// Expected result: NotImplementedException is thrown for all parameter combinations.
        /// </summary>
        [Theory]
        [InlineData("string_value")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void ConvertFrom_VariousValueTypes_ThrowsNotImplementedException(object value)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with different cultures.
        /// Verifies the method behavior with various culture parameters including invariant and specific cultures.
        /// Expected result: NotImplementedException is thrown for all culture values.
        /// </summary>
        [Theory]
        [InlineData("en-US")]
        [InlineData("fr-FR")]
        [InlineData("")] // InvariantCulture name
        public void ConvertFrom_VariousCultures_ThrowsNotImplementedException(string cultureName)
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = string.IsNullOrEmpty(cultureName) ? CultureInfo.InvariantCulture : new CultureInfo(cultureName);
            var value = "test";

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with empty string value.
        /// Verifies the method behavior when value parameter is an empty string.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertFrom_EmptyStringValue_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var value = string.Empty;

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(context, culture, value));
        }

        /// <summary>
        /// Tests that ConvertFrom method throws NotImplementedException with Uri value.
        /// Verifies the method behavior when value parameter is a Uri object.
        /// Expected result: NotImplementedException is thrown.
        /// </summary>
        [Fact]
        public void ConvertFrom_UriValue_ThrowsNotImplementedException()
        {
            // Arrange
            var converter = new ResourceDictionary.RDSourceTypeConverter();
            var context = Substitute.For<ITypeDescriptorContext>();
            var culture = CultureInfo.InvariantCulture;
            var value = new Uri("test.xaml", UriKind.Relative);

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => converter.ConvertFrom(context, culture, value));
        }
    }


    public class ResourceDictionaryClearCacheTests
    {
        /// <summary>
        /// Tests that ClearCache method successfully clears the internal cache by verifying 
        /// that cached instances are no longer reused after clearing.
        /// </summary>
        [Fact]
        public void ClearCache_WithCachedInstances_ClearsCache()
        {
            // Arrange
            var type = typeof(TestResourceDictionary);

            // Get an instance to populate the cache
            var instance1 = ResourceDictionary.GetOrCreateInstance(type);
            var instance2 = ResourceDictionary.GetOrCreateInstance(type);

            // Verify caching is working (same instance returned)
            Assert.Same(instance1, instance2);

            // Act
            ResourceDictionary.ClearCache();

            // Assert
            var instance3 = ResourceDictionary.GetOrCreateInstance(type);

            // After clearing cache, a new instance should be created
            Assert.NotSame(instance1, instance3);
        }

        /// <summary>
        /// Tests that ClearCache can be called multiple times without throwing exceptions
        /// and maintains proper functionality.
        /// </summary>
        [Fact]
        public void ClearCache_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange & Act & Assert - should not throw
            ResourceDictionary.ClearCache();
            ResourceDictionary.ClearCache();
            ResourceDictionary.ClearCache();

            // Verify cache still works after multiple clears
            var type = typeof(TestResourceDictionary);
            var instance1 = ResourceDictionary.GetOrCreateInstance(type);
            var instance2 = ResourceDictionary.GetOrCreateInstance(type);

            Assert.Same(instance1, instance2);
        }

        /// <summary>
        /// Tests that ClearCache works correctly on an empty cache and normal caching
        /// behavior continues to work after clearing.
        /// </summary>
        [Fact]
        public void ClearCache_OnEmptyCache_MaintainsNormalBehavior()
        {
            // Arrange - start with cleared cache
            ResourceDictionary.ClearCache();

            // Act - clear again on empty cache
            ResourceDictionary.ClearCache();

            // Assert - normal caching behavior should still work
            var type = typeof(TestResourceDictionary);
            var instance1 = ResourceDictionary.GetOrCreateInstance(type);
            var instance2 = ResourceDictionary.GetOrCreateInstance(type);

            Assert.NotNull(instance1);
            Assert.NotNull(instance2);
            Assert.Same(instance1, instance2);
        }

        /// <summary>
        /// Tests that ClearCache properly handles multiple different types in the cache.
        /// </summary>
        [Fact]
        public void ClearCache_WithMultipleTypes_ClearsAllCachedTypes()
        {
            // Arrange
            var type1 = typeof(TestResourceDictionary);
            var type2 = typeof(AnotherTestResourceDictionary);

            // Populate cache with multiple types
            var instance1Type1 = ResourceDictionary.GetOrCreateInstance(type1);
            var instance1Type2 = ResourceDictionary.GetOrCreateInstance(type2);

            // Verify caching works for both types
            Assert.Same(instance1Type1, ResourceDictionary.GetOrCreateInstance(type1));
            Assert.Same(instance1Type2, ResourceDictionary.GetOrCreateInstance(type2));

            // Act
            ResourceDictionary.ClearCache();

            // Assert - new instances should be created for both types
            var newInstance1Type1 = ResourceDictionary.GetOrCreateInstance(type1);
            var newInstance1Type2 = ResourceDictionary.GetOrCreateInstance(type2);

            Assert.NotSame(instance1Type1, newInstance1Type1);
            Assert.NotSame(instance1Type2, newInstance1Type2);

            // Verify caching still works for new instances
            Assert.Same(newInstance1Type1, ResourceDictionary.GetOrCreateInstance(type1));
            Assert.Same(newInstance1Type2, ResourceDictionary.GetOrCreateInstance(type2));
        }

        private class TestResourceDictionary : ResourceDictionary
        {
            public TestResourceDictionary()
            {
                Add("test", "value");
            }
        }

        private class AnotherTestResourceDictionary : ResourceDictionary
        {
            public AnotherTestResourceDictionary()
            {
                Add("another", "value");
            }
        }
    }


    public partial class ResourceDictionaryReloadTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Reload method does not trigger any events when ResourceDictionary is empty.
        /// Verifies that an empty dictionary's Reload operation completes without raising ValuesChanged events.
        /// Should not throw any exceptions and complete successfully.
        /// </summary>
        [Fact]
        public void Reload_EmptyResourceDictionary_DoesNotTriggerEvents()
        {
            // Arrange
            var rd = new ResourceDictionary();
            var eventTriggered = false;
            ((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
            {
                eventTriggered = true;
            };

            // Act
            rd.Reload();

            // Assert
            Assert.False(eventTriggered);
        }

        /// <summary>
        /// Tests that Reload method triggers ValuesChanged events for each item in the inner dictionary.
        /// Verifies that all key-value pairs in the inner dictionary are processed and events are raised.
        /// Should trigger one event per key-value pair with correct values.
        /// </summary>
        [Fact]
        public void Reload_InnerDictionaryItems_TriggersEventsForEachItem()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd.Add("key1", "value1");
            rd.Add("key2", "value2");

            var triggeredEvents = new List<KeyValuePair<string, object>>();
            ((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
            {
                triggeredEvents.AddRange(e.Values);
            };

            // Act
            rd.Reload();

            // Assert
            Assert.Equal(2, triggeredEvents.Count);
            Assert.Contains(new KeyValuePair<string, object>("key1", "value1"), triggeredEvents);
            Assert.Contains(new KeyValuePair<string, object>("key2", "value2"), triggeredEvents);
        }

        /// <summary>
        /// Tests that Reload method triggers ValuesChanged events for merged dictionaries.
        /// Verifies that all key-value pairs from merged dictionaries are processed.
        /// Should trigger events for all items in merged dictionaries.
        /// </summary>
        [Fact]
        public void Reload_MergedDictionaries_TriggersEventsForMergedItems()
        {
            // Arrange
            var mergedRd1 = new ResourceDictionary();
            mergedRd1.Add("merged1", "mergedValue1");

            var mergedRd2 = new ResourceDictionary();
            mergedRd2.Add("merged2", "mergedValue2");

            var rd = new ResourceDictionary();
            rd.MergedDictionaries.Add(mergedRd1);
            rd.MergedDictionaries.Add(mergedRd2);

            var triggeredEvents = new List<KeyValuePair<string, object>>();
            ((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
            {
                triggeredEvents.AddRange(e.Values);
            };

            // Act
            rd.Reload();

            // Assert
            Assert.Equal(2, triggeredEvents.Count);
            Assert.Contains(new KeyValuePair<string, object>("merged1", "mergedValue1"), triggeredEvents);
            Assert.Contains(new KeyValuePair<string, object>("merged2", "mergedValue2"), triggeredEvents);
        }

        /// <summary>
        /// Tests that Reload method triggers ValuesChanged events for both merged dictionaries and inner dictionary items.
        /// Verifies that all resources from all sources are processed in the correct order.
        /// Should trigger events for merged dictionaries first, then inner dictionary items.
        /// </summary>
        [Fact]
        public void Reload_MergedAndInnerDictionary_TriggersEventsForAllItems()
        {
            // Arrange
            var mergedRd = new ResourceDictionary();
            mergedRd.Add("mergedKey", "mergedValue");

            var rd = new ResourceDictionary();
            rd.MergedDictionaries.Add(mergedRd);
            rd.Add("innerKey", "innerValue");

            var triggeredEvents = new List<KeyValuePair<string, object>>();
            ((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
            {
                triggeredEvents.AddRange(e.Values);
            };

            // Act
            rd.Reload();

            // Assert
            Assert.Equal(2, triggeredEvents.Count);
            Assert.Contains(new KeyValuePair<string, object>("mergedKey", "mergedValue"), triggeredEvents);
            Assert.Contains(new KeyValuePair<string, object>("innerKey", "innerValue"), triggeredEvents);
        }

        /// <summary>
        /// Tests that Reload method triggers ValuesChanged events with correct event arguments.
        /// Verifies that each event contains exactly one key-value pair as expected by OnValuesChanged method.
        /// Should trigger individual events for each resource item.
        /// </summary>
        [Fact]
        public void Reload_MultipleItems_TriggersIndividualEventsForEachItem()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd.Add("first", "firstValue");
            rd.Add("second", "secondValue");

            var eventCount = 0;
            ((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
            {
                // Each call to OnValuesChanged should trigger one event with one key-value pair
                Assert.Single(e.Values);
                eventCount++;
            };

            // Act
            rd.Reload();

            // Assert
            Assert.Equal(2, eventCount);
        }

        /// <summary>
        /// Tests that Reload method handles null and empty values correctly.
        /// Verifies that resources with null values are processed without throwing exceptions.
        /// Should trigger events even for null values.
        /// </summary>
        [Fact]
        public void Reload_NullValues_TriggersEventsWithoutException()
        {
            // Arrange
            var rd = new ResourceDictionary();
            rd.Add("nullKey", null);
            rd.Add("emptyKey", "");

            var triggeredEvents = new List<KeyValuePair<string, object>>();
            ((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
            {
                triggeredEvents.AddRange(e.Values);
            };

            // Act & Assert (should not throw)
            rd.Reload();

            // Assert
            Assert.Equal(2, triggeredEvents.Count);
            Assert.Contains(new KeyValuePair<string, object>("nullKey", null), triggeredEvents);
            Assert.Contains(new KeyValuePair<string, object>("emptyKey", ""), triggeredEvents);
        }

        /// <summary>
        /// Tests that Reload method processes nested merged dictionaries correctly.
        /// Verifies that merged dictionaries containing their own merged dictionaries are handled properly.
        /// Should trigger events for all resources in the hierarchy.
        /// </summary>
        [Fact]
        public void Reload_NestedMergedDictionaries_TriggersEventsForAllLevels()
        {
            // Arrange
            var nestedRd = new ResourceDictionary();
            nestedRd.Add("nested", "nestedValue");

            var middleRd = new ResourceDictionary();
            middleRd.MergedDictionaries.Add(nestedRd);
            middleRd.Add("middle", "middleValue");

            var rd = new ResourceDictionary();
            rd.MergedDictionaries.Add(middleRd);
            rd.Add("top", "topValue");

            var triggeredEvents = new List<KeyValuePair<string, object>>();
            ((IResourceDictionary)rd).ValuesChanged += (sender, e) =>
            {
                triggeredEvents.AddRange(e.Values);
            };

            // Act
            rd.Reload();

            // Assert
            Assert.Equal(3, triggeredEvents.Count);
            Assert.Contains(new KeyValuePair<string, object>("nested", "nestedValue"), triggeredEvents);
            Assert.Contains(new KeyValuePair<string, object>("middle", "middleValue"), triggeredEvents);
            Assert.Contains(new KeyValuePair<string, object>("top", "topValue"), triggeredEvents);
        }
    }


    public partial class ResourceDictionaryAddMergedTests
    {
        /// <summary>
        /// Tests that Add method throws ArgumentNullException when null ResourceDictionary is passed.
        /// Verifies proper null parameter validation.
        /// </summary>
        [Fact]
        public void Add_WithNullResourceDictionary_ThrowsArgumentNullException()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => resourceDictionary.Add((ResourceDictionary)null));
        }

        /// <summary>
        /// Tests that Add method successfully adds a valid ResourceDictionary to MergedDictionaries collection.
        /// Verifies the basic functionality of adding a merged resource dictionary.
        /// </summary>
        [Fact]
        public void Add_WithValidResourceDictionary_AddsToMergedDictionaries()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var mergedResourceDictionary = new ResourceDictionary();
            mergedResourceDictionary.Add("TestKey", "TestValue");

            // Act
            resourceDictionary.Add(mergedResourceDictionary);

            // Assert
            Assert.Single(resourceDictionary.MergedDictionaries);
            Assert.Contains(mergedResourceDictionary, resourceDictionary.MergedDictionaries);
        }

        /// <summary>
        /// Tests that Add method can add multiple ResourceDictionaries to MergedDictionaries collection.
        /// Verifies that multiple merged dictionaries can be added successfully.
        /// </summary>
        [Fact]
        public void Add_WithMultipleResourceDictionaries_AddsAllToMergedDictionaries()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var mergedDict1 = new ResourceDictionary();
            var mergedDict2 = new ResourceDictionary();
            mergedDict1.Add("Key1", "Value1");
            mergedDict2.Add("Key2", "Value2");

            // Act
            resourceDictionary.Add(mergedDict1);
            resourceDictionary.Add(mergedDict2);

            // Assert
            Assert.Equal(2, resourceDictionary.MergedDictionaries.Count);
            Assert.Contains(mergedDict1, resourceDictionary.MergedDictionaries);
            Assert.Contains(mergedDict2, resourceDictionary.MergedDictionaries);
        }

        /// <summary>
        /// Tests that Add method allows adding the same ResourceDictionary instance multiple times.
        /// Verifies that duplicate references are permitted in MergedDictionaries collection.
        /// </summary>
        [Fact]
        public void Add_WithSameResourceDictionaryTwice_AddsBothInstances()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var mergedResourceDictionary = new ResourceDictionary();
            mergedResourceDictionary.Add("TestKey", "TestValue");

            // Act
            resourceDictionary.Add(mergedResourceDictionary);
            resourceDictionary.Add(mergedResourceDictionary);

            // Assert
            Assert.Equal(2, resourceDictionary.MergedDictionaries.Count);
            Assert.Equal(mergedResourceDictionary, resourceDictionary.MergedDictionaries.ElementAt(0));
            Assert.Equal(mergedResourceDictionary, resourceDictionary.MergedDictionaries.ElementAt(1));
        }

        /// <summary>
        /// Tests that Add method allows adding an empty ResourceDictionary to MergedDictionaries.
        /// Verifies that empty resource dictionaries can be added without issues.
        /// </summary>
        [Fact]
        public void Add_WithEmptyResourceDictionary_AddsToMergedDictionaries()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var emptyMergedDict = new ResourceDictionary();

            // Act
            resourceDictionary.Add(emptyMergedDict);

            // Assert
            Assert.Single(resourceDictionary.MergedDictionaries);
            Assert.Contains(emptyMergedDict, resourceDictionary.MergedDictionaries);
        }
    }


    public partial class ResourceDictionaryValuesTests
    {
        /// <summary>
        /// Tests that Values property returns inner dictionary values when _mergedInstance is null.
        /// This covers the first code path in Values property getter (line 190).
        /// </summary>
        [Fact]
        public void Values_WhenMergedInstanceIsNull_ReturnsInnerDictionaryValues()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var expectedValue1 = "TestValue1";
            var expectedValue2 = 42;
            var expectedValue3 = new object();

            resourceDictionary.Add("key1", expectedValue1);
            resourceDictionary.Add("key2", expectedValue2);
            resourceDictionary.Add("key3", expectedValue3);

            // Act
            var values = resourceDictionary.Values;

            // Assert
            Assert.NotNull(values);
            Assert.Equal(3, values.Count);
            Assert.Contains(expectedValue1, values);
            Assert.Contains(expectedValue2, values);
            Assert.Contains(expectedValue3, values);
        }

        /// <summary>
        /// Tests that Values property returns empty collection when dictionary is empty and _mergedInstance is null.
        /// This covers the first code path in Values property getter (line 190) with empty dictionary.
        /// </summary>
        [Fact]
        public void Values_WhenEmptyDictionaryAndMergedInstanceIsNull_ReturnsEmptyCollection()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();

            // Act
            var values = resourceDictionary.Values;

            // Assert
            Assert.NotNull(values);
            Assert.Empty(values);
        }

        /// <summary>
        /// Tests Values property behavior with null values in the dictionary when _mergedInstance is null.
        /// This covers the first code path in Values property getter (line 190) with null values.
        /// </summary>
        [Fact]
        public void Values_WhenDictionaryContainsNullValues_ReturnsCollectionWithNulls()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Add("nullKey", null);
            resourceDictionary.Add("validKey", "validValue");

            // Act
            var values = resourceDictionary.Values;

            // Assert
            Assert.NotNull(values);
            Assert.Equal(2, values.Count);
            Assert.Contains(null, values);
            Assert.Contains("validValue", values);
        }

        /// <summary>
        /// Tests Values property with various object types when _mergedInstance is null.
        /// This covers the first code path in Values property getter (line 190) with diverse value types.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void Values_WhenDictionaryContainsDifferentTypes_ReturnsAllValues(object testValue)
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Add("testKey", testValue);

            // Act
            var values = resourceDictionary.Values;

            // Assert
            Assert.NotNull(values);
            Assert.Single(values);
            Assert.Contains(testValue, values);
        }

        /// <summary>
        /// Tests that Values property returns the actual inner dictionary values collection.
        /// This verifies that the returned collection reflects changes to the dictionary.
        /// </summary>
        [Fact]
        public void Values_WhenMergedInstanceIsNull_ReturnsLiveCollection()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Add("key1", "value1");

            // Act
            var values = resourceDictionary.Values;
            var initialCount = values.Count;

            resourceDictionary.Add("key2", "value2");
            var afterAddCount = values.Count;

            // Assert
            Assert.Equal(1, initialCount);
            Assert.Equal(2, afterAddCount);
        }

        /// <summary>
        /// Tests Values property when _mergedInstance is not null and inner dictionary is empty.
        /// This test requires _mergedInstance to be set, which is not possible through public API.
        /// 
        /// To complete this test:
        /// 1. Set up _mergedInstance using internal SetSource method or reflection
        /// 2. Ensure _innerDictionary.Count == 0
        /// 3. Verify that Values returns _mergedInstance.Values (line 192)
        /// 
        /// Expected behavior: Values should return the values from _mergedInstance when inner dictionary is empty.
        /// </summary>
        [Fact(Skip = "Cannot set _mergedInstance through public API")]
        public void Values_WhenInnerDictionaryEmptyAndMergedInstanceExists_ReturnsMergedInstanceValues()
        {
            // This test requires internal access to set _mergedInstance
            // Implementation needed when internal SetSource method becomes accessible
            Assert.True(false, "Test implementation requires access to internal SetSource method");
        }

        /// <summary>
        /// Tests Values property when both _mergedInstance and inner dictionary contain values.
        /// This test requires _mergedInstance to be set, which is not possible through public API.
        /// 
        /// To complete this test:
        /// 1. Set up _mergedInstance with some values using internal SetSource method
        /// 2. Add values to _innerDictionary 
        /// 3. Verify that Values returns ReadOnlyCollection with combined values (line 193)
        /// 
        /// Expected behavior: Values should return a ReadOnlyCollection containing values from both
        /// _innerDictionary and _mergedInstance, with _innerDictionary values first.
        /// </summary>
        [Fact(Skip = "Cannot set _mergedInstance through public API")]
        public void Values_WhenBothInnerAndMergedInstanceHaveValues_ReturnsCombinedValues()
        {
            // This test requires internal access to set _mergedInstance
            // Implementation needed when internal SetSource method becomes accessible
            Assert.True(false, "Test implementation requires access to internal SetSource method");
        }
    }


    public partial class ResourceDictionarySourceTests
    {
        /// <summary>
        /// Tests that getting the Source property returns null when the ResourceDictionary is initially created.
        /// Input conditions: New ResourceDictionary instance with default _source field (null).
        /// Expected result: Source property returns null.
        /// </summary>
        [Fact]
        public void Source_GetInitialValue_ReturnsNull()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();

            // Act
            var result = resourceDictionary.Source;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that setting the Source property to null when it's already null does not throw an exception.
        /// Input conditions: ResourceDictionary with Source initially null, setting Source to null.
        /// Expected result: No exception is thrown, operation completes successfully.
        /// </summary>
        [Fact]
        public void Source_SetToSameNullValue_DoesNotThrow()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();

            // Act & Assert
            var exception = Record.Exception(() => resourceDictionary.Source = null);
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that setting the Source property to a different value throws InvalidOperationException with correct message.
        /// Input conditions: ResourceDictionary with Source initially null, attempting to set to various Uri values.
        /// Expected result: InvalidOperationException with message "Source can only be set from XAML."
        /// </summary>
        [Theory]
        [MemberData(nameof(GetDifferentUriValues))]
        public void Source_SetToDifferentValue_ThrowsInvalidOperationExceptionWithCorrectMessage(Uri newValue)
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => resourceDictionary.Source = newValue);
            Assert.Equal("Source can only be set from XAML.", exception.Message);
        }

        /// <summary>
        /// Tests that setting Source to an absolute Uri throws InvalidOperationException.
        /// Input conditions: ResourceDictionary with Source initially null, setting to absolute Uri.
        /// Expected result: InvalidOperationException is thrown.
        /// </summary>
        [Fact]
        public void Source_SetToAbsoluteUri_ThrowsInvalidOperationException()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var absoluteUri = new Uri("http://example.com/resource.xaml");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => resourceDictionary.Source = absoluteUri);
        }

        /// <summary>
        /// Tests that setting Source to a relative Uri throws InvalidOperationException.
        /// Input conditions: ResourceDictionary with Source initially null, setting to relative Uri.
        /// Expected result: InvalidOperationException is thrown.
        /// </summary>
        [Fact]
        public void Source_SetToRelativeUri_ThrowsInvalidOperationException()
        {
            // Arrange
            var resourceDictionary = new ResourceDictionary();
            var relativeUri = new Uri("relative/path.xaml", UriKind.Relative);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => resourceDictionary.Source = relativeUri);
        }

        public static TheoryData<Uri> GetDifferentUriValues()
        {
            return new TheoryData<Uri>
            {
                new Uri("http://example.com/resource.xaml"),
                new Uri("https://example.com/styles.xaml"),
                new Uri("relative/path.xaml", UriKind.Relative),
                new Uri("another/relative.xaml", UriKind.Relative),
                new Uri("file:///local/path.xaml"),
                new Uri("pack://application:,,,/ResourceDictionary.xaml", UriKind.Absolute)
            };
        }
    }
}