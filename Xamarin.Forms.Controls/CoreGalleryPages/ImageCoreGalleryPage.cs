using System;

using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class ImageCoreGalleryPage : CoreGalleryPage<Image>
	{
		static readonly Random Rand = new Random ();

		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement (Image element)
		{
//			var sourceIndex = rand.Next (0, 3);
//
//			var sources = new [] {
//				ImageSource.FromFile ("oasis.jpg"),
//				//ImageSource.FromUri (new Uri("http://www.nasa.gov/sites/default/files/styles/1600x1200_autoletterbox/public/images/298773main_EC02-0282-3_full.jpg")),
//				//ImageSource.FromResource ("Xamarin.Forms.Controls.ControlGalleryPages.crimson.jpg")
//			};
			
			//element.Source = sources[sourceIndex];
			element.Source = "oasissmall.jpg";
		}

		protected override void Build (StackLayout stackLayout)
		{
			base.Build (stackLayout);

			var aspectFillContainer = new ViewContainer<Image> (Test.Image.AspectFill, new Image { Aspect = Aspect.AspectFill });
			var aspectFitContainer = new ViewContainer<Image> (Test.Image.AspectFit, new Image { Aspect = Aspect.AspectFit });
			var fillContainer = new ViewContainer<Image> (Test.Image.Fill, new Image { Aspect = Aspect.Fill });
			var isLoadingContainer = new StateViewContainer<Image> (Test.Image.IsLoading, new Image ());
			var isOpaqueContainer = new StateViewContainer<Image> (Test.Image.IsOpaque, new Image ());

			InitializeElement (aspectFillContainer.View);
			InitializeElement (aspectFitContainer.View);
			InitializeElement (fillContainer.View);
			InitializeElement (isLoadingContainer.View);
			InitializeElement (isOpaqueContainer.View);
			
			var sourceContainer = new ViewContainer<Image> (Test.Image.Source, new Image { Source = "http://sethrosetter.com/images/projects/bezierdraw/bezierdraw_5.jpg" });
		
			Add (aspectFillContainer);
			Add (aspectFitContainer);
			Add (fillContainer);
			Add (isLoadingContainer);
			Add (isOpaqueContainer);
			Add (sourceContainer);
		}
	}
}