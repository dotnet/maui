using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ShellNavigatedArgsTests : ShellTestBase
	{
		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Routing.Clear();
		}

		[Test]
		public async Task RemoveInnerPagesNavigatingArgs()
		{
			Routing.RegisterRoute("SecondPageView", typeof(ContentPage));
			Routing.RegisterRoute("ThirdPageView", typeof(ContentPage));
			Routing.RegisterRoute("FourthPage", typeof(ContentPage));

			var shell = new TestShell(CreateShellItem<FlyoutItem>(shellContentRoute: "HomePageView"));

			await shell.GoToAsync("//HomePageView/SecondPageView/ThirdPageView");
			await shell.GoToAsync("//HomePageView/FourthPage");

			shell.TestNavigatedArgs(ShellNavigationSource.Pop, "//HomePageView/SecondPageView/ThirdPageView", "//HomePageView/FourthPage");
			Assert.AreEqual(3, shell.NavigatedCount);
		}

		[Test]
		public async Task PopToRootSetsCorrectNavigationSource()
		{
			var shell = new TestShell(CreateShellItem());
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PopToRootAsync();
			Assert.AreEqual(ShellNavigationSource.PopToRoot, shell.LastShellNavigatingEventArgs.Source);

			await shell.Navigation.PushAsync(new ContentPage());
			await shell.Navigation.PushAsync(new ContentPage());

			await shell.Navigation.PopAsync();
			Assert.AreEqual(ShellNavigationSource.Pop, shell.LastShellNavigatingEventArgs.Source);

			await shell.Navigation.PopAsync();
			Assert.AreEqual(ShellNavigationSource.PopToRoot, shell.LastShellNavigatingEventArgs.Source);
		}

		[Test]
		public async Task PushingSetsCorrectNavigationSource()
		{
			var shell = new TestShell(CreateShellItem(shellItemRoute: "item1"));
			shell.RegisterPage(nameof(PushingSetsCorrectNavigationSource));
			await shell.GoToAsync(nameof(PushingSetsCorrectNavigationSource));

			shell.TestNavigatingArgs(ShellNavigationSource.Push,
				"//item1", $"{nameof(PushingSetsCorrectNavigationSource)}");

			shell.TestNavigatedArgs(ShellNavigationSource.Push,
				"//item1", $"//item1/{nameof(PushingSetsCorrectNavigationSource)}");
		}

		[Test]
		public async Task ChangingShellItemSetsCorrectNavigationSource()
		{
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item1"),
				CreateShellItem(shellItemRoute: "item2")
			);

			await shell.GoToAsync("//item2");

			shell.TestNavigationArgs(ShellNavigationSource.ShellItemChanged,
				"//item1", "//item2");
		}

		[Test]
		public async Task ChangingShellSectionSetsCorrectNavigationSource()
		{
			var shell = new TestShell(
				CreateShellItem(shellSectionRoute: "item1")
			);

			shell.Items[0].Items.Add(CreateShellSection(shellContentRoute: "item2"));

			await shell.GoToAsync("//item2");

			shell.TestNavigationArgs(ShellNavigationSource.ShellSectionChanged,
				"//item1", "//item2");
		}

		[Test]
		public async Task PoppingSamePageSetsCorrectNavigationSource()
		{
			Routing.RegisterRoute("detailspage", typeof(ContentPage));
			var shell = new TestShell(CreateShellItem(shellItemRoute: "item1"));
			await shell.GoToAsync("detailspage/detailspage");
			await shell.Navigation.PopAsync();


			shell.TestNavigatingArgs(ShellNavigationSource.Pop,
				"//item1/detailspage/detailspage", $"..");

			shell.TestNavigatedArgs(ShellNavigationSource.Pop,
				"//item1/detailspage/detailspage", $"//item1/detailspage");
		}

		[Test]
		public async Task ChangingShellContentSetsCorrectNavigationSource()
		{
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "item1")
			);

			shell.Items[0].Items[0].Items.Add(CreateShellContent(shellContentRoute: "item2"));

			await shell.GoToAsync("//item2");

			shell.TestNavigationArgs(ShellNavigationSource.ShellContentChanged,
				"//item1", "//item2");
		}

		[Test]
		public async Task InsertPageSetsCorrectNavigationSource()
		{
			Routing.RegisterRoute("pagemiddle", typeof(ContentPage));
			Routing.RegisterRoute("page", typeof(ContentPage));
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item")
			);

			await shell.GoToAsync("//item/page");
			await shell.GoToAsync("//item/pagemiddle/page");

			shell.TestNavigationArgs(ShellNavigationSource.Insert,
				"//item/page", "//item/pagemiddle/page");
		}


		[Test]
		public async Task InsertPageFromINavigationSetsCorrectNavigationSource()
		{
			Routing.RegisterRoute("pagemiddle", typeof(ContentPage));
			Routing.RegisterRoute("page", typeof(ContentPage));
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item")
			);

			await shell.GoToAsync("//item/page");
			ContentPage contentPage = new ContentPage();
			Routing.SetRoute(contentPage, "pagemiddle");
			shell.Navigation.InsertPageBefore(contentPage, shell.Navigation.NavigationStack.Last());

			shell.TestNavigationArgs(ShellNavigationSource.Insert,
				"//item/page", "//item/pagemiddle/page");
		}


		[Test]
		public async Task RemovePageFromINavigationSetsCorrectNavigationSource()
		{
			Routing.RegisterRoute("pagemiddle", typeof(ContentPage));
			Routing.RegisterRoute("page", typeof(ContentPage));
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item")
			);

			await shell.GoToAsync("//item/pagemiddle/page");
			shell.Navigation.RemovePage(shell.Navigation.NavigationStack[1]);

			shell.TestNavigationArgs(ShellNavigationSource.Remove,
				"//item/pagemiddle/page", "//item/page");
		}

		[Test]
		public async Task RemovePageSetsCorrectNavigationSource()
		{
			Routing.RegisterRoute("pagemiddle", typeof(ContentPage));
			Routing.RegisterRoute("page", typeof(ContentPage));
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item")
			);

			await shell.GoToAsync("//item/pagemiddle/page");
			await shell.GoToAsync("//item/page");


			shell.TestNavigationArgs(ShellNavigationSource.Remove,
				"//item/pagemiddle/page", "//item/page");
		}

		[Test]
		public async Task InitialNavigatingArgs()
		{
			var shell = new TestShell(
				CreateShellItem(shellItemRoute: "item")
			);

			shell.TestNavigationArgs(ShellNavigationSource.ShellItemChanged,
				null, "//item");
		}
	}
}
