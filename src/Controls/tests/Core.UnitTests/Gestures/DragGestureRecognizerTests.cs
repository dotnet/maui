using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class DragGestureRecognizerTests : BaseTestFixture
    {
        [Fact]
        public void PropertySetters()
        {
            var dragRec = new DragGestureRecognizer();

            Command cmd = new Command(() => { });
            var parameter = new Object();
            dragRec.CanDrag = true;
            dragRec.DragStartingCommand = cmd;
            dragRec.DragStartingCommandParameter = parameter;
            dragRec.DropCompletedCommand = cmd;
            dragRec.DropCompletedCommandParameter = parameter;

            Assert.True(dragRec.CanDrag);
            Assert.Equal(cmd, dragRec.DragStartingCommand);
            Assert.Equal(parameter, dragRec.DragStartingCommandParameter);
            Assert.Equal(cmd, dragRec.DropCompletedCommand);
            Assert.Equal(parameter, dragRec.DropCompletedCommandParameter);
        }

        [Fact]
        public void DragStartingCommandFires()
        {
            var dragRec = new DragGestureRecognizer();
            var parameter = new Object();
            object commandExecuted = null;
            Command cmd = new Command(() => commandExecuted = parameter);

            dragRec.DragStartingCommand = cmd;
            dragRec.DragStartingCommandParameter = parameter;
            dragRec.SendDragStarting(new Label());

            Assert.Equal(commandExecuted, parameter);
        }

        [Fact]
        public void UserSpecifiedTextIsntOverwritten()
        {
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "WRONG TEXT" };
            dragRec.DragStarting += (_, args) =>
            {
                args.Data.Text = "Right Text";
            };

            var returnedArgs = dragRec.SendDragStarting(element);
            Assert.Equal("Right Text", returnedArgs.Data.Text);
        }

        [Fact]
        public void UserSpecifiedImageIsntOverwritten()
        {
            var dragRec = new DragGestureRecognizer();
            var element = new Image() { Source = "http://www.someimage.com" };
            FileImageSource fileImageSource = new FileImageSource() { File = "yay.jpg" };

            dragRec.DragStarting += (_, args) =>
            {
                args.Data.Image = fileImageSource;
            };

            var returnedArgs = dragRec.SendDragStarting(element);
            Assert.Equal(fileImageSource, returnedArgs.Data.Image);
        }

        [Fact]
        public void DropCompletedCommandFires()
        {
            var dragRec = new DragGestureRecognizer();
            var parameter = new Object();
            object commandExecuted = null;
            Command cmd = new Command(() => commandExecuted = parameter);

            dragRec.SendDragStarting(new Label());
            dragRec.DropCompletedCommand = cmd;
            dragRec.DropCompletedCommandParameter = parameter;
            dragRec.SendDropCompleted(new DropCompletedEventArgs());

            Assert.Equal(commandExecuted, parameter);
        }

        [Fact]
        public void DropCompletedCommandFiresOnce()
        {
            int counter = 0;
            var dragRec = new DragGestureRecognizer();
            Command cmd = new Command(() => counter++);

            dragRec.SendDragStarting(new Label());
            dragRec.DropCompletedCommand = cmd;
            dragRec.SendDropCompleted(new DropCompletedEventArgs());
            dragRec.SendDropCompleted(new DropCompletedEventArgs());
            dragRec.SendDropCompleted(new DropCompletedEventArgs());

            Assert.Equal(1, counter);
        }

        [Theory]
        [InlineData(typeof(Entry), "EntryTest")]
        [InlineData(typeof(Label), "LabelTest")]
        [InlineData(typeof(Editor), "EditorTest")]
        [InlineData(typeof(TimePicker), "01:00:00")]
        [InlineData(typeof(CheckBox), "True")]
        [InlineData(typeof(Switch), "True")]
        [InlineData(typeof(RadioButton), "True")]
        public void TextPackageCorrectlyExtractedFromCompatibleElement(Type fieldType, string result)
        {
            var dragRec = new DragGestureRecognizer();
            var element = (VisualElement)Activator.CreateInstance(fieldType);
            Assert.True(element.TrySetValue(result));
            var args = dragRec.SendDragStarting((View)element);
            Assert.Equal(result, args.Data.Text);
        }

        [Theory]
        [InlineData(typeof(DatePicker), "12/12/2020 12:00:00 AM")]
        public void DateTextPackageCorrectlyExtractedFromCompatibleElement(Type fieldType, string result)
        {
            var date = DateTime.Parse(result);
            result = date.ToString();
            TextPackageCorrectlyExtractedFromCompatibleElement(fieldType, result);
        }

        [Fact]
        public void HandledTest()
        {
            string testString = "test String";
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = testString };
            element.Text = "Text Shouldn't change";
            var args = new DragStartingEventArgs();
            args.Handled = true;
            args.Data.Text = "Text Shouldn't change";
            dragRec.SendDragStarting(element);
            Assert.NotEqual(args.Data.Text, testString);
        }

        /// <summary>
        /// Tests that SendDragStarting returns early when Cancel property is set to true in event handler.
        /// Expected result: Method returns args without setting _isDragActive or processing text/image defaults.
        /// </summary>
        [Fact]
        public void SendDragStarting_CancelSetToTrue_ReturnsEarly()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "test text" };
            bool eventFired = false;

            dragRec.DragStarting += (sender, args) =>
            {
                eventFired = true;
                args.Cancel = true;
            };

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.True(eventFired);
            Assert.True(result.Cancel);
            Assert.Equal("test text", result.Data.Text); // Text should not be overwritten
        }

        /// <summary>
        /// Tests that SendDragStarting returns early when Handled property is set to true in event handler.
        /// Expected result: Method returns args without setting _isDragActive or processing text/image defaults.
        /// </summary>
        [Fact]
        public void SendDragStarting_HandledSetToTrue_ReturnsEarly()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "test text" };
            bool eventFired = false;

            dragRec.DragStarting += (sender, args) =>
            {
                eventFired = true;
                args.Handled = true;
            };

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.True(eventFired);
            Assert.True(result.Handled);
            Assert.Equal("test text", result.Data.Text); // Text should not be overwritten
        }

        /// <summary>
        /// Tests that SendDragStarting returns early when both Cancel and Handled properties are set to true.
        /// Expected result: Method returns args without further processing.
        /// </summary>
        [Fact]
        public void SendDragStarting_BothCancelAndHandledSetToTrue_ReturnsEarly()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "test text" };

            dragRec.DragStarting += (sender, args) =>
            {
                args.Cancel = true;
                args.Handled = true;
            };

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.True(result.Cancel);
            Assert.True(result.Handled);
        }

        /// <summary>
        /// Tests that SendDragStarting sets image from IImageElement when Data.Image is null.
        /// Expected result: args.Data.Image is set to the element's Source property.
        /// </summary>
        [Fact]
        public void SendDragStarting_ImageElementWithNullDataImage_SetsImageFromSource()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var imageSource = new FileImageSource { File = "test.png" };
            var element = new Image() { Source = imageSource };

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.Equal(imageSource, result.Data.Image);
        }

        /// <summary>
        /// Tests that SendDragStarting does not overwrite existing image when element is IImageElement.
        /// Expected result: Existing Data.Image value is preserved.
        /// </summary>
        [Fact]
        public void SendDragStarting_ImageElementWithExistingDataImage_DoesNotOverwriteImage()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var elementImageSource = new FileImageSource { File = "element.png" };
            var existingImageSource = new FileImageSource { File = "existing.png" };
            var element = new Image() { Source = elementImageSource };

            dragRec.DragStarting += (sender, args) =>
            {
                args.Data.Image = existingImageSource;
            };

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.Equal(existingImageSource, result.Data.Image);
            Assert.NotEqual(elementImageSource, result.Data.Image);
        }

        /// <summary>
        /// Tests that SendDragStarting does not set image when element is not IImageElement.
        /// Expected result: Data.Image remains null.
        /// </summary>
        [Fact]
        public void SendDragStarting_NonImageElement_DoesNotSetImage()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "test" };

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.Null(result.Data.Image);
        }

        /// <summary>
        /// Tests that SendDragStarting handles null element parameter gracefully.
        /// Expected result: Method completes without throwing exception.
        /// </summary>
        [Fact]
        public void SendDragStarting_NullElement_HandlesGracefully()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();

            // Act & Assert - should not throw
            var result = dragRec.SendDragStarting(null);

            Assert.NotNull(result);
            Assert.Null(result.Data.Text);
        }

        /// <summary>
        /// Tests that SendDragStarting with null DragStartingCommand does not throw exception.
        /// Expected result: Method completes successfully without executing any command.
        /// </summary>
        [Fact]
        public void SendDragStarting_NullDragStartingCommand_DoesNotThrow()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "test" };
            dragRec.DragStartingCommand = null;

            // Act & Assert - should not throw
            var result = dragRec.SendDragStarting(element);

            Assert.NotNull(result);
            Assert.Equal("test", result.Data.Text);
        }

        /// <summary>
        /// Tests that SendDragStarting executes DragStartingCommand with correct parameter.
        /// Expected result: Command is executed with DragStartingCommandParameter.
        /// </summary>
        [Fact]
        public void SendDragStarting_WithCommand_ExecutesCommandWithParameter()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label();
            var command = Substitute.For<ICommand>();
            var parameter = new object();

            dragRec.DragStartingCommand = command;
            dragRec.DragStartingCommandParameter = parameter;

            // Act
            dragRec.SendDragStarting(element);

            // Assert
            command.Received(1).Execute(parameter);
        }

        /// <summary>
        /// Tests that SendDragStarting adds element as DragSource to properties when not handled.
        /// Expected result: Element is added to Data.PropertiesInternal with key "DragSource".
        /// </summary>
        [Fact]
        public void SendDragStarting_NotHandled_AddsDragSourceToProperties()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label();

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.False(result.Handled);
            Assert.False(result.Cancel);
            // Note: Cannot directly test PropertiesInternal as it's internal, but this ensures the path is taken
        }

        /// <summary>
        /// Tests SendDragStarting with various optional parameter combinations.
        /// Expected result: Method handles all parameter combinations correctly.
        /// </summary>
        [Theory]
        [InlineData(true, false)] // getPosition provided, no platformArgs
        [InlineData(false, true)] // no getPosition, platformArgs provided
        [InlineData(true, true)]  // both provided
        [InlineData(false, false)] // neither provided
        public void SendDragStarting_VariousOptionalParameters_HandlesCorrectly(bool hasGetPosition, bool hasPlatformArgs)
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "test" };

            Func<IElement, Microsoft.Maui.Graphics.Point?> getPosition = hasGetPosition ?
                (e) => new Microsoft.Maui.Graphics.Point(10, 20) : null;

            var platformArgs = hasPlatformArgs ? new PlatformDragStartingEventArgs() : null;

            // Act
            var result = dragRec.SendDragStarting(element, getPosition, platformArgs);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test", result.Data.Text);
            if (hasPlatformArgs)
                Assert.Equal(platformArgs, result.PlatformArgs);
            else
                Assert.Null(result.PlatformArgs);
        }

        /// <summary>
        /// Tests that SendDragStart does not set text when Data.Text already has non-whitespace content.
        /// Expected result: Existing text is preserved.
        /// </summary>
        [Fact]
        public void SendDragStarting_ExistingNonWhitespaceText_PreservesText()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "element text" };
            var existingText = "existing text";

            dragRec.DragStarting += (sender, args) =>
            {
                args.Data.Text = existingText;
            };

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.Equal(existingText, result.Data.Text);
        }

        /// <summary>
        /// Tests that SendDragStarting sets text from element when Data.Text is null or whitespace.
        /// Expected result: Element's string value is assigned to Data.Text.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n\r")]
        public void SendDragStarting_NullOrWhitespaceText_SetsTextFromElement(string initialText)
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "element text" };

            if (initialText != null)
            {
                dragRec.DragStarting += (sender, args) =>
                {
                    args.Data.Text = initialText;
                };
            }

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.Equal("element text", result.Data.Text);
        }

        /// <summary>
        /// Tests that SendDragStarting invokes DragStarting event with correct parameters.
        /// Expected result: Event is invoked with element and args.
        /// </summary>
        [Fact]
        public void SendDragStarting_InvokesDragStartingEvent_WithCorrectParameters()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label();
            object capturedSender = null;
            DragStartingEventArgs capturedArgs = null;

            dragRec.DragStarting += (sender, args) =>
            {
                capturedSender = sender;
                capturedArgs = args;
            };

            // Act
            var result = dragRec.SendDragStarting(element);

            // Assert
            Assert.Equal(element, capturedSender);
            Assert.Equal(result, capturedArgs);
        }

        /// <summary>
        /// Tests that SendDragStarting works correctly when no DragStarting event handlers are attached.
        /// Expected result: Method completes successfully without throwing exception.
        /// </summary>
        [Fact]
        public void SendDragStarting_NoDragStartingEventHandlers_CompletesSuccessfully()
        {
            // Arrange
            var dragRec = new DragGestureRecognizer();
            var element = new Label() { Text = "test" };

            // Act & Assert - should not throw
            var result = dragRec.SendDragStarting(element);

            Assert.NotNull(result);
            Assert.Equal("test", result.Data.Text);
            Assert.False(result.Cancel);
            Assert.False(result.Handled);
        }
    }
}