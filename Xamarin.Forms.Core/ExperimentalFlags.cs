using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xamarin.Forms
{
	static class ExperimentalFlags
	{
		internal const string ShellUWPExperimental = "Shell_UWP_Experimental";
		internal const string MarkupExperimental = "Markup_Experimental";
		internal const string RadioButtonExperimental = "RadioButton_Experimental";

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void VerifyFlagEnabled(
			string coreComponentName,
			string flagName,
			string constructorHint = null,
			[CallerMemberName] string memberName = "")
		{
			if (DesignMode.IsDesignModeEnabled)
			{
				return;
			}

			if (Device.Flags == null || !Device.Flags.Contains(flagName))
			{
				if (!String.IsNullOrEmpty(memberName))
				{
					if (!String.IsNullOrEmpty(constructorHint))
					{
						constructorHint = constructorHint + " ";
					}

					var call = $"('{constructorHint}{memberName}')";

					var errorMessage = $"The class, property, or method you are attempting to use {call} is part of "
										+ $"{coreComponentName}; to use it, you must opt-in by calling "
										+ $"Forms.SetFlags(\"{flagName}\") before calling Forms.Init().";
					throw new InvalidOperationException(errorMessage);
				}

				var genericErrorMessage =
					$"To use {coreComponentName} or associated classes, you must opt-in by calling "
					+ $"Forms.SetFlags(\"{flagName}\") before calling Forms.Init().";
				throw new InvalidOperationException(genericErrorMessage);
			}
		}
	}
}