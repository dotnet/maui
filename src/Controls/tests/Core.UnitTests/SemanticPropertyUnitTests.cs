using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class SemanticPropertyUnitTests : BaseTestFixture
	{
		[Test]
		public void FakeBindSemanticProperties_PropertiesPropagate()
		{
			Grid source = new Grid();
			Button dest = new Button();
			SemanticProperties.FakeBindSemanticProperties(source, dest);
			SemanticProperties.SetDescription(source, "test");

			var destDescription = SemanticProperties.GetDescription(dest);
			Assert.AreEqual("test", destDescription);
		}

		[Test]
		public void FakeBindSemanticProperties_PropertiesStopPropagating()
		{
			Grid source = new Grid();
			Button dest = new Button();
			var disconnect = SemanticProperties.FakeBindSemanticProperties(source, dest);
			SemanticProperties.SetDescription(source, "test");
			disconnect.Dispose();

			SemanticProperties.SetDescription(source, "second");
			var destDescription = SemanticProperties.GetDescription(dest);
			Assert.AreEqual("test", destDescription);
		}

		[Test]
		public void FlyoutItemTitlePropagatesToTemplatesSemanticProperties()
		{
			Shell shell = new Shell();
			FlyoutItem flyoutItem = new FlyoutItem() { Title = "title" };
			shell.Items.Add(flyoutItem);

			var content = (shell as IShellController).GetFlyoutItemDataTemplate(flyoutItem).CreateContent() as BindableObject;
			content.BindingContext = flyoutItem;

			var destDescription = SemanticProperties.GetDescription(content);
			Assert.AreEqual("title", destDescription);

			flyoutItem.Title = "new title";
			destDescription = SemanticProperties.GetDescription(content);
			Assert.AreEqual("new title", destDescription);
		}

		[Test]
		public void FlyoutItemSemanticPropertiesPropagateOverTitle()
		{
			Shell shell = new Shell();
			FlyoutItem flyoutItem = new FlyoutItem() { Title = "title" };
			SemanticProperties.SetDescription(flyoutItem, "semantic title");

			shell.Items.Add(flyoutItem);

			var content = (shell as IShellController).GetFlyoutItemDataTemplate(flyoutItem).CreateContent() as BindableObject;
			content.BindingContext = flyoutItem;

			var destDescription = SemanticProperties.GetDescription(content);
			Assert.AreEqual("semantic title", destDescription);

			flyoutItem.Title = "new title";
			destDescription = SemanticProperties.GetDescription(content);
			Assert.AreEqual("semantic title", destDescription);
		}
	}
}
