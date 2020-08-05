using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DragGestureRecognizerTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
			Device.SetFlags(new[] { ExperimentalFlags.DragAndDropExperimental });
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
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

			Assert.AreEqual(true, dragRec.CanDrag);
			Assert.AreEqual(cmd, dragRec.DragStartingCommand);
			Assert.AreEqual(parameter, dragRec.DragStartingCommandParameter);
			Assert.AreEqual(cmd, dragRec.DropCompletedCommand);
			Assert.AreEqual(parameter, dragRec.DropCompletedCommandParameter);
		}

		[Test]
		public void DragStartingCommandFires()
		{
			var dragRec = new DragGestureRecognizer();
			var parameter = new Object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			dragRec.DragStartingCommand = cmd;
			dragRec.DragStartingCommandParameter = parameter;
			dragRec.SendDragStarting(new Label());

			Assert.AreEqual(commandExecuted, parameter);
		}

		[Test]
		public void DropCompletedCommandFires()
		{
			var dragRec = new DragGestureRecognizer();
			var parameter = new Object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			dragRec.DropCompletedCommand = cmd;
			dragRec.DropCompletedCommandParameter = parameter;
			dragRec.SendDropCompleted(new DropCompletedEventArgs());

			Assert.AreEqual(commandExecuted, parameter);
		}

		[TestCase(typeof(Entry), "EntryTest")]
		[TestCase(typeof(Label), "LabelTest")]
		[TestCase(typeof(Editor), "EditorTest")]
		[TestCase(typeof(TimePicker), "01:00:00")]
		[TestCase(typeof(DatePicker), "12/12/2020 12:00:00 AM")]
		[TestCase(typeof(CheckBox), "True")]
		[TestCase(typeof(Switch), "True")]
		[TestCase(typeof(RadioButton), "True")]
		public void TextPackageCorrectlyExtractedFromCompatibleElement(Type fieldType, string result)
		{
			var dragRec = new DragGestureRecognizer();
			var element = (VisualElement)Activator.CreateInstance(fieldType);
			Assert.IsTrue(element.TrySetValue(result));
			var args = dragRec.SendDragStarting(element);
			Assert.AreEqual(result, args.Data.Text);
		}

		[Test]
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
			Assert.AreNotEqual(args.Data.Text, testString);
		}
	}
}
