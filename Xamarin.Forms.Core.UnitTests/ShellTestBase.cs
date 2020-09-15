using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ShellTestBase : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();

		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Routing.Clear();

		}

		protected T FindParentOfType<T>(Element element)
		{
			var navPage = GetParentsPath(element)
				.OfType<T>()
				.FirstOrDefault();

			return navPage;
		}

		protected T GetVisiblePage<T>(Shell shell)
			where T : Page
		{
			if (shell?.CurrentItem?.CurrentItem is IShellSectionController scc)
				return (T)scc.PresentedPage;

			return default(T);
		}

		protected IEnumerable<Element> GetParentsPath(Element self)
		{
			Element current = self;

			while (!Application.IsApplicationOrNull(current.RealParent))
			{
				current = current.RealParent;
				yield return current;
			}
		}

		protected bool IsModal(BindableObject bindableObject)
		{
			return (Shell.GetPresentationMode(bindableObject) & PresentationMode.Modal) == PresentationMode.Modal;
		}

		protected bool IsAnimated(BindableObject bindableObject)
		{
			return (Shell.GetPresentationMode(bindableObject) & PresentationMode.NotAnimated) != PresentationMode.NotAnimated;
		}

		protected Uri CreateUri(string uri) => ShellUriHandler.CreateUri(uri);

		protected ShellSection MakeSimpleShellSection(string route, string contentRoute)
		{
			return MakeSimpleShellSection(route, contentRoute, new ShellTestPage());
		}

		protected ShellSection MakeSimpleShellSection(string route, string contentRoute, ContentPage contentPage)
		{
			var shellSection = new ShellSection();
			shellSection.Route = route;
			var shellContent = new ShellContent { Content = contentPage, Route = contentRoute };
			shellSection.Items.Add(shellContent);
			return shellSection;
		}

		[QueryProperty("SomeQueryParameter", "SomeQueryParameter")]
		[QueryProperty("CancelNavigationOnBackButtonPressed", "CancelNavigationOnBackButtonPressed")]
		public class ShellTestPage : ContentPage
		{
			public string CancelNavigationOnBackButtonPressed { get; set; }
			public ShellTestPage()
			{
			}

			public string SomeQueryParameter
			{
				get;
				set;
			}

			protected override void OnParentSet()
			{
				base.OnParentSet();
			}

			protected override bool OnBackButtonPressed()
			{
				if (CancelNavigationOnBackButtonPressed == "true")
					return true;

				if (CancelNavigationOnBackButtonPressed == "false")
					return false;

				return base.OnBackButtonPressed();
			}
		}

		protected ShellItem CreateShellItem(
			TemplatedPage page = null,
			bool asImplicit = false,
			string shellContentRoute = null,
			string shellSectionRoute = null,
			string shellItemRoute = null,
			bool templated = false)
		{
			return CreateShellItem<ShellItem>(
				page,
				asImplicit,
				shellContentRoute,
				shellSectionRoute,
				shellItemRoute,
				templated);
		}

		protected T CreateShellItem<T>(
			TemplatedPage page = null,
			bool asImplicit = false,
			string shellContentRoute = null,
			string shellSectionRoute = null,
			string shellItemRoute = null,
			bool templated = false) where T : ShellItem
		{
			T item = null;
			var section = CreateShellSection(page, asImplicit, shellContentRoute, shellSectionRoute, templated: templated);

			if (!String.IsNullOrWhiteSpace(shellItemRoute))
			{
				item = Activator.CreateInstance<T>();
				item.Route = shellItemRoute;
				item.Items.Add(section);
			}
			else if (asImplicit)
				item = (T)ShellItem.CreateFromShellSection(section);
			else
			{
				item = Activator.CreateInstance<T>();
				item.Items.Add(section);
			}

			return item;
		}

		protected ShellSection CreateShellSection(
			TemplatedPage page = null,
			bool asImplicit = false,
			string shellContentRoute = null,
			string shellSectionRoute = null,
			bool templated = false)
		{
			return CreateShellSection<ShellSection>(
				page, 
				asImplicit, 
				shellContentRoute,
				shellSectionRoute, 
				templated);
		}

		protected T CreateShellSection<T>(
			TemplatedPage page = null, 
			bool asImplicit = false, 
			string shellContentRoute = null, 
			string shellSectionRoute = null,
			bool templated = false) where T : ShellSection
		{
			var content = CreateShellContent(page, asImplicit, shellContentRoute, templated: templated);

			T section = null;

			if (!String.IsNullOrWhiteSpace(shellSectionRoute))
			{
				section = Activator.CreateInstance<T>();
				section.Route = shellSectionRoute;
				section.Items.Add(content);
			}
			else if (asImplicit)
				section = (T)ShellSection.CreateFromShellContent(content);
			else
			{
				section = Activator.CreateInstance<T>();
				section.Items.Add(content);
			}

			return section;
		}

		protected ShellContent CreateShellContent(TemplatedPage page = null, bool asImplicit = false, string shellContentRoute = null, bool templated = false)
		{
			ShellContent content = null;

			if (!String.IsNullOrWhiteSpace(shellContentRoute))
			{
				if (templated)
					content = new ShellContent() { ContentTemplate = new DataTemplate(() => page ?? new ContentPage()) };
				else
					content = new ShellContent() { Content = page ?? new ContentPage() };

				content.Route = shellContentRoute;
			}
			else if (asImplicit)
				content = (ShellContent)page;
			else
			{
				if (templated)
					content = new ShellContent() { ContentTemplate = new DataTemplate(() => page ?? new ContentPage()) };
				else
					content = new ShellContent() { Content = page ?? new ContentPage() };
			}


			return content;
		}

		protected ReadOnlyCollection<ShellContent> GetItems(ShellSection section)
		{
			return (section as IShellSectionController).GetItems();
		}

		protected ReadOnlyCollection<ShellSection> GetItems(ShellItem item)
		{
			return (item as IShellItemController).GetItems();
		}

		protected ReadOnlyCollection<ShellItem> GetItems(Shell item)
		{
			return (item as IShellController).GetItems();
		}

		public class TestShell : Shell
		{
			public int OnNavigatedCount;
			public int OnNavigatingCount;
			public int NavigatedCount;
			public int NavigatingCount;
			public int OnBackButtonPressedCount;

			public TestShell()
			{
				this.Navigated += (_, __) => NavigatedCount++;
				this.Navigating += (_, __) => NavigatingCount++;
			}

			public Action<ShellNavigatedEventArgs> OnNavigatedHandler { get; set; }
			protected override void OnNavigated(ShellNavigatedEventArgs args)
			{
				base.OnNavigated(args);
				OnNavigatedHandler?.Invoke(args);
				OnNavigatedCount++;
			}

			protected override void OnNavigating(ShellNavigatingEventArgs args)
			{
				base.OnNavigating(args);
				OnNavigatingCount++;
			}

			public Func<bool> OnBackButtonPressedFunc;
			protected override bool OnBackButtonPressed()
			{
				var result = OnBackButtonPressedFunc?.Invoke() ?? false;

				OnBackButtonPressedCount++;

				if(!result)
					result = base.OnBackButtonPressed();

				return result;
			}

			public void Reset()
			{
				OnNavigatedCount = 
					OnNavigatingCount = 
					NavigatedCount = 
					NavigatingCount =
					OnBackButtonPressedCount = 0;
			}

			public void TestCount(int count, string message = null)
			{
				Assert.AreEqual(count, OnNavigatedCount, $"OnNavigatedCount: {message}");
				Assert.AreEqual(count, NavigatingCount, $"NavigatingCount: {message}");
				Assert.AreEqual(count, OnNavigatingCount, $"OnNavigatingCount: {message}");
				Assert.AreEqual(count, NavigatedCount, $"NavigatedCount: {message}");
			}
		}


		public class TestShellViewModel : INotifyPropertyChanged
		{
			private string _text;

			public event PropertyChangedEventHandler PropertyChanged;

			public TestShellViewModel SubViewModel { get; set; }

			public TestShellViewModel SubViewModel2 { get; set; }

			public string Text
			{
				get => _text;
				set
				{
					_text = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
				}
			}
		}
	}
}
