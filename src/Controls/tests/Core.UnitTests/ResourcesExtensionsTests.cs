#nullable disable

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ResourcesExtensionsTests
    {
        /// <summary>
        /// Tests that GetMergedResources returns null when the element parameter is null.
        /// </summary>
        [Fact]
        public void GetMergedResources_NullElement_ReturnsNull()
        {
            // Arrange
            IElementDefinition element = null;

            // Act
            var result = ResourcesExtensions.GetMergedResources(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetMergedResources returns null when element has no resources and no parent.
        /// </summary>
        [Fact]
        public void GetMergedResources_ElementWithNoResourcesNoParent_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition>();
            element.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetMergedResources returns null when element is IResourcesProvider but IsResourcesCreated is false.
        /// </summary>
        [Fact]
        public void GetMergedResources_ResourcesProviderNotCreated_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition, IResourcesProvider>();
            var resourcesProvider = (IResourcesProvider)element;
            resourcesProvider.IsResourcesCreated.Returns(false);
            element.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetMergedResources processes resources from IResourcesProvider with simple key-value pairs.
        /// </summary>
        [Fact]
        public void GetMergedResources_ResourcesProviderWithSimpleResources_ReturnsResources()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition, IResourcesProvider>();
            var resourcesProvider = (IResourcesProvider)element;
            var resourceDictionary = Substitute.For<ResourceDictionary>();

            var mergedResources = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("Key1", "Value1"),
                new KeyValuePair<string, object>("Key2", "Value2")
            };

            resourcesProvider.IsResourcesCreated.Returns(true);
            resourcesProvider.Resources.Returns(resourceDictionary);
            resourceDictionary.MergedResources.Returns((IEnumerable<KeyValuePair<string, object>>)mergedResources);
            resourceDictionary.TryGetValue("Key1", out Arg.Any<object>()).Returns(x => { x[1] = "Value1"; return true; });
            resourceDictionary.TryGetValue("Key2", out Arg.Any<object>()).Returns(x => { x[1] = "Value2"; return true; });
            element.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(element);

            // Assert
            Assert.NotNull(result);
            var resultDict = result as Dictionary<string, object>;
            Assert.NotNull(resultDict);
            Assert.Equal(2, resultDict.Count);
            Assert.Equal("Value1", resultDict["Key1"]);
            Assert.Equal("Value2", resultDict["Key2"]);
        }

        /// <summary>
        /// Tests that GetMergedResources handles style class merging for IResourcesProvider resources.
        /// </summary>
        [Fact]
        public void GetMergedResources_ResourcesProviderWithStyleClassMerging_MergesStyleClasses()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition, IResourcesProvider>();
            var resourcesProvider = (IResourcesProvider)element;
            var resourceDictionary = Substitute.For<ResourceDictionary>();

            var styleClassKey = Style.StyleClassPrefix + "TestClass";
            var existingStyles = new List<Style> { new Style(typeof(View)) };
            var newStyles = new List<Style> { new Style(typeof(Button)) };

            var mergedResources = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(styleClassKey, newStyles)
            };

            resourcesProvider.IsResourcesCreated.Returns(true);
            resourcesProvider.Resources.Returns(resourceDictionary);
            resourceDictionary.MergedResources.Returns((IEnumerable<KeyValuePair<string, object>>)mergedResources);
            element.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(element);

            // Assert
            Assert.NotNull(result);
            var resultDict = result as Dictionary<string, object>;
            Assert.NotNull(resultDict);
            Assert.True(resultDict.ContainsKey(styleClassKey));
            var mergedStyleList = resultDict[styleClassKey] as List<Style>;
            Assert.NotNull(mergedStyleList);
            Assert.Single(mergedStyleList);
        }

        /// <summary>
        /// Tests that GetMergedResources processes Application SystemResources and adds them to the result.
        /// This test specifically targets the uncovered line 39: resources.Add(res.Key, res.Value).
        /// </summary>
        [Fact]
        public void GetMergedResources_ApplicationWithSystemResources_AddsSystemResources()
        {
            // Arrange
            var app = Substitute.For<Application>();
            var systemResources = Substitute.For<IResourceDictionary>();

            var systemResourcesList = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("SystemKey1", "SystemValue1"),
                new KeyValuePair<string, object>("SystemKey2", "SystemValue2")
            };

            app.SystemResources.Returns(systemResources);
            systemResources.Returns((IEnumerable<KeyValuePair<string, object>>)systemResourcesList);
            app.RequestedTheme.Returns(Microsoft.Maui.ApplicationModel.AppTheme.Light);
            app.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(app);

            // Assert
            Assert.NotNull(result);
            var resultDict = result as Dictionary<string, object>;
            Assert.NotNull(resultDict);
            Assert.True(resultDict.ContainsKey("SystemKey1"));
            Assert.True(resultDict.ContainsKey("SystemKey2"));
            Assert.Equal("SystemValue1", resultDict["SystemKey1"]);
            Assert.Equal("SystemValue2", resultDict["SystemValue2"]);
            Assert.True(resultDict.ContainsKey(AppThemeBinding.AppThemeResource));
            Assert.Equal(Microsoft.Maui.ApplicationModel.AppTheme.Light, resultDict[AppThemeBinding.AppThemeResource]);
        }

        /// <summary>
        /// Tests that GetMergedResources handles style class merging for Application SystemResources.
        /// This test specifically targets the uncovered lines 40-44 for system resource style class merging.
        /// </summary>
        [Fact]
        public void GetMergedResources_ApplicationWithSystemResourceStyleClasses_MergesStyleClasses()
        {
            // Arrange
            var app = Substitute.For<Application>();
            var systemResources = Substitute.For<IResourceDictionary>();

            var styleClassKey = Style.StyleClassPrefix + "SystemClass";
            var systemStyles = new List<Style> { new Style(typeof(Label)) };

            var systemResourcesList = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>(styleClassKey, systemStyles)
            };

            app.SystemResources.Returns(systemResources);
            systemResources.Returns((IEnumerable<KeyValuePair<string, object>>)systemResourcesList);
            app.RequestedTheme.Returns(Microsoft.Maui.ApplicationModel.AppTheme.Dark);
            app.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(app);

            // Assert
            Assert.NotNull(result);
            var resultDict = result as Dictionary<string, object>;
            Assert.NotNull(resultDict);
            Assert.True(resultDict.ContainsKey(styleClassKey));
            var styleList = resultDict[styleClassKey] as List<Style>;
            Assert.NotNull(styleList);
            Assert.Single(styleList);
            Assert.Equal(typeof(Label), styleList[0].TargetType);
            Assert.True(resultDict.ContainsKey(AppThemeBinding.AppThemeResource));
            Assert.Equal(Microsoft.Maui.ApplicationModel.AppTheme.Dark, resultDict[AppThemeBinding.AppThemeResource]);
        }

        /// <summary>
        /// Tests that GetMergedResources processes Application even when SystemResources is null, still adding RequestedTheme.
        /// </summary>
        [Fact]
        public void GetMergedResources_ApplicationWithNullSystemResources_AddsOnlyRequestedTheme()
        {
            // Arrange
            var app = Substitute.For<Application>();
            app.SystemResources.Returns((IResourceDictionary)null);
            app.RequestedTheme.Returns(Microsoft.Maui.ApplicationModel.AppTheme.Light);
            app.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(app);

            // Assert
            Assert.NotNull(result);
            var resultDict = result as Dictionary<string, object>;
            Assert.NotNull(resultDict);
            Assert.Single(resultDict);
            Assert.True(resultDict.ContainsKey(AppThemeBinding.AppThemeResource));
            Assert.Equal(Microsoft.Maui.ApplicationModel.AppTheme.Light, resultDict[AppThemeBinding.AppThemeResource]);
        }

        /// <summary>
        /// Tests that GetMergedResources walks up the element hierarchy and merges resources from multiple levels.
        /// </summary>
        [Fact]
        public void GetMergedResources_HierarchyWithMultipleLevels_MergesAllResources()
        {
            // Arrange
            var child = Substitute.For<IElementDefinition, IResourcesProvider>();
            var parent = Substitute.For<Element, IResourcesProvider>();
            var childResourcesProvider = (IResourcesProvider)child;
            var parentResourcesProvider = (IResourcesProvider)parent;

            var childResourceDictionary = Substitute.For<ResourceDictionary>();
            var parentResourceDictionary = Substitute.For<ResourceDictionary>();

            var childResources = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("ChildKey", "ChildValue")
            };

            var parentResources = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("ParentKey", "ParentValue")
            };

            childResourcesProvider.IsResourcesCreated.Returns(true);
            childResourcesProvider.Resources.Returns(childResourceDictionary);
            childResourceDictionary.MergedResources.Returns((IEnumerable<KeyValuePair<string, object>>)childResources);
            childResourceDictionary.TryGetValue("ChildKey", out Arg.Any<object>()).Returns(x => { x[1] = "ChildValue"; return true; });

            parentResourcesProvider.IsResourcesCreated.Returns(true);
            parentResourcesProvider.Resources.Returns(parentResourceDictionary);
            parentResourceDictionary.MergedResources.Returns((IEnumerable<KeyValuePair<string, object>>)parentResources);
            parentResourceDictionary.TryGetValue("ParentKey", out Arg.Any<object>()).Returns(x => { x[1] = "ParentValue"; return true; });

            child.Parent.Returns(parent);
            parent.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(child);

            // Assert
            Assert.NotNull(result);
            var resultDict = result as Dictionary<string, object>;
            Assert.NotNull(resultDict);
            Assert.Equal(2, resultDict.Count);
            Assert.Equal("ChildValue", resultDict["ChildKey"]);
            Assert.Equal("ParentValue", resultDict["ParentKey"]);
        }

        /// <summary>
        /// Tests that GetMergedResources handles key conflicts by preserving the first occurrence (child wins over parent).
        /// </summary>
        [Fact]
        public void GetMergedResources_HierarchyWithKeyConflicts_ChildWinsOverParent()
        {
            // Arrange
            var child = Substitute.For<IElementDefinition, IResourcesProvider>();
            var parent = Substitute.For<Element, IResourcesProvider>();
            var childResourcesProvider = (IResourcesProvider)child;
            var parentResourcesProvider = (IResourcesProvider)parent;

            var childResourceDictionary = Substitute.For<ResourceDictionary>();
            var parentResourceDictionary = Substitute.For<ResourceDictionary>();

            var childResources = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("ConflictKey", "ChildValue")
            };

            var parentResources = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("ConflictKey", "ParentValue")
            };

            childResourcesProvider.IsResourcesCreated.Returns(true);
            childResourcesProvider.Resources.Returns(childResourceDictionary);
            childResourceDictionary.MergedResources.Returns((IEnumerable<KeyValuePair<string, object>>)childResources);
            childResourceDictionary.TryGetValue("ConflictKey", out Arg.Any<object>()).Returns(x => { x[1] = "ChildValue"; return true; });

            parentResourcesProvider.IsResourcesCreated.Returns(true);
            parentResourcesProvider.Resources.Returns(parentResourceDictionary);
            parentResourceDictionary.MergedResources.Returns((IEnumerable<KeyValuePair<string, object>>)parentResources);
            parentResourceDictionary.TryGetValue("ConflictKey", out Arg.Any<object>()).Returns(x => { x[1] = "ParentValue"; return true; });

            child.Parent.Returns(parent);
            parent.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(child);

            // Assert
            Assert.NotNull(result);
            var resultDict = result as Dictionary<string, object>;
            Assert.NotNull(resultDict);
            Assert.Single(resultDict);
            Assert.Equal("ChildValue", resultDict["ConflictKey"]); // Child value should win
        }

        /// <summary>
        /// Tests that GetMergedResources handles the case where TryGetValue returns false for a resource.
        /// </summary>
        [Fact]
        public void GetMergedResources_TryGetValueReturnsFalse_SkipsResource()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition, IResourcesProvider>();
            var resourcesProvider = (IResourcesProvider)element;
            var resourceDictionary = Substitute.For<ResourceDictionary>();

            var mergedResources = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("FailKey", "FailValue"),
                new KeyValuePair<string, object>("SuccessKey", "SuccessValue")
            };

            resourcesProvider.IsResourcesCreated.Returns(true);
            resourcesProvider.Resources.Returns(resourceDictionary);
            resourceDictionary.MergedResources.Returns((IEnumerable<KeyValuePair<string, object>>)mergedResources);
            resourceDictionary.TryGetValue("FailKey", out Arg.Any<object>()).Returns(false);
            resourceDictionary.TryGetValue("SuccessKey", out Arg.Any<object>()).Returns(x => { x[1] = "SuccessValue"; return true; });
            element.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(element);

            // Assert
            Assert.NotNull(result);
            var resultDict = result as Dictionary<string, object>;
            Assert.NotNull(resultDict);
            Assert.Single(resultDict);
            Assert.False(resultDict.ContainsKey("FailKey"));
            Assert.True(resultDict.ContainsKey("SuccessKey"));
            Assert.Equal("SuccessValue", resultDict["SuccessKey"]);
        }

        /// <summary>
        /// Tests that GetMergedResources correctly handles complex hierarchy with mixed Application and IResourcesProvider elements.
        /// </summary>
        [Fact]
        public void GetMergedResources_ComplexHierarchyWithApplicationAndResourcesProvider_CombinesAllResources()
        {
            // Arrange
            var child = Substitute.For<IElementDefinition, IResourcesProvider>();
            var app = Substitute.For<Application>();
            var childResourcesProvider = (IResourcesProvider)child;

            var childResourceDictionary = Substitute.For<ResourceDictionary>();
            var systemResources = Substitute.For<IResourceDictionary>();

            var childResources = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("ChildKey", "ChildValue")
            };

            var systemResourcesList = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("SystemKey", "SystemValue")
            };

            childResourcesProvider.IsResourcesCreated.Returns(true);
            childResourcesProvider.Resources.Returns(childResourceDictionary);
            childResourceDictionary.MergedResources.Returns((IEnumerable<KeyValuePair<string, object>>)childResources);
            childResourceDictionary.TryGetValue("ChildKey", out Arg.Any<object>()).Returns(x => { x[1] = "ChildValue"; return true; });

            app.SystemResources.Returns(systemResources);
            systemResources.Returns((IEnumerable<KeyValuePair<string, object>>)systemResourcesList);
            app.RequestedTheme.Returns(Microsoft.Maui.ApplicationModel.AppTheme.Dark);

            child.Parent.Returns(app);
            app.Parent.Returns((Element)null);

            // Act
            var result = ResourcesExtensions.GetMergedResources(child);

            // Assert
            Assert.NotNull(result);
            var resultDict = result as Dictionary<string, object>;
            Assert.NotNull(resultDict);
            Assert.Equal(3, resultDict.Count);
            Assert.Equal("ChildValue", resultDict["ChildKey"]);
            Assert.Equal("SystemValue", resultDict["SystemKey"]);
            Assert.Equal(Microsoft.Maui.ApplicationModel.AppTheme.Dark, resultDict[AppThemeBinding.AppThemeResource]);
        }

        /// <summary>
        /// Tests TryGetResource returns false when element parameter is null.
        /// </summary>
        [Fact]
        public void TryGetResource_NullElement_ReturnsFalseAndNullValue()
        {
            // Arrange
            IElementDefinition element = null;
            string key = "testKey";

            // Act
            bool result = element.TryGetResource(key, out object value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryGetResource with null key parameter.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TryGetResource_NullOrEmptyKey_SearchesWithGivenKey(string key)
        {
            // Arrange
            var element = Substitute.For<IElementDefinition, IResourcesProvider>();
            var resourcesProvider = (IResourcesProvider)element;
            var resources = Substitute.For<ResourceDictionary>();

            resourcesProvider.IsResourcesCreated.Returns(true);
            resourcesProvider.Resources.Returns(resources);
            resources.TryGetValue(key, out Arg.Any<object>()).Returns(false);
            element.Parent.Returns((IElementDefinition)null);

            // Act
            bool result = element.TryGetResource(key, out object value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
            resources.Received(1).TryGetValue(key, out Arg.Any<object>());
        }

        /// <summary>
        /// Tests TryGetResource returns true when resource is found in element's own resources.
        /// </summary>
        [Fact]
        public void TryGetResource_ResourceFoundInElementResources_ReturnsTrueAndValue()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition, IResourcesProvider>();
            var resourcesProvider = (IResourcesProvider)element;
            var resources = Substitute.For<ResourceDictionary>();
            string key = "testKey";
            object expectedValue = "testValue";

            resourcesProvider.IsResourcesCreated.Returns(true);
            resourcesProvider.Resources.Returns(resources);
            resources.TryGetValue(key, out Arg.Any<object>())
                .Returns(x => { x[1] = expectedValue; return true; });

            // Act
            bool result = element.TryGetResource(key, out object value);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, value);
        }

        /// <summary>
        /// Tests TryGetResource when element implements IResourcesProvider but IsResourcesCreated is false.
        /// </summary>
        [Fact]
        public void TryGetResource_ElementResourcesNotCreated_SearchesParent()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition, IResourcesProvider>();
            var parent = Substitute.For<IElementDefinition, IResourcesProvider>();
            var resourcesProvider = (IResourcesProvider)element;
            var parentResourcesProvider = (IResourcesProvider)parent;
            var parentResources = Substitute.For<ResourceDictionary>();
            string key = "testKey";
            object expectedValue = "parentValue";

            resourcesProvider.IsResourcesCreated.Returns(false);
            element.Parent.Returns(parent);
            parentResourcesProvider.IsResourcesCreated.Returns(true);
            parentResourcesProvider.Resources.Returns(parentResources);
            parentResources.TryGetValue(key, out Arg.Any<object>())
                .Returns(x => { x[1] = expectedValue; return true; });

            // Act
            bool result = element.TryGetResource(key, out object value);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, value);
        }

        /// <summary>
        /// Tests TryGetResource when element doesn't implement IResourcesProvider but parent does.
        /// </summary>
        [Fact]
        public void TryGetResource_ElementNotResourceProvider_SearchesParent()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition>();
            var parent = Substitute.For<IElementDefinition, IResourcesProvider>();
            var parentResourcesProvider = (IResourcesProvider)parent;
            var parentResources = Substitute.For<ResourceDictionary>();
            string key = "testKey";
            object expectedValue = "parentValue";

            element.Parent.Returns(parent);
            parentResourcesProvider.IsResourcesCreated.Returns(true);
            parentResourcesProvider.Resources.Returns(parentResources);
            parentResources.TryGetValue(key, out Arg.Any<object>())
                .Returns(x => { x[1] = expectedValue; return true; });

            // Act
            bool result = element.TryGetResource(key, out object value);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, value);
        }

        /// <summary>
        /// Tests TryGetResource finds resource in Application's SystemResources.
        /// This test covers the uncovered line 20 in the source code.
        /// </summary>
        [Fact]
        public void TryGetResource_ResourceFoundInApplicationSystemResources_ReturnsTrueAndValue()
        {
            // Arrange
            var app = Substitute.For<Application>();
            var systemResources = Substitute.For<IResourceDictionary>();
            var element = Substitute.For<IElementDefinition>();
            string key = "testKey";
            object expectedValue = "systemValue";

            element.Parent.Returns((IElementDefinition)null);
            app.SystemResources.Returns(systemResources);
            systemResources.TryGetValue(key, out Arg.Any<object>())
                .Returns(x => { x[1] = expectedValue; return true; });

            // We need to create an element that is an Application to test this path
            var appElement = Substitute.For<IElementDefinition, Application>();
            var applicationInstance = (Application)appElement;
            applicationInstance.SystemResources.Returns(systemResources);
            systemResources.TryGetValue(key, out Arg.Any<object>())
                .Returns(x => { x[1] = expectedValue; return true; });

            // Act
            bool result = appElement.TryGetResource(key, out object value);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, value);
        }

        /// <summary>
        /// Tests TryGetResource when Application has null SystemResources.
        /// </summary>
        [Fact]
        public void TryGetResource_ApplicationSystemResourcesNull_ContinuesSearch()
        {
            // Arrange
            var appElement = Substitute.For<IElementDefinition, Application>();
            var applicationInstance = (Application)appElement;
            string key = "testKey";

            applicationInstance.SystemResources.Returns((IResourceDictionary)null);
            appElement.Parent.Returns((IElementDefinition)null);

            // Act
            bool result = appElement.TryGetResource(key, out object value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryGetResource fallback to Application.Current when no resource found in hierarchy.
        /// </summary>
        [Fact]
        public void TryGetResource_FallbackToApplicationCurrent_ReturnsResourceFromCurrentApp()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition>();
            var currentApp = Substitute.For<Application, IResourcesProvider>();
            var currentAppResourcesProvider = (IResourcesProvider)currentApp;
            var currentAppResources = Substitute.For<ResourceDictionary>();
            string key = "testKey";
            object expectedValue = "currentAppValue";

            element.Parent.Returns((IElementDefinition)null);
            Application.Current = currentApp;
            currentAppResourcesProvider.IsResourcesCreated.Returns(true);
            currentAppResourcesProvider.Resources.Returns(currentAppResources);
            currentAppResources.TryGetValue(key, out Arg.Any<object>())
                .Returns(x => { x[1] = expectedValue; return true; });

            try
            {
                // Act
                bool result = element.TryGetResource(key, out object value);

                // Assert
                Assert.True(result);
                Assert.Equal(expectedValue, value);
            }
            finally
            {
                // Cleanup
                Application.Current = null;
            }
        }

        /// <summary>
        /// Tests TryGetResource when Application.Current is null.
        /// </summary>
        [Fact]
        public void TryGetResource_ApplicationCurrentNull_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition>();
            string key = "testKey";

            element.Parent.Returns((IElementDefinition)null);
            Application.Current = null;

            // Act
            bool result = element.TryGetResource(key, out object value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryGetResource when Application.Current resources are not created.
        /// </summary>
        [Fact]
        public void TryGetResource_ApplicationCurrentResourcesNotCreated_ReturnsFalse()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition>();
            var currentApp = Substitute.For<Application, IResourcesProvider>();
            var currentAppResourcesProvider = (IResourcesProvider)currentApp;
            string key = "testKey";

            element.Parent.Returns((IElementDefinition)null);
            Application.Current = currentApp;
            currentAppResourcesProvider.IsResourcesCreated.Returns(false);

            try
            {
                // Act
                bool result = element.TryGetResource(key, out object value);

                // Assert
                Assert.False(result);
                Assert.Null(value);
            }
            finally
            {
                // Cleanup
                Application.Current = null;
            }
        }

        /// <summary>
        /// Tests TryGetResource with multiple levels in element hierarchy.
        /// </summary>
        [Fact]
        public void TryGetResource_MultiLevelHierarchy_SearchesAllLevels()
        {
            // Arrange
            var grandparent = Substitute.For<IElementDefinition, IResourcesProvider>();
            var parent = Substitute.For<IElementDefinition>();
            var child = Substitute.For<IElementDefinition>();
            var grandparentResourcesProvider = (IResourcesProvider)grandparent;
            var grandparentResources = Substitute.For<ResourceDictionary>();
            string key = "testKey";
            object expectedValue = "grandparentValue";

            child.Parent.Returns(parent);
            parent.Parent.Returns(grandparent);
            grandparent.Parent.Returns((IElementDefinition)null);
            grandparentResourcesProvider.IsResourcesCreated.Returns(true);
            grandparentResourcesProvider.Resources.Returns(grandparentResources);
            grandparentResources.TryGetValue(key, out Arg.Any<object>())
                .Returns(x => { x[1] = expectedValue; return true; });

            // Act
            bool result = child.TryGetResource(key, out object value);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, value);
        }

        /// <summary>
        /// Tests TryGetResource returns false when resource key is not found anywhere.
        /// </summary>
        [Fact]
        public void TryGetResource_ResourceNotFoundAnywhere_ReturnsFalseAndNullValue()
        {
            // Arrange
            var element = Substitute.For<IElementDefinition, IResourcesProvider>();
            var resourcesProvider = (IResourcesProvider)element;
            var resources = Substitute.For<ResourceDictionary>();
            string key = "nonExistentKey";

            resourcesProvider.IsResourcesCreated.Returns(true);
            resourcesProvider.Resources.Returns(resources);
            resources.TryGetValue(key, out Arg.Any<object>()).Returns(false);
            element.Parent.Returns((IElementDefinition)null);
            Application.Current = null;

            // Act
            bool result = element.TryGetResource(key, out object value);

            // Assert
            Assert.False(result);
            Assert.Null(value);
        }

        /// <summary>
        /// Tests TryGetResource with various types of resource values.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        public void TryGetResource_VariousValueTypes_ReturnsCorrectValue(object expectedValue)
        {
            // Arrange
            var element = Substitute.For<IElementDefinition, IResourcesProvider>();
            var resourcesProvider = (IResourcesProvider)element;
            var resources = Substitute.For<ResourceDictionary>();
            string key = "testKey";

            resourcesProvider.IsResourcesCreated.Returns(true);
            resourcesProvider.Resources.Returns(resources);
            resources.TryGetValue(key, out Arg.Any<object>())
                .Returns(x => { x[1] = expectedValue; return true; });

            // Act
            bool result = element.TryGetResource(key, out object value);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, value);
        }
    }
}