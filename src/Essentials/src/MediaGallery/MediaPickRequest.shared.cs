using System.Linq;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Media
{
	/// <summary>
	/// Request for picking a media file
	/// </summary>
	public record MediaPickRequest
	{
		/// <summary><inheritdoc cref="MediaPickRequest" path="/summary"/></summary>
		/// <param name="title"><inheritdoc cref="Title"  path="/summary"/></param>
		/// <param name="selectionLimit"><inheritdoc cref="SelectionLimit"  path="/summary"/></param>
		/// <param name="presentationSourceBounds"><inheritdoc cref="PresentationSourceBounds"  path="/summary"/></param>
		/// <param name="types"><inheritdoc cref="Types"  path="/summary"/></param>
		public MediaPickRequest(
			string title = null, 
			int selectionLimit = 1,
			Rect  presentationSourceBounds = default,
			params MediaFileType[] types)
		{
			Title = title;
			SelectionLimit = selectionLimit > 0 ? selectionLimit : 1;
			PresentationSourceBounds = presentationSourceBounds;
			Types = types?.Length > 0
				? types.Distinct().ToArray()
				: new[] { MediaFileType.Image, MediaFileType.Video };
		}

		/// <summary>
		/// Gets or sets the title that is displayed when picking media. Only for Android below API 33
		/// </summary>
		public string Title { get; }

		/// <summary>
		/// Maximum count of files to pick. On Android below API 33 the option just sets multiple pick allowed.
		/// </summary>
		public int SelectionLimit { get; }

		/// <summary>
		/// Gets or sets the source rectangle to display the Picker UI from. This is only used on iPad currently.
		/// </summary>
		public Rect PresentationSourceBounds { get; }

		/// <summary>
		/// Media file types available for picking.
		/// </summary>
		public MediaFileType[] Types { get; }
	}
}