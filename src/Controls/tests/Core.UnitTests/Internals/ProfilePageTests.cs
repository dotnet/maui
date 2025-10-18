#nullable disable
#nullable disable

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for ContentPageEx extension methods
    /// </summary>
    public partial class ContentPageExTests
    {
        /// <summary>
        /// Tests that LoadProfile executes without throwing exceptions with empty profile data
        /// and sets up the UI components correctly.
        /// </summary>
        [Fact]
        public void LoadProfile_WithEmptyProfileData_SetsUpUIComponentsCorrectly()
        {
            // Arrange
            var page = new ContentPage();
            ClearStaticData();

            // Act
            page.LoadProfile();

            // Assert
            Assert.NotNull(page.Content);
            Assert.IsType<Grid>(page.Content);

            var grid = (Grid)page.Content;
            Assert.Equal(2, grid.RowDefinitions.Count);
            Assert.Equal(1, grid.ColumnDefinitions.Count);
            Assert.Equal(2, grid.Children.Count);

            // Verify ScrollView is in first row
            Assert.IsType<ScrollView>(grid.Children[0]);
            var scrollView = (ScrollView)grid.Children[0];
            Assert.NotNull(scrollView.Content);
            Assert.IsType<Label>(scrollView.Content);

            // Verify controls Grid is in second row
            Assert.IsType<Grid>(grid.Children[1]);
            var controlsGrid = (Grid)grid.Children[1];
            Assert.Equal(1, controlsGrid.Children.Count);
            Assert.IsType<Button>(controlsGrid.Children[0]);

            var button = (Button)controlsGrid.Children[0];
            Assert.Equal("0s", button.Text);
            Assert.Equal(62, button.HeightRequest);
        }

        /// <summary>
        /// Tests that LoadProfile processes profile data and populates the ContentPageEx.Data collection
        /// when Profile.Data contains valid profile entries.
        /// </summary>
        [Fact]
        public void LoadProfile_WithProfileData_PopulatesDataCollection()
        {
            // Arrange
            var page = new ContentPage();
            ClearStaticData();
            SetupTestProfileData();

            // Act
            page.LoadProfile();

            // Assert
            Assert.NotEmpty(ContentPageEx.Data);

            var firstDatum = ContentPageEx.Data[0];
            Assert.Equal("TestMethod", firstDatum.Name);
            Assert.Equal("TestId", firstDatum.Id);
            Assert.Equal(1, firstDatum.Depth);
            Assert.Equal(1000L, firstDatum.Ticks);
        }

        /// <summary>
        /// Tests that LoadProfile handles negative ticks by converting them to zero
        /// as per the implementation logic.
        /// </summary>
        [Fact]
        public void LoadProfile_WithNegativeTicks_ConvertsToZero()
        {
            // Arrange
            var page = new ContentPage();
            ClearStaticData();
            SetupProfileDataWithNegativeTicks();

            // Act
            page.LoadProfile();

            // Assert
            Assert.NotEmpty(ContentPageEx.Data);
            var datum = ContentPageEx.Data[0];
            Assert.Equal(0L, datum.Ticks);
        }

        /// <summary>
        /// Tests that LoadProfile button click handler toggles showZeros flag
        /// and updates the label content accordingly.
        /// </summary>
        [Fact]
        public void LoadProfile_ButtonClick_TogglesShowZerosAndUpdatesLabel()
        {
            // Arrange
            var page = new ContentPage();
            ClearStaticData();
            SetupTestProfileData();
            page.LoadProfile();

            var grid = (Grid)page.Content;
            var controlsGrid = (Grid)grid.Children[1];
            var button = (Button)controlsGrid.Children[0];
            var scrollView = (ScrollView)grid.Children[0];
            var label = (Label)scrollView.Content;

            var initialText = label.Text;

            // Act - Simulate button click
            var clickedEventArgs = new EventArgs();
            var clickedEvent = button.GetType().GetEvent("Clicked");
            var handlers = button.GetType().GetField("Clicked",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(button);

            if (handlers != null)
            {
                var multicastDelegate = handlers as MulticastDelegate;
                if (multicastDelegate != null)
                {
                    foreach (var handler in multicastDelegate.GetInvocationList())
                    {
                        handler.Method.Invoke(handler.Target, new object[] { button, clickedEventArgs });
                    }
                }
            }

            // Assert
            Assert.NotEqual(initialText, label.Text);
            Assert.Contains("Profiled:", label.Text);
        }

        /// <summary>
        /// Tests that LoadProfile handles multiple profile data entries with different depths
        /// and processes them correctly into the Data collection.
        /// </summary>
        [Theory]
        [InlineData(0, "RootMethod")]
        [InlineData(1, "ChildMethod")]
        [InlineData(2, "DeepMethod")]
        public void LoadProfile_WithVariousDepths_ProcessesCorrectly(int depth, string methodName)
        {
            // Arrange
            var page = new ContentPage();
            ClearStaticData();
            SetupProfileDataWithDepth(depth, methodName);

            // Act
            page.LoadProfile();

            // Assert
            Assert.NotEmpty(ContentPageEx.Data);
            var datum = ContentPageEx.Data[0];
            Assert.Equal(depth, datum.Depth);
            Assert.Equal(methodName, datum.Name);
        }

        /// <summary>
        /// Tests that LoadProfile initializes label with profile information
        /// containing the expected format and content.
        /// </summary>
        [Fact]
        public void LoadProfile_InitializesLabel_WithProfileInformation()
        {
            // Arrange
            var page = new ContentPage();
            ClearStaticData();
            SetupTestProfileData();

            // Act
            page.LoadProfile();

            // Assert
            var grid = (Grid)page.Content;
            var scrollView = (ScrollView)grid.Children[0];
            var label = (Label)scrollView.Content;

            Assert.NotNull(label.Text);
            Assert.Contains("Profiled:", label.Text);
            Assert.Contains("ms", label.Text);
        }

        /// <summary>
        /// Helper method to clear static data before tests
        /// </summary>
        private void ClearStaticData()
        {
            ContentPageEx.Data.Clear();
            Profile.Data.Clear();
        }

        /// <summary>
        /// Helper method to set up test profile data
        /// </summary>
        private void SetupTestProfileData()
        {
            Profile.Data.Add(new Profile.Datum
            {
                Name = "TestMethod",
                Id = "TestId",
                Ticks = 1000L,
                Depth = 1,
                Line = 10
            });
        }

        /// <summary>
        /// Helper method to set up profile data with negative ticks
        /// </summary>
        private void SetupProfileDataWithNegativeTicks()
        {
            Profile.Data.Add(new Profile.Datum
            {
                Name = "NegativeMethod",
                Id = "NegId",
                Ticks = -500L,
                Depth = 0,
                Line = 5
            });
        }

        /// <summary>
        /// Helper method to set up profile data with specific depth
        /// </summary>
        private void SetupProfileDataWithDepth(int depth, string methodName)
        {
            Profile.Data.Add(new Profile.Datum
            {
                Name = methodName,
                Id = "TestId",
                Ticks = 2000L,
                Depth = depth,
                Line = 15
            });
        }
    }
}
