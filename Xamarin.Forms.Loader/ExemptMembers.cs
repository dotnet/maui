using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Loader
{
	internal static class ExemptMembers
	{
		public static Dictionary<Type, IEnumerable<string>> UnitTestedTypes = new Dictionary<Type, IEnumerable<string>>
		{
			// Unit tested
			{ typeof (BindingTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (DateChangedEventArgs), new[] { "OldDate", "NewDate" } },
			{ typeof (ElementEventArgs), new[] { "Element" } },
			{ typeof (ItemTappedEventArgs), new[] { "Group", "Item" } },
			{ typeof (ItemVisibilityEventArgs), new[] { "Item" } },
			{ typeof (NavigationEventArgs), new[] { "Page" } },
			{
				typeof (BindableObject),
				new[]
				{
					"SetValue", "SetValue", "GetValue", "ClearValue", "ClearValue", "SetBinding", "RemoveBinding", "BindingContext",
					"PropertyChanging", "PropertyChanged", "BindingContextChanged"
				}
			},
			{ typeof (BindableObjectExtensions), new[] { "SetBinding", "SetBinding" } },
			{
				typeof (BindableProperty),
				new[]
				{
					"Create", "CreateReadOnly", "Create", "CreateReadOnly", "CreateAttached", "CreateAttachedReadOnly",
					"CreateAttachedReadOnly", "CreateAttached", "PropertyName", "ReturnType", "DeclaringType", "DefaultValue",
					"DefaultBindingMode", "IsReadOnly"
				}
			},
			{
				typeof (BindingBase),
				new[] { "EnableCollectionSynchronization", "DisableCollectionSynchronization", "Mode", "StringFormat" }
			},
			{ typeof (PropertyChangingEventArgs), new[] { "PropertyName" } },
			{ typeof (SelectedItemChangedEventArgs), new[] { "SelectedItem" } },
			{ typeof (TextChangedEventArgs), new[] { "OldTextValue", "NewTextValue" } },
			{ typeof (ToggledEventArgs), new[] { "Value" } },
			{ typeof (TypeConverter), new[] { "CanConvertFrom", "ConvertFrom", "ConvertFrom" } },
			{ typeof (View), new[] { "VerticalOptions", "HorizontalOptions" } },
			{ typeof (Easing), new[] { "Ease" } },
			{ typeof (NavigationPage), new[] { "CurrentPage", "Pushed", "PoppedToRoot" } },
			{ typeof (Page), new[] { "ForceLayout", "Padding", "LayoutChanged" } },
			{ typeof (RectangleTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (ColorTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{
				typeof (ResourceDictionary),
				new[] { "Add", "Clear", "ContainsKey", "GetEnumerator", "Remove", "TryGetValue", "Item", "Count", "Keys", "Values" }
			},
			{ typeof (PointTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (ThicknessTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (ToolbarItem), new[] { "CommandParameter" } },
			{ typeof (MessagingCenter), new[] { "Subscribe", "Subscribe", "Unsubscribe", "Unsubscribe", "Send", "Send" } },
			{ typeof (TextCell), new[] { "CommandParameter" } },
			{ typeof (ItemsView<>), new[] { "ItemsSource", "ItemTemplate" } },
			{
				typeof (TableSectionBase),
				new[]
				{
					"GetEnumerator", "Add", "Add", "Clear", "Contains", "CopyTo", "Remove", "IndexOf", "Insert", "RemoveAt", "Count",
					"Item", "CollectionChanged"
				}
			},
			{ typeof (DataTemplate), new[] { "CreateContent", "SetBinding", "SetValue", "Bindings", "Values" } },
			{ typeof (AbsoluteLayout), new[] { "GetLayoutFlags", "GetLayoutBounds", "AutoSize" } },
			{ typeof (BoundsTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (Button), new[] { "CommandParameter" } },
			{ typeof (Command), new[] { "Execute", "CanExecute", "ChangeCanExecute", "CanExecuteChanged" } },
			{ typeof (DependencyService), new[] { "Get" } },
			{ typeof (KeyboardTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (MasterDetailPage), new[] { "IsPresentedChanged" } },
			{ typeof (ProgressBar), new[] { "ProgressTo" } },
			{ typeof (Constraint), new[] { "Constant", "RelativeToParent", "RelativeToView", "FromExpression" } },
			{ typeof (ConstraintExpression), new[] { "ProvideValue", "Type", "Constant", "Factor", "Property", "ElementName" } },
			{ typeof (ConstraintTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (BoundsConstraint), new[] { "FromExpression" } },
			{
				typeof (RelativeLayout),
				new[] { "GetXConstraint", "GetYConstraint", "GetWidthConstraint", "GetHeightConstraint", "GetBoundsConstraint" }
			},
			{ typeof (ScrollView), new[] { "ContentSize" } },
			{ typeof (SearchBar), new[] { "SearchCommandParameter" } },
			{ typeof (Slider), new[] { "ValueChanged" } },
			{ typeof (Stepper), new[] { "ValueChanged" } },
			{ typeof (Switch), new[] { "Toggled" } },
			{ typeof (TemplateExtensions), new[] { "SetBinding" } },
			{
				typeof (ViewExtensions),
				new[]
				{
					"TranslateTo", "LayoutTo", "RelRotateTo", "RelScaleTo", "RotateTo", "RotateYTo", "RotateXTo", "ScaleTo", "FadeTo",
					"CancelAnimations"
				}
			},
			{ typeof (WebView), new[] { "Eval", "GoBack", "GoForward", "CanGoBack", "CanGoForward", "Navigating", "Navigated" } },
			{ typeof (WebViewSourceTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{
				typeof (Animation),
				new[] { "GetEnumerator", "Insert", "Commit", "Add", "WithConcurrent", "WithConcurrent", "GetCallback" }
			},
			{
				typeof (AnimationExtensions),
				new[]
				{
					"AnimateKinetic", "Animate", "Interpolate", "Animate", "Animate", "Animate", "AbortAnimation",
					"AnimationIsRunning"
				}
			},
			{ typeof (UriTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (GridLengthTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (Grid), new[] { "GetRow", "GetRowSpan", "GetColumn", "GetColumnSpan" } },
			{ typeof (RowDefinition), new[] { "Height", "SizeChanged" } },
			{ typeof (ColumnDefinition), new[] { "Width", "SizeChanged" } },
			{
				typeof (DefinitionCollection<>),
				new[]
				{
					"IndexOf", "Insert", "RemoveAt", "Add", "Clear", "Contains", "CopyTo", "Remove", "GetEnumerator", "Item", "Count",
					"IsReadOnly", "ItemSizeChanged"
				}
			},
			{
				typeof (Element),
				new[]
				{
					"Parent", "ParentView", "ClassId", "StyleId", "Id", "ChildAdded", "ChildRemoved", "DescendantAdded",
					"DescendantRemoved"
				}
			},
			{ typeof (Layout), new[] { "GetSizeRequest", "ForceLayout", "LayoutChildIntoBoundingRegion", "LayoutChanged" } },
			{ typeof (FocusEventArgs), new[] { "IsFocused", "VisualElement" } },
			{
				typeof (VisualElement),
				new[]
				{
					"BatchBegin", "BatchCommit", "GetSizeRequest", "WidthRequest", "HeightRequest", "MinimumWidthRequest",
					"MinimumHeightRequest", "Resources", "ChildrenReordered", "SizeChanged", "MeasureInvalidated", "Focused",
					"Unfocused"
				}
			},
			{ typeof (NameScopeExtensions), new[] { "FindByName" } },
			{ typeof (FontTypeConverter), new[] { "CanConvertFrom", "ConvertFrom" } },
			{ typeof (Picker), new[] { "SelectedIndexChanged" } },
			{ typeof (BindablePropertyKey), new[] { "BindableProperty" } },
			{ typeof (TappedEventArgs), new[] { "Parameter" } }
		};

		public static Dictionary<Type, IEnumerable<string>> CannotTestTypes = new Dictionary<Type, IEnumerable<string>>
		{
			{ typeof (Button), new[] { "BorderColor", "BorderRadius", "BorderWidth", "Image" } }
		};
	}
}