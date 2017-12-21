# Contributing

Thanks you for your interest in contributing to Xamarin.Forms! In this document we'll outline what you need to know about contributing and how to get started.

## Code of Conduct

Please see our [Code of Conduct](CODE_OF_CONDUCT.md).

## Prerequisite

You will need to complete a Contribution License Agreement before any pull request can be accepted. Complete the CLA at https://cla2.dotnetfoundation.org/.

## Contributing Code

Check out [A Beginner's Guide for Contributing to Xamarin.Forms](https://blog.xamarin.com/beginners-guide-contributing-xamarin-forms/).

### Bug Fixes

If you're looking for something to fix, please browse [open issues](https://github.com/xamarin/Xamarin.Forms/issues). Check for issues tagged <a href="/xamarin/Xamarin.Forms/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22" class="label v-align-text-top labelstyle-159818 linked-labelstyle-159818" data-ga-click="Maintainer label education banner, dismiss, repository_nwo:xamarin/Xamarin.Forms; context:issues; label_name:help wanted; public:true; repo_has_help_wanted_label:true; repo_has_good_first_issue_label:false; shows_go_to_labels:true" data-octo-click="maintainer_label_education" data-octo-dimensions="action:click_label,actor_id:41873,user_id:790012,repository_id:54213490,repository_nwo:xamarin/Xamarin.Forms,context:issues,label_name:help wanted,public:true,repo_has_help_wanted_label:true,repo_has_good_first_issue_label:false,shows_go_to_labels:true" style="background-color: #159818; color: #fff" title="Label: help wanted">help wanted</a> and <a href="/xamarin/Xamarin.Forms/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22" class="label v-align-text-top labelstyle-7057ff linked-labelstyle-7057ff" data-ga-click="Maintainer label education banner, dismiss, repository_nwo:xamarin/Xamarin.Forms; context:issues; label_name:good first issue; public:true; repo_has_help_wanted_label:true; repo_has_good_first_issue_label:false; shows_go_to_labels:true" data-octo-click="maintainer_label_education" data-octo-dimensions="action:click_label,actor_id:41873,user_id:790012,repository_id:54213490,repository_nwo:xamarin/Xamarin.Forms,context:issues,label_name:good first issue,public:true,repo_has_help_wanted_label:true,repo_has_good_first_issue_label:false,shows_go_to_labels:true" style="background-color: #7057ff; color: #fff" title="Label: good first issue">good first issue</a>.

Follow the style used by the [.NET Foundation](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md), with two primary exceptions:

- We do not use the `private` keyword as it is the default accessibility level in C#.
- We use hard tabs over spaces. You can change this setting in VS 2015 via `Tools > Options` and navigating to `Text Editor > C#` and selecting the "Keep tabs" radio option.

Read and follow our [Pull Request template](https://github.com/xamarin/Xamarin.Forms/blob/master/PULL_REQUEST_TEMPLATE.md)

### Proposals

To propose a change or new feature, review the guidance below and then [open an issue using this template](https://github.com/xamarin/Xamarin.Forms/issues/new?labels=enhancement,proposal-open&body=%23%23Summary%0APlease%20provide%20a%20brief%20summary%20of%20your%20proposal.%20Two%20to%20three%20sentences%20is%20best%20here.%0A%0A%23%23API%20Changes%0A%0AInclude%20a%20list%20of%20all%20API%20changes%2C%20additions%2C%20subtractions%20as%20would%20be%20required%20by%20your%20proposal.%20These%20APIs%20should%20be%20considered%20placeholders%2C%20so%20the%20naming%20is%20not%20as%20important%20as%20getting%20the%20concepts%20correct.%20If%20possible%20you%20should%20include%20some%20%22example%22%20code%20of%20usage%20of%20your%20new%20API.%0A%0Ae.g.%0A%0AIn%20order%20to%20facilitate%20the%20new%20Shiny%20Button%20api%2C%20a%20bool%20is%20added%20to%20the%20Button%20class.%20This%20is%20done%20as%20a%20bool%20because%20it%20is%20simpler%20to%20data%20bind%20and%20other%20reasons...%0A%0A%20%20%20%20var%20button%20%3D%20new%20Button%20()%3B%0A%20%20%20%20button.MakeShiny%20%3D%20true%3B%20%2F%2F%20new%20API%0A%0AThe%20MakeShiny%20API%20works%20even%20if%20the%20button%20is%20already%20visible.%0A%0A%23%23Intended%20Use%20Case%0AProvide%20a%20detailed%20example%20of%20where%20your%20proposal%20would%20be%20used%20and%20for%20what%20purpose.).

#### Non-Starter Topics
The following topics should generally not be proposed for discussion as they are non-starters:

* Port to WPF/UWP naming
* Large renames of APIs
* Large non-backward-compatible breaking changes
* Platform-Specifics which can be accomplished without changing Xamarin.Forms.Core
* Avoid clutter posts like "+1" which do not serve to further the conversation

#### Proposal States
##### Open
Open proposals are still under discussion. Please leave your concrete, constructive feedback on this proposal. +1s and other clutter posts which do not add to the discussion will be removed.

##### Accepted
Accepted proposals are proposals that both the community and core Xamarin.Forms agree should be a part of Xamarin.Forms. These proposals are ready for implementation, but do not yet have a developer actively working on them. These proposals are available for anyone to work on, both community and the core Xamarin.Forms team.

If you wish to start working on an accepted proposal, please reply to the thread so we can mark you as the implementor and change the title to In Progress. This helps to avoid multiple people working on the same thing. If you decide to work on this proposal publicly, feel free to post a link to the branch as well for folks to follow along.

###### What "Accepted" does mean
* Any community member is welcome to work on the idea.
* The core Xamarin.Forms team _may_ consider working on this idea on their own, but has not done so until it is marked "In Progress" with a team member assigned as the implementor.
* Any pull request implementing the proposal will be welcomed with an API and code review.

###### What "Accepted" does not mean
* The proposal will ever be implemented, either by a community member or by the core Xamarin.Forms team.
* The core Xamarin.Forms team is committing to implementing a proposal, even if nobody else does. Accepted proposals simply mean that the core Xamarin.Forms team and the community agree that this proposal should be a part of Xamarin.Forms.

##### In Progress
Once a developer has begun work on a proposal, either from the core Xamarin.Forms team or a community member, the proposal is marked as in progress with the implementors name and (possibly) a link to a development branch to follow along with progress.

#### Rejected
Rejected proposals will not be implemented or merged into Xamarin.Forms. Once a proposal is rejected, the thread will be closed and the conversation is considered completed, pending considerable new information or changes..
