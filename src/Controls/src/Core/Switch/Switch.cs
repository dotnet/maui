#nullable disable
using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="Type[@FullName='Microsoft.Maui.Controls.Switch']/Docs/*" />
	public partial class Switch : View, IElementConfiguration<Switch>, ISwitch
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='SwitchOnVisualState']/Docs/*" />
		public const string SwitchOnVisualState = "On";
		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='SwitchOffVisualState']/Docs/*" />
		public const string SwitchOffVisualState = "Off";

		/// <summary>Bindable property for <see cref="IsToggled"/>.</summary>
		public static readonly BindableProperty IsToggledProperty = BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(Switch), false, propertyChanged: (bindable, oldValue, newValue) =>
		{
			((Switch)bindable).Toggled?.Invoke(bindable, new ToggledEventArgs((bool)newValue));
			((Switch)bindable).ChangeVisualState();
			((IView)bindable)?.Handler?.UpdateValue(nameof(ISwitch.TrackColor));

		}, defaultBindingMode: BindingMode.TwoWay);

		/// <summary>Bindable property for <see cref="OnColor"/>.</summary>
		public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(Switch), null);

		/// <summary>Bindable property for <see cref="ThumbColor"/>.</summary>
		public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(Switch), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='OnColor']/Docs/*" />
		public Color OnColor
		{
			get { return (Color)GetValue(OnColorProperty); }
			set { SetValue(OnColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='ThumbColor']/Docs/*" />
		public Color ThumbColor
		{
			get { return (Color)GetValue(ThumbColorProperty); }
			set { SetValue(ThumbColorProperty, value); }
		}

		readonly Lazy<PlatformConfigurationRegistry<Switch>> _platformConfigurationRegistry;

		// Colors used for tracking changes in visual state.
		Color _visualStateOnThumbColor;
		Color _visualStateOffThumbColor;
		Color _visualStateOnTrackColor;
		Color _visualStateOffTrackColor;
		Color _overridenTrackColor;
		Color _overridenThumbColor;

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Switch()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<Switch>>(() => new PlatformConfigurationRegistry<Switch>(this));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='IsToggled']/Docs/*" />
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

			// Update thumb and track colors based on the switch state and any overridden colors.
			if (IsEnabled)
			{
				if (IsToggled)
				{
					_visualStateOnThumbColor = ThumbColor;
					_visualStateOnTrackColor = OnColor;
				}
				else
				{
					_visualStateOffThumbColor = ThumbColor;
					_visualStateOffTrackColor = OnColor;
				}

				// Apply overridden thumb color if available.
				if (_overridenThumbColor != null)
				{
					ThumbColor = _overridenThumbColor;
				}

				// Apply overridden track color if available.
				if (_overridenTrackColor != null)
				{
					OnColor = _overridenTrackColor;
				}
				
			}
		}

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

			if (propertyName == ThumbColorProperty.PropertyName)
			{
				if (IsEnabled)
				{
					// Handle overridden thumb color based on switch state.
					if (!VisualStateManager.HasVisualStateGroups(this))
					{
						_overridenThumbColor = ThumbColor;
					}
					else
					{	
						if (IsToggled && _visualStateOnThumbColor != null && ThumbColor != _visualStateOnThumbColor)
						{
							_overridenThumbColor = ThumbColor;
						}
						else if (!IsToggled && _visualStateOffThumbColor != null && ThumbColor != _visualStateOffThumbColor)
						{
							_overridenThumbColor = ThumbColor;
						}
					}
				}

				Handler?.UpdateValue(nameof(ISwitch.ThumbColor));
			}

			if (propertyName == OnColorProperty.PropertyName) 
			{
				if (IsEnabled)
				{
					// Handle overridden thumb color based on switch state.
					if (!VisualStateManager.HasVisualStateGroups(this))
					{
						_overridenTrackColor = OnColor;
					}
					else
					{
						if (IsToggled && _visualStateOnTrackColor != null && OnColor != _visualStateOnTrackColor)
						{
							_overridenTrackColor = OnColor;
						}
						else if (!IsToggled && _visualStateOffTrackColor != null && OnColor != _visualStateOffTrackColor)
						{
							_overridenTrackColor = OnColor;
						}
					}
				}

				Handler?.UpdateValue(nameof(ISwitch.TrackColor));
			}
		}

		Color ISwitch.TrackColor
		{
			get
			{
#if WINDOWS
				return OnColor;
#else
				if (IsToggled)
					return OnColor;

				return null;
#endif
			}
		}

		bool ISwitch.IsOn
		{
			get => IsToggled;
			set => SetValue(IsToggledProperty, value, SetterSpecificity.FromHandler);
		}
	}
}