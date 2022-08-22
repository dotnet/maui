We have created a detailed list to easily show the **.NET MAUI - Gtk status** and evolution.

Note that only the Gtk-Section is actual in this Page.

| Icon | Description |
| ----|:-------|
| ⚠️ | Pending
| ⏳ | Underway
| ✅ | Done
| 💔 | Never implemented in Maui.Controls for this platform

## Overview

To track ongoing progress, filter on the [handlers label](https://github.com/xamarin/Xamarin.Forms/labels/handlers).

### Pages

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| ContentPage | ✅  | ✅  | ✅  | ⚠️  | 
| FlyoutPage | ⚠️  | ⚠️  | ⚠️  | ⚠️  | 
| NavigationPage | ✅  | ✅  | ✅  | ⚠️  | 
| TabbedPage | ✅  | ✅  | ✅  | ⚠️  | 

### Views

### ✅ ActivityIndicator

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| Color      | ✅  | ✅ |  ✅  |  ✅  |
| IsRunning  | ✅  | ✅ |  ✅  |  ✅  |

### ⚠️ Button

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| BackgroundColor  | ✅  | ✅  | ✅  | ✅  | 
| BorderColor  | ⚠️  | ⚠️  | ⚠️  | ✅  | 
| BorderWidth  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 
| CharacterSpacing  | ✅  | ✅  | ✅  | ✅ | 
| Clicked  | ✅  | ✅  | ✅  | ✅  | 
| Command  | ✅  | ✅  | ✅  | ✅  | 
| CommandParameter  | ✅  | ✅  | ✅  |  ⚠️ |
| ContentLayout  | ⚠️  | ⚠️  | ⚠️  |  ⚠️ |
| CornerRadius  | ⚠️  | ⚠️  | ⚠️  |  ⚠️ |
| FontAttributes  | ✅  | ✅  | ✅  | ✅  | 
| FontFamily  | ✅  | ✅  | ✅  | ✅  | 
| FontSize  | ✅  | ✅  | ✅  | ✅  | 
| ImageSource  | ⚠️  | ⚠️  | ⚠️  | ⚠️  | 
| Padding  | ✅  | ✅  | ✅  | ✅  | 
| Pressed  | ✅  | ✅  | ✅  | ✅  | 
| Released  | ✅  | ✅  | ✅  | ✅  | 
| Text  | ✅  | ✅  | ✅  | ✅  | 
| TextColor  | ✅  | ✅  | ✅  | ✅  | 

<!--
### ⚠️ CarouselView

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| CurrentItem  | ⚠️  | ⚠️  | ⚠️  | 
| CurrentItemChangedCommand  | ⚠️  | ⚠️  | ⚠️  | 
| CurrentItemChangedCommandParameter  | ⚠️  | ⚠️  | ⚠️  | 
| IndicatorView  | ⚠️  | ⚠️  | ⚠️  | 
| IsBounceEnabled  | ⚠️  | ⚠️  | ⚠️  | 
| IsDragging  | ⚠️  | ⚠️  | ⚠️  | 
| IsScrollAnimated  | ⚠️  | ⚠️  | ⚠️  | 
| IsSwipeEnabled  | ⚠️  | ⚠️  | ⚠️  | 
| ItemsLayout  | ⚠️  | ⚠️  | ⚠️  | 
| Loop  | ⚠️  | ⚠️  | ⚠️  | 
| PeekAreaInsets  | ⚠️  | ⚠️  | ⚠️  | 
| Position  | ⚠️  | ⚠️  | ⚠️  | 
| PositionChangedCommand  | ⚠️  | ⚠️  | ⚠️  | 
| PositionChangedCommandParameter  | ⚠️  | ⚠️  | ⚠️  | 
| VisibleViews  | ⚠️  | ⚠️  | ⚠️  | 
-->

### ✅ CheckBox

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| Color  | ✅  | ✅  | ✅  | ✅ | 
| CheckedChanged  | ✅  | ✅  | ✅  |  ✅ | 
| IsChecked  | ✅  | ✅  | ✅  |  ✅ | 
<!--
### ⚠️ CollectionView

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| ItemsSource | ⚠️  | ⚠️  | ⚠️  | 
| ItemTemplate | ⚠️  | ⚠️  | ⚠️  | 
| ItemsPanel | ⚠️  | ⚠️  | ⚠️  | 
| ItemSizingStrategy | ⚠️  | ⚠️  | ⚠️  | 
| SelectionMode | ⚠️  | ⚠️  | ⚠️  | 
| SelectedItem | ⚠️  | ⚠️  | ⚠️  | 
| SelectedItems | ⚠️  | ⚠️  | ⚠️  | 
| SelectionChangedCommand | ⚠️  | ⚠️  | ⚠️  | 
| SelectionChangedCommandParameter | ⚠️  | ⚠️  | ⚠️  | 
| EmptyView | ⚠️  | ⚠️  | ⚠️  | 
| Scrolled | ⚠️  | ⚠️  | ⚠️  | 
| ScrollTo | ⚠️  | ⚠️  | ⚠️  | 
| Header | ⚠️  | ⚠️  | ⚠️  | 
| HeaderTemplate | ⚠️  | ⚠️  | ⚠️  | 
| Footer | ⚠️  | ⚠️  | ⚠️  | 
| FooterTemplate | ⚠️  | ⚠️  | ⚠️  | 
| IsGrouped | ⚠️  | ⚠️  | ⚠️  | 
| GroupHeaderTemplate | ⚠️  | ⚠️  | ⚠️  | 
| GroupFooterTemplate | ⚠️  | ⚠️  | ⚠️  | 
-->
### ✅ DatePicker

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| BackgroundColor  | ✅  | ✅  | ✅  |  ⚠️  | 
| CharacterSpacing  | ✅  | ✅  | ✅  |   ⚠️  | 
| Date  | ✅  | ✅  | ✅  |  ⚠️  | 
| DateSelected  | ✅  | ✅  | ✅  | ⚠️  |  
| FontAttributes  | ✅  | ✅  | ✅  |   ⚠️  | 
| FontFamily  | ✅  | ✅  | ✅  |  ⚠️  | 
| FontSize  | ✅  | ✅  | ✅  |  ⚠️  | 
| Format  | ✅  | ✅  | ✅  |  ⚠️  | 
| MaximumDate  | ✅  | ✅  | ✅  |  ⚠️  | 
| MinimumDate  | ✅  | ✅  | ✅  |  ⚠️  | 
| TextColor  | ✅  | ✅  | ✅  |  ⚠️  | 

### ⚠️ Editor

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| AutoSize  | ⏳  | ⏳  | ⏳  |   ⚠️  | 
| Completed  | ✅  | ✅  | ✅  |  ⚠️  | 
| CharacterSpacing  | ✅  | ✅  | ✅  |  ⚠️  | 
| FontAttributes  | ✅  | ✅  | ✅  | ✅  | 
| FontFamily  | ✅  | ✅  | ✅  |  ✅  
| FontSize  | ✅  | ✅  | ✅  |  ✅  
| IsReadOnly  | ✅  | ✅  | ✅  |  ✅  
| IsTextPredictionEnabled  | ✅  | ✅  | ⏳  |⚠️ 
| PlaceHolder  | ✅  | ✅  | ✅  | ⚠️ 
| PlaceHolderColor  | ✅  | ✅  | ✅  | ⚠️ 
| Text  | ✅  | ✅  | ✅  |  ✅  |
| TextColor  | ✅  | ✅  | ✅  | ✅  |
| MaxLength  | ✅  | ✅  | ✅  |  ⚠️   

### ⚠️ Entry

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| ClearButtonVisibility  | ✅  | ✅  | ✅  |  ⚠️ 
| CharacterSpacing  | ✅  | ✅  | ✅  |  ✅  |
| Completed  | ✅  | ✅  | ✅  | ⚠️ 
| CursorPosition  | ✅  | ✅  | ⚠️  |   ✅  
| FontAttributes  | ✅  | ✅  | ✅  |  ✅  
| FontFamily  | ✅  | ✅  | ✅  |  ✅  
| FontSize  | ✅  | ✅  | ✅  |  ✅  
| HorizontalTextAlignment  | ✅  | ✅  | ⏳  |  ✅  |
| IsTextPredictionEnabled  | ✅  | ✅  | ⏳  | ⚠️ 
| IsPassword  | ✅  | ✅  | ⏳ |  ✅  
| PlaceHolder  | ✅  | ✅  | ✅  |  ✅  
| PlaceHolderColor  | ⏳  | ⏳  | ⏳  |   ⚠️   
| VerticalTextAlignment  | ⏳  | ⏳  | ⏳  | ⚠️ 
| ReturnCommand  | ✅  | ✅  | ✅  | ⚠️ 
| ReturnCommandParameter  | ✅  | ✅  | ✅  | ⚠️ 
| ReturnType  | ✅  | ✅  | ✅  | ⚠️ 
| SelectionLength  | ✅  | ✅  | ⚠️  |  ✅  
| Text  | ✅  | ✅  | ✅  |  ✅  
| TextColor  | ✅  | ✅  | ✅  |  ✅  

### ⚠️ Frame

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| BorderColor  | ⏳  | ⏳  | ⚠️  |  ⚠️  | 
| CornerRadius  | ⏳  | ⏳  | ⚠️  |  ⚠️  | 
| HasShadow  | ⏳  | ⏳  | ⚠️  |  ⚠️  | 

<!--
### ⚠️ IndicatorView

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| IndicatorColor  | ⚠️  | ⚠️  | ⚠️  | 
| IndicatorLayout  | ⚠️  | ⚠️  | ⚠️  | 
| IndicatorSize  | ⚠️  | ⚠️  | ⚠️  | 
| IndicatorShape  | ⚠️  | ⚠️  | ⚠️  | 
| IndicatorTemplate  | ⚠️  | ⚠️  | ⚠️  | 
| ItemsSource  | ⚠️  | ⚠️  | ⚠️  | 
| MinimumVisible  | ⚠️  | ⚠️  | ⚠️  | 
| Position  | ⚠️  | ⚠️  | ⚠️  | 
| SelectedIndicatorColor  | ⚠️  | ⚠️  | ⚠️  | 
-->

### ✅ Image

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| Aspect  | ✅  | ✅  | ✅  |  ⚠️  | 
| IsLoading  | ✅  | ✅  | ✅  |  ✅  | 
| Source  | ✅  | ✅  | ✅  |  ✅  | 

<!--
### ⚠️ ImageButton

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| BorderColor  | ⚠️  | ⚠️  | ⚠️  | 
| BorderWidth  | ⚠️  | ⚠️  | ⚠️  | 
| Command  | ⚠️  | ⚠️  | ⚠️  | 
| CommandParameter  | ⚠️  | ⚠️  | ⚠️  | 
| CornerRadius  | ⚠️  | ⚠️  | ⚠️  | 
| IsLoading  | ⚠️  | ⚠️  | ⚠️  | 
| IsOpaque  | ⚠️  | ⚠️  | ⚠️  | 
| IsPressed  | ⚠️  | ⚠️  | ⚠️  | 
| Padding  | ⚠️  | ⚠️  | ⚠️  | 
| Source  | ⚠️  | ⚠️  | ⚠️  | 
| Clicked  | ⚠️  | ⚠️  | ⚠️  | 
| Pressed  | ⚠️  | ⚠️  | ⚠️  | 
| Released  | ⚠️  | ⚠️  | ⚠️  | 
-->

### ⚠️ Label

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| CharacterSpacing  | ✅  | ✅  | ✅  |  ✅  | 
| Font  | ✅  | ✅  | ✅  |  ✅  | 
| FontAttributes  | ✅  | ✅  | ✅  |  ✅  | 
| FontFamily  | ✅  | ✅  | ✅  |  ✅  | 
| FontSize  | ✅  | ✅  | ✅  |  ✅  | 
| FormattedText  | ✅  | ✅  | ⚠️  | ⚠️ |
| HorizontalTextAlignment  | ✅  | ✅  | ✅  |  ✅  | 
| LineBreakMode  | ✅  | ✅  | ✅  | ✅  | 
| LineHeight  | ✅  | ✅  | ✅  |  ✅  | 
| MaxLines  | ✅  | ✅  | ✅  |  ✅  | 
| Padding  | ✅  | ✅  | ✅  |  ✅  | 
| Text  | ✅  | ✅  | ✅  |  ✅  | 
| TextColor  | ✅  | ✅  | ✅  |  ✅  | 
| TextDecorations  | ✅  | ✅  | ✅  |  ✅  | 
| TextType  | ⏳  | ⏳  | ⏳  | ⚠️  | 
| VerticalTextAlignment  | ⚠️  | ⚠️  | ⏳  |   ✅  | 

<!--
### ⚠️ Map

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| HasScrollEnabled  | ⚠️  | ⚠️  | ⚠️  | 
| HasZoomEnabled  | ⚠️  | ⚠️  | ⚠️  | 
| IsShowingUser  | ⚠️  | ⚠️  | ⚠️  | 
| ItemsSource  | ⚠️  | ⚠️  | ⚠️  | 
| ItemTemplate  | ⚠️  | ⚠️  | ⚠️  | 
| ItemTemplateSelector  | ⚠️  | ⚠️  | ⚠️  | 
| LastMoveToRegion  | ⚠️  | ⚠️  | ⚠️  | 
| MapType  | ⚠️  | ⚠️  | ⚠️  | 
| Pins  | ⚠️  | ⚠️  | ⚠️  | 
| TrafficEnabled  | ⚠️  | ⚠️  | ⚠️  | 
| VisibleRegion  | ⚠️  | ⚠️  | ⚠️  | 
| MoveToRegion  | ⚠️  | ⚠️  | ⚠️  | 
| MapClicked  | ⚠️  | ⚠️  | ⚠️  | 
-->

### ⚠️ Picker

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| CharacterSpacing  | ✅  | ✅  | ✅  | ✅  | 
| FontAttributes  | ✅  | ✅  | ✅  | ✅  | 
| FontFamily  | ✅  | ✅  | ✅  | ✅  | 
| FontSize  | ✅  | ✅  | ✅  | ✅  | 
| HorizontalTextAlignment  | ✅  | ✅  | ✅  | ✅  | 
| ItemDisplayBinding  | ✅  | ✅  | ✅  |   ✅   | 
| Items  | ✅  | ✅  | ✅  | ✅
| ItemsSource  | ✅  | ✅  | ✅  | ✅
| SelectedIndex  | ✅  | ✅  | ✅  | ✅
| SelectedIndexChanged  | ✅  | ✅  | ✅  | ✅ | 
| SelectedItem  | ✅  | ✅  | ⚠️  | ✅
| TextColor  | ✅  | ✅  | ⏳  | ✅
| Title  | ✅  | ✅  | ✅  | ⚠️ 
| TitleColor  | ✅  | ✅  | ✅  | ⚠️ 
| VerticalTextAlignment  | ⏳  | ⏳  | ⏳  | ⚠️ 

### ⚠️ ProgressBar

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:| 
| Progress  | ✅  | ✅  | ✅  |  ✅  |
| ProgressColor  | ⏳  | ⏳  | ⏳  |  ✅  |
| ProgressTo  | ✅  | ✅  | ✅  |  ✅  |

### ⚠️ RadioButton

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| CheckedChanged  | ⚠️  | ⚠️  | ⚠️  | ⚠️  | 
| GroupName  | ⚠️  | ⚠️  | ⚠️  | ⚠️  | 
| IsChecked  | ⏳  | ⏳  | ⚠️  | ⚠️  | 

<!--
### ⚠️ RefreshView

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| Command  | ⚠️  | ⚠️  | ⚠️  | 
| CommandParameter  | ⚠️  | ⚠️  | ⚠️  | 
| IsRefreshing  | ⚠️  | ⚠️  | ⚠️  | 
| RefreshColor  | ⚠️  | ⚠️  | ⚠️  | 
| Refreshing  | ⚠️  | ⚠️  | ⚠️  | 
-->

### ⚠️ SearchBar

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| BackgroundColor  | ✅  | ✅  | ✅  |  ✅  | 
| CharacterSpacing  | ✅  | ✅  | ✅  | ✅  | 
| CancelButtonColor  | ⏳  | ⏳  | ✅  |  ⚠️ |
| FontAttributes  | ✅  | ✅  | ⏳  |  ✅  |
| FontSize  | ✅  | ✅  | ⏳  |  ✅  |
| HorizontalTextAlignment  | ✅  | ✅  | ✅  | ✅  |
| MaxLength  | ✅  | ✅  | ⏳ |  ✅  |
| SearchCommand  | ⏳  | ✅  | ✅  |  ⚠️ |
| SearchCommandParameter  | ⏳  | ✅  | ✅  | ⚠️ |
| Text  | ✅  | ✅  | ✅  |  ✅  |
| TextColor  | ✅  | ✅  | ⏳  | ✅  |
| VerticalTextAlignment  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 

### ✅ Shapes

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| Fill  | ✅  | ✅  | ✅  |  ✅  |
| Stroke  | ✅  | ✅  | ✅  |  ✅  |
| StrokeDashArray  | ✅  | ✅  | ✅  | ✅  
| StrokeDashOffset  | ✅  | ✅  | ✅  | ✅ 
| StrokeLineCap  | ✅  | ✅  | ✅  | ✅ 
| StrokeLineJoin  | ✅  | ✅  | ✅  | ✅ 
| StrokeMiterLimit  | ✅  | ✅  | ✅  | ✅ 
| StrokeThickness  | ✅  | ✅  | ✅  | ✅  | 

### ⚠️ Slider

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| DragCompleted  | ✅  | ✅  | ✅  |   ⚠️ |
| DragCompletedCommand  | ✅  | ✅  | ✅  |  ⚠️ |
| DragStarted  | ✅  | ✅  | ✅  |  ⚠️ |
| DragStartedCommand  | ✅  | ✅  | ✅  |  ⚠️ |
| Maximum  | ✅  | ✅  | ✅  | ✅  | 
| MaximumTrackColor  | ✅  | ✅  | ✅  |  ⚠️ |
| Minimum  | ✅  | ✅  | ✅  |  ✅  | 
| MinimumTrackColor  | ✅  | ✅  | ✅  |  ⚠️ |
| ThumbColor  | ✅  | ✅  | ⏳  |  ✅  | 
| ThumbImageSource  | ⏳  | ⏳  | ✅  |  ✅  | 
| Value  | ✅  | ✅  | ✅  |  ✅  |  ✅  | 
| ValueChanged  | ✅  | ✅  | ✅  |  ✅  | 

### ✅ Stepper

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| Increment  | ✅  | ✅  | ✅  |  ✅  | 
| Maximum  | ✅  | ✅  | ✅  |  ✅  | 
| Minimum  | ✅  | ✅  | ✅  |  ✅  | 
| Value  | ✅  | ✅  | ✅  |  ✅  | 
| ValueChanged  | ✅  | ✅  | ✅  |  ✅  | 

<!--
### ⚠️ SwipeView

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| BottomItems  | ⚠️  | ⚠️  | ⚠️  | 
| LeftItems  | ⚠️  | ⚠️  | ⚠️  | 
| RightItems  | ⚠️  | ⚠️  | ⚠️  | 
| TopItems  | ⚠️  | ⚠️  | ⚠️  | 
-->

### ⚠️ Switch

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| IsToggled  | ✅  | ✅  | ✅  |  ✅  | 
| OnColor  | ✅  | ✅  | ⏳  |  ⚠️  | 
| ThumbColor  | ✅  | ✅  | ⏳  |  ⚠️  | 

### ✅ TimePicker

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| BackgroundColor  | ✅  | ✅  | ✅  |  ⚠️  | 
| CharacterSpacing  | ✅  | ✅  | ✅  |  ⚠️  | 
| FontAttributes  | ✅  | ✅  | ✅  |  ⚠️  | 
| FontFamily  | ✅  | ✅  | ✅  |  ⚠️  | 
| FontSize  | ✅  | ✅  | ✅  |  ⚠️  | 
| Format  | ✅  | ✅  | ✅  |  ⚠️  | 
| Time  | ✅  | ✅  | ✅  |  ⚠️  | 
| TextColor  | ✅  | ✅  | ✅  |  ⚠️  | 

### ⚠️ WebView

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| CanGoBack  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 
| CanGoForward  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 
| Cookies  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 
| Source  | ⏳  | ⏳  | ⏳  |  ⚠️  | 
| Eval  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 
| EvaluateJavaScriptAsync  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 
| GoBack  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 
| GoForward  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 
| Reload  | ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 

### Renderer Based Views

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| CarouselView | ✅  | ✅  | ⚠️  |  ⚠️  | 
| CollectionView | ✅  | ✅  | ⚠️  |  ⚠️  | 
| IndicatorView| ⏳  | ✅  | ⚠️  |  ⚠️  | 
| ImageButton| ✅  | ✅  | ✅  |  ⚠️  | 
| Map | ⏳  | ⏳  | ⚠️  |  ⚠️  | 
| RefreshView| ⚠️  | ⚠️  | ⚠️  |  ⚠️  | 
| SwipeView| ✅  | ✅  | ⚠️  |  ⚠️  | 

### Layouts

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| AbsoluteLayout | ✅  | ✅  | ✅  | ⚠️ |
| ContentPresenter | ⚠️  | ⚠️  | ⚠️  | ⚠️  |
| ContentView | ⚠️  | ⚠️  | ⚠️  | ⚠️  |
| FlexLayout | ✅  | ✅  | ✅  | ✅  |
| Grid | ✅  | ✅  | ✅  | ✅  |
| RelativeLayout | ✅  | ✅  | ✅  | ⚠️ |
| ScrollView | ✅  | ✅  | ✅  | ✅  |
| StackLayout | ✅  | ✅  | ✅  | ✅  |
| TemplatedView | ⚠️  | ⚠️  | ⚠️  | ⚠️ |

### Features

| API | Android | iOS / Mac Catalyst | Windows | Gtk |
| ----|:-------:|:------------------:|:-------:|:----:|
| Accessibility | ✅  | ✅  | ✅  | ⚠️  |
| Animation | ✅  | ✅  | ✅  | ⚠️  |
| Border Everywhere | ⏳  | ⏳  | ⏳  | ⚠️  |
| Brushes Everywhere | ✅  | ✅  | ✅  | ⚠️  |
| CornerRadius Everywhere | ⏳  | ⏳  | ⏳  | ⚠️  |
| Device | ⚠️  | ⚠️  | ⚠️  | ⚠️  |
| Gestures | ✅  | ✅  | ✅  | ⚠️  |
| ImageHandlers | ✅  | ✅  | ✅  | ✅  |
| Interactivity (Behaviors, Triggers, Visual State Manager) | ✅  | ✅  | ✅  | ⚠️  |
| FlowDirection (RTL) | ⏳  | ⏳  | ⏳  | ⚠️  |
| Fonts | ✅  | ✅  | ✅  | ✅  |
| Lifecycle Events | ⏳  | ⏳  | ⏳  | ⏳  |
| Themes | ⏳  | ⏳  | ⚠️  | ⚠️  |
| Shell | ⏳  | ⏳  | ⏳  | ⚠️  |
| Styles | ✅  | ✅  | ✅  | ⚠️  |
| View Transforms | ✅  | ✅  | ✅  | ⚠️  |

