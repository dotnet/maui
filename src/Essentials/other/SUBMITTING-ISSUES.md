If you think you've found a bug to report, before doing anything else you should check the [Issues](https://github.com/xamarin/Essentials/issues) page to see if an issue has already been filed! This can help reduce the number of duplicate reports.

If you're ready to submit a new issue, taking the following steps helps expedite the bug triage process:

* Validate that the issue is not resolved with the latest stable, pre-release, or nightly builds.

* If you are able and time allows, create a minimized reproduction project using only the code necessary to reproduce the issue; this helps us as we then know precisely what code is being used.

* Before compressing the reproduction project, make certain to clean the solution as well as manually delete any bin/obj folders, e.g.:

		[ProjectRoot]/ProjectName/bin
		[ProjectRoot]/ProjectName/obj
		[ProjectRoot]/ProjectName.Android/bin
		[ProjectRoot]/ProjectName.Android/obj
		[ProjectRoot]/ProjectName.iOS/bin
		[ProjectRoot]/ProjectName.iOS/obj
		[ProjectRoot]/ProjectName.UWP/bin
		[ProjectRoot]/ProjectName.UWP/obj
		
	Please make certain to remove the packages folder as well to cut down on size.

* Please make note if the issue only occurs on particular platforms, so we can run reproductions on each and compare.

* In the case of a regression, please confirm as such as well as the last working, pre-regression build you may be aware of, if possible. Providing a reproduction with the working package installed to begin with also helps, as we can then simply upgrade it and compare against the latest stable to confirm.

* In some cases, issues may only occur on specific devices, with specific versions of the OS, or when targeting specific OS versions within the project itself. Some issues also occur on devices, but not on simulators. This information is incredibly helpful.

* Please upload reproductions directly to the report as an attachment; this guarantees future access to the file.

# Android Specific Concerns #

Please let us know any issues which may stem from different versions of the support library packages (e.g. Xamarin.Android.Support.v7.AppCompat), as sometimes bugs may only occur when specific versions are used.

# iOS Specific Concerns #

To be added

# UWP-specific Concerns #

Please make note if an issue appears hardware-specific (slower machines, multiple monitors, etc.), due to the desktop component.

# Contributing #

Think you may have a bug fix, or you'd like to work on fixing one yourself? We greatly welcome PRs with fixes from the community! Check out the [Contribute page](CONTRIBUTING.md) for more info.
