using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Mono.Cecil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[ContentProperty("Content")]
	public class CustomView : View
	{
		public string NotBindable { get; set; }

		public View Content { get; set; }

		public MockFlags MockFlags { get; set; }
	}

	[ContentProperty("Children")]
	public class ViewWithChildrenContent : View
	{
		public ViewWithChildrenContent()
		{
			Children = DefaultChildren = new ViewList();
		}
		public ViewList DefaultChildren;
		public ViewList Children { get; set; }
	}

	public class ViewList : List<View>
	{

	}

	public class ReverseConverter : IValueConverter
	{
		public static ReverseConverter Instance = new ReverseConverter();

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var s = value as string;
			if (s == null)
				return value;
			return new string(s.Reverse().ToArray());
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var s = value as string;
			if (s == null)
				return value;
			return new string(s.Reverse().ToArray());
		}
	}

	public class Catalog
	{
		public static readonly BindableProperty MessageProperty =
			BindableProperty.CreateAttached("Message", typeof(string), typeof(Catalog), default(string),
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					var label = bindable as Label;
					label?.SetValue(Label.TextProperty, new string(((string)newvalue).Reverse().ToArray()));
				});

		public static string GetMessage(BindableObject bindable)
		{
			return (string)bindable.GetValue(MessageProperty);
		}

		public static void SetMessage(BindableObject bindable, string value)
		{
			bindable.SetValue(MessageProperty, value);
		}
	}

	[Flags]
	public enum MockFlags
	{
		Foo = 1 << 0,
		Bar = 1 << 1,
		Baz = 1 << 2,
		Qux = 1 << 3,
	}


	public class LoaderTests : BaseTestFixture
	{
		[Fact]
		public void TestRootName()
		{
			var xaml = @"
				<View
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.CustomView"" 
				x:Name=""customView"" 
				/>";

			var view = new CustomView();
			view.LoadFromXaml(xaml);

			Assert.Same(view, ((Maui.Controls.Internals.INameScope)view).FindByName("customView"));
		}

		[Fact]
		public void TestFindByXName()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<StackLayout.Children>
						<Label x:Name=""label0"" Text=""Foo""/>
					</StackLayout.Children>
				</StackLayout>";

			var stacklayout = new StackLayout();
			stacklayout.LoadFromXaml(xaml);

			var label = stacklayout.FindByName<Label>("label0");
			Assert.NotNull(label);
			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void TestUnknownPropertyShouldThrow()
		{
			var xaml = @"
				<Label 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				Text=""Foo""
				UnknownProperty=""Bar""
			    />";

			var label = new Label();
			XamlParseExceptionHelper.AssertThrows(() => label.LoadFromXaml(xaml), 5, 5);
		}

		[Fact]
		public void TestSetValueToBindableProperty()
		{
			var xaml = @"
			<Label 
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			Text=""Foo""
			/>";

			var label = new Label();

			label.LoadFromXaml(xaml);
			Assert.Equal("Foo", label.Text);

		}

		[Fact]
		public void TestSetBindingToBindableProperty()
		{
			var xaml = @"
			<Label 
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			Text=""{Binding Path=labeltext}""
			/>";

			var label = new Label();
			label.LoadFromXaml(xaml);

			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);

			label.BindingContext = new { labeltext = "Foo" };
			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void TestSetBindingToNonBindablePropertyShouldThrow()
		{
			var xaml = @"
				<View 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.CustomView"" 
				Name=""customView"" 
				NotBindable=""{Binding text}""
				/>";

			var view = new CustomView();
			XamlParseExceptionHelper.AssertThrows(() => view.LoadFromXaml(xaml), 6, 5);
		}

		[Fact]
		public void TestBindingPath()
		{
			var xaml = @"
				<cmp:StackLayout 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:cmp=""clr-namespace:Microsoft.Maui.Controls.Compatibility;assembly=Microsoft.Maui.Controls""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<cmp:StackLayout.Children>
						<Label x:Name=""label0"" Text=""{Binding text}""/>
						<Label x:Name=""label1"" Text=""{Binding Path=text}""/>
					</cmp:StackLayout.Children>
				</cmp:StackLayout>";

			var stacklayout = new Compatibility.StackLayout();
			stacklayout.LoadFromXaml(xaml);

			var label0 = stacklayout.FindByName<Label>("label0");
			var label1 = stacklayout.FindByName<Label>("label1");

			Assert.Equal(Label.TextProperty.DefaultValue, label0.Text);
			Assert.Equal(Label.TextProperty.DefaultValue, label1.Text);

			stacklayout.BindingContext = new { text = "Foo" };
			Assert.Equal("Foo", label0.Text);
			Assert.Equal("Foo", label1.Text);
		}


		class ViewModel
		{
			public string Text { get; set; }
		}

		[Fact]
		public void TestBindingModeAndConverter()
		{
			var xaml = @"
				<ContentPage 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:cmp=""clr-namespace:Microsoft.Maui.Controls.Compatibility;assembly=Microsoft.Maui.Controls""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"">
					<ContentPage.Resources>
						<ResourceDictionary>
							<local:ReverseConverter x:Key=""reverseConverter""/>
						</ResourceDictionary>
					</ContentPage.Resources>
					<ContentPage.Content>
						<cmp:StackLayout Orientation=""Vertical"">
							<cmp:StackLayout.Children>
								<Label x:Name=""label0"" Text=""{Binding Text, Converter={StaticResource reverseConverter}}""/>
								<Label x:Name=""label1"" Text=""{Binding Text, Mode=TwoWay}""/>
							</cmp:StackLayout.Children>
						</cmp:StackLayout>
					</ContentPage.Content>
				</ContentPage>";

			var contentPage = new ContentPage();
			contentPage.LoadFromXaml(xaml);
			contentPage.BindingContext = new ViewModel { Text = "foobar" };
			var label0 = contentPage.FindByName<Label>("label0");
			var label1 = contentPage.FindByName<Label>("label1");
			Assert.Equal("raboof", label0.Text);

			label1.Text = "baz";
			Assert.Equal("baz", ((ViewModel)(contentPage.BindingContext)).Text);
		}

		[Fact]
		public void TestNonEmptyCollectionMembers()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<StackLayout.Children>
						<Grid x:Name=""grid0"">
						</Grid>
						<Grid x:Name=""grid1"">
						</Grid>
					</StackLayout.Children>
				</StackLayout>";

			var stacklayout = new StackLayout();
			stacklayout.LoadFromXaml(xaml);
			var grid0 = stacklayout.FindByName<Grid>("grid0");
			var grid1 = stacklayout.FindByName<Grid>("grid1");
			Assert.NotNull(grid0);
			Assert.NotNull(grid1);
		}

		[Fact]
		public void TestUnknownType()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<StackLayout.Children>
						<CustomView />
					</StackLayout.Children>
				</StackLayout>";

			var stacklayout = new StackLayout();
			XamlParseExceptionHelper.AssertThrows(() => stacklayout.LoadFromXaml(xaml), 6, 8);
		}

		[Fact]
		public void TestResources()
		{
			var xaml = @"
				<Label 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"">
					<Label.Resources>
						<ResourceDictionary>
							<local:ReverseConverter x:Key=""reverseConverter""/>
						</ResourceDictionary>
					</Label.Resources>
				</Label>";

			var label = new Label().LoadFromXaml(xaml);
			Assert.NotNull(label.Resources);
			Assert.True(label.Resources.ContainsKey("reverseConverter"));
			Assert.True(label.Resources["reverseConverter"] is ReverseConverter);
		}

		[Fact]
		public void TestResourceDoesRequireKey()
		{
			var xaml = @"
				<Label 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"">
					<Label.Resources>
						<ResourceDictionary>
							<local:ReverseConverter />
						</ResourceDictionary>
					</Label.Resources>
				</Label>";
			var label = new Label();
			XamlParseExceptionHelper.AssertThrows(() => label.LoadFromXaml(xaml), 8, 9);
		}

		[Fact]
		public void UseResourcesOutsideOfBinding()
		{
			var xaml = @"
				<ContentView
				  xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				  xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
				  <ContentView.Resources>
                    <ResourceDictionary>
                      <Label Text=""Foo"" x:Key=""bar""/>
                    </ResourceDictionary>
				  </ContentView.Resources>
				<ContentView.Content>
                  <ContentView Content=""{StaticResource bar}""/>
				</ContentView.Content>
                </ContentView>";

			var contentView = new ContentView().LoadFromXaml(xaml);
			Assert.Equal("Foo", (((ContentView)(contentView.Content)).Content as Label).Text);
		}

		[Fact]
		public void MissingStaticResourceShouldThrow()
		{
			var xaml = @"<Label xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" Text=""{StaticResource foo}""/>";
			var label = new Label();
			XamlParseExceptionHelper.AssertThrows(() => label.LoadFromXaml(xaml), 1, 62);
		}

		public class CustView : Button
		{
			public bool fired = false;
			public void onButtonClicked(object sender, EventArgs e)
			{
				fired = true;
			}

			public void wrongSignature(bool a, string b)
			{
			}
		}

		class MyApp : Application
		{
			public MyApp()
			{
				Resources = new ResourceDictionary {
					{"foo", "FOO"},
					{"bar", "BAR"}
				};
			}
		}

		[Fact]
		public void StaticResourceLookForApplicationResources()
		{
			Application.Current = null;
			Application.Current = new MyApp();
			var xaml = @"
				<ContentView
				  xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				  xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
				  <ContentView.Resources>
                    <ResourceDictionary>
                      <x:String x:Key=""bar"">BAZ</x:String>
                    </ResourceDictionary>
				  </ContentView.Resources>
				  <StackLayout>
				    <Label x:Name=""label0"" Text=""{StaticResource foo}""/>
				    <Label x:Name=""label1"" Text=""{StaticResource bar}""/>
				  </StackLayout>
                </ContentView>";
			var layout = new ContentView().LoadFromXaml(xaml);
			var label0 = layout.FindByName<Label>("label0");
			var label1 = layout.FindByName<Label>("label1");

			//resource from App.Resources
			Assert.Equal("FOO", label0.Text);

			//local resources have precedence
			Assert.Equal("BAZ", label1.Text);
		}

		[Fact]
		public void TestEvent()
		{
			var xaml = @"
				<Button 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.CustView"" Clicked=""onButtonClicked"" />
				</Button>";
			var view = new CustView();
			view.LoadFromXaml(xaml);
			Assert.False(view.fired);
			((IButtonController)view).SendClicked();
			Assert.True(view.fired);
		}

		[Fact]
		public void TestFailingEvent()
		{
			var xaml = @"
				<View 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.CustView"" Clicked=""missingMethod"" />
				</View>";
			var view = new CustView();
			XamlParseExceptionHelper.AssertThrows(() => view.LoadFromXaml(xaml), 5, 63);
		}

		[Fact]
		public void TestConnectingEventOnMethodWithWrongSignature()
		{
			var xaml = @"
				<View 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.CustView"" Clicked=""wrongSignature"" />
				</View>";
			var view = new CustView();

			XamlParseExceptionHelper.AssertThrows(() => view.LoadFromXaml(xaml), 5, 63);
		}


		public class CustEntry : Entry
		{
			public bool fired = false;
			public void onValueChanged(object sender, TextChangedEventArgs e)
			{
				fired = true;
			}

		}

		[Fact]
		public void TestEventWithCustomEventArgs()
		{
			var xaml = @"
			<Entry
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.CustEntry"" TextChanged=""onValueChanged"" />
			</Entry>";
			new CustEntry().LoadFromXaml(xaml);
		}

		[Fact]
		public void TestEmptyTemplate()
		{
			var xaml = @"
			<ContentPage
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
				<ContentPage.Resources>
					<ResourceDictionary>
						<DataTemplate x:Key=""datatemplate""/>
					</ResourceDictionary>
				</ContentPage.Resources>
			</ContentPage>";
			var page = new ContentPage();
			page.LoadFromXaml(xaml);
			var template = page.Resources["datatemplate"] as Maui.Controls.DataTemplate;

			Assert.NotNull(template.CreateContent());
		}

		[Fact]
		public void TestBoolValue()
		{
			var xaml = @"
				<Image 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				IsOpaque=""true""/>";

			var image = new Image();
			Assert.Equal(Image.IsOpaqueProperty.DefaultValue, image.IsOpaque);
			image.LoadFromXaml(xaml);
			Assert.True(image.IsOpaque);
		}

		[Fact]
		public void TestAttachedBP()
		{
			var xaml = @"
				<View 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				Grid.Column=""1"">
					<Grid.Row>2</Grid.Row>
				</View>";
			var view = new View().LoadFromXaml(xaml);
			Assert.Equal(1, Grid.GetColumn(view));
			Assert.Equal(2, Grid.GetRow(view));
		}

		[Fact]
		public void TestAttachedBPWithDifferentNS()
		{
			//If this looks very similar to Vernacular, well... it's on purpose :)
			var xaml = @"
				<Label
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"" 
				local:Catalog.Message=""foobar""/>";
			var label = new Label().LoadFromXaml(xaml);
			Assert.Equal("raboof", label.Text);
		}

		[Fact]
		public void TestBindOnAttachedBP()
		{
			var xaml = @"
				<Label
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"" 
				local:Catalog.Message=""{Binding .}""/>";
			var label = new Label().LoadFromXaml(xaml);
			label.BindingContext = "foobar";
			Assert.Equal("raboof", label.Text);
		}

		[Fact]
		public void TestContentProperties()
		{
			var xaml = @"
				<local:CustomView
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"" >
					<Label x:Name=""contentview""/>
				</local:CustomView>";
			CustomView customView = null;
			// TODO: xUnit does not have DoesNotThrow - verify this test manually`r`n() => customView = new CustomView().LoadFromXaml(xaml);
			Assert.NotNull(customView.Content);
			Assert.Same(customView.Content, ((Maui.Controls.Internals.INameScope)customView).FindByName("contentview"));
		}

		[Fact]
		public void TestCollectionContentProperties()
		{
			var xaml = @"
				<StackLayout xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"">
					<Label Text=""Foo""/>
					<Label Text=""Bar""/>
				</StackLayout>";
			var layout = new StackLayout().LoadFromXaml(xaml);
			Assert.Equal(2, layout.Children.Count);
			Assert.Equal("Foo", ((Label)(layout.Children[0])).Text);
			Assert.Equal("Bar", ((Label)(layout.Children[1])).Text);
		}

		[Fact]
		public void TestCollectionContentPropertiesWithSingleElement()
		{
			var xaml = @"
				<StackLayout xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"">
					<Label Text=""Foo""/>
				</StackLayout>";
			var layout = new StackLayout().LoadFromXaml(xaml);
			Assert.Single(layout.Children);
			Assert.Equal("Foo", ((Label)(layout.Children[0])).Text);
		}

		[Fact]
		public void TestPropertiesWithContentProperties()
		{
			var xaml = @"
				<ContentPage
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
				<Grid.Row>1</Grid.Row>
				<Label Text=""foo""></Label>
				</ContentPage>";
			var contentPage = new ContentPage().LoadFromXaml(xaml);
			Assert.Equal(1, Grid.GetRow(contentPage));
			Assert.NotNull(contentPage.Content);
		}

		[Fact]
		public void LoadFromXamlResource()
		{
			ContentView view = null;
			view = new CustomXamlView();
			Assert.NotNull(view);
			Assert.IsType<Label>(view.Content);
			Assert.Equal("foobar", ((Label)view.Content).Text);
		}

		[Fact]
		public void ThrowOnMissingXamlResource()
		{
			var view = new CustomView();
			Assert.ThrowsAny<Exception>(() => view.LoadFromXaml(typeof(CustomView))); // TODO: Was using XamlParseExceptionConstraint
		}

		[Fact]
		public void CreateNewChildrenCollection()
		{
			var xaml = @"
				<local:ViewWithChildrenContent
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"" >
					<local:ViewWithChildrenContent.Children>
						<local:ViewList>
							<Label x:Name=""child0""/>
							<Label x:Name=""child1""/>
						</local:ViewList>
					</local:ViewWithChildrenContent.Children>
				</local:ViewWithChildrenContent>";
			ViewWithChildrenContent layout = null;
			// TODO: xUnit does not have DoesNotThrow - verify this test manually`r`n() => layout = new ViewWithChildrenContent().LoadFromXaml(xaml);
			Assert.NotNull(layout);
			Assert.NotSame(layout.DefaultChildren, layout.Children);
			Assert.Contains(((Maui.Controls.Internals.INameScope)layout).FindByName("child0"), layout.Children);
			Assert.Contains(((Maui.Controls.Internals.INameScope)layout).FindByName("child1"), layout.Children);
		}

		[Fact]
		public void AddChildrenToCollectionContentProperty()
		{
			var xaml = @"
				<local:ViewWithChildrenContent
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"" >
					<Label x:Name=""child0""/>
					<Label x:Name=""child1""/>
				</local:ViewWithChildrenContent>";
			ViewWithChildrenContent layout = null;
			// TODO: xUnit does not have DoesNotThrow - verify this test manually`r`n() => layout = new ViewWithChildrenContent().LoadFromXaml(xaml);
			Assert.NotNull(layout);
			Assert.Same(layout.DefaultChildren, layout.Children);
			Assert.Contains(((Maui.Controls.Internals.INameScope)layout).FindByName("child0"), layout.Children);
			Assert.Contains(((Maui.Controls.Internals.INameScope)layout).FindByName("child1"), layout.Children);
		}

		[Fact]
		public void AddChildrenToExistingCollection()
		{
			var xaml = @"
				<local:ViewWithChildrenContent
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"" >
					<local:ViewWithChildrenContent.Children>
						<Label x:Name=""child0""/>
						<Label x:Name=""child1""/>
					</local:ViewWithChildrenContent.Children>
				</local:ViewWithChildrenContent>";
			ViewWithChildrenContent layout = null;
			// TODO: xUnit does not have DoesNotThrow - verify this test manually`r`n() => layout = new ViewWithChildrenContent().LoadFromXaml(xaml);
			Assert.NotNull(layout);
			Assert.Same(layout.DefaultChildren, layout.Children);
			Assert.Contains(((Maui.Controls.Internals.INameScope)layout).FindByName("child0"), layout.Children);
			Assert.Contains(((Maui.Controls.Internals.INameScope)layout).FindByName("child1"), layout.Children);

		}

		[Fact]
		public void AddSingleChildToCollectionContentProperty()
		{
			var xaml = @"
				<local:ViewWithChildrenContent
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"" >
					<Label x:Name=""child0""/>
				</local:ViewWithChildrenContent>";
			ViewWithChildrenContent layout = null;
			// TODO: xUnit does not have DoesNotThrow - verify this test manually`r`n() => layout = new ViewWithChildrenContent().LoadFromXaml(xaml);
			Assert.NotNull(layout);
			Assert.Same(layout.DefaultChildren, layout.Children);
			Assert.Contains(((Maui.Controls.Internals.INameScope)layout).FindByName("child0"), layout.Children);
		}

		[Fact]
		public void FindResourceByName()
		{
			var xaml = @"
				<ContentPage
				    xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" 
				    x:Class=""Resources"">
				    <ContentPage.Resources>
						<ResourceDictionary>
						    <Button x:Key=""buttonKey"" x:Name=""buttonName""/>
						</ResourceDictionary>
				    </ContentPage.Resources>
				    <Label x:Name=""label""/>
				</ContentPage>";

			var layout = new ContentPage().LoadFromXaml(xaml);
			Assert.True(layout.Resources.ContainsKey("buttonKey"));
			var resource = layout.FindByName<Button>("buttonName");
			Assert.NotNull(resource);
			Assert.IsType<Button>(resource);
		}

		[Fact]
		public void ParseEnum()
		{
			var xaml = @"
				<local:CustomView
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"" 
				MockFlags=""Bar""
				/>";
			var view = new CustomView().LoadFromXaml(xaml);
			Assert.Equal(MockFlags.Bar, view.MockFlags);

		}

		[Fact]
		public void ParseFlags()
		{
			var xaml = @"
				<local:CustomView
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"" 
				MockFlags=""Baz,Bar""
				/>";
			var view = new CustomView().LoadFromXaml(xaml);
			Assert.Equal(MockFlags.Bar | MockFlags.Baz, view.MockFlags);
		}

		[Fact]
		public void StyleWithoutTargetTypeThrows()
		{
			var xaml = @"
				<Label xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"">
					<Label.Style>
						<Style>
							<Setter Property=""Text"" Value=""Foo"" />
						</Style>
					</Label.Style>
				</Label>";
			var label = new Label();
			XamlParseExceptionHelper.AssertThrows(() => label.LoadFromXaml(xaml), 4, 8);
		}

		[Fact]
		public void BindingIsResolvedAsBindingExtension()
		// https://github.com/xamarin/Microsoft.Maui.Controls/issues/3606#issuecomment-422377338
		{
			var bindingType = XamlParser.GetElementType(new XmlType("http://schemas.microsoft.com/dotnet/2021/maui", "Binding", null), null, null, true, out var ex);
			Assert.Null(ex);
			Assert.Equal(typeof(BindingExtension), bindingType);
			var module = ModuleDefinition.CreateModule("foo", new ModuleParameters()
			{
				AssemblyResolver = new MockAssemblyResolver(),
				Kind = ModuleKind.Dll,
			});
			var bindingTypeRef = new XmlType("http://schemas.microsoft.com/dotnet/2021/maui", "Binding", null).GetTypeReference(new XamlCache(), module, null);
			Assert.Equal("Microsoft.Maui.Controls.Xaml.BindingExtension", bindingType.FullName);
		}
	}
}
