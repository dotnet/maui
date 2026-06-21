#nullable disable
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by platform renderers.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NavigationRequestedEventArgs : NavigationEventArgs
	{
		/// <summary>Creates navigation event args for the specified page.</summary>
		public NavigationRequestedEventArgs(Page page, bool animated) : base(page)
		{
			Animated = animated;
		}

		/// <summary>Creates navigation event args for inserting before another page.</summary>
		public NavigationRequestedEventArgs(Page page, Page before, bool animated) : this(page, animated)
		{
			BeforePage = before;
		}

		/// <summary>For internal use by platform renderers.</summary>
		public bool Animated { get; set; }

		/// <summary>For internal use by platform renderers.</summary>
		public Page BeforePage { get; set; }

		/// <summary>For internal use by platform renderers.</summary>
		public Task<bool> Task { get; set; }

		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public NavigationRequestType RequestType { get; set; } = NavigationRequestType.Unknown;
	}
}