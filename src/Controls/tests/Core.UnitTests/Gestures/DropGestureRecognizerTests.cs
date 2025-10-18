#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class DropGestureRecognizerTests : BaseTestFixture
    {
        [Fact]
        public void PropertySetters()
        {
            var dropRec = new DropGestureRecognizer() { AllowDrop = true };

            Command cmd = new Command(() => { });
            var parameter = new Object();
            dropRec.AllowDrop = true;
            dropRec.DragOverCommand = cmd;
            dropRec.DragOverCommandParameter = parameter;
            dropRec.DropCommand = cmd;
            dropRec.DropCommandParameter = parameter;

            Assert.True(dropRec.AllowDrop);
            Assert.Equal(cmd, dropRec.DragOverCommand);
            Assert.Equal(parameter, dropRec.DragOverCommandParameter);
            Assert.Equal(cmd, dropRec.DropCommand);
            Assert.Equal(parameter, dropRec.DropCommandParameter);
        }

        [Fact]
        public void DragOverCommandFires()
        {
            var dropRec = new DropGestureRecognizer() { AllowDrop = true };
            var parameter = new Object();
            object commandExecuted = null;
            Command cmd = new Command(() => commandExecuted = parameter);

            dropRec.DragOverCommand = cmd;
            dropRec.DragOverCommandParameter = parameter;
            dropRec.SendDragOver(new DragEventArgs(new DataPackage()));

            Assert.Equal(parameter, commandExecuted);
        }

        [Fact]
        public async Task DropCommandFires()
        {
            var dropRec = new DropGestureRecognizer() { AllowDrop = true };
            var parameter = new Object();
            object commandExecuted = null;
            Command cmd = new Command(() => commandExecuted = parameter);

            dropRec.DropCommand = cmd;
            dropRec.DropCommandParameter = parameter;
            await dropRec.SendDrop(new DropEventArgs(new DataPackageView(new DataPackage())));

            Assert.Equal(commandExecuted, parameter);
        }

        [Theory]
        [InlineData(typeof(Entry), "EntryTest")]
        [InlineData(typeof(Label), "LabelTest")]
        [InlineData(typeof(Editor), "EditorTest")]
        [InlineData(typeof(TimePicker), "01:00:00")]
        [InlineData(typeof(CheckBox), "True")]
        [InlineData(typeof(Switch), "True")]
        [InlineData(typeof(RadioButton), "True")]
        public async Task TextPackageCorrectlySetsOnCompatibleTarget(Type fieldType, string result)
        {
            var dropRec = new DropGestureRecognizer() { AllowDrop = true };
            var element = (View)Activator.CreateInstance(fieldType);
            element.GestureRecognizers.Add(dropRec);
            var args = new DropEventArgs(new DataPackageView(new DataPackage() { Text = result }));
            await dropRec.SendDrop(args);
            Assert.Equal(element.GetStringValue(), result);
        }

        [Theory]
        [InlineData(typeof(DatePicker), "12/12/2020 12:00:00 AM")]
        public async Task DateTextPackageCorrectlySetsOnCompatibleTarget(Type fieldType, string result)
        {
            var date = DateTime.Parse(result);
            result = date.ToString();
            await TextPackageCorrectlySetsOnCompatibleTarget(fieldType, result);
        }

        [Fact]
        public async Task HandledTest()
        {
            string testString = "test String";
            var dropTec = new DropGestureRecognizer() { AllowDrop = true };
            var element = new Label();
            element.Text = "Text Shouldn't change";
            var args = new DropEventArgs(new DataPackageView(new DataPackage() { Text = testString }));
            args.Handled = true;
            await dropTec.SendDrop(args);
            Assert.NotEqual(element.Text, testString);
        }

        /// <summary>
        /// Tests that DragLeaveCommandParameter returns null by default.
        /// Verifies the initial state of the property when no value has been set.
        /// Expected result: Property should return null.
        /// </summary>
        [Fact]
        public void DragLeaveCommandParameter_DefaultValue_ReturnsNull()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer();

            // Act
            var result = dropGestureRecognizer.DragLeaveCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that DragLeaveCommandParameter correctly sets and returns various object values.
        /// Verifies the property can handle different types of objects including null, strings, numbers, and complex objects.
        /// Expected result: Property should return the exact same object that was set.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("test string")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(true)]
        [InlineData(false)]
        [InlineData(3.14)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        public void DragLeaveCommandParameter_SetAndGetValue_ReturnsCorrectValue(object testValue)
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer();

            // Act
            dropGestureRecognizer.DragLeaveCommandParameter = testValue;
            var result = dropGestureRecognizer.DragLeaveCommandParameter;

            // Assert
            Assert.Equal(testValue, result);
        }

        /// <summary>
        /// Tests that DragLeaveCommandParameter can handle complex objects and collections.
        /// Verifies the property works with reference types and maintains object identity.
        /// Expected result: Property should return the same reference that was set.
        /// </summary>
        [Fact]
        public void DragLeaveCommandParameter_SetComplexObjects_MaintainsReference()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer();
            var complexObject = new { Name = "Test", Value = 123 };
            var arrayObject = new[] { 1, 2, 3 };
            var command = new Command(() => { });

            // Act & Assert - Test complex anonymous object
            dropGestureRecognizer.DragLeaveCommandParameter = complexObject;
            Assert.Same(complexObject, dropGestureRecognizer.DragLeaveCommandParameter);

            // Act & Assert - Test array
            dropGestureRecognizer.DragLeaveCommandParameter = arrayObject;
            Assert.Same(arrayObject, dropGestureRecognizer.DragLeaveCommandParameter);

            // Act & Assert - Test command object
            dropGestureRecognizer.DragLeaveCommandParameter = command;
            Assert.Same(command, dropGestureRecognizer.DragLeaveCommandParameter);
        }

        /// <summary>
        /// Tests that DragLeaveCommandParameter correctly handles multiple consecutive sets and gets.
        /// Verifies the property maintains consistency across multiple operations.
        /// Expected result: Each set operation should be reflected in subsequent get operations.
        /// </summary>
        [Fact]
        public void DragLeaveCommandParameter_MultipleSetOperations_MaintainsConsistency()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer();
            var firstValue = "first";
            var secondValue = 42;
            var thirdValue = new object();

            // Act & Assert - First set/get cycle
            dropGestureRecognizer.DragLeaveCommandParameter = firstValue;
            Assert.Equal(firstValue, dropGestureRecognizer.DragLeaveCommandParameter);

            // Act & Assert - Second set/get cycle
            dropGestureRecognizer.DragLeaveCommandParameter = secondValue;
            Assert.Equal(secondValue, dropGestureRecognizer.DragLeaveCommandParameter);

            // Act & Assert - Third set/get cycle
            dropGestureRecognizer.DragLeaveCommandParameter = thirdValue;
            Assert.Same(thirdValue, dropGestureRecognizer.DragLeaveCommandParameter);

            // Act & Assert - Reset to null
            dropGestureRecognizer.DragLeaveCommandParameter = null;
            Assert.Null(dropGestureRecognizer.DragLeaveCommandParameter);
        }

        /// <summary>
        /// Tests that SendDragLeave does not throw when DragLeaveCommand is null.
        /// Verifies the null-conditional operator behavior for command execution.
        /// </summary>
        [Fact]
        public void SendDragLeave_DragLeaveCommandNull_DoesNotThrow()
        {
            // Arrange
            var dropRec = new DropGestureRecognizer();
            var args = new DragEventArgs(new DataPackage());
            dropRec.DragLeaveCommand = null;

            // Act & Assert
            dropRec.SendDragLeave(args);
        }

        /// <summary>
        /// Tests that SendDragLeave does not throw when DragLeave event has no subscribers.
        /// Verifies the null-conditional operator behavior for event invocation.
        /// </summary>
        [Fact]
        public void SendDragLeave_DragLeaveEventNull_DoesNotThrow()
        {
            // Arrange
            var dropRec = new DropGestureRecognizer();
            var args = new DragEventArgs(new DataPackage());

            // Act & Assert
            dropRec.SendDragLeave(args);
        }

        /// <summary>
        /// Tests that SendDragLeave executes DragLeaveCommand with DragLeaveCommandParameter when command is not null.
        /// Verifies that the correct parameter is passed to the command execution.
        /// </summary>
        [Fact]
        public void SendDragLeave_DragLeaveCommandNotNull_ExecutesCommandWithParameter()
        {
            // Arrange
            var dropRec = new DropGestureRecognizer();
            var parameter = new object();
            object commandExecuted = null;
            Command cmd = new Command(() => commandExecuted = parameter);
            var args = new DragEventArgs(new DataPackage());

            dropRec.DragLeaveCommand = cmd;
            dropRec.DragLeaveCommandParameter = parameter;

            // Act
            dropRec.SendDragLeave(args);

            // Assert
            Assert.Equal(parameter, commandExecuted);
        }

        /// <summary>
        /// Tests that SendDragLeave executes DragLeaveCommand with null parameter when DragLeaveCommandParameter is null.
        /// Verifies proper handling of null command parameters.
        /// </summary>
        [Fact]
        public void SendDragLeave_DragLeaveCommandParameterNull_ExecutesCommandWithNull()
        {
            // Arrange
            var dropRec = new DropGestureRecognizer();
            object receivedParameter = new object(); // Set to non-null to verify it gets overwritten
            Command cmd = new Command<object>(param => receivedParameter = param);
            var args = new DragEventArgs(new DataPackage());

            dropRec.DragLeaveCommand = cmd;
            dropRec.DragLeaveCommandParameter = null;

            // Act
            dropRec.SendDragLeave(args);

            // Assert
            Assert.Null(receivedParameter);
        }

        /// <summary>
        /// Tests that SendDragLeave invokes DragLeave event with correct sender and args when Parent is null.
        /// Verifies that 'this' is used as sender when Parent property is null.
        /// </summary>
        [Fact]
        public void SendDragLeave_ParentNull_InvokesEventWithThisAsSender()
        {
            // Arrange
            var dropRec = new DropGestureRecognizer();
            var args = new DragEventArgs(new DataPackage());
            object eventSender = null;
            DragEventArgs eventArgs = null;

            dropRec.DragLeave += (sender, e) =>
            {
                eventSender = sender;
                eventArgs = e;
            };

            // Act
            dropRec.SendDragLeave(args);

            // Assert
            Assert.Equal(dropRec, eventSender);
            Assert.Equal(args, eventArgs);
        }

        /// <summary>
        /// Tests that SendDragLeave invokes DragLeave event with Parent as sender when Parent is not null.
        /// Verifies the Parent ?? this logic for determining the event sender.
        /// </summary>
        [Fact]
        public void SendDragLeave_ParentNotNull_InvokesEventWithParentAsSender()
        {
            // Arrange
            var dropRec = new DropGestureRecognizer();
            var parentElement = new Label();
            var args = new DragEventArgs(new DataPackage());
            object eventSender = null;
            DragEventArgs eventArgs = null;

            parentElement.GestureRecognizers.Add(dropRec);
            dropRec.DragLeave += (sender, e) =>
            {
                eventSender = sender;
                eventArgs = e;
            };

            // Act
            dropRec.SendDragLeave(args);

            // Assert
            Assert.Equal(parentElement, eventSender);
            Assert.Equal(args, eventArgs);
        }

        /// <summary>
        /// Tests that SendDragLeave handles null args parameter without throwing.
        /// Verifies proper null handling for the args parameter.
        /// </summary>
        [Fact]
        public void SendDragLeave_ArgsNull_DoesNotThrow()
        {
            // Arrange
            var dropRec = new DropGestureRecognizer();
            var parameter = new object();
            Command cmd = new Command(() => { });
            object eventSender = null;
            DragEventArgs eventArgs = null;

            dropRec.DragLeaveCommand = cmd;
            dropRec.DragLeaveCommandParameter = parameter;
            dropRec.DragLeave += (sender, e) =>
            {
                eventSender = sender;
                eventArgs = e;
            };

            // Act & Assert
            dropRec.SendDragLeave(null);
            Assert.Equal(dropRec, eventSender);
            Assert.Null(eventArgs);
        }

        /// <summary>
        /// Tests that SendDragLeave executes both command and invokes event when both are configured.
        /// Verifies that both operations happen in the same method call.
        /// </summary>
        [Fact]
        public void SendDragLeave_BothCommandAndEvent_ExecutesBoth()
        {
            // Arrange
            var dropRec = new DropGestureRecognizer();
            var parameter = new object();
            var args = new DragEventArgs(new DataPackage());
            object commandExecuted = null;
            object eventSender = null;
            DragEventArgs eventArgs = null;

            Command cmd = new Command(() => commandExecuted = parameter);
            dropRec.DragLeaveCommand = cmd;
            dropRec.DragLeaveCommandParameter = parameter;
            dropRec.DragLeave += (sender, e) =>
            {
                eventSender = sender;
                eventArgs = e;
            };

            // Act
            dropRec.SendDragLeave(args);

            // Assert
            Assert.Equal(parameter, commandExecuted);
            Assert.Equal(dropRec, eventSender);
            Assert.Equal(args, eventArgs);
        }

        /// <summary>
        /// Tests that SendDragLeave invokes multiple event handlers when multiple subscribers exist.
        /// Verifies that all subscribed event handlers are called.
        /// </summary>
        [Fact]
        public void SendDragLeave_MultipleEventHandlers_InvokesAll()
        {
            // Arrange
            var dropRec = new DropGestureRecognizer();
            var args = new DragEventArgs(new DataPackage());
            int handlerCallCount = 0;

            dropRec.DragLeave += (sender, e) => handlerCallCount++;
            dropRec.DragLeave += (sender, e) => handlerCallCount++;

            // Act
            dropRec.SendDragLeave(args);

            // Assert
            Assert.Equal(2, handlerCallCount);
        }

        /// <summary>
        /// Tests that SendDrop returns early when AllowDrop is false, without executing any commands or processing.
        /// </summary>
        [Fact]
        public async Task SendDrop_AllowDropFalse_ReturnsEarly()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = false };
            var commandExecuted = false;
            var command = Substitute.For<ICommand>();
            command.When(x => x.Execute(Arg.Any<object>())).Do(_ => commandExecuted = true);

            dropGestureRecognizer.DropCommand = command;

            var dataPackage = new DataPackage();
            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert
            Assert.False(commandExecuted);
        }

        /// <summary>
        /// Tests that SendDrop executes the DropCommand when AllowDrop is true.
        /// </summary>
        [Fact]
        public async Task SendDrop_AllowDropTrue_ExecutesDropCommand()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var commandParameter = new object();
            var commandExecuted = false;
            var executedParameter = (object)null;

            var command = Substitute.For<ICommand>();
            command.When(x => x.Execute(Arg.Any<object>())).Do(callInfo =>
            {
                commandExecuted = true;
                executedParameter = callInfo.Arg<object>();
            });

            dropGestureRecognizer.DropCommand = command;
            dropGestureRecognizer.DropCommandParameter = commandParameter;

            var dataPackage = new DataPackage();
            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert
            Assert.True(commandExecuted);
            Assert.Equal(commandParameter, executedParameter);
        }

        /// <summary>
        /// Tests that SendDrop does not process further when args.Handled is true, but still executes commands and events.
        /// </summary>
        [Fact]
        public async Task SendDrop_HandledTrue_DoesNotProcessFurther()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var image = new Image();
            image.GestureRecognizers.Add(dropGestureRecognizer);

            var dataPackage = new DataPackage { Text = "test text" };
            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));
            dropEventArgs.Handled = true;

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert - Image source should not be set because Handled = true
            Assert.Null(image.Source);
        }

        /// <summary>
        /// Tests that SendDrop processes DragSource when present in internal properties and dragSource is IImageElement with null sourceTarget.
        /// </summary>
        [Fact]
        public async Task SendDrop_DragSourceWithImageElement_UsesImageElementSource()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var targetImage = new Image();
            targetImage.GestureRecognizers.Add(dropGestureRecognizer);

            var dragSourceImage = new Image();
            var testImageSource = ImageSource.FromFile("test.png");
            dragSourceImage.Source = testImageSource;

            var dataPackage = new DataPackage();
            dataPackage.PropertiesInternal["DragSource"] = dragSourceImage;

            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert
            Assert.Equal(testImageSource, targetImage.Source);
        }

        /// <summary>
        /// Tests that SendDrop gets string value from dragSource when text is null or whitespace.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SendDrop_DragSourceWithNullOrWhitespaceText_GetsStringValueFromDragSource(string text)
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var targetLabel = new Label();
            targetLabel.GestureRecognizers.Add(dropGestureRecognizer);

            var dragSourceLabel = new Label { Text = "DragSource Text" };

            var dataPackage = new DataPackage { Text = text };
            dataPackage.PropertiesInternal["DragSource"] = dragSourceLabel;

            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert
            Assert.Equal("DragSource Text", targetLabel.Text);
        }

        /// <summary>
        /// Tests that SendDrop sets Image.Source when Parent is Image.
        /// </summary>
        [Fact]
        public async Task SendDrop_ParentIsImage_SetsImageSource()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var image = new Image();
            image.GestureRecognizers.Add(dropGestureRecognizer);

            var testImageSource = ImageSource.FromFile("test.png");
            var dataPackage = new DataPackage { Image = testImageSource };
            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert
            Assert.Equal(testImageSource, image.Source);
        }

        /// <summary>
        /// Tests that SendDrop sets ImageButton.Source when Parent is ImageButton.
        /// </summary>
        [Fact]
        public async Task SendDrop_ParentIsImageButton_SetsImageButtonSource()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var imageButton = new ImageButton();
            imageButton.GestureRecognizers.Add(dropGestureRecognizer);

            var testImageSource = ImageSource.FromFile("test.png");
            var dataPackage = new DataPackage { Image = testImageSource };
            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert
            Assert.Equal(testImageSource, imageButton.Source);
        }

        /// <summary>
        /// Tests that SendDrop sets Button.ImageSource when Parent is Button.
        /// </summary>
        [Fact]
        public async Task SendDrop_ParentIsButton_SetsButtonImageSource()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var button = new Button();
            button.GestureRecognizers.Add(dropGestureRecognizer);

            var testImageSource = ImageSource.FromFile("test.png");
            var dataPackage = new DataPackage { Image = testImageSource };
            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert
            Assert.Equal(testImageSource, button.ImageSource);
        }

        /// <summary>
        /// Tests that SendDrop uses text as sourceTarget when Parent is IImageElement and sourceTarget is null.
        /// </summary>
        [Fact]
        public async Task SendDrop_ParentIsImageElementWithNullSourceTarget_UsesTextAsSourceTarget()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var image = new Image();
            image.GestureRecognizers.Add(dropGestureRecognizer);

            var dataPackage = new DataPackage { Text = "test.png" };
            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert - The text should be converted to ImageSource and set on the image
            Assert.NotNull(image.Source);
        }

        /// <summary>
        /// Tests that SendDrop calls TrySetValue on Parent with text.
        /// </summary>
        [Fact]
        public async Task SendDrop_CallsTrySetValueOnParent()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var entry = new Entry();
            entry.GestureRecognizers.Add(dropGestureRecognizer);

            var testText = "test text";
            var dataPackage = new DataPackage { Text = testText };
            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert
            Assert.Equal(testText, entry.Text);
        }

        /// <summary>
        /// Tests that SendDrop handles null DropCommand gracefully.
        /// </summary>
        [Fact]
        public async Task SendDrop_NullDropCommand_DoesNotThrow()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer
            {
                AllowDrop = true,
                DropCommand = null
            };

            var dataPackage = new DataPackage();
            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act & Assert - Should not throw
            await dropGestureRecognizer.SendDrop(dropEventArgs);
        }

        /// <summary>
        /// Tests that SendDrop handles the case when DragSource is not present in internal properties.
        /// </summary>
        [Fact]
        public async Task SendDrop_NoDragSourceInProperties_ProcessesNormally()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };
            var label = new Label();
            label.GestureRecognizers.Add(dropGestureRecognizer);

            var testText = "test text";
            var dataPackage = new DataPackage { Text = testText };
            // Intentionally not adding DragSource to properties

            var dropEventArgs = new DropEventArgs(new DataPackageView(dataPackage));

            // Act
            await dropGestureRecognizer.SendDrop(dropEventArgs);

            // Assert
            Assert.Equal(testText, label.Text);
        }

        /// <summary>
        /// Tests that SendDrop throws ArgumentNullException when args parameter is null.
        /// </summary>
        [Fact]
        public async Task SendDrop_NullArgs_ThrowsArgumentNullException()
        {
            // Arrange
            var dropGestureRecognizer = new DropGestureRecognizer { AllowDrop = true };

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(
                () => dropGestureRecognizer.SendDrop(null));
        }
    }
}