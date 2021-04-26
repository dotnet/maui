using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class BasePage : ContentPage
	{

	}

	[TestFixture]
	public class TestCases
	{
		CultureInfo _defaultCulture;

		[SetUp]
		public virtual void Setup()
		{
			_defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public virtual void TearDown()
		{
			Device.PlatformServices = null;
			System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
		}

		public static readonly BindableProperty InnerViewProperty = BindableProperty.CreateAttached("InnerView", typeof(View), typeof(TestCases), default(View));

		public static View GetInnerView(BindableObject bindable)
		{
			return (View)bindable.GetValue(InnerViewProperty);
		}

		public static void SetInnerView(BindableObject bindable, View value)
		{
			bindable.SetValue(InnerViewProperty, value);
		}

		[Test]
		public void TestCase001()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
			<ContentPage
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
			xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests""
			Title=""Home"">
				<local:TestCases.InnerView>
					<Label x:Name=""innerView""/>
				</local:TestCases.InnerView>
				<ContentPage.Content>
					<Grid RowSpacing=""9"" ColumnSpacing=""6"" Padding=""6,9"" VerticalOptions=""Fill"" HorizontalOptions=""Fill"" BackgroundColor=""Red"">
						<Grid.Children>
							<Label x:Name=""label0""/>
							<Label x:Name=""label1""/>
							<Label x:Name=""label2""/>
							<Label x:Name=""label3""/>
						</Grid.Children>
					</Grid>
				</ContentPage.Content>
			</ContentPage>";
			var contentPage = new ContentPage().LoadFromXaml(xaml);
			var label0 = contentPage.FindByName<Label>("label0");
			var label1 = contentPage.FindByName<Label>("label1");

			Assert.NotNull(GetInnerView(contentPage));
			//			Assert.AreEqual ("innerView", GetInnerView (contentPage).Name);
			Assert.AreEqual(GetInnerView(contentPage), ((Microsoft.Maui.Controls.Internals.INameScope)contentPage).FindByName("innerView"));
			Assert.NotNull(label0);
			Assert.NotNull(label1);
			Assert.AreEqual(4, contentPage.Content.Descendants().Count());
		}


		[Test]
		public void TestCase002()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
            <local:BasePage
                xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
                xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
			    xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests""
                x:Class=""Tramchester.App.Views.HomeView"">
                <local:BasePage.Content>
                  <Label Text=""Hi There!"" />
                </local:BasePage.Content>
           </local:BasePage>";
			var contentPage = new ContentPage().LoadFromXaml(xaml);
			Assert.That(contentPage.Content, Is.InstanceOf<Label>());
		}

		[Test]
		public void TestCase003()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<ContentPage
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
					Title=""People"">

					<StackLayout Spacing=""0"">
						<SearchBar x:Name=""searchBar""/>
						<ListView ItemsSource=""{Binding Path=.}"" RowHeight=""42"" x:Name=""listview"">
							<ListView.ItemTemplate>
								<DataTemplate>
									<ViewCell>
										<ViewCell.View>
											<StackLayout Orientation=""Horizontal"" HorizontalOptions=""FillAndExpand"" VerticalOptions=""CenterAndExpand"" BackgroundColor=""#fff4f4f4"">
												<BoxView WidthRequest=""10""/>
												<Grid WidthRequest=""42"" HeightRequest=""32"" VerticalOptions=""CenterAndExpand"" HorizontalOptions=""Start"">
													<Image WidthRequest=""32"" HeightRequest=""32"" Aspect=""AspectFill"" HorizontalOptions=""FillAndExpand"" Source=""Images/icone_nopic_members_42.png""/>
													<!--<Image WidthRequest=""32"" HeightRequest=""32"" Aspect=""AspectFill"" HorizontalOptions=""FillAndExpand"">
														<Image.Source>
															<UriImageSource Uri=""{Binding Picture}"" CacheValidity=""30""/>
														</Image.Source>
													</Image>-->
													<Image Source=""Images/cropcircle.png"" HorizontalOptions=""FillAndExpand"" VerticalOptions=""FillAndExpand"" WidthRequest=""32"" HeightRequest=""32"" Aspect=""Fill""/>
												</Grid>
												<Label Text=""{Binding FirstName}"" VerticalOptions=""CenterAndExpand""/>
												<Label Text=""{Binding LastName}"" FontFamily=""HelveticaNeue-Bold"" FontSize=""Medium"" VerticalOptions=""CenterAndExpand"" />
											</StackLayout>
										</ViewCell.View>
									</ViewCell>
								</DataTemplate>
							</ListView.ItemTemplate>
						</ListView>
					</StackLayout>
				</ContentPage>";
			var page = new ContentPage().LoadFromXaml(xaml);
			var model = new List<object> {
				new {FirstName = "John", LastName="Lennon", Picture="http://www.johnlennon.com/wp-content/themes/jl/images/home-gallery/2.jpg"},
				new {FirstName = "Paul", LastName="McCartney", Picture="http://t0.gstatic.com/images?q=tbn:ANd9GcRjNUGJ00Mt85n2XDu8CZM0w1em0Wv4ZaemLuIVmLCMwPMOLUO1SQ"},
				new {FirstName = "George", LastName="Harisson", Picture="http://cdn.riffraf.net/wp-content/uploads/2013/02/george-harrison-living.jpg"},
				new {FirstName = "Ringo", LastName="Starr", Picture="http://www.biography.com/imported/images/Biography/Images/Profiles/S/Ringo-Starr-306872-1-402.jpg"},
			};
			page.BindingContext = model;

			var listview = page.FindByName<ListView>("listview");
			Cell cell = null;
			Assert.DoesNotThrow(() => { cell = listview.TemplatedItems[0]; });
			Assert.NotNull(cell);
			Assert.That(cell, Is.TypeOf<ViewCell>());
			Assert.AreSame(model[0], cell.BindingContext);
		}

		[Test]
		public void TestCase004()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""UTF-8"" ?>
				<ContentPage
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
					<ContentPage.Content>
					    <ScrollView Orientation=""Horizontal"">
					        <ScrollView.Content>
					            <Grid>
					                <Grid.ColumnDefinitions>
					                    <ColumnDefinition />
					                    <ColumnDefinition />
					                </Grid.ColumnDefinitions>
					                <Image Grid.Column=""0"" Grid.Row=""0"" Aspect=""AspectFill"">
					                    <Image.Source>
					                        <StreamImageSource Stream=""{Binding HeroPicture.Stream}"" />
					                    </Image.Source>
					                </Image>
					            </Grid>
					        </ScrollView.Content>
					    </ScrollView>
					</ContentPage.Content>
				</ContentPage>";

			var page = new ContentPage();
			Assert.DoesNotThrow(() => page.LoadFromXaml(xaml));
		}

		[Test]
		public void Issue1415()
		{
			var xaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
						<ContentPage 
							xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
							xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"">
							<Label x:Name=""label"" Text=""{Binding Converter={x:Static local:ReverseConverter.Instance}, Mode=TwoWay}""/>
						</ContentPage>";
			var page = new ContentPage().LoadFromXaml(xaml);
			var label = page.FindByName<Label>("label");
			Assert.NotNull(label);
			label.BindingContext = "foo";
			Assert.AreEqual("oof", label.Text);
		}

		[TestCase("en-US"), TestCase("tr-TR"), TestCase("fr-FR")]
		//only happens in european cultures
		public void Issue1493(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);

			var xaml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
						<View 
							xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
							xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							RelativeLayout.HeightConstraint=""{ConstraintExpression Type=RelativeToParent, Property=Height, Factor=0.25}""
							RelativeLayout.WidthConstraint=""{ConstraintExpression Type=RelativeToParent, Property=Width, Factor=0.6}""/>";
			View view = new View();
			view.LoadFromXaml(xaml);
			Assert.DoesNotThrow(() => view.LoadFromXaml(xaml));
		}
	}
}
