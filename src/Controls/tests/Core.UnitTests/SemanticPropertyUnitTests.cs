using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class SemanticPropertyUnitTests : BaseTestFixture
	{
		[Fact]
		public void FakeBindSemanticProperties_PropertiesPropagate()
		{
			Grid source = new Grid();
			Button dest = new Button();
			SemanticProperties.FakeBindSemanticProperties(source, dest);
			SemanticProperties.SetDescription(source, "test");

			var destDescription = SemanticProperties.GetDescription(dest);
			Assert.Equal("test", destDescription);
		}

		[Fact]
		public void FakeBindSemanticProperties_PropertiesStopPropagating()
		{
			Grid source = new Grid();
			Button dest = new Button();
			var disconnect = SemanticProperties.FakeBindSemanticProperties(source, dest);
			SemanticProperties.SetDescription(source, "test");
			disconnect.Dispose();

			SemanticProperties.SetDescription(source, "second");
			var destDescription = SemanticProperties.GetDescription(dest);
			Assert.Equal("test", destDescription);
		}

		[Fact]
		public void FlyoutItemTitlePropagatesToTemplatesSemanticProperties()
		{
			Shell shell = new Shell();
			FlyoutItem flyoutItem = new FlyoutItem() { Title = "title" };
			shell.Items.Add(flyoutItem);

			var content = (shell as IShellController).GetFlyoutItemDataTemplate(flyoutItem).CreateContent() as BindableObject;
			content.BindingContext = flyoutItem;

			var destDescription = SemanticProperties.GetDescription(content);
			Assert.Equal("title", destDescription);

			flyoutItem.Title = "new title";
			destDescription = SemanticProperties.GetDescription(content);
			Assert.Equal("new title", destDescription);
		}

		[Fact]
		public void FlyoutItemSemanticPropertiesPropagateOverTitle()
		{
			Shell shell = new Shell();
			FlyoutItem flyoutItem = new FlyoutItem() { Title = "title" };
			SemanticProperties.SetDescription(flyoutItem, "semantic title");

			shell.Items.Add(flyoutItem);

			var content = (shell as IShellController).GetFlyoutItemDataTemplate(flyoutItem).CreateContent() as BindableObject;
			content.BindingContext = flyoutItem;

			var destDescription = SemanticProperties.GetDescription(content);
			Assert.Equal("semantic title", destDescription);

			flyoutItem.Title = "new title";
			destDescription = SemanticProperties.GetDescription(content);
			Assert.Equal("semantic title", destDescription);
		}

		[Fact]
		public void SemanticPropertyUpdatesBeforePropertyChangePropagates()
		{
			var semanticHandlerStub = new SemanticHandlerStub();
			Button button = new Button();
			button.Handler = semanticHandlerStub;

			SemanticProperties.SetDescription(button, "start");
			SemanticProperties.SetHeadingLevel(button, SemanticHeadingLevel.None);
			SemanticProperties.SetHint(button, "startHint");

			string desc = string.Empty;
			string hint = String.Empty;
			SemanticHeadingLevel semanticHeadingLevel = SemanticHeadingLevel.None;

			button.PropertyChanged += (_, args) =>
			{
				desc = (button as IView).Semantics.Description;
				hint = (button as IView).Semantics.Hint;
				semanticHeadingLevel = (button as IView).Semantics.HeadingLevel;
			};

			SemanticProperties.SetDescription(button, "finish");
			SemanticProperties.SetHeadingLevel(button, SemanticHeadingLevel.Level8);
			SemanticProperties.SetHint(button, "finishHint");

			Assert.Equal("finish", desc);
			Assert.Equal("finishHint", hint);
			Assert.Equal(SemanticHeadingLevel.Level8, semanticHeadingLevel);

			Assert.Equal("finish", semanticHandlerStub.SemanticDescription);
			Assert.Equal("finishHint", semanticHandlerStub.SemanticHint);
			Assert.Equal(SemanticHeadingLevel.Level8, semanticHandlerStub.SemanticHeadingLevel);
		}

		class SemanticHandlerStub : HandlerStub
		{
			public string SemanticDescription { get; private set; }
			public string SemanticHint { get; private set; }
			public SemanticHeadingLevel SemanticHeadingLevel { get; private set; }

			public override void UpdateValue(string property)
			{
				if ((VirtualView as IView).Semantics is Semantics semantics)
				{
					SemanticDescription = semantics.Description;
					SemanticHint = semantics.Hint;
					SemanticHeadingLevel = semantics.HeadingLevel;
				}

				base.UpdateValue(property);
			}
		}
	}
}
