using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A <see cref="View"/> control that provides title bar functionality for a window.
	/// <br/><br/>
	/// The standard title bar height is 32px, but can be set to a larger value.
	/// <br/><br/>
	/// The title bar can also be hidden by setting the <see cref="VisualElement.IsVisible"/> property, which
	/// will cause the window content to be displayed in the title bar region.
	/// </summary>
	public partial class TitleBar : TemplatedView, ITitleBar, ISafeAreaView
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
		public const string SubtitleHiddenState = "SubtitleCollapsed";

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

		/// <summary>Bindable property for <see cref="Icon"/>.</summary>
		public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(ImageSource),
			typeof(TitleBar), null, propertyChanged: OnIconChanged);

		/// <summary>Bindable property for <see cref="LeadingContent"/>.</summary>
		public static readonly BindableProperty LeadingContentProperty = BindableProperty.Create(nameof(LeadingContent), typeof(IView),
			typeof(TitleBar), null, propertyChanged: OnLeadingChanged);

		/// <summary>Bindable property for <see cref="Content"/>.</summary>
		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(IView),
			typeof(TitleBar), null, propertyChanged: OnContentChanged);

		/// <summary>Bindable property for <see cref="TrailingContent"/>.</summary>
		public static readonly BindableProperty TrailingContentProperty = BindableProperty.Create(nameof(TrailingContent), typeof(IView),
			typeof(TitleBar), null, propertyChanged: OnTrailingContentChanged);

		/// <summary>Bindable property for <see cref="Title"/>.</summary>
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string),
			typeof(TitleBar), null, propertyChanged: OnTitleChanged);

		/// <summary>Bindable property for <see cref="Subtitle"/>.</summary>
		public static readonly BindableProperty SubtitleProperty = BindableProperty.Create(nameof(Subtitle), typeof(string),
			typeof(TitleBar), null, propertyChanged: OnSubtitleChanged);

		/// <summary>Bindable property for <see cref="ForegroundColor"/>.</summary>
		public static readonly BindableProperty ForegroundColorProperty = BindableProperty.Create(nameof(ForegroundColor),
			typeof(Color), typeof(TitleBar));

		static void OnLeadingChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				titlebar.ApplyVisibleState(LeadingHiddenState);
			}
			else
			{
				titlebar.ApplyVisibleState(LeadingVisibleState);
			}
		}

		static void OnIconChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			var imageSource = newValue as ImageSource;
			if (imageSource is null || imageSource.IsEmpty)
			{
				titlebar.ApplyVisibleState(IconHiddenState);
			}
			else
			{
				titlebar.ApplyVisibleState(IconVisibleState);
			}
		}

		static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				titlebar.ApplyVisibleState(TitleHiddenState);
			}
			else
			{
				titlebar.ApplyVisibleState(TitleVisibleState);
			}
		}

		static void OnSubtitleChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				titlebar.ApplyVisibleState(SubtitleHiddenState);
			}
			else
			{
				titlebar.ApplyVisibleState(SubtitleVisibleState);
			}
		}

		static void OnContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				titlebar.ApplyVisibleState(ContentHiddenState);
			}
			else
			{
				titlebar.ApplyVisibleState(ContentVisibleState);
			}
		}

		static void OnTrailingContentChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var titlebar = (TitleBar)bindable;
			if (newValue is null)
			{
				titlebar.ApplyVisibleState(TrailingHiddenState);
			}
			else
			{
				titlebar.ApplyVisibleState(TrailingVisibleState);
			}
		}

		/// <summary>
		/// Gets or sets an optional icon image of the title bar. Icon images should be
		/// 16x16 pixels in size.
		/// <br/><br/>
		/// Setting this property to an empty value will hide the icon.
		/// </summary>
		public ImageSource Icon
		{
			get { return (ImageSource)GetValue(IconProperty); }
			set { SetValue(IconProperty, value); }
		}

		/// <summary>
		/// Gets or sets a <see cref="View"/> control that represents the leading content.<br/><br/>
		/// The leading content follows the optional <see cref="Icon"/> and is aligned to
		/// the left or right of the title bar, depending on the <see cref="FlowDirection"/>. Views
		/// set here will be allocated as much space as they require.
		/// <br/><br/>
		/// Views set here will block all input to the title bar region and handle input directly.
		/// </summary>
		public IView? LeadingContent
		{
			get { return (View?)GetValue(LeadingContentProperty); }
			set { SetValue(LeadingContentProperty, value); }
		}

		/// <summary>
		/// Gets or sets the title text of the title bar. The title usually specifies
		/// the name of the application or indicates the purpose of the window
		/// </summary>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <summary>
		/// Gets or sets the subtitle text of the title bar. The subtitle usually specifies
		/// the secondary information about the application or window
		/// </summary>
		public string Subtitle
		{
			get { return (string)GetValue(SubtitleProperty); }
			set { SetValue(SubtitleProperty, value); }
		}

		/// <summary>
		/// Gets or sets a <see cref="View"/> control that represents the content.<br/><br/>
		/// This content is centered in the title bar, and is allocated the remaining space
		/// between the leading and trailing content.<br/>
		/// <br/><br/>
		/// Views set here will block all input to the title bar region and handle input directly.
		/// </summary>
		public IView? Content
		{
			get { return (View?)GetValue(TitleBar.ContentProperty); }
			set { SetValue(TitleBar.ContentProperty, value); }
		}

		/// <summary>
		/// Gets or sets a <see cref="View"/> control that represents the trailing content.<br/><br/>
		/// The trailing content is aligned to the right or left of the title bar, depending 
		/// on the <see cref="FlowDirection"/>. Views set here will be allocated as much space 
		/// as they require.
		/// <br/><br/>
		/// Views set here will block all input to the title bar region and handle input directly.
		/// </summary>
		public IView? TrailingContent
		{
			get { return (View?)GetValue(TrailingContentProperty); }
			set { SetValue(TrailingContentProperty, value); }
		}

		/// <summary>
		/// Gets or sets the foreground color of the title bar. This color is used for the 
		/// title and subtitle text.
		/// </summary>
		public Color ForegroundColor
		{
			get { return (Color)GetValue(ForegroundColorProperty); }
			set { SetValue(ForegroundColorProperty, value); }
		}

		/// <inheritdoc/>
		public IList<IView> PassthroughElements { get; private set; }

		bool ISafeAreaView.IgnoreSafeArea => true;

