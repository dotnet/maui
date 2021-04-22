using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{

	[TestFixture]
	public class NameScopeTests : BaseTestFixture
	{
		[Test]
		public void TopLevelObjectsHaveANameScope()
		{
			var xaml = @"
				<View 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";

			var view = new CustomView().LoadFromXaml(xaml);

			Assert.IsNotNull(Maui.Controls.Internals.NameScope.GetNameScope(view));
			Assert.That(Maui.Controls.Internals.NameScope.GetNameScope(view), Is.TypeOf<Maui.Controls.Internals.NameScope>());
		}

		[Test]
		public void NameScopeAreSharedWithChildren()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" >
					<Label />
					<Label />
				</StackLayout>";

			var layout = new StackLayout().LoadFromXaml(xaml);

			Assert.IsNotNull(Maui.Controls.Internals.NameScope.GetNameScope(layout));
			Assert.That(Maui.Controls.Internals.NameScope.GetNameScope(layout), Is.TypeOf<Maui.Controls.Internals.NameScope>());

			foreach (var child in layout.Children)
			{
				Assert.IsNull(Maui.Controls.Internals.NameScope.GetNameScope(child));
				Assert.AreSame(Maui.Controls.Internals.NameScope.GetNameScope(layout), child.GetNameScope());
			}
		}

		[Test]
		public void DataTemplateChildrenDoesNotParticipateToParentNameScope()
		{
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

			Assert.AreSame(listview, ((Maui.Controls.Internals.INameScope)listview).FindByName("listview"));
			Assert.IsNull(((Maui.Controls.Internals.INameScope)listview).FindByName("textcell"));
		}

		[Test]
		public void ElementsCreatedFromDataTemplateHaveTheirOwnNameScope()
		{
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
			Assert.IsNotNull(Maui.Controls.Internals.NameScope.GetNameScope(listview));
			Assert.That(Maui.Controls.Internals.NameScope.GetNameScope(listview), Is.TypeOf<Maui.Controls.Internals.NameScope>());

			var cell0 = listview.ItemTemplate.CreateContent() as Element;
			var cell1 = listview.ItemTemplate.CreateContent() as Element;

			Assert.IsNotNull(Maui.Controls.Internals.NameScope.GetNameScope(cell0));
			Assert.That(Maui.Controls.Internals.NameScope.GetNameScope(cell0), Is.TypeOf<Maui.Controls.Internals.NameScope>());
			Assert.IsNotNull(Maui.Controls.Internals.NameScope.GetNameScope(cell1));
			Assert.That(Maui.Controls.Internals.NameScope.GetNameScope(cell1), Is.TypeOf<Maui.Controls.Internals.NameScope>());

			Assert.AreNotSame(Maui.Controls.Internals.NameScope.GetNameScope(listview), Maui.Controls.Internals.NameScope.GetNameScope(cell0));
			Assert.AreNotSame(Maui.Controls.Internals.NameScope.GetNameScope(listview), Maui.Controls.Internals.NameScope.GetNameScope(cell1));
			Assert.AreNotSame(Maui.Controls.Internals.NameScope.GetNameScope(cell0), Maui.Controls.Internals.NameScope.GetNameScope(cell1));

			Assert.IsNull(((Maui.Controls.Internals.INameScope)listview).FindByName("textcell"));
			Assert.NotNull(((Maui.Controls.Internals.INameScope)cell0).FindByName("textcell"));
			Assert.NotNull(((Maui.Controls.Internals.INameScope)cell1).FindByName("textcell"));

			Assert.AreNotSame(((Maui.Controls.Internals.INameScope)cell0).FindByName("textcell"), ((Maui.Controls.Internals.INameScope)cell1).FindByName("textcell"));

		}
	}
}
