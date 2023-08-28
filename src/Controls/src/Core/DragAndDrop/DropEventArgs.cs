#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DropEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.DropEventArgs']/Docs/*" />
	public class DropEventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/DropEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DropEventArgs(DataPackageView view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));
			Data = view;
		}

		internal DropEventArgs(DataPackageView view, PlatformDropEventArgs platformArgs) : this(view)
		{
			PlatformArgs = platformArgs;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DropEventArgs.xml" path="//Member[@MemberName='Data']/Docs/*" />
		public DataPackageView Data { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/DropEventArgs.xml" path="//Member[@MemberName='Handled']/Docs/*" />
		public bool Handled { get; set; }

		/// <summary>
		/// Gets the platform-specific arguments associated with the <see cref="DropEventArgs"/>.
		/// </summary>
		public PlatformDropEventArgs PlatformArgs { get; }
	}
}
