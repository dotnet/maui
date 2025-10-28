using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{	public class NameScopeTests : BaseTestFixture
	{
		[Fact]
		public void TopLevelObjectsHaveANameScope()
		{
			var xaml = @"
				<View 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";

			var view = new CustomView().LoadFromXaml(xaml);

			Assert.NotNull(Maui.Controls.Internals.NameScope.GetNameScope(view));
			Assert.IsType<Maui.Controls.Internals.NameScope>(Maui.Controls.Internals.NameScope.GetNameScope(view));
		}

		[Fact]
		public void NameScopeAreSharedWithChildren()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" >
					<Label />
					<Label />
				</StackLayout>";

			var layout = new Controls.Compatibility.StackLayout().LoadFromXaml(xaml);

			Assert.NotNull(Maui.Controls.Internals.NameScope.GetNameScope(layout));
			Assert.IsType<Maui.Controls.Internals.NameScope>(Maui.Controls.Internals.NameScope.GetNameScope(layout));

			foreach (var child in layout.Children)
			{
				Assert.Null(Maui.Controls.Internals.NameScope.GetNameScope(child));
				Assert.Same(Maui.Controls.Internals.NameScope.GetNameScope(layout), child.GetNameScope());
			}
		}

		[Fact]
		public void DataTemplateChildrenDoesNotParticipateToParentNameScope()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			var xaml = @"
				<ListView
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Name=""listview"">
					<ListView.ItemTemplate>
						<DataTemplate>
						    <TextCell Text=""{Binding name}"" x:Name=""textcell""/>
						</DataTemplate>
					</ListView.ItemTemplate>
				</ListView>";

			var listview = new ListView();
			listview.LoadFromXaml(xaml);

			Assert.Same(listview, ((Maui.Controls.Internals.INameScope)listview).FindByName("listview"));
			Assert.Null(((Maui.Controls.Internals.INameScope)listview).FindByName("textcell"));
		}

		[Fact]
		public void ElementsCreatedFromDataTemplateHaveTheirOwnNameScope()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			var xaml = @"
				<ListView
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Name=""listview"">
					<ListView.ItemTemplate>
						<DataTemplate>
						    <TextCell Text=""{Binding name}"" x:Name=""textcell""/>
						</DataTemplate>
					</ListView.ItemTemplate>
				</ListView>";

			var listview = new ListView();
			listview.LoadFromXaml(xaml);
			Assert.NotNull(Maui.Controls.Internals.NameScope.GetNameScope(listview));
			Assert.IsType<Maui.Controls.Internals.NameScope>(Maui.Controls.Internals.NameScope.GetNameScope(listview));

			var cell0 = listview.ItemTemplate.CreateContent() as Element;
			var cell1 = listview.ItemTemplate.CreateContent() as Element;

			Assert.NotNull(Maui.Controls.Internals.NameScope.GetNameScope(cell0));
			Assert.IsType<Maui.Controls.Internals.NameScope>(Maui.Controls.Internals.NameScope.GetNameScope(cell0));
			Assert.NotNull(Maui.Controls.Internals.NameScope.GetNameScope(cell1));
			Assert.IsType<Maui.Controls.Internals.NameScope>(Maui.Controls.Internals.NameScope.GetNameScope(cell1));

			Assert.NotSame(Maui.Controls.Internals.NameScope.GetNameScope(listview), Maui.Controls.Internals.NameScope.GetNameScope(cell0));
			Assert.NotSame(Maui.Controls.Internals.NameScope.GetNameScope(listview), Maui.Controls.Internals.NameScope.GetNameScope(cell1));
			Assert.NotSame(Maui.Controls.Internals.NameScope.GetNameScope(cell0), Maui.Controls.Internals.NameScope.GetNameScope(cell1));

			Assert.Null(((Maui.Controls.Internals.INameScope)listview).FindByName("textcell"));
			Assert.NotNull(((Maui.Controls.Internals.INameScope)cell0).FindByName("textcell"));
			Assert.NotNull(((Maui.Controls.Internals.INameScope)cell1).FindByName("textcell"));

			Assert.NotSame(((Maui.Controls.Internals.INameScope)cell0).FindByName("textcell"), ((Maui.Controls.Internals.INameScope)cell1).FindByName("textcell"));

		}
	}
}
