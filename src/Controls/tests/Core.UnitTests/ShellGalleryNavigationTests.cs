using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public class ShellGalleryNavigationTests : ShellTestBase
{
	[Fact]
	public async Task PushPopInsertAndRemoveKeepTabAndNavigationStacksInSync()
	{
		var root = new ContentPage { Title = "ShellNavigation" };
		var shell = CreateShell(root);
		var options = new ContentPage { Title = "OptionsPage" };
		var first = new ContentPage { Title = "SubPage1" };
		var second = new ContentPage { Title = "SubPage2" };
		var inserted = new ContentPage { Title = "InsertedPage1" };

		AssertStack(shell, root);
		await shell.Navigation.PushAsync(options);
		AssertStack(shell, root, options);
		await shell.Navigation.PushAsync(first);
		AssertStack(shell, root, options, first);
		await shell.Navigation.PushAsync(second);
		AssertStack(shell, root, options, first, second);
		Assert.Same(second, await shell.Navigation.PopAsync());
		AssertStack(shell, root, options, first);
		await shell.Navigation.PopToRootAsync();
		AssertStack(shell, root);

		await shell.Navigation.PushAsync(options);
		shell.Navigation.InsertPageBefore(inserted, options);
		AssertStack(shell, root, inserted, options);
		shell.Navigation.RemovePage(inserted);
		AssertStack(shell, root, options);
		shell.Navigation.InsertPageBefore(inserted, options);
		await shell.Navigation.PushAsync(first);
		AssertStack(shell, root, inserted, options, first);
		await shell.Navigation.PopAsync();
		shell.Navigation.RemovePage(inserted);
		AssertStack(shell, root, options);
		await shell.Navigation.PopAsync();
		AssertStack(shell, root);
	}

	[Theory]
	[InlineData(ShellNavigationSource.Push)]
	[InlineData(ShellNavigationSource.Pop)]
	[InlineData(ShellNavigationSource.PopToRoot)]
	public async Task StackEventsReportSourcePagesAndCurrentState(ShellNavigationSource source)
	{
		var root = new ContentPage { Title = "ShellNavigation" };
		var shell = CreateShell(root);
		var options = new ContentPage { Title = "OptionsPage" };
		var subpage = new ContentPage { Title = "SubPage1" };
		shell.RegisterPage("options", options);
		shell.RegisterPage("subpage", subpage);
		var rootRoute = shell.CurrentState.Location.ToString();

		if (source != ShellNavigationSource.Push)
		{
			await shell.Navigation.PushAsync(options);
			await shell.Navigation.PushAsync(subpage);
		}

		var targetPage = source == ShellNavigationSource.PopToRoot ? root : options;
		var targetRoute = source == ShellNavigationSource.PopToRoot ? rootRoute : $"{rootRoute}/options";
		var navigatingTarget = source switch
		{
			ShellNavigationSource.Push => "options",
			ShellNavigationSource.Pop => "..",
			_ => rootRoute
		};
		await AssertEvents(shell, source, targetPage, targetRoute, async () =>
		{
			if (source == ShellNavigationSource.Push)
				await shell.Navigation.PushAsync(options);
			else if (source == ShellNavigationSource.Pop)
				await shell.Navigation.PopAsync();
			else
				await shell.Navigation.PopToRootAsync();
		}, navigatingTarget);
	}

	[Theory]
	[InlineData(ShellNavigationSource.ShellItemChanged, false)]
	[InlineData(ShellNavigationSource.ShellItemChanged, true)]
	[InlineData(ShellNavigationSource.ShellSectionChanged, false)]
	[InlineData(ShellNavigationSource.ShellSectionChanged, true)]
	[InlineData(ShellNavigationSource.ShellContentChanged, false)]
	[InlineData(ShellNavigationSource.ShellContentChanged, true)]
	public async Task SelectionAndAbsoluteRoutesReportTheSameStructuralEvents(ShellNavigationSource source, bool absoluteRoute)
	{
		var root = new ContentPage { Title = "ShellNavigation" };
		var shell = CreateShell(root);
		var targetPage = new ContentPage { Title = "Page2" };
		string targetRoute;
		Element selection;
		if (source == ShellNavigationSource.ShellItemChanged)
		{
			var item = CreateShellItem(targetPage, shellItemRoute: "page2", shellSectionRoute: "TabA", shellContentRoute: "ContentA1");
			shell.Items.Add(item);
			targetRoute = "//page2/TabA/ContentA1";
			selection = item;
		}
		else if (source == ShellNavigationSource.ShellSectionChanged)
		{
			var section = CreateShellSection(targetPage, shellSectionRoute: "TabB", shellContentRoute: "ContentB1");
			shell.CurrentItem.Items.Add(section);
			targetRoute = "//main/TabB/ContentB1";
			selection = section;
		}
		else
		{
			var content = CreateShellContent(targetPage, shellContentRoute: "Content2");
			shell.CurrentItem.CurrentItem.Items.Add(content);
			targetRoute = "//main/MainTab/Content2";
			selection = content;
		}

		await AssertEvents(shell, source, targetPage, targetRoute, async () =>
		{
			if (absoluteRoute)
				await shell.GoToAsync(targetRoute);
			else
				await shell.Controller.OnFlyoutItemSelectedAsync(selection);
		});

		await AssertEvents(shell, source, root, "//main/MainTab/MainContent",
			() => shell.GoToAsync("//main/MainTab/MainContent"));
	}

	TestShell CreateShell(ContentPage root) =>
		new TestShell(CreateShellItem(root, shellItemRoute: "main", shellSectionRoute: "MainTab", shellContentRoute: "MainContent"));

	static void AssertStack(TestShell shell, params Page[] expected)
	{
		// Shell stores a null root entry and resolves the root page through ShellContent.
		var stack = new Page[] { null }.Concat(expected.Skip(1));
		Assert.Equal(stack, shell.Navigation.NavigationStack);
		Assert.Equal(stack, shell.CurrentItem.CurrentItem.Stack);
		Assert.Same(expected.Last(), shell.CurrentPage);
	}

	static async Task AssertEvents(TestShell shell, ShellNavigationSource source, Page targetPage, string targetRoute, Func<Task> navigate,
		string navigatingTarget = null)
	{
		var previousPage = shell.CurrentPage;
		var previousRoute = shell.CurrentState.Location.ToString();
		var navigating = new List<(ShellNavigatingEventArgs Args, Page Page)>();
		var navigated = new List<(ShellNavigatedEventArgs Args, Page Page)>();
		var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		void OnNavigating(object sender, ShellNavigatingEventArgs args) => navigating.Add((args, shell.CurrentPage));
		void OnNavigated(object sender, ShellNavigatedEventArgs args)
		{
			navigated.Add((args, shell.CurrentPage));
			completed.TrySetResult();
		}
		shell.Navigating += OnNavigating;
		shell.Navigated += OnNavigated;
		try
		{
			await navigate();
			await completed.Task.WaitAsync(TimeSpan.FromSeconds(5));
			var before = Assert.Single(navigating);
			var after = Assert.Single(navigated);
			Assert.Equal(source, before.Args.Source);
			Assert.Equal(previousRoute, before.Args.Current.Location.ToString());
			Assert.Equal(navigatingTarget ?? targetRoute, before.Args.Target.Location.ToString());
			Assert.True(before.Args.CanCancel);
			Assert.False(before.Args.Cancelled);
			Assert.Same(previousPage, before.Page);
			Assert.Equal(source, after.Args.Source);
			Assert.Equal(previousRoute, after.Args.Previous.Location.ToString());
			Assert.Equal(targetRoute, after.Args.Current.Location.ToString());
			Assert.Same(targetPage, after.Page);
			Assert.Same(targetPage, shell.CurrentPage);
			Assert.Equal(targetRoute, shell.CurrentState.Location.ToString());
		}
		finally
		{
			shell.Navigating -= OnNavigating;
			shell.Navigated -= OnNavigated;
		}
	}
}
