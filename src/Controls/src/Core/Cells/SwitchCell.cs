using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwitchCell']/Docs" />
	public class SwitchCell : Cell
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="//Member[@MemberName='OnProperty']/Docs" />
		public static readonly BindableProperty OnProperty = BindableProperty.Create("On", typeof(bool), typeof(SwitchCell), false, propertyChanged: (obj, oldValue, newValue) =>
		{
			var switchCell = (SwitchCell)obj;
			switchCell.OnChanged?.Invoke(obj, new ToggledEventArgs((bool)newValue));
		}, defaultBindingMode: BindingMode.TwoWay);

		/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="//Member[@MemberName='TextProperty']/Docs" />
		public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(SwitchCell), default(string));

		/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="//Member[@MemberName='OnColorProperty']/Docs" />
		public static readonly BindableProperty OnColorProperty = BindableProperty.Create(nameof(OnColor), typeof(Color), typeof(SwitchCell), null);

		/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="//Member[@MemberName='OnColor']/Docs" />
		public Color OnColor
		{
			get { return (Color)GetValue(OnColorProperty); }
			set { SetValue(OnColorProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="//Member[@MemberName='On']/Docs" />
		public bool On
		{
			get { return (bool)GetValue(OnProperty); }
			set { SetValue(OnProperty, value); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/SwitchCell.xml" path="//Member[@MemberName='Text']/Docs" />
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public event EventHandler<ToggledEventArgs> OnChanged;
	}
}