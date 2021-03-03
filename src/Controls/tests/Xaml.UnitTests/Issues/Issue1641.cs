using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1641
	{
		[Test]
		public void StaticResourceInTableView()
		{
			var xaml = @"
					<ContentPage
					xmlns=""http://xamarin.com/schemas/2014/forms""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
						<ContentPage.Resources>
					        <ResourceDictionary>
					          <x:String x:Key=""caption"" >Hello there!</x:String>
					        </ResourceDictionary>
						</ContentPage.Resources>

					    <TableView>                 
					        <TableRoot Title=""x"">
					            <TableSection Title=""y"">
					                <TextCell Text=""{StaticResource caption}"" />
					            </TableSection>
					        </TableRoot>
					    </TableView>
					</ContentPage>";
			var page = new ContentPage().LoadFromXaml(xaml);
			var table = page.Content as TableView;
			Assert.AreEqual("Hello there!", page.Resources["caption"] as string);
			Assert.AreEqual("Hello there!", (table.Root[0][0] as TextCell).Text);

		}
	}
}

