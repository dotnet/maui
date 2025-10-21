﻿namespace Microsoft.Maui.DeviceTests
{
	public static class TestCategory
	{
		// Because we run the ios/catalyst one category at a time, we only want to compile in
		// Categories that iOS/Catalyst are actually using
#if ANDROID || WINDOWS
		public const string MauiContext = "MauiContext";
		public const string Application = "Application";
		public const string BoxView = "BoxView";
		public const string RadioButton = "RadioButton";
		public const string WindowOverlay = "WindowOverlay";
#endif

		public const string ActivityIndicator = "ActivityIndicator";
		public const string Border = "Border";
		public const string Button = "Button";
		public const string CheckBox = "CheckBox";
		public const string ContentView = "ContentView";
		public const string Element = "Element";
		public const string DatePicker = "DatePicker";
		public const string Dispatcher = "Dispatcher";
		public const string Editor = "Editor";
		public const string Entry = "Entry";
		public const string FlowDirection = "FlowDirection";
		public const string FlyoutView = "FlyoutView";
		public const string Fonts = "Fonts";
		public const string Graphics = "Graphics";
		public const string GraphicsView = "GraphicsView";
		public const string Image = "Image";
		public const string ImageButton = "ImageButton";
		public const string ImageSource = "ImageSource";
		public const string IndicatorView = "IndicatorView";
		public const string Label = "Label";
		public const string Layout = "Layout";
		public const string Memory = "Memory";
		public const string NavigationPage = "NavigationPage";
		public const string Page = "Page";
		public const string Picker = "Picker";
		public const string ProgressBar = "ProgressBar";
		public const string RefreshView = "RefreshView";
		public const string ScrollView = "ScrollView";
		public const string SearchBar = "SearchBar";
		public const string ShapeView = "ShapeView";
		public const string Slider = "Slider";
		public const string Stepper = "Stepper";
		public const string SwipeView = "SwipeView";
		public const string Switch = "Switch";
		public const string TextFormatting = "Formatting";
		public const string TimePicker = "TimePicker";
		public const string View = "View";
		public const string WebView = "WebView";
		public const string Window = "Window";
	}
}