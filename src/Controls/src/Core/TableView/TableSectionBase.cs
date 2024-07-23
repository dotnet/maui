#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TableSectionBase.xml" path="Type[@FullName='Microsoft.Maui.Controls.TableSectionBase']/Docs/*" />
	public abstract class TableSectionBase : BindableObject
	{
		/// <summary>Bindable property for <see cref="Title"/>.</summary>
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(TableSectionBase), null);
		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(TableSectionBase), null);

		/// <summary>
		///     Constructs a Section without an empty header.
		/// </summary>
		protected TableSectionBase()
		{
		}

		/// <summary>
		///     Constructs a Section with the specified header.
		/// </summary>
		protected TableSectionBase(string title)
		{
			if (title == null)
				throw new ArgumentNullException(nameof(title));

			Title = title;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TableSectionBase.xml" path="//Member[@MemberName='Title']/Docs/*" />
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TableSectionBase.xml" path="//Member[@MemberName='TextColor']/Docs/*" />
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}
	}
}