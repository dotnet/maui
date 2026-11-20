#nullable disable
using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a control that a user can select or clear.
	/// </summary>
	/// <remarks>
	/// A <see cref="CheckBox"/> can be checked, unchecked, or indeterminate.
	/// Application code and user interaction can set indeterminate when <see cref="IsThreeState"/> is true.
	/// Use the <see cref="IsChecked"/> property to determine or set the state.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class CheckBox : View, IElementConfiguration<CheckBox>, IBorderElement, IColorElement, ICheckBox2, ICommandElement
	{
		readonly Lazy<PlatformConfigurationRegistry<CheckBox>> _platformConfigurationRegistry;
		
		/// <summary>
		/// The visual state name for the checked state of the <see cref="CheckBox"/>.
		/// </summary>
		/// <value>The string "IsChecked".</value>
		public const string IsCheckedVisualState = "IsChecked";

		/// <summary>
		/// The visual state name for the indeterminate state of the <see cref="CheckBox"/>.
		/// </summary>
		/// <value>The string "IsIndeterminate".</value>
		public const string IsIndeterminateVisualState = "IsIndeterminate";

		/// <summary>Bindable property for <see cref="IsChecked"/>. This is a bindable property.</summary>
		public static readonly BindableProperty IsCheckedProperty =
			BindableProperty.Create(nameof(IsChecked), typeof(bool?), typeof(CheckBox), BooleanBoxes.FalseBox,
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					if (bindable is not CheckBox checkBox)
					{
						return;
					}

					checkBox.Handler?.UpdateValue(nameof(ICheckBox.Foreground));
					checkBox.CheckedChanged?.Invoke(bindable, new CheckedChangedEventArgs((bool?)newValue));
					if (checkBox.Command?.CanExecute(checkBox.CommandParameter) == true)
					{
						checkBox.Command.Execute(checkBox.CommandParameter);
					}

					checkBox.ChangeVisualState();
				}, defaultBindingMode: BindingMode.TwoWay,
				coerceValue: (bindable, value) =>
					!((CheckBox)bindable).IsThreeState && value is null ? BooleanBoxes.FalseBox : value);

		/// <summary>Bindable property for <see cref="IsThreeState"/>.</summary>
		public static readonly BindableProperty IsThreeStateProperty =
			BindableProperty.Create(nameof(IsThreeState), typeof(bool), typeof(CheckBox), BooleanBoxes.FalseBox,
				propertyChanged: (bindable, oldValue, newValue) =>
				{
					var checkBox = (CheckBox)bindable;
					if (!(bool)newValue && checkBox.GetContext(IsCheckedProperty) is { } context)
					{
						// Normalize queued values when disabling, preserving their order and specificity.
						if (context.DelayedSetters is { } delayedSetters)
						{
							var count = delayedSetters.Count;
							for (var i = 0; i < count; i++)
							{
								var setter = delayedSetters.Dequeue();
								delayedSetters.Enqueue(setter.Value is null
									? new SetValueArgs(setter.Property, setter.Context, BooleanBoxes.FalseBox,
										setter.CurrentlyApplying, setter.Attributes, setter.Specificity)
									: setter);
							}
						}

						// Clear hidden mixed values too, so clearing a higher-priority setter cannot restore null.
						for (var i = 0; i < context.Values.Count - 1; i++)
						{
							if (context.Values.GetValueAt(i) is null)
							{
								context.Values[context.Values.GetSpecificityAt(i)] = BooleanBoxes.FalseBox;
							}
						}

						if (checkBox.IsChecked is null)
						{
							checkBox.SetValueCore(IsCheckedProperty, BooleanBoxes.FalseBox, SetValueFlags.None,
								SetValuePrivateFlags.Default, context.Values.GetSpecificity());
						}
					}
				}, defaultBindingMode: BindingMode.TwoWay);

		/// <summary>Bindable property for the <see cref="Command"/> property.</summary>
		public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(CheckBox), null, propertyChanging: CommandElement.OnCommandChanging, propertyChanged: CommandElement.OnCommandChanged);

		/// <summary>Bindable property for the <see cref="CommandParameter"/> property.</summary>
		public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(CheckBox), null, propertyChanged: CommandElement.OnCommandParameterChanged);

		/// <summary>
		/// Gets or sets the command executed whenever the checked state changes, including indeterminate. This is a bindable property.
		/// </summary>
		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <summary>
		/// Gets or sets the parameter to pass to the <see cref="Command"/> when it is executed. This is a bindable property.
		/// </summary>
		public object CommandParameter
		{
			get => GetValue(CommandParameterProperty);
			set => SetValue(CommandParameterProperty, value);
		}

		/// <summary>Bindable property for <see cref="Color"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ColorProperty = ColorElement.ColorProperty;

		/// <summary>
		/// Gets or sets the color of the checkbox.
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Graphics.Color"/> of the checkbox. The default is platform-specific.</value>
		public Color Color
		{
			get => (Color)GetValue(ColorProperty);
			set => SetValue(ColorProperty, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CheckBox"/> class.
		/// </summary>
		public CheckBox() => _platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<CheckBox>>(() => new PlatformConfigurationRegistry<CheckBox>(this));

		/// <summary>
		/// Gets or sets a value indicating whether the <see cref="CheckBox"/> is checked.
		/// This is a bindable property.
		/// </summary>
		/// <value>True if checked, false if unchecked, or null if indeterminate. The default is false.</value>
		/// <remarks>When <see cref="IsThreeState"/> is false, assigning null sets the value to false.</remarks>
		public bool? IsChecked
		{
			get => (bool?)GetValue(IsCheckedProperty);
			set => SetValue(IsCheckedProperty, BooleanBoxes.Box(value));
		}

		/// <summary>
		/// Gets or sets whether application code and user interaction can set the indeterminate state.
		/// This is a bindable property.
		/// </summary>
		/// <remarks>
		/// The default is false. When true, user interaction cycles from unchecked to checked
		/// to indeterminate, then back to unchecked. Otherwise, user interaction remains binary.
		/// Disabling three-state support resets an indeterminate <see cref="IsChecked"/> value to false.
		/// </remarks>
		public bool IsThreeState
		{
			get => (bool)GetValue(IsThreeStateProperty);
			set => SetValue(IsThreeStateProperty, BooleanBoxes.Box(value));
		}

		protected internal override void ChangeVisualState()
		{
			if (IsEnabled && IsChecked != false)
			{
				string stateName = IsChecked == true ? IsCheckedVisualState : IsIndeterminateVisualState;
				bool isStateAvailable = false;
				var visualStates = VisualStateManager.GetVisualStateGroups(this);
				foreach (var group in visualStates)
				{
					if (group.Name is not "CommonStates")
					{
						continue;
					}

					foreach (var state in group.States)
					{
						if (state.Name == stateName)
						{
							isStateAvailable = true;
							break;
						}
					}

					break;
				}

				if (isStateAvailable)
				{
					VisualStateManager.GoToState(this, stateName);
				}
				else
				{
					VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
				}
			}
			else
				base.ChangeVisualState();
		}

		/// <summary>
		/// Occurs when the <see cref="IsChecked"/> property changes.
		/// </summary>
		public event EventHandler<CheckedChangedEventArgs> CheckedChanged;

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, CheckBox> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		void IBorderElement.OnBorderColorPropertyChanged(Color oldValue, Color newValue)
		{
		}

		Color IBorderElement.BorderColor => Colors.Transparent;
		int IBorderElement.CornerRadius => 0;
		double IBorderElement.BorderWidth => 0;
		int IBorderElement.CornerRadiusDefaultValue => 0;
		Color IBorderElement.BorderColorDefaultValue => Colors.Transparent;
		double IBorderElement.BorderWidthDefaultValue => 0;
		bool IBorderElement.IsCornerRadiusSet() => false;
		bool IBorderElement.IsBackgroundColorSet() => IsSet(BackgroundColorProperty);
		bool IBorderElement.IsBackgroundSet() => IsSet(BackgroundProperty);
		bool IBorderElement.IsBorderColorSet() => false;
		bool IBorderElement.IsBorderWidthSet() => false;
		void ICommandElement.CanExecuteChanged(object sender, EventArgs e) =>
			RefreshIsEnabledProperty();

		protected override bool IsEnabledCore =>
			base.IsEnabledCore && CommandElement.GetCanExecute(this, CommandProperty);
		public Paint Foreground => Color?.AsPaint();

		bool ICheckBox.IsChecked
		{
			get => IsChecked == true;
			set => SetValue(IsCheckedProperty, BooleanBoxes.Box(value), SetterSpecificity.FromHandler);
		}

		bool? ICheckBox2.IsChecked
		{
			get => IsChecked;
			set => SetValue(IsCheckedProperty, BooleanBoxes.Box(value), SetterSpecificity.FromHandler);
		}

		bool ICheckBox2.IsThreeState => IsThreeState;

		ICommand ICommandElement.Command => Command;

		object ICommandElement.CommandParameter => CommandParameter;

		WeakCommandSubscription ICommandElement.CleanupTracker { get; set; }

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, IsChecked = {IsChecked?.ToString() ?? "Indeterminate"}";
		}

		internal override bool TrySetValue(string text)
		{
			if (string.Equals(text, "Indeterminate", StringComparison.OrdinalIgnoreCase))
			{
				IsChecked = null;
				return true;
			}

			if (bool.TryParse(text, out bool result))
			{
				IsChecked = result;
				return true;
			}

			return false;
		}
	}
}