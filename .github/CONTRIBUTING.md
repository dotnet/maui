# Contributing

Thank you for your interest in contributing to Xamarin.Forms! In this document, we'll outline what you need to know about contributing and how to get started.

## Code of Conduct

Please see our [Code of Conduct](CODE_OF_CONDUCT.md).

## Prerequisite

You will need to complete a Contribution License Agreement before any pull request can be accepted. Review the CLA at https://cla.dotnetfoundation.org/. When you submit a pull request, a CLA assistant bot will confirm you have completed the agreement, or provide you with an opportunity to do so.

## Contributing Code

Check out [A Beginner's Guide for Contributing to Xamarin.Forms](https://devblogs.microsoft.com/xamarin/beginners-guide-contributing-xamarin-forms/).

### What to work on

If you're looking for something to work on, please browse [open issues](https://github.com/xamarin/Xamarin.Forms/issues). Any issue that is not already assigned is up for grabs. You can also look for issues tagged <a href="https://github.com/xamarin/Xamarin.Forms/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22" class="label v-align-text-top labelstyle-159818 linked-labelstyle-159818" data-ga-click="Maintainer label education banner, dismiss, repository_nwo:xamarin/Xamarin.Forms; context:issues; label_name:help wanted; public:true; repo_has_help_wanted_label:true; repo_has_good_first_issue_label:false; shows_go_to_labels:true" data-octo-click="maintainer_label_education" data-octo-dimensions="action:click_label,actor_id:41873,user_id:790012,repository_id:54213490,repository_nwo:xamarin/Xamarin.Forms,context:issues,label_name:help wanted,public:true,repo_has_help_wanted_label:true,repo_has_good_first_issue_label:false,shows_go_to_labels:true" style="background-color: #159818; color: #fff" title="Label: help wanted">help wanted</a> and <a href="https://github.com/xamarin/Xamarin.Forms/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22" class="label v-align-text-top labelstyle-7057ff linked-labelstyle-7057ff" data-ga-click="Maintainer label education banner, dismiss, repository_nwo:xamarin/Xamarin.Forms; context:issues; label_name:good first issue; public:true; repo_has_help_wanted_label:true; repo_has_good_first_issue_label:false; shows_go_to_labels:true" data-octo-click="maintainer_label_education" data-octo-dimensions="action:click_label,actor_id:41873,user_id:790012,repository_id:54213490,repository_nwo:xamarin/Xamarin.Forms,context:issues,label_name:good first issue,public:true,repo_has_help_wanted_label:true,repo_has_good_first_issue_label:false,shows_go_to_labels:true" style="background-color: #7057ff; color: #fff" title="Label: good first issue">good first issue</a>. Before you select an enhancement to work on, see Status of Proposals below. Make sure you're working on something in the Ready For Implementation category!

Follow the style used by the [.NET Foundation](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md), with two primary exceptions:

- We do not use the `private` keyword, as it is the default accessibility level in C#.
- We use hard tabs over spaces.

Read and follow our [Pull Request template](PULL_REQUEST_TEMPLATE.md).

### Pull Request Requirements

We use red-green-refactor test driven development. If you're planning to work on a bug fix, please be sure to create a test case in the UI tests suite (or unit tests, if you're working on Core/XAML code) that proves that the behavior is broken and then proves that the behavior was resolved after your changes. If at all possible, the test should be automated. If the test cannot be automated, then it should include manual testing instructions on screen.

Please check the "Allow edits from maintainers" checkbox on your pull request. This allows us to quickly make minor fixes and resolve conflicts for you.

## Proposals/Enhancements/Suggestions

To propose a change or new feature, open an issue using the [Feature request template](https://github.com/xamarin/Xamarin.Forms/issues/new?assignees=&labels=proposal-open%2C+t%2Fenhancement+%E2%9E%95&template=feature_request.md&title=%5BEnhancement%5D+YOUR+IDEA%21). You may also use the [Spec template](https://github.com/xamarin/Xamarin.Forms/issues/new?assignees=&labels=proposal-open%2C+t%2Fenhancement+%E2%9E%95&template=spec.md&title=%5BSpec%5D++) if you have an idea of what the API should look like.

### Status of Proposals

Proposals (also called Enhancements or Suggestions) will start out in the [Enhancements project](https://github.com/xamarin/Xamarin.Forms/projects/5) and will be sorted into columns based on their current status.

#### Under consideration
This issue is proposed to the community for further support or ideas. Make your votes, voice your opinions, and help develop a specification that someone can work from. A proposal in this column is likely not ready to be worked on yet.

#### Discussion Required
Similar to "Under consideration", except there are clear reasons or concerns about adding this to the platform. This is not quite a rejected state, but this issue requires a lot of problem solving before it should be worked on.

#### Needs Specification
This idea is accepted to be added to Xamarin.Forms. However, it can't be worked on until it has a clear specification, including API changes, sample use cases, etc. See the [Spec template](https://github.com/xamarin/Xamarin.Forms/issues/new?assignees=&labels=proposal-open%2C+t%2Fenhancement+%E2%9E%95&template=spec.md&title=%5BSpec%5D++) for the type of information that is needed.

#### Needs Design Review
The specification is written for this accepted proposal, and now we need to review it to make sure that it is easy to use, extensible, etc.

#### Ready for Implementation
Issues in this column should be ready to implement; all of the required information should be in the issue at this point. Unless the issue has an assignee already, anyone is welcome to pick something from this column!

#### In Progress
Issues that have a matching PR are automatically removed from this project entirely; however, if someone wants to claim an issue and submit a PR later, the issue should be moved to this column so someone else doesn't start working on it at the same time.

#### External
These issues won't involve code that is in the Xamarin.Forms repository, but for one reason or another, it is still tracked here.

#### Closed
Proposals that were closed without being implemented.

## Review Process
All pull requests need to be reviewed and tested by at least two members of the Xamarin.Forms team. We do our best to review pull requests in a timely manner, but please be patient! Two reviewers will be assigned and will start the review process as soon as possible. If there are any changes requested, the contributor should make them at their earliest convenience or let the reviewers know that they are unable to make further contributions. If the pull request requires only minor changes, then someone else may pick it up and finish it. We will do our best to make sure that all credit is retained for contributors. 

Once a pull request has two approvals, it will receive an "approved" label. As long as no UI or unit tests are failing, this pull request can be merged at this time.

## Merge Process
Bug fixes should be targeted at the earliest appropriate branch.
- The _current stable branch_ corresponds to the latest stable version available on NuGet.org. This branch will now only accept regressions or fixes that meet a very high bar and low risk.
- The _current prerelease branch_ corresponds to the latest prerelease version available on NuGet.org. This branch will only accept bug fixes without API changes or breaking changes, with the exception of any API that is under an experimental flag.
- _Master_ corresponds to a version that is not yet tagged. This is also the "nightly" branch. This is where anything that doesn't fit into the stable or prerelease branches should be targeted.

Commits will be merged up from stable to prerelease to master branches on a regular basis (typically every Monday and whenever a new release is tagged).
