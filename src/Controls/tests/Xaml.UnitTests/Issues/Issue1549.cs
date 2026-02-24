using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{

	public class SeverityColorConverter : IValueConverter
	{

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			count++;
			return Colors.Blue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}


		public static int count = 0;
	}

	public class InvertBoolenConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			count++;
			if (value is bool)
			{

				return !(bool)value;
			}
			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
		public static int count = 0;
	}

	public class Item
	{
		public bool IsLocked
		{
			get;
			set;
		}
	}

	[Collection("Issue")]
	public class Issue1549 : IDisposable
	{
		public Issue1549()
		{
			SeverityColorConverter.count = 0;
			InvertBoolenConverter.count = 0;
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Fact]
		public void ConverterIsInvoked()
		{
			var xaml = @"
<ContentPage 							
xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
							xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"">

<ContentPage.Resources>
<ResourceDictionary>
<local:SeverityColorConverter x:Key=""SeverityColorConverter"" />
</ResourceDictionary>
</ContentPage.Resources>
				<Label Text=""{Binding value, StringFormat='{0}'}"" 
					WidthRequest=""50"" 
					TextColor=""Black""
					x:Name=""label""
					BackgroundColor=""{Binding Severity, Converter={StaticResource SeverityColorConverter}}""
					HorizontalTextAlignment=""Center"" VerticalTextAlignment=""Center""/>
</ContentPage>";

			var layout = new ContentPage().LoadFromXaml(xaml);
			layout.BindingContext = new { Value = "Foo", Severity = "Bar" };
			var label = layout.FindByName<Label>("label");
			Assert.Equal(Colors.Blue, label.BackgroundColor);
			Assert.Equal(1, SeverityColorConverter.count);
		}

		[Fact]
		public void ConverterIsInvoked_Escaped()
		{
			var xaml = @"
<ContentPage 							
xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
							xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"">

<ContentPage.Resources>
<ResourceDictionary>
<local:SeverityColorConverter x:Key=""SeverityColorConverter"" />
</ResourceDictionary>
</ContentPage.Resources>
				<Label Text=""{Binding value, StringFormat='{}{0}'}"" 
					WidthRequest=""50"" 
					TextColor=""Black""
					x:Name=""label""
					BackgroundColor=""{Binding Severity, Converter={StaticResource SeverityColorConverter}}""
					HorizontalTextAlignment=""Center"" VerticalTextAlignment=""Center""/>
</ContentPage>";

			var layout = new ContentPage().LoadFromXaml(xaml);
			layout.BindingContext = new { Value = "Foo", Severity = "Bar" };
			var label = layout.FindByName<Label>("label");
			Assert.Equal(Colors.Blue, label.BackgroundColor);
			Assert.Equal(1, SeverityColorConverter.count);
		}

		[Fact]
		public void ResourcesInNonXFBaseClassesAreFound()
		{
			var xaml = @"<local:BaseView 
	xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" 
	xmlns:cmp=""clr-namespace:Microsoft.Maui.Controls.Compatibility;assembly=Microsoft.Maui.Controls""
  	xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests""
	Padding=""0,40,0,0"">
    <local:BaseView.Resources>
    <ResourceDictionary>
     	<local:InvertBoolenConverter x:Key=""cnvInvert""></local:InvertBoolenConverter>
    </ResourceDictionary>
    </local:BaseView.Resources>
	<local:BaseView.Content>
		<ListView x:Name=""lst"" VerticalOptions=""FillAndExpand""
        		HorizontalOptions=""FillAndExpand""
			
				ItemsSource=""{Binding Items}""

			>
		<ListView.ItemTemplate >
			<DataTemplate> 
				<ViewCell >
				<ViewCell.View>
					<cmp:Grid  VerticalOptions=""FillAndExpand"" HorizontalOptions=""FillAndExpand""  >			
					<Label  IsVisible=""{Binding IsLocked}""  Text=""Show Is Locked""  />
					<Label  IsVisible=""{Binding IsLocked, Converter={StaticResource cnvInvert}}"" Text=""Show Is Not locked"" />
				</cmp:Grid>
				</ViewCell.View>
				</ViewCell>
			</DataTemplate>
		</ListView.ItemTemplate>
		</ListView>
	</local:BaseView.Content>
</local:BaseView>";
			var page = new Issue1549Page().LoadFromXaml(xaml);
			var lst = page.FindByName<ListView>("lst");
			ObservableCollection<Item> items;
			lst.BindingContext = new
			{
				Items = items = new ObservableCollection<Item> {
					new Item { IsLocked = true},
					new Item { IsLocked = false},
					new Item { IsLocked = true},
					new Item { IsLocked = true},
				},
			};

			var cell0 = (ViewCell)lst.TemplatedItems.GetOrCreateContent(0, items[0]);
			var cell1 = (ViewCell)lst.TemplatedItems.GetOrCreateContent(1, items[1]);
			var cell2 = (ViewCell)lst.TemplatedItems.GetOrCreateContent(2, items[2]);
			var cell3 = (ViewCell)lst.TemplatedItems.GetOrCreateContent(3, items[3]);

			var label00 = (cell0.View as Compatibility.Grid).Children[0] as Label;
			var label01 = (cell0.View as Compatibility.Grid).Children[1] as Label;

			Assert.Equal("Show Is Locked", label00.Text);
			Assert.Equal("Show Is Not locked", label01.Text);

			Assert.True(label00.IsVisible);
			Assert.False(label01.IsVisible);

			Assert.Equal(4, InvertBoolenConverter.count);

		}
	}

	public class BaseView : ContentPage
	{
	}

	public partial class Issue1549Page : BaseView
	{
	}
}

