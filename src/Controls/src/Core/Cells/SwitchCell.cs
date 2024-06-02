#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwitchCell']/Docs/*" />
	public class SwitchCell : Cell
	{
		/// <summary>Bindable property for <see cref="On"/>.</summary>
		public static readonly BindableProperty OnProperty = BindableProperty.Create(nameof(On), typeof(bool), typeof(SwitchCell), false, propertyChanged: (obj, oldValue, newValue) =>
		{
			var switchCell = (SwitchCell)obj;
			switchCell.OnChanged?.Invoke(obj, new ToggledEventArgs((bool)newValue));
		}, defaultBindingMode: BindingMode.TwoWay);

		/// <summary>Bindable property for <see cref="Text"/>.</summary>
		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(SwitchCell), default(string));

		/// <summary>Bindable property for <see cref="OnColor"/>.</summary>
		public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(SwitchCell), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="//Member[@MemberName='OnColor']/Docs/*" />
		public Color OnColor
		{
			get { return (Color)GetValue(OnColorProperty); }
			set { SetValue(OnColorProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="//Member[@MemberName='On']/Docs/*" />
		public bool On
		{
			get { return (bool)GetValue(OnProperty); }
			set { SetValue(OnProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="//Member[@MemberName='Text']/Docs/*" />
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public event EventHandler<ToggledEventArgs> OnChanged;
	}
}