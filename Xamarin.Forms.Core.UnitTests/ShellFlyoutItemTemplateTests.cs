using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ShellFlyoutItemTemplateTests : ShellTestBase
	{
		[Test]
		public void FlyoutItemDefaultStylesApplied()
		{
			Shell shell = new Shell();
			var shellItem = CreateShellItem();

			shell.Items.Add(shellItem);

			var element = GetFlyoutItemDataTemplateElement<Element>(shell, shellItem);
			var label = element.LogicalChildren.OfType<Label>().First();
			Assert.AreEqual(TextAlignment.Center, label.VerticalTextAlignment);
		}


		[Test]
		public void FlyoutItemLabelStyleCustom()
		{
			var classStyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start }
				},
				Class = "fooClass",
			};

			Shell shell = new Shell();
			shell.Resources = new ResourceDictionary { classStyle };
			var shellItem = CreateShellItem();
			shellItem.StyleClass = new[] { "fooClass" };

			shell.Items.Add(shellItem);

			var element = GetFlyoutItemDataTemplateElement<Element>(shell, shellItem);
			var label = element.LogicalChildren.OfType<Label>().First();
			Assert.AreEqual(TextAlignment.Start, label.VerticalTextAlignment);
		}

		[Test]
		public void MenuItemLabelStyleCustom()
		{
			var classStyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start }
				},
				Class = "fooClass",
			};

			Shell shell = new Shell();
			shell.Resources = new ResourceDictionary { classStyle };
			var shellItem = CreateShellItem();
			var menuItem = new MenuItem();
			var shellMenuItem = new MenuShellItem(menuItem);
			menuItem.StyleClass = new[] { "fooClass" };
			shell.Items.Add(shellItem);
			shell.Items.Add(shellMenuItem);

			var label = GetFlyoutItemDataTemplateElement<Label>(shell, shellMenuItem);
			Assert.AreEqual(TextAlignment.Start, label.VerticalTextAlignment);
		}

		[Test]
		public void FlyoutItemLabelStyleDefault()
		{
			var classStyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start }
				},
				Class = FlyoutItem.LabelStyle,
			};

			Shell shell = new Shell();
			shell.Resources = new ResourceDictionary { classStyle };
			var shellItem = CreateShellItem();

			shell.Items.Add(shellItem);

			var label = GetFlyoutItemDataTemplateElement<Label>(shell, shellItem);
			Assert.AreEqual(TextAlignment.Start, label.VerticalTextAlignment);
		}

		[Test]
		public void FlyoutItemDefaultTemplates()
		{
			Shell shell = new Shell();
			IShellController sc = (IShellController)shell;
			shell.MenuItemTemplate = new DataTemplate(() => new Label() { Text = "MenuItemTemplate" });
			shell.ItemTemplate = new DataTemplate(() => new Label() { Text = "ItemTemplate" });

			var shellItem = CreateShellItem();
			var menuItem = new MenuShellItem(new MenuItem());
			shell.Items.Add(shellItem);
			shell.Items.Add(menuItem);


			DataTemplate triggerDefault = shell.ItemTemplate;
			triggerDefault = shell.MenuItemTemplate;

			Assert.AreEqual("ItemTemplate", GetFlyoutItemDataTemplateElement<Label>(shell, shellItem).Text);
			Assert.AreEqual("MenuItemTemplate", GetFlyoutItemDataTemplateElement<Label>(shell, menuItem).Text);
			Assert.AreEqual("MenuItemTemplate", GetFlyoutItemDataTemplateElement<Label>(shell, menuItem.MenuItem).Text);
		}

		[Test]
		public void FlyoutItemLabelVisualStateManager()
		{
			var groups = new VisualStateGroupList();
			var commonGroup = new VisualStateGroup();
			commonGroup.Name = "CommonStates";
			groups.Add(commonGroup);
			var normalState = new VisualState();
			normalState.Name = "Normal";
			var selectedState = new VisualState();
			selectedState.Name = "Selected";

			normalState.Setters.Add(new Setter
			{
				Property = Label.BackgroundColorProperty,
				Value = Color.Red,
				TargetName = "FlyoutItemLabel"
			});

			selectedState.Setters.Add(new Setter
			{
				Property = Label.BackgroundColorProperty,
				Value = Color.Green,
				TargetName = "FlyoutItemLabel"
			});

			commonGroup.States.Add(normalState);
			commonGroup.States.Add(selectedState);

			var classStyle = new Style(typeof(Grid))
			{
				Setters = {
					new Setter
					{
						Property = VisualStateManager.VisualStateGroupsProperty,
						Value = groups
					}
				},
				Class = FlyoutItem.LayoutStyle,
			};

			Shell shell = new Shell();
			shell.Resources = new ResourceDictionary { classStyle };
			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);
			var grid = GetFlyoutItemDataTemplateElement<Grid>(shell, shellItem);
			var label = grid.LogicalChildren.OfType<Label>().First();

			Assert.AreEqual(Color.Red, label.BackgroundColor);
			Assert.IsTrue(VisualStateManager.GoToState(grid, "Selected"));
			Assert.AreEqual(Color.Green, label.BackgroundColor);
		}



		[Test]
		public void BindingContextFlyoutItems()
		{
			var flyoutItemVM = new TestShellViewModel() { Text = "Dog" };

			Shell shell = new Shell();
			shell.BindingContext = flyoutItemVM;

			var item1 = CreateShellItem<FlyoutItem>();
			item1.SetBinding(FlyoutItem.TitleProperty, "Text", mode: BindingMode.TwoWay);
			shell.Items.Add(item1);

			MenuItem menuItem = new MenuItem();
			menuItem.SetBinding(MenuItem.TextProperty, "Text", mode: BindingMode.TwoWay);
			shell.Items.Add(menuItem);

			var flyoutItemLabel = GetFlyoutItemDataTemplateElement<Label>(shell, shell.Items[0]);
			var menuItemLabel = GetFlyoutItemDataTemplateElement<Label>(shell, shell.Items[1]);

			Assert.AreEqual("Dog", flyoutItemLabel.Text);
			Assert.AreEqual("Dog", menuItemLabel.Text);

			flyoutItemVM.Text = "Cat";

			Assert.AreEqual("Cat", flyoutItemLabel.Text);
			Assert.AreEqual("Cat", menuItemLabel.Text);

		}

		[Test]
		public void BindingContextSetsCorrectlyWhenUsingAsMultipleItemAndImplicitlyGeneratedShellSections()
		{
			Shell shell = new Shell();
			FlyoutItem item = new FlyoutItem() { FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems };
			ShellContent shellContent1 = new ShellContent();

			ShellContent shellContent2 = new ShellContent();

			item.Items.Add(shellContent1);
			item.Items.Add(shellContent2);
			shell.Items.Add(item);

			var vm = new TestShellViewModel();
			vm.SubViewModel = new TestShellViewModel() { Text = "Item1" };
			vm.SubViewModel2 = new TestShellViewModel() { Text = "Item2" };
			shell.BindingContext = vm;

			shellContent1.SetBinding(BindableObject.BindingContextProperty, "SubViewModel");
			shellContent2.SetBinding(BindableObject.BindingContextProperty, "SubViewModel2");

			shell.ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label();

				label.SetBinding(Label.TextProperty, "BindingContext.Text");
				return label;
			});

			var flyoutItems = (shell as IShellController).GenerateFlyoutGrouping();


			var label1 = GetFlyoutItemDataTemplateElement<Label>(shell, flyoutItems[0][0]);
			var label2 = GetFlyoutItemDataTemplateElement<Label>(shell, flyoutItems[0][1]);

			Assert.AreEqual(label1.BindingContext, shellContent1);
			Assert.AreEqual(label2.BindingContext, shellContent2);

			Assert.AreEqual("Item1", label1.Text);
			Assert.AreEqual("Item2", label2.Text);
		}


		T GetFlyoutItemDataTemplateElement<T>(Shell shell, BindableObject bo)
			where T : class
		{
			var content = (shell as IShellController).GetFlyoutItemDataTemplate(bo).CreateContent();

			if (content is BindableObject bindableContent)
			{
				if (bo is MenuItem mi)
					bindableContent.BindingContext = mi.Parent;
				else
					bindableContent.BindingContext = bo;
			}

			if (content is Element e)
			{
				e.Parent = shell;
			}
			else
			{
				e = null;
			}

			if (content is T t)
				return t;

			if (e == null)
				return default(T);

			return e.LogicalChildren.OfType<T>().First();
		}


		//[Test]
		//public void FlyoutItemLabelStyleCanBeChangedAfterRendered()
		//{
		//	var classStyle = new Style(typeof(Label))
		//	{
		//		Setters = {
		//			new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start }
		//		},
		//		Class = "fooClass",
		//	};

		//	Shell shell = new Shell();
		//	shell.Resources = new ResourceDictionary { classStyle };
		//	var shellItem = CreateShellItem();

		//	shell.Items.Add(shellItem);

		//	var flyoutItemTemplate = (shell as IShellController).GetFlyoutItemDataTemplate(shellItem);
		//	var thing = (Element)flyoutItemTemplate.CreateContent();
		//	thing.Parent = shell;

		//	var label = thing.LogicalChildren.OfType<Label>().First();
		//	Assert.AreEqual(TextAlignment.Center, label.VerticalTextAlignment);
		//	shellItem.StyleClass = new[] { "fooClass" };
		//	Assert.AreEqual(TextAlignment.Start, label.VerticalTextAlignment);
		//}

		//[Test]
		//public void MenuItemLabelStyleCanBeChangedAfterRendered()
		//{
		//	var classStyle = new Style(typeof(Label))
		//	{
		//		Setters = {
		//			new Setter { Property = Label.VerticalTextAlignmentProperty, Value = TextAlignment.Start }
		//		},
		//		Class = "fooClass",
		//	};

		//	Shell shell = new Shell();
		//	shell.Resources = new ResourceDictionary { classStyle };
		//	var shellItem = CreateShellItem();
		//	var menuItem = new MenuItem();
		//	var shellMenuItem = new MenuShellItem(menuItem);
		//	shell.Items.Add(shellItem);
		//	shell.Items.Add(shellMenuItem);

		//	var flyoutItemTemplate = (shell as IShellController).GetFlyoutItemDataTemplate(shellMenuItem);
		//	var thing = (Element)flyoutItemTemplate.CreateContent();
		//	thing.Parent = shell;

		//	var label = thing.LogicalChildren.OfType<Label>().First();
		//	Assert.AreEqual(TextAlignment.Center, label.VerticalTextAlignment);
		//	menuItem.StyleClass = new[] { "fooClass" };
		//	Assert.AreEqual(TextAlignment.Start, label.VerticalTextAlignment);
		//}
	}
}