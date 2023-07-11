using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class NavigationModelTests : BaseTestFixture
	{
		[Fact]
		public async Task ParentSetsWhenPushingAndUnsetsWhenPopping()
		{
			var mainPage = new ContentPage() { Title = "Main Page" };
			var modal1 = new ContentPage() { Title = "Modal 1" };
			TestWindow testWindow = new TestWindow(mainPage);

			await mainPage.Navigation.PushModalAsync(modal1);

			Assert.Equal(testWindow, modal1.Parent);
			await mainPage.Navigation.PopModalAsync();
			Assert.Null(modal1.Parent);
		}

		[Fact]
		public async Task ModalsWireUpInCorrectOrderWhenPushedBeforeWindowHasBeenCreated()
		{
			var mainPage = new ContentPage() { Title = "Main Page" };
			var modal1 = new ContentPage() { Title = "Modal 1" };
			var modal2 = new ContentPage() { Title = "Modal 2" };

			await mainPage.Navigation.PushModalAsync(modal1);
			await modal1.Navigation.PushModalAsync(modal2);

			TestWindow testWindow = new TestWindow(mainPage);

			Assert.Equal(modal1, testWindow.Navigation.ModalStack[0]);
			Assert.Equal(modal2, testWindow.Navigation.ModalStack[1]);
		}

		[Fact]
		public async Task ModalsWireUpInCorrectOrderWhenShellIsBuiltAfterModalPush()
		{
			var shell = new Shell();
			var mainPage = new ContentPage() { Title = "Main Page 1" };
			var modal1 = new ContentPage() { Title = "Modal 1" };
			var modal2 = new ContentPage() { Title = "Modal 2" };

			await mainPage.Navigation.PushModalAsync(modal1);
			await modal1.Navigation.PushModalAsync(modal2);

			shell.Items.Add(new ShellContent { Content = mainPage });
			TestWindow testWindow = new TestWindow(shell);

			Assert.Equal(modal1, testWindow.Navigation.ModalStack[0]);
			Assert.Equal(modal2, testWindow.Navigation.ModalStack[1]);
			Assert.Equal(2, testWindow.Navigation.ModalStack.Count);
		}

		[Fact]
		public void CurrentNullWhenEmpty()
		{
			var navModel = new NavigationModel();
			Assert.Null(navModel.CurrentPage);
		}

		[Fact]
		public void CurrentGivesLastViewWithoutModal()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			Assert.Equal(page2, navModel.CurrentPage);
		}

		[Fact]
		public void CurrentGivesLastViewWithModal()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			var modal1 = new ContentPage();
			var modal2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			navModel.PushModal(modal1);
			navModel.Push(modal2, modal1);

			Assert.Equal(modal2, navModel.CurrentPage);
		}

		[Fact]
		public void Roots()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			var modal1 = new ContentPage();
			var modal2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			navModel.PushModal(modal1);
			navModel.Push(modal2, modal1);

			Assert.True(navModel.Roots.SequenceEqual(new[] { page1, modal1 }));
		}

		[Fact]
		public void PushFirstItem()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			navModel.Push(page1, null);

			Assert.Equal(page1, navModel.CurrentPage);
			Assert.Equal(page1, navModel.Roots.First());
		}

		[Fact]
		public void ThrowsWhenPushingWithoutAncestor()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			Assert.Throws<InvalidNavigationException>(() => navModel.Push(page2, null));
		}

		[Fact]
		public void PushFromNonRootAncestor()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();
			var page3 = new ContentPage();

			page2.Parent = page1;
			page3.Parent = page2;

			navModel.Push(page1, null);
			navModel.Push(page2, page1);
			navModel.Push(page3, page2);

			Assert.Equal(page3, navModel.CurrentPage);
		}

		[Fact]
		public void ThrowsWhenPushFromInvalidAncestor()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			Assert.Throws<InvalidNavigationException>(() => navModel.Push(page2, page1));
		}

		[Fact]
		public void Pop()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			navModel.Pop(page1);

			Assert.Equal(page1, navModel.CurrentPage);
		}

		[Fact]
		public void ThrowsPoppingRootItem()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();

			navModel.Push(page1, null);

			Assert.Throws<InvalidNavigationException>(() => navModel.Pop(page1));
		}

		[Fact]
		public void ThrowsPoppingRootOfModal()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			var modal1 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			navModel.PushModal(modal1);
			Assert.Throws<InvalidNavigationException>(() => navModel.Pop(modal1));
		}

		[Fact]
		public void ThrowsPoppingWithInvalidAncestor()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();

			navModel.Push(page1, null);

			Assert.Throws<InvalidNavigationException>(() => navModel.Pop(new ContentPage()));
		}

		[Fact]
		public void PopToRoot()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();
			var page3 = new ContentPage();

			page2.Parent = page1;
			page3.Parent = page2;

			navModel.Push(page1, null);
			navModel.Push(page2, page1);
			navModel.Push(page3, page2);

			navModel.PopToRoot(page2);

			Assert.Equal(page1, navModel.CurrentPage);
		}

		[Fact]
		public void ThrowsWhenPopToRootOnRoot()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();

			navModel.Push(page1, null);
			Assert.Throws<InvalidNavigationException>(() => navModel.PopToRoot(page1));
		}

		[Fact]
		public void ThrowsWhenPopToRootWithInvalidAncestor()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			Assert.Throws<InvalidNavigationException>(() => navModel.PopToRoot(new ContentPage()));
		}

		[Fact]
		public void PopModal()
		{
			var navModel = new NavigationModel();

			var child1 = new ContentPage();
			var modal1 = new ContentPage();

			navModel.Push(child1, null);
			navModel.PushModal(modal1);

			navModel.PopModal();

			Assert.Equal(child1, navModel.CurrentPage);
			Assert.Single(navModel.Roots);
		}

		[Fact]
		public void ReturnsCorrectModal()
		{
			var navModel = new NavigationModel();

			var child1 = new ContentPage();
			var modal1 = new ContentPage();
			var modal2 = new ContentPage();

			navModel.Push(child1, null);
			navModel.PushModal(modal1);
			navModel.PushModal(modal2);

			Assert.Equal(modal2, navModel.PopModal());
		}

		[Fact]
		public void PopTopPageWithoutModals()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			Assert.Equal(page2, navModel.PopTopPage());
		}

		[Fact]
		public void PopTopPageWithSinglePage()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();

			navModel.Push(page1, null);

			Assert.Null(navModel.PopTopPage());
		}

		[Fact]
		public void PopTopPageWithModal()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var modal1 = new ContentPage();

			navModel.Push(page1, null);
			navModel.PushModal(modal1);

			Assert.Equal(modal1, navModel.PopTopPage());
		}
	}
}
