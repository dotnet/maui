using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.Android;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.Android;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CustomHorizontalListview), typeof(HorizontalListviewRendererAndroid))]
namespace Xamarin.Forms.ControlGallery.Android
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