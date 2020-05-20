using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui.Platform
{
	public partial class SearchRenderer
	{
		public static PropertyMapper<ISearch> SearchMapper = new PropertyMapper<ISearch>(ViewRenderer.ViewMapper)
		{
			[nameof(IText.Color)] = MapPropertyColor,
			[nameof(IText.Text)] = MapPropertyText,

			[nameof(ITextInput.Placeholder)] = MapPropertyPlaceholder,
			[nameof(ITextInput.PlaceholderColor)] = MapPropertyPlaceholderColor,
			[nameof(ITextInput.MaxLength)] = MapPropertyMaxLength,

			[nameof(ISearch.CancelColor)] = MapPropertyCancelColor,
#if __IOS__
			[nameof(IView.BackgroundColor)] = MapPropertyBackgroundColor,
#endif
		};

		public SearchRenderer() : base(SearchMapper)
		{

		}

		public SearchRenderer(PropertyMapper mapper) : base(mapper ?? SearchMapper)
		{

		}
	}
}
