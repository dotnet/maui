using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellUriHandlerTests : ShellTestBase
	{

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Routing.Clear();
			}

			base.Dispose(disposing);
		}

		[Fact]
		public void NodeWalkingBasic()
		{
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals2"),
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals")
			);

			ShellUriHandler.NodeLocation nodeLocation = new ShellUriHandler.NodeLocation();
			nodeLocation.SetNode(shell);

			nodeLocation = nodeLocation.WalkToNextNode();
			Assert.Equal(nodeLocation.Content, shell.Items[0].Items[0].Items[0]);

			nodeLocation = nodeLocation.WalkToNextNode();
			Assert.Equal(nodeLocation.Content, shell.Items[1].Items[0].Items[0]);
		}


		[Fact]
		public void NodeWalkingMultipleContent()
		{
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals1"),
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals2"),
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals3"),
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals4")
			);

			var content = CreateShellContent();
			shell.Items[1].Items[0].Items.Add(content);
			shell.Items[2].Items[0].Items.Add(CreateShellContent());

			// add a section with now content
			shell.Items[0].Items.Add(new ShellSection());

			ShellUriHandler.NodeLocation nodeLocation = new ShellUriHandler.NodeLocation();
			nodeLocation.SetNode(content);

			nodeLocation = nodeLocation.WalkToNextNode();
			Assert.Equal(shell.Items[2].Items[0].Items[0], nodeLocation.Content);

			nodeLocation = nodeLocation.WalkToNextNode();
			Assert.Equal(shell.Items[2].Items[0].Items[1], nodeLocation.Content);

			nodeLocation = nodeLocation.WalkToNextNode();
			Assert.Equal(shell.Items[3].Items[0].Items[0], nodeLocation.Content);
		}

		[Fact]
		public async Task GlobalRegisterAbsoluteMatching()
		{
			var shell = new Shell();
			Routing.RegisterRoute("/seg1/seg2/seg3", typeof(object));
			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("/seg1/seg2/seg3"));

			Assert.Equal("app://shell/IMPL_shell/seg1/seg2/seg3", request.Request.FullUri.ToString());
		}


		[Fact]
		public async Task ShellRelativeGlobalRegistration()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellItemRoute: "item1", shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");
			var item2 = CreateShellItem(asImplicit: true, shellItemRoute: "item2", shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");

			Routing.RegisterRoute("section0/edit", typeof(ContentPage));
			Routing.RegisterRoute("item1/section1/edit", typeof(ContentPage));
			Routing.RegisterRoute("item2/section1/edit", typeof(ContentPage));
			Routing.RegisterRoute("//edit", typeof(ContentPage));
			shell.Items.Add(item1);
			shell.Items.Add(item2);
			await shell.GoToAsync("//item1/section1/rootlevelcontent1");
			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("section1/edit"), true);

			Assert.Single(request.Request.GlobalRoutes);
			Assert.Equal("item1/section1/edit", request.Request.GlobalRoutes.First());
		}

		[Fact]
		public async Task ShellSectionWithRelativeEditUpOneLevelMultiple()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");

			Routing.RegisterRoute("section1/edit", typeof(ContentPage));
			Routing.RegisterRoute("section1/add", typeof(ContentPage));

			shell.Items.Add(item1);

			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("//rootlevelcontent1/add/edit"));

			Assert.Equal(2, request.Request.GlobalRoutes.Count);
			Assert.Equal("section1/add", request.Request.GlobalRoutes.First());
			Assert.Equal("section1/edit", request.Request.GlobalRoutes.Skip(1).First());
		}


		[Fact]
		public async Task ShellSectionWithGlobalRouteRelative()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");

			Routing.RegisterRoute("edit", typeof(ContentPage));

			shell.Items.Add(item1);

			await shell.GoToAsync("//rootlevelcontent1");
			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("edit"));

			Assert.Single(request.Request.GlobalRoutes);
			Assert.Equal("edit", request.Request.GlobalRoutes.First());
		}

		[Fact]
		public async Task ShellSectionWithRelativeEditUpOneLevel()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");

			Routing.RegisterRoute("section1/edit", typeof(ContentPage));

			shell.Items.Add(item1);

			await shell.GoToAsync("//rootlevelcontent1");
			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("edit"), true);

			Assert.Equal("section1/edit", request.Request.GlobalRoutes.First());
		}



		[Fact]
		public async Task ShellContentOnly()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");
			var item2 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent2");

			shell.Items.Add(item1);
			shell.Items.Add(item2);


			var builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//rootlevelcontent1"));

			Assert.Single(builders);
			Assert.Equal("//rootlevelcontent1", builders.First().PathNoImplicit);

			builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//rootlevelcontent2"));
			Assert.Single(builders);
			Assert.Equal("//rootlevelcontent2", builders.First().PathNoImplicit);
		}


		[Fact]
		public async Task ShellSectionAndContentOnly()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent", shellSectionRoute: "section1");
			var item2 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent", shellSectionRoute: "section2");

			shell.Items.Add(item1);
			shell.Items.Add(item2);


			var builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//section1/rootlevelcontent")).Select(x => x.PathNoImplicit).ToArray();

			Assert.Single(builders);
			Assert.Contains("//section1/rootlevelcontent", builders);

			builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//section2/rootlevelcontent")).Select(x => x.PathNoImplicit).ToArray();
			Assert.Single(builders);
			Assert.Contains("//section2/rootlevelcontent", builders);
		}

		[Fact]
		public async Task ShellItemAndContentOnly()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent", shellItemRoute: "item1");
			var item2 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent", shellItemRoute: "item2");

			shell.Items.Add(item1);
			shell.Items.Add(item2);


			var builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//item1/rootlevelcontent")).Select(x => x.PathNoImplicit).ToArray();

			Assert.Single(builders);
			Assert.Contains("//item1/rootlevelcontent", builders);

			builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//item2/rootlevelcontent")).Select(x => x.PathNoImplicit).ToArray();
			Assert.Single(builders);
			Assert.Contains("//item2/rootlevelcontent", builders);
		}


		[Fact]
		public async Task AbsoluteNavigationToRelativeWithGlobal()
		{
			var shell = new Shell();

			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "dogs");
			var item2 = CreateShellItem(asImplicit: true, shellSectionRoute: "domestic", shellContentRoute: "cats", shellItemRoute: "animals");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Routing.RegisterRoute("catdetails", typeof(ContentPage));
			await shell.GoToAsync($"//animals/domestic/cats/catdetails?name=domestic");

			Assert.Equal(
				"//animals/domestic/cats/catdetails",
				shell.CurrentState.FullLocation.ToString()
				);
		}

		[Fact]
		public async Task RelativeNavigationToShellElementThrows()
		{
			var shell = new Shell();

			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "dogs");
			var item2 = CreateShellItem(asImplicit: true, shellSectionRoute: "domestic", shellContentRoute: "cats", shellItemRoute: "animals");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			await Assert.ThrowsAnyAsync<Exception>(async () => await shell.GoToAsync($"domestic"));
		}


		[Fact]
		public async Task RelativeNavigationWithRoute()
		{
			var shell = new Shell();

			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "dogs");
			var item2 = CreateShellItem(asImplicit: true, shellSectionRoute: "domestic", shellContentRoute: "cats", shellItemRoute: "animals");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Routing.RegisterRoute("catdetails", typeof(ContentPage));
			await Assert.ThrowsAnyAsync<Exception>(async () => await shell.GoToAsync($"cats/catdetails?name=domestic"));

			// once relative routing with a stack is fixed then we can remove the above exception check and add below back in
			// await shell.GoToAsync($"cats/catdetails?name=domestic")
			//Assert.Equal(
			//	"//animals/domestic/cats/catdetails",
			//	shell.CurrentState.Location.ToString()
			//	);

		}

		[Fact]
		public async Task ConvertToStandardFormat()
		{
			var shell = new Shell();

			Uri[] TestUris = new Uri[] {
				CreateUri("path"),
				CreateUri("//path"),
				CreateUri("/path"),
				CreateUri("shell/path"),
				CreateUri("//shell/path"),
				CreateUri("/shell/path"),
				CreateUri("IMPL_shell/path"),
				CreateUri("//IMPL_shell/path"),
				CreateUri("/IMPL_shell/path"),
				CreateUri("shell/IMPL_shell/path"),
				CreateUri("//shell/IMPL_shell/path"),
				CreateUri("/shell/IMPL_shell/path"),
				CreateUri("app://path"),
				CreateUri("app:/path"),
				CreateUri("app://shell/path"),
				CreateUri("app:/shell/path"),
				CreateUri("app://shell/IMPL_shell/path"),
				CreateUri("app:/shell/IMPL_shell/path"),
				CreateUri("app:/shell/IMPL_shell\\path")
			};


			foreach (var uri in TestUris)
			{
				Assert.Equal(new Uri("app://shell/IMPL_shell/path"), ShellUriHandler.ConvertToStandardFormat(shell, uri));

				if (!uri.IsAbsoluteUri)
				{
					var reverse = new Uri(uri.OriginalString.Replace('/', '\\'), UriKind.Relative);
					Assert.Equal(new Uri("app://shell/IMPL_shell/path"), ShellUriHandler.ConvertToStandardFormat(shell, reverse));
				}

			}
		}
	}
}
