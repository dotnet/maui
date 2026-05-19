using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class DynamicResourceTests : BaseTestFixture
	{
		public DynamicResourceTests()
		{
			Application.Current = new MockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.Current = null;
			}

			base.Dispose(disposing);
		}

		[Fact]
		public void TestDynamicResourceOverride()
		{
			Application.Current.Resources = new ResourceDictionary
			{
				{ "GreenColor", Colors.Green },
				{ "RedColor", Colors.Red }
			};

			var setter = new Setter()
			{
				Property = Label.TextColorProperty,
				Value = new DynamicResource("RedColor")
			};
			var style = new Style(typeof(Label));
			style.Setters.Add(setter);
			Application.Current.Resources.Add(style);

			var label = new Label()
			{
				Text = "Green = :)"
			};
			label.SetDynamicResource(Label.TextColorProperty, "GreenColor");

			Application.Current.LoadPage(new ContentPage
			{
				Content = new StackLayout
				{
					Children = { label }
				}
			});

			Assert.Equal(Colors.Green, label.TextColor);
		}

		[Fact]
		public void TestDynamicResource()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			var layout = new StackLayout
			{
				Children = {
					label
				}
			};

			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);

			layout.Resources = new ResourceDictionary {
				{ "foo", "FOO" }
			};
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void SetResourceTriggerSetValue()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			label.Resources = new ResourceDictionary {
				{"foo", "FOO"}
			};
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void SetResourceOnParentTriggerSetValue()
		{
			var label = new Label();
			var layout = new StackLayout { Children = { label } };
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			layout.Resources = new ResourceDictionary {
				{"foo", "FOO"}
			};
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void SettingResourceTriggersValueChanged()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			label.Resources = new ResourceDictionary {
				{ "foo", "FOO" }
			};
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void AddingAResourceDictionaryTriggersValueChangedForExistingValues()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			var rd = new ResourceDictionary { { "foo", "FOO" } };
			label.Resources = rd;
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void ValueChangedTriggeredOnSubscribeIfKeyAlreadyExists()
		{
			var label = new Label
			{
				Resources = new ResourceDictionary { { "foo", "FOO" } }
			};
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void RemoveDynamicResourceStopsUpdating()
		{
			var label = new Label
			{
				Resources = new ResourceDictionary { { "foo", "FOO" } }
			};
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal("FOO", label.Text);
			label.RemoveDynamicResource(Label.TextProperty);
			label.Resources["foo"] = "BAR";
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void ReparentResubscribe()
		{
			var layout0 = new ContentView { Resources = new ResourceDictionary { { "foo", "FOO" } } };
			var layout1 = new ContentView { Resources = new ResourceDictionary { { "foo", "BAR" } } };

			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			Assert.Equal(Label.TextProperty.DefaultValue, label.Text);

			layout0.Content = label;
			Assert.Equal("FOO", label.Text);

			layout0.Content = null;
			layout1.Content = label;
			Assert.Equal("BAR", label.Text);
		}

		[Fact]
		public void ParentSetAppliesDynamicResourceFromParentWithoutListener()
		{
			var parent = new ContentView
			{
				Resources = new ResourceDictionary {
					{ "foo", "FOO" },
					{ "unrelated", "UNRELATED" },
				}
			};
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");

			label.Parent = parent;

			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void ParentSetDoesNotClearDynamicResourceWhenNewParentDoesNotContainKey()
		{
			var layout0 = new ContentView { Resources = new ResourceDictionary { { "foo", "FOO" } } };
			var layout1 = new ContentView { Resources = new ResourceDictionary { { "bar", "BAR" } } };
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			label.Parent = layout0;

			Assert.Equal("FOO", label.Text);

			label.Parent = layout1;

			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		public void ParentSetDoesNotOverrideLocalDynamicResourceValue()
		{
			var parent = new ContentView { Resources = new ResourceDictionary { { "foo", "PARENT" } } };
			var label = new Label
			{
				Resources = new ResourceDictionary {
					{ "foo", "LOCAL" },
				}
			};
			label.SetDynamicResource(Label.TextProperty, "foo");

			Assert.Equal("LOCAL", label.Text);

			label.Parent = parent;

			Assert.Equal("LOCAL", label.Text);
		}

		[Fact]
		public void ParentSetAppliesImplicitLabelStyleWithoutListener()
		{
			var style = new Style(typeof(Label))
			{
				Setters = {
					new Setter { Property = Label.TextColorProperty, Value = Colors.Red },
				}
			};
			var parent = new ContentView
			{
				Resources = new ResourceDictionary {
					{ "unrelated", "UNRELATED" },
					style,
				}
			};
			var label = new Label();

			label.Parent = parent;

			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void ParentSetMergesStyleClassResourcesWithoutListener()
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
					new Setter { Property = Label.TextColorProperty, Value = Colors.Green },
				},
				Class = "pink",
			};
			var parent = new ContentView
			{
				Resources = new ResourceDictionary {
					buttonStyle,
				}
			};
			var button = new Button
			{
				StyleClass = new[] { "pink" },
				Resources = new ResourceDictionary {
					labelStyle,
				}
			};

			button.Parent = parent;

			Assert.Equal(Colors.Pink, button.TextColor);
		}

		[Fact]
		public void ParentSetAppliesMultipleDynamicResourcesInFullSnapshotOrderWithoutListener()
		{
			var parent = new ContentView
			{
				Resources = new ResourceDictionary {
					{ "first", "FIRST" },
					{ "second", "SECOND" },
				}
			};
			var view = new DynamicResourceOrderView();
			view.SetDynamicResource(DynamicResourceOrderView.SecondProperty, "second");
			view.SetDynamicResource(DynamicResourceOrderView.FirstProperty, "first");

			view.Parent = parent;

			Assert.Equal(new[] { "FIRST", "SECOND" }, view.Changes);
			Assert.Equal("FIRST", view.First);
			Assert.Equal("SECOND", view.Second);
		}

		[Fact]
		public void ParentSetAppliesBindableObjectResourceBindingContextWithoutListener()
		{
			var resourceLabel = new Label();
			var parent = new ContentView
			{
				BindingContext = "PARENT-CONTEXT",
				Resources = new ResourceDictionary {
					{ "resource-label", resourceLabel },
				}
			};
			var view = new BindableResourceView();
			view.SetDynamicResource(BindableResourceView.ResourceProperty, "resource-label");

			view.Parent = parent;

			Assert.Same(resourceLabel, view.Resource);
			Assert.Equal("PARENT-CONTEXT", resourceLabel.BindingContext);
		}

		[Fact]
		public void ClearedResourcesDoesNotClearValues()
		{
			var layout0 = new ContentView { Resources = new ResourceDictionary { { "foo", "FOO" } } };
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			layout0.Content = label;

			Assert.Equal("FOO", label.Text);

			layout0.Resources.Clear();
			Assert.Equal("FOO", label.Text);
		}

		[Fact]
		//Issue 2608
		public void ResourcesCanBeChanged()
		{
			var label = new Label();
			label.BindingContext = new MockViewModel();
			label.SetBinding(Label.TextProperty, "Text", BindingMode.TwoWay);
			label.SetDynamicResource(Label.TextProperty, "foo");
			label.Resources = new ResourceDictionary { { "foo", "FOO" } };

			Assert.Equal("FOO", label.Text);

			label.Resources["foo"] = "BAR";

			Assert.Equal("BAR", label.Text);
		}

		[Fact]
		public void FallbackToApplicationCurrent()
		{
			Application.Current.Resources = new ResourceDictionary { { "foo", "FOO" } };

			var label = new Label();
			label.BindingContext = new MockViewModel();
			label.SetBinding(Label.TextProperty, "Text", BindingMode.TwoWay);
			label.SetDynamicResource(Label.TextProperty, "foo");

			Assert.Equal("FOO", label.Text);
		}

		class DynamicResourceOrderView : View
		{
			public static readonly BindableProperty FirstProperty = BindableProperty.Create(nameof(First), typeof(string), typeof(DynamicResourceOrderView), default(string),
				propertyChanged: (bindable, oldValue, newValue) => ((DynamicResourceOrderView)bindable).Changes.Add((string)newValue));

			public static readonly BindableProperty SecondProperty = BindableProperty.Create(nameof(Second), typeof(string), typeof(DynamicResourceOrderView), default(string),
				propertyChanged: (bindable, oldValue, newValue) => ((DynamicResourceOrderView)bindable).Changes.Add((string)newValue));

			public IList<string> Changes { get; } = new List<string>();

			public string First => (string)GetValue(FirstProperty);

			public string Second => (string)GetValue(SecondProperty);
		}

		class BindableResourceView : View
		{
			public static readonly BindableProperty ResourceProperty = BindableProperty.Create(nameof(Resource), typeof(object), typeof(BindableResourceView));

			public object Resource => GetValue(ResourceProperty);
		}
	}
}