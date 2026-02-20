using System;
using System.Reflection;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Provides a XAML markup extension that returns a <see cref="RelativeBindingSource"/> for relative bindings.
	/// </summary>
	[ContentProperty("Mode")]
	[AcceptEmptyServiceProvider]
	public sealed class RelativeSourceExtension : IMarkupExtension<RelativeBindingSource>
	{
		/// <summary>
		/// Gets or sets the mode of the relative binding source.
		/// </summary>
		public RelativeBindingSourceMode Mode
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the level of ancestor to look for when Mode is FindAncestor or FindAncestorBindingContext.
		/// </summary>
		public int AncestorLevel
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the type of ancestor to look for when Mode is FindAncestor or FindAncestorBindingContext.
		/// </summary>
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
					actualMode = typeof(Element).IsAssignableFrom(AncestorType)
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
