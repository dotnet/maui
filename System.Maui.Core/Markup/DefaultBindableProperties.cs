using System;
using System.Collections.Generic;
using System.Reflection;
using static System.Maui.Core.Markup.Markup;

namespace System.Maui.Markup
{
	public static class DefaultBindableProperties
	{
		static Dictionary<string, BindableProperty> bindableObjectTypeDefaultProperty = new Dictionary<string, BindableProperty>
		{ // Key: full type name of BindableObject, Value: the default BindableProperty
		  // Note that we don't specify default properties for unconstructed generic types
			{ "System.Maui.ActivityIndicator", ActivityIndicator.IsRunningProperty },
			{ "System.Maui.BackButtonBehavior", BackButtonBehavior.CommandProperty },
			{ "System.Maui.BoxView", BoxView.ColorProperty },
			{ "System.Maui.Button", Button.CommandProperty },
			{ "System.Maui.CarouselPage", Page.TitleProperty },
			{ "System.Maui.CheckBox", CheckBox.IsCheckedProperty },
			{ "System.Maui.ClickGestureRecognizer", ClickGestureRecognizer.CommandProperty },
			{ "System.Maui.CollectionView", CollectionView.ItemsSourceProperty },
			{ "System.Maui.ContentPage", Page.TitleProperty },
			{ "System.Maui.DatePicker", DatePicker.DateProperty },
			{ "System.Maui.Editor", Editor.TextProperty },
			{ "System.Maui.Entry", Entry.TextProperty },
			{ "System.Maui.EntryCell", EntryCell.TextProperty },
			{ "System.Maui.FileImageSource", FileImageSource.FileProperty },
			{ "System.Maui.FileMediaSource", FileMediaSource.FileProperty },
			{ "System.Maui.HtmlWebViewSource", HtmlWebViewSource.HtmlProperty },
			{ "System.Maui.Image", Image.SourceProperty },
			{ "System.Maui.ImageButton", ImageButton.CommandProperty },
			{ "System.Maui.ImageCell", ImageCell.ImageSourceProperty },
			{ "System.Maui.ItemsView", ItemsView.ItemsSourceProperty },
			{ "System.Maui.Label", Label.TextProperty },
			{ "System.Maui.ListView", ListView.ItemsSourceProperty },
			{ "System.Maui.MasterDetailPage", Page.TitleProperty },
			{ "System.Maui.MediaElement", MediaElement.SourceProperty },
			{ "System.Maui.MenuItem", MenuItem.CommandProperty },
			{ "System.Maui.MultiPage", Page.TitleProperty },
			{ "System.Maui.NavigationPage", Page.TitleProperty },
			{ "System.Maui.Page", Page.TitleProperty },
			{ "System.Maui.Picker", Picker.SelectedIndexProperty },
			{ "System.Maui.ProgressBar", ProgressBar.ProgressProperty },
			{ "System.Maui.RadioButton", RadioButton.IsCheckedProperty },
			{ "System.Maui.RefreshView", RefreshView.CommandProperty },
			{ "System.Maui.SearchBar", SearchBar.SearchCommandProperty },
			{ "System.Maui.SearchHandler", SearchHandler.CommandProperty },
			{ "System.Maui.Slider", Slider.ValueProperty },
			{ "System.Maui.Span", Span.TextProperty },
			{ "System.Maui.Stepper", Stepper.ValueProperty },
			{ "System.Maui.StreamImageSource", StreamImageSource.StreamProperty },
			{ "System.Maui.SwipeGestureRecognizer", SwipeGestureRecognizer.CommandProperty },
			{ "System.Maui.SwipeItem", SwipeItem.CommandProperty },
			{ "System.Maui.Switch", Switch.IsToggledProperty },
			{ "System.Maui.SwitchCell", SwitchCell.OnProperty },
			{ "System.Maui.TabbedPage", Page.TitleProperty },
			{ "System.Maui.TableRoot", TableRoot.TitleProperty },
			{ "System.Maui.TableSection", TableSection.TitleProperty },
			{ "System.Maui.TableSectionBase", TableSectionBase.TitleProperty },
			{ "System.Maui.TapGestureRecognizer", TapGestureRecognizer.CommandProperty },
			{ "System.Maui.TemplatedPage", Page.TitleProperty },
			{ "System.Maui.TextCell", TextCell.TextProperty },
			{ "System.Maui.TimePicker", TimePicker.TimeProperty },
			{ "System.Maui.ToolbarItem", ToolbarItem.CommandProperty },
			{ "System.Maui.UriImageSource", UriImageSource.UriProperty },
			{ "System.Maui.UriMediaSource", UriMediaSource.UriProperty },
			{ "System.Maui.UrlWebViewSource", UrlWebViewSource.UrlProperty },
			{ "System.Maui.WebView", WebView.SourceProperty }
		};

