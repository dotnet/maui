using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class DropGestureRecognizerTests : BaseTestFixture
	{
		[Fact]
		public void DataPackageOperationsAreIndependentFlags()
		{
			Assert.True(typeof(DataPackageOperation).IsDefined(typeof(FlagsAttribute), false));
			Assert.Equal(0, (int)DataPackageOperation.None);
			Assert.Equal(1, (int)DataPackageOperation.Copy);
			Assert.Equal(2, (int)DataPackageOperation.Move);
			Assert.Equal(3, (int)(DataPackageOperation.Copy | DataPackageOperation.Move));
			Assert.Equal(DataPackageOperation.None, DataPackageOperation.Copy & DataPackageOperation.Move);
		}

		[Fact]
		public void DragOverDefaultsToCopy()
		{
			var args = new DragEventArgs(new DataPackage());
			var dropRec = new DropGestureRecognizer();
			Assert.Equal(DataPackageOperation.Copy, args.AcceptedOperation);

			dropRec.SendDragOver(args);

			Assert.Equal(DataPackageOperation.Copy, args.AcceptedOperation);
		}

		[Theory]
		[InlineData(DataPackageOperation.None)]
		[InlineData(DataPackageOperation.Copy)]
		[InlineData(DataPackageOperation.Move)]
		[InlineData(DataPackageOperation.Copy | DataPackageOperation.Move)]
		public void DragOverPreservesAcceptedOperation(DataPackageOperation operation)
		{
			var args = new DragEventArgs(new DataPackage()) { AcceptedOperation = operation };
			var dropRec = new DropGestureRecognizer();

			dropRec.SendDragOver(args);

			Assert.Equal(operation, args.AcceptedOperation);
		}

		[Theory]
		[InlineData(DataPackageOperation.None)]
		[InlineData(DataPackageOperation.Copy)]
		[InlineData(DataPackageOperation.Move)]
		[InlineData(DataPackageOperation.Copy | DataPackageOperation.Move)]
		public void DragOverCanUpdateAcceptedOperation(DataPackageOperation operation)
		{
			var package = new DataPackage();
			var args = new DragEventArgs(package);
			var dropRec = new DropGestureRecognizer();
			var raised = false;
			dropRec.DragOver += (_, e) =>
			{
				raised = true;
				Assert.Same(args, e);
				Assert.Same(package, e.Data);
				e.AcceptedOperation = operation;
			};

			dropRec.SendDragOver(args);

			Assert.True(raised);
			Assert.Equal(operation, args.AcceptedOperation);
		}

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

		[Fact]
		public void SendDragLeaveThrowsForNullArgs()
		{
			var dropRec = new DropGestureRecognizer();

			Assert.Throws<ArgumentNullException>(() => dropRec.SendDragLeave(null));
		}

		[Fact]
		public void SendDropThrowsSynchronouslyForNullArgs()
		{
			var dropRec = new DropGestureRecognizer();

			Assert.Throws<ArgumentNullException>(() =>
			{
				_ = dropRec.SendDrop(null);
			});
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
	}
}
