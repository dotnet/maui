using ElmSharp;
using Samples.Tizen;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xamarin.Forms.Platform.Tizen;

[assembly: ExportCell(typeof(ViewCell), typeof(CustomViewCellRenderer))]

namespace Samples.Tizen
{
	public sealed class CustomViewCellRenderer : ViewCellRenderer
	{
		protected override EvasObject OnGetContent(Cell cell, string part)
		{
			var view = base.OnGetContent(cell, part);
			view.PropagateEvents = true;
			return view;
		}
	}
}
