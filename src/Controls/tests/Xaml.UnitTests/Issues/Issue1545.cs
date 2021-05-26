using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1545
	{
		[SetUp]
		public void Setup()
		{
			Device.PlatformServices = new MockPlatformServices { RuntimePlatform = Device.iOS };
		}

		[TearDown]
		public void TearDown()
		{
			Device.PlatformServices = null;
		}

		[Test]
		public void BindingCanNotBeReused()
		{
			string xaml = @"<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
						 xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
						 x:Class=""Microsoft.Maui.Controls.ControlGallery.Issue1545"">
						<ListView x:Name=""List"" ItemsSource=""{Binding}"">
							<ListView.ItemTemplate>
								<DataTemplate>
									<TextCell Text=""{Binding}"" />
								</DataTemplate>
							</ListView.ItemTemplate>
						</ListView>
				</ContentPage>";

			ContentPage page = new ContentPage().LoadFromXaml(xaml);

			var items = new[] { "Fu", "Bar" };
			page.BindingContext = items;

			ListView lv = page.FindByName<ListView>("List");

			TextCell cell = (TextCell)lv.TemplatedItems.GetOrCreateContent(0, items[0]);
			Assert.That(cell.Text, Is.EqualTo("Fu"));

			cell = (TextCell)lv.TemplatedItems.GetOrCreateContent(1, items[1]);
			Assert.That(cell.Text, Is.EqualTo("Bar"));
		}

		[Test]
		public void ElementsCanNotBeReused()
		{
			string xaml = @"<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
						 xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
						 x:Class=""Microsoft.Maui.Controls.ControlGallery.Issue1545"">
							<ContentPage.Resources>
							<ResourceDictionary>
							<Color x:Key=""color"">#ff00aa</Color>
							</ResourceDictionary>
							</ContentPage.Resources>

						<ListView x:Name=""List"" ItemsSource=""{Binding}"">
							<ListView.ItemTemplate>
								<DataTemplate>
									<ViewCell>
									<StackLayout>
										<Label Text=""{Binding}"" BackgroundColor=""{StaticResource color}""/>
									</StackLayout>
									</ViewCell>
								</DataTemplate>
							</ListView.ItemTemplate>
						</ListView>
				</ContentPage>";

			ContentPage page = new ContentPage().LoadFromXaml(xaml);

			var items = new[] { "Fu", "Bar" };
			page.BindingContext = items;

			ListView lv = page.FindByName<ListView>("List");

			ViewCell cell0 = (ViewCell)lv.TemplatedItems.GetOrCreateContent(0, items[0]);


			Assert.That(((Label)((StackLayout)cell0.View).Children[0]).Text, Is.EqualTo("Fu"));

			ViewCell cell1 = (ViewCell)lv.TemplatedItems.GetOrCreateContent(1, items[1]);
			Assert.That(((Label)((StackLayout)cell1.View).Children[0]).Text, Is.EqualTo("Bar"));

			Assert.AreNotSame(cell0, cell1);
			Assert.AreNotSame(cell0.View, cell1.View);
			Assert.AreNotSame(((StackLayout)cell0.View).Children[0], ((StackLayout)cell1.View).Children[0]);
			Assert.AreEqual(Color.FromArgb("ff00aa"), ((StackLayout)cell1.View).Children[0].BackgroundColor);
		}

		[Test]
		public void ElementsFromCollectionsAreNotReused()
		{
			var xaml = @"<ListView 
						xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
						xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
						xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests""
						ItemsSource=""{Binding}"">
							<ListView.ItemTemplate>
								<DataTemplate>
									<local:ViewCellWithCollection>
										<local:ViewCellWithCollection.Children>
											<local:ViewList>
												<Label />
												<Label />
											</local:ViewList>
										</local:ViewCellWithCollection.Children>
									</local:ViewCellWithCollection>
								</DataTemplate>
							</ListView.ItemTemplate>
						</ListView>";

			var listview = new ListView();
			var items = new[] { "Foo", "Bar", "Baz" };
			listview.BindingContext = items;
			listview.LoadFromXaml(xaml);
			var cell0 = (ViewCellWithCollection)listview.TemplatedItems.GetOrCreateContent(0, items[0]);
			var cell1 = (ViewCellWithCollection)listview.TemplatedItems.GetOrCreateContent(1, items[1]);

			Assert.AreNotSame(cell0, cell1);
			Assert.AreNotSame(cell0.Children, cell1.Children);
			Assert.AreNotSame(cell0.Children[0], cell1.Children[0]);

		}

		[Test]
		public void ResourcesDeclaredInDataTemplatesAreNotShared()
		{
			var xaml = @"<ListView 
						xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
						xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
						xmlns:sys=""clr-namespace:System;assembly=mscorlib""
						ItemsSource=""{Binding}"">
							<ListView.ItemTemplate>
								<DataTemplate>
									<ViewCell>
										<Label Text=""{Binding}"">
											<Label.Resources>
												<ResourceDictionary>
													<sys:Object x:Key=""object""/>
												</ResourceDictionary>
											</Label.Resources>
										</Label>
									</ViewCell>
								</DataTemplate>
							</ListView.ItemTemplate>
						</ListView>";

			var listview = new ListView();
			var items = new[] { "Foo", "Bar", "Baz" };
			listview.BindingContext = items;

			listview.LoadFromXaml(xaml);
			var cell0 = (ViewCell)listview.TemplatedItems.GetOrCreateContent(0, items[0]);
			var cell1 = (ViewCell)listview.TemplatedItems.GetOrCreateContent(1, items[1]);
			Assert.AreNotSame(cell0, cell1);

			var label0 = (Label)cell0.View;
			var label1 = (Label)cell1.View;
			Assert.AreNotSame(label0, label1);
			Assert.AreEqual("Foo", label0.Text);
			Assert.AreEqual("Bar", label1.Text);

			var res0 = label0.Resources;
			var res1 = label1.Resources;
			Assert.AreNotSame(res0, res1);

			var obj0 = res0["object"];
			var obj1 = res1["object"];

			Assert.NotNull(obj0);
			Assert.NotNull(obj1);

			Assert.AreNotSame(obj0, obj1);
		}
	}

	public class ViewCellWithCollection : ViewCell
	{
		public ViewList Children { get; set; }
	}
}
