using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Text
{
	public class AttributedTextRunComparer : IComparer<IAttributedTextRun>
	{
		public static readonly AttributedTextRunComparer Instance = new AttributedTextRunComparer();

		public int Compare(IAttributedTextRun first, IAttributedTextRun second)
		{
			if (first.Start < second.Start)
			{
			{
				return -1;
			}
			}

			if (first.Start == second.Start)
			{
				if (first.Length < second.Length)

/* Unmerged change from project 'Graphics(net8.0)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-ios)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-maccatalyst)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-android)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-windows10.0.19041.0)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-windows10.0.20348.0)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-macos)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net7.0)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-ios)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-maccatalyst)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-android)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-windows10.0.19041.0)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-windows10.0.20348.0)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-macos)'
Before:
					return -1;
After:
				{
					return -1;
				}
*/
				{

/* Unmerged change from project 'Graphics(net8.0)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-ios)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-maccatalyst)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-android)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-windows10.0.19041.0)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-windows10.0.20348.0)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net8.0-macos)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net7.0)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-ios)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-maccatalyst)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-android)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-windows10.0.19041.0)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-windows10.0.20348.0)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/

/* Unmerged change from project 'Graphics(net7.0-macos)'
Before:
					return 0;
After:
				{
					return 0;
				}
*/
					return -1;
				}

				if (first.Length == second.Length)
				{
					return 0;
				}
			}

			return 1;
		}
	}
}
