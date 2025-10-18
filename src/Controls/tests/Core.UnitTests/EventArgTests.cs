#nullable disable

using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class EventArgTests
    {
        /// <summary>
        /// Tests that the EventArg constructor with string data correctly assigns the value to the Data property.
        /// </summary>
        /// <param name="testData">The string data to test with</param>
        [Theory]
        [InlineData("test string")]
        [InlineData("")]
        [InlineData("very long string with special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
        [InlineData("   whitespace   ")]
        [InlineData(null)]
        public void EventArg_WithStringData_AssignsDataPropertyCorrectly(string testData)
        {
            // Arrange & Act
            var eventArg = new EventArg<string>(testData);

            // Assert
            Assert.Equal(testData, eventArg.Data);
        }

        /// <summary>
        /// Tests that the EventArg constructor with integer data correctly assigns the value to the Data property.
        /// </summary>
        /// <param name="testData">The integer data to test with</param>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(42)]
        public void EventArg_WithIntegerData_AssignsDataPropertyCorrectly(int testData)
        {
            // Arrange & Act
            var eventArg = new EventArg<int>(testData);

            // Assert
            Assert.Equal(testData, eventArg.Data);
        }

        /// <summary>
        /// Tests that the EventArg constructor with double data correctly assigns the value to the Data property,
        /// including special floating-point values.
        /// </summary>
        /// <param name="testData">The double data to test with</param>
        [Theory]
        [InlineData(0.0)]
        [InlineData(1.5)]
        [InlineData(-1.5)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(double.NaN)]
        public void EventArg_WithDoubleData_AssignsDataPropertyCorrectly(double testData)
        {
            // Arrange & Act
            var eventArg = new EventArg<double>(testData);

            // Assert
            if (double.IsNaN(testData))
            {
                Assert.True(double.IsNaN(eventArg.Data));
            }
            else
            {
                Assert.Equal(testData, eventArg.Data);
            }
        }

        /// <summary>
        /// Tests that the EventArg constructor with boolean data correctly assigns the value to the Data property.
        /// </summary>
        /// <param name="testData">The boolean data to test with</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EventArg_WithBooleanData_AssignsDataPropertyCorrectly(bool testData)
        {
            // Arrange & Act
            var eventArg = new EventArg<bool>(testData);

            // Assert
            Assert.Equal(testData, eventArg.Data);
        }

        /// <summary>
        /// Tests that the EventArg constructor with custom object data correctly assigns the reference to the Data property.
        /// </summary>
        [Fact]
        public void EventArg_WithCustomObjectData_AssignsDataPropertyCorrectly()
        {
            // Arrange
            var testObject = new TestObject { Value = "test" };

            // Act
            var eventArg = new EventArg<TestObject>(testObject);

            // Assert
            Assert.Same(testObject, eventArg.Data);
        }

        /// <summary>
        /// Tests that the EventArg constructor with null custom object data correctly assigns null to the Data property.
        /// </summary>
        [Fact]
        public void EventArg_WithNullCustomObjectData_AssignsNullToDataProperty()
        {
            // Arrange
            TestObject testObject = null;

            // Act
            var eventArg = new EventArg<TestObject>(testObject);

            // Assert
            Assert.Null(eventArg.Data);
        }

        /// <summary>
        /// Tests that the EventArg constructor with DateTime data correctly assigns the value to the Data property.
        /// </summary>
        [Fact]
        public void EventArg_WithDateTimeData_AssignsDataPropertyCorrectly()
        {
            // Arrange
            var testDateTime = new DateTime(2023, 12, 25, 10, 30, 45);

            // Act
            var eventArg = new EventArg<DateTime>(testDateTime);

            // Assert
            Assert.Equal(testDateTime, eventArg.Data);
        }

        /// <summary>
        /// Tests that the EventArg constructor with Guid data correctly assigns the value to the Data property.
        /// </summary>
        [Fact]
        public void EventArg_WithGuidData_AssignsDataPropertyCorrectly()
        {
            // Arrange
            var testGuid = Guid.NewGuid();

            // Act
            var eventArg = new EventArg<Guid>(testGuid);

            // Assert
            Assert.Equal(testGuid, eventArg.Data);
        }

        private class TestObject
        {
            public string Value { get; set; }
        }
    }
}
