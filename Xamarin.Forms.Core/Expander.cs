using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using static System.Math;

namespace Xamarin.Forms
{
	[ContentProperty(nameof(Content))]
	public class Expander : TemplatedView
	{
		const string ExpandAnimationName = nameof(ExpandAnimationName);
		const uint DefaultAnimationLength = 250;

		public event EventHandler Tapped;

		public static readonly BindableProperty SpacingProperty = BindableProperty.Create(nameof(Spacing), typeof(double), typeof(Expander), 0d, propertyChanged: (bindable, oldvalue, newvalue)
			=> ((Expander)bindable).ExpanderLayout.Spacing = (double)newvalue);

		public static readonly BindableProperty HeaderProperty = BindableProperty.Create(nameof(Header), typeof(View), typeof(Expander), default(View), propertyChanged: (bindable, oldValue, newValue)
			=> ((Expander)bindable).SetHeader((View)oldValue));

		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(Expander), default(View), propertyChanged: (bindable, oldValue, newValue)
			=> ((Expander)bindable).SetContent((View)oldValue, (View)newValue));

		public static readonly BindableProperty ContentTemplateProperty = BindableProperty.Create(nameof(ContentTemplate), typeof(DataTemplate), typeof(Expander), default(DataTemplate), propertyChanged: (bindable, oldValue, newValue)
			=> ((Expander)bindable).SetContent(true));

