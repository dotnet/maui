using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{

	public class Issue1641
	{
		[Fact]
		public void StaticResourceInTableView()
		{
			var xaml = @"
					<ContentPage
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
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
			Assert.Equal("Hello there!", page.Resources["caption"] as string);
			Assert.Equal("Hello there!", (table.Root[0][0] as TextCell).Text);

		}
	}
}

