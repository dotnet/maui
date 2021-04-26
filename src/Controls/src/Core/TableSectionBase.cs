using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public abstract class TableSectionBase : BindableObject
	{
		public static readonly BindableProperty TitleProperty = BindableProperty.Create("Title", typeof(string), typeof(TableSectionBase), null);
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
				throw new ArgumentNullException("title");

			Title = title;
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}
	}
}