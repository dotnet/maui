using System;

using Microsoft.Maui;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
    /// <summary>
    /// Unit tests for the SafeAreaEdges struct constructor that takes individual edge parameters.
    /// </summary>
    public partial class SafeAreaEdgesTests
    {
        /// <summary>
        /// Tests that the four-parameter constructor correctly assigns each parameter to its corresponding property.
        /// Verifies that left parameter is assigned to Left property, top to Top, right to Right, and bottom to Bottom.
        /// </summary>
        /// <param name="left">The value for the left edge</param>
        /// <param name="top">The value for the top edge</param>
        /// <param name="right">The value for the right edge</param>
        /// <param name="bottom">The value for the bottom edge</param>
        [Theory]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.SoftInput, SafeAreaRegions.Container, SafeAreaRegions.All, SafeAreaRegions.Default)]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.Default, SafeAreaRegions.Default, SafeAreaRegions.Default)]
        [InlineData(SafeAreaRegions.Container, SafeAreaRegions.SoftInput, SafeAreaRegions.None, SafeAreaRegions.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithIndividualParameters_AssignsParametersToCorrectProperties(
            SafeAreaRegions left, SafeAreaRegions top, SafeAreaRegions right, SafeAreaRegions bottom)
        {
            // Act
            var safeAreaEdges = new SafeAreaEdges(left, top, right, bottom);

            // Assert
            Assert.Equal(left, safeAreaEdges.Left);
            Assert.Equal(top, safeAreaEdges.Top);
            Assert.Equal(right, safeAreaEdges.Right);
            Assert.Equal(bottom, safeAreaEdges.Bottom);
        }

        /// <summary>
        /// Tests the constructor with each individual enum value to ensure proper assignment.
        /// Verifies that each defined SafeAreaRegions enum value can be used in constructor.
        /// </summary>
        /// <param name="enumValue">The SafeAreaRegions enum value to test</param>
        [Theory]
        [InlineData(SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.SoftInput)]
        [InlineData(SafeAreaRegions.Container)]
        [InlineData(SafeAreaRegions.Default)]
        [InlineData(SafeAreaRegions.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithSameEnumValueForAllEdges_SetsAllPropertiesToSameValue(SafeAreaRegions enumValue)
        {
            // Act
            var safeAreaEdges = new SafeAreaEdges(enumValue, enumValue, enumValue, enumValue);

            // Assert
            Assert.Equal(enumValue, safeAreaEdges.Left);
            Assert.Equal(enumValue, safeAreaEdges.Top);
            Assert.Equal(enumValue, safeAreaEdges.Right);
            Assert.Equal(enumValue, safeAreaEdges.Bottom);
        }

        /// <summary>
        /// Tests the constructor with combined flag values since SafeAreaRegions is a Flags enum.
        /// Verifies that bitwise OR combinations of enum values work correctly.
        /// </summary>
        [Theory]
        [InlineData(SafeAreaRegions.SoftInput | SafeAreaRegions.Container)]
        [InlineData(SafeAreaRegions.SoftInput | SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.Container | SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.SoftInput | SafeAreaRegions.Container | SafeAreaRegions.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithCombinedFlagValues_HandlesFlagCombinationsCorrectly(SafeAreaRegions combinedValue)
        {
            // Act
            var safeAreaEdges = new SafeAreaEdges(combinedValue, SafeAreaRegions.None, combinedValue, SafeAreaRegions.Default);

            // Assert
            Assert.Equal(combinedValue, safeAreaEdges.Left);
            Assert.Equal(SafeAreaRegions.None, safeAreaEdges.Top);
            Assert.Equal(combinedValue, safeAreaEdges.Right);
            Assert.Equal(SafeAreaRegions.Default, safeAreaEdges.Bottom);
        }

        /// <summary>
        /// Tests the constructor with invalid enum values (outside the defined range).
        /// Verifies that the constructor accepts cast integer values even if they're not defined enum values.
        /// </summary>
        [Theory]
        [InlineData(-999)]
        [InlineData(999)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithInvalidEnumValues_AcceptsCastValues(int invalidValue)
        {
            // Arrange
            var invalidEnum = (SafeAreaRegions)invalidValue;

            // Act
            var safeAreaEdges = new SafeAreaEdges(invalidEnum, SafeAreaRegions.None, invalidEnum, SafeAreaRegions.Default);

            // Assert
            Assert.Equal(invalidEnum, safeAreaEdges.Left);
            Assert.Equal(SafeAreaRegions.None, safeAreaEdges.Top);
            Assert.Equal(invalidEnum, safeAreaEdges.Right);
            Assert.Equal(SafeAreaRegions.Default, safeAreaEdges.Bottom);
        }

        /// <summary>
        /// Tests the constructor with boundary enum values including negative Default value.
        /// Verifies that special values like Default (-1) and All (32768) are handled correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithBoundaryEnumValues_HandlesBoundaryValuesCorrectly()
        {
            // Arrange
            var negativeValue = SafeAreaRegions.Default; // -1
            var largeValue = SafeAreaRegions.All; // 32768
            var zeroValue = SafeAreaRegions.None; // 0
            var smallPositiveValue = SafeAreaRegions.SoftInput; // 1

            // Act
            var safeAreaEdges = new SafeAreaEdges(negativeValue, largeValue, zeroValue, smallPositiveValue);

            // Assert
            Assert.Equal(negativeValue, safeAreaEdges.Left);
            Assert.Equal(largeValue, safeAreaEdges.Top);
            Assert.Equal(zeroValue, safeAreaEdges.Right);
            Assert.Equal(smallPositiveValue, safeAreaEdges.Bottom);
        }

        /// <summary>
        /// Tests that two SafeAreaEdges instances created with the same parameters are equal.
        /// Verifies constructor consistency and that value equality works correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithSameParameters_CreateEqualInstances()
        {
            // Arrange
            var left = SafeAreaRegions.SoftInput;
            var top = SafeAreaRegions.Container;
            var right = SafeAreaRegions.All;
            var bottom = SafeAreaRegions.Default;

            // Act
            var safeAreaEdges1 = new SafeAreaEdges(left, top, right, bottom);
            var safeAreaEdges2 = new SafeAreaEdges(left, top, right, bottom);

            // Assert
            Assert.Equal(safeAreaEdges1, safeAreaEdges2);
            Assert.True(safeAreaEdges1.Equals(safeAreaEdges2));
        }

        /// <summary>
        /// Tests that IsSoftInput returns false when region is Default.
        /// This validates the first conditional branch in the method.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsSoftInput_DefaultRegion_ReturnsFalse()
        {
            // Arrange
            var region = SafeAreaRegions.Default;

            // Act
            var result = SafeAreaEdges.IsSoftInput(region);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsSoftInput returns true when region is All.
        /// This validates the second conditional branch in the method.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsSoftInput_AllRegion_ReturnsTrue()
        {
            // Arrange
            var region = SafeAreaRegions.All;

            // Act
            var result = SafeAreaEdges.IsSoftInput(region);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsSoftInput returns the correct result based on bitwise AND operation with SoftInput flag.
        /// This validates the final return statement and bitwise logic.
        /// </summary>
        /// <param name="region">The SafeAreaRegions value to test.</param>
        /// <param name="expected">The expected result of the IsSoftInput method.</param>
        [Theory]
        [InlineData(SafeAreaRegions.None, false)]
        [InlineData(SafeAreaRegions.SoftInput, true)]
        [InlineData(SafeAreaRegions.Container, false)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsSoftInput_VariousRegions_ReturnsBitwiseAndResult(SafeAreaRegions region, bool expected)
        {
            // Act
            var result = SafeAreaEdges.IsSoftInput(region);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests that IsSoftInput returns true when region contains SoftInput flag combined with other flags.
        /// This validates the bitwise AND operation with flag combinations.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsSoftInput_SoftInputCombinedWithOtherFlags_ReturnsTrue()
        {
            // Arrange
            var region = SafeAreaRegions.SoftInput | SafeAreaRegions.Container;

            // Act
            var result = SafeAreaEdges.IsSoftInput(region);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsSoftInput handles invalid enum values correctly.
        /// This validates the method's behavior with out-of-range enum values.
        /// </summary>
        /// <param name="invalidValue">An invalid SafeAreaRegions value cast from int.</param>
        /// <param name="expected">The expected result based on bitwise AND operation.</param>
        [Theory]
        [InlineData(999, true)]
        [InlineData(1, true)]
        [InlineData(3, true)]
        [InlineData(0, false)]
        [Trait("Category", "auto-generated")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        public void IsSoftInput_InvalidEnumValues_ReturnsBitwiseAndResult(int invalidValue, bool expected)
        {
            // Arrange
            var region = (SafeAreaRegions)invalidValue;

            // Act
            var result = SafeAreaEdges.IsSoftInput(region);

            // Assert
            Assert.Equal(expected, result);
        }

        /// <summary>
        /// Tests the IsContainer method with various SafeAreaRegions values to verify correct flag detection.
        /// </summary>
        /// <param name="region">The SafeAreaRegions value to test.</param>
        /// <param name="expectedResult">The expected result of the IsContainer method.</param>
        [Theory]
        [InlineData(SafeAreaRegions.Default, false)]
        [InlineData(SafeAreaRegions.All, true)]
        [InlineData(SafeAreaRegions.None, false)]
        [InlineData(SafeAreaRegions.SoftInput, false)]
        [InlineData(SafeAreaRegions.Container, true)]
        [InlineData(SafeAreaRegions.SoftInput | SafeAreaRegions.Container, true)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsContainer_WithVariousRegions_ReturnsExpectedResult(SafeAreaRegions region, bool expectedResult)
        {
            // Act
            bool result = SafeAreaEdges.IsContainer(region);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests the IsContainer method with the Default value to ensure it returns false.
        /// This specifically tests the first condition in the method.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsContainer_WithDefaultValue_ReturnsFalse()
        {
            // Arrange
            var region = SafeAreaRegions.Default;

            // Act
            bool result = SafeAreaEdges.IsContainer(region);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests the IsContainer method with the All value to ensure it returns true.
        /// This specifically tests the second condition in the method.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsContainer_WithAllValue_ReturnsTrue()
        {
            // Arrange
            var region = SafeAreaRegions.All;

            // Act
            bool result = SafeAreaEdges.IsContainer(region);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests the IsContainer method with a region that has the Container flag set
        /// to verify the bitwise AND operation works correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsContainer_WithContainerFlag_ReturnsTrue()
        {
            // Arrange
            var region = SafeAreaRegions.Container;

            // Act
            bool result = SafeAreaEdges.IsContainer(region);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests the IsContainer method with a region that does not have the Container flag set
        /// to verify the bitwise AND operation correctly identifies missing flags.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsContainer_WithoutContainerFlag_ReturnsFalse()
        {
            // Arrange
            var region = SafeAreaRegions.SoftInput;

            // Act
            bool result = SafeAreaEdges.IsContainer(region);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests the IsContainer method with combined flags including Container
        /// to verify bitwise operations work correctly with multiple flags.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsContainer_WithCombinedFlagsIncludingContainer_ReturnsTrue()
        {
            // Arrange
            var region = SafeAreaRegions.SoftInput | SafeAreaRegions.Container;

            // Act
            bool result = SafeAreaEdges.IsContainer(region);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests the IsContainer method with an invalid enum value cast from an integer
        /// to verify the method handles undefined enum values correctly.
        /// </summary>
        [Theory]
        [InlineData(0, false)] // None
        [InlineData(1, false)] // SoftInput only
        [InlineData(2, true)]  // Container only
        [InlineData(3, true)]  // SoftInput | Container
        [InlineData(32768, true)] // All
        [InlineData(-1, false)] // Default
        [InlineData(100, false)] // Invalid value without Container flag
        [InlineData(102, true)]  // Invalid value with Container flag (100 | 2)
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsContainer_WithCastIntegerValues_ReturnsExpectedResult(int enumValue, bool expectedResult)
        {
            // Arrange
            var region = (SafeAreaRegions)enumValue;

            // Act
            bool result = SafeAreaEdges.IsContainer(region);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests the IsContainer method with extreme integer values cast to SafeAreaRegions
        /// to verify robustness with boundary conditions.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue, false)]
        [InlineData(int.MaxValue, true)] // int.MaxValue & 2 == 2, so has Container flag
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void IsContainer_WithExtremeValues_ReturnsExpectedResult(int enumValue, bool expectedResult)
        {
            // Arrange
            var region = (SafeAreaRegions)enumValue;

            // Act
            bool result = SafeAreaEdges.IsContainer(region);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that GetEdge returns the correct property value for valid edge indices.
        /// Verifies that edge indices 0-3 correctly map to Left, Top, Right, and Bottom properties respectively.
        /// </summary>
        /// <param name="edgeIndex">The edge index to test (0=Left, 1=Top, 2=Right, 3=Bottom).</param>
        /// <param name="expectedRegion">The expected SafeAreaRegions value for the specified edge.</param>
        [Theory]
        [InlineData(0, SafeAreaRegions.None)]
        [InlineData(1, SafeAreaRegions.SoftInput)]
        [InlineData(2, SafeAreaRegions.Container)]
        [InlineData(3, SafeAreaRegions.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetEdge_ValidEdgeIndex_ReturnsCorrectProperty(int edgeIndex, SafeAreaRegions expectedRegion)
        {
            // Arrange
            var safeAreaEdges = new SafeAreaEdges(
                left: SafeAreaRegions.None,
                top: SafeAreaRegions.SoftInput,
                right: SafeAreaRegions.Container,
                bottom: SafeAreaRegions.All);

            // Act
            var result = safeAreaEdges.GetEdge(edgeIndex);

            // Assert
            Assert.Equal(expectedRegion, result);
        }

        /// <summary>
        /// Tests that GetEdge returns SafeAreaRegions.None for invalid edge indices.
        /// Verifies that any edge index outside the valid range (0-3) returns the default None value.
        /// </summary>
        /// <param name="invalidEdgeIndex">An invalid edge index value.</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetEdge_InvalidEdgeIndex_ReturnsNone(int invalidEdgeIndex)
        {
            // Arrange
            var safeAreaEdges = new SafeAreaEdges(
                left: SafeAreaRegions.All,
                top: SafeAreaRegions.Container,
                right: SafeAreaRegions.SoftInput,
                bottom: SafeAreaRegions.Default);

            // Act
            var result = safeAreaEdges.GetEdge(invalidEdgeIndex);

            // Assert
            Assert.Equal(SafeAreaRegions.None, result);
        }

        /// <summary>
        /// Tests that GetEdge correctly returns different property values when edges have distinct values.
        /// Verifies that each edge property maintains its independent value and is correctly retrieved.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetEdge_DifferentPropertyValues_ReturnsCorrectValues()
        {
            // Arrange
            var safeAreaEdges = new SafeAreaEdges(
                left: SafeAreaRegions.Default,
                top: SafeAreaRegions.All,
                right: SafeAreaRegions.SoftInput,
                bottom: SafeAreaRegions.Container);

            // Act & Assert
            Assert.Equal(SafeAreaRegions.Default, safeAreaEdges.GetEdge(0)); // Left
            Assert.Equal(SafeAreaRegions.All, safeAreaEdges.GetEdge(1)); // Top
            Assert.Equal(SafeAreaRegions.SoftInput, safeAreaEdges.GetEdge(2)); // Right
            Assert.Equal(SafeAreaRegions.Container, safeAreaEdges.GetEdge(3)); // Bottom
        }

        /// <summary>
        /// Tests that GetEdge returns the same value for all edges when initialized with uniform value.
        /// Verifies that the uniform constructor correctly sets all properties to the same value.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetEdge_UniformValue_ReturnsCorrectValue()
        {
            // Arrange
            var uniformValue = SafeAreaRegions.Container;
            var safeAreaEdges = new SafeAreaEdges(uniformValue);

            // Act & Assert
            Assert.Equal(uniformValue, safeAreaEdges.GetEdge(0)); // Left
            Assert.Equal(uniformValue, safeAreaEdges.GetEdge(1)); // Top
            Assert.Equal(uniformValue, safeAreaEdges.GetEdge(2)); // Right
            Assert.Equal(uniformValue, safeAreaEdges.GetEdge(3)); // Bottom
        }

        /// <summary>
        /// Tests that Equals returns true when comparing two SafeAreaEdges instances with identical values for all properties.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_IdenticalInstances_ReturnsTrue()
        {
            // Arrange
            var edges1 = new SafeAreaEdges(SafeAreaRegions.All);
            var edges2 = new SafeAreaEdges(SafeAreaRegions.All);

            // Act
            bool result = edges1.Equals(edges2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing the same instance with itself.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var edges = new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.Default);

            // Act
            bool result = edges.Equals(edges);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing instances with identical complex configurations.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_IdenticalComplexInstances_ReturnsTrue()
        {
            // Arrange
            var edges1 = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Default, SafeAreaRegions.Container);
            var edges2 = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Default, SafeAreaRegions.Container);

            // Act
            bool result = edges1.Equals(edges2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing instances that differ only in the Left property.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_DifferentLeftProperty_ReturnsFalse()
        {
            // Arrange
            var edges1 = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All);
            var edges2 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All);

            // Act
            bool result = edges1.Equals(edges2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing instances that differ only in the Top property.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_DifferentTopProperty_ReturnsFalse()
        {
            // Arrange
            var edges1 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.All);
            var edges2 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All);

            // Act
            bool result = edges1.Equals(edges2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing instances that differ only in the Right property.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_DifferentRightProperty_ReturnsFalse()
        {
            // Arrange
            var edges1 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All);
            var edges2 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All);

            // Act
            bool result = edges1.Equals(edges2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing instances that differ only in the Bottom property.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_DifferentBottomProperty_ReturnsFalse()
        {
            // Arrange
            var edges1 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.None);
            var edges2 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All);

            // Act
            bool result = edges1.Equals(edges2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing instances that differ in multiple properties.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_DifferentMultipleProperties_ReturnsFalse()
        {
            // Arrange
            var edges1 = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.All);
            var edges2 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.None);

            // Act
            bool result = edges1.Equals(edges2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals returns the expected result when comparing various SafeAreaRegions enum combinations.
        /// </summary>
        /// <param name="left1">Left property value for first instance</param>
        /// <param name="top1">Top property value for first instance</param>
        /// <param name="right1">Right property value for first instance</param>
        /// <param name="bottom1">Bottom property value for first instance</param>
        /// <param name="left2">Left property value for second instance</param>
        /// <param name="top2">Top property value for second instance</param>
        /// <param name="right2">Right property value for second instance</param>
        /// <param name="bottom2">Bottom property value for second instance</param>
        /// <param name="expectedResult">Expected result of the equality comparison</param>
        [Theory]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.Default, SafeAreaRegions.Default, SafeAreaRegions.Default, SafeAreaRegions.Default, SafeAreaRegions.Default, SafeAreaRegions.Default, SafeAreaRegions.Default, true)]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, true)]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, true)]
        [InlineData(SafeAreaRegions.Container, SafeAreaRegions.Container, SafeAreaRegions.Container, SafeAreaRegions.Container, SafeAreaRegions.Container, SafeAreaRegions.Container, SafeAreaRegions.Container, SafeAreaRegions.Container, true)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, true)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, false)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.Default, SafeAreaRegions.All, SafeAreaRegions.Container, false)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.Container, false)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None, false)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_VariousEnumCombinations_ReturnsExpectedResult(
            SafeAreaRegions left1, SafeAreaRegions top1, SafeAreaRegions right1, SafeAreaRegions bottom1,
            SafeAreaRegions left2, SafeAreaRegions top2, SafeAreaRegions right2, SafeAreaRegions bottom2,
            bool expectedResult)
        {
            // Arrange
            var edges1 = new SafeAreaEdges(left1, top1, right1, bottom1);
            var edges2 = new SafeAreaEdges(left2, top2, right2, bottom2);

            // Act
            bool result = edges1.Equals(edges2);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that Equals returns true when comparing static property instances with equivalent constructed instances.
        /// </summary>
        [Theory]
        [InlineData(SafeAreaRegions.Default)]
        [InlineData(SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_StaticPropertiesWithEquivalentInstances_ReturnsTrue(SafeAreaRegions uniformValue)
        {
            // Arrange
            var constructedEdges = new SafeAreaEdges(uniformValue);
            SafeAreaEdges staticEdges = uniformValue switch
            {
                SafeAreaRegions.Default => SafeAreaEdges.Default,
                SafeAreaRegions.None => SafeAreaEdges.None,
                SafeAreaRegions.All => SafeAreaEdges.All,
                _ => new SafeAreaEdges(uniformValue)
            };

            // Act
            bool result = constructedEdges.Equals(staticEdges);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals returns false when comparing different static property instances.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_DifferentStaticProperties_ReturnsFalse()
        {
            // Arrange & Act & Assert
            Assert.False(SafeAreaEdges.Default.Equals(SafeAreaEdges.None));
            Assert.False(SafeAreaEdges.Default.Equals(SafeAreaEdges.All));
            Assert.False(SafeAreaEdges.None.Equals(SafeAreaEdges.All));
            Assert.False(SafeAreaEdges.All.Equals(SafeAreaEdges.Default));
            Assert.False(SafeAreaEdges.All.Equals(SafeAreaEdges.None));
            Assert.False(SafeAreaEdges.None.Equals(SafeAreaEdges.Default));
        }

        /// <summary>
        /// Tests that Equals(object?) returns false when the object is null.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            var edges = new SafeAreaEdges(SafeAreaRegions.All);

            // Act
            var result = edges.Equals((object)null);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object?) returns false when the object is not a SafeAreaEdges instance.
        /// </summary>
        /// <param name="obj">The object to compare with SafeAreaEdges.</param>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_WithNonSafeAreaEdgesObject_ReturnsFalse(object obj)
        {
            // Arrange
            var edges = new SafeAreaEdges(SafeAreaRegions.All);

            // Act
            var result = edges.Equals(obj);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object?) returns false when the SafeAreaEdges objects have different values.
        /// </summary>
        /// <param name="left">The left SafeAreaRegions value.</param>
        /// <param name="top">The top SafeAreaRegions value.</param>
        /// <param name="right">The right SafeAreaRegions value.</param>
        /// <param name="bottom">The bottom SafeAreaRegions value.</param>
        /// <param name="otherLeft">The other left SafeAreaRegions value.</param>
        /// <param name="otherTop">The other top SafeAreaRegions value.</param>
        /// <param name="otherRight">The other right SafeAreaRegions value.</param>
        /// <param name="otherBottom">The other bottom SafeAreaRegions value.</param>
        [Theory]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All,
                   SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All,
                   SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All,
                   SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All,
                   SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None,
                   SafeAreaRegions.Default, SafeAreaRegions.Container, SafeAreaRegions.All, SafeAreaRegions.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_WithDifferentSafeAreaEdges_ReturnsFalse(
            SafeAreaRegions left, SafeAreaRegions top, SafeAreaRegions right, SafeAreaRegions bottom,
            SafeAreaRegions otherLeft, SafeAreaRegions otherTop, SafeAreaRegions otherRight, SafeAreaRegions otherBottom)
        {
            // Arrange
            var edges1 = new SafeAreaEdges(left, top, right, bottom);
            var edges2 = new SafeAreaEdges(otherLeft, otherTop, otherRight, otherBottom);

            // Act
            var result = edges1.Equals((object)edges2);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Equals(object?) returns true when the SafeAreaEdges objects have identical values.
        /// </summary>
        /// <param name="left">The left SafeAreaRegions value.</param>
        /// <param name="top">The top SafeAreaRegions value.</param>
        /// <param name="right">The right SafeAreaRegions value.</param>
        /// <param name="bottom">The bottom SafeAreaRegions value.</param>
        [Theory]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.Container, SafeAreaRegions.All, SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_WithIdenticalSafeAreaEdges_ReturnsTrue(
            SafeAreaRegions left, SafeAreaRegions top, SafeAreaRegions right, SafeAreaRegions bottom)
        {
            // Arrange
            var edges1 = new SafeAreaEdges(left, top, right, bottom);
            var edges2 = new SafeAreaEdges(left, top, right, bottom);

            // Act
            var result = edges1.Equals((object)edges2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object?) returns true when comparing an instance with itself.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_WithSameInstance_ReturnsTrue()
        {
            // Arrange
            var edges = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.Default, SafeAreaRegions.Container);

            // Act
            var result = edges.Equals((object)edges);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object?) returns true when SafeAreaEdges instances are created using different constructors but have the same resulting values.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_WithEquivalentInstancesFromDifferentConstructors_ReturnsTrue()
        {
            // Arrange
            var edges1 = new SafeAreaEdges(SafeAreaRegions.All); // Uniform constructor
            var edges2 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All); // Individual constructor

            // Act
            var result = edges1.Equals((object)edges2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object?) returns true when SafeAreaEdges instances are created using horizontal/vertical constructor with same values.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_WithEquivalentInstancesFromHorizontalVerticalConstructor_ReturnsTrue()
        {
            // Arrange
            var edges1 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None); // Horizontal, Vertical constructor
            var edges2 = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.None); // Individual constructor

            // Act
            var result = edges1.Equals((object)edges2);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that Equals(object?) returns true when comparing with static property instances.
        /// </summary>
        [Theory]
        [InlineData("Default")]
        [InlineData("None")]
        [InlineData("All")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Equals_WithStaticPropertyInstances_ReturnsTrue(string propertyName)
        {
            // Arrange
            SafeAreaEdges staticInstance = propertyName switch
            {
                "Default" => SafeAreaEdges.Default,
                "None" => SafeAreaEdges.None,
                "All" => SafeAreaEdges.All,
                _ => throw new ArgumentException("Invalid property name")
            };

            SafeAreaRegions expectedValue = propertyName switch
            {
                "Default" => SafeAreaRegions.Default,
                "None" => SafeAreaRegions.None,
                "All" => SafeAreaRegions.All,
                _ => throw new ArgumentException("Invalid property name")
            };

            var manualInstance = new SafeAreaEdges(expectedValue);

            // Act
            var result = staticInstance.Equals((object)manualInstance);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GetHashCode returns consistent values for the same object instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetHashCode_SameObject_ReturnsConsistentHashCode()
        {
            // Arrange
            var edges = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.Container, SafeAreaRegions.Default);

            // Act
            int hashCode1 = edges.GetHashCode();
            int hashCode2 = edges.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode returns the same value for equal objects with identical property values.
        /// </summary>
        [Theory]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All, SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.Container, SafeAreaRegions.None, SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.All, SafeAreaRegions.None)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetHashCode_EqualObjects_ReturnsSameHashCode(SafeAreaRegions left, SafeAreaRegions top, SafeAreaRegions right, SafeAreaRegions bottom)
        {
            // Arrange
            var edges1 = new SafeAreaEdges(left, top, right, bottom);
            var edges2 = new SafeAreaEdges(left, top, right, bottom);

            // Act
            int hashCode1 = edges1.GetHashCode();
            int hashCode2 = edges2.GetHashCode();

            // Assert
            Assert.Equal(hashCode1, hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode produces values for objects with different property combinations.
        /// While hash collisions are possible, this test validates the method executes successfully.
        /// </summary>
        [Theory]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.All, SafeAreaRegions.Container, SafeAreaRegions.Default)]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.Default, SafeAreaRegions.Container)]
        [InlineData(SafeAreaRegions.Container, SafeAreaRegions.Default, SafeAreaRegions.None, SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.Container, SafeAreaRegions.All, SafeAreaRegions.None)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetHashCode_DifferentObjects_ReturnsHashCode(SafeAreaRegions left, SafeAreaRegions top, SafeAreaRegions right, SafeAreaRegions bottom)
        {
            // Arrange
            var edges = new SafeAreaEdges(left, top, right, bottom);

            // Act
            int hashCode = edges.GetHashCode();

            // Assert - Just ensure the method executes without throwing and returns a value
            Assert.IsType<int>(hashCode);
        }

        /// <summary>
        /// Tests that GetHashCode handles all enum boundary values correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetHashCode_AllEnumValues_ExecutesSuccessfully()
        {
            // Arrange & Act & Assert - Test all possible enum combinations at boundaries
            var edges1 = new SafeAreaEdges((SafeAreaRegions)0, (SafeAreaRegions)1, (SafeAreaRegions)2, (SafeAreaRegions)3);
            var edges2 = new SafeAreaEdges((SafeAreaRegions)int.MaxValue, (SafeAreaRegions)int.MinValue, (SafeAreaRegions)(-1), (SafeAreaRegions)0);

            int hashCode1 = edges1.GetHashCode();
            int hashCode2 = edges2.GetHashCode();

            Assert.IsType<int>(hashCode1);
            Assert.IsType<int>(hashCode2);
        }

        /// <summary>
        /// Tests that GetHashCode works correctly with static property instances.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetHashCode_StaticInstances_ReturnsConsistentHashCodes()
        {
            // Arrange & Act
            int noneHashCode = SafeAreaEdges.None.GetHashCode();
            int allHashCode = SafeAreaEdges.All.GetHashCode();
            int defaultHashCode = SafeAreaEdges.Default.GetHashCode();

            // Assert - Ensure each returns a valid hash code
            Assert.IsType<int>(noneHashCode);
            Assert.IsType<int>(allHashCode);
            Assert.IsType<int>(defaultHashCode);

            // Verify consistency
            Assert.Equal(noneHashCode, SafeAreaEdges.None.GetHashCode());
            Assert.Equal(allHashCode, SafeAreaEdges.All.GetHashCode());
            Assert.Equal(defaultHashCode, SafeAreaEdges.Default.GetHashCode());
        }

        /// <summary>
        /// Tests that GetHashCode handles constructor variations correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetHashCode_ConstructorVariations_ReturnsValidHashCodes()
        {
            // Arrange
            var uniform = new SafeAreaEdges(SafeAreaRegions.All);
            var horizontal = new SafeAreaEdges(SafeAreaRegions.Container, SafeAreaRegions.None);
            var individual = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.Container, SafeAreaRegions.Default);

            // Act
            int uniformHashCode = uniform.GetHashCode();
            int horizontalHashCode = horizontal.GetHashCode();
            int individualHashCode = individual.GetHashCode();

            // Assert
            Assert.IsType<int>(uniformHashCode);
            Assert.IsType<int>(horizontalHashCode);
            Assert.IsType<int>(individualHashCode);
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Tests that GetHashCode uses System.HashCode.Combine for non-NETSTANDARD2_0 targets.
        /// This test specifically targets the code path at line 146.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void GetHashCode_NonNetStandard20_UsesSystemHashCodeCombine()
        {
            // Arrange
            var edges = new SafeAreaEdges(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.Container, SafeAreaRegions.Default);

            // Act
            int actualHashCode = edges.GetHashCode();
            int expectedHashCode = HashCode.Combine(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.Container, SafeAreaRegions.Default);

            // Assert
            Assert.Equal(expectedHashCode, actualHashCode);
        }
#endif

        /// <summary>
        /// Tests ToString method with uniform values for all edges.
        /// Verifies that the string representation follows the format "Left, Top, Right, Bottom".
        /// </summary>
        /// <param name="uniformValue">The SafeAreaRegions value to apply to all edges.</param>
        /// <param name="expectedString">The expected string representation.</param>
        [Theory]
        [InlineData(SafeAreaRegions.None, "None, None, None, None")]
        [InlineData(SafeAreaRegions.Default, "Default, Default, Default, Default")]
        [InlineData(SafeAreaRegions.All, "All, All, All, All")]
        [InlineData(SafeAreaRegions.Container, "Container, Container, Container, Container")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void ToString_UniformValues_ReturnsExpectedFormat(SafeAreaRegions uniformValue, string expectedString)
        {
            // Arrange
            var safeAreaEdges = new SafeAreaEdges(uniformValue);

            // Act
            var result = safeAreaEdges.ToString();

            // Assert
            Assert.Equal(expectedString, result);
        }

        /// <summary>
        /// Tests ToString method with different horizontal and vertical values.
        /// Verifies that the string representation correctly reflects horizontal (left/right) and vertical (top/bottom) groupings.
        /// </summary>
        /// <param name="horizontal">The SafeAreaRegions value for left and right edges.</param>
        /// <param name="vertical">The SafeAreaRegions value for top and bottom edges.</param>
        /// <param name="expectedString">The expected string representation.</param>
        [Theory]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.All, "None, All, None, All")]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.None, "All, None, All, None")]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.Container, "Default, Container, Default, Container")]
        [InlineData(SafeAreaRegions.Container, SafeAreaRegions.Default, "Container, Default, Container, Default")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void ToString_HorizontalVerticalValues_ReturnsExpectedFormat(SafeAreaRegions horizontal, SafeAreaRegions vertical, string expectedString)
        {
            // Arrange
            var safeAreaEdges = new SafeAreaEdges(horizontal, vertical);

            // Act
            var result = safeAreaEdges.ToString();

            // Assert
            Assert.Equal(expectedString, result);
        }

        /// <summary>
        /// Tests ToString method with individual values for each edge.
        /// Verifies that the string representation follows the exact order: Left, Top, Right, Bottom.
        /// </summary>
        /// <param name="left">The SafeAreaRegions value for the left edge.</param>
        /// <param name="top">The SafeAreaRegions value for the top edge.</param>
        /// <param name="right">The SafeAreaRegions value for the right edge.</param>
        /// <param name="bottom">The SafeAreaRegions value for the bottom edge.</param>
        /// <param name="expectedString">The expected string representation.</param>
        [Theory]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.Default, SafeAreaRegions.All, SafeAreaRegions.Container, "None, Default, All, Container")]
        [InlineData(SafeAreaRegions.Container, SafeAreaRegions.All, SafeAreaRegions.Default, SafeAreaRegions.None, "Container, All, Default, None")]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.None, SafeAreaRegions.Container, SafeAreaRegions.Default, "All, None, Container, Default")]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.Container, SafeAreaRegions.None, SafeAreaRegions.All, "Default, Container, None, All")]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void ToString_IndividualValues_ReturnsExpectedFormat(SafeAreaRegions left, SafeAreaRegions top, SafeAreaRegions right, SafeAreaRegions bottom, string expectedString)
        {
            // Arrange
            var safeAreaEdges = new SafeAreaEdges(left, top, right, bottom);

            // Act
            var result = safeAreaEdges.ToString();

            // Assert
            Assert.Equal(expectedString, result);
        }

        /// <summary>
        /// Tests ToString method with static property instances.
        /// Verifies that predefined static instances return the correct string representation.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void ToString_StaticProperties_ReturnsExpectedFormat()
        {
            // Arrange & Act & Assert
            Assert.Equal("Default, Default, Default, Default", SafeAreaEdges.Default.ToString());
            Assert.Equal("None, None, None, None", SafeAreaEdges.None.ToString());
            Assert.Equal("All, All, All, All", SafeAreaEdges.All.ToString());
        }

        /// <summary>
        /// Tests ToString method edge case with all different enum values.
        /// Verifies that the method correctly handles a scenario where each edge has a different SafeAreaRegions value.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void ToString_AllDifferentValues_ReturnsCorrectOrder()
        {
            // Arrange
            var safeAreaEdges = new SafeAreaEdges(
                left: SafeAreaRegions.None,
                top: SafeAreaRegions.Default,
                right: SafeAreaRegions.All,
                bottom: SafeAreaRegions.Container);

            // Act
            var result = safeAreaEdges.ToString();

            // Assert
            Assert.Equal("None, Default, All, Container", result);
        }

        /// <summary>
        /// Tests ToString method with boundary enum values.
        /// Verifies that the method correctly handles enum values that might be at the boundaries of the enum definition.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void ToString_BoundaryEnumValues_ReturnsExpectedFormat()
        {
            // Arrange - Test with the first and conceptually last enum values
            var safeAreaEdges1 = new SafeAreaEdges(SafeAreaRegions.None);
            var safeAreaEdges2 = new SafeAreaEdges(SafeAreaRegions.Container);

            // Act
            var result1 = safeAreaEdges1.ToString();
            var result2 = safeAreaEdges2.ToString();

            // Assert
            Assert.Equal("None, None, None, None", result1);
            Assert.Equal("Container, Container, Container, Container", result2);
        }

        /// <summary>
        /// Tests the horizontal and vertical constructor with all defined enum values.
        /// Verifies that horizontal parameter is assigned to Left and Right properties,
        /// and vertical parameter is assigned to Top and Bottom properties.
        /// </summary>
        /// <param name="horizontal">The horizontal safe area regions value</param>
        /// <param name="vertical">The vertical safe area regions value</param>
        [Theory]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.SoftInput, SafeAreaRegions.Container)]
        [InlineData(SafeAreaRegions.Container, SafeAreaRegions.SoftInput)]
        [InlineData(SafeAreaRegions.Default, SafeAreaRegions.All)]
        [InlineData(SafeAreaRegions.All, SafeAreaRegions.Default)]
        [InlineData(SafeAreaRegions.SoftInput, SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.Container, SafeAreaRegions.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_HorizontalVertical_SetsPropertiesCorrectly(SafeAreaRegions horizontal, SafeAreaRegions vertical)
        {
            // Act
            var edges = new SafeAreaEdges(horizontal, vertical);

            // Assert
            Assert.Equal(horizontal, edges.Left);
            Assert.Equal(horizontal, edges.Right);
            Assert.Equal(vertical, edges.Top);
            Assert.Equal(vertical, edges.Bottom);
        }

        /// <summary>
        /// Tests the horizontal and vertical constructor with flag combinations.
        /// Verifies that combined enum flags are correctly assigned to the appropriate properties.
        /// </summary>
        /// <param name="horizontal">The horizontal safe area regions value with combined flags</param>
        /// <param name="vertical">The vertical safe area regions value with combined flags</param>
        [Theory]
        [InlineData(SafeAreaRegions.SoftInput | SafeAreaRegions.Container, SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.None, SafeAreaRegions.SoftInput | SafeAreaRegions.Container)]
        [InlineData(SafeAreaRegions.SoftInput | SafeAreaRegions.Container, SafeAreaRegions.SoftInput | SafeAreaRegions.Container)]
        [InlineData(SafeAreaRegions.All | SafeAreaRegions.SoftInput, SafeAreaRegions.Container | SafeAreaRegions.Default)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_HorizontalVertical_WithFlagCombinations_SetsPropertiesCorrectly(SafeAreaRegions horizontal, SafeAreaRegions vertical)
        {
            // Act
            var edges = new SafeAreaEdges(horizontal, vertical);

            // Assert
            Assert.Equal(horizontal, edges.Left);
            Assert.Equal(horizontal, edges.Right);
            Assert.Equal(vertical, edges.Top);
            Assert.Equal(vertical, edges.Bottom);
        }

        /// <summary>
        /// Tests the horizontal and vertical constructor with extreme enum values.
        /// Verifies that invalid enum values (cast from boundary integers) are handled correctly.
        /// </summary>
        /// <param name="horizontalValue">The horizontal value cast to SafeAreaRegions</param>
        /// <param name="verticalValue">The vertical value cast to SafeAreaRegions</param>
        [Theory]
        [InlineData(int.MinValue, int.MaxValue)]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(0, int.MinValue)]
        [InlineData(int.MaxValue, 0)]
        [InlineData(-2, 999999)]
        [InlineData(32769, -999999)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_HorizontalVertical_WithExtremeValues_SetsPropertiesCorrectly(int horizontalValue, int verticalValue)
        {
            // Arrange
            var horizontal = (SafeAreaRegions)horizontalValue;
            var vertical = (SafeAreaRegions)verticalValue;

            // Act
            var edges = new SafeAreaEdges(horizontal, vertical);

            // Assert
            Assert.Equal(horizontal, edges.Left);
            Assert.Equal(horizontal, edges.Right);
            Assert.Equal(vertical, edges.Top);
            Assert.Equal(vertical, edges.Bottom);
        }

        /// <summary>
        /// Tests the horizontal and vertical constructor with same values for both parameters.
        /// Verifies that when horizontal and vertical are the same, all four properties have the expected values.
        /// </summary>
        /// <param name="value">The same value to use for both horizontal and vertical parameters</param>
        [Theory]
        [InlineData(SafeAreaRegions.None)]
        [InlineData(SafeAreaRegions.SoftInput)]
        [InlineData(SafeAreaRegions.Container)]
        [InlineData(SafeAreaRegions.Default)]
        [InlineData(SafeAreaRegions.All)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_HorizontalVertical_WithSameValues_SetsAllPropertiesToSameValue(SafeAreaRegions value)
        {
            // Act
            var edges = new SafeAreaEdges(value, value);

            // Assert
            Assert.Equal(value, edges.Left);
            Assert.Equal(value, edges.Right);
            Assert.Equal(value, edges.Top);
            Assert.Equal(value, edges.Bottom);
        }

        /// <summary>
        /// Tests the horizontal and vertical constructor with different horizontal and vertical values.
        /// Verifies that Left and Right differ from Top and Bottom when different parameters are used.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_HorizontalVertical_WithDifferentValues_SetsHorizontalAndVerticalPropertiesDifferently()
        {
            // Arrange
            var horizontal = SafeAreaRegions.SoftInput;
            var vertical = SafeAreaRegions.Container;

            // Act
            var edges = new SafeAreaEdges(horizontal, vertical);

            // Assert
            Assert.Equal(horizontal, edges.Left);
            Assert.Equal(horizontal, edges.Right);
            Assert.Equal(vertical, edges.Top);
            Assert.Equal(vertical, edges.Bottom);
            Assert.NotEqual(edges.Left, edges.Top);
            Assert.NotEqual(edges.Right, edges.Bottom);
            Assert.Equal(edges.Left, edges.Right);
            Assert.Equal(edges.Top, edges.Bottom);
        }
    }
}