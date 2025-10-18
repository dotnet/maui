#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class ImageSourceTests : BaseTestFixture
    {
        [Fact]
        public void TestConstructors()
        {
            var filesource = new FileImageSource { File = "File.png" };
            Assert.Equal("File.png", filesource.File);

            Func<CancellationToken, Task<Stream>> stream = token => new Task<Stream>(() => new FileStream("Foo", System.IO.FileMode.Open), token);
            var streamsource = new StreamImageSource { Stream = stream };
            Assert.Equal(stream, streamsource.Stream);
        }

        [Fact]
        public void TestHelpers()
        {
            var imagesource = ImageSource.FromFile("File.png");
            Assert.IsType<FileImageSource>(imagesource);
            Assert.Equal("File.png", ((FileImageSource)imagesource).File);

            Func<Stream> stream = () => new System.IO.FileStream("Foo", System.IO.FileMode.Open);
            var streamsource = ImageSource.FromStream(stream);
            Assert.IsType<StreamImageSource>(streamsource);

            var urisource = ImageSource.FromUri(new Uri("http://xamarin.com/img.png"));
            Assert.IsType<UriImageSource>(urisource);
            Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(urisource)).Uri.AbsoluteUri);
        }

        [Fact]
        public void TestImplicitFileConversion()
        {
            var image = new Image { Source = "File.png" };
            Assert.True(image.Source != null);
            Assert.IsType<FileImageSource>(image.Source);
            Assert.Equal("File.png", ((FileImageSource)(image.Source)).File);
        }

        [Fact]
        public void TestImplicitStringConversionWhenNull()
        {
            string s = null;
            var sut = (ImageSource)s;
            Assert.IsType<FileImageSource>(sut);
            Assert.Null(((FileImageSource)sut).File);
        }

        [Fact]
        public void TestImplicitUriConversion()
        {
            var image = new Image { Source = new Uri("http://xamarin.com/img.png") };
            Assert.True(image.Source != null);
            Assert.IsType<UriImageSource>(image.Source);
            Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
        }

        [Fact]
        public void TestImplicitStringUriConversion()
        {
            var image = new Image { Source = "http://xamarin.com/img.png" };
            Assert.True(image.Source != null);
            Assert.IsType<UriImageSource>(image.Source);
            Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
        }

        [Fact]
        public void TestImplicitUriConversionWhenNull()
        {
            Uri u = null;
            var sut = (ImageSource)u;
            Assert.Null(sut);
        }

        [Fact]
        public void TestSetStringValue()
        {
            var image = new Image();
            image.SetValue(Image.SourceProperty, "foo.png");
            Assert.NotNull(image.Source);
            Assert.IsType<FileImageSource>(image.Source);
            Assert.Equal("foo.png", ((FileImageSource)(image.Source)).File);
        }

        [Fact]
        public void TextBindToStringValue()
        {
            var image = new Image();
            image.SetBinding(Image.SourceProperty, ".");
            Assert.Null(image.Source);
            image.BindingContext = "foo.png";
            Assert.NotNull(image.Source);
            Assert.IsType<FileImageSource>(image.Source);
            Assert.Equal("foo.png", ((FileImageSource)(image.Source)).File);
        }

        [Fact]
        public void TextBindToStringUriValue()
        {
            var image = new Image();
            image.SetBinding(Image.SourceProperty, ".");
            Assert.Null(image.Source);
            image.BindingContext = "http://xamarin.com/img.png";
            Assert.NotNull(image.Source);
            Assert.IsType<UriImageSource>(image.Source);
            Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
        }

        [Fact]
        public void TextBindToUriValue()
        {
            var image = new Image();
            image.SetBinding(Image.SourceProperty, ".");
            Assert.Null(image.Source);
            image.BindingContext = new Uri("http://xamarin.com/img.png");
            Assert.NotNull(image.Source);
            Assert.IsType<UriImageSource>(image.Source);
            Assert.Equal("http://xamarin.com/img.png", ((UriImageSource)(image.Source)).Uri.AbsoluteUri);
        }

        class MockImageSource : ImageSource
        {
        }

        [Fact]
        public void TestBindingContextPropagation()
        {
            var context = new object();
            var image = new Image();
            image.BindingContext = context;
            var source = new MockImageSource();
            image.Source = source;
            Assert.Same(context, source.BindingContext);

            image = new Image();
            source = new MockImageSource();
            image.Source = source;
            image.BindingContext = context;
            Assert.Same(context, source.BindingContext);
        }

        [Fact]
        public void ImplicitCastOnAbsolutePathsShouldCreateAFileImageSource()
        {
            var path = "/private/var/mobile/Containers/Data/Application/B1E5AB19-F815-4B4A-AB97-BD4571D53743/Documents/temp/IMG_20140603_150614_preview.jpg";
            var image = new Image { Source = path };
            Assert.IsType<FileImageSource>(image.Source);
        }

        [Fact]
        public async Task CancelCompletes()
        {
            var imageSource = new StreamImageSource
            {
                Stream = _ => Task.FromResult<Stream>(new MemoryStream())
            };
            await ((IStreamImageSource)imageSource).GetStreamAsync();
            await imageSource.Cancel(); // This should complete!
        }

        [Fact]
        public void SettingNewImageeSourceClearsParentOnOldImageSource()
        {
            var image = new Image { Source = "File.png" };
            var imageSource = image.Source;
            Assert.Equal(image, imageSource.Parent);
            image.Source = "File2.png";
            Assert.Null(imageSource.Parent);
            Assert.Equal(image, image.Source.Parent);
        }

        /// <summary>
        /// Tests that the default implementation of IsEmpty property returns false.
        /// Verifies that the base ImageSource class considers itself non-empty by default.
        /// </summary>
        [Fact]
        public void IsEmpty_DefaultImplementation_ReturnsFalse()
        {
            // Arrange
            var imageSource = new TestImageSource();

            // Act
            var result = imageSource.IsEmpty;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that the IsEmpty property can be overridden in derived classes.
        /// Verifies the virtual behavior by testing both true and false return values.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsEmpty_OverriddenImplementation_ReturnsExpectedValue(bool expectedValue)
        {
            // Arrange
            var imageSource = new TestImageSourceWithOverriddenIsEmpty(expectedValue);

            // Act
            var result = imageSource.IsEmpty;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Test implementation of ImageSource for testing the IsEmpty property.
        /// Uses the default implementation which always returns false.
        /// </summary>
        class TestImageSource : ImageSource
        {
        }

        /// <summary>
        /// Test implementation of ImageSource that overrides the IsEmpty property.
        /// Allows testing the virtual behavior with custom return values.
        /// </summary>
        class TestImageSourceWithOverriddenIsEmpty : ImageSource
        {
            private readonly bool _isEmpty;

            public TestImageSourceWithOverriddenIsEmpty(bool isEmpty)
            {
                _isEmpty = isEmpty;
            }

            public override bool IsEmpty => _isEmpty;
        }

        /// <summary>
        /// Tests that IsNullOrEmpty returns true when the imageSource parameter is null.
        /// </summary>
        [Fact]
        public void IsNullOrEmpty_WithNullImageSource_ReturnsTrue()
        {
            // Arrange
            ImageSource imageSource = null;

            // Act
            bool result = ImageSource.IsNullOrEmpty(imageSource);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsNullOrEmpty returns true when the imageSource is not null but IsEmpty returns true.
        /// </summary>
        [Fact]
        public void IsNullOrEmpty_WithEmptyImageSource_ReturnsTrue()
        {
            // Arrange
            var imageSource = Substitute.For<ImageSource>();
            imageSource.IsEmpty.Returns(true);

            // Act
            bool result = ImageSource.IsNullOrEmpty(imageSource);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsNullOrEmpty returns false when the imageSource is not null and IsEmpty returns false.
        /// </summary>
        [Fact]
        public void IsNullOrEmpty_WithNonEmptyImageSource_ReturnsFalse()
        {
            // Arrange
            var imageSource = Substitute.For<ImageSource>();
            imageSource.IsEmpty.Returns(false);

            // Act
            bool result = ImageSource.IsNullOrEmpty(imageSource);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Test implementation of ImageSource that exposes protected methods for testing.
        /// </summary>
        private class TestableImageSource : ImageSource
        {
            public async Task StartLoadingAsync()
            {
                await OnLoadingStarted();
            }

            public async Task CompleteLoadingAsync(bool cancelled)
            {
                await OnLoadingCompleted(cancelled);
            }

            public bool GetIsLoading()
            {
                return IsLoading;
            }
        }

        /// <summary>
        /// Tests that Cancel returns false immediately when the ImageSource is not loading.
        /// </summary>
        [Fact]
        public async Task Cancel_WhenNotLoading_ReturnsFalse()
        {
            // Arrange
            var imageSource = new TestableImageSource();

            // Act
            var result = await imageSource.Cancel();

            // Assert
            Assert.False(result);
            Assert.False(imageSource.GetIsLoading());
        }

        /// <summary>
        /// Tests that Cancel cancels the token source and returns false when loading and completion source is null.
        /// </summary>
        [Fact]
        public async Task Cancel_WhenLoadingAndCompletionSourceIsNull_CancelsTokenAndReturnsFalse()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            await imageSource.StartLoadingAsync();

            // Act
            var result = await imageSource.Cancel();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Cancel returns the existing TaskCompletionSource task when already set.
        /// </summary>
        [Fact]
        public async Task Cancel_WhenLoadingAndCompletionSourceExists_ReturnsExistingTask()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            await imageSource.StartLoadingAsync();

            // First call to Cancel should set up the completion source
            var firstCancelTask = imageSource.Cancel();

            // Act
            var secondCancelTask = imageSource.Cancel();

            // Assert
            Assert.Same(firstCancelTask, secondCancelTask);
            Assert.False(await firstCancelTask);
            Assert.False(await secondCancelTask);
        }

        /// <summary>
        /// Tests that Cancel works correctly when called multiple times in sequence while not loading.
        /// </summary>
        [Fact]
        public async Task Cancel_WhenNotLoadingCalledMultipleTimes_AlwaysReturnsFalse()
        {
            // Arrange
            var imageSource = new TestableImageSource();

            // Act
            var result1 = await imageSource.Cancel();
            var result2 = await imageSource.Cancel();
            var result3 = await imageSource.Cancel();

            // Assert
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);
        }

        /// <summary>
        /// Tests that Cancel behaves correctly after loading has completed.
        /// </summary>
        [Fact]
        public async Task Cancel_AfterLoadingCompleted_ReturnsFalse()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            await imageSource.StartLoadingAsync();
            await imageSource.CompleteLoadingAsync(false);

            // Act
            var result = await imageSource.Cancel();

            // Assert
            Assert.False(result);
            Assert.False(imageSource.GetIsLoading());
        }

        /// <summary>
        /// Tests that Cancel handles concurrent calls correctly during loading state.
        /// </summary>
        [Fact]
        public async Task Cancel_ConcurrentCallsWhileLoading_ReturnsConsistentResults()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            await imageSource.StartLoadingAsync();

            // Act
            var task1 = imageSource.Cancel();
            var task2 = imageSource.Cancel();
            var task3 = imageSource.Cancel();

            // Assert
            var result1 = await task1;
            var result2 = await task2;
            var result3 = await task3;

            // All should return the same result
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }

        /// <summary>
        /// Tests that the ImageSource constructor properly initializes the _mergedStyle field with correct parameters.
        /// Verifies that MergedStyle is created with the derived class type and instance reference.
        /// </summary>
        [Fact]
        public void Constructor_InitializesMergedStyle_WithCorrectParameters()
        {
            // Arrange & Act
            var testImageSource = new TestImageSource();

            // Assert
            Assert.NotNull(testImageSource._mergedStyle);
            Assert.Equal(typeof(TestImageSource), testImageSource._mergedStyle.TargetType);
            Assert.Same(testImageSource, testImageSource._mergedStyle.Target);
        }

        /// <summary>
        /// Tests that different derived ImageSource classes each get their specific type passed to MergedStyle.
        /// Verifies that GetType() returns the actual runtime type, not the base ImageSource type.
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentDerivedTypes_UsesDerivedTypeNotBaseType()
        {
            // Arrange & Act
            var testImageSource1 = new TestImageSource();
            var testImageSource2 = new AlternateTestImageSource();

            // Assert
            Assert.Equal(typeof(TestImageSource), testImageSource1._mergedStyle.TargetType);
            Assert.Equal(typeof(AlternateTestImageSource), testImageSource2._mergedStyle.TargetType);
            Assert.NotEqual(typeof(ImageSource), testImageSource1._mergedStyle.TargetType);
            Assert.NotEqual(typeof(ImageSource), testImageSource2._mergedStyle.TargetType);
        }

        /// <summary>
        /// Tests that multiple instances of the same derived type each get their own MergedStyle instance.
        /// Verifies that each instance has its unique MergedStyle with correct target reference.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_EachHasOwnMergedStyleInstance()
        {
            // Arrange & Act
            var testImageSource1 = new TestImageSource();
            var testImageSource2 = new TestImageSource();

            // Assert
            Assert.NotSame(testImageSource1._mergedStyle, testImageSource2._mergedStyle);
            Assert.Same(testImageSource1, testImageSource1._mergedStyle.Target);
            Assert.Same(testImageSource2, testImageSource2._mergedStyle.Target);
            Assert.Equal(typeof(TestImageSource), testImageSource1._mergedStyle.TargetType);
            Assert.Equal(typeof(TestImageSource), testImageSource2._mergedStyle.TargetType);
        }

        private class AlternateTestImageSource : ImageSource
        {
        }
    }


    public partial class ImageSourceFromResourceTests
    {
        /// <summary>
        /// Tests that FromResource with valid resource and type returns an ImageSource.
        /// Input: Valid resource string and valid Type.
        /// Expected: Returns a non-null ImageSource.
        /// </summary>
        [Fact]
        public void FromResource_WithValidResourceAndType_ReturnsImageSource()
        {
            // Arrange
            string resource = "test.resource.png";
            Type resolvingType = typeof(string);

            // Act
            ImageSource result = ImageSource.FromResource(resource, resolvingType);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that FromResource with null Type throws NullReferenceException.
        /// Input: Valid resource string and null Type.
        /// Expected: Throws NullReferenceException when accessing Assembly property.
        /// </summary>
        [Fact]
        public void FromResource_WithNullType_ThrowsNullReferenceException()
        {
            // Arrange
            string resource = "test.resource.png";
            Type resolvingType = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ImageSource.FromResource(resource, resolvingType));
        }

        /// <summary>
        /// Tests FromResource with different resource string values.
        /// Input: Various resource string values (null, empty, whitespace, valid) with valid Type.
        /// Expected: Returns ImageSource for all cases (delegates validation to underlying method).
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("valid.resource.name")]
        [InlineData("folder.subfolder.image.png")]
        public void FromResource_WithVariousResourceStrings_ReturnsImageSource(string resource)
        {
            // Arrange
            Type resolvingType = typeof(int);

            // Act
            ImageSource result = ImageSource.FromResource(resource, resolvingType);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests FromResource with different Type objects.
        /// Input: Valid resource string with various Type objects.
        /// Expected: Returns ImageSource for all type cases.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(ImageSource))]
        [InlineData(typeof(ImageSourceFromResourceTests))]
        public void FromResource_WithDifferentTypes_ReturnsImageSource(Type resolvingType)
        {
            // Arrange
            string resource = "test.resource";

            // Act
            ImageSource result = ImageSource.FromResource(resource, resolvingType);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that FromResource returns StreamImageSource type.
        /// Input: Valid resource and type.
        /// Expected: Returns an instance of StreamImageSource.
        /// </summary>
        [Fact]
        public void FromResource_WithValidInputs_ReturnsStreamImageSource()
        {
            // Arrange
            string resource = "test.resource.png";
            Type resolvingType = typeof(object);

            // Act
            ImageSource result = ImageSource.FromResource(resource, resolvingType);

            // Assert
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests FromResource with extreme string values.
        /// Input: Very long resource string and edge case Type.
        /// Expected: Returns ImageSource without throwing.
        /// </summary>
        [Fact]
        public void FromResource_WithLongResourceString_ReturnsImageSource()
        {
            // Arrange
            string resource = new string('a', 1000) + ".resource.extension";
            Type resolvingType = typeof(Array);

            // Act
            ImageSource result = ImageSource.FromResource(resource, resolvingType);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests FromResource with special characters in resource name.
        /// Input: Resource string containing special characters with valid Type.
        /// Expected: Returns ImageSource (delegates handling to underlying implementation).
        /// </summary>
        [Theory]
        [InlineData("resource-with-dashes.png")]
        [InlineData("resource_with_underscores.jpg")]
        [InlineData("resource.with.dots.gif")]
        [InlineData("resource with spaces.bmp")]
        [InlineData("resource@#$%^&*().ico")]
        public void FromResource_WithSpecialCharacters_ReturnsImageSource(string resource)
        {
            // Arrange
            Type resolvingType = typeof(Exception);

            // Act
            ImageSource result = ImageSource.FromResource(resource, resolvingType);

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that FromResource with valid resource name and explicit assembly returns StreamImageSource.
        /// Input: Valid resource name and current executing assembly.
        /// Expected: Returns StreamImageSource instance.
        /// </summary>
        [Fact]
        public void FromResource_ValidResourceAndAssembly_ReturnsStreamImageSource()
        {
            // Arrange
            var resource = "test.resource";
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = ImageSource.FromResource(resource, assembly);

            // Assert
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests that FromResource with null resource name and explicit assembly returns StreamImageSource.
        /// Input: Null resource name and current executing assembly.
        /// Expected: Returns StreamImageSource instance.
        /// </summary>
        [Fact]
        public void FromResource_NullResourceWithAssembly_ReturnsStreamImageSource()
        {
            // Arrange
            string resource = null;
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = ImageSource.FromResource(resource, assembly);

            // Assert
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests that FromResource with empty resource name and explicit assembly returns StreamImageSource.
        /// Input: Empty resource name and current executing assembly.
        /// Expected: Returns StreamImageSource instance.
        /// </summary>
        [Fact]
        public void FromResource_EmptyResourceWithAssembly_ReturnsStreamImageSource()
        {
            // Arrange
            var resource = string.Empty;
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = ImageSource.FromResource(resource, assembly);

            // Assert
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests that FromResource with whitespace-only resource name and explicit assembly returns StreamImageSource.
        /// Input: Whitespace-only resource name and current executing assembly.
        /// Expected: Returns StreamImageSource instance.
        /// </summary>
        [Fact]
        public void FromResource_WhitespaceResourceWithAssembly_ReturnsStreamImageSource()
        {
            // Arrange
            var resource = "   ";
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = ImageSource.FromResource(resource, assembly);

            // Assert
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests that FromResource with very long resource name and explicit assembly returns StreamImageSource.
        /// Input: Very long resource name and current executing assembly.
        /// Expected: Returns StreamImageSource instance.
        /// </summary>
        [Fact]
        public void FromResource_VeryLongResourceWithAssembly_ReturnsStreamImageSource()
        {
            // Arrange
            var resource = new string('a', 10000);
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = ImageSource.FromResource(resource, assembly);

            // Assert
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests that FromResource with special characters in resource name and explicit assembly returns StreamImageSource.
        /// Input: Resource name with special characters and current executing assembly.
        /// Expected: Returns StreamImageSource instance.
        /// </summary>
        [Theory]
        [InlineData("resource.with.dots")]
        [InlineData("resource-with-dashes")]
        [InlineData("resource_with_underscores")]
        [InlineData("resource with spaces")]
        [InlineData("resource!@#$%^&*()")]
        [InlineData("资源.png")]
        [InlineData("рэсурс.jpg")]
        public void FromResource_SpecialCharactersInResource_ReturnsStreamImageSource(string resource)
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result = ImageSource.FromResource(resource, assembly);

            // Assert
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests that FromResource with null assembly uses calling assembly and returns StreamImageSource.
        /// Input: Valid resource name and null assembly.
        /// Expected: Returns StreamImageSource instance.
        /// </summary>
        [Fact]
        public void FromResource_NullAssembly_ReturnsStreamImageSource()
        {
            // Arrange
            var resource = "test.resource";
            Assembly assembly = null;

            // Act
            var result = ImageSource.FromResource(resource, assembly);

            // Assert
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests that FromResource with both null resource and null assembly returns StreamImageSource.
        /// Input: Null resource name and null assembly.
        /// Expected: Returns StreamImageSource instance.
        /// </summary>
        [Fact]
        public void FromResource_NullResourceAndNullAssembly_ReturnsStreamImageSource()
        {
            // Arrange
            string resource = null;
            Assembly assembly = null;

            // Act
            var result = ImageSource.FromResource(resource, assembly);

            // Assert
            Assert.IsType<StreamImageSource>(result);
        }

        /// <summary>
        /// Tests that FromResource creates different instances for different calls.
        /// Input: Same resource name and assembly called twice.
        /// Expected: Returns different StreamImageSource instances.
        /// </summary>
        [Fact]
        public void FromResource_MultipleCalls_ReturnsDifferentInstances()
        {
            // Arrange
            var resource = "test.resource";
            var assembly = Assembly.GetExecutingAssembly();

            // Act
            var result1 = ImageSource.FromResource(resource, assembly);
            var result2 = ImageSource.FromResource(resource, assembly);

            // Assert
            Assert.IsType<StreamImageSource>(result1);
            Assert.IsType<StreamImageSource>(result2);
            Assert.NotSame(result1, result2);
        }
    }


    public partial class ImageSourceFromStreamAsyncTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that FromStream with a valid async function returns a StreamImageSource with the correct Stream property assigned.
        /// Input: Valid Func&lt;CancellationToken, Task&lt;Stream&gt;&gt;
        /// Expected: StreamImageSource instance with Stream property set to the provided function
        /// </summary>
        [Fact]
        public void FromStreamAsync_ValidFunction_ReturnsStreamImageSourceWithCorrectStream()
        {
            // Arrange
            Func<CancellationToken, Task<Stream>> streamFunc = token => Task.FromResult<Stream>(new MemoryStream());

            // Act
            var result = ImageSource.FromStream(streamFunc);

            // Assert
            Assert.IsType<StreamImageSource>(result);
            var streamImageSource = (StreamImageSource)result;
            Assert.Equal(streamFunc, streamImageSource.Stream);
        }

        /// <summary>
        /// Tests that FromStream with null function returns a StreamImageSource with null Stream property.
        /// Input: null function
        /// Expected: StreamImageSource instance with Stream property set to null
        /// </summary>
        [Fact]
        public void FromStreamAsync_NullFunction_ReturnsStreamImageSourceWithNullStream()
        {
            // Arrange
            Func<CancellationToken, Task<Stream>> streamFunc = null;

            // Act
            var result = ImageSource.FromStream(streamFunc);

            // Assert
            Assert.IsType<StreamImageSource>(result);
            var streamImageSource = (StreamImageSource)result;
            Assert.Null(streamImageSource.Stream);
        }

        /// <summary>
        /// Tests that FromStream preserves the exact function reference without modification.
        /// Input: Specific async function instance
        /// Expected: StreamImageSource with the exact same function reference
        /// </summary>
        [Fact]
        public void FromStreamAsync_ValidFunction_PreservesExactFunctionReference()
        {
            // Arrange
            static async Task<Stream> CreateStreamAsync(CancellationToken token)
            {
                await Task.Delay(1, token);
                return new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
            }

            Func<CancellationToken, Task<Stream>> streamFunc = CreateStreamAsync;

            // Act
            var result = ImageSource.FromStream(streamFunc);

            // Assert
            Assert.IsType<StreamImageSource>(result);
            var streamImageSource = (StreamImageSource)result;
            Assert.Same(streamFunc, streamImageSource.Stream);
        }

        /// <summary>
        /// Tests that FromStream works with various types of async stream functions.
        /// Input: Different async function implementations
        /// Expected: All create valid StreamImageSource instances
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void FromStreamAsync_DifferentAsyncFunctions_ReturnsValidStreamImageSource(bool useTaskRun)
        {
            // Arrange
            Func<CancellationToken, Task<Stream>> streamFunc = useTaskRun
                ? token => Task.Run(() => (Stream)new MemoryStream(), token)
                : token => Task.FromResult<Stream>(new MemoryStream());

            // Act
            var result = ImageSource.FromStream(streamFunc);

            // Assert
            Assert.IsType<StreamImageSource>(result);
            var streamImageSource = (StreamImageSource)result;
            Assert.NotNull(streamImageSource.Stream);
            Assert.Equal(streamFunc, streamImageSource.Stream);
        }
    }


    public partial class ImageSourceFromUriTests
    {
        /// <summary>
        /// Tests that FromUri throws NullReferenceException when uri parameter is null.
        /// Input: null Uri
        /// Expected: NullReferenceException when accessing IsAbsoluteUri property
        /// </summary>
        [Fact]
        public void FromUri_NullUri_ThrowsNullReferenceException()
        {
            // Arrange
            Uri nullUri = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => ImageSource.FromUri(nullUri));
        }

        /// <summary>
        /// Tests that FromUri throws ArgumentException when uri is relative.
        /// Input: relative Uri
        /// Expected: ArgumentException with message "uri is relative"
        /// </summary>
        [Theory]
        [InlineData("/relative/path")]
        [InlineData("relative/path")]
        [InlineData("../relative/path")]
        [InlineData("./relative/path")]
        [InlineData("file.txt")]
        public void FromUri_RelativeUri_ThrowsArgumentException(string relativeUriString)
        {
            // Arrange
            var relativeUri = new Uri(relativeUriString, UriKind.Relative);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => ImageSource.FromUri(relativeUri));
            Assert.Equal("uri is relative", exception.Message);
        }

        /// <summary>
        /// Tests that FromUri creates UriImageSource with correct Uri when uri is absolute.
        /// Input: absolute Uri
        /// Expected: UriImageSource with Uri property set to input uri
        /// </summary>
        [Theory]
        [InlineData("http://example.com/image.png")]
        [InlineData("https://example.com/image.jpg")]
        [InlineData("ftp://example.com/image.gif")]
        [InlineData("file:///C:/path/to/image.png")]
        [InlineData("http://example.com/")]
        [InlineData("https://example.com")]
        public void FromUri_AbsoluteUri_ReturnsUriImageSourceWithCorrectUri(string absoluteUriString)
        {
            // Arrange
            var absoluteUri = new Uri(absoluteUriString, UriKind.Absolute);

            // Act
            var result = ImageSource.FromUri(absoluteUri);

            // Assert
            Assert.IsType<UriImageSource>(result);
            var uriImageSource = (UriImageSource)result;
            Assert.Equal(absoluteUri, uriImageSource.Uri);
        }

        /// <summary>
        /// Tests that FromUri works correctly with various absolute URI schemes.
        /// Input: absolute Uris with different schemes
        /// Expected: UriImageSource with correct Uri property for each scheme
        /// </summary>
        [Theory]
        [InlineData("http://localhost:8080/image.png")]
        [InlineData("https://192.168.1.1/image.jpg")]
        [InlineData("ftp://ftp.example.com/images/test.gif")]
        [InlineData("file:///Users/user/image.png")]
        [InlineData("mailto:test@example.com")]
        [InlineData("tel:+1234567890")]
        public void FromUri_VariousAbsoluteUriSchemes_ReturnsCorrectUriImageSource(string uriString)
        {
            // Arrange
            var uri = new Uri(uriString);

            // Act
            var result = ImageSource.FromUri(uri);

            // Assert
            Assert.IsType<UriImageSource>(result);
            var uriImageSource = (UriImageSource)result;
            Assert.Equal(uri, uriImageSource.Uri);
            Assert.Equal(uriString, uriImageSource.Uri.ToString());
        }

        /// <summary>
        /// Tests that FromUri handles URIs with special characters and encoding correctly.
        /// Input: absolute Uris with special characters
        /// Expected: UriImageSource with correctly preserved Uri
        /// </summary>
        [Theory]
        [InlineData("http://example.com/image%20with%20spaces.png")]
        [InlineData("https://example.com/path/with/unicode/测试.jpg")]
        [InlineData("http://example.com/query?param=value&other=test")]
        [InlineData("https://example.com/fragment#section")]
        public void FromUri_UriWithSpecialCharacters_ReturnsCorrectUriImageSource(string uriString)
        {
            // Arrange
            var uri = new Uri(uriString);

            // Act
            var result = ImageSource.FromUri(uri);

            // Assert
            Assert.IsType<UriImageSource>(result);
            var uriImageSource = (UriImageSource)result;
            Assert.Equal(uri, uriImageSource.Uri);
        }

        /// <summary>
        /// Tests that FromUri correctly handles edge case of Uri with minimal absolute format.
        /// Input: minimal absolute Uri
        /// Expected: UriImageSource with correct Uri property
        /// </summary>
        [Fact]
        public void FromUri_MinimalAbsoluteUri_ReturnsUriImageSource()
        {
            // Arrange
            var uri = new Uri("http://a");

            // Act
            var result = ImageSource.FromUri(uri);

            // Assert
            Assert.IsType<UriImageSource>(result);
            var uriImageSource = (UriImageSource)result;
            Assert.Equal(uri, uriImageSource.Uri);
            Assert.True(uri.IsAbsoluteUri);
        }
    }


    public partial class ImageSourceSourceChangedEventTests : BaseTestFixture
    {
        /// <summary>
        /// Test helper class that exposes protected OnSourceChanged method for testing
        /// </summary>
        class TestableImageSource : ImageSource
        {
            public new void OnSourceChanged()
            {
                base.OnSourceChanged();
            }
        }

        /// <summary>
        /// Tests that adding a valid event handler to SourceChanged does not throw an exception
        /// </summary>
        [Fact]
        public void AddHandler_ValidHandler_NoException()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            var handlerCalled = false;
            System.EventHandler handler = (sender, e) => handlerCalled = true;

            // Act & Assert
            var exception = Record.Exception(() => imageSource.SourceChanged += handler);
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that adding a null event handler to SourceChanged throws ArgumentNullException
        /// </summary>
        [Fact]
        public void AddHandler_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var imageSource = new TestableImageSource();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => imageSource.SourceChanged += null);
        }

        /// <summary>
        /// Tests that removing a valid event handler from SourceChanged does not throw an exception
        /// </summary>
        [Fact]
        public void RemoveHandler_ValidHandler_NoException()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            System.EventHandler handler = (sender, e) => { };
            imageSource.SourceChanged += handler;

            // Act & Assert
            var exception = Record.Exception(() => imageSource.SourceChanged -= handler);
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that removing a null event handler from SourceChanged throws ArgumentNullException
        /// </summary>
        [Fact]
        public void RemoveHandler_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var imageSource = new TestableImageSource();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => imageSource.SourceChanged -= null);
        }

        /// <summary>
        /// Tests that calling OnSourceChanged invokes the registered event handler with correct arguments
        /// </summary>
        [Fact]
        public void OnSourceChanged_WithHandler_HandlerInvokedWithCorrectArguments()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            object receivedSender = null;
            System.EventArgs receivedArgs = null;
            var handlerInvoked = false;

            imageSource.SourceChanged += (sender, e) =>
            {
                receivedSender = sender;
                receivedArgs = e;
                handlerInvoked = true;
            };

            // Act
            imageSource.OnSourceChanged();

            // Assert
            Assert.True(handlerInvoked);
            Assert.Same(imageSource, receivedSender);
            Assert.Same(System.EventArgs.Empty, receivedArgs);
        }

        /// <summary>
        /// Tests that calling OnSourceChanged invokes all registered event handlers
        /// </summary>
        [Fact]
        public void OnSourceChanged_MultipleHandlers_AllHandlersInvoked()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            var handler1Called = false;
            var handler2Called = false;
            var handler3Called = false;

            imageSource.SourceChanged += (sender, e) => handler1Called = true;
            imageSource.SourceChanged += (sender, e) => handler2Called = true;
            imageSource.SourceChanged += (sender, e) => handler3Called = true;

            // Act
            imageSource.OnSourceChanged();

            // Assert
            Assert.True(handler1Called);
            Assert.True(handler2Called);
            Assert.True(handler3Called);
        }

        /// <summary>
        /// Tests that after removing an event handler, it is not invoked when OnSourceChanged is called
        /// </summary>
        [Fact]
        public void OnSourceChanged_RemovedHandler_HandlerNotInvoked()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            var removedHandlerCalled = false;
            var remainingHandlerCalled = false;

            System.EventHandler removedHandler = (sender, e) => removedHandlerCalled = true;
            System.EventHandler remainingHandler = (sender, e) => remainingHandlerCalled = true;

            imageSource.SourceChanged += removedHandler;
            imageSource.SourceChanged += remainingHandler;

            // Act - Remove one handler
            imageSource.SourceChanged -= removedHandler;
            imageSource.OnSourceChanged();

            // Assert
            Assert.False(removedHandlerCalled);
            Assert.True(remainingHandlerCalled);
        }

        /// <summary>
        /// Tests that OnSourceChanged can be called multiple times and handlers are invoked each time
        /// </summary>
        [Fact]
        public void OnSourceChanged_CalledMultipleTimes_HandlerInvokedEachTime()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            var invocationCount = 0;

            imageSource.SourceChanged += (sender, e) => invocationCount++;

            // Act
            imageSource.OnSourceChanged();
            imageSource.OnSourceChanged();
            imageSource.OnSourceChanged();

            // Assert
            Assert.Equal(3, invocationCount);
        }

        /// <summary>
        /// Tests that removing a handler that was never added does not throw an exception
        /// </summary>
        [Fact]
        public void RemoveHandler_HandlerNotPreviouslyAdded_NoException()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            System.EventHandler handler = (sender, e) => { };

            // Act & Assert
            var exception = Record.Exception(() => imageSource.SourceChanged -= handler);
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that adding the same handler multiple times and then calling OnSourceChanged only invokes it once
        /// </summary>
        [Fact]
        public void AddHandler_SameHandlerAddedMultipleTimes_HandlerInvokedOncePerAddition()
        {
            // Arrange
            var imageSource = new TestableImageSource();
            var invocationCount = 0;
            System.EventHandler handler = (sender, e) => invocationCount++;

            // Act - Add same handler multiple times
            imageSource.SourceChanged += handler;
            imageSource.SourceChanged += handler;
            imageSource.SourceChanged += handler;

            imageSource.OnSourceChanged();

            // Assert - Handler should be invoked multiple times (once per addition)
            Assert.Equal(3, invocationCount);
        }
    }
}