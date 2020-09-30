using System;
using System.Threading;

using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class CoreBoxViewGalleryPage : CoreGalleryPage<BoxView>
	{
		static readonly object SyncLock = new object();
		static readonly Random Rand = new Random();

		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement(BoxView element)
		{
			lock (SyncLock)
			{
				var red = Rand.NextDouble();
				var green = Rand.NextDouble();
				var blue = Rand.NextDouble();
				element.Color = new Color(red, green, blue);
			}
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var colorContainer = new ViewContainer<BoxView>(Test.BoxView.Color, new BoxView { Color = Color.Pink });

			Add(colorContainer);

			var cornerRadiusContainer = new ViewContainer<BoxView>(Test.BoxView.CornerRadius, new BoxView { Color = Color.Red, CornerRadius = new CornerRadius(0, 12, 12, 0) });

			Add(cornerRadiusContainer);
		}
	}
}