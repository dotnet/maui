const crypto = require('node:crypto');
const fs = require('node:fs/promises');
const path = require('node:path');

const maxComments = 300;
const maxFileBytes = 1024 * 1024;
// gh-aw strips HTML comments from report content before adding trusted provenance.
const reportPrefix = 'Duplicate detector report fingerprint: ';
const workflowMarker = '<!-- gh-aw-workflow-call-id: dotnet/maui/issue-duplicate-detector -->';

function assert(condition, message) {
    if (!condition) throw new Error(message);
}

function assertKeys(value, required, optional = []) {
    assert(value && typeof value === 'object' && !Array.isArray(value), 'Expected an object.');
    assert(
        required.every((key) => Object.hasOwn(value, key)),
        'Required report fields are missing.',
    );
    assert(
        Object.keys(value).every((key) => required.includes(key) || optional.includes(key)),
        'Unexpected report fields.',
    );
}

function assertText(value, minimum, maximum) {
    assert(
        typeof value === 'string' && value.trim().length >= minimum && value.length <= maximum,
        `Expected text between ${minimum} and ${maximum} characters.`,
    );
}

function hash(value) {
    return crypto.createHash('sha256').update(JSON.stringify(value)).digest('hex');
}

function getReportHash(comment) {
    if (
        comment.author !== 'github-actions[bot]' ||
        !comment.body.includes('<!-- Issue Duplicate Detector -->') ||
        !comment.body.includes(workflowMarker)
    ) {
        return null;
    }
    const marker = comment.body.split(/\r?\n/).find((line) => line.startsWith(reportPrefix));
    const fingerprint = marker?.slice(reportPrefix.length);
    return fingerprint && /^[a-f0-9]{64}$/.test(fingerprint) ? fingerprint : null;
}

function isWorkflowReport(comment) {
    return getReportHash(comment) !== null;
}

