using System;

namespace Xamarin.Forms
{
	public class DropEventArgs
	{
		public DropEventArgs(DataPackageView view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			Data = view;
		}

		public DataPackageView Data { get; }
		public bool Handled { get; set; }
	}
}