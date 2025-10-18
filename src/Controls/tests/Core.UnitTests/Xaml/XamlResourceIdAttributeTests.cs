#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.Maui.Controls.Xaml;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Xaml
{
    public sealed class XamlResourceIdAttributeTests
    {
        /// <summary>
        /// Tests GetResourceIdForType when type parameter is null.
        /// Should throw NullReferenceException when accessing type.Assembly.
        /// </summary>
        [Fact]
        public void GetResourceIdForType_NullType_ThrowsNullReferenceException()
        {
            // Arrange
            Type nullType = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => XamlResourceIdAttribute.GetResourceIdForType(nullType));
        }

        /// <summary>
        /// Tests GetResourceIdForType when assembly has no XamlResourceIdAttribute attributes.
        /// Should return null when no custom attributes are found.
        /// </summary>
        [Fact]
        public void GetResourceIdForType_AssemblyWithNoAttributes_ReturnsNull()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new XamlResourceIdAttribute[0]);

            var type = Substitute.For<Type>();
            type.Assembly.Returns(assembly);

            // Act
            string result = XamlResourceIdAttribute.GetResourceIdForType(type);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetResourceIdForType when assembly has XamlResourceIdAttribute but no matching type.
        /// Should return null when no attribute matches the specified type.
        /// </summary>
        [Fact]
        public void GetResourceIdForType_AssemblyWithNonMatchingAttributes_ReturnsNull()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var targetType = Substitute.For<Type>();
            var otherType = Substitute.For<Type>();

            var attribute = new XamlResourceIdAttribute("ResourceId1", "Path1", otherType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            targetType.Assembly.Returns(assembly);

            // Act
            string result = XamlResourceIdAttribute.GetResourceIdForType(targetType);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests GetResourceIdForType when assembly has matching XamlResourceIdAttribute.
        /// Should return the ResourceId of the matching attribute.
        /// </summary>
        [Fact]
        public void GetResourceIdForType_AssemblyWithMatchingAttribute_ReturnsResourceId()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var targetType = Substitute.For<Type>();
            const string expectedResourceId = "TestResourceId";

            var attribute = new XamlResourceIdAttribute(expectedResourceId, "TestPath", targetType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            targetType.Assembly.Returns(assembly);

            // Act
            string result = XamlResourceIdAttribute.GetResourceIdForType(targetType);

            // Assert
            Assert.Equal(expectedResourceId, result);
        }

        /// <summary>
        /// Tests GetResourceIdForType when assembly has multiple XamlResourceIdAttribute with one matching.
        /// Should return the ResourceId of the first matching attribute found.
        /// </summary>
        [Fact]
        public void GetResourceIdForType_AssemblyWithMultipleAttributesOneMatching_ReturnsFirstMatchingResourceId()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var targetType = Substitute.For<Type>();
            var otherType1 = Substitute.For<Type>();
            var otherType2 = Substitute.For<Type>();
            const string expectedResourceId = "MatchingResourceId";

            var attribute1 = new XamlResourceIdAttribute("NonMatching1", "Path1", otherType1);
            var attribute2 = new XamlResourceIdAttribute(expectedResourceId, "Path2", targetType);
            var attribute3 = new XamlResourceIdAttribute("NonMatching2", "Path3", otherType2);

            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute1, attribute2, attribute3 });

            targetType.Assembly.Returns(assembly);

            // Act
            string result = XamlResourceIdAttribute.GetResourceIdForType(targetType);

            // Assert
            Assert.Equal(expectedResourceId, result);
        }

        /// <summary>
        /// Tests GetResourceIdForType when assembly has multiple matching XamlResourceIdAttribute.
        /// Should return the ResourceId of the first matching attribute encountered during iteration.
        /// </summary>
        [Fact]
        public void GetResourceIdForType_AssemblyWithMultipleMatchingAttributes_ReturnsFirstMatchingResourceId()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var targetType = Substitute.For<Type>();
            const string firstResourceId = "FirstResourceId";
            const string secondResourceId = "SecondResourceId";

            var attribute1 = new XamlResourceIdAttribute(firstResourceId, "Path1", targetType);
            var attribute2 = new XamlResourceIdAttribute(secondResourceId, "Path2", targetType);

            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute1, attribute2 });

            targetType.Assembly.Returns(assembly);

            // Act
            string result = XamlResourceIdAttribute.GetResourceIdForType(targetType);

            // Assert
            Assert.Equal(firstResourceId, result);
        }

        /// <summary>
        /// Tests GetResourceIdForType with empty ResourceId in matching attribute.
        /// Should return empty string when matching attribute has empty ResourceId.
        /// </summary>
        [Fact]
        public void GetResourceIdForType_MatchingAttributeWithEmptyResourceId_ReturnsEmptyString()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var targetType = Substitute.For<Type>();
            const string emptyResourceId = "";

            var attribute = new XamlResourceIdAttribute(emptyResourceId, "TestPath", targetType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            targetType.Assembly.Returns(assembly);

            // Act
            string result = XamlResourceIdAttribute.GetResourceIdForType(targetType);

            // Assert
            Assert.Equal(emptyResourceId, result);
        }

        /// <summary>
        /// Tests GetResourceIdForType with null ResourceId in matching attribute.
        /// Should return null when matching attribute has null ResourceId.
        /// </summary>
        [Fact]
        public void GetResourceIdForType_MatchingAttributeWithNullResourceId_ReturnsNull()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var targetType = Substitute.For<Type>();

            var attribute = new XamlResourceIdAttribute(null, "TestPath", targetType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            targetType.Assembly.Returns(assembly);

            // Act
            string result = XamlResourceIdAttribute.GetResourceIdForType(targetType);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPathForType throws ArgumentNullException when type parameter is null.
        /// Verifies proper null parameter validation.
        /// Expected result: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void GetPathForType_NullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => XamlResourceIdAttribute.GetPathForType(type));
        }

        /// <summary>
        /// Tests that GetPathForType returns null when the assembly has no XamlResourceIdAttribute instances.
        /// Verifies behavior when no custom attributes are present on the assembly.
        /// Expected result: null should be returned.
        /// </summary>
        [Fact]
        public void GetPathForType_AssemblyWithNoAttributes_ReturnsNull()
        {
            // Arrange
            var mockType = Substitute.For<Type>();
            var mockAssembly = Substitute.For<Assembly>();

            mockType.Assembly.Returns(mockAssembly);
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new XamlResourceIdAttribute[0]);

            // Act
            var result = XamlResourceIdAttribute.GetPathForType(mockType);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPathForType returns null when assembly has XamlResourceIdAttribute but with different type.
        /// Verifies behavior when attributes exist but none match the input type.
        /// Expected result: null should be returned.
        /// </summary>
        [Fact]
        public void GetPathForType_AssemblyWithNonMatchingAttribute_ReturnsNull()
        {
            // Arrange
            var mockType = Substitute.For<Type>();
            var mockAssembly = Substitute.For<Assembly>();
            var differentType = Substitute.For<Type>();

            var attribute = new XamlResourceIdAttribute("test.resource", "test/path.xaml", differentType);

            mockType.Assembly.Returns(mockAssembly);
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            // Act
            var result = XamlResourceIdAttribute.GetPathForType(mockType);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetPathForType returns the correct path when assembly has matching XamlResourceIdAttribute.
        /// Verifies successful retrieval of path from matching attribute.
        /// Expected result: The path from the matching attribute should be returned.
        /// </summary>
        [Fact]
        public void GetPathForType_AssemblyWithMatchingAttribute_ReturnsCorrectPath()
        {
            // Arrange
            var mockType = Substitute.For<Type>();
            var mockAssembly = Substitute.For<Assembly>();
            var expectedPath = "Views/TestView.xaml";

            var attribute = new XamlResourceIdAttribute("test.resource", expectedPath, mockType);

            mockType.Assembly.Returns(mockAssembly);
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            // Act
            var result = XamlResourceIdAttribute.GetPathForType(mockType);

            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that GetPathForType returns the path from the first matching attribute when multiple attributes exist.
        /// Verifies behavior with multiple attributes where the first one matches.
        /// Expected result: The path from the first matching attribute should be returned.
        /// </summary>
        [Fact]
        public void GetPathForType_MultipleAttributesFirstMatches_ReturnsFirstMatchPath()
        {
            // Arrange
            var mockType = Substitute.For<Type>();
            var mockAssembly = Substitute.For<Assembly>();
            var otherType = Substitute.For<Type>();
            var expectedPath = "First/Match/Path.xaml";

            var firstAttribute = new XamlResourceIdAttribute("first.resource", expectedPath, mockType);
            var secondAttribute = new XamlResourceIdAttribute("second.resource", "Second/Path.xaml", otherType);

            mockType.Assembly.Returns(mockAssembly);
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { firstAttribute, secondAttribute });

            // Act
            var result = XamlResourceIdAttribute.GetPathForType(mockType);

            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that GetPathForType returns the path from the second matching attribute when first doesn't match.
        /// Verifies behavior with multiple attributes where only the second one matches.
        /// Expected result: The path from the second matching attribute should be returned.
        /// </summary>
        [Fact]
        public void GetPathForType_MultipleAttributesSecondMatches_ReturnsSecondMatchPath()
        {
            // Arrange
            var mockType = Substitute.For<Type>();
            var mockAssembly = Substitute.For<Assembly>();
            var otherType = Substitute.For<Type>();
            var expectedPath = "Second/Match/Path.xaml";

            var firstAttribute = new XamlResourceIdAttribute("first.resource", "First/Path.xaml", otherType);
            var secondAttribute = new XamlResourceIdAttribute("second.resource", expectedPath, mockType);

            mockType.Assembly.Returns(mockAssembly);
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { firstAttribute, secondAttribute });

            // Act
            var result = XamlResourceIdAttribute.GetPathForType(mockType);

            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that GetPathForType handles empty path correctly when type matches.
        /// Verifies behavior when the matching attribute has an empty path.
        /// Expected result: Empty string should be returned.
        /// </summary>
        [Fact]
        public void GetPathForType_MatchingAttributeWithEmptyPath_ReturnsEmptyPath()
        {
            // Arrange
            var mockType = Substitute.For<Type>();
            var mockAssembly = Substitute.For<Assembly>();
            var expectedPath = "";

            var attribute = new XamlResourceIdAttribute("test.resource", expectedPath, mockType);

            mockType.Assembly.Returns(mockAssembly);
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            // Act
            var result = XamlResourceIdAttribute.GetPathForType(mockType);

            // Assert
            Assert.Equal(expectedPath, result);
        }

        /// <summary>
        /// Tests that GetPathForType handles null path correctly when type matches.
        /// Verifies behavior when the matching attribute has a null path.
        /// Expected result: null should be returned.
        /// </summary>
        [Fact]
        public void GetPathForType_MatchingAttributeWithNullPath_ReturnsNull()
        {
            // Arrange
            var mockType = Substitute.For<Type>();
            var mockAssembly = Substitute.For<Assembly>();

            var attribute = new XamlResourceIdAttribute("test.resource", null, mockType);

            mockType.Assembly.Returns(mockAssembly);
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            // Act
            var result = XamlResourceIdAttribute.GetPathForType(mockType);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath throws NullReferenceException when assembly parameter is null.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_NullAssembly_ThrowsNullReferenceException()
        {
            // Arrange
            Assembly assembly = null;
            string path = "test.xaml";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => XamlResourceIdAttribute.GetResourceIdForPath(assembly, path));
        }

        /// <summary>
        /// Tests that GetResourceIdForPath returns null when path parameter is null and no attributes have null path.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_NullPath_ReturnsNull()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "path1", typeof(string)),
                new XamlResourceIdAttribute("resource2", "path2", typeof(int))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string path = null;

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath returns correct ResourceId when path parameter is null and an attribute has null path.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_NullPathWithMatchingNullAttribute_ReturnsResourceId()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "path1", typeof(string)),
                new XamlResourceIdAttribute("resourceNull", null, typeof(int))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string path = null;

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Equal("resourceNull", result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath returns correct ResourceId when path parameter is empty string.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_EmptyPath_ReturnsMatchingResourceId()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "path1", typeof(string)),
                new XamlResourceIdAttribute("resourceEmpty", "", typeof(int))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string path = "";

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Equal("resourceEmpty", result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath returns null when assembly has no XamlResourceIdAttribute attributes.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_AssemblyWithNoAttributes_ReturnsNull()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new List<XamlResourceIdAttribute>());
            string path = "test.xaml";

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath returns null when assembly has attributes but none match the specified path.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_NoMatchingPath_ReturnsNull()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "path1.xaml", typeof(string)),
                new XamlResourceIdAttribute("resource2", "path2.xaml", typeof(int))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string path = "nonexistent.xaml";

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath returns correct ResourceId when assembly has one matching attribute.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_SingleMatchingAttribute_ReturnsCorrectResourceId()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "path1.xaml", typeof(string)),
                new XamlResourceIdAttribute("targetResource", "target.xaml", typeof(int)),
                new XamlResourceIdAttribute("resource3", "path3.xaml", typeof(double))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string path = "target.xaml";

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Equal("targetResource", result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath returns the first matching ResourceId when multiple attributes have the same path.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_MultipleMatchingAttributes_ReturnsFirstMatch()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("firstMatch", "duplicate.xaml", typeof(string)),
                new XamlResourceIdAttribute("secondMatch", "duplicate.xaml", typeof(int)),
                new XamlResourceIdAttribute("resource3", "path3.xaml", typeof(double))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string path = "duplicate.xaml";

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Equal("firstMatch", result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath handles paths with special characters correctly.
        /// </summary>
        [Theory]
        [InlineData("path with spaces.xaml", "spaceResource")]
        [InlineData("path/with/slashes.xaml", "slashResource")]
        [InlineData("path\\with\\backslashes.xaml", "backslashResource")]
        [InlineData("path.with.dots.xaml", "dotResource")]
        [InlineData("path-with-dashes.xaml", "dashResource")]
        [InlineData("path_with_underscores.xaml", "underscoreResource")]
        [InlineData("päth_wîth_ünïcödé.xaml", "unicodeResource")]
        public void GetResourceIdForPath_PathWithSpecialCharacters_ReturnsCorrectResourceId(string testPath, string expectedResourceId)
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("spaceResource", "path with spaces.xaml", typeof(string)),
                new XamlResourceIdAttribute("slashResource", "path/with/slashes.xaml", typeof(int)),
                new XamlResourceIdAttribute("backslashResource", "path\\with\\backslashes.xaml", typeof(double)),
                new XamlResourceIdAttribute("dotResource", "path.with.dots.xaml", typeof(float)),
                new XamlResourceIdAttribute("dashResource", "path-with-dashes.xaml", typeof(bool)),
                new XamlResourceIdAttribute("underscoreResource", "path_with_underscores.xaml", typeof(char)),
                new XamlResourceIdAttribute("unicodeResource", "päth_wîth_ünïcödé.xaml", typeof(byte))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, testPath);

            // Assert
            Assert.Equal(expectedResourceId, result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath handles whitespace-only path correctly.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_WhitespaceOnlyPath_ReturnsCorrectResourceId()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "normalpath.xaml", typeof(string)),
                new XamlResourceIdAttribute("whitespaceResource", "   ", typeof(int))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string path = "   ";

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Equal("whitespaceResource", result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath performs case-sensitive path comparison.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_CaseSensitiveComparison_ReturnsNull()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "Test.xaml", typeof(string))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string path = "test.xaml"; // Different case

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetResourceIdForPath returns correct ResourceId for exact case match.
        /// </summary>
        [Fact]
        public void GetResourceIdForPath_ExactCaseMatch_ReturnsCorrectResourceId()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("exactCaseResource", "Test.xaml", typeof(string))
            };
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string path = "Test.xaml"; // Exact case match

            // Act
            var result = XamlResourceIdAttribute.GetResourceIdForPath(assembly, path);

            // Assert
            Assert.Equal("exactCaseResource", result);
        }

        /// <summary>
        /// Tests that GetTypeForResourceId throws NullReferenceException when assembly parameter is null.
        /// Input: null assembly, any resourceId
        /// Expected: NullReferenceException thrown
        /// </summary>
        [Fact]
        public void GetTypeForResourceId_NullAssembly_ThrowsNullReferenceException()
        {
            // Arrange
            Assembly assembly = null;
            string resourceId = "test.resource";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => XamlResourceIdAttribute.GetTypeForResourceId(assembly, resourceId));
        }

        /// <summary>
        /// Tests that GetTypeForResourceId returns null when assembly has no XamlResourceIdAttribute attributes.
        /// Input: assembly with no custom attributes, any resourceId
        /// Expected: null returned
        /// </summary>
        [Theory]
        [InlineData("test.resource")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void GetTypeForResourceId_AssemblyWithNoAttributes_ReturnsNull(string resourceId)
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new XamlResourceIdAttribute[0]);

            // Act
            var result = XamlResourceIdAttribute.GetTypeForResourceId(assembly, resourceId);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetTypeForResourceId returns correct type when resourceId matches an attribute.
        /// Input: assembly with matching XamlResourceIdAttribute, matching resourceId
        /// Expected: Type from matching attribute returned
        /// </summary>
        [Fact]
        public void GetTypeForResourceId_MatchingResourceId_ReturnsCorrectType()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var expectedType = typeof(string);
            var matchingAttribute = new XamlResourceIdAttribute("test.resource", "test.path", expectedType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { matchingAttribute });

            // Act
            var result = XamlResourceIdAttribute.GetTypeForResourceId(assembly, "test.resource");

            // Assert
            Assert.Equal(expectedType, result);
        }

        /// <summary>
        /// Tests that GetTypeForResourceId returns null when resourceId doesn't match any attributes.
        /// Input: assembly with non-matching XamlResourceIdAttribute, non-matching resourceId
        /// Expected: null returned
        /// </summary>
        [Theory]
        [InlineData("different.resource")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void GetTypeForResourceId_NonMatchingResourceId_ReturnsNull(string searchResourceId)
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attribute = new XamlResourceIdAttribute("test.resource", "test.path", typeof(string));
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            // Act
            var result = XamlResourceIdAttribute.GetTypeForResourceId(assembly, searchResourceId);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetTypeForResourceId returns type from first matching attribute when multiple attributes exist.
        /// Input: assembly with multiple XamlResourceIdAttributes where first matches
        /// Expected: Type from first matching attribute returned
        /// </summary>
        [Fact]
        public void GetTypeForResourceId_MultipleAttributesFirstMatches_ReturnsFirstMatchingType()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var firstType = typeof(string);
            var secondType = typeof(int);
            var firstAttribute = new XamlResourceIdAttribute("test.resource", "first.path", firstType);
            var secondAttribute = new XamlResourceIdAttribute("test.resource", "second.path", secondType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { firstAttribute, secondAttribute });

            // Act
            var result = XamlResourceIdAttribute.GetTypeForResourceId(assembly, "test.resource");

            // Assert
            Assert.Equal(firstType, result);
        }

        /// <summary>
        /// Tests that GetTypeForResourceId returns type from second matching attribute when first doesn't match.
        /// Input: assembly with multiple XamlResourceIdAttributes where second matches
        /// Expected: Type from second matching attribute returned
        /// </summary>
        [Fact]
        public void GetTypeForResourceId_MultipleAttributesSecondMatches_ReturnsSecondMatchingType()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var firstType = typeof(string);
            var secondType = typeof(int);
            var firstAttribute = new XamlResourceIdAttribute("first.resource", "first.path", firstType);
            var secondAttribute = new XamlResourceIdAttribute("second.resource", "second.path", secondType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { firstAttribute, secondAttribute });

            // Act
            var result = XamlResourceIdAttribute.GetTypeForResourceId(assembly, "second.resource");

            // Assert
            Assert.Equal(secondType, result);
        }

        /// <summary>
        /// Tests that GetTypeForResourceId handles null ResourceId in attribute when searching for null.
        /// Input: assembly with XamlResourceIdAttribute having null ResourceId, null search resourceId
        /// Expected: Type from attribute with null ResourceId returned
        /// </summary>
        [Fact]
        public void GetTypeForResourceId_AttributeWithNullResourceIdSearchingForNull_ReturnsType()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var expectedType = typeof(string);
            var attribute = new XamlResourceIdAttribute(null, "test.path", expectedType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            // Act
            var result = XamlResourceIdAttribute.GetTypeForResourceId(assembly, null);

            // Assert
            Assert.Equal(expectedType, result);
        }

        /// <summary>
        /// Tests that GetTypeForResourceId handles null ResourceId in attribute when searching for non-null.
        /// Input: assembly with XamlResourceIdAttribute having null ResourceId, non-null search resourceId
        /// Expected: null returned (no match)
        /// </summary>
        [Fact]
        public void GetTypeForResourceId_AttributeWithNullResourceIdSearchingForNonNull_ReturnsNull()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var attribute = new XamlResourceIdAttribute(null, "test.path", typeof(string));
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            // Act
            var result = XamlResourceIdAttribute.GetTypeForResourceId(assembly, "test.resource");

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetTypeForResourceId performs exact string matching including empty strings.
        /// Input: assembly with XamlResourceIdAttribute having empty ResourceId, empty search resourceId
        /// Expected: Type from attribute with empty ResourceId returned
        /// </summary>
        [Fact]
        public void GetTypeForResourceId_ExactEmptyStringMatch_ReturnsType()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var expectedType = typeof(string);
            var attribute = new XamlResourceIdAttribute("", "test.path", expectedType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            // Act
            var result = XamlResourceIdAttribute.GetTypeForResourceId(assembly, "");

            // Assert
            Assert.Equal(expectedType, result);
        }

        /// <summary>
        /// Tests that GetTypeForResourceId performs exact string matching including whitespace strings.
        /// Input: assembly with XamlResourceIdAttribute having whitespace ResourceId, whitespace search resourceId
        /// Expected: Type from attribute with whitespace ResourceId returned
        /// </summary>
        [Fact]
        public void GetTypeForResourceId_ExactWhitespaceMatch_ReturnsType()
        {
            // Arrange
            var assembly = Substitute.For<Assembly>();
            var expectedType = typeof(string);
            var attribute = new XamlResourceIdAttribute("   ", "test.path", expectedType);
            assembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new[] { attribute });

            // Act
            var result = XamlResourceIdAttribute.GetTypeForResourceId(assembly, "   ");

            // Assert
            Assert.Equal(expectedType, result);
        }

        /// <summary>
        /// Tests that GetTypeForPath throws NullReferenceException when assembly parameter is null.
        /// </summary>
        [Fact]
        public void GetTypeForPath_NullAssembly_ThrowsNullReferenceException()
        {
            // Arrange
            Assembly nullAssembly = null;
            string path = "test.xaml";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => XamlResourceIdAttribute.GetTypeForPath(nullAssembly, path));
        }

        /// <summary>
        /// Tests that GetTypeForPath returns null when assembly has no XamlResourceIdAttribute attributes.
        /// </summary>
        [Fact]
        public void GetTypeForPath_AssemblyWithNoAttributes_ReturnsNull()
        {
            // Arrange
            var mockAssembly = Substitute.For<Assembly>();
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(new List<XamlResourceIdAttribute>());
            string path = "test.xaml";

            // Act
            var result = XamlResourceIdAttribute.GetTypeForPath(mockAssembly, path);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetTypeForPath returns null when no attributes match the given path.
        /// </summary>
        [Fact]
        public void GetTypeForPath_NoMatchingPath_ReturnsNull()
        {
            // Arrange
            var mockAssembly = Substitute.For<Assembly>();
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "path1.xaml", typeof(string)),
                new XamlResourceIdAttribute("resource2", "path2.xaml", typeof(int))
            };
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string searchPath = "nonexistent.xaml";

            // Act
            var result = XamlResourceIdAttribute.GetTypeForPath(mockAssembly, searchPath);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetTypeForPath returns correct type when a matching path is found.
        /// </summary>
        [Fact]
        public void GetTypeForPath_MatchingPathExists_ReturnsCorrectType()
        {
            // Arrange
            var mockAssembly = Substitute.For<Assembly>();
            var expectedType = typeof(string);
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "path1.xaml", typeof(int)),
                new XamlResourceIdAttribute("resource2", "test.xaml", expectedType),
                new XamlResourceIdAttribute("resource3", "path3.xaml", typeof(double))
            };
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string searchPath = "test.xaml";

            // Act
            var result = XamlResourceIdAttribute.GetTypeForPath(mockAssembly, searchPath);

            // Assert
            Assert.Equal(expectedType, result);
        }

        /// <summary>
        /// Tests that GetTypeForPath returns first matching type when multiple attributes have the same path.
        /// </summary>
        [Fact]
        public void GetTypeForPath_MultipleMatchingPaths_ReturnsFirstMatch()
        {
            // Arrange
            var mockAssembly = Substitute.For<Assembly>();
            var firstMatchType = typeof(string);
            var secondMatchType = typeof(int);
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource1", "duplicate.xaml", firstMatchType),
                new XamlResourceIdAttribute("resource2", "duplicate.xaml", secondMatchType)
            };
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string searchPath = "duplicate.xaml";

            // Act
            var result = XamlResourceIdAttribute.GetTypeForPath(mockAssembly, searchPath);

            // Assert
            Assert.Equal(firstMatchType, result);
        }

        /// <summary>
        /// Tests GetTypeForPath with various edge case path values including null, empty, and whitespace.
        /// </summary>
        /// <param name="searchPath">The path to search for.</param>
        /// <param name="attributePath">The path stored in the attribute.</param>
        /// <param name="shouldMatch">Whether the paths should match.</param>
        [Theory]
        [InlineData(null, null, true)]
        [InlineData(null, "", false)]
        [InlineData("", null, false)]
        [InlineData("", "", true)]
        [InlineData("   ", "   ", true)]
        [InlineData("test", "test", true)]
        [InlineData("Test", "test", false)]
        [InlineData("path/with/slashes.xaml", "path/with/slashes.xaml", true)]
        [InlineData("path\\with\\backslashes.xaml", "path\\with\\backslashes.xaml", true)]
        [InlineData("path with spaces.xaml", "path with spaces.xaml", true)]
        [InlineData("path-with-special-chars!@#$.xaml", "path-with-special-chars!@#$.xaml", true)]
        public void GetTypeForPath_EdgeCasePaths_HandlesCorrectly(string searchPath, string attributePath, bool shouldMatch)
        {
            // Arrange
            var mockAssembly = Substitute.For<Assembly>();
            var expectedType = typeof(string);
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource", attributePath, expectedType)
            };
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);

            // Act
            var result = XamlResourceIdAttribute.GetTypeForPath(mockAssembly, searchPath);

            // Assert
            if (shouldMatch)
            {
                Assert.Equal(expectedType, result);
            }
            else
            {
                Assert.Null(result);
            }
        }

        /// <summary>
        /// Tests GetTypeForPath with very long path string to ensure no issues with string comparison.
        /// </summary>
        [Fact]
        public void GetTypeForPath_VeryLongPath_HandlesCorrectly()
        {
            // Arrange
            var mockAssembly = Substitute.For<Assembly>();
            var longPath = new string('a', 10000) + ".xaml";
            var expectedType = typeof(string);
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource", longPath, expectedType)
            };
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);

            // Act
            var result = XamlResourceIdAttribute.GetTypeForPath(mockAssembly, longPath);

            // Assert
            Assert.Equal(expectedType, result);
        }

        /// <summary>
        /// Tests GetTypeForPath with path containing Unicode characters.
        /// </summary>
        [Fact]
        public void GetTypeForPath_UnicodeCharactersInPath_HandlesCorrectly()
        {
            // Arrange
            var mockAssembly = Substitute.For<Assembly>();
            var unicodePath = "测试文件名.xaml";
            var expectedType = typeof(string);
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("resource", unicodePath, expectedType)
            };
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);

            // Act
            var result = XamlResourceIdAttribute.GetTypeForPath(mockAssembly, unicodePath);

            // Assert
            Assert.Equal(expectedType, result);
        }

        /// <summary>
        /// Tests GetTypeForPath with single attribute having matching path.
        /// </summary>
        [Fact]
        public void GetTypeForPath_SingleAttributeMatches_ReturnsType()
        {
            // Arrange
            var mockAssembly = Substitute.For<Assembly>();
            var expectedType = typeof(double);
            var attributes = new List<XamlResourceIdAttribute>
            {
                new XamlResourceIdAttribute("singleResource", "single.xaml", expectedType)
            };
            mockAssembly.GetCustomAttributes<XamlResourceIdAttribute>().Returns(attributes);
            string searchPath = "single.xaml";

            // Act
            var result = XamlResourceIdAttribute.GetTypeForPath(mockAssembly, searchPath);

            // Assert
            Assert.Equal(expectedType, result);
        }

        /// <summary>
        /// Tests that the XamlResourceIdAttribute constructor correctly assigns all three parameters to their respective properties
        /// when provided with valid non-null values.
        /// Expected: All properties should be set to the provided parameter values.
        /// </summary>
        [Fact]
        public void Constructor_WithValidParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var resourceId = "TestResource";
            var path = "/test/path";
            var type = typeof(string);

            // Act
            var attribute = new XamlResourceIdAttribute(resourceId, path, type);

            // Assert
            Assert.Equal(resourceId, attribute.ResourceId);
            Assert.Equal(path, attribute.Path);
            Assert.Equal(type, attribute.Type);
        }

        /// <summary>
        /// Tests that the XamlResourceIdAttribute constructor handles null values correctly for all parameters.
        /// Expected: All properties should be set to null without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_WithAllNullParameters_SetsPropertiesToNull()
        {
            // Arrange
            string resourceId = null;
            string path = null;
            Type type = null;

            // Act
            var attribute = new XamlResourceIdAttribute(resourceId, path, type);

            // Assert
            Assert.Null(attribute.ResourceId);
            Assert.Null(attribute.Path);
            Assert.Null(attribute.Type);
        }

        /// <summary>
        /// Tests that the XamlResourceIdAttribute constructor handles various string edge cases correctly.
        /// Expected: Properties should be set to the exact string values provided, including empty and whitespace strings.
        /// </summary>
        [Theory]
        [InlineData("", "", typeof(int))]
        [InlineData(" ", " ", typeof(double))]
        [InlineData("\t\n\r", "\t\n\r", typeof(object))]
        [InlineData("Very long resource id with special characters !@#$%^&*()", "Very/long/path/with/special/characters/!@#$%^&*()", typeof(string))]
        public void Constructor_WithStringEdgeCases_SetsPropertiesCorrectly(string resourceId, string path, Type type)
        {
            // Act
            var attribute = new XamlResourceIdAttribute(resourceId, path, type);

            // Assert
            Assert.Equal(resourceId, attribute.ResourceId);
            Assert.Equal(path, attribute.Path);
            Assert.Equal(type, attribute.Type);
        }

        /// <summary>
        /// Tests that the XamlResourceIdAttribute constructor handles different types correctly including
        /// abstract classes, interfaces, generic types, and sealed types.
        /// Expected: Type property should be set to the exact Type provided.
        /// </summary>
        [Theory]
        [InlineData(typeof(System.Collections.IEnumerable))]
        [InlineData(typeof(System.Collections.Generic.List<>))]
        [InlineData(typeof(System.Collections.Generic.Dictionary<string, int>))]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(void))]
        public void Constructor_WithDifferentTypeParameters_SetsTypePropertyCorrectly(Type type)
        {
            // Arrange
            var resourceId = "TestResource";
            var path = "/test/path";

            // Act
            var attribute = new XamlResourceIdAttribute(resourceId, path, type);

            // Assert
            Assert.Equal(type, attribute.Type);
        }

        /// <summary>
        /// Tests that the XamlResourceIdAttribute constructor handles individual null parameters mixed with valid values.
        /// Expected: Each property should be set to its corresponding parameter value, including null where provided.
        /// </summary>
        [Theory]
        [InlineData(null, "validPath", typeof(string))]
        [InlineData("validResource", null, typeof(int))]
        [InlineData("validResource", "validPath", null)]
        [InlineData(null, null, typeof(object))]
        [InlineData(null, "validPath", null)]
        [InlineData("validResource", null, null)]
        public void Constructor_WithMixedNullAndValidParameters_SetsPropertiesCorrectly(string resourceId, string path, Type type)
        {
            // Act
            var attribute = new XamlResourceIdAttribute(resourceId, path, type);

            // Assert
            Assert.Equal(resourceId, attribute.ResourceId);
            Assert.Equal(path, attribute.Path);
            Assert.Equal(type, attribute.Type);
        }

        /// <summary>
        /// Tests that the XamlResourceIdAttribute constructor handles extremely long strings without issues.
        /// Expected: Properties should be set to the exact long string values provided.
        /// </summary>
        [Fact]
        public void Constructor_WithVeryLongStrings_SetsPropertiesCorrectly()
        {
            // Arrange
            var longResourceId = new string('A', 10000);
            var longPath = new string('B', 10000);
            var type = typeof(DateTime);

            // Act
            var attribute = new XamlResourceIdAttribute(longResourceId, longPath, type);

            // Assert
            Assert.Equal(longResourceId, attribute.ResourceId);
            Assert.Equal(longPath, attribute.Path);
            Assert.Equal(type, attribute.Type);
        }

        /// <summary>
        /// Tests that the XamlResourceIdAttribute constructor handles strings with various control characters and Unicode.
        /// Expected: Properties should be set to the exact string values including control characters.
        /// </summary>
        [Fact]
        public void Constructor_WithControlCharactersAndUnicode_SetsPropertiesCorrectly()
        {
            // Arrange
            var resourceIdWithControls = "Test\0\x01\x02Resource";
            var pathWithUnicode = "/test/🌟/path/ñáéíóú";
            var type = typeof(char);

            // Act
            var attribute = new XamlResourceIdAttribute(resourceIdWithControls, pathWithUnicode, type);

            // Assert
            Assert.Equal(resourceIdWithControls, attribute.ResourceId);
            Assert.Equal(pathWithUnicode, attribute.Path);
            Assert.Equal(type, attribute.Type);
        }
    }
}