using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;

namespace Microsoft.Maui.Controls
{
	/// <summary>Represents an <see cref="Element"/> with base functionality for styling. Does not necessarily render on screen.</summary>
	public abstract class StyleableElement : Element, IStyleSelectable
	{
		/// <summary>Bindable property for <see cref="Style"/>.</summary>
		public static readonly BindableProperty StyleProperty =
			BindableProperty.Create(nameof(Style), typeof(Style), typeof(StyleableElement), default(Style),
				propertyChanged: (bindable, oldvalue, newvalue) => ((StyleableElement)bindable)._mergedStyle.Style = (Style)newvalue);

		internal readonly MergedStyle _mergedStyle;

		public StyleableElement()
		{
			_mergedStyle = new MergedStyle(GetType(), this);
		}

		/// <summary>Gets or sets the unique <see cref="Style"/> for this element.</summary>
		public Style? Style
		{
			get { return (Style?)GetValue(StyleProperty); }
			set { SetValue(StyleProperty, value); }
		}

		/// <summary>Gets or sets the style classes for the element.</summary>
		/// <remarks>
		///		<para>Equiavalent to <see cref="@class"/>.</para>
		///		<para>Style classes enable multiple styles to be applied to a control, without resorting to style inheritance.</para>
		/// </remarks>
		/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/styles/xaml?view=net-maui-8.0#style-classes">Conceptual documentation on style classes</seealso>
		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> StyleClass
		{
			get => @class;
			set => @class = value;
		}

		/// <summary>Gets or sets the style classes for the element.</summary>
		/// <remarks>
		///		<para>Equiavalent to <see cref="StyleClass"/>.</para>
		///		<para>Style classes enable multiple styles to be applied to a control, without resorting to style inheritance.</para>
		/// </remarks>
		/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/styles/xaml?view=net-maui-8.0#style-classes">Conceptual documentation on style classes</seealso>
		[System.ComponentModel.TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> @class
		{
			get => _mergedStyle.StyleClass;
			set => _mergedStyle.StyleClass = value;
		}

		IList<string> IStyleSelectable.Classes => StyleClass;
	}
}
