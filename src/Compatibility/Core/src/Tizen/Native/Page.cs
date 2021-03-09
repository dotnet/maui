using System;
using System.Collections.Generic;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	/// <summary>
	/// A basic page which can hold a single view.
	/// </summary>
	public class Page : Background, IContainable<EvasObject>
	{
		/// <summary>
		/// The name of the part to be used when setting content.
		/// </summary>
		[Obsolete("ContentPartName is obsolete. Please use the ThemeConstants.Background.Parts.Overlay instead.")]
		public const string ContentPartName = ThemeConstants.Background.Parts.Overlay;

		/// <summary>
		/// Exposes the Children property, mapping it to the _canvas' Children property.
		/// </summary>
		public new IList<EvasObject> Children => Forms.UseFastLayout ? EvasFormsCanvas?.Children : Canvas?.Children;

		/// <summary>
		/// The canvas, used as a container for other objects.
		/// </summary>
		/// <remarks>
		/// The canvas holds all the Views that the ContentPage is composed of.
		/// </remarks>
		internal Container _canvas;

		EvasFormsCanvas EvasFormsCanvas => _canvas as EvasFormsCanvas;

		Canvas Canvas => _canvas as Canvas;

		/// <summary>
		/// Initializes a new instance of the ContentPage class.
		/// </summary>
		public Page(EvasObject parent) : base(parent)
		{
			if (Forms.UseFastLayout)
				_canvas = new EvasFormsCanvas(this);
			else
				_canvas = new Canvas(this);
			this.SetOverlayPart(_canvas);
		}

		/// <summary>
		/// Allows custom handling of events emitted when the layout has been updated.
		/// </summary>
		public event EventHandler<LayoutEventArgs> LayoutUpdated
		{
			add
			{
				if (Forms.UseFastLayout)
					EvasFormsCanvas.LayoutUpdated += value;
				else
					Canvas.LayoutUpdated += value;

			}
			remove
			{
				if (Forms.UseFastLayout)
					EvasFormsCanvas.LayoutUpdated -= value;
				else
					Canvas.LayoutUpdated -= value;
			}
		}

		/// <summary>
		/// Handles the disposing of a ContentPage
		/// </summary>
		/// <remarks>
		/// Takes the proper care of discarding the canvas, then calls the base method.
		/// </remarks>
		protected override void OnUnrealize()
		{
			_canvas.Unrealize();
			base.OnUnrealize();
		}
	}
}
