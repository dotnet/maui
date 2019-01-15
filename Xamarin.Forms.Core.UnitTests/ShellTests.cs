using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ShellTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			Device.SetFlags(new[] { Shell.ShellExperimental });
			base.Setup();

		}

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

		ShellSection MakeSimpleShellSection(string route, string contentRoute)
		{
			return MakeSimpleShellSection(route, contentRoute, new ShellTestPage());
		}

		ShellSection MakeSimpleShellSection (string route, string contentRoute, ContentPage contentPage)
		{
			var shellSection = new ShellSection();
			shellSection.Route = route;
			var shellContent = new ShellContent { Content = contentPage, Route = contentRoute };
			shellSection.Items.Add(shellContent);
			return shellSection;
		}

		[QueryProperty("SomeQueryParameter", "SomeQueryParameter")]
		public class ShellTestPage : ContentPage
		{
			public string SomeQueryParameter { get; set; }
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

			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/one/tabone/content/"));

			shell.GoToAsync(new ShellNavigationState("app:///s/two/tabfour/"));

			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tabfour/content/"));
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

			await shell.GoToAsync("/tab22");
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tab22/content/"));

			await shell.GoToAsync("tab21");
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tab21/content/"));

			await shell.GoToAsync("/tab23");
			Assert.That(shell.CurrentState.Location.ToString(), Is.EqualTo("app:///s/two/tab23/content/"));

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

			shell.FlyoutHeader = label;

			Assert.AreEqual(((IShellController)shell).FlyoutHeader, label);

			Label label2 = null;

			shell.FlyoutHeaderTemplate = new DataTemplate(() =>
			{
				return label2 = new Label();
			});

			Assert.AreEqual(((IShellController)shell).FlyoutHeader, label2);
			Assert.AreEqual(((IShellController)shell).FlyoutHeader.BindingContext, label);

			shell.FlyoutHeaderTemplate = null;

			Assert.AreEqual(((IShellController)shell).FlyoutHeader, label);
		}
	}
}
