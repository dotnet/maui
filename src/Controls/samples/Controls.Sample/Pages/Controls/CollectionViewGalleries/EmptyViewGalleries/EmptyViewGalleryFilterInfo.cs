// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.EmptyViewGalleries
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