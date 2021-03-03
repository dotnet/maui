using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ShellUriHandlerTests : ShellTestBase
	{
		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Routing.Clear();
		}

		[Test]
		public void NodeWalkingBasic()
		{
			var shell = new TestShell(
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals2"),
				CreateShellItem(shellContentRoute: "monkeys", shellItemRoute: "animals")
			);

			ShellUriHandler.NodeLocation nodeLocation = new ShellUriHandler.NodeLocation();
			nodeLocation.SetNode(shell);

			nodeLocation = nodeLocation.WalkToNextNode();
			Assert.AreEqual(nodeLocation.Content, shell.Items[0].Items[0].Items[0]);

			nodeLocation = nodeLocation.WalkToNextNode();
			Assert.AreEqual(nodeLocation.Content, shell.Items[1].Items[0].Items[0]);
		}


		[Test]
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
			Assert.AreEqual(shell.Items[2].Items[0].Items[0], nodeLocation.Content);

			nodeLocation = nodeLocation.WalkToNextNode();
			Assert.AreEqual(shell.Items[2].Items[0].Items[1], nodeLocation.Content);

			nodeLocation = nodeLocation.WalkToNextNode();
			Assert.AreEqual(shell.Items[3].Items[0].Items[0], nodeLocation.Content);
		}

		[Test]
		public async Task GlobalRegisterAbsoluteMatching()
		{
			var shell = new Shell();
			Routing.RegisterRoute("/seg1/seg2/seg3", typeof(object));
			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("/seg1/seg2/seg3"));

			Assert.AreEqual("app://shell/IMPL_shell/seg1/seg2/seg3", request.Request.FullUri.ToString());
		}


		[Test]
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

			Assert.AreEqual(1, request.Request.GlobalRoutes.Count);
			Assert.AreEqual("item1/section1/edit", request.Request.GlobalRoutes.First());
		}

		[Test]
		public async Task ShellSectionWithRelativeEditUpOneLevelMultiple()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");

			Routing.RegisterRoute("section1/edit", typeof(ContentPage));
			Routing.RegisterRoute("section1/add", typeof(ContentPage));

			shell.Items.Add(item1);

			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("//rootlevelcontent1/add/edit"));

			Assert.AreEqual(2, request.Request.GlobalRoutes.Count);
			Assert.AreEqual("section1/add", request.Request.GlobalRoutes.First());
			Assert.AreEqual("section1/edit", request.Request.GlobalRoutes.Skip(1).First());
		}


		[Test]
		public async Task ShellSectionWithGlobalRouteRelative()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");

			Routing.RegisterRoute("edit", typeof(ContentPage));

			shell.Items.Add(item1);

			await shell.GoToAsync("//rootlevelcontent1");
			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("edit"));

			Assert.AreEqual(1, request.Request.GlobalRoutes.Count);
			Assert.AreEqual("edit", request.Request.GlobalRoutes.First());
		}

		[Test]
		public async Task ShellSectionWithRelativeEditUpOneLevel()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1", shellSectionRoute: "section1");

			Routing.RegisterRoute("section1/edit", typeof(ContentPage));

			shell.Items.Add(item1);

			await shell.GoToAsync("//rootlevelcontent1");
			var request = ShellUriHandler.GetNavigationRequest(shell, CreateUri("edit"), true);

			Assert.AreEqual("section1/edit", request.Request.GlobalRoutes.First());
		}



		[Test]
		public async Task ShellContentOnly()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent1");
			var item2 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent2");

			shell.Items.Add(item1);
			shell.Items.Add(item2);


			var builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//rootlevelcontent1"));

			Assert.AreEqual(1, builders.Count);
			Assert.AreEqual("//rootlevelcontent1", builders.First().PathNoImplicit);

			builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//rootlevelcontent2"));
			Assert.AreEqual(1, builders.Count);
			Assert.AreEqual("//rootlevelcontent2", builders.First().PathNoImplicit);
		}


		[Test]
		public async Task ShellSectionAndContentOnly()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent", shellSectionRoute: "section1");
			var item2 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent", shellSectionRoute: "section2");

			shell.Items.Add(item1);
			shell.Items.Add(item2);


			var builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//section1/rootlevelcontent")).Select(x => x.PathNoImplicit).ToArray();

			Assert.AreEqual(1, builders.Length);
			Assert.IsTrue(builders.Contains("//section1/rootlevelcontent"));

			builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//section2/rootlevelcontent")).Select(x => x.PathNoImplicit).ToArray();
			Assert.AreEqual(1, builders.Length);
			Assert.IsTrue(builders.Contains("//section2/rootlevelcontent"));
		}

		[Test]
		public async Task ShellItemAndContentOnly()
		{
			var shell = new Shell();
			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent", shellItemRoute: "item1");
			var item2 = CreateShellItem(asImplicit: true, shellContentRoute: "rootlevelcontent", shellItemRoute: "item2");

			shell.Items.Add(item1);
			shell.Items.Add(item2);


			var builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//item1/rootlevelcontent")).Select(x => x.PathNoImplicit).ToArray();

			Assert.AreEqual(1, builders.Length);
			Assert.IsTrue(builders.Contains("//item1/rootlevelcontent"));

			builders = ShellUriHandler.GenerateRoutePaths(shell, CreateUri("//item2/rootlevelcontent")).Select(x => x.PathNoImplicit).ToArray();
			Assert.AreEqual(1, builders.Length);
			Assert.IsTrue(builders.Contains("//item2/rootlevelcontent"));
		}


		[Test]
		public async Task AbsoluteNavigationToRelativeWithGlobal()
		{
			var shell = new Shell();

			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "dogs");
			var item2 = CreateShellItem(asImplicit: true, shellSectionRoute: "domestic", shellContentRoute: "cats", shellItemRoute: "animals");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Routing.RegisterRoute("catdetails", typeof(ContentPage));
			await shell.GoToAsync($"//animals/domestic/cats/catdetails?name=domestic");

			Assert.AreEqual(
				"//animals/domestic/cats/catdetails",
				shell.CurrentState.FullLocation.ToString()
				);
		}

		[Test]
		public async Task RelativeNavigationToShellElementThrows()
		{
			var shell = new Shell();

			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "dogs");
			var item2 = CreateShellItem(asImplicit: true, shellSectionRoute: "domestic", shellContentRoute: "cats", shellItemRoute: "animals");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Assert.That(async () => await shell.GoToAsync($"domestic"), Throws.Exception);
		}


		[Test]
		public async Task RelativeNavigationWithRoute()
		{
			var shell = new Shell();

			var item1 = CreateShellItem(asImplicit: true, shellContentRoute: "dogs");
			var item2 = CreateShellItem(asImplicit: true, shellSectionRoute: "domestic", shellContentRoute: "cats", shellItemRoute: "animals");

			shell.Items.Add(item1);
			shell.Items.Add(item2);

			Routing.RegisterRoute("catdetails", typeof(ContentPage));
			Assert.That(async () => await shell.GoToAsync($"cats/catdetails?name=domestic"), Throws.Exception);

			// once relative routing with a stack is fixed then we can remove the above exception check and add below back in
			// await shell.GoToAsync($"cats/catdetails?name=domestic")
			//Assert.AreEqual(
			//	"//animals/domestic/cats/catdetails",
			//	shell.CurrentState.Location.ToString()
			//	);

		}

		[Test]
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
				Assert.AreEqual(new Uri("app://shell/IMPL_shell/path"), ShellUriHandler.ConvertToStandardFormat(shell, uri), $"{uri}");

				if (!uri.IsAbsoluteUri)
				{
					var reverse = new Uri(uri.OriginalString.Replace("/", "\\"), UriKind.Relative);
					Assert.AreEqual(new Uri("app://shell/IMPL_shell/path"), ShellUriHandler.ConvertToStandardFormat(shell, reverse));
				}

			}
		}
	}
}
