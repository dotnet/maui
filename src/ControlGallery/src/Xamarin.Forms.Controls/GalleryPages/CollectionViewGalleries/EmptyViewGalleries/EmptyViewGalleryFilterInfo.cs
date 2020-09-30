using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.EmptyViewGalleries
{
	[Preserve(AllMembers = true)]
	public class EmptyViewGalleryFilterInfo : INotifyPropertyChanged
	{
		string _filter;

		public string Filter
		{
			get => _filter;
			set
			{
				_filter = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}