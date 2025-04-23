# Issue Management Policies

We have a lot of issue traffic to manage, so we have some policies in place to help us do that. This is a brief summary of some of the policies we have in place and the justification for them.

## Commenting on closed issues

In general, we recommend you open a new issue if you have a bug, feature request, or question to discuss. If you find a closed issue that is related, open a *new issue* and link to the closed issue rather than posting on the closed issue. Closed issues don't appear in our triage process, so only the people who have been active on the original thread will be notified of your comment. A new issue will get more attention from the team.

*In general* we don't mind getting duplicate issues. It's easier for us to close duplicate issues than to discuss multiple root causes on a single issue! We may close your issue as a duplicate if we determine it has the same root cause as another. Don't worry! It's not a waste of our time!

## Needs Author Feedback

If a contributor reviews an issue and determines that more information is needed from the author, they will post a comment requesting that information and apply the `s/needs-info` label. This label indicates that the author needs to post a response in order for us to continue investigating the issue.

If the author does not post a response within **7 days**, the issue will be automatically closed. If the author responds within **7 days** after the issue is closed, the issue will be automatically re-opened. We recognize that you may not be able to respond immediately to our requests; we're happy to hear from you whenever you're able to provide the new information.

### PR: pending author input
Similar to the `Needs Author Feedback` process above, PRs also require author input from time to time. When a member of our team asks for some follow-up changes from the author, we mark these PRs with the `s/pr-needs-author-input` label. After doing so, we expect the author to respond within 14 days.
If the author does not post a response or updates the PR within **14 days**, the PR will be automatically closed. If the author responds within **7 days** after the PR is closed, the PR will be automatically re-opened. We recognize that you may not be able to respond immediately to our requests, we're happy to hear from you whenever you're able to provide the new information.

## Duplicate issues

If we determine that the issue is a duplicate of another, we will close the issue and comment with the issue we believe is the duplicate one. Note that we might close an issue as duplicate which is not the one that has been reported the earliest. We might close an issue that has been reported earlier because the later issue has more relevant information and/or discussion.

## Locking closed issues

After an issue has been closed and had no activity for **30 days** it will be automatically locked as *resolved*. This is done in order to reduce confusion as to where to post new comments. If you are still encountering the problem reported in an issue or have a related question or bug report, feel free to open a *new issue* and link to the original (now locked) issue!