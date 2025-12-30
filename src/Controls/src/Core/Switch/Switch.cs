#nullable disable
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="Type[@FullName='Microsoft.Maui.Controls.Switch']/Docs/*" />
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
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
		public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(Switch), null,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				((IView)bindable)?.Handler?.UpdateValue(nameof(ISwitch.TrackColor));
			});

		/// <summary>Bindable property for <see cref="OffColor"/>.</summary>
		public static readonly BindableProperty OffColorProperty = BindableProperty.Create(nameof(OffColor), typeof(Color), typeof(Switch), null,
			propertyChanged: (bindable, oldValue, newValue) =>
			{
				((IView)bindable)?.Handler?.UpdateValue(nameof(ISwitch.TrackColor));
			});

		/// <summary>Bindable property for <see cref="ThumbColor"/>.</summary>
		public static readonly BindableProperty ThumbColorProperty = BindableProperty.Create(nameof(ThumbColor), typeof(Color), typeof(Switch), null);

		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='OnColor']/Docs/*" />
		public Color OnColor
		{
			get { return (Color)GetValue(OnColorProperty); }
			set { SetValue(OnColorProperty, value); }
		}

		/// <summary>
		/// Gets or sets the color of the toggle switch's track when it is in the off state.
		/// If not set, the default color will be used for the off-track appearance.
		/// </summary>
		public Color OffColor
		{
			get { return (Color)GetValue(OffColorProperty); }
			set { SetValue(OffColorProperty, value); }
		}


		/// <include file="../../docs/Microsoft.Maui.Controls/Switch.xml" path="//Member[@MemberName='ThumbColor']/Docs/*" />
		public Color ThumbColor
		{
			get { return (Color)GetValue(ThumbColorProperty); }
			set { SetValue(ThumbColorProperty, value); }
		}

		readonly Lazy<PlatformConfigurationRegistry<Switch>> _platformConfigurationRegistry;

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