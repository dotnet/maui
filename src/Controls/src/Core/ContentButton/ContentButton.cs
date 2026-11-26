#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>A native button that presents a single view, optionally through a control template.</summary>
	/// <remarks>
	/// Content is presentation-only; interactive descendants are not supported.
	/// Set <see cref="SemanticProperties.DescriptionProperty"/> to give the button an accessible name.
	/// The command executes before <see cref="Clicked"/>, as with <see cref="Button"/>.
	/// </remarks>
	public partial class ContentButton : ContentView, IContentButton, IButtonElement, ICommandElement,
		IElementConfiguration<ContentButton>, ICrossPlatformLayout
	{
		/// <summary>Bindable property for <see cref="Command"/>.</summary>
		public static readonly BindableProperty CommandProperty = ButtonElement.CommandProperty;

		/// <summary>Bindable property for <see cref="CommandParameter"/>.</summary>
		public static readonly BindableProperty CommandParameterProperty = ButtonElement.CommandParameterProperty;

		/// <summary>Bindable property for <see cref="BorderColor"/>.</summary>
		public static readonly BindableProperty BorderColorProperty =
			BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(ContentButton), null);

		/// <summary>Bindable property for <see cref="BorderWidth"/>.</summary>
		public static readonly BindableProperty BorderWidthProperty =
			BindableProperty.Create(nameof(BorderWidth), typeof(double), typeof(ContentButton), -1d,
				propertyChanged: OnBorderWidthChanged);

		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create(nameof(CornerRadius), typeof(int), typeof(ContentButton), BorderElement.DefaultCornerRadius);

		static readonly BindablePropertyKey IsPressedPropertyKey =
			BindableProperty.CreateReadOnly(nameof(IsPressed), typeof(bool), typeof(ContentButton), false);

		/// <summary>Bindable property for the read-only <see cref="IsPressed"/> property.</summary>
		public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;

		readonly Lazy<PlatformConfigurationRegistry<ContentButton>> _platformConfigurationRegistry;
		WeakCommandSubscription? _commandSubscription;

		/// <summary>Creates a content button.</summary>
		public ContentButton()
		{
			_platformConfigurationRegistry = new(() => new PlatformConfigurationRegistry<ContentButton>(this));
		}

		/// <summary>Gets or sets the command invoked when the button is activated.</summary>
		public ICommand? Command
		{
			get => (ICommand?)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <summary>Gets or sets the parameter supplied to the command.</summary>
		public object? CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		/// <summary>Gets or sets the border color.</summary>
		public Color? BorderColor
		{
			get => (Color?)GetValue(BorderColorProperty);
			set => SetValue(BorderColorProperty, value);
		}

		/// <summary>Gets or sets the border width in device-independent units. The default is -1.</summary>
		public double BorderWidth
		{
			get => (double)GetValue(BorderWidthProperty);
			set => SetValue(BorderWidthProperty, value);
		}

		/// <summary>Gets or sets the corner radius in device-independent units. The default uses the native radius.</summary>
		public int CornerRadius
		{
			get => (int)GetValue(CornerRadiusProperty);
			set => SetValue(CornerRadiusProperty, value);
		}

		/// <summary>Gets whether the button is pressed.</summary>
		public bool IsPressed => (bool)GetValue(IsPressedProperty);

		/// <summary>Occurs after the command executes when the button is activated.</summary>
		public event EventHandler? Clicked;

		/// <summary>Occurs when the enabled button is pressed.</summary>
		public event EventHandler? Pressed;

		/// <summary>Occurs when the press ends, including cancellation.</summary>
		public event EventHandler? Released;

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, ContentButton> On<T>() where T : IConfigPlatform =>
			_platformConfigurationRegistry.Value.On<T>();

		/// <inheritdoc/>
		protected override bool IsEnabledCore => base.IsEnabledCore && CommandElement.GetCanExecute(this, CommandProperty);

		/// <inheritdoc/>
		protected internal override void ChangeVisualState()
		{
			if (IsEnabled && IsPressed)
				VisualStateManager.GoToState(this, ButtonElement.PressedVisualState);
			else
				base.ChangeVisualState();
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == BorderColorProperty.PropertyName)
				Handler?.UpdateValue(nameof(IButtonStroke.StrokeColor));
			else if (propertyName == BorderWidthProperty.PropertyName)
				Handler?.UpdateValue(nameof(IButtonStroke.StrokeThickness));
		}

		static void OnBorderWidthChanged(BindableObject bindable, object oldValue, object newValue) =>
			((ContentButton)bindable).InvalidateMeasure();

		WeakCommandSubscription? ICommandElement.CleanupTracker
		{
			get => _commandSubscription;
			set => _commandSubscription = value;
		}

		void ICommandElement.CanExecuteChanged(object? sender, EventArgs e) => RefreshIsEnabledProperty();
		void IButtonElement.SetIsPressed(bool isPressed) => SetValue(IsPressedPropertyKey, isPressed);
		void IButtonElement.PropagateUpClicked() => Clicked?.Invoke(this, EventArgs.Empty);
		void IButtonElement.PropagateUpPressed() => Pressed?.Invoke(this, EventArgs.Empty);
		void IButtonElement.PropagateUpReleased() => Released?.Invoke(this, EventArgs.Empty);
		void IButton.Clicked() => ButtonElement.ElementClicked(this, this);
		void IButton.Pressed() => ButtonElement.ElementPressed(this, this);
		void IButton.Released() => ButtonElement.ElementReleased(this, this);
		Color IButtonStroke.StrokeColor => BorderColor!;
		double IButtonStroke.StrokeThickness => BorderWidth;
		int IButtonStroke.CornerRadius => CornerRadius;
		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint) =>
			ContentButtonLayout.Measure(this, widthConstraint, heightConstraint);
		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds) => ContentButtonLayout.Arrange(this, bounds);
		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint) =>
			((ICrossPlatformLayout)this).CrossPlatformMeasure(widthConstraint, heightConstraint);
		Size IContentView.CrossPlatformArrange(Rect bounds) => ((ICrossPlatformLayout)this).CrossPlatformArrange(bounds);
	}
}
