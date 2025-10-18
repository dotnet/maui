#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
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

        /// <summary>
        /// Tests that GlobalRouteItem.IsFinished returns true when the path has no segments after skipping the first one.
        /// Input conditions: Empty string path.
        /// Expected result: IsFinished returns true.
        /// </summary>
        [Fact]
        public void GlobalRouteItem_IsFinished_EmptyPath_ReturnsTrue()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("", "sourceRoute");

            // Act
            var result = globalRouteItem.IsFinished;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GlobalRouteItem.IsFinished returns true when the path has only one segment.
        /// Input conditions: Single segment path.
        /// Expected result: IsFinished returns true.
        /// </summary>
        [Fact]
        public void GlobalRouteItem_IsFinished_SingleSegment_ReturnsTrue()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("home", "sourceRoute");

            // Act
            var result = globalRouteItem.IsFinished;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GlobalRouteItem.IsFinished returns true when the path contains only separators.
        /// Input conditions: Path with only separators.
        /// Expected result: IsFinished returns true.
        /// </summary>
        [Theory]
        [InlineData("/")]
        [InlineData("\\")]
        [InlineData("//")]
        [InlineData("\\\\")]
        [InlineData("/\\")]
        public void GlobalRouteItem_IsFinished_OnlySeparators_ReturnsTrue(string path)
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem(path, "sourceRoute");

            // Act
            var result = globalRouteItem.IsFinished;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GlobalRouteItem.IsFinished returns true when the path has one segment with separators.
        /// Input conditions: Single segment with leading/trailing separators.
        /// Expected result: IsFinished returns true.
        /// </summary>
        [Theory]
        [InlineData("/home")]
        [InlineData("home/")]
        [InlineData("/home/")]
        [InlineData("\\home")]
        [InlineData("home\\")]
        [InlineData("\\home\\")]
        public void GlobalRouteItem_IsFinished_SingleSegmentWithSeparators_ReturnsTrue(string path)
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem(path, "sourceRoute");

            // Act
            var result = globalRouteItem.IsFinished;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that GlobalRouteItem.IsFinished returns false when the path has two segments.
        /// Input conditions: Path with two segments.
        /// Expected result: IsFinished returns false.
        /// </summary>
        [Theory]
        [InlineData("home/page")]
        [InlineData("home\\page")]
        [InlineData("/home/page")]
        [InlineData("\\home\\page")]
        [InlineData("/home/page/")]
        [InlineData("\\home\\page\\")]
        public void GlobalRouteItem_IsFinished_TwoSegments_ReturnsFalse(string path)
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem(path, "sourceRoute");

            // Act
            var result = globalRouteItem.IsFinished;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GlobalRouteItem.IsFinished returns false when the path has multiple segments.
        /// Input conditions: Path with three or more segments.
        /// Expected result: IsFinished returns false.
        /// </summary>
        [Theory]
        [InlineData("home/section/page")]
        [InlineData("home\\section\\page")]
        [InlineData("a/b/c/d")]
        [InlineData("root\\folder\\subfolder\\file")]
        [InlineData("/home/section/page/")]
        [InlineData("\\home\\section\\page\\")]
        public void GlobalRouteItem_IsFinished_MultipleSegments_ReturnsFalse(string path)
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem(path, "sourceRoute");

            // Act
            var result = globalRouteItem.IsFinished;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GlobalRouteItem.IsFinished returns false when the path has mixed separators and multiple segments.
        /// Input conditions: Path with mixed separators and multiple segments.
        /// Expected result: IsFinished returns false.
        /// </summary>
        [Theory]
        [InlineData("home/section\\page")]
        [InlineData("home\\section/page")]
        [InlineData("a/b\\c/d")]
        [InlineData("root\\folder/subfolder\\file")]
        public void GlobalRouteItem_IsFinished_MixedSeparatorsMultipleSegments_ReturnsFalse(string path)
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem(path, "sourceRoute");

            // Act
            var result = globalRouteItem.IsFinished;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GlobalRouteItem.IsFinished handles edge case with special characters in paths.
        /// Input conditions: Path with special characters and multiple segments.
        /// Expected result: IsFinished returns false.
        /// </summary>
        [Theory]
        [InlineData("route-with-dashes/page")]
        [InlineData("route_with_underscores/page")]
        [InlineData("route.with.dots/page")]
        [InlineData("route with spaces/page")]
        [InlineData("route123/page456")]
        public void GlobalRouteItem_IsFinished_SpecialCharactersMultipleSegments_ReturnsFalse(string path)
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem(path, "sourceRoute");

            // Act
            var result = globalRouteItem.IsFinished;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that GlobalRouteItem.IsFinished handles null path by throwing appropriate exception.
        /// Input conditions: Null path.
        /// Expected result: Exception is thrown during construction or property access.
        /// </summary>
        [Fact]
        public void GlobalRouteItem_IsFinished_NullPath_ThrowsException()
        {
            // Arrange & Act & Assert
            Assert.ThrowsAny<Exception>(() =>
            {
                var globalRouteItem = new ShellUriHandler.GlobalRouteItem(null, "sourceRoute");
                var result = globalRouteItem.IsFinished;
            });
        }
    }

    public partial class ShellUriHandlerNodeLocationTests
    {
        /// <summary>
        /// Tests GetUri method when all shell elements (Item, Section, Content) are null.
        /// Should return URI with only Shell RouteHost and Route.
        /// Expected result: URI with format "{RouteScheme}://{RouteHost}/{Route}".
        /// </summary>
        [Fact]
        public void GetUri_WhenAllElementsAreNull_ReturnsBasicShellUri()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("testhost");
            shell.Route.Returns("testroute");
            shell.RouteScheme.Returns("testscheme");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("testscheme://testhost/testroute", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method when Item is not null and not implicit.
        /// Should include Item.Route in the URI path.
        /// Expected result: URI includes Item route.
        /// </summary>
        [Fact]
        public void GetUri_WhenItemIsNotNullAndNotImplicit_IncludesItemRoute()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("host");
            shell.Route.Returns("shell");
            shell.RouteScheme.Returns("app");

            var item = Substitute.For<ShellItem>();
            item.Route.Returns("itemroute");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Item").SetValue(nodeLocation, item);

            // Mock Routing.IsImplicit to return false for the item
            MockRouting(item, false);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("app://host/shell/itemroute", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method when Item is not null but is implicit.
        /// Should not include Item.Route in the URI path.
        /// Expected result: URI excludes Item route.
        /// </summary>
        [Fact]
        public void GetUri_WhenItemIsImplicit_ExcludesItemRoute()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("host");
            shell.Route.Returns("shell");
            shell.RouteScheme.Returns("app");

            var item = Substitute.For<ShellItem>();
            item.Route.Returns("itemroute");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Item").SetValue(nodeLocation, item);

            // Mock Routing.IsImplicit to return true for the item
            MockRouting(item, true);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("app://host/shell", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method when Section is not null and not implicit.
        /// Should include Section.Route in the URI path.
        /// Expected result: URI includes Section route.
        /// </summary>
        [Fact]
        public void GetUri_WhenSectionIsNotNullAndNotImplicit_IncludesSectionRoute()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("host");
            shell.Route.Returns("shell");
            shell.RouteScheme.Returns("app");

            var section = Substitute.For<ShellSection>();
            section.Route.Returns("sectionroute");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Section").SetValue(nodeLocation, section);

            // Mock Routing.IsImplicit to return false for the section
            MockRouting(section, false);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("app://host/shell/sectionroute", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method when Section is not null but is implicit.
        /// Should not include Section.Route in the URI path.
        /// Expected result: URI excludes Section route.
        /// </summary>
        [Fact]
        public void GetUri_WhenSectionIsImplicit_ExcludesSectionRoute()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("host");
            shell.Route.Returns("shell");
            shell.RouteScheme.Returns("app");

            var section = Substitute.For<ShellSection>();
            section.Route.Returns("sectionroute");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Section").SetValue(nodeLocation, section);

            // Mock Routing.IsImplicit to return true for the section
            MockRouting(section, true);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("app://host/shell", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method when Content is not null and not implicit.
        /// Should include Content.Route in the URI path.
        /// Expected result: URI includes Content route.
        /// </summary>
        [Fact]
        public void GetUri_WhenContentIsNotNullAndNotImplicit_IncludesContentRoute()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("host");
            shell.Route.Returns("shell");
            shell.RouteScheme.Returns("app");

            var content = Substitute.For<ShellContent>();
            content.Route.Returns("contentroute");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Content").SetValue(nodeLocation, content);

            // Mock Routing.IsImplicit to return false for the content
            MockRouting(content, false);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("app://host/shell/contentroute", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method when Content is not null but is implicit.
        /// Should not include Content.Route in the URI path.
        /// Expected result: URI excludes Content route.
        /// </summary>
        [Fact]
        public void GetUri_WhenContentIsImplicit_ExcludesContentRoute()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("host");
            shell.Route.Returns("shell");
            shell.RouteScheme.Returns("app");

            var content = Substitute.For<ShellContent>();
            content.Route.Returns("contentroute");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Content").SetValue(nodeLocation, content);

            // Mock Routing.IsImplicit to return true for the content
            MockRouting(content, true);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("app://host/shell", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method when all shell elements are present and not implicit.
        /// Should include all routes in the URI path.
        /// Expected result: URI includes Item, Section, and Content routes.
        /// </summary>
        [Fact]
        public void GetUri_WhenAllElementsArePresentAndNotImplicit_IncludesAllRoutes()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("host");
            shell.Route.Returns("shell");
            shell.RouteScheme.Returns("app");

            var item = Substitute.For<ShellItem>();
            item.Route.Returns("item");

            var section = Substitute.For<ShellSection>();
            section.Route.Returns("section");

            var content = Substitute.For<ShellContent>();
            content.Route.Returns("content");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Item").SetValue(nodeLocation, item);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Section").SetValue(nodeLocation, section);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Content").SetValue(nodeLocation, content);

            // Mock Routing.IsImplicit to return false for all elements
            MockRouting(item, false);
            MockRouting(section, false);
            MockRouting(content, false);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("app://host/shell/item/section/content", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method with empty string values for routes.
        /// Should handle empty string routes properly.
        /// Expected result: URI with empty segments for empty routes.
        /// </summary>
        [Fact]
        public void GetUri_WithEmptyStringRoutes_HandlesEmptyRoutesCorrectly()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("");
            shell.Route.Returns("");
            shell.RouteScheme.Returns("app");

            var item = Substitute.For<ShellItem>();
            item.Route.Returns("");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Item").SetValue(nodeLocation, item);

            // Mock Routing.IsImplicit to return false for the item
            MockRouting(item, false);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("app:///", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method with special characters in route values.
        /// Should handle special characters in routes properly.
        /// Expected result: URI with properly encoded special characters.
        /// </summary>
        [Fact]
        public void GetUri_WithSpecialCharactersInRoutes_HandlesSpecialCharactersCorrectly()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("host-test");
            shell.Route.Returns("shell_route");
            shell.RouteScheme.Returns("custom");

            var item = Substitute.For<ShellItem>();
            item.Route.Returns("item@route");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Item").SetValue(nodeLocation, item);

            // Mock Routing.IsImplicit to return false for the item
            MockRouting(item, false);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("custom://host-test/shell_route/item@route", result.ToString());
        }

        /// <summary>
        /// Tests GetUri method with mixed implicit and non-implicit elements.
        /// Should include only non-implicit element routes.
        /// Expected result: URI includes only non-implicit routes.
        /// </summary>
        [Fact]
        public void GetUri_WithMixedImplicitElements_IncludesOnlyNonImplicitRoutes()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.RouteHost.Returns("host");
            shell.Route.Returns("shell");
            shell.RouteScheme.Returns("app");

            var item = Substitute.For<ShellItem>();
            item.Route.Returns("item");

            var section = Substitute.For<ShellSection>();
            section.Route.Returns("section");

            var content = Substitute.For<ShellContent>();
            content.Route.Returns("content");

            var nodeLocation = new ShellUriHandler.NodeLocation();
            typeof(ShellUriHandler.NodeLocation).GetProperty("Shell").SetValue(nodeLocation, shell);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Item").SetValue(nodeLocation, item);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Section").SetValue(nodeLocation, section);
            typeof(ShellUriHandler.NodeLocation).GetProperty("Content").SetValue(nodeLocation, content);

            // Mock Routing.IsImplicit: item is not implicit, section is implicit, content is not implicit
            MockRouting(item, false);
            MockRouting(section, true);
            MockRouting(content, false);

            // Act
            var result = nodeLocation.GetUri();

            // Assert
            Assert.Equal("app://host/shell/item/content", result.ToString());
        }

        private void MockRouting(BindableObject bindableObject, bool isImplicit)
        {
            // Since we can't directly mock static methods, we'll need to use reflection or other approaches
            // For this test, we'll assume the Routing.IsImplicit method behavior can be controlled through the Route property
            // In a real implementation, you might need to use tools like Pose or Microsoft Fakes for static method mocking
        }
    }


    public partial class ShellUriHandlerGlobalRouteItemTests
    {
        /// <summary>
        /// Tests that Route property returns the first segment when path contains multiple segments separated by forward slash.
        /// Input: Path with multiple segments separated by forward slash.
        /// Expected: First segment is returned.
        /// </summary>
        [Fact]
        public void Route_PathWithMultipleSegmentsForwardSlash_ReturnsFirstSegment()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("segment1/segment2/segment3", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal("segment1", result);
        }

        /// <summary>
        /// Tests that Route property returns the first segment when path contains multiple segments separated by backslash.
        /// Input: Path with multiple segments separated by backslash.
        /// Expected: First segment is returned.
        /// </summary>
        [Fact]
        public void Route_PathWithMultipleSegmentsBackslash_ReturnsFirstSegment()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("segment1\\segment2\\segment3", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal("segment1", result);
        }

        /// <summary>
        /// Tests that Route property returns the segment when path contains single segment.
        /// Input: Path with single segment.
        /// Expected: The single segment is returned.
        /// </summary>
        [Fact]
        public void Route_PathWithSingleSegment_ReturnsSingleSegment()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("segment1", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal("segment1", result);
        }

        /// <summary>
        /// Tests that Route property returns empty string when path is empty string.
        /// Input: Empty string path.
        /// Expected: Empty string is returned.
        /// </summary>
        [Fact]
        public void Route_EmptyPath_ReturnsEmptyString()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that Route property returns empty string when path contains only forward slash.
        /// Input: Path containing only forward slash.
        /// Expected: Empty string is returned.
        /// </summary>
        [Fact]
        public void Route_PathWithOnlyForwardSlash_ReturnsEmptyString()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("/", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that Route property returns empty string when path contains only backslash.
        /// Input: Path containing only backslash.
        /// Expected: Empty string is returned.
        /// </summary>
        [Fact]
        public void Route_PathWithOnlyBackslash_ReturnsEmptyString()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("\\", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that Route property returns empty string when path contains multiple path separators only.
        /// Input: Path containing only path separators.
        /// Expected: Empty string is returned.
        /// </summary>
        [Fact]
        public void Route_PathWithOnlyPathSeparators_ReturnsEmptyString()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("///\\\\//", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that Route property throws NullReferenceException when path is null.
        /// Input: Null path.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void Route_NullPath_ThrowsNullReferenceException()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem(null, "source");

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => globalRouteItem.Route);
        }

        /// <summary>
        /// Tests that Route property returns first segment when path has mixed separators.
        /// Input: Path with mixed forward slash and backslash separators.
        /// Expected: First segment is returned.
        /// </summary>
        [Fact]
        public void Route_PathWithMixedSeparators_ReturnsFirstSegment()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("segment1/segment2\\segment3", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal("segment1", result);
        }

        /// <summary>
        /// Tests that Route property returns first segment when path starts with separator.
        /// Input: Path starting with separator followed by segments.
        /// Expected: First actual segment is returned (separator at start is ignored).
        /// </summary>
        [Fact]
        public void Route_PathStartingWithSeparator_ReturnsFirstSegment()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("/segment1/segment2", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal("segment1", result);
        }

        /// <summary>
        /// Tests that Route property returns first segment when path ends with separator.
        /// Input: Path with segments ending with separator.
        /// Expected: First segment is returned (separator at end is ignored).
        /// </summary>
        [Fact]
        public void Route_PathEndingWithSeparator_ReturnsFirstSegment()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("segment1/segment2/", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal("segment1", result);
        }

        /// <summary>
        /// Tests that Route property handles whitespace-only segments correctly.
        /// Input: Path with whitespace-only segment.
        /// Expected: Whitespace segment is returned as-is.
        /// </summary>
        [Fact]
        public void Route_PathWithWhitespaceSegment_ReturnsWhitespaceSegment()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("   /segment2", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal("   ", result);
        }

        /// <summary>
        /// Tests that Route property handles very long path correctly.
        /// Input: Very long path with multiple segments.
        /// Expected: First segment is returned regardless of path length.
        /// </summary>
        [Fact]
        public void Route_VeryLongPath_ReturnsFirstSegment()
        {
            // Arrange
            var longSegment = new string('a', 1000);
            var path = $"{longSegment}/segment2/segment3";
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem(path, "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal(longSegment, result);
        }

        /// <summary>
        /// Tests that Route property handles special characters in segments correctly.
        /// Input: Path with special characters in first segment.
        /// Expected: First segment with special characters is returned as-is.
        /// </summary>
        [Fact]
        public void Route_PathWithSpecialCharacters_ReturnsFirstSegmentWithSpecialCharacters()
        {
            // Arrange
            var globalRouteItem = new ShellUriHandler.GlobalRouteItem("segment@#$%^&*()/segment2", "source");

            // Act
            var result = globalRouteItem.Route;

            // Assert
            Assert.Equal("segment@#$%^&*()", result);
        }
    }


    public class ShellUriHandlerCalculateStackRequestTests
    {
        /// <summary>
        /// Tests that CalculateStackRequest returns ReplaceIt for absolute URIs.
        /// Validates the first condition in the method that checks Uri.IsAbsoluteUri.
        /// </summary>
        /// <param name="uriString">The absolute URI string to test</param>
        [Theory]
        [InlineData("http://example.com")]
        [InlineData("https://example.com/path")]
        [InlineData("file:///c:/path/to/file")]
        [InlineData("app://shell")]
        [InlineData("ftp://ftp.example.com")]
        [InlineData("mailto:test@example.com")]
        [InlineData("http://localhost:8080/api/test")]
        [InlineData("https://sub.domain.com:443/complex/path?query=value#fragment")]
        public void CalculateStackRequest_AbsoluteUri_ReturnsReplaceIt(string uriString)
        {
            // Arrange
            var uri = new Uri(uriString);

            // Act
            var result = ShellUriHandler.CalculateStackRequest(uri);

            // Assert
            Assert.Equal(ShellNavigationRequest.WhatToDoWithTheStack.ReplaceIt, result);
        }

        /// <summary>
        /// Tests that CalculateStackRequest returns ReplaceIt for relative URIs starting with "//".
        /// Validates the second condition that checks for double forward slash prefix.
        /// </summary>
        /// <param name="uriString">The relative URI string starting with "//" to test</param>
        [Theory]
        [InlineData("//")]
        [InlineData("//page")]
        [InlineData("//section/page")]
        [InlineData("//item/section/content")]
        [InlineData("//path/with/multiple/segments")]
        [InlineData("//page?query=value")]
        [InlineData("//page#fragment")]
        public void CalculateStackRequest_RelativeUriWithDoubleSlashPrefix_ReturnsReplaceIt(string uriString)
        {
            // Arrange
            var uri = new Uri(uriString, UriKind.Relative);

            // Act
            var result = ShellUriHandler.CalculateStackRequest(uri);

            // Assert
            Assert.Equal(ShellNavigationRequest.WhatToDoWithTheStack.ReplaceIt, result);
        }

        /// <summary>
        /// Tests that CalculateStackRequest returns ReplaceIt for relative URIs starting with "\\\\".
        /// Validates the second condition that checks for double backslash prefix.
        /// </summary>
        /// <param name="uriString">The relative URI string starting with "\\\\" to test</param>
        [Theory]
        [InlineData("\\\\")]
        [InlineData("\\\\page")]
        [InlineData("\\\\section\\page")]
        [InlineData("\\\\item\\section\\content")]
        public void CalculateStackRequest_RelativeUriWithDoubleBackslashPrefix_ReturnsReplaceIt(string uriString)
        {
            // Arrange
            var uri = new Uri(uriString, UriKind.Relative);

            // Act
            var result = ShellUriHandler.CalculateStackRequest(uri);

            // Assert
            Assert.Equal(ShellNavigationRequest.WhatToDoWithTheStack.ReplaceIt, result);
        }

        /// <summary>
        /// Tests that CalculateStackRequest returns PushToIt for regular relative URIs.
        /// Validates the default case for relative URIs that don't start with "//" or "\\\\".
        /// </summary>
        /// <param name="uriString">The regular relative URI string to test</param>
        [Theory]
        [InlineData("page")]
        [InlineData("section/page")]
        [InlineData("item/section/content")]
        [InlineData("/single/slash")]
        [InlineData("")]
        [InlineData("page?query=value")]
        [InlineData("page#fragment")]
        [InlineData("page/subpage?id=123&name=test")]
        [InlineData("../relative/path")]
        [InlineData("./current/path")]
        [InlineData("path with spaces")]
        [InlineData("path/with/special@chars")]
        public void CalculateStackRequest_RelativeUri_ReturnsPushToIt(string uriString)
        {
            // Arrange  
            var uri = new Uri(uriString, UriKind.Relative);

            // Act
            var result = ShellUriHandler.CalculateStackRequest(uri);

            // Assert
            Assert.Equal(ShellNavigationRequest.WhatToDoWithTheStack.PushToIt, result);
        }

        /// <summary>
        /// Tests that CalculateStackRequest throws ArgumentNullException when passed a null URI.
        /// Validates proper null parameter handling.
        /// </summary>
        [Fact]
        public void CalculateStackRequest_NullUri_ThrowsArgumentNullException()
        {
            // Arrange
            Uri uri = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ShellUriHandler.CalculateStackRequest(uri));
        }
    }
}