using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	[Preserve(AllMembers = true)]
	public class CollectionViewGalleryTestItem : INotifyPropertyChanged
	{
		string _caption;

		public DateTime Date { get; set; }

		public string Caption
		{
			get => _caption;
			set
			{
				_caption = value;
				OnPropertyChanged();
			}
		}

		public string Image { get; set; }

		public int Index { get; set; }
		public ICommand MoreCommand { get; set; }
		public ICommand LessCommand { get; set; }

		public CollectionViewGalleryTestItem(DateTime date, string caption, string image, int index)
		{
			Date = date;
			Caption = caption;
			Image = image;
			Index = index;

			var text = " Lorem ipsum dolor sit amet, qui eleifend adversarium ei, pro tamquam pertinax inimicus ut. Quis assentior ius no, ne vel modo tantas omnium, sint labitur id nec. Mel ad cetero repudiare definiebas, eos sint placerat cu.";

			MoreCommand = new Command(() => Caption += text);
			LessCommand = new Command(() =>
			{
				var last = Caption.LastIndexOf(text);
				if (last > 0)
				{
					Caption = Caption.Substring(0, last);
				}

			});
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public override string ToString()
		{
			return $"Item: {Index}";
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
