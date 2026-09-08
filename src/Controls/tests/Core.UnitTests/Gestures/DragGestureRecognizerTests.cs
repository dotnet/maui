using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	}
}
