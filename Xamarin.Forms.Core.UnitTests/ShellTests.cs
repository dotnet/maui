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
			Assert.IsEmpty(shell.MenuItems);
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

			NavigationProxy shellProxy = proxy.Inner as ShellSection.NavigationImpl;
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
			shell.Route = "s";

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

			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/one/tabone/content"));

			shell.GoToAsync(new ShellNavigationState("app:///s/two/tabfour/"));

			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tabfour/content"));
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
		public async Task RelativeGoTo()
		{
			var shell = new Shell
			{
				Route = "s"
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

			await shell.GoToAsync("app:///s/two/tab21/");

			await shell.GoToAsync("/tab22", false, true);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tab22/content"));

			await shell.GoToAsync("tab21", false, true);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tab21/content"));

			await shell.GoToAsync("/tab23", false, true);
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tab23/content"));

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
			shell.Route = "s";

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content", null);

			one.Items.Add(tabone);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			ShellTestPage pagetoTest = new ShellTestPage();
			await shell.GoToAsync(new ShellNavigationState($"app:///s/two/tabfour/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
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
		public async Task NavigationWithQueryStringAndNoDataTemplate()
		{
			var shell = new Shell();
			shell.Route = "s";

			var one = new ShellItem { Route = "one" };
			var two = new ShellItem { Route = "two" };

			var tabone = MakeSimpleShellSection("tabone", "content");
			var tabfour = MakeSimpleShellSection("tabfour", "content");

			one.Items.Add(tabone);
			two.Items.Add(tabfour);

			shell.Items.Add(one);
			shell.Items.Add(two);

			await shell.GoToAsync(new ShellNavigationState($"app:///s/two/tabfour/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			Assert.AreEqual("1234", (two.CurrentItem.CurrentItem.Content as ShellTestPage).SomeQueryParameter);

		}

		[Test]
		public void CancelNavigation()
		{
			var shell = new Shell();
			shell.Route = "s";

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

			Assume.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/one/tabone/content/"));

			shell.Navigating += (s, e) =>
			{
				e.Cancel();
			};

			shell.GoToAsync(new ShellNavigationState("app:///s/two/tabfour/"));

			Assume.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/one/tabone/content/"));
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
	}
}
