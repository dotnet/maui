using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
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

			await shell.GoToAsync("/tab22", false, true);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//two/tab22/content"));

			await shell.GoToAsync("tab21", false, true);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//two/tab21/content"));

			await shell.GoToAsync("/tab23", false, true);
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
		public async Task NavigationWithQueryStringWhenPageMatchesBindingContext()
		{
			var shell = new Shell();

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content", null);

			one.Items.Add(tabone);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			ShellTestPage pagetoTest = new ShellTestPage();
			await shell.GoToAsync(new ShellNavigationState($"//two/tabfour/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			two.CurrentItem.CurrentItem.ContentTemplate = new DataTemplate(() =>
			{
				pagetoTest = new ShellTestPage();
				pagetoTest.BindingContext = pagetoTest;
				return pagetoTest;
			});

			
			var page = (two.CurrentItem.CurrentItem as IShellContentController).GetOrCreateContent();
			Assert.AreEqual("1234", (page as ShellTestPage).SomeQueryParameter);

		}


		[Test]
		public async Task NavigationWithQueryStringThenWithoutQueryString()
		{
			var shell = new Shell();

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content", null);

			one.Items.Add(tabone);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			ShellTestPage pagetoTest = new ShellTestPage();
			await shell.GoToAsync(new ShellNavigationState($"//two/tabfour/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			two.CurrentItem.CurrentItem.ContentTemplate = new DataTemplate(() =>
			{
				pagetoTest = new ShellTestPage();
				pagetoTest.BindingContext = pagetoTest;
				return pagetoTest;
			});


			await shell.GoToAsync(new ShellNavigationState($"//one/tabone/content"));
			await shell.GoToAsync(new ShellNavigationState($"//two/tabfour/content"));

			var page = (two.CurrentItem.CurrentItem as IShellContentController).GetOrCreateContent();
			Assert.AreEqual(null, (page as ShellTestPage).SomeQueryParameter);
		}


		[Test]
		public async Task NavigationBetweenShellContentsPassesQueryString()
		{
			var shell = new Shell();

			var item = CreateShellItem(shellSectionRoute: "section2");
			var content = CreateShellContent(shellContentRoute: "content");
			item.Items[0].Items.Add(content);

			Routing.RegisterRoute("details", typeof(ShellTestPage));

			shell.Items.Add(item);


			await shell.GoToAsync(new ShellNavigationState($"//section2/details?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			await shell.GoToAsync(new ShellNavigationState($"//content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			await shell.GoToAsync(new ShellNavigationState($"//section2/details?{nameof(ShellTestPage.SomeQueryParameter)}=4321"));

			var testPage = (shell.CurrentItem.CurrentItem as IShellSectionController).PresentedPage as ShellTestPage;
			Assert.AreEqual("4321", testPage.SomeQueryParameter);
		}

		[Test]
		public async Task BasicQueryStringTest()
		{
			var shell = new Shell();

			var item = CreateShellItem(shellSectionRoute: "section2");
			Routing.RegisterRoute("details", typeof(ShellTestPage));
			shell.Items.Add(item);
			await shell.GoToAsync(new ShellNavigationState($"details?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			var testPage = (shell.CurrentItem.CurrentItem as IShellSectionController).PresentedPage as ShellTestPage;
			Assert.AreEqual("1234", testPage.SomeQueryParameter);
		}


		[Test]
		public async Task NavigationWithQueryStringAndNoDataTemplate()
		{
			var shell = new Shell();

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content");

			one.Items.Add(tabone);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			await shell.GoToAsync(new ShellNavigationState($"//two/tabfour/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			Assert.AreEqual("1234", (two.CurrentItem.CurrentItem.Content as ShellTestPage).SomeQueryParameter);
		}

		[Test]
		public void CancelNavigation()
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

			Assume.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//one/tabone/content"));

			shell.Navigating += (s, e) =>
			{
				e.Cancel();
			};

			shell.GoToAsync(new ShellNavigationState("//two/tabfour/"));

			Assume.That(shell.CurrentState.Location.ToString(), Is.EqualTo("//one/tabone/content"));
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
			var shellITem = new ShellItem() { FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,  };
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

			Assert.True(shell.ChildrenNotDrawnByThisElement.Contains(layout));
			shell.FlyoutHeader = null;

			Assert.False(shell.ChildrenNotDrawnByThisElement.Contains(layout));
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
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));

			Assert.AreEqual(1, shell.Items.Count);
			Assert.AreEqual(3, shell.Items[0].Items.Count);

			Assert.AreEqual(FlyoutBehavior.Disabled, Shell.GetFlyoutBehavior(shell.Items[0]));


			shell = new Shell();
			shell.Items.Add(new TabBar());
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));

			Assert.AreEqual(2, shell.Items.Count);
			Assert.AreEqual(0, shell.Items[0].Items.Count);
			Assert.AreEqual(3, shell.Items[1].Items.Count);


			shell = new Shell();
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));
			shell.Items.Add(new TabBar());
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));
			shell.Items.Add(ShellItem.CreateFromShellSection(new Tab()));

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
		public async Task ShellItemNotVisible()
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
	}
}
