using NUnit.Framework;

using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{

	[TestFixture]
	public class NameScopeTests : BaseTestFixture
	{
		[Test]
		public void TopLevelObjectsHaveANameScope ()
		{
			var xaml = @"
				<View 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" />";

			var view = new CustomView ().LoadFromXaml (xaml);

			Assert.IsNotNull (System.Maui.Internals.NameScope.GetNameScope (view));
			Assert.That (System.Maui.Internals.NameScope.GetNameScope (view), Is.TypeOf<System.Maui.Internals.NameScope> ());
		}

		[Test]
		public void NameScopeAreSharedWithChildren ()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" >
					<Label />
					<Label />
				</StackLayout>";

			var layout = new StackLayout ().LoadFromXaml (xaml);

			Assert.IsNotNull (System.Maui.Internals.NameScope.GetNameScope (layout));
			Assert.That (System.Maui.Internals.NameScope.GetNameScope (layout), Is.TypeOf<System.Maui.Internals.NameScope> ());

			foreach (var child in layout.Children) {
				Assert.IsNull (System.Maui.Internals.NameScope.GetNameScope (child));
				Assert.AreSame (System.Maui.Internals.NameScope.GetNameScope (layout), child.GetNameScope());
			}
		}

		[Test]
		public void DataTemplateChildrenDoesNotParticipateToParentNameScope ()
		{
			var xaml = @"
				<ListView
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Name=""listview"">
					<ListView.ItemTemplate>
						<DataTemplate>
						    <TextCell Text=""{Binding name}"" x:Name=""textcell""/>
						</DataTemplate>
					</ListView.ItemTemplate>
				</ListView>";

			var listview = new ListView ();
			listview.LoadFromXaml (xaml);	

			Assert.AreSame (listview, ((System.Maui.Internals.INameScope)listview).FindByName ("listview"));
			Assert.IsNull (((System.Maui.Internals.INameScope)listview).FindByName ("textcell"));
		}

		[Test]
		public void ElementsCreatedFromDataTemplateHaveTheirOwnNameScope ()
		{
			var xaml = @"
				<ListView
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Name=""listview"">
					<ListView.ItemTemplate>
						<DataTemplate>
						    <TextCell Text=""{Binding name}"" x:Name=""textcell""/>
						</DataTemplate>
					</ListView.ItemTemplate>
				</ListView>";

			var listview = new ListView ();
			listview.LoadFromXaml (xaml);	
			Assert.IsNotNull (System.Maui.Internals.NameScope.GetNameScope (listview));
			Assert.That (System.Maui.Internals.NameScope.GetNameScope (listview), Is.TypeOf<System.Maui.Internals.NameScope> ());

			var cell0 = listview.ItemTemplate.CreateContent () as Element;
			var cell1 = listview.ItemTemplate.CreateContent () as Element;

			Assert.IsNotNull (System.Maui.Internals.NameScope.GetNameScope (cell0));
			Assert.That (System.Maui.Internals.NameScope.GetNameScope (cell0), Is.TypeOf<System.Maui.Internals.NameScope> ());
			Assert.IsNotNull (System.Maui.Internals.NameScope.GetNameScope (cell1));
			Assert.That (System.Maui.Internals.NameScope.GetNameScope (cell1), Is.TypeOf<System.Maui.Internals.NameScope> ());

			Assert.AreNotSame (System.Maui.Internals.NameScope.GetNameScope (listview), System.Maui.Internals.NameScope.GetNameScope (cell0));
			Assert.AreNotSame (System.Maui.Internals.NameScope.GetNameScope (listview), System.Maui.Internals.NameScope.GetNameScope (cell1));
			Assert.AreNotSame (System.Maui.Internals.NameScope.GetNameScope (cell0), System.Maui.Internals.NameScope.GetNameScope (cell1));

			Assert.IsNull (((System.Maui.Internals.INameScope)listview).FindByName ("textcell"));
			Assert.NotNull (((System.Maui.Internals.INameScope)cell0).FindByName ("textcell"));
			Assert.NotNull (((System.Maui.Internals.INameScope)cell1).FindByName ("textcell"));

			Assert.AreNotSame (((System.Maui.Internals.INameScope)cell0).FindByName ("textcell"), ((System.Maui.Internals.INameScope)cell1).FindByName ("textcell"));

		}
	}
}
