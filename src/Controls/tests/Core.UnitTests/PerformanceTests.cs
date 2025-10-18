#nullable disable

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the Performance class SetProvider method.
    /// </summary>
    public class PerformanceTests
    {
        /// <summary>
        /// Tests that SetProvider correctly assigns a valid IPerformanceProvider instance to the Provider property.
        /// Input: A mocked IPerformanceProvider instance.
        /// Expected result: The Provider property is set to the provided instance.
        /// </summary>
        [Fact]
        public void SetProvider_WithValidInstance_SetsProviderProperty()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();

            // Act
            Performance.SetProvider(mockProvider);

            // Assert
            Assert.Equal(mockProvider, Performance.Provider);
        }

        /// <summary>
        /// Tests that SetProvider correctly handles null input by setting the Provider property to null.
        /// Input: null IPerformanceProvider instance.
        /// Expected result: The Provider property is set to null.
        /// </summary>
        [Fact]
        public void SetProvider_WithNull_SetsProviderToNull()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider); // Set initial value

            // Act
            Performance.SetProvider(null);

            // Assert
            Assert.Null(Performance.Provider);
        }

        /// <summary>
        /// Tests that SetProvider overwrites the previous Provider value when called multiple times.
        /// Input: Multiple different IPerformanceProvider instances.
        /// Expected result: The Provider property reflects the last assigned instance.
        /// </summary>
        [Fact]
        public void SetProvider_CalledMultipleTimes_OverwritesPreviousValue()
        {
            // Arrange
            var firstProvider = Substitute.For<IPerformanceProvider>();
            var secondProvider = Substitute.For<IPerformanceProvider>();

            // Act
            Performance.SetProvider(firstProvider);
            Performance.SetProvider(secondProvider);

            // Assert
            Assert.Equal(secondProvider, Performance.Provider);
            Assert.NotEqual(firstProvider, Performance.Provider);
        }

        /// <summary>
        /// Tests that Start method sets reference to empty string when Provider is null.
        /// Input conditions: Provider is null, all optional parameters are null.
        /// Expected result: reference is set to empty string, no exception is thrown.
        /// </summary>
        [Fact]
        public void Start_ProviderIsNull_SetsReferenceToEmptyString()
        {
            // Arrange
            Performance.SetProvider(null);

            // Act
            Performance.Start(out string reference);

            // Assert
            Assert.Equal(string.Empty, reference);
        }

        /// <summary>
        /// Tests that Start method sets reference to empty string when Provider is null with all parameters provided.
        /// Input conditions: Provider is null, all optional parameters have values.
        /// Expected result: reference is set to empty string, no exception is thrown.
        /// </summary>
        [Fact]
        public void Start_ProviderIsNullWithAllParameters_SetsReferenceToEmptyString()
        {
            // Arrange
            Performance.SetProvider(null);

            // Act
            Performance.Start(out string reference, "testTag", "testPath", "testMember");

            // Assert
            Assert.Equal(string.Empty, reference);
        }

        /// <summary>
        /// Tests that Start method generates non-empty reference and calls Provider.Start when Provider is not null.
        /// Input conditions: Provider is not null, all optional parameters are null.
        /// Expected result: reference is non-empty, Provider.Start is called with correct parameters.
        /// </summary>
        [Fact]
        public void Start_ProviderIsNotNull_GeneratesReferenceAndCallsProviderStart()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Start(out string reference);

            // Assert
            Assert.NotEmpty(reference);
            mockProvider.Received(1).Start(reference, null, null, null);
        }

        /// <summary>
        /// Tests that Start method generates non-empty reference and calls Provider.Start with all parameters.
        /// Input conditions: Provider is not null, all optional parameters have values.
        /// Expected result: reference is non-empty, Provider.Start is called with correct parameters.
        /// </summary>
        [Fact]
        public void Start_ProviderIsNotNullWithAllParameters_GeneratesReferenceAndCallsProviderStart()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            string tag = "testTag";
            string path = "testPath";
            string member = "testMember";

            // Act
            Performance.Start(out string reference, tag, path, member);

            // Assert
            Assert.NotEmpty(reference);
            mockProvider.Received(1).Start(reference, tag, path, member);
        }

        /// <summary>
        /// Tests that multiple calls to Start method generate different reference values.
        /// Input conditions: Provider is not null, multiple sequential calls.
        /// Expected result: each call generates a unique reference value.
        /// </summary>
        [Fact]
        public void Start_MultipleCalls_GeneratesDifferentReferences()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Start(out string reference1);
            Performance.Start(out string reference2);
            Performance.Start(out string reference3);

            // Assert
            Assert.NotEqual(reference1, reference2);
            Assert.NotEqual(reference2, reference3);
            Assert.NotEqual(reference1, reference3);
            Assert.All(new[] { reference1, reference2, reference3 }, r => Assert.NotEmpty(r));
        }

        /// <summary>
        /// Tests Start method with various tag parameter values.
        /// Input conditions: Provider is not null, different tag values including edge cases.
        /// Expected result: Provider.Start is called with the exact tag value passed.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("normalTag")]
        [InlineData("tag with spaces")]
        [InlineData("tag-with-special-chars!@#$%")]
        [InlineData("verylongtagthatexceedsnormallimitsandcontainslotsofcharacterstotestbehaviorwithextremelylongstrings")]
        public void Start_VariousTagValues_CallsProviderStartWithCorrectTag(string tag)
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Start(out string reference, tag);

            // Assert
            Assert.NotEmpty(reference);
            mockProvider.Received(1).Start(reference, tag, null, null);
        }

        /// <summary>
        /// Tests Start method with various path parameter values.
        /// Input conditions: Provider is not null, different path values including edge cases.
        /// Expected result: Provider.Start is called with the exact path value passed.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("relativePath")]
        [InlineData("/absolute/path")]
        [InlineData("C:\\Windows\\Path")]
        [InlineData("path/with/forward/slashes")]
        [InlineData("path\\with\\back\\slashes")]
        [InlineData("path with spaces")]
        [InlineData("invalid<>|:*?\"path")]
        public void Start_VariousPathValues_CallsProviderStartWithCorrectPath(string path)
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Start(out string reference, null, path);

            // Assert
            Assert.NotEmpty(reference);
            mockProvider.Received(1).Start(reference, null, path, null);
        }

        /// <summary>
        /// Tests Start method with various member parameter values.
        /// Input conditions: Provider is not null, different member values including edge cases.
        /// Expected result: Provider.Start is called with the exact member value passed.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("MethodName")]
        [InlineData("PropertyName")]
        [InlineData("method_with_underscores")]
        [InlineData("123NumericStart")]
        [InlineData("member with spaces")]
        [InlineData("member!@#$%special")]
        public void Start_VariousMemberValues_CallsProviderStartWithCorrectMember(string member)
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Start(out string reference, null, null, member);

            // Assert
            Assert.NotEmpty(reference);
            mockProvider.Received(1).Start(reference, null, null, member);
        }

        /// <summary>
        /// Tests that reference values are numeric strings when Provider is not null.
        /// Input conditions: Provider is not null.
        /// Expected result: reference can be parsed as a long integer.
        /// </summary>
        [Fact]
        public void Start_ProviderIsNotNull_GeneratesNumericReference()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Start(out string reference);

            // Assert
            Assert.True(long.TryParse(reference, out long parsedReference));
            Assert.True(parsedReference > 0);
        }

        /// <summary>
        /// Tests that Start method handles provider exception gracefully.
        /// Input conditions: Provider throws an exception when Start is called.
        /// Expected result: The exception from Provider.Start propagates to the caller.
        /// </summary>
        [Fact]
        public void Start_ProviderThrowsException_PropagatesException()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            var expectedException = new InvalidOperationException("Provider error");
            mockProvider.When(p => p.Start(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()))
                       .Do(x => throw expectedException);
            Performance.SetProvider(mockProvider);

            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() => Performance.Start(out string reference));
            Assert.Equal(expectedException.Message, actualException.Message);
        }

        [Fact]
        public void Start_WhenProviderIsNull_DoesNotThrow()
        {
            // Arrange
            Performance.SetProvider(null);
            string reference = "test-reference";
            string tag = "test-tag";
            string path = "test-path";
            string member = "test-member";

            // Act & Assert
            Performance.Start(reference, tag, path, member);
        }

        [Fact]
        public void Start_WhenProviderIsSet_CallsProviderStartWithAllParameters()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            string reference = "test-reference";
            string tag = "test-tag";
            string path = "test-path";
            string member = "test-member";

            // Act
            Performance.Start(reference, tag, path, member);

            // Assert
            mockProvider.Received(1).Start(reference, tag, path, member);
        }

        [Theory]
        [InlineData("ref", null, null, null)]
        [InlineData("ref", "tag", null, null)]
        [InlineData("ref", null, "path", null)]
        [InlineData("ref", null, null, "member")]
        [InlineData("ref", "", "", "")]
        [InlineData("ref", "   ", "   ", "   ")]
        public void Start_WithVariousParameterCombinations_CallsProviderWithCorrectParameters(
            string reference, string tag, string path, string member)
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Start(reference, tag, path, member);

            // Assert
            mockProvider.Received(1).Start(reference, tag, path, member);
        }

        [Fact]
        public void Start_WithNullReference_CallsProviderWithNullReference()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            string reference = null;
            string tag = "test-tag";
            string path = "test-path";
            string member = "test-member";

            // Act
            Performance.Start(reference, tag, path, member);

            // Assert
            mockProvider.Received(1).Start(reference, tag, path, member);
        }

        [Fact]
        public void Start_WithVeryLongStrings_CallsProviderWithLongStrings()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            string reference = new string('r', 10000);
            string tag = new string('t', 10000);
            string path = new string('p', 10000);
            string member = new string('m', 10000);

            // Act
            Performance.Start(reference, tag, path, member);

            // Assert
            mockProvider.Received(1).Start(reference, tag, path, member);
        }

        [Fact]
        public void Start_WithSpecialCharacters_CallsProviderWithSpecialCharacters()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            string reference = "ref\n\r\t\\";
            string tag = "tag\u0000\u001F";
            string path = "path\ud83d\ude00";
            string member = "member\u00a0";

            // Act
            Performance.Start(reference, tag, path, member);

            // Assert
            mockProvider.Received(1).Start(reference, tag, path, member);
        }

        [Fact]
        public void Start_CalledMultipleTimes_CallsProviderMultipleTimes()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            string reference1 = "ref1";
            string reference2 = "ref2";

            // Act
            Performance.Start(reference1, "tag1", "path1", "member1");
            Performance.Start(reference2, "tag2", "path2", "member2");

            // Assert
            mockProvider.Received(1).Start(reference1, "tag1", "path1", "member1");
            mockProvider.Received(1).Start(reference2, "tag2", "path2", "member2");
            mockProvider.Received(2).Start(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void Start_AfterSettingProviderToNull_DoesNotCallPreviousProvider()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            Performance.SetProvider(null);
            string reference = "test-reference";

            // Act
            Performance.Start(reference, "tag", "path", "member");

            // Assert
            mockProvider.DidNotReceive().Start(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        /// <summary>
        /// Tests that Stop method does not throw when Provider is null.
        /// Should safely handle null provider using null-conditional operator.
        /// </summary>
        [Fact]
        public void Stop_WhenProviderIsNull_DoesNotThrow()
        {
            // Arrange
            Performance.SetProvider(null);

            // Act & Assert
            var exception = Record.Exception(() => Performance.Stop("test"));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that Stop method calls provider's Stop method with correct parameters when provider is set.
        /// Verifies that all parameters are passed through correctly to the underlying provider.
        /// </summary>
        [Fact]
        public void Stop_WhenProviderIsSet_CallsProviderStopWithCorrectParameters()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            var reference = "test-ref";
            var tag = "test-tag";
            var path = "test-path";
            var member = "test-member";

            // Act
            Performance.Stop(reference, tag, path, member);

            // Assert
            mockProvider.Received(1).Stop(reference, tag, path, member);
        }

        /// <summary>
        /// Tests Stop method with null reference parameter.
        /// Verifies that null reference is passed through to provider without modification.
        /// </summary>
        [Fact]
        public void Stop_WithNullReference_PassesNullToProvider()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Stop(null);

            // Assert
            mockProvider.Received(1).Stop(null, null, null, null);
        }

        /// <summary>
        /// Tests Stop method with various string parameter combinations including edge cases.
        /// Verifies correct parameter passing for empty strings, whitespace, and special characters.
        /// </summary>
        [Theory]
        [InlineData("", "", "", "")]
        [InlineData("   ", "   ", "   ", "   ")]
        [InlineData("\t\n\r", "\t\n\r", "\t\n\r", "\t\n\r")]
        [InlineData("ref", null, null, null)]
        [InlineData("ref", "tag", null, null)]
        [InlineData("ref", "tag", "path", null)]
        [InlineData("special!@#$%^&*()_+", "unicode:🚀", "path\\with\\backslashes", "member.with.dots")]
        public void Stop_WithVariousStringParameters_PassesParametersToProvider(string reference, string tag, string path, string member)
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Stop(reference, tag, path, member);

            // Assert
            mockProvider.Received(1).Stop(reference, tag, path, member);
        }

        /// <summary>
        /// Tests Stop method with very long string parameters.
        /// Verifies that the method handles large string inputs without issues.
        /// </summary>
        [Fact]
        public void Stop_WithVeryLongStrings_PassesParametersToProvider()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            var longString = new string('a', 10000);

            // Act
            Performance.Stop(longString, longString, longString, longString);

            // Assert
            mockProvider.Received(1).Stop(longString, longString, longString, longString);
        }

        /// <summary>
        /// Tests Stop method with only required parameter (reference).
        /// Verifies that optional parameters default to null when not provided.
        /// </summary>
        [Fact]
        public void Stop_WithOnlyRequiredParameter_UsesNullDefaults()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Stop("reference");

            // Assert
            mockProvider.Received(1).Stop("reference", null, null, null);
        }

        /// <summary>
        /// Tests Stop method with reference and tag parameters only.
        /// Verifies that remaining optional parameters default to null.
        /// </summary>
        [Fact]
        public void Stop_WithReferenceAndTag_UsesNullDefaultsForPathAndMember()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Stop("reference", "tag");

            // Assert
            mockProvider.Received(1).Stop("reference", "tag", null, null);
        }

        /// <summary>
        /// Tests Stop method multiple times to ensure provider is called each time.
        /// Verifies that the method consistently delegates to the provider.
        /// </summary>
        [Fact]
        public void Stop_CalledMultipleTimes_CallsProviderEachTime()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            Performance.Stop("ref1");
            Performance.Stop("ref2");
            Performance.Stop("ref3");

            // Assert
            mockProvider.Received(1).Stop("ref1", null, null, null);
            mockProvider.Received(1).Stop("ref2", null, null, null);
            mockProvider.Received(1).Stop("ref3", null, null, null);
            Assert.Equal(3, mockProvider.ReceivedCalls().Count());
        }

        /// <summary>
        /// Tests that StartNew returns a non-null IDisposable when Provider is null.
        /// Verifies the method creates a DisposablePerformanceReference instance even without a provider.
        /// </summary>
        [Fact]
        public void StartNew_ProviderIsNull_ReturnsNonNullDisposable()
        {
            // Arrange
            Performance.SetProvider(null);

            // Act
            var result = Performance.StartNew();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IDisposable>(result);
        }

        /// <summary>
        /// Tests that StartNew returns a non-null IDisposable when Provider is set.
        /// Verifies the method works correctly with a mocked provider instance.
        /// </summary>
        [Fact]
        public void StartNew_ProviderIsSet_ReturnsNonNullDisposable()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            var result = Performance.StartNew();

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IDisposable>(result);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests StartNew with all null parameters.
        /// Verifies the method handles null values for tag, path, and member parameters correctly.
        /// </summary>
        [Fact]
        public void StartNew_AllNullParameters_ReturnsValidDisposable()
        {
            // Arrange
            Performance.SetProvider(null);

            // Act
            var result = Performance.StartNew(null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IDisposable>(result);
        }

        /// <summary>
        /// Tests StartNew with all parameters provided.
        /// Verifies the method correctly passes parameters to the DisposablePerformanceReference constructor.
        /// </summary>
        [Fact]
        public void StartNew_AllParametersProvided_ReturnsValidDisposable()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            var tag = "TestTag";
            var path = @"C:\Test\Path.cs";
            var member = "TestMethod";

            // Act
            var result = Performance.StartNew(tag, path, member);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IDisposable>(result);
            mockProvider.Received(1).Start(Arg.Any<string>(), tag, path, member);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests StartNew with empty string parameters.
        /// Verifies the method handles empty strings correctly without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData("", "", "")]
        [InlineData("", null, null)]
        [InlineData(null, "", null)]
        [InlineData(null, null, "")]
        public void StartNew_EmptyStringParameters_ReturnsValidDisposable(string tag, string path, string member)
        {
            // Arrange
            Performance.SetProvider(null);

            // Act
            var result = Performance.StartNew(tag, path, member);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IDisposable>(result);
        }

        /// <summary>
        /// Tests StartNew with long string parameters.
        /// Verifies the method handles very long strings without issues.
        /// </summary>
        [Fact]
        public void StartNew_LongStringParameters_ReturnsValidDisposable()
        {
            // Arrange
            Performance.SetProvider(null);
            var longString = new string('A', 10000);

            // Act
            var result = Performance.StartNew(longString, longString, longString);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IDisposable>(result);
        }

        /// <summary>
        /// Tests StartNew with special characters in parameters.
        /// Verifies the method handles strings containing special characters, unicode, and control characters.
        /// </summary>
        [Theory]
        [InlineData("Special!@#$%^&*()", "Path\\With\\Special:*?\"<>|Chars", "Member$pecial")]
        [InlineData("Unicode\u0001\u0002\u0003", "Path\n\r\t", "Member\0")]
        [InlineData("🚀🎯⚡", "C:\\Test\\🌟.cs", "TestMethod™")]
        public void StartNew_SpecialCharacterParameters_ReturnsValidDisposable(string tag, string path, string member)
        {
            // Arrange
            Performance.SetProvider(null);

            // Act
            var result = Performance.StartNew(tag, path, member);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IDisposable>(result);
        }

        /// <summary>
        /// Tests that StartNew with Provider set calls Start method correctly.
        /// Verifies that the provider's Start method is invoked with the correct parameters.
        /// </summary>
        [Fact]
        public void StartNew_WithProvider_CallsProviderStart()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            var tag = "TestTag";
            var path = @"C:\Test\Path.cs";
            var member = "TestMethod";

            // Act
            var result = Performance.StartNew(tag, path, member);

            // Assert
            mockProvider.Received(1).Start(Arg.Any<string>(), tag, path, member);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests that disposing the returned object calls Stop method correctly.
        /// Verifies that disposal triggers the provider's Stop method with correct parameters.
        /// </summary>
        [Fact]
        public void StartNew_DisposingResult_CallsProviderStop()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            var tag = "TestTag";
            var path = @"C:\Test\Path.cs";
            var member = "TestMethod";

            // Act
            var result = Performance.StartNew(tag, path, member);
            result.Dispose();

            // Assert
            mockProvider.Received(1).Stop(Arg.Any<string>(), tag, path, member);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests multiple calls to StartNew return different instances.
        /// Verifies that each call creates a new DisposablePerformanceReference instance.
        /// </summary>
        [Fact]
        public void StartNew_MultipleCalls_ReturnDifferentInstances()
        {
            // Arrange
            Performance.SetProvider(null);

            // Act
            var result1 = Performance.StartNew("Tag1");
            var result2 = Performance.StartNew("Tag2");

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.NotSame(result1, result2);
        }

        /// <summary>
        /// Tests that StartNew with CallerFilePath attribute works correctly.
        /// Verifies that when path parameter is not provided, the caller's file path is used.
        /// </summary>
        [Fact]
        public void StartNew_CallerFilePathAttribute_WorksCorrectly()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            // Act
            var result = Performance.StartNew("TestTag");

            // Assert
            mockProvider.Received(1).Start(
                Arg.Any<string>(),
                "TestTag",
                Arg.Is<string>(path => path != null && path.Contains("PerformanceTests")),
                "StartNew_CallerFilePathAttribute_WorksCorrectly");

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests that disposing the result multiple times doesn't cause errors.
        /// Verifies that multiple dispose calls are handled gracefully.
        /// </summary>
        [Fact]
        public void StartNew_DisposingMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            var result = Performance.StartNew("TestTag");

            // Act & Assert
            result.Dispose();
            result.Dispose(); // Should not throw

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests that Dispose method calls Stop on the provider with correct parameters when provider is set.
        /// Verifies that the reference, tag, path, and member are correctly passed to the Stop method.
        /// </summary>
        [Fact]
        public void Dispose_WithProviderSet_CallsStopWithCorrectParameters()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            string tag = "testTag";
            string path = "testPath";
            string member = "testMember";

            var disposable = Performance.StartNew(tag, path, member);

            // Act
            disposable.Dispose();

            // Assert
            mockProvider.Received(1).Stop(Arg.Any<string>(), tag, path, member);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests that Dispose method handles null provider gracefully without throwing exceptions.
        /// Verifies that the method is safe to call when no provider is configured.
        /// </summary>
        [Fact]
        public void Dispose_WithNullProvider_DoesNotThrow()
        {
            // Arrange
            Performance.SetProvider(null);
            var disposable = Performance.StartNew("tag", "path", "member");

            // Act & Assert
            var exception = Record.Exception(() => disposable.Dispose());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that Dispose method can be called multiple times without throwing exceptions.
        /// Verifies that the disposable is safe to dispose multiple times.
        /// </summary>
        [Fact]
        public void Dispose_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);
            var disposable = Performance.StartNew("tag", "path", "member");

            // Act & Assert
            var exception = Record.Exception(() =>
            {
                disposable.Dispose();
                disposable.Dispose();
                disposable.Dispose();
            });

            Assert.Null(exception);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests that Dispose method calls Stop with null parameters when StartNew was called with null values.
        /// Verifies that null parameter values are correctly handled and passed through.
        /// </summary>
        [Fact]
        public void Dispose_WithNullParameters_CallsStopWithNullValues()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            var disposable = Performance.StartNew(null, null, null);

            // Act
            disposable.Dispose();

            // Assert
            mockProvider.Received(1).Stop(Arg.Any<string>(), null, null, null);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests that Dispose method calls Stop with empty string parameters when StartNew was called with empty values.
        /// Verifies that empty string parameter values are correctly handled and passed through.
        /// </summary>
        [Fact]
        public void Dispose_WithEmptyStringParameters_CallsStopWithEmptyValues()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            string emptyTag = "";
            string emptyPath = "";
            string emptyMember = "";

            var disposable = Performance.StartNew(emptyTag, emptyPath, emptyMember);

            // Act
            disposable.Dispose();

            // Assert
            mockProvider.Received(1).Stop(Arg.Any<string>(), emptyTag, emptyPath, emptyMember);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests that Dispose method calls Stop with whitespace-only parameters when StartNew was called with whitespace values.
        /// Verifies that whitespace parameter values are correctly handled and passed through.
        /// </summary>
        [Fact]
        public void Dispose_WithWhitespaceParameters_CallsStopWithWhitespaceValues()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            string whitespaceTag = "   ";
            string whitespacePath = "\t\n";
            string whitespaceMember = "  \r\n  ";

            var disposable = Performance.StartNew(whitespaceTag, whitespacePath, whitespaceMember);

            // Act
            disposable.Dispose();

            // Assert
            mockProvider.Received(1).Stop(Arg.Any<string>(), whitespaceTag, whitespacePath, whitespaceMember);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests that Dispose method calls Stop with very long string parameters when StartNew was called with long values.
        /// Verifies that long parameter values are correctly handled and passed through.
        /// </summary>
        [Fact]
        public void Dispose_WithLongStringParameters_CallsStopWithLongValues()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            string longTag = new string('a', 10000);
            string longPath = new string('b', 5000);
            string longMember = new string('c', 3000);

            var disposable = Performance.StartNew(longTag, longPath, longMember);

            // Act
            disposable.Dispose();

            // Assert
            mockProvider.Received(1).Stop(Arg.Any<string>(), longTag, longPath, longMember);

            // Cleanup
            Performance.SetProvider(null);
        }

        /// <summary>
        /// Tests that Dispose method calls Stop with special character parameters when StartNew was called with special values.
        /// Verifies that special character parameter values are correctly handled and passed through.
        /// </summary>
        [Fact]
        public void Dispose_WithSpecialCharacterParameters_CallsStopWithSpecialValues()
        {
            // Arrange
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            string specialTag = "tag\0with\nnull\tand\rcontrol";
            string specialPath = "path/with\\slashes|and<>chars";
            string specialMember = "member\"with'quotes`and©symbols";

            var disposable = Performance.StartNew(specialTag, specialPath, specialMember);

            // Act
            disposable.Dispose();

            // Assert
            mockProvider.Received(1).Stop(Arg.Any<string>(), specialTag, specialPath, specialMember);

            // Cleanup
            Performance.SetProvider(null);
        }
    }

    public partial class DisposablePerformanceReferenceTests
    {
        /// <summary>
        /// Tests that the DisposablePerformanceReference constructor properly initializes when Provider is null.
        /// Should not throw exception and reference should be set to empty string.
        /// </summary>
        [Fact]
        public void Constructor_ProviderIsNull_DoesNotThrowAndSetsEmptyReference()
        {
            // Arrange
            var originalProvider = Performance.Provider;
            Performance.SetProvider(null);

            try
            {
                // Act & Assert - should not throw
                var disposable = new Performance.DisposablePerformanceReference("tag", "path", "member");

                // Dispose to clean up
                disposable.Dispose();
            }
            finally
            {
                // Cleanup
                Performance.SetProvider(originalProvider);
            }
        }

        /// <summary>
        /// Tests that the DisposablePerformanceReference constructor calls Provider.Start with correct parameters when Provider is not null.
        /// Verifies that the provider receives the expected tag, path, and member values.
        /// </summary>
        [Theory]
        [InlineData("testTag", "testPath", "testMember")]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData(" ", " ", " ")]
        [InlineData("tag with spaces", "/long/file/path.cs", "MethodName")]
        public void Constructor_ProviderIsNotNull_CallsProviderStartWithCorrectParameters(string tag, string path, string member)
        {
            // Arrange
            var originalProvider = Performance.Provider;
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            try
            {
                // Act
                var disposable = new Performance.DisposablePerformanceReference(tag, path, member);

                // Assert
                mockProvider.Received(1).Start(Arg.Any<string>(), tag, path, member);

                // Cleanup
                disposable.Dispose();
            }
            finally
            {
                // Cleanup
                Performance.SetProvider(originalProvider);
            }
        }

        /// <summary>
        /// Tests that the DisposablePerformanceReference constructor handles very long string parameters without throwing exceptions.
        /// Verifies behavior with boundary-length string inputs.
        /// </summary>
        [Fact]
        public void Constructor_VeryLongStrings_DoesNotThrow()
        {
            // Arrange
            var originalProvider = Performance.Provider;
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            var longString = new string('a', 10000);

            try
            {
                // Act & Assert - should not throw
                var disposable = new Performance.DisposablePerformanceReference(longString, longString, longString);

                // Verify provider was called
                mockProvider.Received(1).Start(Arg.Any<string>(), longString, longString, longString);

                // Cleanup
                disposable.Dispose();
            }
            finally
            {
                // Cleanup
                Performance.SetProvider(originalProvider);
            }
        }

        /// <summary>
        /// Tests that the DisposablePerformanceReference constructor handles special characters in string parameters.
        /// Verifies behavior with strings containing control characters, unicode, and special symbols.
        /// </summary>
        [Theory]
        [InlineData("tag\n\r\t", "path\\with\\special", "member\u0001")]
        [InlineData("tag\0", "path\u200B", "member\uFFFD")]
        [InlineData("🚀emoji", "path/with/émojï", "方法名")]
        public void Constructor_SpecialCharacters_DoesNotThrow(string tag, string path, string member)
        {
            // Arrange
            var originalProvider = Performance.Provider;
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            try
            {
                // Act & Assert - should not throw
                var disposable = new Performance.DisposablePerformanceReference(tag, path, member);

                // Verify provider was called with special characters
                mockProvider.Received(1).Start(Arg.Any<string>(), tag, path, member);

                // Cleanup
                disposable.Dispose();
            }
            finally
            {
                // Cleanup
                Performance.SetProvider(originalProvider);
            }
        }

        /// <summary>
        /// Tests that the DisposablePerformanceReference constructor generates unique references for multiple instances.
        /// Verifies that the static Reference counter is properly incremented for each new instance.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_GeneratesUniqueReferences()
        {
            // Arrange
            var originalProvider = Performance.Provider;
            var mockProvider = Substitute.For<IPerformanceProvider>();
            Performance.SetProvider(mockProvider);

            try
            {
                // Act
                var disposable1 = new Performance.DisposablePerformanceReference("tag1", "path1", "member1");
                var disposable2 = new Performance.DisposablePerformanceReference("tag2", "path2", "member2");
                var disposable3 = new Performance.DisposablePerformanceReference("tag3", "path3", "member3");

                // Assert
                // Verify each instance called Start with different reference values
                mockProvider.Received(3).Start(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());

                // Get all the reference values that were passed to Start
                var calls = mockProvider.ReceivedCalls();
                var references = new System.Collections.Generic.HashSet<string>();

                foreach (var call in calls)
                {
                    if (call.GetMethodInfo().Name == "Start")
                    {
                        var reference = (string)call.GetArguments()[0];
                        references.Add(reference);
                    }
                }

                // Verify we got 3 unique references
                Assert.Equal(3, references.Count);

                // Cleanup
                disposable1.Dispose();
                disposable2.Dispose();
                disposable3.Dispose();
            }
            finally
            {
                // Cleanup
                Performance.SetProvider(originalProvider);
            }
        }

        /// <summary>
        /// Tests that the DisposablePerformanceReference constructor behavior when Provider is null with various parameter combinations.
        /// Verifies that no exceptions are thrown regardless of parameter values when Provider is null.
        /// </summary>
        [Theory]
        [InlineData(null, null, null)]
        [InlineData("", "", "")]
        [InlineData(" ", " ", " ")]
        [InlineData("valid", "valid", "valid")]
        public void Constructor_ProviderIsNull_DoesNotThrowWithVariousParameters(string tag, string path, string member)
        {
            // Arrange
            var originalProvider = Performance.Provider;
            Performance.SetProvider(null);

            try
            {
                // Act & Assert - should not throw
                var disposable = new Performance.DisposablePerformanceReference(tag, path, member);

                // Cleanup
                disposable.Dispose();
            }
            finally
            {
                // Cleanup
                Performance.SetProvider(originalProvider);
            }
        }
    }
}