using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class NavigationProxyTests : BaseTestFixture
	{
		class NavigationTest : INavigation
		{
			public Page LastPushed { get; set; }
			public Page LastPushedModal { get; set; }

			public bool Popped { get; set; }
			public bool PoppedModal { get; set; }

			public Task PushAsync(Page root)
			{
				return PushAsync(root, true);
			}

			public Task<Page> PopAsync()
			{
				return PopAsync(true);
			}

			public Task PopToRootAsync()
			{
				return PopToRootAsync(true);
			}

			public Task PushModalAsync(Page root)
			{
				return PushModalAsync(root, true);
			}

			public Task<Page> PopModalAsync()
			{
				return PopModalAsync(true);
			}

			public Task PushAsync(Page root, bool animated)
			{
				LastPushed = root;
				return Task.FromResult(root);
			}

			public Task<Page> PopAsync(bool animated)
			{
				Popped = true;
				return Task.FromResult(LastPushed);
			}

			public Task PopToRootAsync(bool animated)
			{
				return Task.FromResult<Page>(null);
			}

			public Task PushModalAsync(Page root, bool animated)
			{
				LastPushedModal = root;
				return Task.FromResult<object>(null);
			}

			public Task<Page> PopModalAsync(bool animated)
			{
				PoppedModal = true;
				return Task.FromResult<Page>(null);
			}

			public void RemovePage(Page page)
			{
			}

			public void InsertPageBefore(Page page, Page before)
			{
			}

			public System.Collections.Generic.IReadOnlyList<Page> NavigationStack
			{
				get { return new List<Page>(); }
			}

			public System.Collections.Generic.IReadOnlyList<Page> ModalStack
			{
				get { return new List<Page>(); }
			}
		}

		[Fact]
		public void Constructor()
		{
			var proxy = new NavigationProxy();

			Assert.Null(proxy.Inner);
		}

		[Fact]
		public async Task PushesIntoNextInner()
		{
			var page = new ContentPage();
			var navProxy = new NavigationProxy();

			await navProxy.PushAsync(page);

			var navTest = new NavigationTest();
			navProxy.Inner = navTest;

			Assert.Equal(page, navTest.LastPushed);
		}

		[Fact]
		public async Task PushesModalIntoNextInner()
		{
			var page = new ContentPage();
			var navProxy = new NavigationProxy();

			await navProxy.PushModalAsync(page);

			var navTest = new NavigationTest();
			navProxy.Inner = navTest;

			Assert.Equal(page, navTest.LastPushedModal);
		}

		[Fact]
		public async Task TestPushWithInner()
		{
			var proxy = new NavigationProxy();
			var inner = new NavigationTest();

			proxy.Inner = inner;

			var child = new ContentPage { Content = new View() };
			await proxy.PushAsync(child);

			Assert.Equal(child, inner.LastPushed);
		}

		[Fact]
		public async Task TestPushModalWithInner()
		{
			var proxy = new NavigationProxy();
			var inner = new NavigationTest();

			proxy.Inner = inner;

			var child = new ContentPage { Content = new View() };
			await proxy.PushModalAsync(child);

			Assert.Equal(child, inner.LastPushedModal);
		}

		[Fact]
		public async Task TestPopWithInner()
		{
			var proxy = new NavigationProxy();
			var inner = new NavigationTest();

			proxy.Inner = inner;

			var child = new ContentPage { Content = new View() };
			await proxy.PushAsync(child);

			var result = await proxy.PopAsync();
			Assert.Equal(child, result);
			Assert.True(inner.Popped, "Pop was never called on the inner proxy item");
		}

		[Fact]
		public async Task TestPopModalWithInner()
		{
			var proxy = new NavigationProxy();
			var inner = new NavigationTest();

			proxy.Inner = inner;

			var child = new ContentPage { Content = new View() };
			await proxy.PushModalAsync(child);

			await proxy.PopModalAsync();
			Assert.True(inner.PoppedModal, "Pop was never called on the inner proxy item");
		}
	}
}
