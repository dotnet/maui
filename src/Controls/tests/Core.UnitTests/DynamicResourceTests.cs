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

		// Regression tests for https://github.com/dotnet/maui/issues/37540
		// SetDynamicResource did not update a property that already had a manually-set local value.

		[Fact]
		public void SetDynamicResourceOverridesPriorManualValue()
		{
			var label = new Label();
			label.Text = "Manual"; // manual value set first

			Application.Current.Resources = new ResourceDictionary { { "textKey", "FromResource" } };
			label.SetDynamicResource(Label.TextProperty, "textKey");

			Assert.Equal("FromResource", label.Text);
		}

		[Fact]
		public void SetDynamicResourceViaIDynamicResourceHandlerOverridesPriorManualValue()
		{
			// Exercises the same path used by XAML-compiled {DynamicResource} markup extensions.
			var label = new Label();
			label.Text = "Manual"; // manual value set first

			Application.Current.Resources = new ResourceDictionary { { "textKey", "FromResource" } };
			((IDynamicResourceHandler)label).SetDynamicResource(Label.TextProperty, "textKey");

			Assert.Equal("FromResource", label.Text);
		}

		[Fact]
		public void SetDynamicResourceKeepsDynamicResourceSpecificityAfterOverridingManualValue()
		{
			// After SetDynamicResource wins over a stale manual value, a later manual SetValue
			// must still be able to override it (i.e. the resolved value must not be permanently
			// re-tagged as a manual value with equal priority to future manual assignments).
			var label = new Label();
			label.Text = "Manual";

			Application.Current.Resources = new ResourceDictionary { { "textKey", "FromResource" } };
			label.SetDynamicResource(Label.TextProperty, "textKey");
			Assert.Equal("FromResource", label.Text);

			label.Text = "ManualAgain";
			Assert.Equal("ManualAgain", label.Text);
		}

		[Fact]
		public void AmbientResourceChangeDoesNotOverrideLaterManualValueAfterSetDynamicResource()
		{
			// A subsequent ambient ResourceDictionary change for the same key must not clobber a
			// manual value that was set *after* SetDynamicResource ran.
			var label = new Label();
			label.Text = "Manual";

			Application.Current.Resources = new ResourceDictionary { { "textKey", "FromResource" } };
			label.SetDynamicResource(Label.TextProperty, "textKey");
			Assert.Equal("FromResource", label.Text);

			label.Text = "ManualAgain";
			Assert.Equal("ManualAgain", label.Text);

			Application.Current.Resources = new ResourceDictionary { { "textKey", "FromResourceUpdated" } };
			Assert.Equal("ManualAgain", label.Text);
		}

		[Fact]
		public void SetDynamicResourceDoesNotExposeIntermediateDefaultValue()
		{
			var label = new Label { Text = "Manual" };
			var observedValues = new List<string>();
			label.PropertyChanged += (_, args) =>
			{
				if (args.PropertyName == nameof(Label.Text))
					observedValues.Add(label.Text);
			};

			Application.Current.Resources = new ResourceDictionary { { "textKey", "FromResource" } };
			label.SetDynamicResource(Label.TextProperty, "textKey");

			Assert.Equal(new[] { "FromResource" }, observedValues);
		}

		[Fact]
		public void ManualValueSetDuringDynamicResourceChangeWins()
		{
			var label = new Label { Text = "Manual" };
			var updateFromCallback = true;

			label.PropertyChanging += (_, args) =>
			{
				if (args.PropertyName == nameof(Label.Text) && updateFromCallback)
				{
					updateFromCallback = false;
					label.Text = "FromCallback";
				}
			};

			Application.Current.Resources = new ResourceDictionary { { "textKey", "FromResource" } };

			label.SetDynamicResource(Label.TextProperty, "textKey");

			Assert.Equal("FromCallback", label.Text);
		}

		[Fact]
		public void UnresolvedDynamicResourceRetainsLocalValueUntilResourceIsAvailable()
		{
			var label = new Label { Text = "Manual" };

			label.SetDynamicResource(Label.TextProperty, "textKey");

			Assert.Equal("Manual", label.Text);

			label.Resources["textKey"] = "FromResource";

			Assert.Equal("FromResource", label.Text);
		}

		[Fact]
		public void UnresolvedDynamicResourceDoesNotOverrideNewerManualValue()
		{
			var label = new Label { Text = "Manual" };

			label.SetDynamicResource(Label.TextProperty, "textKey");
			label.Text = "ManualAgain";

			label.Resources["textKey"] = "FromResource";

			Assert.Equal("ManualAgain", label.Text);
		}

		[Fact]
		public void DynamicResourceReplacesLocalValue()
		{
			var label = new Label
			{
				Text = "LOCAL",
				Resources = new ResourceDictionary
				{
					{ "foo", "RESOURCE" }
				}
			};

			// The dynamic resource should replace the old local value.
			label.SetDynamicResource(Label.TextProperty, "foo");

			Assert.Equal("RESOURCE", label.Text);

			// A newer local value should replace the dynamic resource.
			label.Text = "NEW LOCAL";

			// Updating the resource must not replace that newer local value.
			label.Resources["foo"] = "UPDATED RESOURCE";

			Assert.Equal("NEW LOCAL", label.Text);
		}
	}
}