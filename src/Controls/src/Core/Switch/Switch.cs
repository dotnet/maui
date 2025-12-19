#nullable disable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a control that the user can toggle between two states: on or off.
	/// </summary>
	/// <remarks>
	/// A <see cref="Switch"/> is a UI element that can be toggled between on and off states.
	/// Use the <see cref="IsToggled"/> property to determine or set the current state.
	/// </remarks>
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	public partial class Switch : View, IElementConfiguration<Switch>, ISwitch
	{
		/// <summary>
		/// The visual state name for when the switch is in the on position.
		/// </summary>
		/// <value>The string "On".</value>
		public const string SwitchOnVisualState = "On";
		
		/// <summary>
		/// The visual state name for when the switch is in the off position.
		/// </summary>
		/// <value>The string "Off".</value>
		public const string SwitchOffVisualState = "Off";

		/// <summary>Bindable property for <see cref="IsToggled"/>. This is a bindable property.</summary>
		public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(Switch), false, propertyChanged: (bindable, oldValue, newValue) =>
		{
			((Switch)bindable).Toggled?.Invoke(bindable, new ToggledEventArgs((bool)newValue));
			((Switch)bindable).ChangeVisualState();
			((IView)bindable)?.Handler?.UpdateValue(nameof(ISwitch.TrackColor));

		}, defaultBindingMode: BindingMode.TwoWay);

		/// <summary>Bindable property for <see cref="OnColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(Switch), null,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				((IView)bindable)?.Handler?.UpdateValue(nameof(ISwitch.TrackColor));
			});

		/// <summary>Bindable property for <see cref="OffColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty OffColorProperty = BindableProperty.Create(nameof(OffColor), typeof(Color), typeof(Switch), null,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				((IView)bindable)?.Handler?.UpdateValue(nameof(ISwitch.TrackColor));
			});

		/// <summary>Bindable property for <see cref="ThumbColor"/>. This is a bindable property.</summary>
		public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(Switch), null);

		/// <summary>
		/// Gets or sets the color of the switch track when it is in the on position.
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Color"/> of the track when on. The default is <see langword="null"/>, which uses the platform default.</value>
		public Color OnColor
		{
			get { return (Color)GetValue(OnColorProperty); }
			set { SetValue(OnColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the color of the switch track when it is in the off position.
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Color"/> of the track when off. The default is <see langword="null"/>, which uses the platform default.</value>
		public Color OffColor
		{
			get { return (Color)GetValue(OffColorProperty); }
			set { SetValue(OffColorProperty, value); }
		}


		/// <summary>
		/// Gets or sets the color of the switch thumb (the movable circular part).
		/// This is a bindable property.
		/// </summary>
		/// <value>The <see cref="Color"/> of the thumb. The default is <see langword="null"/>, which uses the platform default.</value>
		public Color ThumbColor
		{
			get { return (Color)GetValue(ThumbColorProperty); }
			set { SetValue(ThumbColorProperty, value); }
		}

		readonly Lazy<PlatformConfigurationRegistry<Switch>> _platformConfigurationRegistry;

		/// <summary>
		/// Initializes a new instance of the <see cref="Switch"/> class.
		/// </summary>
		public Switch()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Switch>>(() => new PlatformConfigurationRegistry<Switch>(this));
		}

		/// <summary>
		/// Gets or sets a value indicating whether the switch is in the on position.
		/// This is a bindable property.
		/// </summary>
		/// <value><see langword="true"/> if the switch is on; otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
		public bool IsToggled
		{
			get { return (bool)GetValue(IsToggledProperty); }
			set { SetValue(IsToggledProperty, value); }
		}

		protected internal override void ChangeVisualState()
		{
			base.ChangeVisualState();
			if (IsEnabled && IsToggled)
				VisualStateManager.GoToState(this, SwitchOnVisualState);
			else if (IsEnabled && !IsToggled)
				VisualStateManager.GoToState(this, SwitchOffVisualState);
		}

		/// <summary>
	/// Occurs when the <see cref="IsToggled"/> property changes.
	/// </summary>
	public event EventHandler<ToggledEventArgs> Toggled;

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, Switch> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		// TODO This should get moved to a remapped mapper
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == IsToggledProperty.PropertyName)
				Handler?.UpdateValue(nameof(ISwitch.IsOn));
		}

		Color ISwitch.TrackColor
		{
			get
			{
				if (IsToggled)
				{
					return OnColor;
				}
				else
				{
					return OffColor;
				}
			}
		}

		bool ISwitch.IsOn
		{
			get => IsToggled;
			set => SetValue(IsToggledProperty, value, SetterSpecificity.FromHandler);
		}

		private protected override string GetDebuggerDisplay()
		{
			return $"{base.GetDebuggerDisplay()}, IsToggled = {IsToggled}";
		}
	}
}