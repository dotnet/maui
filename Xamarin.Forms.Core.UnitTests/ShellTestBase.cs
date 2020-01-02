using System;
using System.Collections.ObjectModel;
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
		public class ShellTestPage : ContentPage
		{
			public string SomeQueryParameter
			{
				get;
				set;
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
			ShellItem item = null;
			var section = CreateShellSection(page, asImplicit, shellContentRoute, shellSectionRoute, templated: templated);

			if (!String.IsNullOrWhiteSpace(shellItemRoute))
			{
				item = new ShellItem();
				item.Route = shellItemRoute;
				item.Items.Add(section);
			}
			else if (asImplicit)
				item = ShellItem.CreateFromShellSection(section);
			else
			{
				item = new ShellItem();
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
			var content = CreateShellContent(page, asImplicit, shellContentRoute, templated: templated);

			ShellSection section = null;

			if (!String.IsNullOrWhiteSpace(shellSectionRoute))
			{
				section = new ShellSection();
				section.Route = shellSectionRoute;
				section.Items.Add(content);
			}
			else if (asImplicit)
				section = ShellSection.CreateFromShellContent(content);
			else
			{
				section = new ShellSection();
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

	}
}
