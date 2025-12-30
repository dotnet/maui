using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[Collection("Issue")]
	public class Issue1794 : IDisposable
	{
		public Issue1794() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Fact]
		public void FindNameInDT()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
						 xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
						 xmlns:cmp=""clr-namespace:Microsoft.Maui.Controls.Compatibility;assembly=Microsoft.Maui.Controls""
						 xmlns:local=""clr-namespace:Microsoft.Maui.ControlsFormsXamlSample;assembly=Microsoft.Maui.ControlsFormsXamlSample""
						 xmlns:constants=""clr-namespace:Microsoft.Maui.ControlsFormsSample;assembly=Microsoft.Maui.ControlsFormsXamlSample""
						 x:Class=""UxDemoAppXF.Layouts.Menu""
						 Title=""Employee List"">
 
					<ListView x:Name=""listView""
						IsVisible=""true""
						ItemsSource=""{Binding MenuItems}""
						SelectedItem=""{Binding ListItemSelected}"">
		
						<ListView.ItemTemplate>
							<DataTemplate>
								<ViewCell>
									<cmp:RelativeLayout>
										<Label x:Name=""LinkText""
													 Text=""{Binding Name}""
													 cmp:RelativeLayout.XConstraint=
														""{cmp:ConstraintExpression  Type=RelativeToParent, 
																										Property=Width, 
																										Factor=0.5}""/>
										<Image x:Name=""LinkImage"" 
													 Source=""{Binding ImageSource}""
													 cmp:RelativeLayout.XConstraint=
														""{cmp:ConstraintExpression  Type=RelativeToView,
																				Property=Width, 
																				ElementName=LinkText,
																				Constant=5}""/>
									 
				 
									</cmp:RelativeLayout>
								</ViewCell>
							</DataTemplate>
						</ListView.ItemTemplate>
					</ListView> 
				</ContentPage>";
			var layout = new ContentPage().LoadFromXaml(xaml);
			var list = layout.FindByName<ListView>("listView");
			var item0 = list.TemplatedItems.GetOrCreateContent(0, null);
			var item1 = list.TemplatedItems.GetOrCreateContent(1, null);
			Assert.NotNull(item0);
			Assert.NotNull(item1);
		}
	}
}