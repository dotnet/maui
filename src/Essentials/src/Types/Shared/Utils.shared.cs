using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel
{
	static class Utils
	{
		internal static Version ParseVersion(string version)
		{
			if (Version.TryParse(version, out var number))

/* Unmerged change from project 'Essentials(net8.0)'
Before:
				return number;
After:
			{
				return number;
			}
*/

/* Unmerged change from project 'Essentials(net8.0-maccatalyst)'
Before:
				return number;
After:
			{
				return number;
			}
*/

/* Unmerged change from project 'Essentials(net8.0-windows10.0.19041.0)'
Before:
				return number;
After:
			{
				return number;
			}
*/

/* Unmerged change from project 'Essentials(net8.0-windows10.0.20348.0)'
Before:
				return number;
After:
			{
				return number;
			}
*/

/* Unmerged change from project 'Essentials(net7.0)'
Before:
				return number;
After:
			{
				return number;
			}
*/

/* Unmerged change from project 'Essentials(net7.0-ios)'
Before:
				return number;
After:
			{
				return number;
			}
*/

/* Unmerged change from project 'Essentials(net7.0-maccatalyst)'
Before:
				return number;
After:
			{
				return number;
			}
*/

/* Unmerged change from project 'Essentials(net7.0-android)'
Before:
				return number;
After:
			{
				return number;
			}
*/

/* Unmerged change from project 'Essentials(net7.0-windows10.0.19041.0)'
Before:
				return number;
After:
			{
				return number;
			}
*/

/* Unmerged change from project 'Essentials(net7.0-windows10.0.20348.0)'
Before:
				return number;
After:
			{
				return number;
			}
*/
			{

/* Unmerged change from project 'Essentials(net8.0)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/

/* Unmerged change from project 'Essentials(net8.0-maccatalyst)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/

/* Unmerged change from project 'Essentials(net8.0-windows10.0.19041.0)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/

/* Unmerged change from project 'Essentials(net8.0-windows10.0.20348.0)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/

/* Unmerged change from project 'Essentials(net7.0)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/

/* Unmerged change from project 'Essentials(net7.0-ios)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/

/* Unmerged change from project 'Essentials(net7.0-maccatalyst)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/

/* Unmerged change from project 'Essentials(net7.0-android)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/

/* Unmerged change from project 'Essentials(net7.0-windows10.0.19041.0)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/

/* Unmerged change from project 'Essentials(net7.0-windows10.0.20348.0)'
Before:
				return new Version(major, 0);
After:
			{
				return new Version(major, 0);
			}
*/
				return number;
			}

			if (int.TryParse(version, out var major))
			{
				return new Version(major, 0);
			}

			return new Version(0, 0);
		}

		internal static CancellationToken TimeoutToken(CancellationToken cancellationToken, TimeSpan timeout)
		{
			// create a new linked cancellation token source
			var cancelTokenSrc = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

			// if a timeout was given, make the token source cancel after it expires
			if (timeout > TimeSpan.Zero)
			{
				cancelTokenSrc.CancelAfter(timeout);
			}

			// our Cancel method will handle the actual cancellation logic
			return cancelTokenSrc.Token;
		}

		internal static async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeSpan)
		{
			var retTask = await Task.WhenAny(task, Task.Delay(timeSpan))
				.ConfigureAwait(false);

			return retTask is Task<T> ? task.Result : default(T);
		}
	}
}
