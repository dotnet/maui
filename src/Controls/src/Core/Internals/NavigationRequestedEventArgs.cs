#nullable disable
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by platform renderers.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NavigationRequestedEventArgs : NavigationEventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestedEventArgs.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public NavigationRequestedEventArgs(Page page, bool animated) : base(page)
		{
			Animated = animated;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestedEventArgs.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
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