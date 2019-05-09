using System;
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
			public string SomeQueryParameter { get; set; }
		}

		protected ShellItem CreateShellItem(TemplatedPage page = null, bool asImplicit = false, string shellContentRoute = null, string shellSectionRoute = null, string shellItemRoute = null)
		{
			page = page ?? new ContentPage();
			ShellItem item = null;
			var section = CreateShellSection(page, asImplicit, shellContentRoute, shellSectionRoute);

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

		protected ShellSection CreateShellSection(TemplatedPage page = null, bool asImplicit = false, string shellContentRoute = null, string shellSectionRoute = null)
		{
			var content = CreateShellContent(page, asImplicit, shellContentRoute);

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

		protected ShellContent CreateShellContent(TemplatedPage page = null, bool asImplicit = false, string shellContentRoute = null)
		{
			page = page ?? new ContentPage();
			ShellContent content = null;

			if(!String.IsNullOrWhiteSpace(shellContentRoute))
			{
				content = new ShellContent() { Content = page };
				content.Route = shellContentRoute;
			}
			else if (asImplicit)
				content = (ShellContent)page;
			else
				content = new ShellContent() { Content = page };


			return content;
		}

	}
}
