using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ShellNavigatingTests : ShellTestBase
	{

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

		[TestCase("PopToRoot")]
		[TestCase("Pop")]
		public async Task DeferPopNavigation(string testCase)
		{
			TestShell shell = new TestShell()
			{
				Items = { CreateShellItem<FlyoutItem>() }
			};

			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());

			ShellNavigatingDeferral _token = null;
			shell.Navigating += async (_, args) =>
			{
				_token = args.GetDeferral();
				await Task.Delay(500);
				Assert.AreEqual(3, shell.Navigation.NavigationStack.Count);
				_token.Complete();
			};

			var source = new TaskCompletionSource<object>();
			shell.Navigated += (_, args) =>
			{
				source.SetResult(true);
			};

			if (testCase == "Pop")
			{
				await shell.Navigation.PopAsync();
				await source.Task;
				Assert.AreEqual(2, shell.Navigation.NavigationStack.Count);
			}
			else
			{
				await shell.Navigation.PopToRootAsync();
				await source.Task;
				Assert.AreEqual(1, shell.Navigation.NavigationStack.Count);
			}
		}

		[TestCase("PopToRoot")]
		[TestCase("Pop")]
		[TestCase("GoToAsync")]
		[TestCase("Push")]
		public async Task NavigationTaskCompletesAfterDeferalHasFinished(string testCase)
		{
			Routing.RegisterRoute(nameof(NavigationTaskCompletesAfterDeferalHasFinished), typeof(ContentPage));
			var shell = new TestShell()
			{
				Items = { CreateShellItem<FlyoutItem>() }
			};

			ShellNavigatingDeferral _token = null;
			shell.Navigating += async (_, args) =>
			{
				_token = args.GetDeferral();
				await Task.Delay(500);
				_token.Complete();
			};

			switch (testCase)
			{
				case "PopToRoot":
					await shell.Navigation.PopToRootAsync();
					break;
				case "Pop":
					await shell.Navigation.PopAsync();
					break;
				case "GoToAsync":
					await shell.GoToAsync(nameof(NavigationTaskCompletesAfterDeferalHasFinished));
					break;
				case "Push":
					await shell.Navigation.PushAsync(new ContentPage());
					break;
			}

			Assert.IsTrue(_token.IsCompleted);
		}

		[Test]
		public void CompletingTheSameDeferalTokenTwiceDoesntDoAnything()
		{
			var args = CreateShellNavigatedEventArgs();
			var token = args.GetDeferral();
			args.GetDeferral();

			Assert.AreEqual(2, args.DeferralCount);
			token.Complete();
			Assert.AreEqual(1, args.DeferralCount);
			token.Complete();
			Assert.AreEqual(1, args.DeferralCount);
		}

		[Test]
		public async Task DotDotNavigateBackFromPagesWithDefaultRoute()
		{
			var flyoutItem = CreateShellItem<FlyoutItem>();
			var itemRoute = Routing.GetRoute(flyoutItem.CurrentItem.CurrentItem);
			var page1 = new ContentPage();
			var page2 = new ContentPage();
			TestShell shell = new TestShell()
			{
				Items = { flyoutItem }
			};

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}"));

			await shell.Navigation.PushAsync(page1);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}"));

			await shell.Navigation.PushAsync(page2);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}/{Routing.GetRoute(page2)}"));

			await shell.GoToAsync("..");

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}"));
		}

		[Test]
		public async Task NavigationPushAndPopBasic()
		{
			var flyoutItem =
				CreateShellItem<FlyoutItem>(
					 shellItemRoute: "item",
					 shellSectionRoute: "section",
					 shellContentRoute: "content"
					);

			var itemRoute = "item/section/content";
			var page1 = new ContentPage();
			var page2 = new ContentPage();
			var shell = new TestShell(flyoutItem);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}"));

			await shell.Navigation.PushAsync(page1);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}"));

			await shell.Navigation.PushAsync(page2);

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}/{Routing.GetRoute(page2)}"));

			await shell.Navigation.PopAsync();

			Assert.That(shell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{itemRoute}/{Routing.GetRoute(page1)}"));
		}

		[Test]
		public async Task NavigateToDefaultShellContent()
		{
			TestShell testShell = new TestShell(CreateShellItem<FlyoutItem>());
			var page = new ContentPage();

			var contentRoute = testShell.CurrentItem.CurrentItem.CurrentItem.Route;
			var pageRoute = Routing.GetRoute(page);

			await testShell.Navigation.PushAsync(new ContentPage());
			await testShell.Navigation.PushAsync(page);

			await testShell.GoToAsync($"//{contentRoute}/{pageRoute}");


			Assert.That(testShell.CurrentState.Location.ToString(),
				Is.EqualTo($"//{contentRoute}/{pageRoute}"));
		}

		[Test]
		public async Task PopToRootWithMultipleFlyoutItems()
		{
			TestShell testShell = new TestShell(
				CreateShellItem<FlyoutItem>(shellItemRoute: "store", shellContentRoute: "home"),
				CreateShellItem<FlyoutItem>(shellItemRoute: "second", shellContentRoute: "home")
			);

			await testShell.Navigation.PushAsync(new ContentPage());
			await testShell.Navigation.PushAsync(new ContentPage());
			await testShell.Navigation.PopToRootAsync();
		}

		ShellNavigatingEventArgs CreateShellNavigatedEventArgs() =>
			new ShellNavigatingEventArgs("..", "../newstate", ShellNavigationSource.Push, true);
	}
}
