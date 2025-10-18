#nullable disable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class DataPackageTests : BaseTestFixture
    {
        [Fact]
        public void PropertySetters()
        {
            var dataPackage = new DataPackage();

            ImageSource imageSource = "somefile.jpg";
            dataPackage.Text = "text";
            dataPackage.Image = imageSource;
            dataPackage.Properties["key"] = "value";

            Assert.Equal("text", dataPackage.Text);
            Assert.Equal(imageSource, dataPackage.Image);
            Assert.Equal("value", dataPackage.Properties["key"]);
        }

        [Fact]
        public async Task DataPackageViewGetters()
        {
            var dataPackage = new DataPackage();

            ImageSource imageSource = "somefile.jpg";
            dataPackage.Text = "text";
            dataPackage.Image = imageSource;
            dataPackage.Properties["key"] = "value";
            var dataView = dataPackage.View;

            Assert.Equal("text", await dataView.GetTextAsync());
            Assert.Equal(imageSource, await dataView.GetImageAsync());
            Assert.Equal("value", dataView.Properties["key"]);
        }


        [Fact]
        public async Task DataPackageViewGettersArentTiedToInitialDataPackage()
        {
            var dataPackage = new DataPackage();

            ImageSource imageSource = "somefile.jpg";
            dataPackage.Text = "text";
            dataPackage.Image = imageSource;
            dataPackage.Properties["key"] = "value";
            var dataView = dataPackage.View;


            dataPackage.Text = "fail";
            dataPackage.Image = "differentfile.jpg";
            dataPackage.Properties["key"] = "fail";


            Assert.Equal("text", await dataView.GetTextAsync());
            Assert.Equal(imageSource, await dataView.GetImageAsync());
            Assert.Equal("value", dataView.Properties["key"]);
        }
    }

    public partial class DataPackageCloneTests : BaseTestFixture
    {
        /// <summary>
        /// Tests that Clone method creates an independent copy of an empty DataPackage.
        /// Verifies that the cloned package has separate property collections and default values.
        /// </summary>
        [Fact]
        public void Clone_WithEmptyDataPackage_ReturnsIndependentCopy()
        {
            // Arrange
            var original = new DataPackage();

            // Act
            var clone = original.Clone();

            // Assert
            Assert.NotSame(original, clone);
            Assert.Null(clone.Text);
            Assert.Null(clone.Image);
            Assert.NotSame(original.Properties, clone.Properties);
            Assert.NotSame(original.PropertiesInternal, clone.PropertiesInternal);
            Assert.Equal(0, clone.Properties.Count);
            Assert.Equal(0, clone.PropertiesInternal.Count);
        }

        /// <summary>
        /// Tests that Clone method correctly copies Text and Image properties.
        /// Verifies that string and ImageSource properties are properly transferred to the clone.
        /// </summary>
        [Fact]
        public void Clone_WithTextAndImage_CopiesProperties()
        {
            // Arrange
            var original = new DataPackage();
            ImageSource imageSource = "test.jpg";
            original.Text = "test text";
            original.Image = imageSource;

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Equal("test text", clone.Text);
            Assert.Equal(imageSource, clone.Image);
        }

        /// <summary>
        /// Tests that Clone method copies all items from the public Properties collection.
        /// Verifies that key-value pairs are properly transferred and the collections remain independent.
        /// </summary>
        [Fact]
        public void Clone_WithPublicProperties_CopiesAllProperties()
        {
            // Arrange
            var original = new DataPackage();
            original.Properties["key1"] = "value1";
            original.Properties["key2"] = 42;
            original.Properties["key3"] = new object();

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Equal(3, clone.Properties.Count);
            Assert.Equal("value1", clone.Properties["key1"]);
            Assert.Equal(42, clone.Properties["key2"]);
            Assert.Equal(original.Properties["key3"], clone.Properties["key3"]);
            Assert.NotSame(original.Properties, clone.Properties);
        }

        /// <summary>
        /// Tests that Clone method copies all items from the internal PropertiesInternal collection.
        /// Verifies that internal properties are properly transferred and covers the uncovered foreach loop.
        /// </summary>
        [Fact]
        public void Clone_WithInternalProperties_CopiesAllInternalProperties()
        {
            // Arrange
            var original = new DataPackage();
            original.PropertiesInternal["internal1"] = "internal_value1";
            original.PropertiesInternal["internal2"] = 123;
            original.PropertiesInternal["internal3"] = new DateTime(2023, 1, 1);

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Equal(3, clone.PropertiesInternal.Count);
            Assert.Equal("internal_value1", clone.PropertiesInternal["internal1"]);
            Assert.Equal(123, clone.PropertiesInternal["internal2"]);
            Assert.Equal(new DateTime(2023, 1, 1), clone.PropertiesInternal["internal3"]);
            Assert.NotSame(original.PropertiesInternal, clone.PropertiesInternal);
        }

        /// <summary>
        /// Tests that Clone method handles null Text and Image values correctly.
        /// Verifies that null values are preserved in the cloned package.
        /// </summary>
        [Fact]
        public void Clone_WithNullTextAndImage_HandlesNullValues()
        {
            // Arrange
            var original = new DataPackage();
            original.Text = null;
            original.Image = null;

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Null(clone.Text);
            Assert.Null(clone.Image);
        }

        /// <summary>
        /// Tests that modifying the original DataPackage after cloning does not affect the clone.
        /// Verifies that the clone is truly independent and not affected by changes to the original.
        /// </summary>
        [Fact]
        public void Clone_ModifyingOriginalAfterClone_DoesNotAffectClone()
        {
            // Arrange
            var original = new DataPackage();
            original.Text = "original text";
            ImageSource originalImage = "original.jpg";
            original.Image = originalImage;
            original.Properties["key"] = "original value";
            original.PropertiesInternal["internal"] = "original internal";

            // Act
            var clone = original.Clone();

            // Modify original after cloning
            original.Text = "modified text";
            original.Image = "modified.jpg";
            original.Properties["key"] = "modified value";
            original.Properties["new_key"] = "new value";
            original.PropertiesInternal["internal"] = "modified internal";
            original.PropertiesInternal["new_internal"] = "new internal";

            // Assert
            Assert.Equal("original text", clone.Text);
            Assert.Equal(originalImage, clone.Image);
            Assert.Equal("original value", clone.Properties["key"]);
            Assert.Equal("original internal", clone.PropertiesInternal["internal"]);
            Assert.False(clone.Properties.ContainsKey("new_key"));
            Assert.False(clone.PropertiesInternal.ContainsKey("new_internal"));
        }

        /// <summary>
        /// Tests that modifying the clone after creation does not affect the original DataPackage.
        /// Verifies bidirectional independence between original and cloned packages.
        /// </summary>
        [Fact]
        public void Clone_ModifyingCloneAfterCreation_DoesNotAffectOriginal()
        {
            // Arrange
            var original = new DataPackage();
            original.Text = "original text";
            ImageSource originalImage = "original.jpg";
            original.Image = originalImage;
            original.Properties["key"] = "original value";
            original.PropertiesInternal["internal"] = "original internal";

            // Act
            var clone = original.Clone();

            // Modify clone after creation
            clone.Text = "clone text";
            clone.Image = "clone.jpg";
            clone.Properties["key"] = "clone value";
            clone.Properties["clone_key"] = "clone value";
            clone.PropertiesInternal["internal"] = "clone internal";
            clone.PropertiesInternal["clone_internal"] = "clone internal";

            // Assert
            Assert.Equal("original text", original.Text);
            Assert.Equal(originalImage, original.Image);
            Assert.Equal("original value", original.Properties["key"]);
            Assert.Equal("original internal", original.PropertiesInternal["internal"]);
            Assert.False(original.Properties.ContainsKey("clone_key"));
            Assert.False(original.PropertiesInternal.ContainsKey("clone_internal"));
        }

        /// <summary>
        /// Tests that Clone method works correctly with both public and internal properties populated.
        /// Verifies comprehensive copying of all property types in a single test scenario.
        /// </summary>
        [Fact]
        public void Clone_WithBothPublicAndInternalProperties_CopiesAllCorrectly()
        {
            // Arrange
            var original = new DataPackage();
            original.Text = "comprehensive test";
            ImageSource image = "comprehensive.jpg";
            original.Image = image;

            original.Properties["public1"] = "public_value1";
            original.Properties["public2"] = 456;

            original.PropertiesInternal["internal1"] = "internal_value1";
            original.PropertiesInternal["internal2"] = 789;

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Equal("comprehensive test", clone.Text);
            Assert.Equal(image, clone.Image);

            Assert.Equal(2, clone.Properties.Count);
            Assert.Equal("public_value1", clone.Properties["public1"]);
            Assert.Equal(456, clone.Properties["public2"]);

            Assert.Equal(2, clone.PropertiesInternal.Count);
            Assert.Equal("internal_value1", clone.PropertiesInternal["internal1"]);
            Assert.Equal(789, clone.PropertiesInternal["internal2"]);
        }
    }
}