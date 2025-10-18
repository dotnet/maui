#nullable disable

#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class CastingEnumeratorTests
    {
        /// <summary>
        /// Tests that the CastingEnumerator constructor properly stores a valid enumerator parameter.
        /// Input: A valid IEnumerator mock instance.
        /// Expected: Constructor completes successfully and the enumerator is accessible through subsequent operations.
        /// </summary>
        [Fact]
        public void CastingEnumerator_ValidEnumerator_StoresEnumeratorCorrectly()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<object>>();
            mockEnumerator.MoveNext().Returns(true);

            // Act
            var castingEnumerator = new CastingEnumerator<string, object>(mockEnumerator);

            // Assert
            // Verify constructor completed without throwing
            Assert.NotNull(castingEnumerator);

            // Verify the enumerator was stored by testing delegation to underlying enumerator
            var result = castingEnumerator.MoveNext();
            Assert.True(result);
            mockEnumerator.Received(1).MoveNext();
        }

        /// <summary>
        /// Tests that the CastingEnumerator constructor accepts a null enumerator parameter.
        /// Input: null enumerator parameter.
        /// Expected: Constructor completes without throwing, but subsequent operations may fail.
        /// </summary>
        [Fact]
        public void CastingEnumerator_NullEnumerator_AcceptsNullParameter()
        {
            // Arrange
            IEnumerator<object> nullEnumerator = null;

            // Act & Assert
            var exception = Record.Exception(() => new CastingEnumerator<string, object>(nullEnumerator));

            // Constructor should not throw with null parameter
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the CastingEnumerator constructor works with different generic type combinations.
        /// Input: Various combinations of reference types for T and TFrom generic parameters.
        /// Expected: Constructor completes successfully for all valid reference type combinations.
        /// </summary>
        [Theory]
        [InlineData(typeof(string), typeof(object))]
        [InlineData(typeof(object), typeof(string))]
        [InlineData(typeof(List<int>), typeof(IEnumerable<int>))]
        public void CastingEnumerator_DifferentGenericTypes_ConstructorSucceeds(Type tType, Type tFromType)
        {
            // Arrange
            var enumeratorType = typeof(IEnumerator<>).MakeGenericType(tFromType);
            var mockEnumerator = Substitute.For(new[] { enumeratorType }, new object[0]);

            var castingEnumeratorType = typeof(CastingEnumerator<,>).MakeGenericType(tType, tFromType);

            // Act
            var exception = Record.Exception(() =>
                Activator.CreateInstance(castingEnumeratorType, mockEnumerator));

            // Assert
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the constructor properly initializes the disposed state to false.
        /// Input: A valid enumerator.
        /// Expected: The CastingEnumerator is created in a non-disposed state and can be used normally.
        /// </summary>
        [Fact]
        public void CastingEnumerator_NewInstance_IsNotDisposed()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<object>>();
            mockEnumerator.Current.Returns(new object());

            // Act
            var castingEnumerator = new CastingEnumerator<object, object>(mockEnumerator);

            // Assert
            // Verify the enumerator can be used (not disposed) by accessing Current
            var current = castingEnumerator.Current;

            // Should not throw, indicating the enumerator is properly initialized and not disposed
            mockEnumerator.Received(1).Current;
        }

        /// <summary>
        /// Tests that Reset method calls Reset on the underlying enumerator.
        /// Input conditions: Valid CastingEnumerator with mocked underlying enumerator.
        /// Expected result: Underlying enumerator's Reset method is called once.
        /// </summary>
        [Fact]
        public void Reset_CallsUnderlyingEnumeratorReset_WhenCalled()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<string>>();
            var castingEnumerator = new CastingEnumerator<object, string>(mockEnumerator);

            // Act
            castingEnumerator.Reset();

            // Assert
            mockEnumerator.Received(1).Reset();
        }

        /// <summary>
        /// Tests that Reset method propagates exceptions thrown by the underlying enumerator.
        /// Input conditions: Underlying enumerator that throws InvalidOperationException on Reset.
        /// Expected result: InvalidOperationException is propagated to the caller.
        /// </summary>
        [Fact]
        public void Reset_PropagatesException_WhenUnderlyingEnumeratorThrows()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<string>>();
            var expectedException = new InvalidOperationException("Reset not supported");
            mockEnumerator.When(x => x.Reset()).Do(_ => throw expectedException);
            var castingEnumerator = new CastingEnumerator<object, string>(mockEnumerator);

            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() => castingEnumerator.Reset());
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that Reset method can be called multiple times successfully.
        /// Input conditions: Valid CastingEnumerator called Reset multiple times.
        /// Expected result: Underlying enumerator's Reset method is called the expected number of times.
        /// </summary>
        [Fact]
        public void Reset_CanBeCalledMultipleTimes_WithoutIssues()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<string>>();
            var castingEnumerator = new CastingEnumerator<object, string>(mockEnumerator);

            // Act
            castingEnumerator.Reset();
            castingEnumerator.Reset();
            castingEnumerator.Reset();

            // Assert
            mockEnumerator.Received(3).Reset();
        }

        /// <summary>
        /// Tests that Reset method propagates NotSupportedException thrown by the underlying enumerator.
        /// Input conditions: Underlying enumerator that throws NotSupportedException on Reset (common for many enumerators).
        /// Expected result: NotSupportedException is propagated to the caller.
        /// </summary>
        [Fact]
        public void Reset_PropagatesNotSupportedException_WhenUnderlyingEnumeratorThrows()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<string>>();
            var expectedException = new NotSupportedException("Reset is not supported");
            mockEnumerator.When(x => x.Reset()).Do(_ => throw expectedException);
            var castingEnumerator = new CastingEnumerator<object, string>(mockEnumerator);

            // Act & Assert
            var actualException = Assert.Throws<NotSupportedException>(() => castingEnumerator.Reset());
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that the first call to Dispose properly disposes the wrapped enumerator.
        /// Verifies that the wrapped enumerator's Dispose method is called exactly once.
        /// </summary>
        [Fact]
        public void Dispose_FirstCall_DisposesWrappedEnumerator()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<string>>();
            var castingEnumerator = new CastingEnumerator<string, string>(mockEnumerator);

            // Act
            castingEnumerator.Dispose();

            // Assert
            mockEnumerator.Received(1).Dispose();
        }

        /// <summary>
        /// Tests that subsequent calls to Dispose do not call the wrapped enumerator's Dispose method again.
        /// This test covers the double-disposal protection logic and ensures the _disposed flag works correctly.
        /// </summary>
        [Fact]
        public void Dispose_SecondCall_DoesNotDisposeWrappedEnumeratorAgain()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<string>>();
            var castingEnumerator = new CastingEnumerator<string, string>(mockEnumerator);

            // Act
            castingEnumerator.Dispose(); // First call
            castingEnumerator.Dispose(); // Second call

            // Assert
            mockEnumerator.Received(1).Dispose(); // Should only be called once
        }

        /// <summary>
        /// Tests that multiple calls to Dispose are safe and do not cause issues.
        /// Verifies the double-disposal protection works correctly across multiple calls.
        /// </summary>
        [Fact]
        public void Dispose_MultipleCalls_SafeAndWrappedEnumeratorDisposedOnlyOnce()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<string>>();
            var castingEnumerator = new CastingEnumerator<string, string>(mockEnumerator);

            // Act
            castingEnumerator.Dispose(); // First call
            castingEnumerator.Dispose(); // Second call
            castingEnumerator.Dispose(); // Third call

            // Assert
            mockEnumerator.Received(1).Dispose(); // Should only be called once
        }

        /// <summary>
        /// Tests that if the wrapped enumerator throws an exception during disposal,
        /// the exception is properly propagated to the caller.
        /// </summary>
        [Fact]
        public void Dispose_WrappedEnumeratorThrowsException_ExceptionPropagated()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<string>>();
            var expectedException = new InvalidOperationException("Disposal failed");
            mockEnumerator.When(x => x.Dispose()).Do(x => throw expectedException);
            var castingEnumerator = new CastingEnumerator<string, string>(mockEnumerator);

            // Act & Assert
            var thrownException = Assert.Throws<InvalidOperationException>(() => castingEnumerator.Dispose());
            Assert.Same(expectedException, thrownException);
        }

        /// <summary>
        /// Tests that after an exception occurs during the first disposal attempt,
        /// subsequent calls to Dispose do not attempt to dispose the wrapped enumerator again.
        /// This verifies that the _disposed flag is still set even when an exception occurs.
        /// </summary>
        [Fact]
        public void Dispose_ExceptionOnFirstCallThenSecondCall_DoesNotCallWrappedEnumeratorAgain()
        {
            // Arrange
            var mockEnumerator = Substitute.For<IEnumerator<string>>();
            var expectedException = new InvalidOperationException("Disposal failed");
            mockEnumerator.When(x => x.Dispose()).Do(x => throw expectedException);
            var castingEnumerator = new CastingEnumerator<string, string>(mockEnumerator);

            // Act
            Assert.Throws<InvalidOperationException>(() => castingEnumerator.Dispose()); // First call throws
            castingEnumerator.Dispose(); // Second call should be safe

            // Assert
            mockEnumerator.Received(1).Dispose(); // Should only be called once despite the exception
        }
    }
}