using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1554
	{
		[SetUp]
		public void Setup()
		{
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public void TearDown()
		{
			Device.PlatformServices = null;
		}

		[Test]
		public void CollectionItemsInDataTemplate()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
				<ListView 
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" 
					ItemsSource=""{Binding}"">
			        <ListView.ItemTemplate>
			          <DataTemplate>
			            <ViewCell>
			              <ViewCell.View>
			                <StackLayout>
			                  <Label Text=""{Binding}""></Label>
			                  <Label Text=""{Binding}""></Label>
			                </StackLayout>
			              </ViewCell.View>
			            </ViewCell>
			          </DataTemplate>
			        </ListView.ItemTemplate>
			      </ListView>";
			var listview = new ListView();
			var items = new[] { "Foo", "Bar", "Baz" };
			listview.BindingContext = items;

			listview.LoadFromXaml(xaml);

			ViewCell cell0 = null;
			Assert.DoesNotThrow(() =>
			{
				cell0 = (ViewCell)listview.TemplatedItems.GetOrCreateContent(0, items[0]);
			});
			ViewCell cell1 = null;
			Assert.DoesNotThrow(() =>
			{
				cell1 = (ViewCell)listview.TemplatedItems.GetOrCreateContent(1, items[1]);
			});

			Assert.AreNotSame(cell0, cell1);
			Assert.AreNotSame(cell0.View, cell1.View);
			Assert.AreNotSame(((StackLayout)cell0.View).Children[0], ((StackLayout)cell1.View).Children[0]);
			Assert.AreNotSame(((StackLayout)cell0.View).Children[1], ((StackLayout)cell1.View).Children[1]);

		}
	}
}

