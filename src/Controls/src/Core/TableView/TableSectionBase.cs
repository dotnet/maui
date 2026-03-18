#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Abstract base class for a section in a <see cref="TableView"/>.
	/// </summary>
	public abstract class TableSectionBase : BindableObject
	{
		/// <summary>Bindable property for <see cref="Title"/>.</summary>
		public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(TableSectionBase), null);
		/// <summary>Bindable property for <see cref="TextColor"/>.</summary>
		public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(TableSectionBase), null);

		/// <summary>
		/// Creates a new <see cref="TableSectionBase"/> with default values.
		/// </summary>
		protected TableSectionBase()
		{
		}

		/// <summary>
		/// Creates a new <see cref="TableSectionBase"/> with the specified title.
		/// </summary>
		protected TableSectionBase(string title)
		{
			if (title == null)
				throw new ArgumentNullException(nameof(title));

			Title = title;
		}

		/// <summary>
		/// Gets or sets the title for the section. This is a bindable property.
		/// </summary>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		/// <summary>
		/// Gets or sets the text color for the section header. This is a bindable property.
		/// </summary>
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}
	}
}