# Contributing

Thanks you for your interest in contributing to Microsoft Caboodle! In this document we'll outline what you need to know about contributing and how to get started.

## Code of Conduct

Please see our [Code of Conduct](CODE_OF_CONDUCT.md).

## Prerequisite

You will need to complete a Contribution License Agreement before any pull request can be accepted. Complete the CLA at https://cla2.dotnetfoundation.org/.

## Contributing Code

Check out [A Beginner's Guide for Contributing to Microsoft Caboodle](https://github.com/xamarin/Caboodle/wiki/A-Beginner's-Guide-for-Contributing-to-Microsoft-Caboodle).

### Bug Fixes

If you're looking for something to fix, please browse [open issues](https://github.com/xamarin/Caboodle/issues). 

Follow the style used by the [.NET Foundation](https://github.com/dotnet/corefx/blob/master/Documentation/coding-guidelines/coding-style.md), with two primary exceptions:

- We do not use the `private` keyword as it is the default accessibility level in C#.
- We will **not** use `_` or `_s` as a prefix for internal or private field names
- We will use `camelCaseFieldName` for naming internal or private fields in both instance and static implementations

Read and follow our [Pull Request template](https://github.com/xamarin/Caboodle/blob/master/PULL_REQUEST_TEMPLATE.md)

### Proposals

To propose a change or new feature, review the guidance below and then [open an issue using this template](https://github.com/xamarin/Caboodle/issues/new).

#### Non-Starter Topics
The following topics should generally not be proposed for discussion as they are non-starters:

* Large renames of APIs
* Large non-backward-compatible breaking changes
* Platform-Specifics which can be accomplished without changing Microsoft Caboodle
* Avoid clutter posts like "+1" which do not serve to further the conversation

#### Proposal States
##### Open
Open proposals are still under discussion. Please leave your concrete, constructive feedback on this proposal. +1s and other clutter posts which do not add to the discussion will be removed.

##### Accepted
Accepted proposals are proposals that both the community and core Microsoft Caboodle agree should be a part of Microsoft Caboodle. These proposals are ready for implementation, but do not yet have a developer actively working on them. These proposals are available for anyone to work on, both community and the core Microsoft Caboodle team.

If you wish to start working on an accepted proposal, please reply to the thread so we can mark you as the implementor and change the title to In Progress. This helps to avoid multiple people working on the same thing. If you decide to work on this proposal publicly, feel free to post a link to the branch as well for folks to follow along.

###### What "Accepted" does mean
* Any community member is welcome to work on the idea.
* The core Microsoft Caboodle team _may_ consider working on this idea on their own, but has not done so until it is marked "In Progress" with a team member assigned as the implementor.
* Any pull request implementing the proposal will be welcomed with an API and code review.

###### What "Accepted" does not mean
* The proposal will ever be implemented, either by a community member or by the core Microsoft Caboodle team.
* The core Microsoft Caboodle team is committing to implementing a proposal, even if nobody else does. Accepted proposals simply mean that the core Microsoft Caboodle team and the community agree that this proposal should be a part of Microsoft Caboodle.

##### In Progress
Once a developer has begun work on a proposal, either from the core Microsoft Caboodle team or a community member, the proposal is marked as in progress with the implementors name and (possibly) a link to a development branch to follow along with progress.

#### Rejected
Rejected proposals will not be implemented or merged into Microsoft Caboodle. Once a proposal is rejected, the thread will be closed and the conversation is considered completed, pending considerable new information or changes.
