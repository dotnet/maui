using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class HybridWebViewTests
    {
        /// <summary>
        /// Tests that the HybridRoot property returns the default value "wwwroot" when no value has been explicitly set.
        /// </summary>
        [Fact]
        public void HybridRoot_DefaultValue_ReturnsWwwroot()
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act
            var result = hybridWebView.HybridRoot;

            // Assert
            Assert.Equal("wwwroot", result);
        }

        /// <summary>
        /// Tests that the HybridRoot property can be set to and retrieved as various string values including null, empty, whitespace, normal strings, long strings, and strings with special characters.
        /// </summary>
        /// <param name="value">The value to set and retrieve from the HybridRoot property.</param>
        /// <param name="expectedValue">The expected value that should be returned when getting the property.</param>
        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData("  \t\n\r  ", "  \t\n\r  ")]
        [InlineData("custom-root", "custom-root")]
        [InlineData("assets/web", "assets/web")]
        [InlineData("C:\\Users\\Test\\WebRoot", "C:\\Users\\Test\\WebRoot")]
        [InlineData("/var/www/html", "/var/www/html")]
        [InlineData("root with spaces", "root with spaces")]
        [InlineData("root-with-special-chars!@#$%^&*()", "root-with-special-chars!@#$%^&*()")]
        [InlineData("unicode-测试-🌐", "unicode-测试-🌐")]
        public void HybridRoot_SetAndGet_ReturnsExpectedValue(string value, string expectedValue)
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act
            hybridWebView.HybridRoot = value;
            var result = hybridWebView.HybridRoot;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that the HybridRoot property can handle very long string values without issues.
        /// </summary>
        [Fact]
        public void HybridRoot_VeryLongString_SetAndRetrievedSuccessfully()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var longValue = new string('a', 10000);

            // Act
            hybridWebView.HybridRoot = longValue;
            var result = hybridWebView.HybridRoot;

            // Assert
            Assert.Equal(longValue, result);
        }

        /// <summary>
        /// Tests that multiple get operations on HybridRoot return the same value consistently.
        /// </summary>
        [Fact]
        public void HybridRoot_MultipleGets_ReturnsSameValue()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var setValue = "test-value";
            hybridWebView.HybridRoot = setValue;

            // Act
            var result1 = hybridWebView.HybridRoot;
            var result2 = hybridWebView.HybridRoot;
            var result3 = hybridWebView.HybridRoot;

            // Assert
            Assert.Equal(setValue, result1);
            Assert.Equal(setValue, result2);
            Assert.Equal(setValue, result3);
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        /// <summary>
        /// Tests that setting HybridRoot multiple times with different values updates the property correctly.
        /// </summary>
        [Fact]
        public void HybridRoot_MultipleSets_UpdatesValueCorrectly()
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act & Assert
            hybridWebView.HybridRoot = "first-value";
            Assert.Equal("first-value", hybridWebView.HybridRoot);

            hybridWebView.HybridRoot = "second-value";
            Assert.Equal("second-value", hybridWebView.HybridRoot);

            hybridWebView.HybridRoot = null;
            Assert.Null(hybridWebView.HybridRoot);

            hybridWebView.HybridRoot = "";
            Assert.Equal("", hybridWebView.HybridRoot);
        }

        /// <summary>
        /// Tests that SetInvokeJavaScriptTarget correctly sets both InvokeJavaScriptTarget and InvokeJavaScriptType properties
        /// when provided with a valid class object.
        /// </summary>
        [Fact]
        public void SetInvokeJavaScriptTarget_WithValidClassObject_SetsPropertiesCorrectly()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var target = new TestClass { Name = "Test" };

            // Act
            hybridWebView.SetInvokeJavaScriptTarget(target);

            // Assert
            var hybridInterface = (IHybridWebView)hybridWebView;
            Assert.Same(target, hybridInterface.InvokeJavaScriptTarget);
            Assert.Equal(typeof(TestClass), hybridInterface.InvokeJavaScriptType);
        }

        /// <summary>
        /// Tests that SetInvokeJavaScriptTarget correctly handles null target objects,
        /// setting InvokeJavaScriptTarget to null while still setting the correct type.
        /// </summary>
        [Fact]
        public void SetInvokeJavaScriptTarget_WithNullTarget_SetsTargetToNullAndCorrectType()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            TestClass? target = null;

            // Act
            hybridWebView.SetInvokeJavaScriptTarget(target);

            // Assert
            var hybridInterface = (IHybridWebView)hybridWebView;
            Assert.Null(hybridInterface.InvokeJavaScriptTarget);
            Assert.Equal(typeof(TestClass), hybridInterface.InvokeJavaScriptType);
        }

        /// <summary>
        /// Tests that SetInvokeJavaScriptTarget works correctly with different class types,
        /// ensuring the type information is preserved accurately for each specific type.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetTestObjectsData))]
        public void SetInvokeJavaScriptTarget_WithDifferentTypes_SetsCorrectTypeAndTarget(object target, Type expectedType)
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act
            hybridWebView.SetInvokeJavaScriptTarget(target);

            // Assert
            var hybridInterface = (IHybridWebView)hybridWebView;
            Assert.Same(target, hybridInterface.InvokeJavaScriptTarget);
            Assert.Equal(expectedType, hybridInterface.InvokeJavaScriptType);
        }

        /// <summary>
        /// Tests that SetInvokeJavaScriptTarget correctly handles interface implementations,
        /// preserving the concrete type information rather than the interface type.
        /// </summary>
        [Fact]
        public void SetInvokeJavaScriptTarget_WithInterfaceImplementation_SetsConcreteType()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var target = new TestInterfaceImplementation { Value = 42 };

            // Act
            hybridWebView.SetInvokeJavaScriptTarget(target);

            // Assert
            var hybridInterface = (IHybridWebView)hybridWebView;
            Assert.Same(target, hybridInterface.InvokeJavaScriptTarget);
            Assert.Equal(typeof(TestInterfaceImplementation), hybridInterface.InvokeJavaScriptType);
        }

        /// <summary>
        /// Tests that SetInvokeJavaScriptTarget correctly handles derived class objects,
        /// preserving the most specific type information.
        /// </summary>
        [Fact]
        public void SetInvokeJavaScriptTarget_WithDerivedClass_SetsDerivedType()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var target = new DerivedTestClass { Name = "Derived", DerivedProperty = "Extra" };

            // Act
            hybridWebView.SetInvokeJavaScriptTarget(target);

            // Assert
            var hybridInterface = (IHybridWebView)hybridWebView;
            Assert.Same(target, hybridInterface.InvokeJavaScriptTarget);
            Assert.Equal(typeof(DerivedTestClass), hybridInterface.InvokeJavaScriptType);
        }

        /// <summary>
        /// Tests that multiple calls to SetInvokeJavaScriptTarget correctly overwrite
        /// the previous target and type information.
        /// </summary>
        [Fact]
        public void SetInvokeJavaScriptTarget_WithMultipleCalls_OverwritesPreviousValues()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var firstTarget = new TestClass { Name = "First" };
            var secondTarget = new AnotherTestClass { Id = 123 };

            // Act
            hybridWebView.SetInvokeJavaScriptTarget(firstTarget);
            hybridWebView.SetInvokeJavaScriptTarget(secondTarget);

            // Assert
            var hybridInterface = (IHybridWebView)hybridWebView;
            Assert.Same(secondTarget, hybridInterface.InvokeJavaScriptTarget);
            Assert.Equal(typeof(AnotherTestClass), hybridInterface.InvokeJavaScriptType);
            Assert.NotSame(firstTarget, hybridInterface.InvokeJavaScriptTarget);
        }

        public static TheoryData<object, Type> GetTestObjectsData()
        {
            return new TheoryData<object, Type>
            {
                { new TestClass { Name = "Test" }, typeof(TestClass) },
                { new AnotherTestClass { Id = 123 }, typeof(AnotherTestClass) },
                { new DerivedTestClass { Name = "Derived", DerivedProperty = "Extra" }, typeof(DerivedTestClass) },
                { "StringObject", typeof(string) },
                { new object(), typeof(object) }
            };
        }

        private class TestClass
        {
            public string Name { get; set; } = string.Empty;
        }

        private class AnotherTestClass
        {
            public int Id { get; set; }
        }

        private class DerivedTestClass : TestClass
        {
            public string DerivedProperty { get; set; } = string.Empty;
        }

        private interface ITestInterface
        {
            int Value { get; set; }
        }

        private class TestInterfaceImplementation : ITestInterface
        {
            public int Value { get; set; }
        }

        /// <summary>
        /// Tests that SendRawMessage does not throw when Handler is null.
        /// This verifies the null-conditional operator prevents exceptions when no handler is set.
        /// Should not attempt to invoke any methods on the null handler.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("test message")]
        [InlineData("very long message with special characters: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
        public void SendRawMessage_HandlerIsNull_DoesNotThrow(string rawMessage)
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            // Handler is null by default

            // Act & Assert
            var exception = Record.Exception(() => hybridWebView.SendRawMessage(rawMessage));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SendRawMessage creates a new HybridWebViewRawMessage instance for each call.
        /// This verifies that a fresh message object is created and the Message property is set correctly.
        /// Multiple calls should create separate instances.
        /// </summary>
        [Fact]
        public void SendRawMessage_HandlerIsNotNull_CreatesNewMessageInstanceForEachCall()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            hybridWebView.Handler = mockHandler;
            var capturedMessages = new List<HybridWebViewRawMessage>();

            mockHandler.When(h => h.Invoke(Arg.Any<string>(), Arg.Any<HybridWebViewRawMessage>()))
                      .Do(callInfo => capturedMessages.Add((HybridWebViewRawMessage)callInfo[1]));

            // Act
            hybridWebView.SendRawMessage("first message");
            hybridWebView.SendRawMessage("second message");

            // Assert
            Assert.Equal(2, capturedMessages.Count);
            Assert.NotSame(capturedMessages[0], capturedMessages[1]); // Different instances
            Assert.Equal("first message", capturedMessages[0].Message);
            Assert.Equal("second message", capturedMessages[1].Message);
        }

        /// <summary>
        /// Tests that SendRawMessage uses the correct method name from nameof expression.
        /// This verifies that the first parameter to Invoke is exactly "SendRawMessage".
        /// The method name should be consistent and derived from the interface method name.
        /// </summary>
        [Fact]
        public void SendRawMessage_HandlerIsNotNull_UsesCorrectMethodName()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            hybridWebView.Handler = mockHandler;

            // Act
            hybridWebView.SendRawMessage("test");

            // Assert
            mockHandler.Received(1).Invoke("SendRawMessage", Arg.Any<HybridWebViewRawMessage>());
        }

        /// <summary>
        /// Tests SendRawMessage with extreme string values to verify robustness.
        /// This covers boundary conditions and potential edge cases in string handling.
        /// Should handle all string values without throwing exceptions when handler is present.
        /// </summary>
        [Theory]
        [InlineData("\0")] // Null character
        [InlineData("\u0001\u0002\u0003")] // Control characters
        [InlineData("\"'`\\/")] // Quote and escape characters
        [InlineData("<script>alert('xss')</script>")] // Potential XSS content
        public void SendRawMessage_HandlerIsNotNull_HandlesSpecialStringValues(string rawMessage)
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            hybridWebView.Handler = mockHandler;

            // Act
            var exception = Record.Exception(() => hybridWebView.SendRawMessage(rawMessage));

            // Assert
            Assert.Null(exception);
            mockHandler.Received(1).Invoke(
                "SendRawMessage",
                Arg.Is<HybridWebViewRawMessage>(msg => msg.Message == rawMessage)
            );
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync throws ArgumentException when methodName is null.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_NullMethodName_ThrowsArgumentException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                hybridWebView.InvokeJavaScriptAsync(null));

            Assert.Equal("methodName", exception.ParamName);
            Assert.Contains("The method name cannot be null or empty.", exception.Message);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync throws ArgumentException when methodName is empty string.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_EmptyMethodName_ThrowsArgumentException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                hybridWebView.InvokeJavaScriptAsync(""));

            Assert.Equal("methodName", exception.ParamName);
            Assert.Contains("The method name cannot be null or empty.", exception.Message);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync throws ArgumentException when paramValues is provided but paramJsonTypeInfos is null.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_ParamValuesProvidedButJsonTypeInfosNull_ThrowsArgumentException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var paramValues = new object[] { "test" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                hybridWebView.InvokeJavaScriptAsync("testMethod", paramValues, null));

            Assert.Equal("paramJsonTypeInfos", exception.ParamName);
            Assert.Contains("The parameter values were provided, but the parameter JSON type infos were not.", exception.Message);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync throws ArgumentException when paramValues is null but paramJsonTypeInfos is provided.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_ParamValuesNullButJsonTypeInfosProvided_ThrowsArgumentException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var paramJsonTypeInfos = new JsonTypeInfo[] { JsonTypeInfo.Create<string>(new JsonSerializerOptions()) };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                hybridWebView.InvokeJavaScriptAsync("testMethod", null, paramJsonTypeInfos));

            Assert.Equal("paramValues", exception.ParamName);
            Assert.Contains("The parameter JSON type infos were provided, but the parameter values were not.", exception.Message);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync throws ArgumentException when paramValues and paramJsonTypeInfos have different lengths.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_ParamValuesDifferentLengthThanJsonTypeInfos_ThrowsArgumentException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var paramValues = new object[] { "test1", "test2" };
            var paramJsonTypeInfos = new JsonTypeInfo[] { JsonTypeInfo.Create<string>(new JsonSerializerOptions()) };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                hybridWebView.InvokeJavaScriptAsync("testMethod", paramValues, paramJsonTypeInfos));

            Assert.Equal("paramValues", exception.ParamName);
            Assert.Contains("The number of parameter values does not match the number of parameter JSON type infos.", exception.Message);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync executes successfully with valid methodName and no parameters when Handler is set.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_ValidMethodNameNoParameters_ExecutesSuccessfully()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(null));
            hybridWebView.Handler = mockHandler;

            // Act
            await hybridWebView.InvokeJavaScriptAsync("testMethod");

            // Assert
            await mockHandler.Received(1).InvokeAsync<object>(
                nameof(IHybridWebView.InvokeJavaScriptAsync),
                Arg.Is<HybridWebViewInvokeJavaScriptRequest>(req =>
                    req.MethodName == "testMethod" &&
                    req.ReturnTypeJsonTypeInfo == null &&
                    req.ParamValues == null &&
                    req.ParamJsonTypeInfos == null));
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync executes successfully with valid parameters when Handler is set.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_ValidMethodNameWithParameters_ExecutesSuccessfully()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(null));
            hybridWebView.Handler = mockHandler;

            var paramValues = new object[] { "test", 42 };
            var paramJsonTypeInfos = new JsonTypeInfo[] {
                JsonTypeInfo.Create<string>(new JsonSerializerOptions()),
                JsonTypeInfo.Create<int>(new JsonSerializerOptions())
            };

            // Act
            await hybridWebView.InvokeJavaScriptAsync("testMethod", paramValues, paramJsonTypeInfos);

            // Assert
            await mockHandler.Received(1).InvokeAsync<object>(
                nameof(IHybridWebView.InvokeJavaScriptAsync),
                Arg.Is<HybridWebViewInvokeJavaScriptRequest>(req =>
                    req.MethodName == "testMethod" &&
                    req.ReturnTypeJsonTypeInfo == null &&
                    req.ParamValues == paramValues &&
                    req.ParamJsonTypeInfos == paramJsonTypeInfos));
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync executes successfully when Handler is null.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_HandlerIsNull_ExecutesWithoutException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            hybridWebView.Handler = null;

            // Act & Assert - Should not throw
            await hybridWebView.InvokeJavaScriptAsync("testMethod");
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync accepts whitespace-only methodName as valid.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_WhitespaceMethodName_ExecutesSuccessfully()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(null));
            hybridWebView.Handler = mockHandler;

            // Act
            await hybridWebView.InvokeJavaScriptAsync("   ");

            // Assert
            await mockHandler.Received(1).InvokeAsync<object>(
                nameof(IHybridWebView.InvokeJavaScriptAsync),
                Arg.Is<HybridWebViewInvokeJavaScriptRequest>(req => req.MethodName == "   "));
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync executes successfully with empty arrays for parameters.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_EmptyArraysForParameters_ExecutesSuccessfully()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(null));
            hybridWebView.Handler = mockHandler;

            var paramValues = new object[0];
            var paramJsonTypeInfos = new JsonTypeInfo[0];

            // Act
            await hybridWebView.InvokeJavaScriptAsync("testMethod", paramValues, paramJsonTypeInfos);

            // Assert
            await mockHandler.Received(1).InvokeAsync<object>(
                nameof(IHybridWebView.InvokeJavaScriptAsync),
                Arg.Is<HybridWebViewInvokeJavaScriptRequest>(req =>
                    req.MethodName == "testMethod" &&
                    req.ParamValues == paramValues &&
                    req.ParamJsonTypeInfos == paramJsonTypeInfos));
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync throws ArgumentException when methodName is whitespace only.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_WhitespaceMethodName_ThrowsArgumentException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var returnTypeInfo = JsonTypeInfo.Create<string>(new JsonSerializerOptions());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                hybridWebView.InvokeJavaScriptAsync<string>("   ", returnTypeInfo));

            Assert.Equal("methodName", exception.ParamName);
            Assert.Contains("method name cannot be null or empty", exception.Message);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync throws ArgumentException when paramValues is provided but paramJsonTypeInfos is null.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_ParamValuesProvidedButTypeInfosNull_ThrowsArgumentException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var returnTypeInfo = JsonTypeInfo.Create<string>(new JsonSerializerOptions());
            var paramValues = new object[] { "test" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                hybridWebView.InvokeJavaScriptAsync<string>("testMethod", returnTypeInfo, paramValues, null));

            Assert.Equal("paramJsonTypeInfos", exception.ParamName);
            Assert.Contains("parameter values were provided, but the parameter JSON type infos were not", exception.Message);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync throws ArgumentException when paramJsonTypeInfos is provided but paramValues is null.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_TypeInfosProvidedButParamValuesNull_ThrowsArgumentException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var returnTypeInfo = JsonTypeInfo.Create<string>(new JsonSerializerOptions());
            var paramTypeInfos = new JsonTypeInfo[] { JsonTypeInfo.Create<string>(new JsonSerializerOptions()) };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                hybridWebView.InvokeJavaScriptAsync<string>("testMethod", returnTypeInfo, null, paramTypeInfos));

            Assert.Equal("paramValues", exception.ParamName);
            Assert.Contains("parameter JSON type infos were provided, but the parameter values were not", exception.Message);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync throws ArgumentException when paramValues and paramJsonTypeInfos have different lengths.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_MismatchedParameterArrayLengths_ThrowsArgumentException()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var returnTypeInfo = JsonTypeInfo.Create<string>(new JsonSerializerOptions());
            var paramValues = new object[] { "test1", "test2" };
            var paramTypeInfos = new JsonTypeInfo[] { JsonTypeInfo.Create<string>(new JsonSerializerOptions()) };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                hybridWebView.InvokeJavaScriptAsync<string>("testMethod", returnTypeInfo, paramValues, paramTypeInfos));

            Assert.Equal("paramValues", exception.ParamName);
            Assert.Contains("number of parameter values does not match the number of parameter JSON type infos", exception.Message);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync returns default value when handler returns null.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_HandlerReturnsNull_ReturnsDefault()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            hybridWebView.Handler = mockHandler;

            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(null!));

            var returnTypeInfo = JsonTypeInfo.Create<string>(new JsonSerializerOptions());

            // Act
            var result = await hybridWebView.InvokeJavaScriptAsync<string>("testMethod", returnTypeInfo);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync successfully returns cast result when handler returns valid object.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_HandlerReturnsValidObject_ReturnsCastResult()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            hybridWebView.Handler = mockHandler;

            var expectedResult = "test result";
            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(expectedResult));

            var returnTypeInfo = JsonTypeInfo.Create<string>(new JsonSerializerOptions());

            // Act
            var result = await hybridWebView.InvokeJavaScriptAsync<string>("testMethod", returnTypeInfo);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync works correctly with integer return type.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_IntegerReturnType_ReturnsCorrectValue()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            hybridWebView.Handler = mockHandler;

            var expectedResult = 42;
            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(expectedResult));

            var returnTypeInfo = JsonTypeInfo.Create<int>(new JsonSerializerOptions());

            // Act
            var result = await hybridWebView.InvokeJavaScriptAsync<int>("testMethod", returnTypeInfo);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync works correctly with no parameters.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_NoParameters_CallsHandlerCorrectly()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            hybridWebView.Handler = mockHandler;

            var expectedResult = "success";
            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(expectedResult));

            var returnTypeInfo = JsonTypeInfo.Create<string>(new JsonSerializerOptions());

            // Act
            var result = await hybridWebView.InvokeJavaScriptAsync<string>("testMethod", returnTypeInfo);

            // Assert
            Assert.Equal(expectedResult, result);
            await mockHandler.Received(1).InvokeAsync<object>("InvokeJavaScriptAsync", Arg.Any<HybridWebViewInvokeJavaScriptRequest>());
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync works correctly with matching parameter arrays.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_MatchingParameterArrays_CallsHandlerCorrectly()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            hybridWebView.Handler = mockHandler;

            var expectedResult = "success";
            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(expectedResult));

            var returnTypeInfo = JsonTypeInfo.Create<string>(new JsonSerializerOptions());
            var paramValues = new object[] { "param1", 123 };
            var paramTypeInfos = new JsonTypeInfo[]
            {
                JsonTypeInfo.Create<string>(new JsonSerializerOptions()),
                JsonTypeInfo.Create<int>(new JsonSerializerOptions())
            };

            // Act
            var result = await hybridWebView.InvokeJavaScriptAsync<string>("testMethod", returnTypeInfo, paramValues, paramTypeInfos);

            // Assert
            Assert.Equal(expectedResult, result);
            await mockHandler.Received(1).InvokeAsync<object>("InvokeJavaScriptAsync", Arg.Any<HybridWebViewInvokeJavaScriptRequest>());
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync returns default when Handler is null.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_NullHandler_ReturnsDefault()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            hybridWebView.Handler = null;

            var returnTypeInfo = JsonTypeInfo.Create<string>(new JsonSerializerOptions());

            // Act
            var result = await hybridWebView.InvokeJavaScriptAsync<string>("testMethod", returnTypeInfo);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that InvokeJavaScriptAsync returns default for value types when handler returns null.
        /// </summary>
        [Fact]
        public async Task InvokeJavaScriptAsync_HandlerReturnsNullForValueType_ReturnsDefault()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var mockHandler = Substitute.For<IViewHandler>();
            hybridWebView.Handler = mockHandler;

            mockHandler.InvokeAsync<object>(Arg.Any<string>(), Arg.Any<HybridWebViewInvokeJavaScriptRequest>())
                .Returns(Task.FromResult<object>(null!));

            var returnTypeInfo = JsonTypeInfo.Create<int>(new JsonSerializerOptions());

            // Act
            var result = await hybridWebView.InvokeJavaScriptAsync<int>("testMethod", returnTypeInfo);

            // Assert
            Assert.Equal(0, result); // default(int) is 0
        }

        /// <summary>
        /// Tests that the DefaultFile property returns the default value when not explicitly set.
        /// This test verifies that the property getter correctly retrieves the default value "index.html"
        /// from the DefaultFileProperty bindable property.
        /// </summary>
        [Fact]
        public void DefaultFile_WhenNotSet_ReturnsDefaultValue()
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act
            var result = hybridWebView.DefaultFile;

            // Assert
            Assert.Equal("index.html", result);
        }

        /// <summary>
        /// Tests setting and getting various string values for the DefaultFile property.
        /// This test verifies that the property setter correctly stores values using SetValue
        /// and the getter correctly retrieves them using GetValue.
        /// </summary>
        /// <param name="value">The value to set and retrieve.</param>
        /// <param name="expectedResult">The expected result when getting the property.</param>
        [Theory]
        [InlineData("test.html", "test.html")]
        [InlineData("", "")]
        [InlineData(" ", " ")]
        [InlineData("   ", "   ")]
        [InlineData("\t", "\t")]
        [InlineData("\n", "\n")]
        [InlineData("\r\n", "\r\n")]
        [InlineData("file with spaces.html", "file with spaces.html")]
        [InlineData("file-with-dashes.html", "file-with-dashes.html")]
        [InlineData("file_with_underscores.html", "file_with_underscores.html")]
        [InlineData("файл.html", "файл.html")]
        [InlineData("特殊字符.html", "特殊字符.html")]
        [InlineData("file\\with\\backslashes.html", "file\\with\\backslashes.html")]
        [InlineData("file/with/forward/slashes.html", "file/with/forward/slashes.html")]
        [InlineData("file\"with\"quotes.html", "file\"with\"quotes.html")]
        [InlineData("file'with'apostrophes.html", "file'with'apostrophes.html")]
        public void DefaultFile_SetAndGet_ReturnsExpectedValue(string value, string expectedResult)
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act
            hybridWebView.DefaultFile = value;
            var result = hybridWebView.DefaultFile;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests setting the DefaultFile property to null.
        /// This test verifies that the property setter correctly handles null values
        /// and the getter returns null when the property is set to null.
        /// </summary>
        [Fact]
        public void DefaultFile_SetToNull_ReturnsNull()
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act
            hybridWebView.DefaultFile = null;
            var result = hybridWebView.DefaultFile;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests setting the DefaultFile property to a very long string.
        /// This test verifies that the property can handle extremely long file names
        /// without issues and that the full value is preserved.
        /// </summary>
        [Fact]
        public void DefaultFile_SetToVeryLongString_ReturnsFullValue()
        {
            // Arrange
            var hybridWebView = new HybridWebView();
            var longString = new string('a', 10000) + ".html";

            // Act
            hybridWebView.DefaultFile = longString;
            var result = hybridWebView.DefaultFile;

            // Assert
            Assert.Equal(longString, result);
            Assert.Equal(10005, result.Length);
        }

        /// <summary>
        /// Tests that setting the DefaultFile property multiple times correctly updates the value.
        /// This test verifies that subsequent calls to the setter properly update the stored value
        /// and that the getter always returns the most recently set value.
        /// </summary>
        [Fact]
        public void DefaultFile_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act & Assert - Set first value
            hybridWebView.DefaultFile = "first.html";
            Assert.Equal("first.html", hybridWebView.DefaultFile);

            // Act & Assert - Set second value
            hybridWebView.DefaultFile = "second.html";
            Assert.Equal("second.html", hybridWebView.DefaultFile);

            // Act & Assert - Set null
            hybridWebView.DefaultFile = null;
            Assert.Null(hybridWebView.DefaultFile);

            // Act & Assert - Set third value
            hybridWebView.DefaultFile = "third.html";
            Assert.Equal("third.html", hybridWebView.DefaultFile);
        }

        /// <summary>
        /// Tests setting the DefaultFile property to strings containing control characters.
        /// This test verifies that the property can handle strings with various control characters
        /// and that these characters are preserved in the stored value.
        /// </summary>
        [Theory]
        [InlineData("\0")]
        [InlineData("\a")]
        [InlineData("\b")]
        [InlineData("\f")]
        [InlineData("\v")]
        [InlineData("\x00\x01\x02")]
        [InlineData("\x7F\x80\x81")]
        public void DefaultFile_SetToStringWithControlCharacters_PreservesValue(string value)
        {
            // Arrange
            var hybridWebView = new HybridWebView();

            // Act
            hybridWebView.DefaultFile = value;
            var result = hybridWebView.DefaultFile;

            // Assert
            Assert.Equal(value, result);
        }
    }
}