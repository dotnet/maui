using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class StyleTests : BaseTestFixture
	{

		public StyleTests()
		{

			ApplicationExtensions.CreateAndSetMockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.ClearCurrent();
			}
			base.Dispose(disposing);
		}

		[Fact]
		public void ApplyUnapplyStyle()
		{
			var style = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "foo" },
					new Setter { Property = VisualElement.BackgroundColorProperty, Value = Colors.Pink },
				}
			};

			var label = new Label
			{
				Style = style
			};
			Assert.Equal("foo", label.Text);
			Assert.Equal(Colors.Pink, label.BackgroundColor);

			label.Style = null;
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, label.BackgroundColor);
		}

		[Fact]
		public void BindingAndDynamicResourcesInStyle()
		{
			var style = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter { Property = VisualElement.IsEnabledProperty, Value = false }
				}
			};
			style.Setters.AddBinding(Label.TextProperty, new Binding("foo"));
			style.Setters.AddDynamicResource(VisualElement.BackgroundColorProperty, "qux");

			var label = new Label
			{
				Style = style
			};

			label.BindingContext = new { foo = "FOO" };
			Assert.Equal("FOO", label.Text);

			label.Resources = new ResourceDictionary {
				{"qux", Colors.Pink}
			};
			Assert.Equal(Colors.Pink, label.BackgroundColor);

			label.Style = null;
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, label.BackgroundColor);
		}

		[Fact]
		public void StyleCanBeAppliedMultipleTimes()
		{
			var style = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter{ Property = VisualElement.IsEnabledProperty, Value = false }
				}
			};
			style.Setters.AddBinding(Label.TextProperty, new Binding("foo"));
			style.Setters.AddDynamicResource(VisualElement.BackgroundColorProperty, "qux");

			var label0 = new Label
			{
				Style = style
			};
			var label1 = new Label
			{
				Style = style
			};

			label0.BindingContext = label1.BindingContext = new { foo = "FOO" };
			label0.Resources = label1.Resources = new ResourceDictionary {
				{"qux", Colors.Pink}
			};

			Assert.Equal("FOO", label0.Text);
			Assert.Equal("FOO", label1.Text);

			Assert.Equal(Colors.Pink, label0.BackgroundColor);
			Assert.Equal(Colors.Pink, label1.BackgroundColor);

			label0.Style = label1.Style = null;

			Assert.Equal(Label.TextProperty.DefaultValue, label0.Text);
			Assert.Equal(Label.TextProperty.DefaultValue, label1.Text);
			Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, label0.BackgroundColor);
			Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, label1.BackgroundColor);
		}

		[Fact]
		public void BaseStyleIsAppliedUnapplied()
		{
			var baseStyle = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "baseStyle" },
				}
			};
			var style = new Style(typeof(Label))
			{
				BasedOn = baseStyle,
			};

			var label = new Label
			{
				Style = style
			};
			Assert.Equal("baseStyle", label.Text);

			label.Style = null;
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
		}

		[Fact]
		public void StyleOverrideBaseStyle()
		{
			var baseStyle = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter{Property= Label.TextProperty,Value= "baseStyle" },
				}
			};
			var style = new Style(typeof(VisualElement))
			{
				BasedOn = baseStyle,
				Setters = {
					new Setter{Property= Label.TextProperty,Value= "style" },
				}
			};

			var label = new Label
			{
				Style = style
			};
			Assert.Equal("style", label.Text);

			label.Style = null;
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
		}

		[Fact]
		public void AddImplicitStyleToResourceDictionary()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = {
						new Setter { Property = Label.TextColorProperty, Value = Colors.Pink },
					}
				},
				{ "foo", "FOO" },
				{"labelStyle", new Style (typeof(Label)) { Setters = {
							new Setter { Property = Label.TextColorProperty, Value = Colors.Purple }
						}
					}
				}
			};

			Assert.Equal(3, rd.Count);
			Assert.Contains("Microsoft.Maui.Controls.Label", rd.Keys);
		}

		[Fact]
		public void ImplicitStylesAreAppliedOnSettingRD()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = {
						new Setter { Property = Label.TextColorProperty, Value = Colors.Pink },
					}
				},
				{ "foo", "FOO" },
				{"labelStyle", new Style (typeof(Label)) { Setters = {
							new Setter { Property = Label.TextColorProperty, Value = Colors.Purple }
						}
					}
				}
			};

			var label = new Label();
			var layout = new StackLayout { Children = { label } };

			Assert.Equal(label.TextColor, Label.TextColorProperty.DefaultValue);
			layout.Resources = rd;
			Assert.Equal(label.TextColor, Colors.Pink);
		}

		[Fact]
		public void ImplicitStylesAreAppliedOnSettingParrent()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = {
						new Setter { Property = Label.TextColorProperty, Value = Colors.Pink },
					}
				},
				{ "foo", "FOO" },
				{"labelStyle", new Style (typeof(Label)) { Setters = {
							new Setter { Property = Label.TextColorProperty, Value = Colors.Purple }
						}
					}
				}
			};

			var label = new Label();
			var layout = new StackLayout();
			layout.Resources = rd;

			Assert.Equal(label.TextColor, Label.TextColorProperty.DefaultValue);
			layout.Children.Add(label);
			Assert.Equal(label.TextColor, Colors.Pink);
		}

		[Fact]
		public void ImplicitStylesOverridenByStyle()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = { new Setter { Property = Label.TextColorProperty, Value = Colors.Pink }, } },
				{ "foo", "FOO" },
				{ "labelStyle", new Style (typeof(Label)) { Setters = { new Setter { Property = Label.TextColorProperty, Value = Colors.Purple } } } },
			};

			var label = new Label();

			var layout = new StackLayout { Children = { label }, Resources = rd };
			Assert.Equal(label.TextColor, Colors.Pink);

			label.SetDynamicResource(VisualElement.StyleProperty, "labelStyle");
			Assert.Equal(label.TextColor, Colors.Purple);
		}

		[Fact]
		public void UnsettingStyleReApplyImplicit()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = {
						new Setter { Property = Label.TextColorProperty, Value = Colors.Pink },
					}
				},
				{ "foo", "FOO" },
				{ "labelStyle", new Style (typeof(Label)) { Setters = {
							new Setter { Property = Label.TextColorProperty, Value = Colors.Purple }
						}
					}
				}
			};

			var label = new Label();
			label.SetDynamicResource(VisualElement.StyleProperty, "labelStyle");
			var layout = new StackLayout { Children = { label }, Resources = rd };

			Assert.Equal(label.TextColor, Colors.Purple);
			label.Style = null;
			Assert.Equal(label.TextColor, Colors.Pink);
		}

		[Fact]
		public void DynamicStyle()
		{
			var baseStyle0 = new Style(typeof(Label))
			{
				Setters = {
					new Setter {Property = Label.TextProperty, Value = "foo"},
					new Setter {Property = Label.TextColorProperty, Value = Colors.Pink}
				}
			};
			var baseStyle1 = new Style(typeof(Label))
			{
				Setters = {
					new Setter {Property = Label.TextProperty, Value = "bar"},
					new Setter {Property = Label.TextColorProperty, Value = Colors.Purple}
				}
			};
			var style = new Style(typeof(Label))
			{
				BaseResourceKey = "basestyle",
				Setters = {
					new Setter { Property = Label.BackgroundColorProperty, Value = Colors.Red },
					new Setter { Property = Label.TextColorProperty, Value = Colors.Red },
				}
			};

			var label0 = new Label
			{
				Style = style
			};

			Assert.Equal(Colors.Red, label0.BackgroundColor);
			Assert.Equal(Colors.Red, label0.TextColor);
			Assert.Equal(Label.TextProperty.DefaultValue, label0.Text);

			var layout0 = new StackLayout
			{
				Resources = new ResourceDictionary {
					{"basestyle", baseStyle0}
				},
				Children = {
					label0
				}
			};

			Assert.Equal(Colors.Red, label0.BackgroundColor);
			Assert.Equal(Colors.Red, label0.TextColor);
			Assert.Equal("foo", label0.Text);

			var label1 = new Label
			{
				Style = style
			};

			Assert.Equal(Colors.Red, label1.BackgroundColor);
			Assert.Equal(Colors.Red, label1.TextColor);
			Assert.Equal(Label.TextProperty.DefaultValue, label1.Text);

			var layout1 = new StackLayout
			{
				Children = {
					label1
				}
			};
			layout1.Resources = new ResourceDictionary {
				{"basestyle", baseStyle1}
			};

			Assert.Equal(Colors.Red, label1.BackgroundColor);
			Assert.Equal(Colors.Red, label1.TextColor);
			Assert.Equal("bar", label1.Text);
		}

		[Fact]
		public void TestTriggersAndBehaviors()
		{
			var behavior = new MockBehavior<Entry>();
			var style = new Style(typeof(Entry))
			{
				Setters = {
					new Setter {Property = Entry.TextProperty, Value = "foo"},
				},
				Triggers = {
					new Trigger (typeof (VisualElement)) {Property = Entry.IsPasswordProperty, Value=true, Setters = {
							new Setter {Property = VisualElement.ScaleProperty, Value = 2d},
						}}
				},
				Behaviors = {
					behavior,
				}
			};

			var entry = new Entry { Style = style };
			Assert.Equal("foo", entry.Text);
			Assert.Equal(1d, entry.Scale);

			entry.IsPassword = true;
			Assert.Equal(2d, entry.Scale);

			Assert.True(behavior.attached);

			entry.Style = null;

			Assert.Equal(Entry.TextProperty.DefaultValue, entry.Text);
			Assert.True(entry.IsPassword);
			Assert.Equal(1d, entry.Scale);
			Assert.True(behavior.detached);
		}

		[Fact]
		//Issue #2124
		public void SetValueOverridesStyle()
		{
			var style = new Style(typeof(Label))
			{
				Setters = {
					new Setter {Property = Label.TextColorProperty, Value=Colors.Black},
				}
			};

			var label = new Label { TextColor = Colors.White, Style = style };
			Assert.Equal(Colors.White, label.TextColor);
		}

		[Fact]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=28556
		public void TriggersAppliedAfterSetters()
		{
			var style = new Style(typeof(Entry))
			{
				Setters = {
					new Setter { Property = Entry.TextColorProperty, Value = Colors.Yellow }
				},
				Triggers = {
					new Trigger (typeof(Entry)) {
						Property = VisualElement.IsEnabledProperty,
						Value = false,
						Setters = {
							new Setter { Property = Entry.TextColorProperty, Value = Colors.Red }
						},
					}
				},
			};

			var entry = new Entry { IsEnabled = false, Style = style };
			Assert.Equal(Colors.Red, entry.TextColor);
			entry.IsEnabled = true;
			Assert.Equal(Colors.Yellow, entry.TextColor);
		}

		[Fact]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=31207
		public async Task StyleDontHoldStrongReferences()
		{
			var style = new Style(typeof(Label));
			var label = new Label();
			var tracker = new WeakReference(label);
			label.Style = style;
			label = null;

			await Task.Delay(10);
			GC.Collect();

			Assert.False(tracker.IsAlive);
			Assert.NotNull(style);
		}

		class MyLabel : Label
		{
		}

		class MyButton : Button
		{
		}

		[Fact]
		public void ImplicitStylesNotAppliedToDerivedTypesByDefault()
		{
			var style = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "Foo" }
				}
			};
			var view = new ContentView
			{
				Resources = new ResourceDictionary { style },
				Content = new MyLabel(),
			};

			Assert.Equal(Label.TextProperty.DefaultValue, ((MyLabel)view.Content).Text);
		}

		[Fact]
		public void ImplicitStylesAreAppliedToDerivedIfSpecified()
		{
			var style = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "Foo" }
				},
				ApplyToDerivedTypes = true
			};
			var view = new ContentView
			{
				Resources = new ResourceDictionary { style },
				Content = new MyLabel(),
			};

			Assert.Equal("Foo", ((MyLabel)view.Content).Text);
		}

		[Fact]
		public void ClassStylesAreApplied()
		{
			var classstyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "Foo" }
				},
				Class = "fooClass",
			};
			var style = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.Red }
				},
			};
			var view = new ContentView
			{
				Resources = new ResourceDictionary { classstyle },
				Content = new Label
				{
					StyleClass = new[] { "fooClass" },
					Style = style
				}
			};
			Assert.Equal("Foo", ((Label)view.Content).Text);
			Assert.Equal(Colors.Red, ((Label)view.Content).TextColor);
		}

		[Fact]
		public void ImplicitStylesAppliedByDefaultIfAStyleExists()
		{
			var implicitstyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "Foo" }
				},
			};
			var style = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.Red }
				},
			};
			var view = new ContentView
			{
				Resources = new ResourceDictionary { implicitstyle },
				Content = new Label
				{
					Style = style
				}
			};
			Assert.Equal("Foo", ((Label)view.Content).Text);
			Assert.Equal(Colors.Red, ((Label)view.Content).TextColor);
		}

		[Fact]
		public void ImplicitStylesAppliedIfStyleCanCascade()
		{
			var implicitstyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "Foo" }
				},
			};
			var style = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.Red },
				},
				CanCascade = true
			};
			var view = new ContentView
			{
				Resources = new ResourceDictionary { implicitstyle },
				Content = new Label
				{
					Style = style
				}
			};
			Assert.Equal("Foo", ((Label)view.Content).Text);
			Assert.Equal(Colors.Red, ((Label)view.Content).TextColor);
		}

		[Fact]
		public void MultipleStylesCanShareTheSameClassName()
		{
			var buttonStyle = new Style(typeof(Button))
			{
				Setters = {
					new Setter { Property = Button.TextColorProperty, Value = Colors.Pink },
				},
				Class = "pink",
				ApplyToDerivedTypes = true,
			};
			var labelStyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Pink },
				},
				Class = "pink",
				ApplyToDerivedTypes = false,
			};


			var button = new Button
			{
				StyleClass = new[] { "pink" },
			};
			var myButton = new MyButton
			{
				StyleClass = new[] { "pink" },
			};

			var label = new Label
			{
				StyleClass = new[] { "pink" },
			};
			var myLabel = new MyLabel
			{
				StyleClass = new[] { "pink" },
			};


			new StackLayout
			{
				Resources = new ResourceDictionary { buttonStyle, labelStyle },
				Children = {
					button,
					label,
					myLabel,
					myButton,
				}
			};

			Assert.Equal(Colors.Pink, button.TextColor);
			Assert.Null(button.BackgroundColor);

			Assert.Equal(Colors.Pink, myButton.TextColor);
			Assert.Null(myButton.BackgroundColor);

			Assert.Equal(Colors.Pink, label.BackgroundColor);
			Assert.Null(label.TextColor);

			Assert.Null(myLabel.BackgroundColor);
			Assert.Null(myLabel.TextColor);
		}

		[Fact]
		public void StyleClassAreCorrecltyMerged()
		{
			var buttonStyle = new Style(typeof(Button))
			{
				Setters = {
					new Setter { Property = Button.TextColorProperty, Value = Colors.Pink },
				},
				Class = "pink",
				ApplyToDerivedTypes = true,
			};
			var labelStyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Pink },
				},
				Class = "pink",
				ApplyToDerivedTypes = false,
			};

			var button = new Button
			{
				StyleClass = new[] { "pink" },
			};
			var label = new Label
			{
				StyleClass = new[] { "pink" },
			};

			var cv = new ContentView
			{
				Resources = new ResourceDictionary { buttonStyle },
				Content = new StackLayout
				{
					Resources = new ResourceDictionary { labelStyle },
					Children = {
						button,
						label,
					}
				}
			};

			Assert.Equal(Colors.Pink, button.TextColor);
			Assert.Null(button.BackgroundColor);

			Assert.Equal(Colors.Pink, label.BackgroundColor);
			Assert.Null(label.TextColor);
		}

		[Fact]
		public void StyleClassAreCorrecltyMergedForAlreadyParentedPArents()
		{
			var buttonStyle = new Style(typeof(Button))
			{
				Setters = {
						new Setter { Property = Button.TextColorProperty, Value = Colors.Pink },
					},
				Class = "pink",
				ApplyToDerivedTypes = true,
			};
			var labelStyle = new Style(typeof(Label))
			{
				Setters = {
						new Setter { Property = Button.BackgroundColorProperty, Value = Colors.Pink },
					},
				Class = "pink",
				ApplyToDerivedTypes = false,
			};

			var button = new Button
			{
				StyleClass = new[] { "pink" },
			};
			var label = new Label
			{
				StyleClass = new[] { "pink" },
			};

			var cv = new ContentView
			{
				Resources = new ResourceDictionary { buttonStyle },
				Content = new StackLayout
				{
					Resources = new ResourceDictionary { labelStyle },
				}
			};

			(cv.Content as StackLayout).Children.Add(button);
			(cv.Content as StackLayout).Children.Add(label);

			Assert.Equal(Colors.Pink, button.TextColor);
			Assert.Null(button.BackgroundColor);

			Assert.Equal(Colors.Pink, label.BackgroundColor);
			Assert.Null(label.TextColor);
		}

		[Fact]
		public void MultipleStyleClassAreApplied()
		{
			var pinkStyle = new Style(typeof(Button))
			{
				Setters = {
					new Setter { Property = Button.TextColorProperty, Value = Colors.Pink },
				},
				Class = "pink",
				ApplyToDerivedTypes = true,
			};
			var bigStyle = new Style(typeof(Button))
			{
				Setters = {
					new Setter { Property = Button.FontSizeProperty, Value = 20 },
				},
				Class = "big",
				ApplyToDerivedTypes = true,
			};
			var button = new Button
			{
				StyleClass = new[] { "pink", "big" },
			};

			new ContentView
			{
				Resources = new ResourceDictionary { pinkStyle, bigStyle },
				Content = button
			};

			Assert.Equal(Colors.Pink, button.TextColor);
			Assert.Equal(20d, button.FontSize);
		}

		[Fact]
		public void ReplacingResourcesDoesNotOverrideManuallySetProperties()
		{
			var label0 = new Label
			{
				TextColor = Colors.Pink
			};
			var label1 = new Label();

			Assert.Equal(label0.TextColor, Colors.Pink);
			Assert.Null(label1.TextColor);

			var rd0 = new ResourceDictionary {
				new Style (typeof(Label)) {
					Setters = {
						new Setter {Property = Label.TextColorProperty, Value = Colors.Olive}
					}
				}
			};
			var rd1 = new ResourceDictionary {
				new Style (typeof(Label)) {
					Setters = {
						new Setter {Property = Label.TextColorProperty, Value = Colors.Lavender}
					}
				}
			};

			var mockApp = new MockApplication();
			Application.Current = mockApp;
			mockApp.Resources = rd0;

			var layout = new StackLayout
			{
				Children = {
					label0,
					label1,
				}
			};

			mockApp.LoadPage(new ContentPage { Content = layout });
			//Assert.Equal(label0.TextColor, Color.Pink);
			//Assert.Equal(label1.TextColor, null);

			Assert.Equal(label0.TextColor, Colors.Pink);
			Assert.Equal(label1.TextColor, Colors.Olive);

			mockApp.Resources = rd1;
			Assert.Equal(label0.TextColor, Colors.Pink);
			Assert.Equal(label1.TextColor, Colors.Lavender);
		}

		[Fact]
		public void ImplicitInheritedStyleForTemplatedElementIsAppliedCorrectlyForContentPage()
		{
			var controlTemplate = new ControlTemplate(typeof(ContentPresenter));

			var rd0 = new ResourceDictionary {
				new Style (typeof(ContentPage)) {
					Setters = {
						new Setter {Property = TemplatedPage.ControlTemplateProperty, Value = controlTemplate}
					},
					ApplyToDerivedTypes = true
				}
			};

			MockApplication.Current.Resources = rd0;
			MockApplication.Current.LoadPage(new MyPage()
			{
				Content = new Button()
			});

			var parentPage = (ContentPage)MockApplication.Current.MainPage;
			var pageContent = parentPage.Content;
			Assert.Same(pageContent?.Parent, parentPage);
		}

		[Fact]
		public void ImplicitInheritedStyleForTemplatedElementIsAppliedCorrectlyForContentView()
		{
			var controlTemplate = new ControlTemplate(typeof(ContentPresenter));

			var rd0 = new ResourceDictionary {
				new Style (typeof(ContentView)) {
					Setters = {
						new Setter {Property = TemplatedView.ControlTemplateProperty, Value = controlTemplate}
					},
					ApplyToDerivedTypes = true
				}
			};

			var mockApp = new MockApplication();
			mockApp.Resources = rd0;
			mockApp.LoadPage(new ContentPage()
			{
				Content = new MyContentView()
				{
					Content = new Button()
				}
			});

			Application.Current = mockApp;

			var parentView = (ContentView)((ContentPage)mockApp.MainPage).Content;
			var content = parentView.Content;
			Assert.Same(content?.Parent, parentView);
		}

		class MyPage : ContentPage
		{
		}

		class MyContentView : ContentView
		{
		}

		[Fact]
		public void MismatchTargetTypeLogsWarningMessage1()
		{
			var s = new Style(typeof(Button));
			var t = new View();

			t.Style = s;

			Assert.Single(MockApplication.MockLogger.Messages);
			Assert.Equal($"Style TargetType Microsoft.Maui.Controls.Button is not compatible with element target type Microsoft.Maui.Controls.View", MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Fact]
		public void MismatchTargetTypeLogsWarningMessage2()
		{
			var s = new Style(typeof(Button));
			var t = new Label();

			t.Style = s;

			Assert.Single(MockApplication.MockLogger.Messages);
			Assert.Equal($"Style TargetType Microsoft.Maui.Controls.Button is not compatible with element target type Microsoft.Maui.Controls.Label", MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Fact]
		public void MatchTargetTypeDoesntLogWarningMessage()
		{
			var s = new Style(typeof(View));
			var t = new Button();

			t.Style = s;

			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"A warning was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Fact]
		public async Task CreatingStyledElementsOffMainThreadShouldNotCrash()
		{
			List<Task> tasks = new List<Task>();

			var style = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "foo" },
					new Setter { Property = VisualElement.BackgroundColorProperty, Value = Colors.Pink },
				}
			};

			for (int n = 0; n < 100000; n++)
			{
				tasks.Add(Task.Run(() =>
				{
					var label = new Label
					{
						Style = style
					};
				}));
			}

			await Task.WhenAll(tasks);
		}

		[Fact]
		public async Task ApplyAndRemoveStyleOffMainThreadShouldNotCrash()
		{
			List<Task> tasks = new List<Task>();

			var style = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "foo" },
					new Setter { Property = VisualElement.BackgroundColorProperty, Value = Colors.Pink },
				}
			};

			for (int n = 0; n < 10000; n++)
			{
				tasks.Add(Task.Run(() =>
				{
					var label = new Label
					{
						Style = style
					};

					label.Style = null;
				}));
			}

			await Task.WhenAll(tasks);
		}

		[Fact]
		//https://github.com/dotnet/maui/issues/4617
		public void ClearValueShouldntUnapplyStyles()
		{
			var button = new Button();
			var layout = new StackLayout
			{
				Resources = new ResourceDictionary {
					{ "Pinker", Colors.HotPink},
					new Style (typeof(Button)){ Setters = {
						new Setter{ Property=Button.BackgroundColorProperty, Value = new DynamicResource("Pinker")}
						}
					}
				},
				Children = { button },
			};

			Assert.Equal(button.BackgroundColor, Colors.HotPink);
			button.ClearValue(Button.BackgroundColorProperty);
			Assert.Equal(button.BackgroundColor, Colors.HotPink);
			button.BackgroundColor = Colors.Red;
			Assert.Equal(button.BackgroundColor, Colors.Red);
			button.ClearValue(Button.BackgroundColorProperty);
			Assert.Equal(button.BackgroundColor, Colors.HotPink);
		}

		[Fact]
		public void UnapplyingValueDefaultToStyle()
		{
			var label = new Label();
			var style = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "foo" },
				}
			};

			Assert.Equal(label.Text, Label.TextProperty.DefaultValue);

			label.Style = style;
			Assert.Equal("foo", label.Text);

			label.Text = "bar";
			Assert.Equal("bar", label.Text);

			label.ClearValue(Label.TextProperty);
			Assert.Equal("foo", label.Text);
		}

		[Fact]
		public void UnapplyingValueAfterStyleRemoval()
		{
			var label = new Label();
			var style = new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "foo" } } };

			Assert.Equal(label.Text, Label.TextProperty.DefaultValue);

			label.Style = style;
			Assert.Equal("foo", label.Text);

			label.Text = "bar";
			Assert.Equal("bar", label.Text);

			label.ClearValue(Label.StyleProperty);
			Assert.Equal("bar", label.Text);

			label.ClearValue(Label.TextProperty);
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
		}

		[Fact]
		public void UnapplyingStyleDefaultToImplicit1()
		{
			var stackLayout = new StackLayout
			{
				Resources = new ResourceDictionary {
					new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "implicit" } } },
				}
			};
			var label = new Label();
			var style = new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "style" }, } };

			Assert.Equal(label.Text, Label.TextProperty.DefaultValue);

			stackLayout.Children.Add(label);
			Assert.Equal("implicit", label.Text);

			label.Style = style;
			Assert.Equal("style", label.Text);

			label.Text = "value";
			Assert.Equal("value", label.Text);

			label.ClearValue(Label.StyleProperty);
			Assert.Equal("value", label.Text);

			label.ClearValue(Label.TextProperty);
			Assert.Equal("implicit", label.Text);
		}

		[Fact]
		public void UnapplyingStyleDefaultToImplicit2()
		{
			var stackLayout = new StackLayout
			{
				Resources = new ResourceDictionary {
					new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "implicit" } } },
				}
			};
			var label = new Label();
			var style = new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "style" }, } };

			Assert.Equal(label.Text, Label.TextProperty.DefaultValue);

			label.Style = style;
			Assert.Equal("style", label.Text);

			stackLayout.Children.Add(label);
			Assert.Equal("style", label.Text);

			label.Text = "value";
			Assert.Equal("value", label.Text);

			label.ClearValue(Label.StyleProperty);
			Assert.Equal("value", label.Text);

			label.ClearValue(Label.TextProperty);
			Assert.Equal("implicit", label.Text);
		}

		[Fact]
		public void UnapplyingStyleDefaultToImplicit3()
		{
			var stackLayout = new StackLayout
			{
				Resources = new ResourceDictionary {
					new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "implicit" } } },
				}
			};
			var label = new Label();
			var style = new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "style" }, } };

			Assert.Equal(label.Text, Label.TextProperty.DefaultValue);

			label.Text = "value";
			Assert.Equal("value", label.Text);

			label.Style = style;
			Assert.Equal("value", label.Text);

			stackLayout.Children.Add(label);
			Assert.Equal("value", label.Text);

			label.ClearValue(Label.StyleProperty);
			Assert.Equal("value", label.Text);

			label.ClearValue(Label.TextProperty);
			Assert.Equal("implicit", label.Text);
		}

		[Fact]
		public void UnapplyingStyleDefaultToImplicit4()
		{
			var stackLayout = new StackLayout
			{
				Resources = new ResourceDictionary {
					new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "implicit" } } },
				}
			};
			var label = new Label();
			var style = new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "style" }, } };

			Assert.Equal(label.Text, Label.TextProperty.DefaultValue);

			stackLayout.Children.Add(label);
			Assert.Equal("implicit", label.Text);

			label.Style = style;
			Assert.Equal("style", label.Text);

			label.ClearValue(Label.StyleProperty);
			Assert.Equal("implicit", label.Text);
		}

		[Fact]
		public void UnapplyingStyleDefaultToImplicit5()
		{
			var stackLayout = new StackLayout
			{
				Resources = new ResourceDictionary {
					new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "implicit" } } },
				}
			};
			var label = new Label();
			var style = new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "style" }, } };

			Assert.Equal(label.Text, Label.TextProperty.DefaultValue);

			label.Style = style;
			Assert.Equal("style", label.Text);

			stackLayout.Children.Add(label);
			Assert.Equal("style", label.Text);

			label.ClearValue(Label.StyleProperty);
			Assert.Equal("implicit", label.Text);
		}

		[Fact]
		public void UnapplyingBasedOn()
		{
			var basedOn = new Style(typeof(Label)) { Setters = { new Setter { Property = Label.TextProperty, Value = "basedOn" }, } };
			var style = new Style(typeof(Label)) { BasedOn = basedOn };

			var label = new Label { Style = style };
			Assert.Equal("basedOn", label.Text);

			style.BasedOn = null;
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);

		}
	}
}
