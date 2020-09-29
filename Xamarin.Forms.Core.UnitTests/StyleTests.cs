using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class StyleTests : BaseTestFixture
	{
		internal class Logger : LogListener
		{
			public IReadOnlyList<string> Messages
			{
				get { return messages; }
			}

			public override void Warning(string category, string message)
			{
				messages.Add("[" + category + "] " + message);
			}

			readonly List<string> messages = new List<string>();
		}

		internal Logger log;

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			log = new Logger();
			Device.PlatformServices = new MockPlatformServices();
			Log.Listeners.Add(log);
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Log.Listeners.Remove(log);
			Application.Current = null;
		}

		[Test]
		public void ApplyUnapplyStyle()
		{
			var style = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "foo" },
					new Setter { Property = VisualElement.BackgroundColorProperty, Value = Color.Pink },
				}
			};

			var label = new Label
			{
				Style = style
			};
			Assert.AreEqual("foo", label.Text);
			Assert.AreEqual(Color.Pink, label.BackgroundColor);

			label.Style = null;
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
			Assert.AreEqual(VisualElement.BackgroundColorProperty.DefaultValue, label.BackgroundColor);
		}

		[Test]
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
			Assert.AreEqual("FOO", label.Text);

			label.Resources = new ResourceDictionary {
				{"qux", Color.Pink}
			};
			Assert.AreEqual(Color.Pink, label.BackgroundColor);

			label.Style = null;
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
			Assert.AreEqual(VisualElement.BackgroundColorProperty.DefaultValue, label.BackgroundColor);
		}

		[Test]
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
				{"qux", Color.Pink}
			};

			Assert.AreEqual("FOO", label0.Text);
			Assert.AreEqual("FOO", label1.Text);

			Assert.AreEqual(Color.Pink, label0.BackgroundColor);
			Assert.AreEqual(Color.Pink, label1.BackgroundColor);

			label0.Style = label1.Style = null;

			Assert.AreEqual(Label.TextProperty.DefaultValue, label0.Text);
			Assert.AreEqual(Label.TextProperty.DefaultValue, label1.Text);
			Assert.AreEqual(VisualElement.BackgroundColorProperty.DefaultValue, label0.BackgroundColor);
			Assert.AreEqual(VisualElement.BackgroundColorProperty.DefaultValue, label1.BackgroundColor);
		}

		[Test]
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
			Assert.AreEqual("baseStyle", label.Text);

			label.Style = null;
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
		}

		[Test]
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
			Assert.AreEqual("style", label.Text);

			label.Style = null;
			Assert.AreEqual(Label.TextProperty.DefaultValue, label.Text);
		}

		[Test]
		public void AddImplicitStyleToResourceDictionary()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = {
						new Setter { Property = Label.TextColorProperty, Value = Color.Pink },
					}
				},
				{ "foo", "FOO" },
				{"labelStyle", new Style (typeof(Label)) { Setters = {
							new Setter { Property = Label.TextColorProperty, Value = Color.Purple }
						}
					}
				}
			};

			Assert.AreEqual(3, rd.Count);
			Assert.Contains("Xamarin.Forms.Label", (System.Collections.ICollection)rd.Keys);
		}

		[Test]
		public void ImplicitStylesAreAppliedOnSettingRD()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = {
						new Setter { Property = Label.TextColorProperty, Value = Color.Pink },
					}
				},
				{ "foo", "FOO" },
				{"labelStyle", new Style (typeof(Label)) { Setters = {
							new Setter { Property = Label.TextColorProperty, Value = Color.Purple }
						}
					}
				}
			};

			var label = new Label();
			var layout = new StackLayout { Children = { label } };

			Assert.AreEqual(label.TextColor, Label.TextColorProperty.DefaultValue);
			layout.Resources = rd;
			Assert.AreEqual(label.TextColor, Color.Pink);
		}

		[Test]
		public void ImplicitStylesAreAppliedOnSettingParrent()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = {
						new Setter { Property = Label.TextColorProperty, Value = Color.Pink },
					}
				},
				{ "foo", "FOO" },
				{"labelStyle", new Style (typeof(Label)) { Setters = {
							new Setter { Property = Label.TextColorProperty, Value = Color.Purple }
						}
					}
				}
			};

			var label = new Label();
			var layout = new StackLayout();
			layout.Resources = rd;

			Assert.AreEqual(label.TextColor, Label.TextColorProperty.DefaultValue);
			layout.Children.Add(label);
			Assert.AreEqual(label.TextColor, Color.Pink);
		}

		[Test]
		public void ImplicitStylesOverridenByStyle()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = {
						new Setter { Property = Label.TextColorProperty, Value = Color.Pink },
					}
				},
				{ "foo", "FOO" },
				{"labelStyle", new Style (typeof(Label)) { Setters = {
							new Setter { Property = Label.TextColorProperty, Value = Color.Purple }
						}
					}
				}
			};

			var label = new Label();
			label.SetDynamicResource(VisualElement.StyleProperty, "labelStyle");
			var layout = new StackLayout { Children = { label }, Resources = rd };

			Assert.AreEqual(label.TextColor, Color.Purple);
		}

		[Test]
		public void UnsettingStyleReApplyImplicit()
		{
			var rd = new ResourceDictionary {
				new Style (typeof(Label)) { Setters = {
						new Setter { Property = Label.TextColorProperty, Value = Color.Pink },
					}
				},
				{ "foo", "FOO" },
				{"labelStyle", new Style (typeof(Label)) { Setters = {
							new Setter { Property = Label.TextColorProperty, Value = Color.Purple }
						}
					}
				}
			};

			var label = new Label();
			label.SetDynamicResource(VisualElement.StyleProperty, "labelStyle");
			var layout = new StackLayout { Children = { label }, Resources = rd };

			Assert.AreEqual(label.TextColor, Color.Purple);
			label.Style = null;
			Assert.AreEqual(label.TextColor, Color.Pink);
		}

		[Test]
		public void DynamicStyle()
		{
			var baseStyle0 = new Style(typeof(Label))
			{
				Setters = {
					new Setter {Property = Label.TextProperty, Value = "foo"},
					new Setter {Property = Label.TextColorProperty, Value = Color.Pink}
				}
			};
			var baseStyle1 = new Style(typeof(Label))
			{
				Setters = {
					new Setter {Property = Label.TextProperty, Value = "bar"},
					new Setter {Property = Label.TextColorProperty, Value = Color.Purple}
				}
			};
			var style = new Style(typeof(Label))
			{
				BaseResourceKey = "basestyle",
				Setters = {
					new Setter { Property = Label.BackgroundColorProperty, Value = Color.Red },
					new Setter { Property = Label.TextColorProperty, Value = Color.Red },
				}
			};

			var label0 = new Label
			{
				Style = style
			};

			Assert.AreEqual(Color.Red, label0.BackgroundColor);
			Assert.AreEqual(Color.Red, label0.TextColor);
			Assert.AreEqual(Label.TextProperty.DefaultValue, label0.Text);

			var layout0 = new StackLayout
			{
				Resources = new ResourceDictionary {
					{"basestyle", baseStyle0}
				},
				Children = {
					label0
				}
			};

			Assert.AreEqual(Color.Red, label0.BackgroundColor);
			Assert.AreEqual(Color.Red, label0.TextColor);
			Assert.AreEqual("foo", label0.Text);

			var label1 = new Label
			{
				Style = style
			};

			Assert.AreEqual(Color.Red, label1.BackgroundColor);
			Assert.AreEqual(Color.Red, label1.TextColor);
			Assert.AreEqual(Label.TextProperty.DefaultValue, label1.Text);

			var layout1 = new StackLayout
			{
				Children = {
					label1
				}
			};
			layout1.Resources = new ResourceDictionary {
				{"basestyle", baseStyle1}
			};

			Assert.AreEqual(Color.Red, label1.BackgroundColor);
			Assert.AreEqual(Color.Red, label1.TextColor);
			Assert.AreEqual("bar", label1.Text);
		}

		[Test]
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
			Assert.AreEqual("foo", entry.Text);
			Assert.AreEqual(1d, entry.Scale);

			entry.IsPassword = true;
			Assert.AreEqual(2d, entry.Scale);

			Assert.True(behavior.attached);

			entry.Style = null;

			Assert.AreEqual(Entry.TextProperty.DefaultValue, entry.Text);
			Assert.True(entry.IsPassword);
			Assert.AreEqual(1d, entry.Scale);
			Assert.True(behavior.detached);
		}

		[Test]
		//Issue #2124
		public void SetValueOverridesStyle()
		{
			var style = new Style(typeof(Label))
			{
				Setters = {
					new Setter {Property = Label.TextColorProperty, Value=Color.Black},
				}
			};

			var label = new Label { TextColor = Color.White, Style = style };
			Assert.AreEqual(Color.White, label.TextColor);
		}

		[Test]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=28556
		public void TriggersAppliedAfterSetters()
		{
			var style = new Style(typeof(Entry))
			{
				Setters = {
					new Setter { Property = Entry.TextColorProperty, Value = Color.Yellow }
				},
				Triggers = {
					new Trigger (typeof(Entry)) {
						Property = VisualElement.IsEnabledProperty,
						Value = false,
						Setters = {
							new Setter { Property = Entry.TextColorProperty, Value = Color.Red }
						},
					}
				},
			};

			var entry = new Entry { IsEnabled = false, Style = style };
			Assert.AreEqual(Color.Red, entry.TextColor);
			entry.IsEnabled = true;
			Assert.AreEqual(Color.Yellow, entry.TextColor);
		}

		[Test]
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

		[Test]
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

			Assert.AreEqual(Label.TextProperty.DefaultValue, ((MyLabel)view.Content).Text);
		}

		[Test]
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

			Assert.AreEqual("Foo", ((MyLabel)view.Content).Text);
		}

		[Test]
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
					new Setter { Property = Label.TextColorProperty, Value = Color.Red }
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
			Assert.AreEqual("Foo", ((Label)view.Content).Text);
			Assert.AreEqual(Color.Red, ((Label)view.Content).TextColor);
		}

		[Test]
		public void ImplicitStylesNotAppliedByDefaultIfAStyleExists()
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
					new Setter { Property = Label.TextColorProperty, Value = Color.Red }
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
			Assert.AreEqual(Label.TextProperty.DefaultValue, ((Label)view.Content).Text);
			Assert.AreEqual(Color.Red, ((Label)view.Content).TextColor);
		}

		[Test]
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
					new Setter { Property = Label.TextColorProperty, Value = Color.Red },
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
			Assert.AreEqual("Foo", ((Label)view.Content).Text);
			Assert.AreEqual(Color.Red, ((Label)view.Content).TextColor);
		}

		[Test]
		public void MultipleStylesCanShareTheSameClassName()
		{
			var buttonStyle = new Style(typeof(Button))
			{
				Setters = {
					new Setter { Property = Button.TextColorProperty, Value = Color.Pink },
				},
				Class = "pink",
				ApplyToDerivedTypes = true,
			};
			var labelStyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Button.BackgroundColorProperty, Value = Color.Pink },
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

			Assert.AreEqual(Color.Pink, button.TextColor);
			Assert.AreEqual(Color.Default, button.BackgroundColor);

			Assert.AreEqual(Color.Pink, myButton.TextColor);
			Assert.AreEqual(Color.Default, myButton.BackgroundColor);

			Assert.AreEqual(Color.Pink, label.BackgroundColor);
			Assert.AreEqual(Color.Default, label.TextColor);

			Assert.AreEqual(Color.Default, myLabel.BackgroundColor);
			Assert.AreEqual(Color.Default, myLabel.TextColor);
		}

		[Test]
		public void StyleClassAreCorrecltyMerged()
		{
			var buttonStyle = new Style(typeof(Button))
			{
				Setters = {
					new Setter { Property = Button.TextColorProperty, Value = Color.Pink },
				},
				Class = "pink",
				ApplyToDerivedTypes = true,
			};
			var labelStyle = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Button.BackgroundColorProperty, Value = Color.Pink },
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

			Assert.AreEqual(Color.Pink, button.TextColor);
			Assert.AreEqual(Color.Default, button.BackgroundColor);

			Assert.AreEqual(Color.Pink, label.BackgroundColor);
			Assert.AreEqual(Color.Default, label.TextColor);
		}

		[Test]
		public void StyleClassAreCorrecltyMergedForAlreadyParentedPArents()
		{
			var buttonStyle = new Style(typeof(Button))
			{
				Setters = {
						new Setter { Property = Button.TextColorProperty, Value = Color.Pink },
					},
				Class = "pink",
				ApplyToDerivedTypes = true,
			};
			var labelStyle = new Style(typeof(Label))
			{
				Setters = {
						new Setter { Property = Button.BackgroundColorProperty, Value = Color.Pink },
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

			Assert.AreEqual(Color.Pink, button.TextColor);
			Assert.AreEqual(Color.Default, button.BackgroundColor);

			Assert.AreEqual(Color.Pink, label.BackgroundColor);
			Assert.AreEqual(Color.Default, label.TextColor);
		}

		[Test]
		public void MultipleStyleClassAreApplied()
		{
			var pinkStyle = new Style(typeof(Button))
			{
				Setters = {
					new Setter { Property = Button.TextColorProperty, Value = Color.Pink },
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

			Assert.AreEqual(Color.Pink, button.TextColor);
			Assert.AreEqual(20d, button.FontSize);
		}

		[Test]
		public void ReplacingResourcesDoesNotOverrideManuallySetProperties()
		{
			var label0 = new Label
			{
				TextColor = Color.Pink
			};
			var label1 = new Label();

			Assume.That(label0.TextColor, Is.EqualTo(Color.Pink));
			Assume.That(label1.TextColor, Is.EqualTo(Color.Default));

			var rd0 = new ResourceDictionary {
				new Style (typeof(Label)) {
					Setters = {
						new Setter {Property = Label.TextColorProperty, Value = Color.Olive}
					}
				}
			};
			var rd1 = new ResourceDictionary {
				new Style (typeof(Label)) {
					Setters = {
						new Setter {Property = Label.TextColorProperty, Value = Color.Lavender}
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

			mockApp.MainPage = new ContentPage { Content = layout };
			//Assert.That(label0.TextColor, Is.EqualTo(Color.Pink));
			//Assert.That(label1.TextColor, Is.EqualTo(Color.Default));

			Assert.That(label0.TextColor, Is.EqualTo(Color.Pink));
			Assert.That(label1.TextColor, Is.EqualTo(Color.Olive));

			mockApp.Resources = rd1;
			Assert.That(label0.TextColor, Is.EqualTo(Color.Pink));
			Assert.That(label1.TextColor, Is.EqualTo(Color.Lavender));
		}

		[Test]
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

			var mockApp = new MockApplication();
			mockApp.Resources = rd0;
			mockApp.MainPage = new MyPage()
			{
				Content = new Button()
			};

			Application.Current = mockApp;

			var parentPage = (ContentPage)mockApp.MainPage;
			var pageContent = parentPage.Content;
			Assert.That(Equals(pageContent?.Parent, parentPage));
		}

		[Test]
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
			mockApp.MainPage = new ContentPage()
			{
				Content = new MyContentView()
				{
					Content = new Button()
				}
			};

			Application.Current = mockApp;

			var parentView = (ContentView)((ContentPage)mockApp.MainPage).Content;
			var content = parentView.Content;
			Assert.That(Equals(content?.Parent, parentView));
		}

		class MyPage : ContentPage
		{
		}

		class MyContentView : ContentView
		{
		}

		[Test]
		public void MismatchTargetTypeLogsWarningMessage1()
		{
			var s = new Style(typeof(Button));
			var t = new View();

			t.Style = s;

			Assert.AreEqual(log.Messages.Count, 1);
			Assert.AreEqual(log.Messages.FirstOrDefault(), $"[Styles] Style TargetType Xamarin.Forms.Button is not compatible with element target type Xamarin.Forms.View");
		}

		[Test]
		public void MismatchTargetTypeLogsWarningMessage2()
		{
			var s = new Style(typeof(Button));
			var t = new Label();

			t.Style = s;

			Assert.AreEqual(log.Messages.Count, 1);
			Assert.AreEqual(log.Messages.FirstOrDefault(), $"[Styles] Style TargetType Xamarin.Forms.Button is not compatible with element target type Xamarin.Forms.Label");
		}

		[Test]
		public void MatchTargetTypeDoesntLogWarningMessage()
		{
			var s = new Style(typeof(View));
			var t = new Button();

			t.Style = s;

			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"A warning was logged: " + log.Messages.FirstOrDefault());
		}

		[Test]
		public async Task CreatingStyledElementsOffMainThreadShouldNotCrash()
		{
			List<Task> tasks = new List<Task>();

			var style = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "foo" },
					new Setter { Property = VisualElement.BackgroundColorProperty, Value = Color.Pink },
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

		[Test]
		public async Task ApplyAndRemoveStyleOffMainThreadShouldNotCrash()
		{
			List<Task> tasks = new List<Task>();

			var style = new Style(typeof(VisualElement))
			{
				Setters = {
					new Setter { Property = Label.TextProperty, Value = "foo" },
					new Setter { Property = VisualElement.BackgroundColorProperty, Value = Color.Pink },
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
	}
}