		static Dictionary<string, (BindableProperty, BindableProperty)> bindableObjectTypeDefaultCommandAndParameterProperties = new Dictionary<string, (BindableProperty, BindableProperty)>
		{ // Key: full type name of BindableObject, Value: command property and corresponding commandParameter property 
			{ "System.Maui.Button", (Button.CommandProperty, Button.CommandParameterProperty) },
			{ "System.Maui.TextCell", (TextCell.CommandProperty, TextCell.CommandParameterProperty) },
			{ "System.Maui.ClickGestureRecognizer", (ClickGestureRecognizer.CommandProperty, ClickGestureRecognizer.CommandParameterProperty) },
			{ "System.Maui.ImageButton", (ImageButton.CommandProperty, ImageButton.CommandParameterProperty) },
			{ "System.Maui.MenuItem", (MenuItem.CommandProperty, MenuItem.CommandParameterProperty) },
			{ "System.Maui.RefreshView", (RefreshView.CommandProperty, RefreshView.CommandParameterProperty) },
			{ "System.Maui.SwipeGestureRecognizer", (SwipeGestureRecognizer.CommandProperty, SwipeGestureRecognizer.CommandParameterProperty) },
			{ "System.Maui.SwipeItemView", (SwipeItemView.CommandProperty, SwipeItemView.CommandParameterProperty) },
			{ "System.Maui.TapGestureRecognizer", (TapGestureRecognizer.CommandProperty, TapGestureRecognizer.CommandParameterProperty) }
		};

		public static void Register(params BindableProperty[] properties)
		{
			VerifyExperimental();
			foreach (var property in properties)
				bindableObjectTypeDefaultProperty.Add(property.DeclaringType.FullName, property);
		}

		public static void RegisterForCommand(params (BindableProperty commandProperty, BindableProperty parameterProperty)[] propertyPairs)
		{
			VerifyExperimental();
			foreach (var propertyPair in propertyPairs)
				bindableObjectTypeDefaultCommandAndParameterProperties.Add(propertyPair.commandProperty.DeclaringType.FullName, propertyPair);
		}

		internal static void Unregister(BindableProperty property)
			=> bindableObjectTypeDefaultProperty.Remove(property.DeclaringType.FullName);

		internal static BindableProperty GetFor(BindableObject bindableObject)
		{
			var type = bindableObject.GetType();
			var defaultProperty = GetFor(type);
			if (defaultProperty == null)
				throw new ArgumentException(
					"No default bindable property is registered for BindableObject type " + type.FullName +
					"\r\nEither specify a property when calling Bind() or register a default bindable property for this BindableObject type");
			return defaultProperty;
		}

		internal static BindableProperty GetFor(Type bindableObjectType)
		{
			BindableProperty defaultProperty;

			do
			{
				string bindableObjectTypeName = bindableObjectType.FullName;
				if (bindableObjectTypeDefaultProperty.TryGetValue(bindableObjectTypeName, out defaultProperty))
					break;
				if (bindableObjectTypeName.StartsWith("System.Maui.", StringComparison.Ordinal))
					break;

				bindableObjectType = bindableObjectType.GetTypeInfo().BaseType;
			} while (bindableObjectType != null);

			return defaultProperty;
		}

		internal static void UnregisterForCommand(BindableProperty commandProperty)
			=> bindableObjectTypeDefaultCommandAndParameterProperties.Remove(commandProperty.DeclaringType.FullName);

		internal static (BindableProperty, BindableProperty) GetForCommand(BindableObject bindableObject)
		{
			var type = bindableObject.GetType();
			(var commandProperty, var parameterProperty) = GetForCommand(type);
			if (commandProperty == null)
				throw new ArgumentException(
					"No command + command parameter properties are registered for BindableObject type " + type.FullName +
					"\r\nRegister command + command parameter properties for this BindableObject type");
			return (commandProperty, parameterProperty);
		}

		internal static (BindableProperty, BindableProperty) GetForCommand(Type bindableObjectType)
		{
			(BindableProperty, BindableProperty) commandAndParameterProperties;

			do
			{
				string bindableObjectTypeName = bindableObjectType.FullName;
				if (bindableObjectTypeDefaultCommandAndParameterProperties.TryGetValue(bindableObjectTypeName, out commandAndParameterProperties))
					break;
				if (bindableObjectTypeName.StartsWith("System.Maui.", StringComparison.Ordinal))
					break;

				bindableObjectType = bindableObjectType.GetTypeInfo().BaseType;
			} while (bindableObjectType != null);

			return commandAndParameterProperties;
		}
	}
}
