#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines an interface for accessing and managing platform-specific fonts.
	/// </summary>
	public interface IPlatformFonts
	{
		/// <summary>
		/// Gets the default platform font.
		/// </summary>
		IFont Default { get; }

		/// <summary>
		/// Gets the default bold platform font.
		/// </summary>
		IFont DefaultBold { get; }

		/// <summary>
		/// Registers fonts with the specified alias.
		/// </summary>
		/// <param name="alias">The alias to associate with the font sources.</param>
		/// <param name="sources">The font sources to register.</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="alias"/> or <paramref name="sources"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when <paramref name="sources"/> is empty.</exception>
		void Register(string alias, params FontSource[] sources);

		/// <summary>
		/// Gets the platform-specific font object for the specified font.
		/// </summary>
		/// <param name="font">The font to get the platform-specific object for.</param>
		/// <returns>A platform-specific font object.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="font"/> is null.</exception>
		object Get(IFont font);

		/// <summary>
		/// Gets the platform-specific font object for the specified alias, weight, and style.
		/// </summary>
		/// <param name="alias">The font alias.</param>
		/// <param name="weight">The font weight (default is Normal).</param>
		/// <param name="fontStyleType">The font style type (default is Normal).</param>
		/// <returns>A platform-specific font object.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="alias"/> is null.</exception>
		/// <exception cref="ArgumentException">Thrown when the specified alias is not registered.</exception>
		object Get(string alias, int weight = FontWeights.Normal, FontStyleType fontStyleType = FontStyleType.Normal);
	}
}
