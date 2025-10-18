#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using NSubstitute;
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

        /// <summary>
        /// Tests that getting the BasedOn property returns the current based-on style.
        /// </summary>
        [Fact]
        public void BasedOn_Get_ReturnsCurrentBasedOnStyle()
        {
            // Arrange
            var targetStyle = new Style(typeof(Button));
            var basedOnStyle = new Style(typeof(Button));
            targetStyle.BasedOn = basedOnStyle;

            // Act
            var result = targetStyle.BasedOn;

            // Assert
            Assert.Same(basedOnStyle, result);
        }

        /// <summary>
        /// Tests that setting the BasedOn property to the same value does not trigger any side effects.
        /// </summary>
        [Fact]
        public void BasedOn_SetSameValue_DoesNotTriggerSideEffects()
        {
            // Arrange
            var targetStyle = new Style(typeof(Button));
            var basedOnStyle = new Style(typeof(Button));
            targetStyle.BasedOn = basedOnStyle;
            var originalBaseResourceKey = targetStyle.BaseResourceKey;

            // Act
            targetStyle.BasedOn = basedOnStyle;

            // Assert
            Assert.Same(basedOnStyle, targetStyle.BasedOn);
            Assert.Equal(originalBaseResourceKey, targetStyle.BaseResourceKey);
        }

        /// <summary>
        /// Tests that setting the BasedOn property to null clears the based-on style.
        /// </summary>
        [Fact]
        public void BasedOn_SetToNull_ClearsBasedOnStyle()
        {
            // Arrange
            var targetStyle = new Style(typeof(Button));
            var basedOnStyle = new Style(typeof(Button));
            targetStyle.BasedOn = basedOnStyle;

            // Act
            targetStyle.BasedOn = null;

            // Assert
            Assert.Null(targetStyle.BasedOn);
        }

        /// <summary>
        /// Tests that setting the BasedOn property to a valid style sets the based-on style and clears BaseResourceKey.
        /// </summary>
        [Fact]
        public void BasedOn_SetValidStyle_SetsBasedOnAndClearsBaseResourceKey()
        {
            // Arrange
            var targetStyle = new Style(typeof(Button));
            var basedOnStyle = new Style(typeof(Button));
            targetStyle.BaseResourceKey = "TestKey";

            // Act
            targetStyle.BasedOn = basedOnStyle;

            // Assert
            Assert.Same(basedOnStyle, targetStyle.BasedOn);
            Assert.Null(targetStyle.BaseResourceKey);
        }

        /// <summary>
        /// Tests that setting the BasedOn property to a style with compatible target type succeeds.
        /// Derived types should be assignable to their base types.
        /// </summary>
        [Fact]
        public void BasedOn_SetCompatibleTargetType_Succeeds()
        {
            // Arrange
            var targetStyle = new Style(typeof(Button)); // Button derives from View
            var basedOnStyle = new Style(typeof(View));   // View is base class

            // Act & Assert
            targetStyle.BasedOn = basedOnStyle;
            Assert.Same(basedOnStyle, targetStyle.BasedOn);
        }

        /// <summary>
        /// Tests that setting the BasedOn property to a style with incompatible target type throws ArgumentException.
        /// This tests the validation failure path that ensures BasedOn.TargetType is compatible with TargetType.
        /// </summary>
        [Fact]
        public void BasedOn_SetIncompatibleTargetType_ThrowsArgumentException()
        {
            // Arrange
            var buttonStyle = new Style(typeof(Button));
            var labelStyle = new Style(typeof(Label)); // Label and Button are not related in inheritance

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => buttonStyle.BasedOn = labelStyle);
            Assert.Equal("BasedOn.TargetType is not compatible with TargetType", exception.Message);
        }

        /// <summary>
        /// Tests that setting the BasedOn property from null to a valid style works correctly.
        /// </summary>
        [Fact]
        public void BasedOn_SetFromNullToValid_SetsBasedOnStyle()
        {
            // Arrange
            var targetStyle = new Style(typeof(Button));
            var basedOnStyle = new Style(typeof(Button));
            Assert.Null(targetStyle.BasedOn); // Initially null

            // Act
            targetStyle.BasedOn = basedOnStyle;

            // Assert
            Assert.Same(basedOnStyle, targetStyle.BasedOn);
        }

        /// <summary>
        /// Tests that changing the BasedOn property from one valid style to another valid style works correctly.
        /// </summary>
        [Fact]
        public void BasedOn_ChangeFromValidToValid_UpdatesBasedOnStyle()
        {
            // Arrange
            var targetStyle = new Style(typeof(Button));
            var firstBasedOnStyle = new Style(typeof(Button));
            var secondBasedOnStyle = new Style(typeof(Button));
            targetStyle.BasedOn = firstBasedOnStyle;

            // Act
            targetStyle.BasedOn = secondBasedOnStyle;

            // Assert
            Assert.Same(secondBasedOnStyle, targetStyle.BasedOn);
            Assert.NotSame(firstBasedOnStyle, targetStyle.BasedOn);
        }

        /// <summary>
        /// Tests that the Style constructor throws ArgumentNullException when targetType is null.
        /// Verifies that null validation is properly enforced for the required targetType parameter.
        /// Expected result: ArgumentNullException with parameter name "targetType".
        /// </summary>
        [Fact]
        public void Constructor_NullTargetType_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new Style(null));
            Assert.Equal("targetType", exception.ParamName);
        }

        /// <summary>
        /// Tests that the Style constructor properly initializes with valid targetType values.
        /// Verifies that TargetType property is set correctly and Setters collection is initialized as empty.
        /// Expected result: TargetType matches input, Setters is empty list.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(Button))]
        [InlineData(typeof(Label))]
        [InlineData(typeof(int))]
        [InlineData(typeof(double))]
        [InlineData(typeof(IDisposable))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(string[]))]
        public void Constructor_ValidTargetType_InitializesCorrectly(Type targetType)
        {
            // Arrange & Act
            var style = new Style(targetType);

            // Assert
            Assert.Equal(targetType, style.TargetType);
            Assert.NotNull(style.Setters);
            Assert.Empty(style.Setters);
            Assert.IsAssignableFrom<IList<Setter>>(style.Setters);
        }

        /// <summary>
        /// Tests that the Style constructor works with abstract class types.
        /// Verifies that abstract types can be used as targetType without issues.
        /// Expected result: TargetType is set to the abstract type, Setters is initialized.
        /// </summary>
        [Fact]
        public void Constructor_AbstractTargetType_InitializesCorrectly()
        {
            // Arrange
            var targetType = typeof(BindableObject);

            // Act
            var style = new Style(targetType);

            // Assert
            Assert.Equal(targetType, style.TargetType);
            Assert.NotNull(style.Setters);
            Assert.Empty(style.Setters);
        }

        /// <summary>
        /// Tests that the Style constructor works with sealed class types.
        /// Verifies that sealed types can be used as targetType without issues.
        /// Expected result: TargetType is set to the sealed type, Setters is initialized.
        /// </summary>
        [Fact]
        public void Constructor_SealedTargetType_InitializesCorrectly()
        {
            // Arrange
            var targetType = typeof(string);

            // Act
            var style = new Style(targetType);

            // Assert
            Assert.Equal(targetType, style.TargetType);
            Assert.NotNull(style.Setters);
            Assert.Empty(style.Setters);
        }

        /// <summary>
        /// Tests that the Style constructor works with generic type definitions.
        /// Verifies that open generic types can be used as targetType.
        /// Expected result: TargetType is set to the generic type definition, Setters is initialized.
        /// </summary>
        [Fact]
        public void Constructor_GenericTypeDefinition_InitializesCorrectly()
        {
            // Arrange
            var targetType = typeof(List<>);

            // Act
            var style = new Style(targetType);

            // Assert
            Assert.Equal(targetType, style.TargetType);
            Assert.NotNull(style.Setters);
            Assert.Empty(style.Setters);
        }

        /// <summary>
        /// Tests that multiple Style instances can be created with the same targetType.
        /// Verifies that each instance maintains its own independent Setters collection.
        /// Expected result: Each Style has correct TargetType and separate Setters instances.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstancesSameTargetType_CreatesIndependentInstances()
        {
            // Arrange
            var targetType = typeof(Button);

            // Act
            var style1 = new Style(targetType);
            var style2 = new Style(targetType);

            // Assert
            Assert.Equal(targetType, style1.TargetType);
            Assert.Equal(targetType, style2.TargetType);
            Assert.NotSame(style1.Setters, style2.Setters);
            Assert.Empty(style1.Setters);
            Assert.Empty(style2.Setters);
        }
    }


    /// <summary>
    /// Tests for the Style.BaseResourceKey property
    /// </summary>
    public partial class StyleBaseResourceKeyTests
    {
        /// <summary>
        /// Tests that setting BaseResourceKey to the same value returns early without changes.
        /// Input: BaseResourceKey set to same value twice
        /// Expected: No side effects, property remains unchanged
        /// </summary>
        [Fact]
        public void BaseResourceKey_SameValue_NoChange()
        {
            // Arrange
            var style = new Style(typeof(Label));
            var testKey = "TestKey";
            style.BaseResourceKey = testKey;

            // Act
            style.BaseResourceKey = testKey; // Set to same value

            // Assert
            Assert.Equal(testKey, style.BaseResourceKey);
        }

        /// <summary>
        /// Tests that setting BaseResourceKey to null removes dynamic resource from all targets.
        /// Input: Style with targets, BaseResourceKey set to null
        /// Expected: RemoveDynamicResource called on all targets
        /// </summary>
        [Fact]
        public void BaseResourceKey_NullValue_RemovesDynamicResourceFromTargets()
        {
            // Arrange
            var style = new Style(typeof(Label));
            var mockBindable1 = Substitute.For<BindableObject>();
            var mockBindable2 = Substitute.For<BindableObject>();

            // Apply style to add targets
            ((IStyle)style).Apply(mockBindable1, SetterSpecificity.DefaultValue);
            ((IStyle)style).Apply(mockBindable2, SetterSpecificity.DefaultValue);

            style.BaseResourceKey = "InitialKey";

            // Act
            style.BaseResourceKey = null;

            // Assert
            Assert.Null(style.BaseResourceKey);
            mockBindable1.Received(1).RemoveDynamicResource(Arg.Any<BindableProperty>());
            mockBindable2.Received(1).RemoveDynamicResource(Arg.Any<BindableProperty>());
        }

        /// <summary>
        /// Tests that setting BaseResourceKey to non-null value sets dynamic resource on all targets.
        /// Input: Style with targets, BaseResourceKey set to non-null value
        /// Expected: RemoveDynamicResource and SetDynamicResource called on all targets
        /// </summary>
        [Fact]
        public void BaseResourceKey_NonNullValue_SetsDynamicResourceOnTargets()
        {
            // Arrange
            var style = new Style(typeof(Label));
            var mockBindable1 = Substitute.For<BindableObject>();
            var mockBindable2 = Substitute.For<BindableObject>();

            // Apply style to add targets
            ((IStyle)style).Apply(mockBindable1, SetterSpecificity.DefaultValue);
            ((IStyle)style).Apply(mockBindable2, SetterSpecificity.DefaultValue);

            var newKey = "NewResourceKey";

            // Act
            style.BaseResourceKey = newKey;

            // Assert
            Assert.Equal(newKey, style.BaseResourceKey);
            mockBindable1.Received(1).RemoveDynamicResource(Arg.Any<BindableProperty>());
            mockBindable1.Received(1).SetDynamicResource(Arg.Any<BindableProperty>(), newKey);
            mockBindable2.Received(1).RemoveDynamicResource(Arg.Any<BindableProperty>());
            mockBindable2.Received(1).SetDynamicResource(Arg.Any<BindableProperty>(), newKey);
        }

        /// <summary>
        /// Tests that setting BaseResourceKey to non-null value sets BasedOn to null.
        /// Input: Style with BasedOn set, BaseResourceKey set to non-null value
        /// Expected: BasedOn property becomes null
        /// </summary>
        [Fact]
        public void BaseResourceKey_NonNullValue_SetsBasedOnToNull()
        {
            // Arrange
            var baseStyle = new Style(typeof(Label));
            var style = new Style(typeof(Label))
            {
                BasedOn = baseStyle
            };

            // Act
            style.BaseResourceKey = "TestKey";

            // Assert
            Assert.Equal("TestKey", style.BaseResourceKey);
            Assert.Null(style.BasedOn);
        }

        /// <summary>
        /// Tests that setting BaseResourceKey works correctly when no targets exist.
        /// Input: Style with no applied targets, BaseResourceKey set
        /// Expected: Property is set without exception
        /// </summary>
        [Fact]
        public void BaseResourceKey_NoTargets_NoException()
        {
            // Arrange
            var style = new Style(typeof(Label));

            // Act & Assert
            style.BaseResourceKey = "TestKey";
            Assert.Equal("TestKey", style.BaseResourceKey);

            style.BaseResourceKey = null;
            Assert.Null(style.BaseResourceKey);
        }

        /// <summary>
        /// Tests BaseResourceKey with edge case string values.
        /// Input: Various edge case string values
        /// Expected: Values are accepted and stored correctly
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\n")]
        [InlineData("Very.Long.Resource.Key.With.Many.Dots.And.Characters")]
        [InlineData("Key with spaces")]
        [InlineData("Key\nwith\nnewlines")]
        public void BaseResourceKey_EdgeCases_AcceptsValues(string testValue)
        {
            // Arrange
            var style = new Style(typeof(Label));

            // Act
            style.BaseResourceKey = testValue;

            // Assert
            Assert.Equal(testValue, style.BaseResourceKey);
        }

        /// <summary>
        /// Tests that changing BaseResourceKey multiple times on style with targets works correctly.
        /// Input: Style with targets, BaseResourceKey changed multiple times
        /// Expected: Dynamic resources updated correctly each time
        /// </summary>
        [Fact]
        public void BaseResourceKey_MultipleChanges_UpdatesTargetsCorrectly()
        {
            // Arrange
            var style = new Style(typeof(Label));
            var mockBindable = Substitute.For<BindableObject>();

            // Apply style to add target
            ((IStyle)style).Apply(mockBindable, SetterSpecificity.DefaultValue);

            // Act & Assert - First change
            style.BaseResourceKey = "FirstKey";
            mockBindable.Received(1).RemoveDynamicResource(Arg.Any<BindableProperty>());
            mockBindable.Received(1).SetDynamicResource(Arg.Any<BindableProperty>(), "FirstKey");

            // Act & Assert - Second change
            style.BaseResourceKey = "SecondKey";
            mockBindable.Received(2).RemoveDynamicResource(Arg.Any<BindableProperty>());
            mockBindable.Received(1).SetDynamicResource(Arg.Any<BindableProperty>(), "SecondKey");

            // Act & Assert - Change to null
            style.BaseResourceKey = null;
            mockBindable.Received(3).RemoveDynamicResource(Arg.Any<BindableProperty>());
            // SetDynamicResource should not be called again
            mockBindable.DidNotReceive().SetDynamicResource(Arg.Any<BindableProperty>(), null);
        }
    }
}