using System;

namespace Xamarin.Forms
{
	public abstract class TableSectionBase : BindableObject
	{
		public static readonly BindableProperty TitleProperty = BindableProperty.Create("Title", typeof(string), typeof(TableSectionBase), null);

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
	}
}