namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 14801, "NullReferenceException in UpdateLeftBarButtonItem", PlatformAffected.Android)]
	public class Issue14801 : Shell
	{
		public Issue14801()
		{
			var mainContent = new ShellContent
			{
				ContentTemplate = new DataTemplate(() => new InitialPage()),
				Title = "Initial",
				Route = "initial"
			};

			Items.Add(mainContent);
			Routing.RegisterRoute("Issue14801_child", typeof(ChildPage));
		}
	}

	file class InitialPage : ContentPage
	{
		public InitialPage()
		{
			Padding = 24;
			Content = new Button
			{
				Text = "Go to child page",
				AutomationId = "goToChildPage",
				Command = new Command(() => Shell.Current.GoToAsync("Issue14801_child"))
			};
		}
	}

	file class ChildPage : ContentPage
	{
		public ChildPage()
		{
			Shell.SetBackButtonBehavior(this, new BackButtonBehavior
			{
				IconOverride = new StreamImageSource
				{
					Stream = async _ =>
					{
						// Wait until page is popped
						while (Parent != null)
						{
							await Task.Delay(60);
						}

						// Give it time to finish the navigation
						await Task.Delay(200);

						// Return the image
						var imageBytes = Convert.FromBase64String(IconB64);
						return new MemoryStream(imageBytes);
					}
				}
			});

			Padding = 24;
			Content = new Button
			{
				Text = "Go back",
				AutomationId = "goBack",
				Command = new Command(() => Shell.Current.GoToAsync(".."))
			};
		}

		const string IconB64 =
			"iVBORw0KGgoAAAANSUhEUgAAAGAAAABgCAYAAADimHc4AAANYElEQVR4Xu1cC3BcVRk+52zSPKAVKSS1AToWR0cEB3zhA7VWS1s7DiJNLa27yT662aZkysOqBYSUN0KppTbZrNlHdrEUAqi8CyNFpwoygC8eOtoy5dVm6UNaSJpm9x6/s0khDXvvPffu3nt3x70zzE3Z/z//f/7vPP7z//+5lFQeRy1AHZVeEU4qADg8CCoAVABw2AIOi6/MgAoADlvAYfGVGVABwLwFPP7QFwR3Mhp+xnwrznKW9QwAAMkxADzOmtG89LIFYMmS5R+uquNp0fXMEG3YtKl7v3kzOMdZtgB4fMHLKWXXC9NxrlyRjEVucM6M5iWXKwC0xd+2ixDaONp1PtAX7fmI+MO8KZzhLEsAWnyhCxBEuWe8yRTKL0j19tznjBnNSy1PAPyhrejyrPHd5pxvTcZ6Zps3hTOcZQeAO9D+ScaVl/KZS6HstFRv18vOmNKc1LIDoMXX1k0oDal0t7svGm43ZwpnuMoKAJ/PNzlLq3dj863Pby4+6OIj02Kx2EFnzGlcalkB4PYFL2aUrdPspkJW9sXDtxs3hTMcZQUAXM//YPSfqm0qvh0u6cecMadxqWUDQKu/bR4n9BGpLnJlbl8s8pgUrcNEZQOAx9/2ECX02zL24pw8mIyFvyND6zRNWQCwNBA4yaW4XqV4ZAyGMwHPsuwpv+rtfV2G3kkaqQ45qaCQ7fG1rYXtLzWiByf81mS0Z5URHidoSx6A5ubmSXWTp76FsT/FiIGwDB0YOrj3xP7+/sNG+OymLXkA3L7QMkZJxJRhOA/0xXqipnhtYip5AHDyfREn39NM2YPzlwDAp0zx2sRU0gDA7/8a/P7fF2iLcxCe+GOBbVjGXtoA+EJ3Iey8SLX3nI+GHCidrE5DNvfFwhdaZsECGy5ZAC4MBBon8SrEfdQfeDrrkYJh8JA61KiQocnww6QplQrn0pel9pQsAG5/27WM0CtVDSucfZbNhSWo4tqudUaAR9SJg9maUjN+TvdSVAqup6t+yvEYsfR49ZHNH4afv0D87vGFHoabOl9jrgwMHtjXBJc0W2r9LUkAkHJciqFxh5axFIXMS8XDWwSNTJyIU7442dtzVwUACQu0+ENPgeyLGhvrDmysR0VFAdp2gDZTY8nahpTlVyXE20ri+AxwBwIfJVnWgK20gRLWyIkyQ2vtF9bB5rsCy0/XeEu1+IIrCGW/0LIe9oI18Jheh4wBoqCmyKWkU729r9hq8QnCig0A9Xq9J2RYTSNRSIOLkQaONxFvQhopx98U/+ENIzZi4zzGaOdHQwx10/r71w2N521uvqSubvLQbqMhi1FAyTvQLQ3d0vhHmuMNwwAkvBlJZ/FmVdkBlsmk4/H4nlGW4jy6ALjd7mNITU2DGKWMuhqhHAyYhSFpzpBjtTl4kwZ0/gS8WXFUU21lHQ5WeQNzCFn/HCHrlVbKF4FW9HkvZKQp5QMYEGPAcYBGBxQB4tjs2lNX9+YjGzYMa+mjC4CY2kiEbJANBVvceU4y9ORkMvxGPjliOdNzSa3Ub3zbwkumhHcgMbSxIAAEc4s/uIBweg9Gfa1dHcgrh5P7sfmep6UDYkcPQs+ce+rYw/khQvnCvmjkIT0ddGfAkQZ+4A1+hjG6BdNMLDMOPfxbyPf+ThuA4LnYjHPuqRMP9rY9isLn3hGPPC8jXxoA0ZjX236yQpUntdw9GaFmaLDrvYx7AFJRUT2X1Ix8KR5OdjDOZsXjXa9J0YPIEACi0dbW1uMUVvMo9oSzZYUUgw4AhABAj0xbMi6pTDtGaLDk/5kpw/MSicR/jfAZBkA0HgwGqw9l6WYsR98zImwCrQIvai/WyvT7noTwLMZcQUJFFixNszz97iSe7o9E3jYiy+/3H3+YVE+HK9nECJ+OsTadM/EmTZTjb0LwN5+GgeQy0m4+Wiw799W6+OJIJDJitC1TABwRghjMjTDST2SFotP7cS5YmHFlXryzt3dAls9KOlTbTR8BUDgHNMGtPAlAXYdN/DhZmRg8NyHQt1qWfiJdQQCIxjz+5QG4W2JpkPL/AcLODKezN8W6d5hV2go+cd6h1cfciwE1V7J9hKNIayoaTknS5yUrGADRKtZceB701+o1mxNl83243bIg0dv9dCHKF4tXzIIsqX5cPvXJB3Et5/xiFH8VBQBhiNbW4BncRR9//9aKrnmGcbZY1Bfrvl+X0kIC4V67GH1YXm8+gH1pTiIR+Ucx1CoaAEIZMZIytHorNuePyyiXOy1SehlCC9oFtzKNmaAxfMBEkt9FRuag+vpNE+KsW4LGt7x0accUV83hh2DYcwwoaXtdPxL+P8Sov1l67+J8Gx8ZnJdKpd410C9d0qLOgCPSZs3qrJoxc3cKp4zFuhqMEcCV++3QgX2LrC6k6uzsZK+8uiuO9V7+bjEnm3fumOZ+8snOjGx/ZOksAeCIcLip18Cr+KmsMliRnsocYgusuvM76unUP4DZ+Q15ncgauJmdsvRG6SwFQCjj9ofclPO4/IGHb+cjfHYyGXnVaGe06EfDKNlHDXg6Iwhnuq1OY1oOgDBKayA0S+HkAQg7VsqonLyFDNn8vt7u56TodYiMBhJzCRpC5tlR0GULALmZgNuNVFEex5LUJGNU7AldSDuukKHVo0GiZiM8M6nLezjZvsEZm2PXbUvbAMiB4A41sGq+VWoZUPiSvnjPnXrGlfldpspCtIOR/3cUcc2xs4jLVgBGQZDbCPmIMqNY+4DIlDFepRn6wMjfglzz+RNzzTIAF0JjOwC5PcEf/C4qChG6UHk4eROZL6mlSrbz8Mj2YPmbqkZPiXJ+Ihr5jWx7xaJzBAAsCbfijHCZBgBFL6jFPnCvZvick7UAHYczex9nANAtvFIu0ktmGzUTrjldCld4rQbf0/B6vmS03ULpbQcAGbVa7qo9qqZnYieynJ95R6znb3qdEzfnD9bW8v6urnf0aN3e4NmMMc3oK80eqkNG65BeW8X83XYAdC9doOYft1o+NOqUqD/Cs0G95y2IqKIUB7fjY+F7tehFwW/dlKmHQFulTse/jqT/H4ppYL22nAAA2SOq/nUrzh8DAKpJERH2VhgLY0P98vjOCS8myzIBraupqDndBp6vaABwOQC4Uc9oxfzddgD0LlzDkFch9nLtxE6ORVmvw/9vVwtrgHcIqcXrd74y7eZ8gTPsAz8Dr+rVVRz+3it5L6aRtdqyHQAUTh3QulKEPN83UXb+xDilKb6O6IVhb4LndKKMYQDEv7CCeVENLaqs33s83rbzKKPqrubo8mfoOqyMPiUDAAJzpyNxrJVJUtL11fVH6ilh+LOgfBijJPd9UKMPNpEUGXFdkkxuFLWcRFRKZEh17m+1B3neM5DnfcGoLLP0ts4AGLQNAsNqyiIc/SxG7ec9nhVTaVXmBsyUAGilkv2qbaISA7+tRk2RuGvMocO/oYPq11SM1B+ZNfp4PlsBwPLTp5UIEZfusHz8Exez8TlK9etJZjoOwz6TVbi3ipIfa+tAUgBLPlljRpnx62uB/IbY9UoGx8LAciFrQ5LfJwbAL8CDOl2VHeWFE2/fmBQlxWbbDJBZf6U0toGoioxMjUaj+2wQZbw21KxS8EAWwgPpN8v/AT5OXsNobqcMqR7O8CE/cnKx2laIsigVjRRPVw3FbJsBxbq9go36MHz5WxA2uO5I2GAsvHElfluF3yYVCoTYi5AMurjQdmT4bQMAp9BnodBnZZRS95LIExlCl6mVNS7xLZ9ZRZU4op74xkRBz3MIzH2uoBYkmW0BYH5HR03D4MggdDLnUiI/AN5LsDneLdMvT6Dt+4gQ3YZlSVRDm3mOOo+YaUCWxxYA3N7QbMaI5s2WfAqL7zygBvP2oVrX1TIRz/FtNLe3H1s/rFyDNjq0A3D5TZXnRC5rU0N0tgCACOhV8OvXGNEMG+yfkBwPFJoczxUDcCVh/DTNr0Zg7hojOpuhtQcAX9sWHH7OlVIQJSkIB/woFQsnpOjliGiLt60VOtwsG0/CzNOMysqJ1aeyAwCKE/Dbmt/0GdUTN2Z4hCrDq41e89Hv5ihF7nqVq/YmdHqZ7n4kmZeQla1GZzkAra3LzuQu1180FeX8eZRJBxAC0KYrtLdj/GNBPrEsfVqrSZrNnpVI/PKvRRKbtxnLAUBnL4KQDSqb7H6qkCvwrWcRoCva9X8Zg4ki3R2v7QqJ/IHalSSxgWNQaH5/QkaWJsiFNqDHj/jPnROrpMW9AHQ66VKGV+HbC2/ptWHl7/i2xYlZVrMWg8T9ATmoirb6c2eWzwAA8MZR/jguOWBkBSYmS6w0skzbyJahIoKiiJh84j16C+qTJupiKQAeT/AUWs125oRiU4PhO4cO7ltfil+uEirm7jWcuutiTM+rYZhcVLaYFXr5BoKlAMD1uxBVzpuwut9NFbYykejS/AifzEi1gyZ31YpUr0dcaSG+fFK0GlXbAUAAbiUQfkHv+w52GNWMDHH7E7PhdCyXt5nhl+GxdAbIKPD/TlMBwOERUAGgAoDDFnBYfGUGVABw2AIOi6/MgAoADlvAYfGVGVABwGELOCy+MgMqADhsAYfF/w8ZXsSdphrvXQAAAABJRU5ErkJggg==";
	}
}
