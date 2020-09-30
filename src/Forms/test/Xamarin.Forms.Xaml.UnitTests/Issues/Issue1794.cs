using System;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1794
	{
		[Test]
		public void FindNameInDT()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
						 xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
						 xmlns:local=""clr-namespace:XamarinFormsXamlSample;assembly=XamarinFormsXamlSample""
						 xmlns:constants=""clr-namespace:XamarinFormsSample;assembly=XamarinFormsXamlSample""
						 x:Class=""UxDemoAppXF.Layouts.Menu""
						 Title=""Employee List"">
 
					<ListView x:Name=""listView""
						IsVisible=""true""
						ItemsSource=""{Binding MenuItems}""
						SelectedItem=""{Binding ListItemSelected}"">
		
						<ListView.ItemTemplate>
							<DataTemplate>
								<ViewCell>
									<RelativeLayout>
										<Label x:Name=""LinkText""
													 Text=""{Binding Name}""
													 RelativeLayout.XConstraint=
														""{ConstraintExpression  Type=RelativeToParent, 
																										Property=Width, 
																										Factor=0.5}""/>
										<Image x:Name=""LinkImage"" 
													 Source=""{Binding ImageSource}""
													 RelativeLayout.XConstraint=
														""{ConstraintExpression  Type=RelativeToView,
																				Property=Width, 
																				ElementName=LinkText,
																				Constant=5}""/>
									 
				 
									</RelativeLayout>
								</ViewCell>
							</DataTemplate>
						</ListView.ItemTemplate>
					</ListView> 
				</ContentPage>";
			var layout = new ContentPage().LoadFromXaml(xaml);
			var list = layout.FindByName<ListView>("listView");
			var item0 = list.TemplatedItems.GetOrCreateContent(0, null);
			var item1 = list.TemplatedItems.GetOrCreateContent(1, null);
			Assert.Pass();
		}
	}
}