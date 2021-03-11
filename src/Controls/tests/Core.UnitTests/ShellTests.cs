using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ShellTests : ShellTestBase
	{

		[Test]
		public void DefaultState()
		{
			var shell = new Shell();

			Assert.IsEmpty(shell.Items);
		}

		[Test]
		public void CurrentItemAutoSets()
		{
			var shell = new Shell();
			var shellItem = new ShellItem();
			var shellSection = new ShellSection();
			var shellContent = new ShellContent { Content = new ContentPage() };
			shellSection.Items.Add(shellContent);
			shellItem.Items.Add(shellSection);
			shell.Items.Add(shellItem);

			Assert.That(shell.CurrentItem, Is.EqualTo(shellItem));
		}


		[TestCase(true)]
		[TestCase(false)]
		public void SetCurrentItemWithImplicitlyWrappedShellContent(bool useShellContent)
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			BaseShellItem shellElement = null;

			if (useShellContent)
				shellElement = CreateShellContent(shellContentRoute: "TestMe");
			else
				shellElement = CreateShellSection(shellSectionRoute: "TestMe");

			if (useShellContent)
				shell.Items.Add((ShellContent)shellElement);
			else
				shell.Items.Add((ShellSection)shellElement);

			var item2 = shell.Items[1];

			Assert.AreEqual(FindParentOfType<ShellItem>(shellElement), item2);

			if (useShellContent)
				shell.CurrentItem = (ShellContent)shellElement;
			else
				shell.CurrentItem = (ShellSection)shellElement;

			Assert.AreEqual(2, shell.Items.Count);
			Assert.AreEqual(FindParentOfType<ShellItem>(shellElement), item2);
			Assert.AreEqual(item2, shell.CurrentItem);
		}


		[Test]
		public void SetCurrentItemAddsToShellCollection()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem();
			var shellSection = CreateShellSection();
			var shellContent = CreateShellContent();

			shell.CurrentItem = shellItem;
			Assert.IsTrue(shell.Items.Contains(shellItem));
			Assert.AreEqual(shell.CurrentItem, shellItem);

			shell.CurrentItem = shellSection;
			Assert.AreEqual(shell.CurrentItem, shellSection.Parent);

			shell.CurrentItem = shellContent;
			Assert.AreEqual(shell.CurrentItem, shellContent.Parent.Parent);
		}


		[Test]
		public void ShellChildrenBindingContext()
		{
			var shell = new Shell();

			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);

			object viewModel = new object();
			shell.BindingContext = viewModel;

			Assert.AreSame(shell.BindingContext, viewModel);
			Assert.AreSame(shellItem.BindingContext, viewModel);
			Assert.AreSame(shellItem.Items[0].BindingContext, viewModel);
			Assert.AreSame(shellItem.Items[0].Items[0].BindingContext, viewModel);
			Assert.AreSame((shellItem.Items[0].Items[0].Content as BindableObject).BindingContext, viewModel);
		}

		[Test]
		public void ShellPropagateBindingContextWhenAddingNewShellItem()
		{
			var shell = new Shell();

			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);

			Assert.AreSame(shellItem.BindingContext, viewModel);
			Assert.AreSame(shellItem.Items[0].BindingContext, viewModel);
			Assert.AreSame(shellItem.Items[0].Items[0].BindingContext, viewModel);
			Assert.AreSame((shellItem.Items[0].Items[0].Content as BindableObject).BindingContext, viewModel);
		}

		[Test]
		public void ShellPropagateBindingContextWhenAddingNewShellSection()
		{
			var shell = new Shell();

			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var shellSection = CreateShellSection();
			shell.Items[0].Items.Add(shellSection);

			Assert.AreSame(shellSection.BindingContext, viewModel);
			Assert.AreSame(GetItems(shellSection)[0].BindingContext, viewModel);
			Assert.AreSame((GetItems(shellSection)[0].Content as BindableObject).BindingContext, viewModel);
		}

		[Test]
		public void ShellPropagateBindingContextWhenAddingNewShellContent()
		{
			var shell = new Shell();

			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var shellContent = CreateShellContent();
			shell.Items[0].Items[0].Items.Add(shellContent);

			Assert.AreSame(shellContent.BindingContext, viewModel);
			Assert.AreSame((shellContent.Content as BindableObject).BindingContext, viewModel);
		}

		[Test]
		public void ShellPropagateBindingContextWhenChangingContent()
		{
			var shell = new Shell();

			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var contentPage = new ContentPage();

			shell.Items[0].Items[0].Items[0].Content = contentPage;
			Assert.AreSame(contentPage.BindingContext, viewModel);
		}

		[Test]
		public async Task ShellPropagateBindingContextWhenPushingContent()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var contentPage = new ContentPage();
			await shell.Navigation.PushAsync(contentPage);

			Assert.AreSame(contentPage.BindingContext, viewModel);
		}

		[Test]
		public void NavigationProxyWireUpTest()
		{
			var page = new ContentPage();
			var shell = new Shell();
			var shellItem = new ShellItem();
			var shellSection = new ShellSection();
			var shellContent = new ShellContent { Content = page };
			shellSection.Items.Add(shellContent);
			shellItem.Items.Add(shellSection);
			shell.Items.Add(shellItem);

			NavigationProxy proxy = page.NavigationProxy.Inner as NavigationProxy;
			Assert.IsNotNull(proxy);

			var shellProxy = proxy.Inner;
			Assert.IsNotNull(shellProxy);
		}


		[Test]
		public void CurrentItemDoesNotChangeOnSecondAdd()
		{
			var shell = new Shell();
			var shellItem = new ShellItem();
			var shellSection = new ShellSection();
			var shellContent = new ShellContent { Content = new ContentPage() };
			shellSection.Items.Add(shellContent);
			shellItem.Items.Add(shellSection);
			shell.Items.Add(shellItem);

			Assume.That(shell.CurrentItem, Is.EqualTo(shellItem));

			shell.Items.Add(new ShellItem());

			Assert.AreEqual(shellItem, shell.CurrentItem);
		}

		[Test]
		public void SimpleGoTo()
		{
			var shell = new Shell();

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabtwo = MakeSimpleShellSection("tabtwo", "content");
			var tabthree = MakeSimpleShellSection("tabthree", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content");

			one.Items.Add(tabone);
			one.Items.Add(tabtwo);

			two.Items.Add(tabthree);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//one/tabone/content"));

			shell.GoToAsync(new ShellNavigationState("//two/tabfour/"));

			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//two/tabfour/content"));
		}

		[Test]
		public async Task CaseIgnoreRouting()
		{
			var routes = new[] { "Tab1", "TAB2", "@-_-@", "+:~", "=%", "Super_Simple+-Route.doc", "1/2", @"1\2/3", "app://tab" };

			foreach (var route in routes)
			{
				var formattedRoute = Routing.FormatRoute(route);
				Routing.RegisterRoute(formattedRoute, typeof(ShellItem));

				var content1 = Routing.GetOrCreateContent(formattedRoute);
				Assert.IsNotNull(content1);
				Assert.AreEqual(Routing.GetRoute(content1), formattedRoute);
			}

			Assert.Catch(typeof(ArgumentException), () => Routing.RegisterRoute("app://IMPL_tab21", typeof(ShellItem)));

			Assert.Catch(typeof(ArgumentException), () => Routing.RegisterRoute(@"app:\\IMPL_tab21", typeof(ShellItem)));

			Assert.Catch(typeof(ArgumentException), () => Routing.RegisterRoute(string.Empty, typeof(ShellItem)));

			Assert.Catch(typeof(ArgumentNullException), () => Routing.RegisterRoute(null, typeof(ShellItem)));

			Assert.Catch(typeof(ArgumentException), () => Routing.RegisterRoute("tab1/IMPL_tab11", typeof(ShellItem)));

			Assert.Catch(typeof(ArgumentException), () => Routing.RegisterRoute("IMPL_shell", typeof(ShellItem)));

			Assert.Catch(typeof(ArgumentException), () => Routing.RegisterRoute("app://tab2/IMPL_tab21", typeof(ShellItem)));
		}

		[Test]
		public async Task FailWhenAddingDuplicatedRouting()
		{
			var route = "dogs";
			Routing.RegisterRoute(route, typeof(ShellItem));

			Assert.Catch(typeof(ArgumentException), () => Routing.RegisterRoute("dogs", typeof(ContentPage)));
		}

		[Test]
		public async Task SucceedWhenAddingDuplicateRouteOfSameType()
		{
			var route = "dogs";
			Routing.RegisterRoute(route, typeof(ShellItem));
			Routing.RegisterRoute(route, typeof(ShellItem));
		}

		[Test]
		public async Task RelativeGoTo()
		{
			Routing.RegisterRoute("RelativeGoTo_Page1", typeof(ContentPage));
			Routing.RegisterRoute("RelativeGoTo_Page2", typeof(ContentPage));

			var shell = new Shell
			{
			};

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tab11 = MakeSimpleShellSection("tab11", "content");
			var tab12 = MakeSimpleShellSection("tab12", "content");
			var tab21 = MakeSimpleShellSection("tab21", "content");
			var tab22 = MakeSimpleShellSection("tab22", "content");
			var tab23 = MakeSimpleShellSection("tab23", "content");

			one.Items.Add(tab11);
			one.Items.Add(tab12);

			two.Items.Add(tab21);
			two.Items.Add(tab22);
			two.Items.Add(tab23);

			shell.Items.Add(one);
			shell.Items.Add(two);

			await shell.GoToAsync("//two/tab21/");

			await shell.NavigationManager.GoToAsync("/tab22", false, true);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//two/tab22/content"));

			await shell.NavigationManager.GoToAsync("tab21", false, true);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//two/tab21/content"));

			await shell.NavigationManager.GoToAsync("/tab23", false, true);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//two/tab23/content"));

			await shell.GoToAsync("RelativeGoTo_Page1", false);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//two/tab23/content/RelativeGoTo_Page1"));

			await shell.GoToAsync("../RelativeGoTo_Page2", false);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//two/tab23/content/RelativeGoTo_Page2"));

			await shell.GoToAsync("..", false);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//two/tab23/content"));

			/*
			 * removing support for .. notation for now
			await shell.GoToAsync("../one/tab11");
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/one/tab11/content/"));

			await shell.GoToAsync("/eee/hm../../../../two/../one/../two/tab21");
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tab21/content/"));

			await shell.GoToAsync(new ShellNavigationState("../one/tab11"));
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/one/tab11/content/"));

			await shell.GoToAsync(new ShellNavigationState($"../two/tab23/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tab23/content/"));
			Assert.AreEqual("1234", (two.CurrentItem.CurrentItem.Content as ShellTestPage).SomeQueryParameter);

			await shell.GoToAsync(new ShellNavigationState($"../one/tab11#fragment"));
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/one/tab11/content/"));
			*/
		}


		[Test]
		public async Task DotDotAdheresToAnimationParameter()
		{
			Routing.RegisterRoute(nameof(DotDotAdheresToAnimationParameter), typeof(ContentPage));
			var shellContent = new ShellContent();
			var shell = new TestShell(new TestFlyoutItem(new TestShellSection(shellContent)));
			await shell.GoToAsync(nameof(DotDotAdheresToAnimationParameter));
			await shell.GoToAsync("..", true);
			Assert.IsTrue(shell.LastPopWasAnimated);
		}

		[Test]
		public async Task DefaultRoutesMaintainedIfThatsAllThereIs()
		{
			Routing.RegisterRoute(nameof(DefaultRoutesMaintainedIfThatsAllThereIs), typeof(ContentPage));
			var shell = new Shell();
			var shellContent = new ShellContent();
			FlyoutItem flyoutItem = new FlyoutItem()
			{
				Items =
				{
					shellContent
				}
			};
			shell.Items.Add(flyoutItem);

			await shell.GoToAsync(nameof(DefaultRoutesMaintainedIfThatsAllThereIs));
			Assume.That(shell.CurrentState.Location.ToString(), Is.EqualTo($"//{Routing.GetRoute(shellContent)}/{nameof(DefaultRoutesMaintainedIfThatsAllThereIs)}"));
			await shell.GoToAsync("..");
		}

		[Test]
		public async Task RoutePathDefaultRemovalWithGlobalRoutesKeepsOneDefaultRoute()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			Routing.RegisterRoute(nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneDefaultRoute), typeof(ContentPage));
			await shell.GoToAsync(nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneDefaultRoute));

			// If all routes on the shell are default we still need to make sure it appends something that represents where you are in the
			// shell structure
			Assert.AreNotEqual($"//{nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneDefaultRoute)}", shell.CurrentState.Location.ToString());
		}


		[Test]
		public async Task RoutePathDefaultRemovalWithGlobalRoutesKeepsOneNamedRoute()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "content"));

			Routing.RegisterRoute(nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneNamedRoute), typeof(ContentPage));
			await shell.GoToAsync(nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneNamedRoute));

			Assert.AreEqual($"//content/{nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneNamedRoute)}", shell.CurrentState.Location.ToString());
		}



		[Test]
		public async Task OnBackbuttonPressedPageReturnsTrue()
		{
			TestShell shell = new TestShell();

			Routing.RegisterRoute("OnBackbuttonPressedFiresOnPage", typeof(ShellTestPage));
			shell.Items.Add(CreateShellItem());
			await shell.GoToAsync($"OnBackbuttonPressedFiresOnPage?CancelNavigationOnBackButtonPressed=true");

			shell.SendBackButtonPressed();
			Assert.AreEqual(2, shell.Navigation.NavigationStack.Count);
		}

		[Test]
		public async Task OnBackbuttonPressedPageReturnsFalse()
		{
			TestShell shell = new TestShell();

			Routing.RegisterRoute("OnBackbuttonPressedFiresOnPage", typeof(ShellTestPage));
			shell.Items.Add(CreateShellItem());
			await shell.GoToAsync($"OnBackbuttonPressedFiresOnPage?CancelNavigationOnBackButtonPressed=false");

			shell.SendBackButtonPressed();
			Assert.AreEqual(1, shell.Navigation.NavigationStack.Count);
		}

		[Test]
		public async Task OnBackbuttonPressedShellReturnsTrue()
		{
			TestShell shell = new TestShell();

			Routing.RegisterRoute("OnBackbuttonPressedShellReturnsTrue", typeof(ShellTestPage));
			shell.Items.Add(CreateShellItem());
			await shell.GoToAsync($"OnBackbuttonPressedShellReturnsTrue");
			shell.OnBackButtonPressedFunc = () => true;
			shell.SendBackButtonPressed();
			Assert.AreEqual(2, shell.Navigation.NavigationStack.Count);
		}

		[Test]
		public void BackButtonBehaviorSet()
		{
			var page = new ContentPage();

			Assert.IsNull(Shell.GetBackButtonBehavior(page));

			var backButtonBehavior = new BackButtonBehavior();

			Shell.SetBackButtonBehavior(page, backButtonBehavior);

			Assert.AreEqual(backButtonBehavior, Shell.GetBackButtonBehavior(page));
		}

		[Test]
		public void ModalSetters()
		{
			var page = new ContentPage();

			Assert.IsFalse(IsModal(page));
			Assert.IsTrue(IsAnimated(page));

			Shell.SetPresentationMode(page, PresentationMode.Modal | PresentationMode.NotAnimated);

			Assert.IsTrue(IsModal(page));
			Assert.IsFalse(IsAnimated(page));
		}

		[Test]
		public void BackButtonBehaviorBindingContextPropagation()
		{
			object bindingContext = new object();
			var page = new ContentPage();
			var backButtonBehavior = new BackButtonBehavior();

			Shell.SetBackButtonBehavior(page, backButtonBehavior);
			page.BindingContext = bindingContext;

			Assert.AreEqual(page.BindingContext, backButtonBehavior.BindingContext);
		}

		[Test]
		public void BackButtonBehaviorBindingContextPropagationWithExistingBindingContext()
		{
			object bindingContext = new object();
			var page = new ContentPage();
			var backButtonBehavior = new BackButtonBehavior();

			page.BindingContext = bindingContext;
			Shell.SetBackButtonBehavior(page, backButtonBehavior);

			Assert.AreEqual(page.BindingContext, backButtonBehavior.BindingContext);
		}

		[Test]
		public void FlyoutHeaderProjection()
		{
			var shell = new Shell();

			var label = new Label();

			var viewModel = new Object();
			shell.BindingContext = viewModel;

			shell.FlyoutHeader = label;

			Assert.AreEqual(((IShellController)shell).FlyoutHeader, label);

			Label label2 = null;

			shell.FlyoutHeaderTemplate = new DataTemplate(() =>
			{
				return label2 = new Label();
			});

			Assert.AreEqual(((IShellController)shell).FlyoutHeader, label2);
			Assert.AreEqual(((IShellController)shell).FlyoutHeader.BindingContext, viewModel);

			shell.FlyoutHeaderTemplate = null;

			Assert.AreEqual(((IShellController)shell).FlyoutHeader, label);
		}

		[Test]
		public async Task FlyoutNavigateToImplicitContentPage()
		{
			var shell = new Shell();
			var shellITem = new ShellItem() { FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems, };
			var shellSection = new ShellSection() { Title = "can navigate to" };
			shellSection.Items.Add(new ContentPage());

			var shellSection2 = new ShellSection() { Title = "can navigate to" };
			shellSection2.Items.Add(new ContentPage());

			var implicitSection = CreateShellSection(new ContentPage(), asImplicit: true);

			shellITem.Items.Add(shellSection);
			shellITem.Items.Add(shellSection2);
			shellITem.Items.Add(implicitSection);

			shell.Items.Add(shellITem);
			IShellController shellController = (IShellController)shell;

			await shellController.OnFlyoutItemSelectedAsync(shellSection2);
			Assert.AreEqual(shellSection2, shell.CurrentItem.CurrentItem);

			await shellController.OnFlyoutItemSelectedAsync(shellSection);
			Assert.AreEqual(shellSection, shell.CurrentItem.CurrentItem);

			await shellController.OnFlyoutItemSelectedAsync(implicitSection);
			Assert.AreEqual(implicitSection, shell.CurrentItem.CurrentItem);

		}




		[Test]
		public async Task UriNavigationTests()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");
			var item2 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent2");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			shell.GoToAsync("//rootlevelcontent2");
			Assert.AreEqual(shell.CurrentItem, item2);

			shell.GoToAsync("//rootlevelcontent1");
			Assert.AreEqual(shell.CurrentItem, item1);
		}

		[Test]
		public async Task TitleViewBindingContext()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));
			page.BindingContext = new { Text = "Binding" };

			// setup title view
			StackLayout layout = new StackLayout() { BackgroundColor = Color.White };
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			layout.Children.Add(label);
			Shell.SetTitleView(page, layout);

			Assert.AreEqual("Binding", label.Text);
			page.BindingContext = new { Text = "Binding Changed" };
			Assert.AreEqual("Binding Changed", label.Text);
		}

		[Test]
		public async Task VisualPropagationPageLevel()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));

			// setup title view
			StackLayout titleView = new StackLayout() { BackgroundColor = Color.White };
			Button button = new Button();
			titleView.Children.Add(button);
			Shell.SetTitleView(page, titleView);
			IVisualController visualController = button as IVisualController;


			Assert.AreEqual(page, titleView.Parent);

			Assert.AreEqual(VisualMarker.Default, ((IVisualController)button).EffectiveVisual);
			page.Visual = VisualMarker.Material;
			Assert.AreEqual(VisualMarker.Material, ((IVisualController)button).EffectiveVisual);
		}

		[Test]
		public async Task VisualPropagationShellLevel()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));

			// setup title view
			StackLayout titleView = new StackLayout() { BackgroundColor = Color.White };
			Button button = new Button();
			titleView.Children.Add(button);
			Shell.SetTitleView(page, titleView);
			IVisualController visualController = button as IVisualController;


			Assert.AreEqual(page, titleView.Parent);
			Assert.AreEqual(VisualMarker.Default, ((IVisualController)button).EffectiveVisual);
			shell.Visual = VisualMarker.Material;
			Assert.AreEqual(VisualMarker.Material, ((IVisualController)button).EffectiveVisual);
		}

		[Test]
		public async Task FlyoutViewVisualPropagation()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));


			// setup title view
			StackLayout flyoutView = new StackLayout() { BackgroundColor = Color.White };
			Button button = new Button();
			flyoutView.Children.Add(button);
			shell.SetValue(Shell.FlyoutHeaderProperty, flyoutView);

			IVisualController visualController = button as IVisualController;
			Assert.AreEqual(VisualMarker.Default, visualController.EffectiveVisual);
			shell.Visual = VisualMarker.Material;
			Assert.AreEqual(VisualMarker.Material, visualController.EffectiveVisual);
		}

		[Test]
		public async Task FlyoutViewBindingContext()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));
			shell.BindingContext = new { Text = "Binding" };

			// setup title view
			StackLayout flyoutView = new StackLayout() { BackgroundColor = Color.White };
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			flyoutView.Children.Add(label);
			shell.SetValue(Shell.FlyoutHeaderProperty, flyoutView);

			Assert.AreEqual("Binding", label.Text);
			shell.BindingContext = new { Text = "Binding Changed" };
			Assert.AreEqual("Binding Changed", label.Text);
			shell.SetValue(Shell.FlyoutHeaderProperty, new ContentView());
			Assert.AreEqual(null, flyoutView.BindingContext);
		}

		[Test]
		public void MenuItemBindingContext()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));
			shell.BindingContext = new { Text = "Binding" };


			object bindingContext = new object();

			var menuItem = new MenuItem();
			shell.Items.Add(new MenuShellItem(menuItem));

			shell.BindingContext = bindingContext;

			var menuItem2 = new MenuItem();
			shell.Items.Add(new MenuShellItem(menuItem2));


			Assert.AreEqual(bindingContext, menuItem.BindingContext);
			Assert.AreEqual(bindingContext, menuItem2.BindingContext);
		}

		[Test]
		public async Task TitleViewLogicalChild()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));
			page.BindingContext = new { Text = "Binding" };

			// setup title view
			StackLayout layout = new StackLayout() { BackgroundColor = Color.White };
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			layout.Children.Add(label);
			Shell.SetTitleView(page, layout);


			Assert.True(page.ChildrenNotDrawnByThisElement.Contains(layout));
		}


		[Test]
		public async Task FlyoutHeaderLogicalChild()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));

			// setup title view
			StackLayout layout = new StackLayout() { BackgroundColor = Color.White };
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			layout.Children.Add(label);


			shell.FlyoutHeader = null;
			shell.FlyoutHeader = layout;

			Assert.True(shell.LogicalChildren.Contains(layout));
			shell.FlyoutHeader = null;

			Assert.False(shell.LogicalChildren.Contains(layout));
		}


		[Test]
		public async Task ShellFlyoutChangeableOnShellWithFlyoutItem()
		{
			Shell shell = new Shell();
			var flyoutItem = CreateShellItem<FlyoutItem>();
			shell.Items.Add(flyoutItem);
			Assert.AreEqual(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());
			shell.FlyoutBehavior = FlyoutBehavior.Locked;
			Assert.AreEqual(FlyoutBehavior.Locked, shell.GetEffectiveFlyoutBehavior());
			shell.FlyoutBehavior = FlyoutBehavior.Disabled;
			Assert.AreEqual(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());
		}

		[Test]
		public async Task ShellFlyoutChangeableOnShellWithTabBar()
		{
			Shell shell = new Shell();
			var tabBarItem = CreateShellItem<TabBar>();
			shell.Items.Add(tabBarItem);
			Assert.AreEqual(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());
			shell.FlyoutBehavior = FlyoutBehavior.Flyout;
			Assert.AreEqual(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());
			shell.FlyoutBehavior = FlyoutBehavior.Locked;
			Assert.AreEqual(FlyoutBehavior.Locked, shell.GetEffectiveFlyoutBehavior());
		}


		[Test]
		public async Task ShellFlyoutBehaviorCalculation()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page: page));
			Assert.AreEqual(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());

			Shell.SetFlyoutBehavior(page, FlyoutBehavior.Disabled);
			Shell.SetFlyoutBehavior(shell.Items[0].Items[0].Items[0], FlyoutBehavior.Flyout);
			Shell.SetFlyoutBehavior(shell.Items[0].Items[0], FlyoutBehavior.Disabled);
			Shell.SetFlyoutBehavior(shell.Items[0], FlyoutBehavior.Locked);

			Assert.AreEqual(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());

			page.ClearValue(Shell.FlyoutBehaviorProperty);
			Assert.AreEqual(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());

			shell.Items[0].Items[0].Items[0].ClearValue(Shell.FlyoutBehaviorProperty);
			Assert.AreEqual(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());

			shell.Items[0].Items[0].ClearValue(Shell.FlyoutBehaviorProperty);
			Assert.AreEqual(FlyoutBehavior.Locked, shell.GetEffectiveFlyoutBehavior());

			shell.Items[0].ClearValue(Shell.FlyoutBehaviorProperty);
			Assert.AreEqual(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());
		}

		[Test]
		public async Task TabBarAutoCreation()
		{
			Shell shell = new Shell();
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));

			Assert.AreEqual(1, shell.Items.Count);
			Assert.AreEqual(3, shell.Items[0].Items.Count);

			Assert.AreEqual(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());


			shell = new Shell();
			shell.Items.Add(new TabBar());
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));

			Assert.AreEqual(2, shell.Items.Count);
			Assert.AreEqual(0, shell.Items[0].Items.Count);
			Assert.AreEqual(3, shell.Items[1].Items.Count);


			shell = new Shell();
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(new TabBar());
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));

			Assert.AreEqual(3, shell.Items.Count);
			Assert.AreEqual(3, shell.Items[0].Items.Count);
			Assert.AreEqual(0, shell.Items[1].Items.Count);
			Assert.AreEqual(3, shell.Items[0].Items.Count);
		}


		[Test]
		public async Task NavigatedFiresAfterContentIsCreatedWhenUsingTemplate()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");

			shell.Items.Add(item1);
			Routing.RegisterRoute("cat", typeof(ContentPage));
			Routing.RegisterRoute("details", typeof(ContentPage));

			await shell.GoToAsync("cat");
			await shell.GoToAsync("details");

			Assert.AreEqual("//rootlevelcontent1/cat/details", shell.CurrentState.Location.ToString());
			await shell.GoToAsync("//rootlevelcontent1/details");
			Assert.AreEqual("//rootlevelcontent1/details", shell.CurrentState.Location.ToString());
		}

		[Test]
		public async Task ShellItemNotVisibleWhenContentPageNotVisible()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(new ContentPage() { IsVisible = false });
			var item2 = CreateShellItem();

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Assert.IsFalse(GetItems(shell).Contains(item1));
			Assert.IsTrue(GetItems(shell).Contains(item2));
		}

		[Test]
		public async Task BaseShellItemNotVisible()
		{
			var shell = new Shell();
			var item1 = CreateShellItem();
			var item2 = CreateShellItem();

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			item1.IsVisible = false;
			Assert.IsFalse(GetItems(shell).Contains(item1));
			Assert.IsTrue(GetItems(shell).Contains(item2));

			item1.IsVisible = true;
			Assert.IsTrue(GetItems(shell).Contains(item1));

			item1.Items[0].IsVisible = false;
			Assert.IsFalse(GetItems(shell).Contains(item1));
			item1.Items[0].IsVisible = true;
			Assert.IsTrue(GetItems(shell).Contains(item1));

			item1.Items[0].Items[0].IsVisible = false;
			Assert.IsFalse(GetItems(shell).Contains(item1));
			item1.Items[0].Items[0].IsVisible = true;
			Assert.IsTrue(GetItems(shell).Contains(item1));
		}

		[Test]
		public async Task CantNavigateToNotVisibleShellItem()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(shellItemRoute: "NotVisible");
			var item2 = CreateShellItem();

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			item1.IsVisible = false;

			Assert.That(async () => await shell.GoToAsync($"//NotVisible"), Throws.Exception);

			Assert.AreEqual(shell.CurrentItem, item2);
		}


		[Test]
		public async Task FlyoutItemVisible()
		{
			var shell = new Shell();
			var item1 = CreateShellItem<FlyoutItem>(shellItemRoute: "NotVisible");
			var item2 = CreateShellItem();

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Shell.SetFlyoutItemIsVisible(item1, false);
			Assert.IsTrue(GetItems(shell).Contains(item1));

			bool hasFlyoutItem =
				(shell as IShellController)
					.GenerateFlyoutGrouping()
					.SelectMany(i => i)
					.Contains(item1);

			Assert.IsFalse(hasFlyoutItem);
		}

		[Test]
		public async Task ShellContentCollectionClear()
		{
			var shell = new Shell();
			var item1 = CreateShellItem();
			var section2 = CreateShellSection();

			shell.Items.Add(item1);
			item1.Items.Add(section2);

			var mainTab = item1.Items[0];
			var content1 = CreateShellContent();
			var clearedContent = mainTab.Items[0];
			mainTab.Items.Clear();
			mainTab.Items.Add(content1);
			mainTab.Items.Add(CreateShellContent());

			Assert.IsNull(clearedContent.Parent);
			Assert.AreEqual(2, mainTab.Items.Count);
			Assert.AreEqual(content1, mainTab.CurrentItem);
		}

		[Test]
		public async Task ShellItemCollectionClear()
		{
			var shell = new Shell();
			var item1 = CreateShellItem();
			shell.Items.Add(item1);


			var item2 = CreateShellItem();
			var item3 = CreateShellItem();

			shell.Items.Clear();
			shell.Items.Add(item2);
			shell.Items.Add(item3);

			Assert.IsNull(item1.Parent);
			Assert.AreEqual(2, shell.Items.Count);
			Assert.AreEqual(item2, shell.CurrentItem);
		}

		[Test]
		public async Task ShellSectionCollectionClear()
		{
			var shell = new Shell();
			var item1 = CreateShellItem();
			shell.Items.Add(item1);

			var section1 = CreateShellSection();
			var section2 = CreateShellSection();
			var clearedSection = item1.Items[0];

			Assert.IsNotNull(clearedSection.Parent);
			item1.Items.Clear();
			item1.Items.Add(section1);
			item1.Items.Add(section2);

			Assert.IsNull(clearedSection.Parent);
			Assert.AreEqual(2, item1.Items.Count);
			Assert.AreEqual(section1, shell.CurrentItem.CurrentItem);
		}

		[Test]
		public async Task ShellVisibleItemsReAddedIntoSameOrder()
		{
			var shell = new Shell();
			var item1 = CreateShellItem();
			shell.Items.Add(item1);
			var shellSection = item1.Items[0];
			var shellSectionController = (IShellSectionController)shellSection;
			ContentPage hideMe = new ContentPage();
			var shellContent = CreateShellContent(hideMe);


			shellSection.Items.Insert(0, shellContent);

			Assert.AreEqual(0, shellSection.Items.IndexOf(shellContent));
			Assert.AreEqual(0, shellSectionController.GetItems().IndexOf(shellContent));

			hideMe.IsVisible = false;

			Assert.AreEqual(0, shellSection.Items.IndexOf(shellContent));
			Assert.AreEqual(-1, shellSectionController.GetItems().IndexOf(shellContent));

			hideMe.IsVisible = true;

			Assert.AreEqual(0, shellSection.Items.IndexOf(shellContent));
			Assert.AreEqual(0, shellSectionController.GetItems().IndexOf(shellContent));
		}

		[Test]
		public async Task HidingShellItemSetsNewCurrentItem()
		{
			var shell = new Shell();
			ContentPage contentPage = new ContentPage();
			var item1 = CreateShellItem(contentPage);
			shell.Items.Add(item1);
			var item2 = CreateShellItem();
			shell.Items.Add(item2);

			Assert.AreEqual(shell.CurrentItem, item1);
			contentPage.IsVisible = false;
			Assert.AreEqual(shell.CurrentItem, item2);
		}


		[Test]
		public async Task HidingShellSectionSetsNewCurrentItem()
		{
			var shell = new Shell();
			ContentPage contentPage = new ContentPage();
			var item1 = CreateShellItem(contentPage);
			shell.Items.Add(item1);
			var shellSection2 = CreateShellSection();
			item1.Items.Add(shellSection2);

			Assert.AreEqual(shell.CurrentItem.CurrentItem, item1.Items[0]);
			contentPage.IsVisible = false;
			Assert.AreEqual(shell.CurrentItem.CurrentItem, shellSection2);
		}


		[Test]
		public async Task HidingShellContentSetsNewCurrentItem()
		{
			var shell = new Shell();
			ContentPage contentPage = new ContentPage();
			var item1 = CreateShellItem(contentPage);
			shell.Items.Add(item1);
			var shellContent2 = CreateShellContent();
			item1.Items[0].Items.Add(shellContent2);

			Assert.AreEqual(shell.CurrentItem.CurrentItem.CurrentItem, item1.Items[0].Items[0]);
			contentPage.IsVisible = false;
			Assert.AreEqual(shell.CurrentItem.CurrentItem.CurrentItem, shellContent2);
		}

		[Test]
		public async Task ShellLocationRestoredWhenItemsAreReAdded()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "root1"));
			shell.Items.Add(CreateShellItem(shellContentRoute: "root2"));

			await shell.GoToAsync("//root2");
			Assert.AreEqual("//root2", shell.CurrentState.Location.ToString());

			shell.Items.Add(CreateShellItem(shellContentRoute: "root1"));
			shell.Items.Add(CreateShellItem(shellContentRoute: "root2"));

			shell.Items.Clear();
			Assert.AreEqual("//root2", shell.CurrentState.Location.ToString());
		}

		[Test]
		public void ClearingShellContentAndReAddingSetsCurrentItem()
		{
			Shell shell = new Shell();
			var item = CreateShellItem();
			item.CurrentItem.Items.Add(CreateShellContent());
			item.CurrentItem.Items.Add(CreateShellContent());
			var item2 = CreateShellItem();

			shell.Items.Add(item);
			shell.Items.Add(item2);

			item.Items[0].Items.Clear();

			var content = CreateShellContent();
			item.Items[0].Items.Add(content);
			item.Items[0].Items.Add(CreateShellContent());

			Assert.IsNotNull(item.CurrentItem);
			Assert.IsNotNull(item.CurrentItem.CurrentItem);
		}

		[Test]
		public void ClearingShellSectionAndReAddingSetsCurrentItem()
		{
			Shell shell = new Shell();
			var item = CreateShellItem();
			item.CurrentItem.Items.Add(CreateShellContent());
			item.CurrentItem.Items.Add(CreateShellContent());
			var item2 = CreateShellItem();

			shell.Items.Add(item);
			shell.Items.Add(item2);

			item.Items.Clear();

			var section = CreateShellSection();
			item.Items.Add(section);
			item.Items.Add(CreateShellSection());

			Assert.IsNotNull(item.CurrentItem);
			Assert.IsNotNull(item.CurrentItem.CurrentItem);
		}

		[Test]

		public async Task GetCurrentPageInShellNavigation()
		{
			Shell shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");

			shell.Items.Add(item1);
			Routing.RegisterRoute("cat", typeof(ContentPage));

			Page page = null;

			shell.Navigated += (_, __) =>
			{
				page = shell.CurrentPage;
			};

			await shell.GoToAsync("cat");
			Assert.IsNotNull(page);
			Assert.AreEqual(page.GetType(), typeof(ContentPage));
			Assert.AreEqual(shell.Navigation.NavigationStack[1], page);
		}

		[Test]
		public async Task GetCurrentPageBetweenSections()
		{
			var shell = new Shell();
			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabtwo = MakeSimpleShellSection("tabtwo", "content");
			var tabthree = MakeSimpleShellSection("tabthree", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content");

			one.Items.Add(tabone);
			one.Items.Add(tabtwo);

			two.Items.Add(tabthree);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			Page page = null;

			shell.Navigated += (_, __) =>
			{
				page = shell.CurrentPage;
			};

			shell.GoToAsync(new ShellNavigationState("//two/tabfour/"));
			Assert.IsNotNull(page);
			Assert.AreEqual(page.GetType(), typeof(ShellTestPage));
			Assert.AreEqual((tabfour as IShellSectionController).PresentedPage, page);
		}

		[Test]
		public void GetCurrentPageOnInit()
		{
			var shell = new Shell();
			Page page = null;
			shell.Navigated += (_, __) =>
			{
				page = shell.CurrentPage;
			};
			var tabone = MakeSimpleShellSection("tabone", "content");
			shell.Items.Add(tabone);
			Assert.IsNotNull(page);
		}


		public async Task HotReloadStaysOnActiveItem()
		{
			Shell shell = new Shell();

			shell.Items.Add(CreateShellItem(shellItemRoute: "item1"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "item2"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "item3"));

			await shell.GoToAsync("//item3");
			Assert.AreEqual("//item3", shell.CurrentState.Location.ToString());

			shell.Items.Add(CreateShellItem(shellItemRoute: "item1"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "item2"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "item3"));

			shell.Items.RemoveAt(0);
			shell.Items.RemoveAt(0);
			shell.Items.RemoveAt(0);

			Assert.AreEqual("//item3", shell.CurrentState.Location.ToString());

		}

		[TestCase("ContentPage")]
		[TestCase("ShellItem")]
		[TestCase("Shell")]
		public void TabBarIsVisible(string test)
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			var shellItem = CreateShellItem(page);
			shell.Items.Add(shellItem);

			switch (test)
			{
				case "ContentPage":
					Shell.SetTabBarIsVisible(page, false);
					break;
				case "ShellItem":
					Shell.SetTabBarIsVisible(shellItem, false);
					break;
				case "Shell":
					Shell.SetTabBarIsVisible(shell, false);
					break;
			}

			Assert.IsFalse((shellItem as IShellItemController).ShowTabs);
		}

		[Test]
		public void SendStructureChangedFiresWhenAddingItems()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());

			int count = 0;
			int previousCount = 0;
			(shell as IShellController).StructureChanged += (_, __) => count++;


			shell.Items.Add(CreateShellItem());
			Assert.Greater(count, previousCount, "StructureChanged not fired when adding Shell Item");

			previousCount = count;
			shell.CurrentItem.Items.Add(CreateShellSection());
			Assert.Greater(count, previousCount, "StructureChanged not fired when adding Shell Section");

			previousCount = count;
			shell.CurrentItem.CurrentItem.Items.Add(CreateShellContent());
			Assert.Greater(count, previousCount, "StructureChanged not fired when adding Shell Content");
		}

	}
}
