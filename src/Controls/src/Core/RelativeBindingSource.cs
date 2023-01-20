#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.RelativeBindingSource']/Docs/*" />
	public sealed class RelativeBindingSource
	{
		static RelativeBindingSource _self;
		static RelativeBindingSource _templatedParent;

		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSource.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public RelativeBindingSource(
			RelativeBindingSourceMode mode,
			Type ancestorType = null,
			int ancestorLevel = 1)
		{
			if ((mode == RelativeBindingSourceMode.FindAncestor ||
				 mode == RelativeBindingSourceMode.FindAncestorBindingContext) &&
				ancestorType == null)
			{
				throw new InvalidOperationException(
					$"{nameof(RelativeBindingSourceMode.FindAncestor)} and " +
					$"{nameof(RelativeBindingSourceMode.FindAncestorBindingContext)} " +
					$"require non-null {nameof(AncestorType)}");
			}

			Mode = mode;
			AncestorType = ancestorType;
			AncestorLevel = ancestorLevel;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSource.xml" path="//Member[@MemberName='Mode']/Docs/*" />
		public RelativeBindingSourceMode Mode
		{
			get;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSource.xml" path="//Member[@MemberName='AncestorType']/Docs/*" />
		public Type AncestorType
		{
			get;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSource.xml" path="//Member[@MemberName='AncestorLevel']/Docs/*" />
		public int AncestorLevel
		{
			get;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSource.xml" path="//Member[@MemberName='Self']/Docs/*" />
		public static RelativeBindingSource Self
		{
			get
			{
				return _self ?? (_self = new RelativeBindingSource(RelativeBindingSourceMode.Self));
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/RelativeBindingSource.xml" path="//Member[@MemberName='TemplatedParent']/Docs/*" />
		public static RelativeBindingSource TemplatedParent
		{
			get
			{
				return _templatedParent ?? (_templatedParent = new RelativeBindingSource(RelativeBindingSourceMode.TemplatedParent));
			}
		}
	}
}