function plainText(value) {
    return value
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/([\\`*_{}\[\]#|])/g, '\\$1')
        .replace(/@/g, '&#64;')
        .replace(/\s+/g, ' ')
        .trim();
}

async function readJson(file) {
    const stat = await fs.lstat(file);
    assert(
        stat.isFile() && !stat.isSymbolicLink() && stat.size <= maxFileBytes,
        'Expected a bounded regular JSON file.',
    );
    return JSON.parse(await fs.readFile(file, 'utf8'));
}

async function assertPublicRepository(github, repository) {
    const { data } = await github.rest.repos.get(repository);
    assert(
        data.full_name === 'dotnet/maui' && data.private === false && data.visibility === 'public',
        'Duplicate detection requires the public dotnet/maui repository.',
    );
}

async function getSnapshot(github, repository, issueNumber, issue) {
    if (!issue) {
        ({ data: issue } = await github.rest.issues.get({
            ...repository,
            issue_number: issueNumber,
        }));
    }
    assert(!issue.pull_request, 'Duplicate detection accepts issues, not pull requests.');
    assert(
        issue.comments <= maxComments,
        `Issue #${issueNumber} exceeds the ${maxComments}-comment limit.`,
    );
    const comments = [];
    for await (const { data } of github.paginate.iterator(github.rest.issues.listComments, {
        ...repository,
        issue_number: issueNumber,
        per_page: 100,
    })) {
        comments.push(
            ...data.map((comment) => ({
                id: comment.id,
                body: comment.body ?? '',
                author: comment.user?.login ?? null,
                url: comment.html_url,
                updatedAt: comment.updated_at,
            })),
        );
        assert(
            comments.length <= maxComments,
            'Issue comments changed beyond the collection limit.',
        );
    }
    const target = {
        issueNumber: issue.number,
        title: issue.title,
        body: issue.body ?? '',
        url: issue.html_url,
        state: issue.state,
        locked: issue.locked,
        authorType: issue.user?.type ?? null,
        createdAt: issue.created_at,
        updatedAt: issue.updated_at,
        labels: issue.labels.map((label) => label.name),
    };
    // Ignore label changes and our own reports so they cannot stale evidence or defeat deduplication.
    const contextHash = hash({
        issueNumber,
        title: target.title,
        body: target.body,
        comments: comments
            .filter((comment) => !isWorkflowReport(comment))
            .map(({ id, body, updatedAt }) => ({ id, body, updatedAt })),
    });
    return { target, comments, contextHash };
}

function findEvidence(snapshot, quote) {
    assertText(quote, 20, 400);
    const normalized = quote.replace(/\s+/g, ' ').trim();
    assertText(normalized, 20, 400);
    const sources = [
        { body: snapshot.target.title, url: snapshot.target.url },
        { body: snapshot.target.body, url: snapshot.target.url },
        ...snapshot.comments.filter(
            (comment) =>
                !isWorkflowReport(comment) &&
                !comment.body.startsWith("Hi I'm an AI powered bot that finds similar issues"),
        ),
    ];
    const source = sources.find((value) => value.body.replace(/\s+/g, ' ').includes(normalized));
    assert(source, `Evidence excerpt was not found on issue #${snapshot.target.issueNumber}.`);
    return { quote: normalized, url: source.url };
}

async function gather({ github, core, context, issueNumber, outputDirectory }) {
    core.setOutput('should_run', 'false');
    assert(
        Number.isSafeInteger(issueNumber) && issueNumber > 0,
        'A positive integer issue number is required.',
    );
    if (
        context.payload.repository.full_name !== 'dotnet/maui' ||
        context.ref !== `refs/heads/${context.payload.repository.default_branch}`
    ) {
        core.info(
            'Duplicate detection runs only on trusted dotnet/maui default-branch infrastructure.',
        );
        return;
    }
    assert(
        context.eventName === 'issues' || context.eventName === 'workflow_dispatch',
        'Unsupported event.',
    );
    assert(
        (context.payload.inputs?.aw_context ?? '') === '',
        'Caller workspace context is not accepted.',
    );
    await assertPublicRepository(github, context.repo);
    const { data: issue } = await github.rest.issues.get({
        ...context.repo,
        issue_number: issueNumber,
    });
    assert(!issue.pull_request, 'Duplicate detection accepts issues, not pull requests.');
    if (
        issue.state !== 'open' ||
        issue.locked ||
        issue.labels.some((label) => /^s\/duplicate\b/.test(label.name)) ||
        (context.eventName === 'issues' && issue.user?.type === 'Bot')
    ) {
        core.info(
            'Skipping a closed, locked, already-duplicate, or automatically created bot issue.',
        );
        return;
    }
    const snapshot = await getSnapshot(github, context.repo, issueNumber, issue);
    const prepared = JSON.stringify(snapshot);
    assert(
        Buffer.byteLength(prepared) <= maxFileBytes,
        'Issue evidence exceeds the context size limit.',
    );
    await fs.mkdir(outputDirectory, { recursive: true });
    await fs.writeFile(path.join(outputDirectory, 'context.json'), prepared);
    core.setOutput('should_run', 'true');
}

async function validate({
    github,
    core,
    context,
    issueNumber,
    staged,
    contextDirectory,
    agentOutputPath,
}) {
    assert(Number.isSafeInteger(issueNumber) && issueNumber > 0, 'Invalid publication inputs.');
    assert(typeof staged === 'boolean', 'A boolean staging flag is required.');
    assert(
        context.payload.repository.full_name === 'dotnet/maui' &&
            context.ref === `refs/heads/${context.payload.repository.default_branch}`,
        'Untrusted publication context.',
    );
    await assertPublicRepository(github, context.repo);
    const original = await readJson(path.join(contextDirectory, 'context.json'));
    const expectedHash = original.contextHash;
    assert(
        original.target.issueNumber === issueNumber &&
            typeof expectedHash === 'string' &&
            /^[a-f0-9]{64}$/.test(expectedHash),
        'The trusted context artifact does not match the requested issue.',
    );
    const payload = await readJson(agentOutputPath);
    assertKeys(payload, ['items'], ['errors', 'warnings']);
    assert(!payload.errors?.length, 'The agent reported errors; refusing partial publication.');
    assert(
        Array.isArray(payload.items) && payload.items.length === 1,
        'Expected exactly one safe output.',
    );
    const item = payload.items[0];
    if (item.type === 'noop') {
        core.info('No duplicate report requested.');
        return;
    }
    assert(
        item.type === 'add_comment',
        'Detection is incomplete or requested an unsupported output.',
    );
    assertKeys(
        item,
        ['type', 'item_number', 'body', 'data'],
        ['temporary_id', 'secrecy', 'integrity'],
    );
    assert(item.item_number === issueNumber, 'The proposed comment targets a different issue.');
    assertKeys(item.data, ['duplicates']);
    const proposal = item.data.duplicates;
    assertKeys(proposal, ['issueNumber', 'contextHash', 'matches']);
    assert(
        proposal.issueNumber === issueNumber && proposal.contextHash === expectedHash,
        'The proposed report does not match the prepared issue.',
    );
    assert(
        Array.isArray(proposal.matches) &&
            proposal.matches.length >= 1 &&
            proposal.matches.length <= 5,
        'A report requires between one and five matches.',
    );
    const current = await getSnapshot(github, context.repo, issueNumber);
    assert(
        current.target.state === 'open' &&
            !current.target.locked &&
            !current.target.labels.some((label) => /^s\/duplicate\b/.test(label)),
        'The issue is no longer eligible for duplicate suggestions.',
    );
    assert(current.contextHash === expectedHash, 'Issue evidence changed; request a fresh run.');
    const seen = new Set([issueNumber]);
    const matches = [];
    const candidateSnapshots = [];
    for (const match of proposal.matches) {
        assertKeys(match, [
            'issueNumber',
            'probability',
            'updatedAt',
            'evidence',
            'differences',
            'targetQuote',
            'candidateQuote',
        ]);
        assert(
            Number.isSafeInteger(match.issueNumber) &&
                match.issueNumber > 0 &&
                !seen.has(match.issueNumber),
            'Candidate numbers must be distinct positive integers, excluding the target.',
        );
        seen.add(match.issueNumber);
        assert(
            Number.isInteger(match.probability) &&
                match.probability >= 60 &&
                match.probability <= 100,
            'Every match requires an integer duplicate probability between 60 and 100.',
        );
        assertText(match.updatedAt, 20, 40);
        assertText(match.evidence, 20, 800);
        assertText(match.differences, 1, 800);
        const candidate = await getSnapshot(github, context.repo, match.issueNumber);
        assert(
            candidate.target.updatedAt === match.updatedAt,
            'Candidate evidence changed; request a fresh run.',
        );
        candidateSnapshots.push(candidate);
        matches.push({
            issueNumber: match.issueNumber,
            title: candidate.target.title,
            url: candidate.target.url,
            state: candidate.target.state,
            probability: match.probability,
            evidence: match.evidence,
            differences: match.differences,
            targetEvidence: findEvidence(current, match.targetQuote),
            candidateEvidence: findEvidence(candidate, match.candidateQuote),
        });
    }
    matches.sort(
        (left, right) =>
            right.probability - left.probability || left.issueNumber - right.issueNumber,
    );
    const reportHash = hash({ contextHash: expectedHash, matches });
    const marker = `${reportPrefix}${reportHash}`;
    const existing = current.comments.some((comment) => getReportHash(comment) === reportHash);
    if (existing) {
        payload.items = [
            {
                type: 'noop',
                message: 'An identical duplicate report is already present.',
            },
        ];
    } else {
        const rows = matches.map(
            (match) =>
                `| [${plainText(match.title)} (#${match.issueNumber})](${match.url}) | ${match.state} | ` +
                `**${match.probability}%** | ${match.probability >= 85 ? 'Likely duplicate' : 'Possible duplicate'} | ` +
                `${plainText(match.evidence)}<br>**Differences/uncertainty:** ${plainText(match.differences)} |`,
        );
        const excerpts = matches.map(
            (match) =>
                `**#${match.issueNumber} evidence:** [Current report](${match.targetEvidence.url}): ` +
                `"${plainText(match.targetEvidence.quote)}"; [candidate](${match.candidateEvidence.url}): ` +
                `"${plainText(match.candidateEvidence.quote)}".`,
        );
        item.body = [
            '## Possible duplicate issues',
            '',
            'These probabilities are uncalibrated AI estimates of the same underlying issue, not title-similarity scores or confirmed duplicate decisions. Maintainers decide whether to consolidate reports; this workflow never labels or closes issues.',
            '',
            '| Issue | State | Duplicate probability | Assessment | Evidence |',
            '| --- | --- | --- | --- | --- |',
            ...rows,
            '',
            ...excerpts.flatMap((excerpt) => [excerpt, '']),
            'Closed candidates are historical context. A recurrence after a fix can be a new regression, not a duplicate.',
            '',
            `[Workflow result](https://github.com/${context.repo.owner}/${context.repo.repo}/actions/runs/${context.runId}).`,
            '',
            marker,
        ].join('\n');
        delete item.data;
        delete item.temporary_id;
        await Promise.all(
            candidateSnapshots.map(async (previous) => {
                const latest = await getSnapshot(github, context.repo, previous.target.issueNumber);
                assert(
                    latest.contextHash === previous.contextHash &&
                        latest.target.updatedAt === previous.target.updatedAt &&
                        latest.target.state === previous.target.state &&
                        latest.target.locked === previous.target.locked,
                    `Candidate #${previous.target.issueNumber} evidence changed during validation; request a fresh run.`,
                );
            }),
        );
        await assertPublicRepository(github, context.repo);
        const final = await getSnapshot(github, context.repo, issueNumber);
        assert(
            final.contextHash === expectedHash &&
                final.target.state === 'open' &&
                !final.target.locked &&
                !final.target.labels.some((label) => /^s\/duplicate\b/.test(label)),
            'Issue evidence changed during validation; request a fresh run.',
        );
    }
    await fs.writeFile(agentOutputPath, JSON.stringify(payload));
    if (staged && payload.items[0].type === 'add_comment') {
        await core.summary
            .addHeading('Validated duplicate report preview')
            .addEOL()
            .addRaw(
                'Staged run: no duplicate-report comment is posted. The preview below is the validated report before gh-aw adds publication cautions and workflow markers.',
                true,
            )
            .addEOL()
            .addRaw(payload.items[0].body, true)
            .write();
    }
    core.info(
        `Validated duplicate probabilities and source excerpts for ${matches.length} candidate(s).`,
    );
}

module.exports = { gather, validate };
