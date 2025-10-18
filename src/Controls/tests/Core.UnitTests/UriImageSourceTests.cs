#nullable disable

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class UriImageSourceTests : BaseTestFixture
    {

        public UriImageSourceTests()
        {

            networkcalls = 0;
        }

        static Random rnd = new Random();
        static int networkcalls = 0;

        static async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
        {
            await Task.Delay(rnd.Next(30, 2000));
            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();
            networkcalls++;
            return typeof(UriImageSourceTests).Assembly.GetManifestResourceStream(uri.LocalPath.Substring(1));
        }

        [Fact(Skip = "LoadImageFromStream")]
        public void LoadImageFromStream()
        {
            IStreamImageSource loader = new UriImageSource
            {
                Uri = new Uri("http://foo.com/Images/crimson.jpg"),
            };
            Stream s0 = loader.GetStreamAsync().Result;

            Assert.Equal(79109, s0.Length);
        }

        [Fact(Skip = "SecondCallLoadFromCache")]
        public void SecondCallLoadFromCache()
        {
            IStreamImageSource loader = new UriImageSource
            {
                Uri = new Uri("http://foo.com/Images/crimson.jpg"),
            };
            Assert.Equal(0, networkcalls);

            using (var s0 = loader.GetStreamAsync().Result)
            {
                Assert.Equal(79109, s0.Length);
                Assert.Equal(1, networkcalls);
            }

            using (var s1 = loader.GetStreamAsync().Result)
            {
                Assert.Equal(79109, s1.Length);
                Assert.Equal(1, networkcalls);
            }
        }

        [Fact(Skip = "DoNotKeepFailedRetrieveInCache")]
        public void DoNotKeepFailedRetrieveInCache()
        {
            IStreamImageSource loader = new UriImageSource
            {
                Uri = new Uri("http://foo.com/missing.png"),
            };
            Assert.Equal(0, networkcalls);

            var s0 = loader.GetStreamAsync().Result;
            Assert.Null(s0);
            Assert.Equal(1, networkcalls);

            var s1 = loader.GetStreamAsync().Result;
            Assert.Null(s1);
            Assert.Equal(2, networkcalls);
        }

        [Fact(Skip = "ConcurrentCallsOnSameUriAreQueued")]
        public void ConcurrentCallsOnSameUriAreQueued()
        {
            IStreamImageSource loader = new UriImageSource
            {
                Uri = new Uri("http://foo.com/Images/crimson.jpg"),
            };
            Assert.Equal(0, networkcalls);

            var t0 = loader.GetStreamAsync();
            var t1 = loader.GetStreamAsync();

            //var s0 = t0.Result;
            using (var s1 = t1.Result)
            {
                Assert.Equal(1, networkcalls);
                Assert.Equal(79109, s1.Length);
            }
        }

        [Fact]
        public void NullUriDoesNotCrash()
        {
            var loader = new UriImageSource();
            loader.Uri = null;
        }

        [Fact]
        public void UrlHashKeyAreTheSame()
        {
            var urlHash1 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd");
            var urlHash2 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd");
            Assert.True(urlHash1 == urlHash2);
        }

        [Fact]
        public void UrlHashKeyAreNotTheSame()
        {
            var urlHash1 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd");
            var urlHash2 = Crc64.ComputeHashString("http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasda");
            Assert.True(urlHash1 != urlHash2);
        }


        /// <summary>
        /// Tests that the CachingEnabled property returns the default value of true when not explicitly set.
        /// This verifies the getter functionality and the default value defined in the BindableProperty.
        /// </summary>
        [Fact]
        public void CachingEnabled_DefaultValue_ReturnsTrue()
        {
            // Arrange
            var uriImageSource = new UriImageSource();

            // Act
            bool result = uriImageSource.CachingEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the CachingEnabled property correctly stores and retrieves a false value.
        /// This verifies both setter and getter functionality with a false boolean value.
        /// </summary>
        [Fact]
        public void CachingEnabled_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var uriImageSource = new UriImageSource();

            // Act
            uriImageSource.CachingEnabled = false;
            bool result = uriImageSource.CachingEnabled;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the CachingEnabled property correctly stores and retrieves a true value.
        /// This verifies both setter and getter functionality with a true boolean value.
        /// </summary>
        [Fact]
        public void CachingEnabled_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var uriImageSource = new UriImageSource();

            // Act
            uriImageSource.CachingEnabled = true;
            bool result = uriImageSource.CachingEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the CachingEnabled property can be toggled multiple times and maintains correct values.
        /// This verifies the property's consistency across multiple set/get operations.
        /// </summary>
        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, false)]
        [InlineData(true, true, true)]
        [InlineData(false, false, false)]
        public void CachingEnabled_MultipleOperations_MaintainsCorrectValues(bool firstValue, bool secondValue, bool thirdValue)
        {
            // Arrange
            var uriImageSource = new UriImageSource();

            // Act & Assert
            uriImageSource.CachingEnabled = firstValue;
            Assert.Equal(firstValue, uriImageSource.CachingEnabled);

            uriImageSource.CachingEnabled = secondValue;
            Assert.Equal(secondValue, uriImageSource.CachingEnabled);

            uriImageSource.CachingEnabled = thirdValue;
            Assert.Equal(thirdValue, uriImageSource.CachingEnabled);
        }

        /// <summary>
        /// Tests the ToString method returns the correct string representation when Uri is null.
        /// Input: UriImageSource with null Uri property.
        /// Expected: Returns "Uri: " (empty string after colon and space).
        /// </summary>
        [Fact]
        public void ToString_WhenUriIsNull_ReturnsUriWithEmptyValue()
        {
            // Arrange
            var uriImageSource = new UriImageSource();

            // Act
            var result = uriImageSource.ToString();

            // Assert
            Assert.Equal("Uri: ", result);
        }

        /// <summary>
        /// Tests the ToString method returns the correct string representation for various valid URIs.
        /// Input: UriImageSource with different valid absolute URI values.
        /// Expected: Returns "Uri: {actualUriValue}" format for each URI.
        /// </summary>
        [Theory]
        [InlineData("http://example.com", "Uri: http://example.com/")]
        [InlineData("https://example.com/image.png", "Uri: https://example.com/image.png")]
        [InlineData("https://example.com/path/to/image.jpg?param=value", "Uri: https://example.com/path/to/image.jpg?param=value")]
        [InlineData("file:///Users/test/image.png", "Uri: file:///Users/test/image.png")]
        [InlineData("ftp://ftp.example.com/image.gif", "Uri: ftp://ftp.example.com/image.gif")]
        public void ToString_WithValidUri_ReturnsCorrectStringRepresentation(string uriString, string expectedResult)
        {
            // Arrange
            var uriImageSource = new UriImageSource
            {
                Uri = new Uri(uriString)
            };

            // Act
            var result = uriImageSource.ToString();

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that IsEmpty returns true when Uri is null.
        /// This validates that an image source with no URI is considered empty.
        /// </summary>
        [Fact]
        public void IsEmpty_WhenUriIsNull_ReturnsTrue()
        {
            // Arrange
            var uriImageSource = new UriImageSource();

            // Act
            var result = uriImageSource.IsEmpty;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsEmpty returns false when Uri is set to a valid absolute URI.
        /// This validates that an image source with a URI is not considered empty.
        /// </summary>
        [Theory]
        [InlineData("https://example.com/image.png")]
        [InlineData("http://test.com/photo.jpg")]
        [InlineData("file:///path/to/image.gif")]
        [InlineData("ftp://server.com/image.bmp")]
        public void IsEmpty_WhenUriIsNotNull_ReturnsFalse(string uriString)
        {
            // Arrange
            var uriImageSource = new UriImageSource
            {
                Uri = new Uri(uriString)
            };

            // Act
            var result = uriImageSource.IsEmpty;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsEmpty returns true after setting Uri to null.
        /// This validates that clearing the URI makes the image source empty.
        /// </summary>
        [Fact]
        public void IsEmpty_WhenUriIsSetToNull_ReturnsTrue()
        {
            // Arrange
            var uriImageSource = new UriImageSource
            {
                Uri = new Uri("https://example.com/image.png")
            };

            // Act
            uriImageSource.Uri = null;
            var result = uriImageSource.IsEmpty;

            // Assert
            Assert.True(result);
        }
    }

    /// <summary>
    /// Unit tests for UriImageSource CacheValidity property.
    /// </summary>
    public partial class UriImageSourceCacheValidityTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that CacheValidity getter returns the default value of 1 day when no value has been explicitly set.
        /// Input: Newly created UriImageSource instance.
        /// Expected: CacheValidity should return TimeSpan.FromDays(1).
        /// </summary>
        [Fact]
        public void CacheValidity_DefaultValue_ReturnsOneDay()
        {
            // Arrange
            var uriImageSource = new UriImageSource();

            // Act
            var result = uriImageSource.CacheValidity;

            // Assert
            Assert.Equal(TimeSpan.FromDays(1), result);
        }

        /// <summary>
        /// Tests that CacheValidity getter returns the correct value after setting it to various TimeSpan values.
        /// Input: Various TimeSpan values including edge cases and common scenarios.
        /// Expected: CacheValidity getter should return the exact value that was set.
        /// </summary>
        [Theory]
        [InlineData(0, 0, 0, 0, 0)] // TimeSpan.Zero
        [InlineData(0, 1, 0, 0, 0)] // 1 hour
        [InlineData(1, 0, 0, 0, 0)] // 1 day
        [InlineData(7, 0, 0, 0, 0)] // 1 week
        [InlineData(30, 0, 0, 0, 0)] // 30 days
        [InlineData(0, 0, 30, 0, 0)] // 30 minutes
        [InlineData(0, 0, 0, 30, 0)] // 30 seconds
        [InlineData(0, 0, 0, 0, 500)] // 500 milliseconds
        public void CacheValidity_SetAndGet_ReturnsCorrectValue(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            // Arrange
            var uriImageSource = new UriImageSource();
            var expectedTimeSpan = new TimeSpan(days, hours, minutes, seconds, milliseconds);

            // Act
            uriImageSource.CacheValidity = expectedTimeSpan;
            var result = uriImageSource.CacheValidity;

            // Assert
            Assert.Equal(expectedTimeSpan, result);
        }

        /// <summary>
        /// Tests that CacheValidity can handle extreme TimeSpan values including maximum and minimum values.
        /// Input: TimeSpan.MaxValue and TimeSpan.MinValue.
        /// Expected: CacheValidity getter should return the exact extreme values that were set.
        /// </summary>
        [Theory]
        [InlineData(long.MaxValue)] // TimeSpan.MaxValue ticks
        [InlineData(long.MinValue)] // TimeSpan.MinValue ticks
        [InlineData(1)] // TimeSpan.FromTicks(1)
        [InlineData(-1)] // Negative single tick
        public void CacheValidity_ExtremeValues_ReturnsCorrectValue(long ticks)
        {
            // Arrange
            var uriImageSource = new UriImageSource();
            var expectedTimeSpan = new TimeSpan(ticks);

            // Act
            uriImageSource.CacheValidity = expectedTimeSpan;
            var result = uriImageSource.CacheValidity;

            // Assert
            Assert.Equal(expectedTimeSpan, result);
        }

        /// <summary>
        /// Tests that CacheValidity can handle negative TimeSpan values which may represent invalid cache durations.
        /// Input: Various negative TimeSpan values.
        /// Expected: CacheValidity getter should return the exact negative values that were set.
        /// </summary>
        [Theory]
        [InlineData(-1, 0, 0, 0, 0)] // -1 day
        [InlineData(0, -1, 0, 0, 0)] // -1 hour
        [InlineData(0, 0, -30, 0, 0)] // -30 minutes
        [InlineData(-365, 0, 0, 0, 0)] // -1 year
        public void CacheValidity_NegativeValues_ReturnsCorrectValue(int days, int hours, int minutes, int seconds, int milliseconds)
        {
            // Arrange
            var uriImageSource = new UriImageSource();
            var expectedTimeSpan = new TimeSpan(days, hours, minutes, seconds, milliseconds);

            // Act
            uriImageSource.CacheValidity = expectedTimeSpan;
            var result = uriImageSource.CacheValidity;

            // Assert
            Assert.Equal(expectedTimeSpan, result);
        }

        /// <summary>
        /// Tests that CacheValidity maintains precision for very small TimeSpan values.
        /// Input: Very small positive TimeSpan values with precise tick counts.
        /// Expected: CacheValidity getter should return the exact precise values that were set.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(999)]
        [InlineData(10000)] // 1 millisecond in ticks
        public void CacheValidity_SmallPreciseValues_MaintainsPrecision(long ticks)
        {
            // Arrange
            var uriImageSource = new UriImageSource();
            var expectedTimeSpan = TimeSpan.FromTicks(ticks);

            // Act
            uriImageSource.CacheValidity = expectedTimeSpan;
            var result = uriImageSource.CacheValidity;

            // Assert
            Assert.Equal(expectedTimeSpan, result);
            Assert.Equal(ticks, result.Ticks);
        }

        /// <summary>
        /// Tests that multiple CacheValidity set/get operations work correctly and don't interfere with each other.
        /// Input: Multiple different TimeSpan values set sequentially.
        /// Expected: Each CacheValidity getter call should return the most recently set value.
        /// </summary>
        [Fact]
        public void CacheValidity_MultipleSetOperations_ReturnsLatestValue()
        {
            // Arrange
            var uriImageSource = new UriImageSource();
            var firstValue = TimeSpan.FromHours(2);
            var secondValue = TimeSpan.FromDays(5);
            var thirdValue = TimeSpan.FromMinutes(45);

            // Act & Assert
            uriImageSource.CacheValidity = firstValue;
            Assert.Equal(firstValue, uriImageSource.CacheValidity);

            uriImageSource.CacheValidity = secondValue;
            Assert.Equal(secondValue, uriImageSource.CacheValidity);

            uriImageSource.CacheValidity = thirdValue;
            Assert.Equal(thirdValue, uriImageSource.CacheValidity);
        }
    }
}