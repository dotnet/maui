# Contributing

Thank you for your interest in contributing to Microsoft.Maui! In this document, we'll outline what you need to know about contributing and how to get started.

## Code of Conduct

Please see our [Code of Conduct](CODE_OF_CONDUCT.md).

## Prerequisite

You will need to complete a Contribution License Agreement before any pull request can be accepted. Review the CLA at https://cla.dotnetfoundation.org/. When you submit a pull request, a CLA assistant bot will confirm you have completed the agreement, or provide you with an opportunity to do so.

## Contributing Code

Currently, we are in the beginning phases of building up MAUI. Yet, we are still very excited for you to join us during this exciting time :)

### What to work on

If you're looking for something to work on, please browse our [Handler Property Backlog](https://github.com/dotnet/maui/projects/4). Any issue that is not already assigned is up for grabs. Make sure to read over the [Handler Property PR Guidelines](https://github.com/dotnet/maui/wiki/Handler-Property-PR-Guidelines) to acquaint yourself on how you can get started in contributing to our handler conversion efforts. Included is also a sample PR you can model after.

Follow the style used by the [.NET Foundation](https://github.com/dotnet/runtime/blob/master/docs/coding-guidelines/coding-style.md), with two primary exceptions:

- We do not use the `private` keyword, as it is the default accessibility level in C#.
- We use hard tabs over spaces.

Read and follow our [Pull Request template](PULL_REQUEST_TEMPLATE.md).

### Pull Request Requirements

Please refer to our [Handler Property PR Guidelines](https://github.com/dotnet/maui/wiki/Handler-Property-PR-Guidelines) and [Pull Request template](PULL_REQUEST_TEMPLATE.md).

Please check the "Allow edits from maintainers" checkbox on your pull request. This allows us to quickly make minor fixes and resolve conflicts for you.

## Proposals/Enhancements/Suggestions

To propose a change or new feature, open an issue using the [Feature request template](https://github.com/dotnet/maui/issues/new?assignees=&labels=proposal-open%2C+t%2Fenhancement+➕&template=feature_request.md&title=[Enhancement]+YOUR+IDEA!). You may also use the [Spec template](https://github.com/dotnet/maui/issues/new?assignees=&labels=proposal-open%2C+t%2Fenhancement+➕&template=spec.md&title=[Spec]++) if you have an idea of what the API should look like. Be sure to also browse current issues and [discussions](https://github.com/dotnet/maui/discussions) that may be related to what you have in mind.

## Review Process
All pull requests need to be reviewed and tested by at least two members of the MAUI team. We do our best to review pull requests in a timely manner, but please be patient! Two reviewers will be assigned and will start the review process as soon as possible. If there are any changes requested, the contributor should make them at their earliest convenience or let the reviewers know that they are unable to make further contributions. If the pull request requires only minor changes, then someone else may pick it up and finish it. We will do our best to make sure that all credit is retained for contributors. 

Once a pull request has two approvals, it will receive an "approved" label. As long as no UI or unit tests are failing, this pull request can be merged at this time.

## Merge Process
Handler property PRs should target the main branch.
