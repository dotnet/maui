#nullable disable

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;


namespace Microsoft.Maui.Controls.Internals.UnitTests
{
    public class EvalRequestedTests
    {
        /// <summary>
        /// Tests that the EvalRequested constructor properly assigns the script parameter to the Script property
        /// for various string inputs including edge cases.
        /// </summary>
        /// <param name="script">The script value to test with the constructor</param>
        [Theory]
        [InlineData("console.log('Hello World');")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("function test() { return 42; }")]
        [InlineData("var x = 'string with special chars: !@#$%^&*()[]{}|\\:;\"'<>?,./`~'")]
        [InlineData("a very long script that contains multiple statements and functions to test handling of longer strings")]
        public void Constructor_WithVariousScriptValues_AssignsScriptPropertyCorrectly(string script)
        {
            // Arrange & Act
            var evalRequested = new EvalRequested(script);

            // Assert
            Assert.Equal(script, evalRequested.Script);
        }

        /// <summary>
        /// Tests that the EvalRequested constructor properly handles null script parameter
        /// and assigns it to the Script property without throwing an exception.
        /// </summary>
        [Fact]
        public void Constructor_WithNullScript_AssignsNullToScriptProperty()
        {
            // Arrange
            string script = null;

            // Act
            var evalRequested = new EvalRequested(script);

            // Assert
            Assert.Null(evalRequested.Script);
        }

        /// <summary>
        /// Tests that the EvalRequested constructor creates an instance that inherits from EventArgs
        /// to verify proper inheritance chain.
        /// </summary>
        [Fact]
        public void Constructor_WithValidScript_CreatesEventArgsInstance()
        {
            // Arrange
            string script = "test script";

            // Act
            var evalRequested = new EvalRequested(script);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(evalRequested);
        }
    }
}
