#nullable disable

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class StreamWrapperTests
    {
        /// <summary>
        /// Tests that the StreamWrapper constructor throws ArgumentNullException when passed a null stream.
        /// This verifies that the convenience constructor properly delegates validation to the main constructor.
        /// </summary>
        [Fact]
        public void Constructor_WithNullStream_ThrowsArgumentNullException()
        {
            // Arrange
            Stream nullStream = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new StreamWrapper(nullStream));
            Assert.Equal("wrapped", exception.ParamName);
        }

        /// <summary>
        /// Tests that the StreamWrapper constructor successfully creates an instance when passed a valid stream.
        /// This verifies that the convenience constructor properly delegates to the main constructor with null as the additional disposable.
        /// </summary>
        [Fact]
        public void Constructor_WithValidStream_CreatesWrapperSuccessfully()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();

            // Act
            var wrapper = new StreamWrapper(mockStream);

            // Assert
            Assert.NotNull(wrapper);
        }

        /// <summary>
        /// Tests that the StreamWrapper constructor works with different types of stream implementations.
        /// This ensures the convenience constructor handles various stream types correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithMemoryStream_CreatesWrapperSuccessfully()
        {
            // Arrange
            using var memoryStream = new MemoryStream();

            // Act
            var wrapper = new StreamWrapper(memoryStream);

            // Assert
            Assert.NotNull(wrapper);
        }

        /// <summary>
        /// Tests that the constructor throws ArgumentNullException when the wrapped parameter is null.
        /// Validates that the correct exception type is thrown with the expected parameter name.
        /// </summary>
        [Fact]
        public void Constructor_WithNullWrapped_ThrowsArgumentNullException()
        {
            // Arrange
            Stream wrapped = null;
            IDisposable additionalDisposable = Substitute.For<IDisposable>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new StreamWrapper(wrapped, additionalDisposable));
            Assert.Equal("wrapped", exception.ParamName);
        }

        /// <summary>
        /// Tests that the constructor completes successfully when provided with a valid wrapped stream
        /// and null additionalDisposable parameter. Verifies that the object is created without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_WithValidWrappedAndNullAdditionalDisposable_CreatesInstance()
        {
            // Arrange
            var wrapped = Substitute.For<Stream>();
            IDisposable additionalDisposable = null;

            // Act
            var streamWrapper = new StreamWrapper(wrapped, additionalDisposable);

            // Assert
            Assert.NotNull(streamWrapper);
        }

        /// <summary>
        /// Tests that the constructor completes successfully when provided with valid parameters
        /// for both wrapped stream and additionalDisposable. Verifies that the object is created without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_WithValidWrappedAndValidAdditionalDisposable_CreatesInstance()
        {
            // Arrange
            var wrapped = Substitute.For<Stream>();
            var additionalDisposable = Substitute.For<IDisposable>();

            // Act
            var streamWrapper = new StreamWrapper(wrapped, additionalDisposable);

            // Assert
            Assert.NotNull(streamWrapper);
        }

        /// <summary>
        /// Tests that the CanRead property correctly delegates to the wrapped stream's CanRead property.
        /// Tests both true and false scenarios to ensure proper delegation behavior.
        /// </summary>
        /// <param name="canRead">The value that the wrapped stream's CanRead property should return</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanRead_DelegatesToWrappedStream_ReturnsExpectedValue(bool canRead)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanRead.Returns(canRead);
            var streamWrapper = new StreamWrapper(mockStream);

            // Act
            var result = streamWrapper.CanRead;

            // Assert
            Assert.Equal(canRead, result);
        }

        /// <summary>
        /// Test wrapper class that exposes the protected Dispose method for testing purposes.
        /// </summary>
        private class TestableStreamWrapper : StreamWrapper
        {
            public TestableStreamWrapper(Stream wrapped) : base(wrapped)
            {
            }

            public TestableStreamWrapper(Stream wrapped, IDisposable additionalDisposable) : base(wrapped, additionalDisposable)
            {
            }

            public void TestDispose(bool disposing)
            {
                Dispose(disposing);
            }
        }

        /// <summary>
        /// Tests that Dispose method properly disposes the wrapped stream, raises the Disposed event,
        /// disposes additional disposable, sets it to null, and calls base disposal with disposing=true.
        /// </summary>
        [Fact]
        public void Dispose_WithDisposingTrueAndAdditionalDisposable_DisposesAllResourcesAndRaisesEvent()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var additionalDisposable = Substitute.For<IDisposable>();
            var wrapper = new TestableStreamWrapper(wrappedStream, additionalDisposable);

            var eventRaised = false;
            EventArgs capturedEventArgs = null;
            object capturedSender = null;

            wrapper.Disposed += (sender, args) =>
            {
                eventRaised = true;
                capturedSender = sender;
                capturedEventArgs = args;
            };

            // Act
            wrapper.TestDispose(true);

            // Assert
            wrappedStream.Received(1).Dispose();
            additionalDisposable.Received(1).Dispose();
            Assert.True(eventRaised);
            Assert.Same(wrapper, capturedSender);
            Assert.Same(EventArgs.Empty, capturedEventArgs);
        }

        /// <summary>
        /// Tests that Dispose method properly disposes the wrapped stream, raises the Disposed event,
        /// disposes additional disposable, sets it to null, and calls base disposal with disposing=false.
        /// </summary>
        [Fact]
        public void Dispose_WithDisposingFalseAndAdditionalDisposable_DisposesAllResourcesAndRaisesEvent()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var additionalDisposable = Substitute.For<IDisposable>();
            var wrapper = new TestableStreamWrapper(wrappedStream, additionalDisposable);

            var eventRaised = false;
            wrapper.Disposed += (sender, args) => eventRaised = true;

            // Act
            wrapper.TestDispose(false);

            // Assert
            wrappedStream.Received(1).Dispose();
            additionalDisposable.Received(1).Dispose();
            Assert.True(eventRaised);
        }

        /// <summary>
        /// Tests that Dispose method properly disposes the wrapped stream and raises the Disposed event
        /// when additional disposable is null, ensuring no null reference exception occurs.
        /// </summary>
        [Fact]
        public void Dispose_WithNullAdditionalDisposable_DisposesWrappedStreamAndRaisesEvent()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var wrapper = new TestableStreamWrapper(wrappedStream, null);

            var eventRaised = false;
            wrapper.Disposed += (sender, args) => eventRaised = true;

            // Act
            wrapper.TestDispose(true);

            // Assert
            wrappedStream.Received(1).Dispose();
            Assert.True(eventRaised);
        }

        /// <summary>
        /// Tests that Dispose method works correctly when there are no event subscribers,
        /// ensuring no exceptions are thrown and all resources are properly disposed.
        /// </summary>
        [Fact]
        public void Dispose_WithNoEventSubscribers_DisposesResourcesWithoutException()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var additionalDisposable = Substitute.For<IDisposable>();
            var wrapper = new TestableStreamWrapper(wrappedStream, additionalDisposable);

            // Act & Assert (should not throw)
            wrapper.TestDispose(true);

            wrappedStream.Received(1).Dispose();
            additionalDisposable.Received(1).Dispose();
        }

        /// <summary>
        /// Tests that Dispose method correctly handles multiple event subscribers,
        /// ensuring all subscribers are notified when disposal occurs.
        /// </summary>
        [Fact]
        public void Dispose_WithMultipleEventSubscribers_NotifiesAllSubscribers()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var wrapper = new TestableStreamWrapper(wrappedStream);

            var eventRaisedCount = 0;
            EventHandler handler1 = (sender, args) => eventRaisedCount++;
            EventHandler handler2 = (sender, args) => eventRaisedCount++;
            EventHandler handler3 = (sender, args) => eventRaisedCount++;

            wrapper.Disposed += handler1;
            wrapper.Disposed += handler2;
            wrapper.Disposed += handler3;

            // Act
            wrapper.TestDispose(true);

            // Assert
            Assert.Equal(3, eventRaisedCount);
            wrappedStream.Received(1).Dispose();
        }

        /// <summary>
        /// Tests idempotent behavior of Dispose method to ensure multiple calls
        /// don't cause issues, though Stream.Dispose() will be called multiple times.
        /// </summary>
        [Fact]
        public void Dispose_CalledMultipleTimes_HandlesMultipleCallsGracefully()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var additionalDisposable = Substitute.For<IDisposable>();
            var wrapper = new TestableStreamWrapper(wrappedStream, additionalDisposable);

            var eventRaisedCount = 0;
            wrapper.Disposed += (sender, args) => eventRaisedCount++;

            // Act
            wrapper.TestDispose(true);
            wrapper.TestDispose(false);
            wrapper.TestDispose(true);

            // Assert
            wrappedStream.Received(3).Dispose();
            additionalDisposable.Received(3).Dispose();
            Assert.Equal(3, eventRaisedCount);
        }

        /// <summary>
        /// Tests that the CanWrite property correctly delegates to the wrapped stream's CanWrite property.
        /// Verifies that the StreamWrapper returns the same value as the underlying stream for both true and false cases.
        /// </summary>
        /// <param name="canWrite">The value that the wrapped stream's CanWrite property should return.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanWrite_WhenWrappedStreamCanWriteIs_ReturnsExpectedValue(bool canWrite)
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            wrappedStream.CanWrite.Returns(canWrite);
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act
            var result = streamWrapper.CanWrite;

            // Assert
            Assert.Equal(canWrite, result);
        }

        /// <summary>
        /// Tests that Length property returns the wrapped stream's length for various valid length values.
        /// Verifies correct delegation to the underlying stream across different boundary conditions.
        /// </summary>
        /// <param name="expectedLength">The length value that the wrapped stream should return.</param>
        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(100L)]
        [InlineData(1024L)]
        [InlineData(long.MaxValue)]
        public void Length_WithValidWrappedStreamLength_ReturnsWrappedLength(long expectedLength)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.Length.Returns(expectedLength);
            var streamWrapper = new StreamWrapper(mockStream);

            // Act
            var actualLength = streamWrapper.Length;

            // Assert
            Assert.Equal(expectedLength, actualLength);
        }

        /// <summary>
        /// Tests that Length property correctly propagates NotSupportedException when the wrapped stream doesn't support length.
        /// Verifies that exceptions from the underlying stream are properly bubbled up to the caller.
        /// </summary>
        [Fact]
        public void Length_WhenWrappedStreamThrowsNotSupportedException_PropagatesException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.Length.Returns(x => throw new NotSupportedException("Length not supported"));
            var streamWrapper = new StreamWrapper(mockStream);

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => streamWrapper.Length);
            Assert.Equal("Length not supported", exception.Message);
        }

        /// <summary>
        /// Tests that Length property correctly propagates ObjectDisposedException when the wrapped stream is disposed.
        /// Verifies proper exception handling when accessing length of a disposed stream.
        /// </summary>
        [Fact]
        public void Length_WhenWrappedStreamThrowsObjectDisposedException_PropagatesException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.Length.Returns(x => throw new ObjectDisposedException("stream"));
            var streamWrapper = new StreamWrapper(mockStream);

            // Act & Assert
            var exception = Assert.Throws<ObjectDisposedException>(() => streamWrapper.Length);
            Assert.Equal("stream", exception.ObjectName);
        }

        /// <summary>
        /// Tests that Length property correctly propagates IOException when the wrapped stream encounters an I/O error.
        /// Verifies that I/O exceptions from the underlying stream are properly handled.
        /// </summary>
        [Fact]
        public void Length_WhenWrappedStreamThrowsIOException_PropagatesException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.Length.Returns(x => throw new IOException("I/O error occurred"));
            var streamWrapper = new StreamWrapper(mockStream);

            // Act & Assert
            var exception = Assert.Throws<IOException>(() => streamWrapper.Length);
            Assert.Equal("I/O error occurred", exception.Message);
        }

        /// <summary>
        /// Tests that Length property returns negative values when the wrapped stream returns them.
        /// While unusual, this tests the direct delegation behavior without additional validation.
        /// </summary>
        [Theory]
        [InlineData(-1L)]
        [InlineData(-100L)]
        [InlineData(long.MinValue)]
        public void Length_WithNegativeWrappedStreamLength_ReturnsNegativeLength(long expectedLength)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.Length.Returns(expectedLength);
            var streamWrapper = new StreamWrapper(mockStream);

            // Act
            var actualLength = streamWrapper.Length;

            // Assert
            Assert.Equal(expectedLength, actualLength);
        }

        /// <summary>
        /// Tests that GetStreamAsync throws ArgumentNullException when HttpClient is null.
        /// Input: null HttpClient
        /// Expected: ArgumentNullException
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_NullHttpClient_ThrowsArgumentNullException()
        {
            // Arrange
            var uri = new Uri("https://example.com");
            var cancellationToken = CancellationToken.None;
            HttpClient client = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                StreamWrapper.GetStreamAsync(uri, cancellationToken, client));
        }

        /// <summary>
        /// Tests that GetStreamAsync throws ArgumentNullException when Uri is null.
        /// Input: null Uri
        /// Expected: ArgumentNullException
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_NullUri_ThrowsArgumentNullException()
        {
            // Arrange
            Uri uri = null;
            var cancellationToken = CancellationToken.None;
            var client = Substitute.For<HttpClient>();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                StreamWrapper.GetStreamAsync(uri, cancellationToken, client));
        }

        /// <summary>
        /// Tests that GetStreamAsync throws OperationCancelledException when CancellationToken is cancelled.
        /// Input: cancelled CancellationToken
        /// Expected: OperationCancelledException
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_CancelledToken_ThrowsOperationCancelledException()
        {
            // Arrange
            var uri = new Uri("https://example.com");
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();
            var cancellationToken = cancellationTokenSource.Token;
            var client = Substitute.For<HttpClient>();

            client.GetAsync(uri, cancellationToken).Returns(Task.FromCanceled<HttpResponseMessage>(cancellationToken));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCancelledException>(() =>
                StreamWrapper.GetStreamAsync(uri, cancellationToken, client));
        }

        /// <summary>
        /// Tests that GetStreamAsync returns null when HTTP response is unsuccessful.
        /// Input: unsuccessful HTTP response (status code 404)
        /// Expected: null return value
        /// </summary>
        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        public async Task GetStreamAsync_UnsuccessfulResponse_ReturnsNull(HttpStatusCode statusCode)
        {
            // Arrange
            var uri = new Uri("https://example.com");
            var cancellationToken = CancellationToken.None;
            var client = Substitute.For<HttpClient>();
            var response = Substitute.For<HttpResponseMessage>();

            response.IsSuccessStatusCode.Returns(false);
            response.StatusCode.Returns(statusCode);
            client.GetAsync(uri, cancellationToken).Returns(Task.FromResult(response));

            // Act
            var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetStreamAsync returns StreamWrapper when HTTP response is successful.
        /// Input: successful HTTP response (status code 200)
        /// Expected: StreamWrapper containing response stream
        /// </summary>
        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NoContent)]
        public async Task GetStreamAsync_SuccessfulResponse_ReturnsStreamWrapper(HttpStatusCode statusCode)
        {
            // Arrange
            var uri = new Uri("https://example.com");
            var cancellationToken = CancellationToken.None;
            var client = Substitute.For<HttpClient>();
            var response = Substitute.For<HttpResponseMessage>();
            var content = Substitute.For<HttpContent>();
            var responseStream = new MemoryStream();

            response.IsSuccessStatusCode.Returns(true);
            response.StatusCode.Returns(statusCode);
            response.Content.Returns(content);
            content.ReadAsStreamAsync().Returns(Task.FromResult<Stream>(responseStream));
            client.GetAsync(uri, cancellationToken).Returns(Task.FromResult(response));

            // Act
            var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<StreamWrapper>(result);
        }

        /// <summary>
        /// Tests that GetStreamAsync handles edge case with minimum and maximum integer values in cancellation token.
        /// Input: CancellationToken with different timeout values
        /// Expected: Proper handling without exceptions for valid scenarios
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_ValidCancellationTokenTimeouts_HandlesCorrectly()
        {
            // Arrange
            var uri = new Uri("https://example.com");
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(int.MaxValue));
            var cancellationToken = cancellationTokenSource.Token;
            var client = Substitute.For<HttpClient>();
            var response = Substitute.For<HttpResponseMessage>();
            var content = Substitute.For<HttpContent>();
            var responseStream = new MemoryStream();

            response.IsSuccessStatusCode.Returns(true);
            response.Content.Returns(content);
            content.ReadAsStreamAsync().Returns(Task.FromResult<Stream>(responseStream));
            client.GetAsync(uri, cancellationToken).Returns(Task.FromResult(response));

            // Act
            var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<StreamWrapper>(result);
        }

        /// <summary>
        /// Tests that GetStreamAsync handles special URI schemes and edge cases.
        /// Input: Various URI schemes and formats
        /// Expected: Proper delegation to HttpClient without modification
        /// </summary>
        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://localhost:8080/path")]
        [InlineData("https://example.com/path/with/special-chars?query=value&other=123")]
        [InlineData("https://example.com:443/secure")]
        public async Task GetStreamAsync_VariousUriFormats_HandlesCorrectly(string uriString)
        {
            // Arrange
            var uri = new Uri(uriString);
            var cancellationToken = CancellationToken.None;
            var client = Substitute.For<HttpClient>();
            var response = Substitute.For<HttpResponseMessage>();
            var content = Substitute.For<HttpContent>();
            var responseStream = new MemoryStream();

            response.IsSuccessStatusCode.Returns(true);
            response.Content.Returns(content);
            content.ReadAsStreamAsync().Returns(Task.FromResult<Stream>(responseStream));
            client.GetAsync(uri, cancellationToken).Returns(Task.FromResult(response));

            // Act
            var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client);

            // Assert
            Assert.NotNull(result);
            await client.Received(1).GetAsync(uri, cancellationToken);
        }

        /// <summary>
        /// Tests that GetStreamAsync properly handles when response content reading fails.
        /// Input: HttpContent.ReadAsStreamAsync throws exception
        /// Expected: Exception propagates to caller
        /// </summary>
        [Fact]
        public async Task GetStreamAsync_ResponseContentReadFails_ThrowsException()
        {
            // Arrange
            var uri = new Uri("https://example.com");
            var cancellationToken = CancellationToken.None;
            var client = Substitute.For<HttpClient>();
            var response = Substitute.For<HttpResponseMessage>();
            var content = Substitute.For<HttpContent>();

            response.IsSuccessStatusCode.Returns(true);
            response.Content.Returns(content);
            content.ReadAsStreamAsync().Returns(Task.FromException<Stream>(new IOException("Content read failed")));
            client.GetAsync(uri, cancellationToken).Returns(Task.FromResult(response));

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(() =>
                StreamWrapper.GetStreamAsync(uri, cancellationToken, client));
        }

        /// <summary>
        /// Tests that GetStreamAsync handles boundary HTTP status codes correctly.
        /// Input: Boundary status codes (199, 200, 299, 300)
        /// Expected: Correct success/failure determination
        /// </summary>
        [Theory]
        [InlineData(199, false)] // Just below success range
        [InlineData(200, true)]  // Start of success range
        [InlineData(299, true)]  // End of success range
        [InlineData(300, false)] // Just above success range
        public async Task GetStreamAsync_BoundaryStatusCodes_HandlesCorrectly(int statusCodeValue, bool shouldSucceed)
        {
            // Arrange
            var uri = new Uri("https://example.com");
            var cancellationToken = CancellationToken.None;
            var client = Substitute.For<HttpClient>();
            var response = Substitute.For<HttpResponseMessage>();
            var content = Substitute.For<HttpContent>();
            var responseStream = new MemoryStream();

            response.IsSuccessStatusCode.Returns(shouldSucceed);
            response.StatusCode.Returns((HttpStatusCode)statusCodeValue);
            response.Content.Returns(content);
            content.ReadAsStreamAsync().Returns(Task.FromResult<Stream>(responseStream));
            client.GetAsync(uri, cancellationToken).Returns(Task.FromResult(response));

            // Act
            var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client);

            // Assert
            if (shouldSucceed)
            {
                Assert.NotNull(result);
                Assert.IsType<StreamWrapper>(result);
            }
            else
            {
                Assert.Null(result);
            }
        }

        /// <summary>
        /// Tests that the Position getter returns the position from the wrapped stream.
        /// </summary>
        /// <param name="expectedPosition">The position value to test</param>
        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(100L)]
        [InlineData(1024L)]
        [InlineData(long.MaxValue)]
        public void Position_Get_ReturnsWrappedStreamPosition(long expectedPosition)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.Position.Returns(expectedPosition);
            var streamWrapper = new StreamWrapper(mockStream);

            // Act
            var actualPosition = streamWrapper.Position;

            // Assert
            Assert.Equal(expectedPosition, actualPosition);
        }

        /// <summary>
        /// Tests that the Position setter sets the position on the wrapped stream.
        /// </summary>
        /// <param name="positionToSet">The position value to set</param>
        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(100L)]
        [InlineData(1024L)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        public void Position_Set_SetsWrappedStreamPosition(long positionToSet)
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var streamWrapper = new StreamWrapper(mockStream);

            // Act
            streamWrapper.Position = positionToSet;

            // Assert
            mockStream.Received(1).Position = positionToSet;
        }

        /// <summary>
        /// Tests that getting Position multiple times returns consistent values from the wrapped stream.
        /// </summary>
        [Fact]
        public void Position_GetMultipleTimes_ReturnsConsistentValue()
        {
            // Arrange
            const long expectedPosition = 42L;
            var mockStream = Substitute.For<Stream>();
            mockStream.Position.Returns(expectedPosition);
            var streamWrapper = new StreamWrapper(mockStream);

            // Act
            var position1 = streamWrapper.Position;
            var position2 = streamWrapper.Position;

            // Assert
            Assert.Equal(expectedPosition, position1);
            Assert.Equal(expectedPosition, position2);
            Assert.Equal(position1, position2);
        }

        /// <summary>
        /// Tests that Position property works correctly when wrapped stream position changes externally.
        /// </summary>
        [Fact]
        public void Position_Get_ReflectsExternalChangesToWrappedStream()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var streamWrapper = new StreamWrapper(mockStream);

            // First position value
            mockStream.Position.Returns(10L);
            var firstPosition = streamWrapper.Position;

            // Change the mock to return a different value
            mockStream.Position.Returns(20L);

            // Act
            var secondPosition = streamWrapper.Position;

            // Assert
            Assert.Equal(10L, firstPosition);
            Assert.Equal(20L, secondPosition);
        }

        /// <summary>
        /// Tests that Flush method delegates to the wrapped stream's Flush method.
        /// Verifies that the wrapped stream's Flush method is called exactly once.
        /// </summary>
        [Fact]
        public void Flush_ValidWrappedStream_CallsWrappedStreamFlush()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var streamWrapper = new StreamWrapper(mockStream);

            // Act
            streamWrapper.Flush();

            // Assert
            mockStream.Received(1).Flush();
        }

        /// <summary>
        /// Tests that Flush method propagates exceptions from the wrapped stream.
        /// Verifies that when the wrapped stream throws an exception during Flush, 
        /// the same exception is propagated to the caller.
        /// </summary>
        [Fact]
        public void Flush_WrappedStreamThrowsException_PropagatesException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var expectedException = new IOException("Test exception");
            mockStream.When(x => x.Flush()).Do(x => { throw expectedException; });
            var streamWrapper = new StreamWrapper(mockStream);

            // Act & Assert
            var actualException = Assert.Throws<IOException>(() => streamWrapper.Flush());
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that Flush method works correctly with a wrapped stream and additional disposable.
        /// Verifies that the Flush operation delegates correctly regardless of the additional disposable parameter.
        /// </summary>
        [Fact]
        public void Flush_WithAdditionalDisposable_CallsWrappedStreamFlush()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var mockDisposable = Substitute.For<IDisposable>();
            var streamWrapper = new StreamWrapper(mockStream, mockDisposable);

            // Act
            streamWrapper.Flush();

            // Assert
            mockStream.Received(1).Flush();
        }

        /// <summary>
        /// Tests that Read method properly delegates to the wrapped stream with valid parameters
        /// and returns the expected result.
        /// </summary>
        [Fact]
        public void Read_ValidParameters_ReturnsWrappedStreamResult()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var buffer = new byte[10];
            const int offset = 2;
            const int count = 5;
            const int expectedResult = 3;

            wrappedStream.Read(buffer, offset, count).Returns(expectedResult);
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act
            var result = streamWrapper.Read(buffer, offset, count);

            // Assert
            Assert.Equal(expectedResult, result);
            wrappedStream.Received(1).Read(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Read method properly delegates when reading zero bytes.
        /// </summary>
        [Fact]
        public void Read_ZeroCount_ReturnsWrappedStreamResult()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var buffer = new byte[10];
            const int offset = 0;
            const int count = 0;
            const int expectedResult = 0;

            wrappedStream.Read(buffer, offset, count).Returns(expectedResult);
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act
            var result = streamWrapper.Read(buffer, offset, count);

            // Assert
            Assert.Equal(expectedResult, result);
            wrappedStream.Received(1).Read(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Read method properly delegates when using an empty buffer.
        /// </summary>
        [Fact]
        public void Read_EmptyBuffer_ReturnsWrappedStreamResult()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var buffer = new byte[0];
            const int offset = 0;
            const int count = 0;
            const int expectedResult = 0;

            wrappedStream.Read(buffer, offset, count).Returns(expectedResult);
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act
            var result = streamWrapper.Read(buffer, offset, count);

            // Assert
            Assert.Equal(expectedResult, result);
            wrappedStream.Received(1).Read(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Read method properly delegates when reading at maximum buffer offset.
        /// </summary>
        [Fact]
        public void Read_MaximumOffset_ReturnsWrappedStreamResult()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var buffer = new byte[10];
            const int offset = 9;
            const int count = 1;
            const int expectedResult = 1;

            wrappedStream.Read(buffer, offset, count).Returns(expectedResult);
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act
            var result = streamWrapper.Read(buffer, offset, count);

            // Assert
            Assert.Equal(expectedResult, result);
            wrappedStream.Received(1).Read(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Read method properly propagates ArgumentNullException from wrapped stream
        /// when buffer is null.
        /// </summary>
        [Fact]
        public void Read_NullBuffer_ThrowsArgumentNullException()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            byte[] buffer = null;
            const int offset = 0;
            const int count = 0;

            wrappedStream.When(x => x.Read(buffer, offset, count))
                .Do(x => throw new ArgumentNullException("buffer"));
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => streamWrapper.Read(buffer, offset, count));
            Assert.Equal("buffer", exception.ParamName);
        }

        /// <summary>
        /// Tests that Read method properly propagates ArgumentOutOfRangeException from wrapped stream
        /// when offset is negative.
        /// </summary>
        [Fact]
        public void Read_NegativeOffset_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var buffer = new byte[10];
            const int offset = -1;
            const int count = 5;

            wrappedStream.When(x => x.Read(buffer, offset, count))
                .Do(x => throw new ArgumentOutOfRangeException("offset"));
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => streamWrapper.Read(buffer, offset, count));
            Assert.Equal("offset", exception.ParamName);
        }

        /// <summary>
        /// Tests that Read method properly propagates ArgumentOutOfRangeException from wrapped stream
        /// when count is negative.
        /// </summary>
        [Fact]
        public void Read_NegativeCount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var buffer = new byte[10];
            const int offset = 0;
            const int count = -1;

            wrappedStream.When(x => x.Read(buffer, offset, count))
                .Do(x => throw new ArgumentOutOfRangeException("count"));
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act & Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => streamWrapper.Read(buffer, offset, count));
            Assert.Equal("count", exception.ParamName);
        }

        /// <summary>
        /// Tests that Read method properly propagates ArgumentException from wrapped stream
        /// when offset plus count exceeds buffer length.
        /// </summary>
        [Fact]
        public void Read_OffsetPlusCountExceedsBufferLength_ThrowsArgumentException()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var buffer = new byte[10];
            const int offset = 8;
            const int count = 5; // offset + count = 13 > buffer.Length = 10

            wrappedStream.When(x => x.Read(buffer, offset, count))
                .Do(x => throw new ArgumentException("Offset and length were out of bounds for the array or count is greater than the number of elements from index to the end of the source collection."));
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => streamWrapper.Read(buffer, offset, count));
        }

        /// <summary>
        /// Tests that Read method with maximum integer values delegates properly to wrapped stream.
        /// </summary>
        [Fact]
        public void Read_MaximumValidValues_ReturnsWrappedStreamResult()
        {
            // Arrange
            var wrappedStream = Substitute.For<Stream>();
            var buffer = new byte[int.MaxValue];
            const int offset = 0;
            const int count = int.MaxValue;
            const int expectedResult = 1000;

            wrappedStream.Read(buffer, offset, count).Returns(expectedResult);
            var streamWrapper = new StreamWrapper(wrappedStream);

            // Act
            var result = streamWrapper.Read(buffer, offset, count);

            // Assert
            Assert.Equal(expectedResult, result);
            wrappedStream.Received(1).Read(buffer, offset, count);
        }

        /// <summary>
        /// Tests that the Seek method correctly delegates to the wrapped stream and returns the expected position
        /// for various offset and origin combinations.
        /// </summary>
        /// <param name="offset">The byte offset to seek to</param>
        /// <param name="origin">The reference point for the offset</param>
        /// <param name="expectedReturnValue">The expected return value from the wrapped stream</param>
        [Theory]
        [InlineData(0L, SeekOrigin.Begin, 0L)]
        [InlineData(100L, SeekOrigin.Begin, 100L)]
        [InlineData(-50L, SeekOrigin.Current, 50L)]
        [InlineData(0L, SeekOrigin.End, 1000L)]
        [InlineData(long.MaxValue, SeekOrigin.Begin, long.MaxValue)]
        [InlineData(long.MinValue, SeekOrigin.Current, 0L)]
        public void Seek_WithValidParameters_DelegatesToWrappedStreamAndReturnsCorrectValue(long offset, SeekOrigin origin, long expectedReturnValue)
        {
            // Arrange
            var mockWrappedStream = Substitute.For<Stream>();
            mockWrappedStream.Seek(offset, origin).Returns(expectedReturnValue);
            var streamWrapper = new StreamWrapper(mockWrappedStream);

            // Act
            var result = streamWrapper.Seek(offset, origin);

            // Assert
            Assert.Equal(expectedReturnValue, result);
            mockWrappedStream.Received(1).Seek(offset, origin);
        }

        /// <summary>
        /// Tests that the Seek method correctly handles all valid SeekOrigin enum values.
        /// </summary>
        /// <param name="origin">The SeekOrigin value to test</param>
        [Theory]
        [InlineData(SeekOrigin.Begin)]
        [InlineData(SeekOrigin.Current)]
        [InlineData(SeekOrigin.End)]
        public void Seek_WithAllValidSeekOriginValues_DelegatesToWrappedStream(SeekOrigin origin)
        {
            // Arrange
            var mockWrappedStream = Substitute.For<Stream>();
            var expectedPosition = 42L;
            mockWrappedStream.Seek(Arg.Any<long>(), origin).Returns(expectedPosition);
            var streamWrapper = new StreamWrapper(mockWrappedStream);
            var offset = 10L;

            // Act
            var result = streamWrapper.Seek(offset, origin);

            // Assert
            Assert.Equal(expectedPosition, result);
            mockWrappedStream.Received(1).Seek(offset, origin);
        }

        /// <summary>
        /// Tests that the Seek method handles invalid SeekOrigin enum values by delegating to the wrapped stream,
        /// allowing the wrapped stream to handle the invalid value appropriately.
        /// </summary>
        [Fact]
        public void Seek_WithInvalidSeekOriginValue_DelegatesToWrappedStream()
        {
            // Arrange
            var mockWrappedStream = Substitute.For<Stream>();
            var invalidOrigin = (SeekOrigin)999;
            var expectedPosition = 123L;
            mockWrappedStream.Seek(Arg.Any<long>(), invalidOrigin).Returns(expectedPosition);
            var streamWrapper = new StreamWrapper(mockWrappedStream);
            var offset = 5L;

            // Act
            var result = streamWrapper.Seek(offset, invalidOrigin);

            // Assert
            Assert.Equal(expectedPosition, result);
            mockWrappedStream.Received(1).Seek(offset, invalidOrigin);
        }

        /// <summary>
        /// Tests that the Seek method propagates exceptions thrown by the wrapped stream's Seek method.
        /// </summary>
        /// <param name="exceptionType">The type of exception to test</param>
        [Theory]
        [InlineData(typeof(IOException))]
        [InlineData(typeof(ArgumentException))]
        [InlineData(typeof(NotSupportedException))]
        [InlineData(typeof(ObjectDisposedException))]
        public void Seek_WhenWrappedStreamThrowsException_PropagatesException(Type exceptionType)
        {
            // Arrange
            var mockWrappedStream = Substitute.For<Stream>();
            var exception = (Exception)Activator.CreateInstance(exceptionType, "Test exception");
            mockWrappedStream.Seek(Arg.Any<long>(), Arg.Any<SeekOrigin>()).Throws(exception);
            var streamWrapper = new StreamWrapper(mockWrappedStream);

            // Act & Assert
            var thrownException = Assert.Throws(exceptionType, () => streamWrapper.Seek(100L, SeekOrigin.Begin));
            Assert.Equal("Test exception", thrownException.Message);
            mockWrappedStream.Received(1).Seek(100L, SeekOrigin.Begin);
        }

        /// <summary>
        /// Tests that the Seek method correctly handles boundary values for the offset parameter.
        /// </summary>
        /// <param name="offset">The boundary offset value to test</param>
        [Theory]
        [InlineData(long.MinValue)]
        [InlineData(long.MaxValue)]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(-1L)]
        public void Seek_WithBoundaryOffsetValues_DelegatesToWrappedStream(long offset)
        {
            // Arrange
            var mockWrappedStream = Substitute.For<Stream>();
            var expectedPosition = Math.Abs(offset / 2);
            mockWrappedStream.Seek(offset, Arg.Any<SeekOrigin>()).Returns(expectedPosition);
            var streamWrapper = new StreamWrapper(mockWrappedStream);

            // Act
            var result = streamWrapper.Seek(offset, SeekOrigin.Begin);

            // Assert
            Assert.Equal(expectedPosition, result);
            mockWrappedStream.Received(1).Seek(offset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Tests that the Seek method properly handles the case where the wrapped stream returns negative positions,
        /// ensuring the return value is correctly propagated.
        /// </summary>
        [Fact]
        public void Seek_WhenWrappedStreamReturnsNegativePosition_ReturnsCorrectValue()
        {
            // Arrange
            var mockWrappedStream = Substitute.For<Stream>();
            var negativePosition = -100L;
            mockWrappedStream.Seek(Arg.Any<long>(), Arg.Any<SeekOrigin>()).Returns(negativePosition);
            var streamWrapper = new StreamWrapper(mockWrappedStream);

            // Act
            var result = streamWrapper.Seek(50L, SeekOrigin.Current);

            // Assert
            Assert.Equal(negativePosition, result);
            mockWrappedStream.Received(1).Seek(50L, SeekOrigin.Current);
        }

        /// <summary>
        /// Tests that SetLength forwards the call to the wrapped stream with a positive value.
        /// Input: positive long value
        /// Expected: wrapped stream's SetLength is called with the same value
        /// </summary>
        [Fact]
        public void SetLength_WithPositiveValue_ForwardsToWrappedStream()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var streamWrapper = new StreamWrapper(mockStream);
            long testValue = 1024L;

            // Act
            streamWrapper.SetLength(testValue);

            // Assert
            mockStream.Received(1).SetLength(testValue);
        }

        /// <summary>
        /// Tests that SetLength forwards the call to the wrapped stream with zero value.
        /// Input: zero
        /// Expected: wrapped stream's SetLength is called with zero
        /// </summary>
        [Fact]
        public void SetLength_WithZeroValue_ForwardsToWrappedStream()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var streamWrapper = new StreamWrapper(mockStream);
            long testValue = 0L;

            // Act
            streamWrapper.SetLength(testValue);

            // Assert
            mockStream.Received(1).SetLength(testValue);
        }

        /// <summary>
        /// Tests that SetLength forwards the call to the wrapped stream with negative value.
        /// Input: negative long value
        /// Expected: wrapped stream's SetLength is called with the same negative value
        /// </summary>
        [Fact]
        public void SetLength_WithNegativeValue_ForwardsToWrappedStream()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var streamWrapper = new StreamWrapper(mockStream);
            long testValue = -100L;

            // Act
            streamWrapper.SetLength(testValue);

            // Assert
            mockStream.Received(1).SetLength(testValue);
        }

        /// <summary>
        /// Tests that SetLength forwards the call to the wrapped stream with maximum long value.
        /// Input: long.MaxValue
        /// Expected: wrapped stream's SetLength is called with long.MaxValue
        /// </summary>
        [Fact]
        public void SetLength_WithMaxValue_ForwardsToWrappedStream()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var streamWrapper = new StreamWrapper(mockStream);
            long testValue = long.MaxValue;

            // Act
            streamWrapper.SetLength(testValue);

            // Assert
            mockStream.Received(1).SetLength(testValue);
        }

        /// <summary>
        /// Tests that SetLength forwards the call to the wrapped stream with minimum long value.
        /// Input: long.MinValue
        /// Expected: wrapped stream's SetLength is called with long.MinValue
        /// </summary>
        [Fact]
        public void SetLength_WithMinValue_ForwardsToWrappedStream()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var streamWrapper = new StreamWrapper(mockStream);
            long testValue = long.MinValue;

            // Act
            streamWrapper.SetLength(testValue);

            // Assert
            mockStream.Received(1).SetLength(testValue);
        }

        /// <summary>
        /// Tests that SetLength propagates exceptions thrown by the wrapped stream.
        /// Input: any long value when wrapped stream throws IOException
        /// Expected: IOException is propagated to caller
        /// </summary>
        [Fact]
        public void SetLength_WhenWrappedStreamThrowsIOException_PropagatesException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var expectedException = new IOException("Test exception");
            mockStream.When(x => x.SetLength(Arg.Any<long>())).Do(x => throw expectedException);
            var streamWrapper = new StreamWrapper(mockStream);
            long testValue = 1024L;

            // Act & Assert
            var actualException = Assert.Throws<IOException>(() => streamWrapper.SetLength(testValue));
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that SetLength propagates NotSupportedException thrown by the wrapped stream.
        /// Input: any long value when wrapped stream throws NotSupportedException
        /// Expected: NotSupportedException is propagated to caller
        /// </summary>
        [Fact]
        public void SetLength_WhenWrappedStreamThrowsNotSupportedException_PropagatesException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var expectedException = new NotSupportedException("SetLength not supported");
            mockStream.When(x => x.SetLength(Arg.Any<long>())).Do(x => throw expectedException);
            var streamWrapper = new StreamWrapper(mockStream);
            long testValue = 500L;

            // Act & Assert
            var actualException = Assert.Throws<NotSupportedException>(() => streamWrapper.SetLength(testValue));
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that SetLength propagates ArgumentException thrown by the wrapped stream.
        /// Input: any long value when wrapped stream throws ArgumentException
        /// Expected: ArgumentException is propagated to caller
        /// </summary>
        [Fact]
        public void SetLength_WhenWrappedStreamThrowsArgumentException_PropagatesException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var expectedException = new ArgumentException("Invalid length", "value");
            mockStream.When(x => x.SetLength(Arg.Any<long>())).Do(x => throw expectedException);
            var streamWrapper = new StreamWrapper(mockStream);
            long testValue = -1L;

            // Act & Assert
            var actualException = Assert.Throws<ArgumentException>(() => streamWrapper.SetLength(testValue));
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that Write method properly delegates to the wrapped stream with valid parameters.
        /// Input: Valid byte array, offset, and count.
        /// Expected: Wrapped stream's Write method is called with the same parameters.
        /// </summary>
        [Fact]
        public void Write_ValidParameters_CallsWrappedStreamWrite()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var wrapper = new StreamWrapper(mockStream);
            var buffer = new byte[] { 1, 2, 3, 4, 5 };
            var offset = 1;
            var count = 3;

            // Act
            wrapper.Write(buffer, offset, count);

            // Assert
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Write method handles null buffer by delegating to wrapped stream.
        /// Input: Null buffer.
        /// Expected: Wrapped stream's Write method is called with null buffer, any exception from wrapped stream is propagated.
        /// </summary>
        [Fact]
        public void Write_NullBuffer_CallsWrappedStreamWrite()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var wrapper = new StreamWrapper(mockStream);
            byte[] buffer = null;
            var offset = 0;
            var count = 0;

            // Act
            wrapper.Write(buffer, offset, count);

            // Assert
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Write method handles empty buffer by delegating to wrapped stream.
        /// Input: Empty byte array.
        /// Expected: Wrapped stream's Write method is called with empty buffer.
        /// </summary>
        [Fact]
        public void Write_EmptyBuffer_CallsWrappedStreamWrite()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var wrapper = new StreamWrapper(mockStream);
            var buffer = new byte[0];
            var offset = 0;
            var count = 0;

            // Act
            wrapper.Write(buffer, offset, count);

            // Assert
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Write method handles negative offset by delegating to wrapped stream.
        /// Input: Valid buffer with negative offset.
        /// Expected: Wrapped stream's Write method is called with negative offset, any exception from wrapped stream is propagated.
        /// </summary>
        [Fact]
        public void Write_NegativeOffset_CallsWrappedStreamWrite()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var wrapper = new StreamWrapper(mockStream);
            var buffer = new byte[] { 1, 2, 3 };
            var offset = -1;
            var count = 1;

            // Act
            wrapper.Write(buffer, offset, count);

            // Assert
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Write method handles negative count by delegating to wrapped stream.
        /// Input: Valid buffer with negative count.
        /// Expected: Wrapped stream's Write method is called with negative count, any exception from wrapped stream is propagated.
        /// </summary>
        [Fact]
        public void Write_NegativeCount_CallsWrappedStreamWrite()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var wrapper = new StreamWrapper(mockStream);
            var buffer = new byte[] { 1, 2, 3 };
            var offset = 0;
            var count = -1;

            // Act
            wrapper.Write(buffer, offset, count);

            // Assert
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Write method handles zero count by delegating to wrapped stream.
        /// Input: Valid buffer with zero count.
        /// Expected: Wrapped stream's Write method is called with zero count.
        /// </summary>
        [Fact]
        public void Write_ZeroCount_CallsWrappedStreamWrite()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var wrapper = new StreamWrapper(mockStream);
            var buffer = new byte[] { 1, 2, 3 };
            var offset = 0;
            var count = 0;

            // Act
            wrapper.Write(buffer, offset, count);

            // Assert
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Write method handles offset beyond buffer bounds by delegating to wrapped stream.
        /// Input: Valid buffer with offset beyond buffer length.
        /// Expected: Wrapped stream's Write method is called with out-of-bounds offset, any exception from wrapped stream is propagated.
        /// </summary>
        [Fact]
        public void Write_OffsetBeyondBuffer_CallsWrappedStreamWrite()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var wrapper = new StreamWrapper(mockStream);
            var buffer = new byte[] { 1, 2, 3 };
            var offset = 5;
            var count = 1;

            // Act
            wrapper.Write(buffer, offset, count);

            // Assert
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Write method handles count larger than available buffer space by delegating to wrapped stream.
        /// Input: Valid buffer with count exceeding available space from offset.
        /// Expected: Wrapped stream's Write method is called with oversized count, any exception from wrapped stream is propagated.
        /// </summary>
        [Fact]
        public void Write_CountLargerThanAvailableSpace_CallsWrappedStreamWrite()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var wrapper = new StreamWrapper(mockStream);
            var buffer = new byte[] { 1, 2, 3 };
            var offset = 2;
            var count = 5;

            // Act
            wrapper.Write(buffer, offset, count);

            // Assert
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Write method propagates exceptions thrown by the wrapped stream.
        /// Input: Valid parameters but wrapped stream throws exception.
        /// Expected: Exception from wrapped stream is propagated to caller.
        /// </summary>
        [Fact]
        public void Write_WrappedStreamThrowsException_PropagatesException()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var expectedException = new InvalidOperationException("Test exception");
            mockStream.When(s => s.Write(Arg.Any<byte[]>(), Arg.Any<int>(), Arg.Any<int>()))
                      .Do(x => throw expectedException);

            var wrapper = new StreamWrapper(mockStream);
            var buffer = new byte[] { 1, 2, 3 };
            var offset = 0;
            var count = 3;

            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() => wrapper.Write(buffer, offset, count));
            Assert.Same(expectedException, actualException);
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that Write method handles maximum integer values for offset and count.
        /// Input: Valid buffer with int.MaxValue for offset and count.
        /// Expected: Wrapped stream's Write method is called with maximum values, any exception from wrapped stream is propagated.
        /// </summary>
        [Fact]
        public void Write_MaxIntegerValues_CallsWrappedStreamWrite()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            var wrapper = new StreamWrapper(mockStream);
            var buffer = new byte[] { 1, 2, 3 };
            var offset = int.MaxValue;
            var count = int.MaxValue;

            // Act
            wrapper.Write(buffer, offset, count);

            // Assert
            mockStream.Received(1).Write(buffer, offset, count);
        }

        /// <summary>
        /// Tests that CanSeek property correctly delegates to the wrapped stream's CanSeek property.
        /// Verifies both true and false return values to ensure proper delegation behavior.
        /// </summary>
        /// <param name="canSeek">The value that the wrapped stream's CanSeek property should return</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CanSeek_WhenWrappedStreamHasCanSeekValue_ReturnsWrappedStreamCanSeek(bool canSeek)
        {
            // Arrange
            var mockWrappedStream = Substitute.For<Stream>();
            mockWrappedStream.CanSeek.Returns(canSeek);
            var streamWrapper = new StreamWrapper(mockWrappedStream);

            // Act
            var result = streamWrapper.CanSeek;

            // Assert
            Assert.Equal(canSeek, result);
        }

        /// <summary>
        /// Tests that CanSeek property accesses the wrapped stream's CanSeek property exactly once.
        /// Ensures that the property delegation doesn't involve caching or multiple calls.
        /// </summary>
        [Fact]
        public void CanSeek_WhenAccessed_AccessesWrappedStreamCanSeekOnce()
        {
            // Arrange
            var mockWrappedStream = Substitute.For<Stream>();
            mockWrappedStream.CanSeek.Returns(true);
            var streamWrapper = new StreamWrapper(mockWrappedStream);

            // Act
            var result = streamWrapper.CanSeek;

            // Assert
            Assert.True(result);
            var canSeekProperty = mockWrappedStream.Received(1).CanSeek;
        }
    }
}