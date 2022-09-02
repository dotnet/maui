using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.NavigationRequestedEventArgs']/Docs" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NavigationRequestedEventArgs : NavigationEventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestedEventArgs.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public NavigationRequestedEventArgs(Page page, bool animated) : base(page)
		{
			Animated = animated;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestedEventArgs.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public NavigationRequestedEventArgs(Page page, Page before, bool animated) : this(page, animated)
		{
			BeforePage = before;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestedEventArgs.xml" path="//Member[@MemberName='Animated']/Docs" />
		public bool Animated { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestedEventArgs.xml" path="//Member[@MemberName='BeforePage']/Docs" />
		public Page BeforePage { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestedEventArgs.xml" path="//Member[@MemberName='Task']/Docs" />
		public Task<bool> Task { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NavigationRequestedEventArgs.xml" path="//Member[@MemberName='RequestType']/Docs" />
		public NavigationRequestType RequestType { get; set; } = NavigationRequestType.Unknown;
	}
}