using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public partial class TitleBar : TemplatedView, ITitleBar
	{
		public const string TemplateRootName = "PART_Root";

		public const string TitleBarIcon = "PART_Icon";
		public const string IconVisibleState = "IconVisible";
		public const string IconHiddenState = "IconCollapsed";

		public const string TitleBarTitle = "PART_Title";
		public const string TitleVisibleState = "TitleVisible";
		public const string TitleHiddenState = "TitleCollapsed";

		public const string TitleBarSubtitle = "PART_Subtitle";
		public const string SubtitleVisibleState = "SubtitleVisible";
		public const string SubtitleHiddenState = "SubTitleCollapsed";

		public const string TitleBarLeading = "PART_LeadingContent";
		public const string LeadingVisibleState = "LeadingContentVisible";
		public const string LeadingHiddenState = "LeadingContentCollapsed";

		public const string TitleBarContent = "PART_Content";
		public const string ContentVisibleState = "ContentVisible";
		public const string ContentHiddenState = "ContentCollapsed";

		public const string TitleBarTrailing = "PART_TrailingContent";
		public const string TrailingVisibleState = "TrailingContentVisible";
		public const string TrailingHiddenState = "TrailingContentCollapsed";

		public const string TitleBarActiveState = "TitleBarTitleActive";
		public const string TitleBarInactiveState = "TitleBarTitleInactive";

		public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(ImageSource),
			typeof(TitleBar), null, propertyChanged: OnIconChanged);

		public static readonly BindableProperty LeadingContentProperty = BindableProperty.Create(nameof(LeadingContent), typeof(IView),
			typeof(TitleBar), null, propertyChanged: OnLeadingChanged);

		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(IView),
			typeof(TitleBar), null, propertyChanged: OnContentChanged);

		public static readonly BindableProperty TrailingContentProperty = BindableProperty.Create(nameof(TrailingContent), typeof(IView),
			typeof(TitleBar), null, propertyChanged: OnTrailingContentChanged);

		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string),
			typeof(TitleBar), null, propertyChanged: OnTitleChanged);

		public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(nameof(Subtitle), typeof(string),
			typeof(TitleBar), null, propertyChanged: OnSubtitleChanged);

		public static readonly BindableProperty ForegroundColorProperty = BindableProperty.Create(nameof(ForegroundColor),
			typeof(Color), typeof(TitleBar));

		public static readonly BindableProperty InactiveBackgroundColorProperty = BindableProperty.Create(nameof(InactiveBackgroundColor),
			typeof(Color), typeof(TitleBar), null);

		public static readonly BindableProperty InactiveForegroundColorProperty = BindableProperty.Create(nameof(InactiveForegroundColor),
			typeof(Color), typeof(TitleBar), null);

		static void OnLeadingChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				VisualStateManager.GoToState(titlebar._templateRoot, LeadingHiddenState);
			}
			else
			{
				VisualStateManager.GoToState(titlebar._templateRoot, LeadingVisibleState);
			}
		}

		static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			var imageSource = newValue as ImageSource;
			if (imageSource is null || imageSource.IsEmpty)
			{
				VisualStateManager.GoToState(titlebar._templateRoot, IconHiddenState);
			}
			else
			{
				VisualStateManager.GoToState(titlebar._templateRoot, IconVisibleState);
			}
		}

		static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				VisualStateManager.GoToState(titlebar._templateRoot, TitleHiddenState);
			}
			else
			{
				VisualStateManager.GoToState(titlebar._templateRoot, TitleVisibleState);
			}
		}

		static void OnSubtitleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				VisualStateManager.GoToState(titlebar._templateRoot, SubtitleHiddenState);
			}
			else
			{
				VisualStateManager.GoToState(titlebar._templateRoot, SubtitleVisibleState);
			}
		}

		static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				VisualStateManager.GoToState(titlebar._templateRoot, ContentHiddenState);
			}
			else
			{
				VisualStateManager.GoToState(titlebar._templateRoot, ContentVisibleState);
			}
		}

		static void OnTrailingContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				VisualStateManager.GoToState(titlebar._templateRoot, TrailingHiddenState);
			}
			else
			{
				VisualStateManager.GoToState(titlebar._templateRoot, TrailingVisibleState);
			}
		}

		public ImageSource Icon
		{
			get { return (ImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		public IView? LeadingContent
		{
			get { return (View?)GetValue(LeadingContentProperty); }
			set { SetValue(LeadingContentProperty, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public string Subtitle
		{
			get { return (string)GetValue(SubtitleProperty); }
			set { SetValue(SubtitleProperty, value); }
		}

		public IView? Content
		{
			get { return (View?)GetValue(TitleBar.ContentProperty); }
			set { SetValue(TitleBar.ContentProperty, value); }
		}

		public IView? TrailingContent
		{
			get { return (View?)GetValue(TrailingContentProperty); }
			set { SetValue(TrailingContentProperty, value); }
		}

		public Color ForegroundColor
		{
			get { return (Color)GetValue(ForegroundColorProperty); }
			set { SetValue(ForegroundColorProperty, value); }
		}

		public Color InactiveBackgroundColor
		{
			get { return (Color)GetValue(InactiveBackgroundColorProperty); }
			set { SetValue(InactiveBackgroundColorProperty, value); }
		}

		public Color InactiveForegroundColor
		{
			get { return (Color)GetValue(InactiveForegroundColorProperty); }
			set { SetValue(InactiveForegroundColorProperty, value); }
		}

		public IList<IView> PassthroughElements { get; private set; }

#nullable disable
		IView IContentView.PresentedContent => (this as IControlTemplated).TemplateRoot as IView;
#nullable enable

		static ControlTemplate? _defaultTemplate;
		View? _templateRoot;
		View? _titleLabel;
		Color? _backgroundColor;

		static Color TextFillColorPrimaryLight = new(0, 0, 0, 228);
		static Color TextFillInactiveColorPrimaryLight = new(0, 0, 0, 135);

		static Color TextFillColorPrimaryDark = new(255, 255, 255, 255);
		static Color TextFillInactiveColorPrimaryDark = new(255, 255, 255, 114);

		public TitleBar()
		{
			PassthroughElements = new List<IView>();
			ControlTemplate = DefaultTemplate;
		}

		public static ControlTemplate DefaultTemplate
		{
			get
			{
				if (_defaultTemplate == null)
				{
					_defaultTemplate = new ControlTemplate(() => BuildDefaultTemplate());
				}

				return _defaultTemplate;
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			var controlTemplate = (this as IControlTemplated);

			_templateRoot = controlTemplate?.TemplateRoot as View;

			_titleLabel = controlTemplate?.GetTemplateChild(TitleBarTitle) as View;

			var leadingContent = controlTemplate?.GetTemplateChild(TitleBarLeading) as IView;
			if (leadingContent is not null)
			{
				PassthroughElements.Add(leadingContent);
			}

			var content = controlTemplate?.GetTemplateChild(TitleBarContent) as IView;
			if (content is not null)
			{
				PassthroughElements.Add(content);
			}

			var trailingContent = controlTemplate?.GetTemplateChild(TitleBarTrailing) as IView;
			if (trailingContent is not null)
			{
				PassthroughElements.Add(trailingContent);
			}

			VisualStateManager.GoToState(_templateRoot, TitleBarActiveState);
		}

		internal void NotifyWindowReady(Window window)
		{
			window.Activated += Window_Activated;
			window.Deactivated += Window_Deactivated;
		}

		internal void UnhookWindowEvents(Window window)
		{
			window.Activated -= Window_Activated;
			window.Deactivated -= Window_Deactivated;
		}

		private void Window_Activated(object? sender, System.EventArgs e)
		{
			// Restore the color
			if (_backgroundColor is not null && _templateRoot is not null)
			{
				BackgroundColor = _backgroundColor;
			}

			VisualStateManager.GoToState(_templateRoot, TitleBarActiveState);
		}

		private void Window_Deactivated(object? sender, System.EventArgs e)
		{
			// Save color
			if (BackgroundColor is not null)
			{
				_backgroundColor = BackgroundColor;
			}

			// Set to inactive color
			if (InactiveBackgroundColor is not null && _templateRoot is not null)
			{
				BackgroundColor = InactiveBackgroundColor;
			}

			VisualStateManager.GoToState(_templateRoot, TitleBarInactiveState);
		}

		static View BuildDefaultTemplate()
		{
			VisualStateGroupList visualStateGroups = new VisualStateGroupList();

			#region Root Grid
			var contentGrid = new Grid()
			{
				HorizontalOptions = LayoutOptions.Fill,
				ColumnDefinitions =
				{
					new ColumnDefinition(GridLength.Auto), // Leading content
					new ColumnDefinition(GridLength.Auto), // Icon content
					new ColumnDefinition(GridLength.Auto), // Title content
					new ColumnDefinition(GridLength.Auto), // Subtitle content
					new ColumnDefinition(GridLength.Star), // Content
					new ColumnDefinition(GridLength.Auto), // Trailing content
					new ColumnDefinition(150),			   // Min drag region + padding for system buttons
				}
			};
			BindToTemplatedParent(contentGrid, BackgroundColorProperty, OpacityProperty);
			#endregion

			#region Leading content
			var leadingContent = new ContentPresenter()
			{
				IsVisible = false
			};

			contentGrid.Add(leadingContent);
			contentGrid.SetColumn(leadingContent, 0);

			leadingContent.SetBinding(ContentPresenter.ContentProperty,
				new Binding(LeadingContentProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			var leadingVisibleGroup = GetVisibleStateGroup(TitleBarLeading, LeadingVisibleState, LeadingHiddenState);
			leadingVisibleGroup.Name = "LeadingContentGroup";
			visualStateGroups.Add(leadingVisibleGroup);
			#endregion

			#region Icon
			var icon = new Image()
			{
				WidthRequest = 20,
				HeightRequest = 20,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(16, 0, 0, 0),
				IsVisible = false,
			};

			contentGrid.Add(icon);
			contentGrid.SetColumn(icon, 1);

			icon.SetBinding(Image.SourceProperty,
				new Binding(IconProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			var iconVisibleGroup = GetVisibleStateGroup(TitleBarIcon, IconVisibleState, IconHiddenState);
			iconVisibleGroup.Name = "IconGroup";
			visualStateGroups.Add(iconVisibleGroup);
			#endregion

			#region Title
			var titleLabel = new Label()
			{
				Margin = new Thickness(16, 0),
				LineBreakMode = LineBreakMode.NoWrap,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
				MinimumWidthRequest = 48,
				FontSize = 12,
				IsVisible = false
			};
			
			contentGrid.Add(titleLabel);
			contentGrid.SetColumn(titleLabel, 2);

			titleLabel.SetBinding(Label.TextProperty,
				new Binding(TitleProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			titleLabel.SetBinding(Label.TextColorProperty,
				new Binding(ForegroundColorProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			var activeVisualState = new VisualState() { Name = TitleBarActiveState };
			activeVisualState.Setters.Add(
				new Setter()
				{
					Property = Label.OpacityProperty,
					TargetName = TitleBarTitle,
					Value = 1.0
				});

			var inactiveVisualState = new VisualState() { Name = TitleBarInactiveState };
			inactiveVisualState.Setters.Add(
				new Setter()
				{
					Property = Label.OpacityProperty,
					TargetName = TitleBarTitle,
					Value = 0.7
				});

			var labelActiveStateGroup = new VisualStateGroup() { Name = "TitleActiveStates" };
			labelActiveStateGroup.States.Add(activeVisualState);
			labelActiveStateGroup.States.Add(inactiveVisualState);
			visualStateGroups.Add(labelActiveStateGroup);

			var titleVisibleGroup = GetVisibleStateGroup(TitleBarTitle, TitleVisibleState, TitleHiddenState);
			titleVisibleGroup.Name = "TitleTextGroup";
			visualStateGroups.Add(titleVisibleGroup);
			#endregion

			#region Subtitle
			var subtitleLabel = new Label()
			{
				Margin = new Thickness(0, 0, 16, 0),
				LineBreakMode = LineBreakMode.NoWrap,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
				MinimumWidthRequest = 48,
				FontSize = 12,
				Opacity = 0.7,
				IsVisible = false
			};

			contentGrid.Add(subtitleLabel);
			contentGrid.SetColumn(subtitleLabel, 3);

			subtitleLabel.SetBinding(Label.TextProperty,
				new Binding(SubtitleProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			subtitleLabel.SetBinding(Label.TextColorProperty,
				new Binding(ForegroundColorProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			var subtitleVisibleGroup = GetVisibleStateGroup(TitleBarSubtitle, SubtitleVisibleState, SubtitleHiddenState);
			subtitleVisibleGroup.Name = "SubtitleTextGroup";
			visualStateGroups.Add(subtitleVisibleGroup);
			#endregion

			#region Content
			var content = new ContentPresenter()
			{
				IsVisible = false
			};

			contentGrid.Add(content);
			contentGrid.SetColumn(content, 4);

			content.SetBinding(ContentPresenter.ContentProperty,
				new Binding(ContentProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			var contentVisibleGroup = GetVisibleStateGroup(TitleBarContent, ContentVisibleState, ContentHiddenState);
			contentVisibleGroup.Name = "ContentGroup";
			visualStateGroups.Add(contentVisibleGroup);
			#endregion

			#region Trailing content
			var trailingContent = new ContentPresenter()
			{
				IsVisible = false
			};

			contentGrid.Add(trailingContent);
			contentGrid.SetColumn(trailingContent, 5);

			trailingContent.SetBinding(ContentPresenter.ContentProperty,
				new Binding(TrailingContentProperty.PropertyName,
							source: RelativeBindingSource.TemplatedParent));

			var trailingContentVisibleGroup = GetVisibleStateGroup(TitleBarTrailing, TrailingVisibleState, TrailingHiddenState);
			trailingContentVisibleGroup.Name = "TrailingContentGroup";
			visualStateGroups.Add(trailingContentVisibleGroup);
			#endregion

			INameScope nameScope = new NameScope();
			NameScope.SetNameScope(contentGrid, nameScope);
			nameScope.RegisterName(TemplateRootName, contentGrid);
			nameScope.RegisterName(TitleBarLeading, leadingContent);
			nameScope.RegisterName(TitleBarIcon, icon);
			nameScope.RegisterName(TitleBarTitle, titleLabel);
			nameScope.RegisterName(TitleBarSubtitle, subtitleLabel);
			nameScope.RegisterName(TitleBarContent, content);
			nameScope.RegisterName(TitleBarTrailing, trailingContent);

			VisualStateManager.SetVisualStateGroups(contentGrid, visualStateGroups);

			return contentGrid;
		}

		static VisualStateGroup GetVisibleStateGroup(string targetName, string visibleState, string hiddenState)
		{
			var visualGroup = new VisualStateGroup();
			var visualVisibleState = new VisualState() { Name = visibleState };
			visualVisibleState.Setters.Add(
				new Setter()
				{
					Property = IsVisibleProperty,
					TargetName = targetName,
					Value = true
				});
			visualGroup.States.Add(visualVisibleState);

			var collapsedState = new VisualState() { Name = hiddenState };
			collapsedState.Setters.Add(
				new Setter()
				{
					Property = IsVisibleProperty,
					TargetName = targetName,
					Value = false
				});
			visualGroup.States.Add(collapsedState);
			return visualGroup;
		}

		static void BindToTemplatedParent(BindableObject bindableObject, params BindableProperty[] properties)
		{
			foreach (var property in properties)
			{
				bindableObject.SetBinding(property, new Binding(property.PropertyName,
					source: RelativeBindingSource.TemplatedParent));
			}
		}
	}
}
