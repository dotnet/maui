using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class NavigationModelTests : BaseTestFixture
	{
		[Test]
		public void CurrentNullWhenEmpty()
		{
			var navModel = new NavigationModel();
			Assert.Null(navModel.CurrentPage);
		}

		[Test]
		public void CurrentGivesLastViewWithoutModal()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			Assert.AreEqual(page2, navModel.CurrentPage);
		}

		[Test]
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

			Assert.AreEqual(modal2, navModel.CurrentPage);
		}

		[Test]
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

		[Test]
		public void PushFirstItem()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			navModel.Push(page1, null);

			Assert.AreEqual(page1, navModel.CurrentPage);
			Assert.AreEqual(page1, navModel.Roots.First());
		}

		[Test]
		public void ThrowsWhenPushingWithoutAncestor()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			Assert.Throws<InvalidNavigationException>(() => navModel.Push(page2, null));
		}

		[Test]
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

			Assert.AreEqual(page3, navModel.CurrentPage);
		}

		[Test]
		public void ThrowsWhenPushFromInvalidAncestor()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			Assert.Throws<InvalidNavigationException>(() => navModel.Push(page2, page1));
		}

		[Test]
		public void Pop()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			navModel.Pop(page1);

			Assert.AreEqual(page1, navModel.CurrentPage);
		}

		[Test]
		public void ThrowsPoppingRootItem()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();

			navModel.Push(page1, null);

			Assert.Throws<InvalidNavigationException>(() => navModel.Pop(page1));
		}

		[Test]
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

		[Test]
		public void ThrowsPoppingWithInvalidAncestor()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();

			navModel.Push(page1, null);

			Assert.Throws<InvalidNavigationException>(() => navModel.Pop(new ContentPage()));
		}

		[Test]
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

			Assert.AreEqual(page1, navModel.CurrentPage);
		}

		[Test]
		public void ThrowsWhenPopToRootOnRoot()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();

			navModel.Push(page1, null);
			Assert.Throws<InvalidNavigationException>(() => navModel.PopToRoot(page1));
		}

		[Test]
		public void ThrowsWhenPopToRootWithInvalidAncestor()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			Assert.Throws<InvalidNavigationException>(() => navModel.PopToRoot(new ContentPage()));
		}

		[Test]
		public void PopModal()
		{
			var navModel = new NavigationModel();

			var child1 = new ContentPage();
			var modal1 = new ContentPage();

			navModel.Push(child1, null);
			navModel.PushModal(modal1);

			navModel.PopModal();

			Assert.AreEqual(child1, navModel.CurrentPage);
			Assert.AreEqual(1, navModel.Roots.Count());
		}

		[Test]
		public void ReturnsCorrectModal()
		{
			var navModel = new NavigationModel();

			var child1 = new ContentPage();
			var modal1 = new ContentPage();
			var modal2 = new ContentPage();

			navModel.Push(child1, null);
			navModel.PushModal(modal1);
			navModel.PushModal(modal2);

			Assert.AreEqual(modal2, navModel.PopModal());
		}

		[Test]
		public void PopTopPageWithoutModals()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var page2 = new ContentPage();

			navModel.Push(page1, null);
			navModel.Push(page2, page1);

			Assert.AreEqual(page2, navModel.PopTopPage());
		}

		[Test]
		public void PopTopPageWithSinglePage()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();

			navModel.Push(page1, null);

			Assert.Null(navModel.PopTopPage());
		}

		[Test]
		public void PopTopPageWithModal()
		{
			var navModel = new NavigationModel();

			var page1 = new ContentPage();
			var modal1 = new ContentPage();

			navModel.Push(page1, null);
			navModel.PushModal(modal1);

			Assert.AreEqual(modal1, navModel.PopTopPage());
		}
	}
}