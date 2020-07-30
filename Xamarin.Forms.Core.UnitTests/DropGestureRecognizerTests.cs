using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DropGestureRecognizerTests : BaseTestFixture
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
			var dropRec = new DropGestureRecognizer();

			Command cmd = new Command(() => { });
			var parameter = new Object();
			dropRec.AllowDrop = true;
			dropRec.DragOverCommand = cmd;
			dropRec.DragOverCommandParameter = parameter;
			dropRec.DropCommand = cmd;
			dropRec.DropCommandParameter = parameter;

			Assert.AreEqual(true, dropRec.AllowDrop);
			Assert.AreEqual(cmd, dropRec.DragOverCommand);
			Assert.AreEqual(parameter, dropRec.DragOverCommandParameter);
			Assert.AreEqual(cmd, dropRec.DropCommand);
			Assert.AreEqual(parameter, dropRec.DropCommandParameter);
		}

		[Test]
		public void DragOverCommandFires()
		{
			var dropRec = new DropGestureRecognizer();
			var parameter = new Object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			dropRec.DragOverCommand = cmd;
			dropRec.DragOverCommandParameter = parameter;
			dropRec.SendDragOver(new DragEventArgs(new DataPackage()));

			Assert.AreEqual(parameter, commandExecuted);
		}

		[Test]
		public void DropCommandFires()
		{
			var dropRec = new DropGestureRecognizer();
			var parameter = new Object();
			object commandExecuted = null;
			Command cmd = new Command(() => commandExecuted = parameter);

			dropRec.DropCommand = cmd;
			dropRec.DropCommandParameter = parameter;
			dropRec.SendDrop(new DropEventArgs(new DataPackageView(new DataPackage())), new Label());

			Assert.AreEqual(commandExecuted, parameter);
		}
	}
}