#nullable disable
		IView IContentView.PresentedContent => (this as IControlTemplated).TemplateRoot as IView;
#nullable enable

		static ControlTemplate? _defaultTemplate;
		View? _templateRoot;

		public TitleBar()
		{
			PassthroughElements = new List<IView>();
			PropertyChanged += TitleBar_PropertyChanged;

			if (ControlTemplate is null)
			{
				ControlTemplate = DefaultTemplate;
			}
		}

		private void TitleBar_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Window) && Window is not null)
			{
				Window.Activated -= Window_Activated;
				Window.Deactivated -= Window_Deactivated;

				Window.Activated += Window_Activated;
				Window.Deactivated += Window_Deactivated;
			}
		}

		internal void ApplyVisibleState(string stateGroup)
		{
			VisualStateManager.GoToState(this, stateGroup);

			if (_templateRoot is not null)
			{
				VisualStateManager.GoToState(_templateRoot, stateGroup);
			}
		}

		/// <summary>
		/// Gets the default template for the title bar
		/// </summary>
		public static ControlTemplate DefaultTemplate
		{
			get
			{
				_defaultTemplate ??= new ControlTemplate(() => BuildDefaultTemplate());

				return _defaultTemplate;
			}
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			var controlTemplate = (this as IControlTemplated);

			_templateRoot = controlTemplate?.TemplateRoot as View;

			if (controlTemplate?.GetTemplateChild(TitleBarLeading) is IView leadingContent)
			{
				PassthroughElements.Add(leadingContent);
			}

			if (controlTemplate?.GetTemplateChild(TitleBarContent) is IView content)
			{
				PassthroughElements.Add(content);
			}

			if (controlTemplate?.GetTemplateChild(TitleBarTrailing) is IView trailingContent)
			{
				PassthroughElements.Add(trailingContent);
			}

			ApplyVisibleState(TitleBarActiveState);
		}

		private void Window_Activated(object? sender, System.EventArgs e)
		{
			ApplyVisibleState(TitleBarActiveState);
		}

		private void Window_Deactivated(object? sender, System.EventArgs e)
		{
			ApplyVisibleState(TitleBarInactiveState);
		}

		static View BuildDefaultTemplate()
		{
			VisualStateGroupList visualStateGroups = new VisualStateGroupList();

			#region Root Grid
			var contentGrid = new Grid()
			{
#if MACCATALYST
				Margin = new Thickness(80, 0, 0, 0),
#endif
				HorizontalOptions = LayoutOptions.Fill,
				ColumnDefinitions =
				{
					new ColumnDefinition(GridLength.Auto), // Leading content
					new ColumnDefinition(GridLength.Auto), // Icon content
					new ColumnDefinition(GridLength.Auto), // Title content
					new ColumnDefinition(GridLength.Auto), // Subtitle content
					new ColumnDefinition(GridLength.Star), // Content
					new ColumnDefinition(GridLength.Auto), // Trailing content
#if !MACCATALYST
					new ColumnDefinition(150),             // Min drag region + padding for system buttons
#endif
				}
			};

			contentGrid.SetBinding(
				BackgroundColorProperty,
				static (TitleBar tb) => tb.BackgroundColor,
				source: RelativeBindingSource.TemplatedParent);

			contentGrid.SetBinding(
				BackgroundProperty,
				static (TitleBar tb) => tb.Background,
				source: RelativeBindingSource.TemplatedParent);

			contentGrid.SetBinding(
				OpacityProperty,
				static (TitleBar tb) => tb.Opacity,
				source: RelativeBindingSource.TemplatedParent);
			#endregion

			#region Leading content
			var leadingContent = new ContentView()
			{
				IsVisible = false
			};

			contentGrid.Add(leadingContent);
			contentGrid.SetColumn(leadingContent, 0);

			leadingContent.SetBinding(
				ContentView.ContentProperty,
				static (TitleBar tb) => tb.LeadingContent,
				source: RelativeBindingSource.TemplatedParent);

			var leadingVisibleGroup = GetVisibleStateGroup(TitleBarLeading, LeadingVisibleState, LeadingHiddenState);
			leadingVisibleGroup.Name = "LeadingContentGroup";
			visualStateGroups.Add(leadingVisibleGroup);
			#endregion

			#region Icon
			var icon = new Image()
			{
				WidthRequest = 16,
				HeightRequest = 16,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(16, 0, 0, 0),
				IsVisible = false,
			};

			contentGrid.Add(icon);
			contentGrid.SetColumn(icon, 1);

			icon.SetBinding(
				Image.SourceProperty,
				static (TitleBar tb) => tb.Icon,
				source: RelativeBindingSource.TemplatedParent);

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

			titleLabel.SetBinding(
				Label.TextProperty,
				static (TitleBar tb) => tb.Title,
				source: RelativeBindingSource.TemplatedParent);

			titleLabel.SetBinding(
				Label.TextColorProperty,
				static (TitleBar tb) => tb.ForegroundColor,
				source: RelativeBindingSource.TemplatedParent);

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

			subtitleLabel.SetBinding(
				Label.TextProperty,
				static (TitleBar tb) => tb.Subtitle,
				source: RelativeBindingSource.TemplatedParent);

			subtitleLabel.SetBinding(
				Label.TextColorProperty,
				static (TitleBar tb) => tb.ForegroundColor,
				source: RelativeBindingSource.TemplatedParent);

			var subtitleVisibleGroup = GetVisibleStateGroup(TitleBarSubtitle, SubtitleVisibleState, SubtitleHiddenState);
			subtitleVisibleGroup.Name = "SubtitleTextGroup";
			visualStateGroups.Add(subtitleVisibleGroup);
			#endregion

			#region Content
			var content = new ContentView()
			{
				IsVisible = false
			};

			contentGrid.Add(content);
			contentGrid.SetColumn(content, 4);

			content.SetBinding(
				ContentView.ContentProperty,
				static (TitleBar tb) => tb.Content,
				source: RelativeBindingSource.TemplatedParent);

			var contentVisibleGroup = GetVisibleStateGroup(TitleBarContent, ContentVisibleState, ContentHiddenState);
			contentVisibleGroup.Name = "ContentGroup";
			visualStateGroups.Add(contentVisibleGroup);
			#endregion

			#region Trailing content
			var trailingContent = new ContentView()
			{
				IsVisible = false
			};

			contentGrid.Add(trailingContent);
			contentGrid.SetColumn(trailingContent, 5);

			trailingContent.SetBinding(
				ContentView.ContentProperty,
				static (TitleBar tb) => tb.TrailingContent,
				source: RelativeBindingSource.TemplatedParent);

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
	}
}
