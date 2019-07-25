using NUnit.Framework;
using System.Linq;
using System;
using System.Collections.Generic;

using Xamarin.Forms.Core.UnitTests;
using Xamarin.Forms.Build.Tasks;
using Mono.Cecil;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[ContentProperty ("Content")]
	public class CustomView : View
	{
		public string NotBindable { get; set; }

		public View Content { get; set; }

		public MockFlags MockFlags { get; set; }
	}

	[ContentProperty ("Children")]
	public class ViewWithChildrenContent : View
	{
		public ViewWithChildrenContent ()
		{
			Children = DefaultChildren = new ViewList ();
		}
		public ViewList DefaultChildren;
		public ViewList Children { get; set; }
	}

	public class ViewList : List<View>
	{

	}

	public class ReverseConverter : IValueConverter
	{
		public static ReverseConverter Instance = new ReverseConverter ();

		public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var s = value as string;
			if (s == null)
				return value;
			return new string (s.Reverse ().ToArray ());
		}

		public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var s = value as string;
			if (s == null)
				return value;
			return new string (s.Reverse ().ToArray ());
		}
	}

	public class Catalog
	{
		public static readonly BindableProperty MessageProperty = 
#pragma warning disable 618
			BindableProperty.CreateAttached<Catalog, string> (bindable => GetMessage (bindable), default(string), 
#pragma warning restore 618
				propertyChanged: (bindable, oldvalue, newvalue) => {
					var label = bindable as Label;
					if (label != null)
						label.SetValue (Label.TextProperty, new string (newvalue.Reverse ().ToArray ()));
				});

		public static string GetMessage (BindableObject bindable)
		{
			return (string)bindable.GetValue (MessageProperty);
		}

		public static void SetMessage (BindableObject bindable, string value)
		{
			bindable.SetValue (MessageProperty, value);
		}
	}
	
	[Flags]
	public enum MockFlags
	{
		Foo = 1<<0,
		Bar = 1<<1,
		Baz = 1<<2,
		Qux = 1<<3,
	}


	[TestFixture]
	public class LoaderTests : BaseTestFixture
	{
		[Test]
		public void TestRootName ()
		{
			var xaml = @"
				<View
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Xamarin.Forms.Xaml.UnitTests.CustomView"" 
				x:Name=""customView"" 
				/>";

			var view = new CustomView ();
			view.LoadFromXaml (xaml);

			Assert.AreSame (view, ((Forms.Internals.INameScope)view).FindByName("customView"));
		}

		[Test]
		public void TestFindByXName ()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<StackLayout.Children>
						<Label x:Name=""label0"" Text=""Foo""/>
					</StackLayout.Children>
				</StackLayout>";

			var stacklayout = new StackLayout ();
			stacklayout.LoadFromXaml (xaml);

			var label = stacklayout.FindByName<Label> ("label0");
			Assert.NotNull (label);
			Assert.AreEqual ("Foo", label.Text);		
		}

		[Test]
		public void TestUnknownPropertyShouldThrow ()
		{
			var xaml = @"
				<Label 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				Text=""Foo""
				UnknownProperty=""Bar""
			    />";

			var label = new Label ();
			Assert.Throws (new XamlParseExceptionConstraint (5, 5), () => label.LoadFromXaml (xaml));
		}

		[Test]
		public void TestSetValueToBindableProperty ()
		{
			var xaml = @"
			<Label 
			xmlns=""http://xamarin.com/schemas/2014/forms""
			Text=""Foo""
			/>";

			var label = new Label ();

			label.LoadFromXaml (xaml);
			Assert.AreEqual ("Foo", label.Text);

		}

		[Test]
		public void TestSetBindingToBindableProperty ()
		{
			var xaml = @"
			<Label 
			xmlns=""http://xamarin.com/schemas/2014/forms""
			Text=""{Binding Path=labeltext}""
			/>";

			var label = new Label ();
			label.LoadFromXaml (xaml);

			Assert.AreEqual (Label.TextProperty.DefaultValue, label.Text);

			label.BindingContext = new {labeltext="Foo"};
			Assert.AreEqual ("Foo", label.Text);
		}

		[Test]
		public void TestSetBindingToNonBindablePropertyShouldThrow ()
		{
			var xaml = @"
				<View 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Xamarin.Forms.Xaml.UnitTests.CustomView"" 
				Name=""customView"" 
				NotBindable=""{Binding text}""
				/>";

			var view = new CustomView ();
			Assert.Throws (new XamlParseExceptionConstraint (6, 5), () => view.LoadFromXaml (xaml));
		}

		[Test]
		public void TestBindingPath ()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<StackLayout.Children>
						<Label x:Name=""label0"" Text=""{Binding text}""/>
						<Label x:Name=""label1"" Text=""{Binding Path=text}""/>
					</StackLayout.Children>
				</StackLayout>";

			var stacklayout = new StackLayout ();
			stacklayout.LoadFromXaml (xaml);

			var label0 = stacklayout.FindByName<Label> ("label0");
			var label1 = stacklayout.FindByName<Label> ("label1");

			Assert.AreEqual (Label.TextProperty.DefaultValue, label0.Text);
			Assert.AreEqual (Label.TextProperty.DefaultValue, label1.Text);

			stacklayout.BindingContext = new {text = "Foo"};
			Assert.AreEqual ("Foo", label0.Text);
			Assert.AreEqual ("Foo", label1.Text);
		}


		class ViewModel
		{
			public string Text { get; set; }
		}

		[Test]
		public void TestBindingModeAndConverter ()
		{
			var xaml = @"
				<ContentPage 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"">
					<ContentPage.Resources>
						<ResourceDictionary>
							<local:ReverseConverter x:Key=""reverseConverter""/>
						</ResourceDictionary>
					</ContentPage.Resources>
					<ContentPage.Content>
						<StackLayout Orientation=""Vertical"">
							<StackLayout.Children>
								<Label x:Name=""label0"" Text=""{Binding Text, Converter={StaticResource reverseConverter}}""/>
								<Label x:Name=""label1"" Text=""{Binding Text, Mode=TwoWay}""/>
							</StackLayout.Children>
						</StackLayout>
					</ContentPage.Content>
				</ContentPage>";

			var contentPage = new ContentPage ();
			contentPage.LoadFromXaml (xaml);
			contentPage.BindingContext = new ViewModel { Text = "foobar" };
			var label0 = contentPage.FindByName<Label> ("label0");
			var label1 = contentPage.FindByName<Label> ("label1");
			Assert.AreEqual ("raboof", label0.Text);
	
			label1.Text = "baz";
			Assert.AreEqual ("baz", ((ViewModel)(contentPage.BindingContext)).Text);
		}

		[Test]
		public void TestNonEmptyCollectionMembers ()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<StackLayout.Children>
						<Grid x:Name=""grid0"">
						</Grid>
						<Grid x:Name=""grid1"">
						</Grid>
					</StackLayout.Children>
				</StackLayout>";

			var stacklayout = new StackLayout ();
			stacklayout.LoadFromXaml (xaml);
			var grid0 = stacklayout.FindByName<Grid> ("grid0");
			var grid1 = stacklayout.FindByName<Grid> ("grid1");
			Assert.NotNull (grid0);
			Assert.NotNull (grid1);
		}

		[Test]
		public void TestUnknownType ()
		{
			var xaml = @"
				<StackLayout 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<StackLayout.Children>
						<CustomView />
					</StackLayout.Children>
				</StackLayout>";

			var stacklayout = new StackLayout ();
			Assert.Throws (new XamlParseExceptionConstraint (6, 8), () => stacklayout.LoadFromXaml (xaml));
		}

		[Test]
		public void TestResources ()
		{
			var xaml = @"
				<Label 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"">
					<Label.Resources>
						<ResourceDictionary>
							<local:ReverseConverter x:Key=""reverseConverter""/>
						</ResourceDictionary>
					</Label.Resources>
				</Label>";

			var label = new Label ().LoadFromXaml (xaml);
			Assert.NotNull (label.Resources);
			Assert.True (label.Resources.ContainsKey ("reverseConverter"));
			Assert.True (label.Resources ["reverseConverter"] is ReverseConverter);
		}

		[Test]
		public void TestResourceDoesRequireKey ()
		{
			var xaml = @"
				<Label 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"">
					<Label.Resources>
						<ResourceDictionary>
							<local:ReverseConverter />
						</ResourceDictionary>
					</Label.Resources>
				</Label>";
			var label = new Label ();
			Assert.Throws (new XamlParseExceptionConstraint (8, 9), () => label.LoadFromXaml (xaml));
		}

		[Test]
		public void UseResourcesOutsideOfBinding ()
		{
			var xaml = @"
				<ContentView
				  xmlns=""http://xamarin.com/schemas/2014/forms""
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

			var contentView = new ContentView ().LoadFromXaml (xaml);
			Assert.AreEqual ("Foo", (((ContentView)(contentView.Content)).Content as Label).Text);
		}

		[Test]
		public void MissingStaticResourceShouldThrow ()
		{
			var xaml = @"<Label xmlns=""http://xamarin.com/schemas/2014/forms"" Text=""{StaticResource foo}""/>";
			var label = new Label ();
			Assert.Throws (new XamlParseExceptionConstraint (1, 54), () => label.LoadFromXaml (xaml));
		}

		public class CustView : Button
		{
			public bool fired = false;
			public void onButtonClicked (object sender, EventArgs e)
			{
				fired = true;
			}

			public void wrongSignature (bool a, string b)
			{
			}
		}

		class MyApp : Application 
		{
			public MyApp ()
			{
				Resources = new ResourceDictionary {
					{"foo", "FOO"},
					{"bar", "BAR"}
				};
			}
		}

		[Test]
		public void StaticResourceLookForApplicationResources ()
		{
			Device.PlatformServices = new MockPlatformServices ();
			Application.Current = null;

			Application.Current = new MyApp ();
			var xaml = @"
				<ContentView
				  xmlns=""http://xamarin.com/schemas/2014/forms""
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
			var layout = new ContentView ().LoadFromXaml (xaml);
			var label0 = layout.FindByName<Label> ("label0");
			var label1 = layout.FindByName<Label> ("label1");

			//resource from App.Resources
			Assert.AreEqual ("FOO", label0.Text);

			//local resources have precedence
			Assert.AreEqual ("BAZ", label1.Text);
		}

		[Test]
		public void TestEvent ()
		{
			var xaml = @"
				<Button 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Xamarin.Forms.Xaml.UnitTests.CustView"" Clicked=""onButtonClicked"" />
				</Button>";
			var view = new CustView ();
			view.LoadFromXaml (xaml);
			Assert.False (view.fired);
			((IButtonController) view).SendClicked ();
			Assert.True (view.fired);
		}

		[Test]
		public void TestFailingEvent ()
		{
			var xaml = @"
				<View 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Xamarin.Forms.Xaml.UnitTests.CustView"" Activated=""missingMethod"" />
				</View>";
			var view = new CustView ();
			Assert.Throws (new XamlParseExceptionConstraint (5, 53), () => view.LoadFromXaml (xaml));
		}

		[Test]
		public void TestConnectingEventOnMethodWithWrongSignature ()
		{
			var xaml = @"
				<View 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Xamarin.Forms.Xaml.UnitTests.CustView"" Activated=""wrongSignature"" />
				</View>";
			var view = new CustView ();

			Assert.Throws (new XamlParseExceptionConstraint (5, 53), () => view.LoadFromXaml (xaml));
		}


		public class CustEntry : Entry
		{
			public bool fired = false;
			public void onValueChanged (object sender, TextChangedEventArgs e)
			{
				fired = true;
			}

		}

		[Test]
		public void TestEventWithCustomEventArgs ()
		{
			var xaml = @"
			<Entry
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				x:Class=""Xamarin.Forms.Xaml.UnitTests.CustEntry"" TextChanged=""onValueChanged"" />
			</Entry>";
			new CustEntry ().LoadFromXaml (xaml);
		}

		[Test]
		public void TestEmptyTemplate ()
		{
			var xaml = @"
			<ContentPage
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
				<ContentPage.Resources>
					<ResourceDictionary>
						<DataTemplate x:Key=""datatemplate""/>
					</ResourceDictionary>
				</ContentPage.Resources>
			</ContentPage>";
			var page = new ContentPage ();
			page.LoadFromXaml (xaml);
			var template = page.Resources["datatemplate"]as Forms.DataTemplate;
			Assert.Throws<InvalidOperationException>(() => template.CreateContent());
		}

		[Test]
		public void TestBoolValue ()
		{
			var xaml = @"
				<Image 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				IsOpaque=""true""/>";

			var image = new Image ();
			Assert.AreEqual (Image.IsOpaqueProperty.DefaultValue, image.IsOpaque);
			image.LoadFromXaml (xaml);
			Assert.AreEqual (true, image.IsOpaque);
		}

		[Test]
		public void TestAttachedBP ()
		{
			var xaml = @"
				<View 
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				Grid.Column=""1"">
					<Grid.Row>2</Grid.Row>
				</View>";
			var view = new View ().LoadFromXaml (xaml);
			Assert.AreEqual (1, Grid.GetColumn (view));
			Assert.AreEqual (2, Grid.GetRow (view));
		}

		[Test]
		public void TestAttachedBPWithDifferentNS ()
		{
			//If this looks very similar to Vernacular, well... it's on purpose :)
			var xaml = @"
				<Label
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" 
				local:Catalog.Message=""foobar""/>";
			var label = new Label ().LoadFromXaml (xaml);
			Assert.AreEqual ("raboof", label.Text);
		}

		[Test]
		public void TestBindOnAttachedBP ()
		{
			var xaml = @"
				<Label
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" 
				local:Catalog.Message=""{Binding .}""/>";
			var label = new Label ().LoadFromXaml (xaml);
			label.BindingContext = "foobar";
			Assert.AreEqual ("raboof", label.Text);
		}

		[Test]
		public void TestContentProperties ()
		{
			var xaml = @"
				<local:CustomView
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" >
					<Label x:Name=""contentview""/>
				</local:CustomView>";
			CustomView customView = null;
			Assert.DoesNotThrow(()=> customView = new CustomView ().LoadFromXaml (xaml));
			Assert.NotNull (customView.Content);
			Assert.AreSame (customView.Content, ((Forms.Internals.INameScope)customView).FindByName("contentview"));
		}

		[Test]
		public void TestCollectionContentProperties ()
		{
			var xaml = @"
				<StackLayout xmlns=""http://xamarin.com/schemas/2014/forms"">
					<Label Text=""Foo""/>
					<Label Text=""Bar""/>
				</StackLayout>";
			var layout = new StackLayout ().LoadFromXaml (xaml);
			Assert.AreEqual (2, layout.Children.Count);
			Assert.AreEqual ("Foo", ((Label)(layout.Children [0])).Text);
			Assert.AreEqual ("Bar", ((Label)(layout.Children [1])).Text);
		}

		[Test]
		public void TestCollectionContentPropertiesWithSingleElement ()
		{
			var xaml = @"
				<StackLayout xmlns=""http://xamarin.com/schemas/2014/forms"">
					<Label Text=""Foo""/>
				</StackLayout>";
			var layout = new StackLayout ().LoadFromXaml (xaml);
			Assert.AreEqual (1, layout.Children.Count);
			Assert.AreEqual ("Foo", ((Label)(layout.Children [0])).Text);
		}

		[Test]
		public void TestPropertiesWithContentProperties ()
		{
			var xaml = @"
				<ContentPage
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
				<Grid.Row>1</Grid.Row>
				<Label Text=""foo""></Label>
				</ContentPage>";
			var contentPage = new ContentPage ().LoadFromXaml (xaml);
			Assert.AreEqual (1, Grid.GetRow (contentPage));
			Assert.NotNull (contentPage.Content);
		}

		[Test]
		public void LoadFromXamlResource ()
		{
			ContentView view = null;
			Assert.DoesNotThrow (() => view = new CustomXamlView ());
			Assert.NotNull (view);
			Assert.That (view.Content, Is.TypeOf<Label> ());
			Assert.AreEqual ("foobar", ((Label)view.Content).Text);
		}

		[Test]
		public void ThrowOnMissingXamlResource ()
		{
			var view = new CustomView ();
			Assert.Throws (new XamlParseExceptionConstraint (), () => view.LoadFromXaml (typeof(CustomView)));
		}

		[Test]
		public void CreateNewChildrenCollection ()
		{
			var xaml = @"
				<local:ViewWithChildrenContent
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" >
					<local:ViewWithChildrenContent.Children>
						<local:ViewList>
							<Label x:Name=""child0""/>
							<Label x:Name=""child1""/>
						</local:ViewList>
					</local:ViewWithChildrenContent.Children>
				</local:ViewWithChildrenContent>";
			ViewWithChildrenContent layout = null;
			Assert.DoesNotThrow (() => layout = new ViewWithChildrenContent ().LoadFromXaml (xaml));
			Assert.IsNotNull (layout);
			Assert.AreNotSame (layout.DefaultChildren, layout.Children);
			Assert.Contains (((Forms.Internals.INameScope)layout).FindByName ("child0"), layout.Children);
			Assert.Contains (((Forms.Internals.INameScope)layout).FindByName ("child1"), layout.Children);
		}

		[Test]
		public void AddChildrenToCollectionContentProperty ()
		{
			var xaml = @"
				<local:ViewWithChildrenContent
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" >
					<Label x:Name=""child0""/>
					<Label x:Name=""child1""/>
				</local:ViewWithChildrenContent>";
			ViewWithChildrenContent layout = null;
			Assert.DoesNotThrow (() => layout = new ViewWithChildrenContent ().LoadFromXaml (xaml));
			Assert.IsNotNull (layout);
			Assert.AreSame (layout.DefaultChildren, layout.Children);
			Assert.Contains (((Forms.Internals.INameScope)layout).FindByName ("child0"), layout.Children);
			Assert.Contains (((Forms.Internals.INameScope)layout).FindByName ("child1"), layout.Children);
		}

		[Test]
		public void AddChildrenToExistingCollection ()
		{
			var xaml = @"
				<local:ViewWithChildrenContent
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" >
					<local:ViewWithChildrenContent.Children>
						<Label x:Name=""child0""/>
						<Label x:Name=""child1""/>
					</local:ViewWithChildrenContent.Children>
				</local:ViewWithChildrenContent>";
			ViewWithChildrenContent layout = null;
			Assert.DoesNotThrow (() => layout = new ViewWithChildrenContent ().LoadFromXaml (xaml));
			Assert.IsNotNull (layout);
			Assert.AreSame (layout.DefaultChildren, layout.Children);
			Assert.Contains (((Forms.Internals.INameScope)layout).FindByName ("child0"), layout.Children);
			Assert.Contains (((Forms.Internals.INameScope)layout).FindByName ("child1"), layout.Children);

		}

		[Test]
		public void AddSingleChildToCollectionContentProperty ()
		{
			var xaml = @"
				<local:ViewWithChildrenContent
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" >
					<Label x:Name=""child0""/>
				</local:ViewWithChildrenContent>";
			ViewWithChildrenContent layout = null;
			Assert.DoesNotThrow (() => layout = new ViewWithChildrenContent ().LoadFromXaml (xaml));
			Assert.IsNotNull (layout);
			Assert.AreSame (layout.DefaultChildren, layout.Children);
			Assert.Contains (((Forms.Internals.INameScope)layout).FindByName ("child0"), layout.Children);
		}

		[Test]
		public void FindResourceByName ()
		{
			var xaml = @"
				<ContentPage
				    xmlns=""http://xamarin.com/schemas/2014/forms""
				    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" 
				    x:Class=""Resources"">
				    <ContentPage.Resources>
						<ResourceDictionary>
						    <Button x:Key=""buttonKey"" x:Name=""buttonName""/>
						</ResourceDictionary>
				    </ContentPage.Resources>
				    <Label x:Name=""label""/>
				</ContentPage>";

			var layout = new ContentPage ().LoadFromXaml (xaml);
			Assert.True (layout.Resources.ContainsKey ("buttonKey"));
			var resource = layout.FindByName<Button> ("buttonName");
			Assert.NotNull (resource);
			Assert.That (resource, Is.TypeOf<Button> ());
		}

		[Test]
		public void ParseEnum ()
		{
			var xaml = @"
				<local:CustomView
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" 
				MockFlags=""Bar""
				/>";
			var view = new CustomView ().LoadFromXaml (xaml);
			Assert.AreEqual (MockFlags.Bar, view.MockFlags);
					
		}

		[Test]
		public void ParseFlags ()
		{
			var xaml = @"
				<local:CustomView
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" 
				MockFlags=""Baz,Bar""
				/>";
			var view = new CustomView ().LoadFromXaml (xaml);
			Assert.AreEqual (MockFlags.Bar | MockFlags.Baz, view.MockFlags);
		}

		[Test]
		public void StyleWithoutTargetTypeThrows ()
		{
			var xaml = @"
				<Label xmlns=""http://xamarin.com/schemas/2014/forms"">
					<Label.Style>
						<Style>
							<Setter Property=""Text"" Value=""Foo"" />
						</Style>
					</Label.Style>
				</Label>";
			var label = new Label ();
			Assert.Throws (new XamlParseExceptionConstraint (4, 8), () => label.LoadFromXaml (xaml));
		}

		[Test]
		public void BindingIsResolvedAsBindingExtension()
		// https://github.com/xamarin/Xamarin.Forms/issues/3606#issuecomment-422377338
		{
			var bindingType = XamlParser.GetElementType(new XmlType("http://xamarin.com/schemas/2014/forms", "Binding", null), null, null, out var ex);
			Assert.That(ex, Is.Null);
			Assert.That(bindingType, Is.EqualTo(typeof(BindingExtension)));
			var module = ModuleDefinition.CreateModule("foo", new ModuleParameters()
			{
				AssemblyResolver = new MockAssemblyResolver(),
				Kind = ModuleKind.Dll,
			});
			var bindingTypeRef = new XmlType("http://xamarin.com/schemas/2014/forms", "Binding", null).GetTypeReference(module, null);
			Assert.That(bindingType.FullName, Is.EqualTo("Xamarin.Forms.Xaml.BindingExtension"));
		}
	}
}
