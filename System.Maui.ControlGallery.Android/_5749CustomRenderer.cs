using Android.Content;
using System.Maui;
using System.Maui.ControlGallery.Android;
using System.Maui.Controls.Issues;
using System.Maui.Platform.Android;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CustomHorizontalListview), typeof(HorizontalListviewRendererAndroid))]
namespace System.Maui.ControlGallery.Android
{
	public class HorizontalListviewRendererAndroid : ScrollViewRenderer
	{
		public HorizontalListviewRendererAndroid(Context context) : base(context)
		{
		}
		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			var element = e.NewElement as CustomHorizontalListview;
			element?.Render();

			if (e.OldElement != null)
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;

			e.NewElement.PropertyChanged += OnElementPropertyChanged;

		}

		protected void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			
		}
	}
}