		public static readonly BindableProperty IsExpandedProperty = BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(Expander), default(bool), BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue)
			=> ((Expander)bindable).SetContent(false));

		public static readonly BindableProperty ExpandAnimationLengthProperty = BindableProperty.Create(nameof(ExpandAnimationLength), typeof(uint), typeof(Expander), DefaultAnimationLength);

		public static readonly BindableProperty CollapseAnimationLengthProperty = BindableProperty.Create(nameof(CollapseAnimationLength), typeof(uint), typeof(Expander), DefaultAnimationLength);

		public static readonly BindableProperty ExpandAnimationEasingProperty = BindableProperty.Create(nameof(ExpandAnimationEasing), typeof(Easing), typeof(Expander), default(Easing));

		public static readonly BindableProperty CollapseAnimationEasingProperty = BindableProperty.Create(nameof(CollapseAnimationEasing), typeof(Easing), typeof(Expander), default(Easing));

		public static readonly BindableProperty StateProperty = BindableProperty.Create(nameof(State), typeof(ExpanderState), typeof(Expander), default(ExpanderState), BindingMode.OneWayToSource);

		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(Expander), default(object));

		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(Expander), default(ICommand));

		public static readonly BindableProperty ForceUpdateSizeCommandProperty = BindableProperty.Create(nameof(ForceUpdateSizeCommand), typeof(ICommand), typeof(Expander), default(ICommand), BindingMode.OneWayToSource);

		DataTemplate _previousTemplate;
		double _contentHeightRequest = -1;
		double _lastVisibleHeight = -1;
		double _previousWidth = -1;
		double _startHeight;
		double _endHeight;
		bool _shouldIgnoreContentSetting;
		bool _shouldIgnoreAnimation;
		static bool isExperimentalFlagSet = false;

		public Expander()
		{
			ExpanderLayout = new StackLayout { Spacing = Spacing };
			ForceUpdateSizeCommand = new Command(ForceUpdateSize);
			InternalChildren.Add(ExpanderLayout);
		}

		internal static void VerifyExperimental([CallerMemberName] string memberName = "", string constructorHint = null)
		{
			if (isExperimentalFlagSet)
				return;

			ExperimentalFlags.VerifyFlagEnabled(nameof(Markup), ExperimentalFlags.ExpanderExperimental, constructorHint, memberName);

			isExperimentalFlagSet = true;
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{			
			VerifyExperimental();
			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		StackLayout ExpanderLayout { get; }

		public double Spacing
		{
			get => (double)GetValue(SpacingProperty);
			set => SetValue(SpacingProperty, value);
		}

		public View Header
		{
			get => (View)GetValue(HeaderProperty);
			set => SetValue(HeaderProperty, value);
		}

		public View Content
		{
			get => (View)GetValue(ContentProperty);
			set => SetValue(ContentProperty, value);
		}

		public DataTemplate ContentTemplate
		{
			get => (DataTemplate)GetValue(ContentTemplateProperty);
			set => SetValue(ContentTemplateProperty, value);
		}

		public bool IsExpanded
		{
			get => (bool)GetValue(IsExpandedProperty);
			set => SetValue(IsExpandedProperty, value);
		}

		public uint ExpandAnimationLength
		{
			get => (uint)GetValue(ExpandAnimationLengthProperty);
			set => SetValue(ExpandAnimationLengthProperty, value);
		}

		public uint CollapseAnimationLength
		{
			get => (uint)GetValue(CollapseAnimationLengthProperty);
			set => SetValue(CollapseAnimationLengthProperty, value);
		}

		public Easing ExpandAnimationEasing
		{
			get => (Easing)GetValue(ExpandAnimationEasingProperty);
			set => SetValue(ExpandAnimationEasingProperty, value);
		}

		public Easing CollapseAnimationEasing
		{
			get => (Easing)GetValue(CollapseAnimationEasingProperty);
			set => SetValue(CollapseAnimationEasingProperty, value);
		}

		public ExpanderState State
		{
			get => (ExpanderState)GetValue(StateProperty);
			set => SetValue(StateProperty, value);
		}

		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		public ICommand ForceUpdateSizeCommand
		{
			get => (ICommand)GetValue(ForceUpdateSizeCommandProperty);
			set => SetValue(ForceUpdateSizeCommandProperty, value);
		}

		public void ForceUpdateSize()
		{
			_lastVisibleHeight = -1;
			OnIsExpandedChanged();
		}

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();
			_lastVisibleHeight = -1;
			SetContent(true, true);
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			if (Abs(width - _previousWidth) >= double.Epsilon)
			{
				ForceUpdateSize();
			}
			_previousWidth = width;
		}

		void OnIsExpandedChanged(bool isBindingContextChanged = false)
		{
			if (Content == null || (!IsExpanded && !Content.IsVisible))
			{
				return;
			}

			Content.SizeChanged -= OnContentSizeChanged;

			var isAnimationRunning = Content.AnimationIsRunning(ExpandAnimationName);
			Content.AbortAnimation(ExpandAnimationName);


			_startHeight = Content.IsVisible
				? Max(Content.Height - (Content is Layout l ? l.Padding.Top + l.Padding.Bottom : 0), 0)
				: 0;

			if (IsExpanded)
			{
				Content.IsVisible = true;
			}

			_endHeight = _contentHeightRequest >= 0
				? _contentHeightRequest
				: _lastVisibleHeight;

			var shouldInvokeAnimation = true;

			if (IsExpanded)
			{
				if (_endHeight <= 0)
				{
					shouldInvokeAnimation = false;
					Content.SizeChanged += OnContentSizeChanged;
					Content.HeightRequest = -1;
				}
			}
			else
			{
				_lastVisibleHeight = _startHeight = _contentHeightRequest >= 0
						? _contentHeightRequest
						: !isAnimationRunning
							? Content.Height - (Content is Layout layout
								? layout.Padding.Top + layout.Padding.Bottom
								: 0)
							: _lastVisibleHeight;
				_endHeight = 0;
			}

			_shouldIgnoreAnimation = isBindingContextChanged || Height < 0;

			if (shouldInvokeAnimation)
			{
				InvokeAnimation();
			}
		}

		void SetHeader(View oldHeader)
		{
			if (oldHeader != null)
			{
				ExpanderLayout.Children.Remove(oldHeader);
			}
			if (Header != null)
			{
				ExpanderLayout.Children.Insert(0, Header);
				Header.GestureRecognizers.Add(new TapGestureRecognizer
				{
					CommandParameter = this,
					Command = new Command(parameter =>
					{
						var parent = (parameter as View).Parent;
						while (parent != null && !(parent is Page))
						{
							if (parent is Expander ancestorExpander)
							{
								ancestorExpander.Content.HeightRequest = -1;
							}
							parent = parent.Parent;
						}
						IsExpanded = !IsExpanded;
						Command?.Execute(CommandParameter);
						Tapped?.Invoke(this, EventArgs.Empty);
					})
				});
			}
		}

		void SetContent(bool isForceUpdate, bool isBindingContextChanged = false)
		{
			if (IsExpanded && (Content == null || isForceUpdate))
			{
				_shouldIgnoreContentSetting = true;
				Content = CreateContent() ?? Content;
				_shouldIgnoreContentSetting = false;
			}
			OnIsExpandedChanged(isBindingContextChanged);
		}

		void SetContent(View oldContent, View newContent)
		{
			if (oldContent != null)
			{
				oldContent.SizeChanged -= OnContentSizeChanged;
				ExpanderLayout.Children.Remove(oldContent);
			}
			if (newContent != null)
			{
				if (newContent is Layout layout)
				{
					layout.IsClippedToBounds = true;
				}
				_contentHeightRequest = newContent.HeightRequest;
				newContent.HeightRequest = 0;
				newContent.IsVisible = false;
				ExpanderLayout.Children.Add(newContent);
			}

			if (!_shouldIgnoreContentSetting)
			{
				SetContent(true);
			}
		}

		View CreateContent()
		{
			var template = ContentTemplate;
			while (template is DataTemplateSelector selector)
			{
				template = selector.SelectTemplate(BindingContext, this);
			}
			if (template == _previousTemplate && Content != null)
			{
				return null;
			}
			_previousTemplate = template;
			return (View)template?.CreateContent();
		}

		void OnContentSizeChanged(object sender, EventArgs e)
		{
			if (Content.Height <= 0)
			{
				return;
			}
			Content.SizeChanged -= OnContentSizeChanged;
			Content.HeightRequest = 0;
			_endHeight = Content.Height;
			InvokeAnimation();
		}

		void InvokeAnimation()
		{
			State = IsExpanded ? ExpanderState.Expanding : ExpanderState.Collapsing;

			if (_shouldIgnoreAnimation)
			{
				State = IsExpanded ? ExpanderState.Expanded : ExpanderState.Collapsed;
				Content.HeightRequest = _endHeight;
				Content.IsVisible = IsExpanded;
				return;
			}

			var length = ExpandAnimationLength;
			var easing = ExpandAnimationEasing;
			if (!IsExpanded)
			{
				length = CollapseAnimationLength;
				easing = CollapseAnimationEasing;
			}

			if (_lastVisibleHeight > 0)
			{
				length = Max((uint)(length * (Abs(_endHeight - _startHeight) / _lastVisibleHeight)), 1);
			}

			new Animation(v => Content.HeightRequest = v, _startHeight, _endHeight)
				.Commit(Content, ExpandAnimationName, 16, length, easing, (value, isInterrupted) =>
				{
					if (isInterrupted)
					{
						return;
					}
					if (!IsExpanded)
					{
						Content.IsVisible = false;
						State = ExpanderState.Collapsed;
						return;
					}
					State = ExpanderState.Expanded;
				});
		}
	}
}
