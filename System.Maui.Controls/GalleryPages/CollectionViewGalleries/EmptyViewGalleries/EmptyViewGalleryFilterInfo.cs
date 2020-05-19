using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.CollectionViewGalleries.EmptyViewGalleries
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