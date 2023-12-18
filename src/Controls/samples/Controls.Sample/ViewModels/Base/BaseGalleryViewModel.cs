using System;
using System.Collections.Generic;
using System.Linq;
using Maui.Controls.Sample.Models;

namespace Maui.Controls.Sample.ViewModels.Base
{
	public abstract class BaseGalleryViewModel : BaseViewModel
	{
		string? _filterValue;

		public BaseGalleryViewModel()
		{
			var items = CreateItems();

			if (items != null)
				Items = items.ToList();

			Filter();
		}

		public IReadOnlyList<SectionModel>? Items { get; }

		public string? FilterValue
		{
			get { return _filterValue; }
			set
			{
				_filterValue = value;
				Filter();
			}
		}

		public IEnumerable<SectionModel> FilteredItems { get; private set; } = Enumerable.Empty<SectionModel>();

		protected abstract IEnumerable<SectionModel> CreateItems();

		void Filter()
		{
			FilterValue ??= string.Empty;
			FilteredItems = string.IsNullOrEmpty(FilterValue) ? Items! : Items!.Where(item => item.Title.IndexOf(FilterValue, StringComparison.InvariantCultureIgnoreCase) >= 0);
			OnPropertyChanged(nameof(FilteredItems));
		}
	}
}