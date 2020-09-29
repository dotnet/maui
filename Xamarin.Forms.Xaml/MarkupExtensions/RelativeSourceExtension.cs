using System;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty("Mode")]
	[AcceptEmptyServiceProvider]
	public sealed class RelativeSourceExtension : IMarkupExtension<RelativeBindingSource>
	{
		public RelativeBindingSourceMode Mode
		{
			get;
			set;
		}

		public int AncestorLevel
		{
			get;
			set;
		}

		public Type AncestorType
		{
			get;
			set;
		}

		RelativeBindingSource IMarkupExtension<RelativeBindingSource>.ProvideValue(IServiceProvider serviceProvider)
		{
			if (AncestorType != null)
			{
				RelativeBindingSourceMode actualMode;

				if (Mode != RelativeBindingSourceMode.FindAncestor &&
					Mode != RelativeBindingSourceMode.FindAncestorBindingContext)
				{
					// Note to documenters:

					// This permits "{Binding Source={RelativeSource AncestorType={x:Type MyType}}}" syntax
					// where Mode hasn't been explicitly set, consistent with WPF.

					// Also, we assume FindAncestor is meant if the ancestor type is a visual 
					// Element, otherwise assume FindAncestorBindingContext is intended. (The
					// mode can also be explicitly set in XAML)
					actualMode = typeof(Element).GetTypeInfo().IsAssignableFrom(AncestorType.GetTypeInfo())
						? RelativeBindingSourceMode.FindAncestor
						: RelativeBindingSourceMode.FindAncestorBindingContext;
				}
				else
				{
					actualMode = Mode;
				}

				return new RelativeBindingSource(actualMode, AncestorType, AncestorLevel);
			}
			else if (Mode == RelativeBindingSourceMode.FindAncestor ||
					Mode == RelativeBindingSourceMode.FindAncestorBindingContext)
			{
				throw new XamlParseException(
					$"{nameof(RelativeBindingSourceMode.FindAncestor)} and " +
					$"{nameof(RelativeBindingSourceMode.FindAncestorBindingContext)} " +
					$"require {nameof(AncestorType)}.");
			}
			else if (Mode == RelativeBindingSourceMode.Self)
			{
				return RelativeBindingSource.Self;
			}
			else if (Mode == RelativeBindingSourceMode.TemplatedParent)
			{
				return RelativeBindingSource.TemplatedParent;
			}
			else
			{
				throw new XamlParseException($"Invalid {nameof(Mode)}");
			}
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<RelativeBindingSource>).ProvideValue(serviceProvider);
		}
	}
}