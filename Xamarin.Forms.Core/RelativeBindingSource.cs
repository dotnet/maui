using System;

namespace Xamarin.Forms
{
	public sealed class RelativeBindingSource
	{
		static RelativeBindingSource _self;
		static RelativeBindingSource _templatedParent;

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

		public RelativeBindingSourceMode Mode
		{
			get;
		}

		public Type AncestorType
		{
			get;
		}

		public int AncestorLevel
		{
			get;
		}

		public static RelativeBindingSource Self
		{
			get
			{
				return _self ?? (_self = new RelativeBindingSource(RelativeBindingSourceMode.Self));
			}
		}

		public static RelativeBindingSource TemplatedParent
		{
			get
			{
				return _templatedParent ?? (_templatedParent = new RelativeBindingSource(RelativeBindingSourceMode.TemplatedParent));
			}
		}
	}
}