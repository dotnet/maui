using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class ShellTests : ShellTestBase
	{

		[Fact]
		public void DefaultState()
		{
			var shell = new Shell();

			Assert.Empty(shell.Items);
		}

		[Fact]
		public void CurrentItemAutoSets()
		{
			var shell = new Shell();
			var shellItem = new ShellItem();
			var shellSection = new ShellSection();
			var shellContent = new ShellContent { Content = new ContentPage() };
			shellSection.Items.Add(shellContent);
			shellItem.Items.Add(shellSection);
			shell.Items.Add(shellItem);

			Assert.Equal(shell.CurrentItem, shellItem);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
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

			Assert.Equal(shellElement.FindParentOfType<ShellItem>(), item2);

			if (useShellContent)
				shell.CurrentItem = (ShellContent)shellElement;
			else
				shell.CurrentItem = (ShellSection)shellElement;

			Assert.Equal(2, shell.Items.Count);
			Assert.Equal(shellElement.FindParentOfType<ShellItem>(), item2);
			Assert.Equal(item2, shell.CurrentItem);
		}


		[Fact]
		public void SettingCurrentItemOnShellViaContentPage()
		{
			var page1 = new ContentPage();
			var page2 = new ContentPage();
			var shell = new TestShell()
			{
				Items =
				{
					new TabBar()
					{
						Items =
						{
							new ShellContent() { Content = page1 },
							new ShellContent() { Content = page2 },
						}
					}
				}
			};

			shell.CurrentItem = page2;
			Assert.Single(shell.Items);
			Assert.Equal(2, shell.Items[0].Items.Count);
			Assert.Single(shell.Items[0].Items[0].Items);
			Assert.Single(shell.Items[0].Items[1].Items);
			Assert.Equal(shell.CurrentItem.CurrentItem, shell.Items[0].Items[1]);
		}

		[Fact]
		public void SetCurrentItemAddsToShellCollection()
		{
			var shell = new Shell();
			var shellItem = CreateShellItem();
			var shellSection = CreateShellSection();
			var shellContent = CreateShellContent();

			shell.CurrentItem = shellItem;
			Assert.True(shell.Items.Contains(shellItem));
			Assert.Equal(shell.CurrentItem, shellItem);

			shell.CurrentItem = shellSection;
			Assert.Equal(shell.CurrentItem, shellSection.Parent);

			shell.CurrentItem = shellContent;
			Assert.Equal(shell.CurrentItem, shellContent.Parent.Parent);
		}


		[Fact]
		public void ShellChildrenBindingContext()
		{
			var shell = new Shell();

			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);

			object viewModel = new object();
			shell.BindingContext = viewModel;

			Assert.Same(shell.BindingContext, viewModel);
			Assert.Same(shellItem.BindingContext, viewModel);
			Assert.Same(shellItem.Items[0].BindingContext, viewModel);
			Assert.Same(shellItem.Items[0].Items[0].BindingContext, viewModel);
			Assert.Same((shellItem.Items[0].Items[0].Content as BindableObject).BindingContext, viewModel);
		}

		[Fact]
		public void ShellPropagateBindingContextWhenAddingNewShellItem()
		{
			var shell = new Shell();

			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var shellItem = CreateShellItem();
			shell.Items.Add(shellItem);

			Assert.Same(shellItem.BindingContext, viewModel);
			Assert.Same(shellItem.Items[0].BindingContext, viewModel);
			Assert.Same(shellItem.Items[0].Items[0].BindingContext, viewModel);
			Assert.Same((shellItem.Items[0].Items[0].Content as BindableObject).BindingContext, viewModel);
		}

		[Fact]
		public void ShellPropagateBindingContextWhenAddingNewShellSection()
		{
			var shell = new Shell();

			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var shellSection = CreateShellSection();
			shell.Items[0].Items.Add(shellSection);

			Assert.Same(shellSection.BindingContext, viewModel);
			Assert.Same(GetItems(shellSection)[0].BindingContext, viewModel);
			Assert.Same((GetItems(shellSection)[0].Content as BindableObject).BindingContext, viewModel);
		}

		[Fact]
		public void ShellPropagateBindingContextWhenAddingNewShellContent()
		{
			var shell = new Shell();

			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var shellContent = CreateShellContent();
			shell.Items[0].Items[0].Items.Add(shellContent);

			Assert.Same(shellContent.BindingContext, viewModel);
			Assert.Same((shellContent.Content as BindableObject).BindingContext, viewModel);
		}

		[Fact]
		public void ShellPropagateBindingContextWhenChangingContent()
		{
			var shell = new Shell();

			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var contentPage = new ContentPage();

			shell.Items[0].Items[0].Items[0].Content = contentPage;
			Assert.Same(contentPage.BindingContext, viewModel);
		}

		[Fact]
		public async Task ShellPropagateBindingContextWhenPushingContent()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			object viewModel = new object();
			shell.BindingContext = viewModel;
			var contentPage = new ContentPage();
			await shell.Navigation.PushAsync(contentPage);

			Assert.Same(contentPage.BindingContext, viewModel);
		}

		[Fact]
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
			Assert.NotNull(proxy);

			var shellProxy = proxy.Inner;
			Assert.NotNull(shellProxy);
		}


		[Fact]
		public void CurrentItemDoesNotChangeOnSecondAdd()
		{
			var shell = new Shell();
			var shellItem = new ShellItem();
			var shellSection = new ShellSection();
			var shellContent = new ShellContent { Content = new ContentPage() };
			shellSection.Items.Add(shellContent);
			shellItem.Items.Add(shellSection);
			shell.Items.Add(shellItem);

			Assert.Equal(shell.CurrentItem, shellItem);

			shell.Items.Add(new ShellItem());

			Assert.Equal(shellItem, shell.CurrentItem);
		}

		[Fact]
		public void AddRemoveItemsDoesNotCrash()
		{
			var shell = new Shell();

			var rootItem = new FlyoutItem()
			{
				FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems
			};

			for (int i = 0; i < 3; i++)
			{
				var shellContent = new ShellContent
				{
					Content = new ContentPage(),
					Title = $"Item {i}",
				};
				rootItem.Items.Add(shellContent);
			}
			shell.Items.Add(rootItem);
			shell.CurrentItem = rootItem.Items[0];

			shell.Items.ElementAt(0).Items.RemoveAt(1);
			shell.CurrentItem = rootItem.Items[1];

			Assert.Same(shell.CurrentSection, rootItem.Items[1]);
			Assert.True(shell.Items.ElementAt(0).Items.Count == 2);
			Assert.True(shell.CurrentSection.Title == "Item 2");
		}

		[Fact]
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

			Assert.Equal("//one/tabone/content", shell.CurrentState.Location.ToString());

			shell.GoToAsync(new ShellNavigationState("//two/tabfour/"));

			Assert.Equal("//two/tabfour/content", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task CaseIgnoreRouting()
		{
			var routes = new[] { "Tab1", "TAB2", "@-_-@", "+:~", "=%", "Super_Simple+-Route.doc", "1/2", @"1\2/3", "app://tab" };
			var services = Substitute.For<IServiceProvider>();

			foreach (var route in routes)
			{
				var formattedRoute = Routing.FormatRoute(route);
				Routing.RegisterRoute(formattedRoute, typeof(ShellItem));

				var content1 = Routing.GetOrCreateContent(formattedRoute, services);
				Assert.NotNull(content1);
				Assert.Equal(Routing.GetRoute(content1), formattedRoute);
			}

			Assert.ThrowsAny<ArgumentException>(() => Routing.RegisterRoute("app://IMPL_tab21", typeof(ShellItem)));

			Assert.ThrowsAny<ArgumentException>(() => Routing.RegisterRoute(@"app:\\IMPL_tab21", typeof(ShellItem)));

			Assert.ThrowsAny<ArgumentException>(() => Routing.RegisterRoute(string.Empty, typeof(ShellItem)));

			Assert.ThrowsAny<ArgumentNullException>(() => Routing.RegisterRoute(null, typeof(ShellItem)));

			Assert.ThrowsAny<ArgumentException>(() => Routing.RegisterRoute("tab1/IMPL_tab11", typeof(ShellItem)));

			Assert.ThrowsAny<ArgumentException>(() => Routing.RegisterRoute("IMPL_shell", typeof(ShellItem)));

			Assert.ThrowsAny<ArgumentException>(() => Routing.RegisterRoute("app://tab2/IMPL_tab21", typeof(ShellItem)));
		}

		[Fact]
		public async Task FailWhenAddingDuplicatedRouting()
		{
			var route = "dogs";
			Routing.RegisterRoute(route, typeof(ShellItem));

			Assert.ThrowsAny<ArgumentException>(() => Routing.RegisterRoute("dogs", typeof(ContentPage)));
		}

		[Fact]
		public async Task SucceedWhenAddingDuplicateRouteOfSameType()
		{
			var route = "dogs";
			Routing.RegisterRoute(route, typeof(ShellItem));
			Routing.RegisterRoute(route, typeof(ShellItem));
		}

		[Fact]
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
			Assert.Equal("//two/tab22/content", shell.CurrentState.Location.ToString());

			await shell.NavigationManager.GoToAsync("tab21", false, true);
			Assert.Equal("//two/tab21/content", shell.CurrentState.Location.ToString());

			await shell.NavigationManager.GoToAsync("/tab23", false, true);
			Assert.Equal("//two/tab23/content", shell.CurrentState.Location.ToString());

			await shell.GoToAsync("RelativeGoTo_Page1", false);
			Assert.Equal("//two/tab23/content/RelativeGoTo_Page1", shell.CurrentState.Location.ToString());

			await shell.GoToAsync("../RelativeGoTo_Page2", false);
			Assert.Equal("//two/tab23/content/RelativeGoTo_Page2", shell.CurrentState.Location.ToString());

			await shell.GoToAsync("..", false);
			Assert.Equal("//two/tab23/content", shell.CurrentState.Location.ToString());

			/*
			 * removing support for .. notation for now
			await shell.GoToAsync("../one/tab11");
			Assert.Equal(shell.CurrentState.Location.ToString(), "app:///s/one/tab11/content/");

			await shell.GoToAsync("/eee/hm../../../../two/../one/../two/tab21");
			Assert.Equal(shell.CurrentState.Location.ToString(), "app:///s/two/tab21/content/");

			await shell.GoToAsync(new ShellNavigationState("../one/tab11"));
			Assert.Equal(shell.CurrentState.Location.ToString(), "app:///s/one/tab11/content/");

			await shell.GoToAsync(new ShellNavigationState($"../two/tab23/content?{nameof(ShellTestPage.SomeQueryParameter)}=1234"));
			Assert.Equal(shell.CurrentState.Location.ToString(), "app:///s/two/tab23/content/");
			Assert.Equal("1234", (two.CurrentItem.CurrentItem.Content as ShellTestPage).SomeQueryParameter);

			await shell.GoToAsync(new ShellNavigationState($"../one/tab11#fragment"));
			Assert.Equal(shell.CurrentState.Location.ToString(), "app:///s/one/tab11/content/");
			*/
		}


		[Fact]
		public async Task DotDotAdheresToAnimationParameter()
		{
			Routing.RegisterRoute(nameof(DotDotAdheresToAnimationParameter), typeof(ContentPage));
			var shellContent = new ShellContent();
			var shell = new TestShell(new TestFlyoutItem(new TestShellSection(shellContent)));
			await shell.GoToAsync(nameof(DotDotAdheresToAnimationParameter));
			await shell.GoToAsync("..", true);
			Assert.True(shell.LastPopWasAnimated);
		}

		[Fact]
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
			Assert.Equal(shell.CurrentState.Location.ToString(), $"//{Routing.GetRoute(shellContent)}/{nameof(DefaultRoutesMaintainedIfThatsAllThereIs)}");
			await shell.GoToAsync("..");
		}

		[Fact]
		public async Task RoutePathDefaultRemovalWithGlobalRoutesKeepsOneDefaultRoute()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem());

			Routing.RegisterRoute(nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneDefaultRoute), typeof(ContentPage));
			await shell.GoToAsync(nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneDefaultRoute));

			// If all routes on the shell are default we still need to make sure it appends something that represents where you are in the
			// shell structure
			Assert.NotEqual($"//{nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneDefaultRoute)}", shell.CurrentState.Location.ToString());
		}


		[Fact]
		public async Task RoutePathDefaultRemovalWithGlobalRoutesKeepsOneNamedRoute()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "content"));

			Routing.RegisterRoute(nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneNamedRoute), typeof(ContentPage));
			await shell.GoToAsync(nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneNamedRoute));

			Assert.Equal($"//content/{nameof(RoutePathDefaultRemovalWithGlobalRoutesKeepsOneNamedRoute)}", shell.CurrentState.Location.ToString());
		}



		[Fact]
		public async Task OnBackbuttonPressedPageReturnsTrue()
		{
			TestShell shell = new TestShell();

			Routing.RegisterRoute("OnBackbuttonPressedFiresOnPage", typeof(ShellTestPage));
			shell.Items.Add(CreateShellItem());
			await shell.GoToAsync($"OnBackbuttonPressedFiresOnPage?CancelNavigationOnBackButtonPressed=true");

			shell.SendBackButtonPressed();
			Assert.Equal(2, shell.Navigation.NavigationStack.Count);
		}

		[Fact]
		public async Task OnBackbuttonPressedPageReturnsFalse()
		{
			TestShell shell = new TestShell();

			Routing.RegisterRoute("OnBackbuttonPressedFiresOnPage", typeof(ShellTestPage));
			shell.Items.Add(CreateShellItem());
			await shell.GoToAsync($"OnBackbuttonPressedFiresOnPage?CancelNavigationOnBackButtonPressed=false");

			shell.SendBackButtonPressed();
			Assert.Single(shell.Navigation.NavigationStack);
		}

		[Fact]
		public async Task OnBackbuttonPressedShellReturnsTrue()
		{
			TestShell shell = new TestShell();

			Routing.RegisterRoute("OnBackbuttonPressedShellReturnsTrue", typeof(ShellTestPage));
			shell.Items.Add(CreateShellItem());
			await shell.GoToAsync($"OnBackbuttonPressedShellReturnsTrue");
			shell.OnBackButtonPressedFunc = () => true;
			shell.SendBackButtonPressed();
			Assert.Equal(2, shell.Navigation.NavigationStack.Count);
		}

		[Fact]
		public void ModalSetters()
		{
			var page = new ContentPage();

			Assert.False(IsModal(page));
			Assert.True(IsAnimated(page));

			Shell.SetPresentationMode(page, PresentationMode.Modal | PresentationMode.NotAnimated);

			Assert.True(IsModal(page));
			Assert.False(IsAnimated(page));
		}

		[Fact]
		public void FlyoutHeaderProjection()
		{
			var shell = new Shell();

			var label = new Label();

			var viewModel = new Object();
			shell.BindingContext = viewModel;

			shell.FlyoutHeader = label;

			Assert.Equal(((IShellController)shell).FlyoutHeader, label);

			Label label2 = null;

			shell.FlyoutHeaderTemplate = new DataTemplate(() =>
			{
				return label2 = new Label();
			});

			Assert.Equal(((IShellController)shell).FlyoutHeader, label2);
			Assert.Equal(((IShellController)shell).FlyoutHeader.BindingContext, viewModel);

			shell.FlyoutHeaderTemplate = null;

			Assert.Equal(((IShellController)shell).FlyoutHeader, label);
		}

		[Fact]
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
			Assert.Equal(shellSection2, shell.CurrentItem.CurrentItem);

			await shellController.OnFlyoutItemSelectedAsync(shellSection);
			Assert.Equal(shellSection, shell.CurrentItem.CurrentItem);

			await shellController.OnFlyoutItemSelectedAsync(implicitSection);
			Assert.Equal(implicitSection, shell.CurrentItem.CurrentItem);

		}




		[Fact]
		public async Task UriNavigationTests()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");
			var item2 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent2");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			shell.GoToAsync("//rootlevelcontent2");
			Assert.Equal(shell.CurrentItem, item2);

			shell.GoToAsync("//rootlevelcontent1");
			Assert.Equal(shell.CurrentItem, item1);
		}

		[Fact]
		public async Task TitleViewBindingContext()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));
			page.BindingContext = new { Text = "Binding" };

			// setup title view
			StackLayout layout = new StackLayout() { BackgroundColor = Colors.White };
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			layout.Children.Add(label);
			Shell.SetTitleView(page, layout);

			Assert.Equal("Binding", label.Text);
			page.BindingContext = new { Text = "Binding Changed" };
			Assert.Equal("Binding Changed", label.Text);
		}

		[Fact]
		public async Task VisualPropagationPageLevel()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));

			// setup title view
			StackLayout titleView = new StackLayout() { BackgroundColor = Colors.White };
			Button button = new Button();
			titleView.Children.Add(button);
			Shell.SetTitleView(page, titleView);
			IVisualController visualController = button as IVisualController;


			Assert.Equal(page, titleView.Parent);

			Assert.Equal(VisualMarker.Default, ((IVisualController)button).EffectiveVisual);
			page.Visual = VisualMarker.Material;
			Assert.Equal(VisualMarker.Material, ((IVisualController)button).EffectiveVisual);
		}

		[Fact]
		public async Task VisualPropagationShellLevel()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));

			// setup title view
			StackLayout titleView = new StackLayout() { BackgroundColor = Colors.White };
			Button button = new Button();
			titleView.Children.Add(button);
			Shell.SetTitleView(page, titleView);
			IVisualController visualController = button as IVisualController;


			Assert.Equal(page, titleView.Parent);
			Assert.Equal(VisualMarker.Default, ((IVisualController)button).EffectiveVisual);
			shell.Visual = VisualMarker.Material;
			Assert.Equal(VisualMarker.Material, ((IVisualController)button).EffectiveVisual);
		}

		[Fact]
		public async Task FlyoutViewVisualPropagation()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));


			// setup title view
			StackLayout flyoutView = new StackLayout() { BackgroundColor = Colors.White };
			Button button = new Button();
			flyoutView.Children.Add(button);
			shell.SetValue(Shell.FlyoutHeaderProperty, flyoutView);

			IVisualController visualController = button as IVisualController;
			Assert.Equal(VisualMarker.Default, visualController.EffectiveVisual);
			shell.Visual = VisualMarker.Material;
			Assert.Equal(VisualMarker.Material, visualController.EffectiveVisual);
		}

		[Fact]
		public async Task FlyoutViewBindingContext()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));
			shell.BindingContext = new { Text = "Binding" };

			// setup title view
			StackLayout flyoutView = new StackLayout() { BackgroundColor = Colors.White };
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			flyoutView.Children.Add(label);
			shell.SetValue(Shell.FlyoutHeaderProperty, flyoutView);

			Assert.Equal("Binding", label.Text);
			shell.BindingContext = new { Text = "Binding Changed" };
			Assert.Equal("Binding Changed", label.Text);
			shell.SetValue(Shell.FlyoutHeaderProperty, new ContentView());
			Assert.Null(flyoutView.BindingContext);
		}

		[Fact]
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


			Assert.Equal(bindingContext, menuItem.BindingContext);
			Assert.Equal(bindingContext, menuItem2.BindingContext);
		}

		[Fact]
		public async Task TitleViewLogicalChild()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));
			page.BindingContext = new { Text = "Binding" };

			// setup title view
			StackLayout layout = new StackLayout() { BackgroundColor = Colors.White };
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			layout.Children.Add(label);
			Shell.SetTitleView(page, layout);


			Assert.Contains(layout, page.LogicalChildren);
			Assert.DoesNotContain(layout, page.InternalChildren);
			Assert.Contains(layout, ((IVisualTreeElement)page).GetVisualChildren());
		}

		[Fact]
		public async Task ShellLogicalChildrenContainsTitleView()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));
			page.BindingContext = new { Text = "Binding" };

			// setup title view
			StackLayout layout = new StackLayout() { BackgroundColor = Colors.White };
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			layout.Children.Add(label);
			Shell.SetTitleView(shell, layout);

			// Should contain Layout in logical children of shell.
			Assert.Contains(layout, shell.LogicalChildrenInternal);

			Shell.SetTitleView(shell, null);

			// Should now be removed.
			Assert.DoesNotContain(layout, shell.LogicalChildrenInternal);
		}

		[Fact]
		public async Task FlyoutHeaderLogicalChild()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page));

			// setup title view
			StackLayout layout = new StackLayout() { BackgroundColor = Colors.White };
			Label label = new Label();
			label.SetBinding(Label.TextProperty, "Text");
			layout.Children.Add(label);


			shell.FlyoutHeader = null;
			shell.FlyoutHeader = layout;

			Assert.Contains(layout, shell.LogicalChildrenInternal);
			shell.FlyoutHeader = null;

			Assert.DoesNotContain(layout, shell.LogicalChildrenInternal);
		}


		[Fact]
		public async Task ShellFlyoutChangeableOnShellWithFlyoutItem()
		{
			Shell shell = new Shell();
			var flyoutItem = CreateShellItem<FlyoutItem>();
			shell.Items.Add(flyoutItem);
			Assert.Equal(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());
			shell.FlyoutBehavior = FlyoutBehavior.Locked;
			Assert.Equal(FlyoutBehavior.Locked, shell.GetEffectiveFlyoutBehavior());
			shell.FlyoutBehavior = FlyoutBehavior.Disabled;
			Assert.Equal(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());
		}

		[Fact]
		public async Task ShellFlyoutChangeableOnShellWithTabBar()
		{
			Shell shell = new Shell();
			var tabBarItem = CreateShellItem<TabBar>();
			shell.Items.Add(tabBarItem);
			Assert.Equal(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());
			shell.FlyoutBehavior = FlyoutBehavior.Flyout;
			Assert.Equal(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());
			shell.FlyoutBehavior = FlyoutBehavior.Locked;
			Assert.Equal(FlyoutBehavior.Locked, shell.GetEffectiveFlyoutBehavior());
		}


		[Fact]
		public async Task ShellFlyoutBehaviorCalculation()
		{
			Shell shell = new Shell();
			ContentPage page = new ContentPage();
			shell.Items.Add(CreateShellItem(page: page));
			Assert.Equal(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());

			Shell.SetFlyoutBehavior(page, FlyoutBehavior.Disabled);
			Shell.SetFlyoutBehavior(shell.Items[0].Items[0].Items[0], FlyoutBehavior.Flyout);
			Shell.SetFlyoutBehavior(shell.Items[0].Items[0], FlyoutBehavior.Disabled);
			Shell.SetFlyoutBehavior(shell.Items[0], FlyoutBehavior.Locked);

			Assert.Equal(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());

			page.ClearValue(Shell.FlyoutBehaviorProperty);
			Assert.Equal(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());

			shell.Items[0].Items[0].Items[0].ClearValue(Shell.FlyoutBehaviorProperty);
			Assert.Equal(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());

			shell.Items[0].Items[0].ClearValue(Shell.FlyoutBehaviorProperty);
			Assert.Equal(FlyoutBehavior.Locked, shell.GetEffectiveFlyoutBehavior());

			shell.Items[0].ClearValue(Shell.FlyoutBehaviorProperty);
			Assert.Equal(FlyoutBehavior.Flyout, shell.GetEffectiveFlyoutBehavior());
		}

		[Fact]
		public async Task TabBarAutoCreation()
		{
			Shell shell = new Shell();
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));

			Assert.Single(shell.Items);
			Assert.Equal(3, shell.Items[0].Items.Count);

			Assert.Equal(FlyoutBehavior.Disabled, shell.GetEffectiveFlyoutBehavior());


			shell = new Shell();
			shell.Items.Add(new TabBar());
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));

			Assert.Equal(2, shell.Items.Count);
			Assert.Empty(shell.Items[0].Items);
			Assert.Equal(3, shell.Items[1].Items.Count);


			shell = new Shell();
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(new TabBar());
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));
			shell.Items.Add(ShellItem.CreateFromShellSection(CreateShellSection<Tab>()));

			Assert.Equal(3, shell.Items.Count);
			Assert.Equal(3, shell.Items[0].Items.Count);
			Assert.Empty(shell.Items[1].Items);
			Assert.Equal(3, shell.Items[0].Items.Count);
		}


		[Fact]
		public async Task NavigatedFiresAfterContentIsCreatedWhenUsingTemplate()
		{

			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");

			shell.Items.Add(item1);
			Routing.RegisterRoute("cat", typeof(ContentPage));
			Routing.RegisterRoute("details", typeof(ContentPage));

			await shell.GoToAsync("cat");
			await shell.GoToAsync("details");

			Assert.Equal("//rootlevelcontent1/cat/details", shell.CurrentState.Location.ToString());
			await shell.GoToAsync("//rootlevelcontent1/details");
			Assert.Equal("//rootlevelcontent1/details", shell.CurrentState.Location.ToString());
		}

		[Fact]
		public async Task ShellItemNotVisibleWhenContentPageNotVisible()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(new ContentPage() { IsVisible = false });
			var item2 = CreateShellItem();

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Assert.DoesNotContain(item1, GetItems(shell));
			Assert.Contains(item2, GetItems(shell));
		}

		[Fact]
		public async Task BaseShellItemNotVisible()
		{
			var shell = new Shell();
			var item1 = CreateShellItem();
			var item2 = CreateShellItem();

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			item1.IsVisible = false;
			Assert.DoesNotContain(item1, GetItems(shell));
			Assert.Contains(item2, GetItems(shell));

			item1.IsVisible = true;
			Assert.Contains(item1, GetItems(shell));

			item1.Items[0].IsVisible = false;
			Assert.DoesNotContain(item1, GetItems(shell));
			item1.Items[0].IsVisible = true;
			Assert.Contains(item1, GetItems(shell));

			item1.Items[0].Items[0].IsVisible = false;
			Assert.DoesNotContain(item1, GetItems(shell));
			item1.Items[0].Items[0].IsVisible = true;
			Assert.Contains(item1, GetItems(shell));
		}

		[Fact]
		public async Task CantNavigateToNotVisibleShellItem()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(shellItemRoute: "NotVisible");
			var item2 = CreateShellItem();

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			item1.IsVisible = false;

			await Assert.ThrowsAnyAsync<Exception>(() => shell.GoToAsync($"//NotVisible"));

			Assert.Equal(shell.CurrentItem, item2);
		}


		[Fact]
		public async Task FlyoutItemVisible()
		{
			var shell = new Shell();
			var item1 = CreateShellItem<FlyoutItem>(shellItemRoute: "NotVisible");
			var item2 = CreateShellItem();

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Shell.SetFlyoutItemIsVisible(item1, false);
			Assert.Contains(item1, GetItems(shell));

			bool hasFlyoutItem =
				(shell as IShellController)
					.GenerateFlyoutGrouping()
					.SelectMany(i => i)
					.Contains(item1);

			Assert.False(hasFlyoutItem);
		}

		[Fact]
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

			Assert.Null(clearedContent.Parent);
			Assert.Equal(2, mainTab.Items.Count);
			Assert.Equal(content1, mainTab.CurrentItem);
		}

		[Fact]
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

			Assert.Null(item1.Parent);
			Assert.Equal(2, shell.Items.Count);
			Assert.Equal(item2, shell.CurrentItem);
		}

		[Fact]
		public async Task ShellSectionCollectionClear()
		{
			var shell = new Shell();
			var item1 = CreateShellItem();
			shell.Items.Add(item1);

			var section1 = CreateShellSection();
			var section2 = CreateShellSection();
			var clearedSection = item1.Items[0];

			Assert.NotNull(clearedSection.Parent);
			item1.Items.Clear();
			item1.Items.Add(section1);
			item1.Items.Add(section2);

			Assert.Null(clearedSection.Parent);
			Assert.Equal(2, item1.Items.Count);
			Assert.Equal(section1, shell.CurrentItem.CurrentItem);
		}

		[Fact]
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

			Assert.Equal(0, shellSection.Items.IndexOf(shellContent));
			Assert.Equal(0, shellSectionController.GetItems().IndexOf(shellContent));

			hideMe.IsVisible = false;

			Assert.Equal(0, shellSection.Items.IndexOf(shellContent));
			Assert.Equal(-1, shellSectionController.GetItems().IndexOf(shellContent));

			hideMe.IsVisible = true;

			Assert.Equal(0, shellSection.Items.IndexOf(shellContent));
			Assert.Equal(0, shellSectionController.GetItems().IndexOf(shellContent));
		}

		[Fact]
		public async Task HidingShellItemSetsNewCurrentItem()
		{
			var shell = new Shell();
			ContentPage contentPage = new ContentPage();
			var item1 = CreateShellItem(contentPage);
			shell.Items.Add(item1);
			var item2 = CreateShellItem();
			shell.Items.Add(item2);

			Assert.Equal(shell.CurrentItem, item1);
			contentPage.IsVisible = false;
			Assert.Equal(shell.CurrentItem, item2);
		}


		[Fact]
		public async Task HidingShellSectionSetsNewCurrentItem()
		{
			var shell = new Shell();
			ContentPage contentPage = new ContentPage();
			var item1 = CreateShellItem(contentPage);
			shell.Items.Add(item1);
			var shellSection2 = CreateShellSection();
			item1.Items.Add(shellSection2);

			Assert.Equal(shell.CurrentItem.CurrentItem, item1.Items[0]);
			contentPage.IsVisible = false;
			Assert.Equal(shell.CurrentItem.CurrentItem, shellSection2);
		}


		[Fact]
		public async Task HidingShellContentSetsNewCurrentItem()
		{
			var shell = new Shell();
			ContentPage contentPage = new ContentPage();
			var item1 = CreateShellItem(contentPage);
			shell.Items.Add(item1);
			var shellContent2 = CreateShellContent();
			item1.Items[0].Items.Add(shellContent2);

			Assert.Equal(shell.CurrentItem.CurrentItem.CurrentItem, item1.Items[0].Items[0]);
			contentPage.IsVisible = false;
			Assert.Equal(shell.CurrentItem.CurrentItem.CurrentItem, shellContent2);
		}

		[Fact]
		public async Task ShellLocationRestoredWhenItemsAreReAdded()
		{
			var shell = new Shell();
			shell.Items.Add(CreateShellItem(shellContentRoute: "root1"));
			shell.Items.Add(CreateShellItem(shellContentRoute: "root2"));

			await shell.GoToAsync("//root2");
			Assert.Equal("//root2", shell.CurrentState.Location.ToString());

			shell.Items.Add(CreateShellItem(shellContentRoute: "root1"));
			shell.Items.Add(CreateShellItem(shellContentRoute: "root2"));

			shell.Items.Clear();
			Assert.Equal("//root2", shell.CurrentState.Location.ToString());
		}

		[Fact]
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

			Assert.NotNull(item.CurrentItem);
			Assert.NotNull(item.CurrentItem.CurrentItem);
		}

		[Fact]
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

			Assert.NotNull(item.CurrentItem);
			Assert.NotNull(item.CurrentItem.CurrentItem);
		}

		[Fact]

		public async Task GetCurrentPageInShellNavigation()
		{
			Shell shell = new TestShell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");

			shell.Items.Add(item1);
			Routing.RegisterRoute("cat", typeof(ContentPage));

			Page page = null;

			shell.Navigated += (_, __) =>
			{
				page = shell.CurrentPage;
			};

			await shell.GoToAsync("cat");
			Assert.NotNull(page);
			Assert.IsType<ContentPage>(page);
			Assert.Equal(shell.Navigation.NavigationStack[1], page);
		}

		[Fact]
		public async Task GetCurrentPageBetweenSections()
		{
			var shell = new Shell();
			_ = new Window() { Page = shell };
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
			Assert.NotNull(page);
			Assert.IsType<ShellTestPage>(page);
			Assert.Equal((tabfour as IShellSectionController).PresentedPage, page);
		}

		[Fact]
		public void GetCurrentPageOnInit()
		{
			var shell = new Shell();
			_ = new Window() { Page = shell };
			Page page = null;
			shell.Navigated += (_, __) =>
			{
				page = shell.CurrentPage;
			};
			var tabone = MakeSimpleShellSection("tabone", "content");
			shell.Items.Add(tabone);
			Assert.NotNull(page);
		}

		[Fact]
		public async Task HotReloadStaysOnActiveItem()
		{
			Shell shell = new Shell();

			shell.Items.Add(CreateShellItem(shellItemRoute: "item1"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "item2"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "item3"));

			await shell.GoToAsync("//item3");
			Assert.Equal("//item3", shell.CurrentState.Location.ToString());

			shell.Items.Add(CreateShellItem(shellItemRoute: "item1"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "item2"));
			shell.Items.Add(CreateShellItem(shellItemRoute: "item3"));

			shell.Items.RemoveAt(0);
			shell.Items.RemoveAt(0);
			shell.Items.RemoveAt(0);

			Assert.Equal("//item3", shell.CurrentState.Location.ToString());

		}

		[Theory]
		[InlineData("ContentPage")]
		[InlineData("ShellItem")]
		[InlineData("Shell")]
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

			Assert.False((shellItem as IShellItemController).ShowTabs);
		}

		[Fact]
		public void SendStructureChangedFiresWhenAddingItems()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());

			int count = 0;
			int previousCount = 0;
			(shell as IShellController).StructureChanged += (_, __) => count++;


			shell.Items.Add(CreateShellItem());
			Assert.True(count > previousCount, "StructureChanged not fired when adding Shell Item");

			previousCount = count;
			shell.CurrentItem.Items.Add(CreateShellSection());
			Assert.True(count > previousCount, "StructureChanged not fired when adding Shell Section");

			previousCount = count;
			shell.CurrentItem.CurrentItem.Items.Add(CreateShellContent());
			Assert.True(count > previousCount, "StructureChanged not fired when adding Shell Content");
		}

		[Fact]
		public void VisualTreeHelperCount()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());
			shell.CurrentItem.Items.Add(CreateShellSection());
			shell.CurrentItem.CurrentItem.Items.Add(CreateShellContent());
			var shellCount = (shell as IVisualTreeElement).GetVisualChildren();
			var shellItemCount = (shell.CurrentItem as IVisualTreeElement).GetVisualChildren();
			Assert.Single(shellCount);
			Assert.Equal(2, shellItemCount.Count);
		}

		[Fact]
		public void ShellToolbarTitle()
		{
			var shellContent = CreateShellContent();
			TestShell testShell = new TestShell(shellContent)
			{
				Title = "Shell Title"
			};

			var shellToolBar = testShell.Toolbar;

			Assert.Equal(String.Empty, shellToolBar.Title);

			shellContent.Title = "Shell Content Title";
			Assert.Equal("Shell Content Title", shellToolBar.Title);

			var page = testShell.CurrentPage;
			page.Title = "ContentPage Title";
			Assert.Equal("ContentPage Title", shellToolBar.Title);
		}

		[Fact]
		public void ShellToolbarTitleChangesWithCurrentItem()
		{
			var shellContent = CreateShellContent();
			var shellContent2 = CreateShellContent();
			TestShell testShell = new TestShell(shellContent, shellContent2)
			{
				Title = "Shell Title"
			};

			shellContent.Title = "Content 1";
			shellContent2.Title = "Content 2";

			var shellToolBar = testShell.Toolbar;
			Assert.Equal("Content 1", shellToolBar.Title);

			testShell.CurrentItem = shellContent2;
			Assert.Equal("Content 2", shellToolBar.Title);
		}

		[Fact]
		public void ShellToolbarTitleEmptyStringOnNullOrEmptyPageTitle()
		{
			var shellContent = CreateShellContent();
			var shellContent2 = CreateShellContent();
			TestShell testShell = new TestShell(shellContent, shellContent2)
			{
				Title = "Shell Title"
			};

			var shellToolBar = testShell.Toolbar;
			Assert.Equal(String.Empty, shellToolBar.Title);

			var page = testShell.CurrentPage;
			page.Title = String.Empty;
			Assert.Equal(String.Empty, shellToolBar.Title);
			page.Title = "set";
			Assert.Equal("set", shellToolBar.Title);
			page.Title = null;
			Assert.Equal(String.Empty, shellToolBar.Title);
		}

		[Fact]
		public async Task ShellToolbarTitleIgnoresModalTitle()
		{
			var shellContent = CreateShellContent();
			shellContent.Title = "Shell Title";
			TestShell testShell = new TestShell(shellContent);

			await testShell.CurrentSection.Navigation.PushModalAsync(new ContentPage()
			{
				Title = "Modal Page"
			});

			var shellToolBar = testShell.Toolbar;
			Assert.Equal("Shell Title", shellToolBar.Title);

			await testShell.CurrentSection.Navigation.PopModalAsync();

			Assert.Equal("Shell Title", shellToolBar.Title);
		}

		[Fact]
		public async Task ShellToolbarTitleIgnoresModalTitle_ShellContent()
		{
			var shellContent = CreateShellContent();
			shellContent.Title = "Shell Content Title";

			var flyoutItem = new FlyoutItem()
			{
				Title = "Flyout Item 1",
				Items =
				{
					shellContent
				}
			};

			TestShell testShell = new TestShell(flyoutItem)
			{
				Title = "Welcome to Shell"
			};

			await testShell.CurrentSection.Navigation.PushModalAsync(new ContentPage()
			{
				Title = "Modal Page"
			});

			var shellToolBar = testShell.Toolbar;
			Assert.Equal("Shell Content Title", shellToolBar.Title);
			await testShell.CurrentSection.Navigation.PopModalAsync();
			Assert.Equal("Shell Content Title", shellToolBar.Title);
		}

		[Fact]
		public async Task PushedPageDoesntUsesTitleOnShellSection()
		{
			var shellContent = CreateShellContent();
			shellContent.Title = "Shell Content Title";

			var shellSection = new ShellSection();
			shellSection.Title = "Shell Section Title";
			shellSection.Items.Add(shellContent);

			TestShell testShell = new TestShell(shellSection)
			{
				Title = "Shell Title"
			};

			var shellToolBar = testShell.Toolbar;
			Assert.Equal("Shell Content Title", shellToolBar.Title);

			await testShell.CurrentSection.Navigation.PushAsync(new ContentPage());

			Assert.Equal("", shellToolBar.Title);

			await testShell.CurrentSection.Navigation.PopAsync();
			Assert.Equal("Shell Content Title", shellToolBar.Title);
		}

		[Fact]
		public void WindowTitleSetToShellTitle()
		{
			TestShell testShell = new TestShell(new ContentPage())
			{
				Title = "Shell Title"
			};

			Window window = new Window()
			{
				Page = testShell
			};

			Assert.Equal("Shell Title", (window as IWindow).Title);

			window.Title = "Window Title";
			Assert.Equal("Window Title", (window as IWindow).Title);

			window.Title = null;
			Assert.Equal("Shell Title", (window as IWindow).Title);
		}

		[Fact]
		public void FlyoutIsPresentedRemainsTrueAfterShellIsInitialized()
		{
			TestShell testShell = new TestShell()
			{
				FlyoutIsPresented = true
			};

			testShell.Items.Add(CreateShellItem<FlyoutItem>());

			Assert.True(testShell.FlyoutIsPresented);
		}

		[Fact]
		public void ShellToolbarNotVisibleWithBasicContentPage()
		{
			TestShell testShell = new TestShell(new ContentPage());
			var shellToolBar = testShell.Toolbar;
			Assert.False(shellToolBar.IsVisible);
		}

		[Fact]
		public async Task ChangingTextColorAfterSetValueFromRendererDoesntFire()
		{
			Button button = new Button()
			{
				TextColor = Colors.Green
			};

			button.SetValue(
				Button.TextColorProperty, Colors.Purple, specificity: SetterSpecificity.FromHandler);

			bool fired = false;
			button.PropertyChanged += (x, args) =>
			{
				if (args.PropertyName == Button.TextColorProperty.PropertyName)
				{
					fired = true;
				}
			};

			button.TextColor = Colors.Green; // Won't fire a property Changed event
			Assert.Equal(Colors.Green, button.TextColor);
			Assert.True(fired);
		}

		[Fact]
		public async Task ShellSectionChangedFires()
		{
			var page1 = new ContentPage()
			{ Content = new Label() { Text = "Page 1" }, Title = "Page 1" };
			var page2 = new ContentPage()
			{ Content = new Label() { Text = "Page 2" }, Title = "Page 2" };

			var shell = new Shell();
			var tabBar = new TabBar()
			{
				Items =
				{
					new ShellContent(){ Content = page1 },
					new ShellContent(){ Content = page2 },
				}
			};

			shell.Items.Add(tabBar);
			shell.CurrentItem = page2;
			page2.IsVisible = false;
			Assert.Equal(shell.CurrentPage, page1);

			bool fired = false;
			shell.CurrentItem.PropertyChanged += (x, args) =>
			{
				if (args.PropertyName == nameof(ShellItem.CurrentItem))
				{
					fired = true;
				}
			};

			page2.IsVisible = true;
			shell.CurrentItem = page2;
			Assert.Equal(shell.CurrentPage, page2);
			Assert.True(fired);
		}
	}
}
