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

		public static readonly BindableProperty HeaderProperty = BindableProperty.Create(nameof(Header), typeof(View), typeof(Expander), default(View), propertyChanged: (bindable, oldValue, newValue)
			=> ((Expander)bindable).SetHeader((View)oldValue));

		public static readonly BindableProperty ContentProperty = BindableProperty.Create(nameof(Content), typeof(View), typeof(Expander), default(View), propertyChanged: (bindable, oldValue, newValue)
			=> ((Expander)bindable).SetContent());

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
		double _lastVisibleHeight = -1;
		double _previousWidth = -1;
		double _startHeight;
		double _endHeight;
		bool _shouldIgnoreContentSetting;
		bool _shouldIgnoreAnimation;
		static bool isExperimentalFlagSet = false;

		public Expander()
		{
			ExpanderLayout = new StackLayout { Spacing = 0 };
			ForceUpdateSizeCommand = new Command(ForceUpdateSize);
			InternalChildren.Add(ExpanderLayout);
		}

		internal static void VerifyExperimental([CallerMemberName] string memberName = "", string constructorHint = null)
		{
			if (isExperimentalFlagSet)
				return;

			ExperimentalFlags.VerifyFlagEnabled(nameof(Expander), ExperimentalFlags.ExpanderExperimental, constructorHint, memberName);

			isExperimentalFlagSet = true;
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{			
			VerifyExperimental(nameof(Expander));
			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		StackLayout ExpanderLayout { get; }

		ContentView ContentHolder { get; set; }
		
		double ContentHeightRequest
		{
			get
			{
				var heightRequest = Content.HeightRequest;
				if (heightRequest < 0 || !(Content is Layout layout))
					return heightRequest;
				return heightRequest + layout.Padding.VerticalThickness;
			}
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
			if (ContentHolder == null || (!IsExpanded && !ContentHolder.IsVisible))
			{
				return;
			}

			var isAnimationRunning = ContentHolder.AnimationIsRunning(ExpandAnimationName);
			ContentHolder.AbortAnimation(ExpandAnimationName);

			_startHeight = ContentHolder.IsVisible
				? Max(ContentHolder.Height, 0)
				: 0;

			if (IsExpanded)
			{
				ContentHolder.IsVisible = true;
			}

			_endHeight = ContentHeightRequest >= 0
				? ContentHeightRequest
				: _lastVisibleHeight;

			if (IsExpanded)
			{
				if (_endHeight <= 0)
				{
					ContentHolder.HeightRequest = -1;
					_endHeight = ContentHolder.Measure(Width, double.PositiveInfinity).Request.Height;
					ContentHolder.HeightRequest = 0;
				}
			}
			else
			{
				_lastVisibleHeight = _startHeight = ContentHeightRequest >= 0
						? ContentHeightRequest
						: !isAnimationRunning
							? ContentHolder.Height
							: _lastVisibleHeight;
				_endHeight = 0;
			}

			_shouldIgnoreAnimation = isBindingContextChanged || Height < 0;

			InvokeAnimation();
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
								ancestorExpander.ContentHolder.HeightRequest = -1;
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

		void SetContent()
		{
			if (ContentHolder != null)
			{
				ContentHolder.AbortAnimation(ExpandAnimationName);
				ExpanderLayout.Children.Remove(ContentHolder);
				ContentHolder = null;
			}
			if (Content != null)
			{
				ContentHolder = new ContentView
				{
					IsClippedToBounds = true,
					IsVisible = false,
					HeightRequest = 0,
					Content = Content
				};
				ExpanderLayout.Children.Add(ContentHolder);
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

		void InvokeAnimation()
		{
			State = IsExpanded ? ExpanderState.Expanding : ExpanderState.Collapsing;

			if (_shouldIgnoreAnimation)
			{
				State = IsExpanded ? ExpanderState.Expanded : ExpanderState.Collapsed;
				ContentHolder.HeightRequest = _endHeight;
				ContentHolder.IsVisible = IsExpanded;
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

			new Animation(v => ContentHolder.HeightRequest = v, _startHeight, _endHeight)
				.Commit(ContentHolder, ExpandAnimationName, 16, length, easing, (value, isInterrupted) =>
				{
					if (isInterrupted)
					{
						return;
					}
					if (!IsExpanded)
					{
						ContentHolder.IsVisible = false;
						State = ExpanderState.Collapsed;
						return;
					}
					State = ExpanderState.Expanded;
				});
		}
	}
}
