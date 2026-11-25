// @ts-check
'use strict';

/**
 * PR Journey tracker.
 *
 * Maintains a single sticky comment on every pull request that shows the
 * steps a contribution goes through, the current step, and who has to act
 * next. It is deterministic: no AI, no PR code is checked out or executed.
 * PR bodies and comments are only parsed with fixed patterns.
 */

const MARKER = '<!-- pr-journey -->';
const STATE_PREFIX = '<!-- pr-journey-state:';
const FIX_LABEL = 's/agent-fix-allowed';
const MAX_HISTORY = 15;
const DOCS_URL = 'https://github.com/dotnet/maui/blob/main/.github/docs/pr-journey.md';

const DEFAULT_BOT_LOGINS = ['mauibot', 'maui-bot', 'maui-bot[bot]', 'github-actions[bot]'];
const MAINTAINER_ASSOCIATIONS = new Set(['OWNER', 'MEMBER', 'COLLABORATOR']);
// CI is Azure Pipelines check runs plus commit statuses (e.g. maui-pr-uitests).
// GitHub Actions checks (labelers, this tracker, ...) are not CI for contributors.
const CI_CHECK_APPS = new Set(['azure-pipelines']);
const IGNORED_STATUS_CONTEXTS = new Set(['license/cla']);
const CHECK_OK = new Set(['success', 'neutral', 'skipped']);

const MODE_OPTIONS = [
  { mode: 'fix', text: 'Review and improve automatically', line: '- [{x}] **Review and improve automatically**: the MAUI agent and maintainers may push fixes for review findings to this branch. Your approach is kept.' },
  { mode: 'suggest', text: 'Review only', line: '- [{x}] **Review only**: I will address review findings myself.' },
];
const MODE_HEADING = '### 🤖 Help from the MAUI agent';
const MODE_HINT = '<!-- Pick one. You can change it later by commenting /agent fix or /agent stop. -->';

const MODE_LABEL = {
  fix: '🤖 Review and improve automatically',
  suggest: '🙋 Review only',
  unspecified: '❔ Not chosen',
};

function botLogins() {
  const extra = (process.env.PR_JOURNEY_BOT_LOGINS || '')
    .split(',')
    .map(s => s.trim().toLowerCase())
    .filter(Boolean);
  return new Set([...DEFAULT_BOT_LOGINS, ...extra]);
}

function short(sha) {
  return sha ? String(sha).slice(0, 7) : '';
}

/** Returns 'fix' | 'suggest' | 'unspecified'. */
function parseMode(body) {
  const lines = String(body || '').split(/\r?\n/);
  for (const line of lines) {
    const m = /^\s*[-*]\s*\[([xX])\]\s*(.*)$/.exec(line);
    if (!m) continue;
    const text = m[2].replace(/\*/g, '');
    for (const opt of MODE_OPTIONS) {
      if (text.trimStart().startsWith(opt.text)) return opt.mode;
    }
  }
  return 'unspecified';
}

/** Returns the PR body with the agent-help section set to the given mode. */
function setMode(body, mode) {
  const src = String(body || '');
  const lines = src.split(/\r?\n/);
  let found = false;
  const out = lines.map(line => {
    const m = /^(\s*[-*]\s*)\[([ xX])\](\s*)(.*)$/.exec(line);
    if (!m) return line;
    const text = m[4].replace(/\*/g, '').trimStart();
    const opt = MODE_OPTIONS.find(o => text.startsWith(o.text));
    if (!opt) return line;
    found = true;
    return `${m[1]}[${opt.mode === mode ? 'x' : ' '}]${m[3]}${m[4]}`;
  });
  if (found) return out.join('\n');
  const section = [
    '',
    MODE_HEADING,
    MODE_HINT,
    ...MODE_OPTIONS.map(o => o.line.replace('{x}', o.mode === mode ? 'x' : ' ')),
  ].join('\n');
  return src.replace(/\s*$/, '') + '\n' + section + '\n';
}

function decodeState(body) {
  const i = String(body || '').indexOf(STATE_PREFIX);
  if (i < 0) return {};
  const rest = body.slice(i + STATE_PREFIX.length);
  const end = rest.indexOf('-->');
  if (end < 0) return {};
  try {
    return JSON.parse(Buffer.from(rest.slice(0, end).trim(), 'base64').toString('utf8')) || {};
  } catch {
    return {};
  }
}

function encodeState(state) {
  return `${STATE_PREFIX}${Buffer.from(JSON.stringify(state), 'utf8').toString('base64')}-->`;
}

/**
 * Normalizes a GraphQL pull request node into the plain facts the state
 * machine needs.
 */
/** Summarizes CI for a commit: { state, total, done, failing: [{ name, url }] }. */
function summarizeCi(checkRuns, statuses) {
  const items = [];
  for (const r of checkRuns) {
    if (!CI_CHECK_APPS.has(r.app?.slug)) continue;
    const done = r.status === 'completed';
    items.push({ name: r.name, url: r.html_url, done, ok: done && CHECK_OK.has(r.conclusion) });
  }
  for (const st of statuses) {
    if (IGNORED_STATUS_CONTEXTS.has(st.context)) continue;
    const done = st.state !== 'pending';
    items.push({ name: st.context, url: st.target_url, done, ok: st.state === 'success' });
  }
  const failing = items.filter(i => i.done && !i.ok);
  const state = !items.length ? null : failing.length ? 'FAILURE' : items.every(i => i.done) ? 'SUCCESS' : 'PENDING';
  return { state, total: items.length, done: items.filter(i => i.done).length, failing };
}

function toFacts(pr, ci = summarizeCi([], [])) {
  const bots = botLogins();
  const author = pr.author?.login || 'ghost';
  const labels = (pr.labels?.nodes || []).map(l => l.name);
  const head = pr.commits?.nodes?.[0]?.commit;
  const reviews = pr.reviews?.nodes || [];

  const aiReviews = reviews.filter(r => bots.has(String(r.author?.login || '').toLowerCase()));
  const lastAi = aiReviews[aiReviews.length - 1];

  const opinions = (pr.latestOpinionatedReviews?.nodes || []).filter(r => {
    const login = String(r.author?.login || '').toLowerCase();
    return !bots.has(login) && login !== author.toLowerCase() && MAINTAINER_ASSOCIATIONS.has(r.authorAssociation);
  });
  const changes = opinions.filter(r => r.state === 'CHANGES_REQUESTED');
  const approvals = opinions.filter(r => r.state === 'APPROVED');

  return {
    number: pr.number,
    author,
    state: pr.merged ? 'MERGED' : pr.state,
    isDraft: !!pr.isDraft,
    baseRef: pr.baseRefName,
    headSha: head?.oid || '',
    ci: ci.state,
    ciSummary: ci,
    isCrossRepository: !!pr.isCrossRepository,
    maintainerCanModify: pr.maintainerCanModify !== false,
    mode: parseMode(pr.body),
    labels,
    aiInProgress: labels.includes('s/agent-review-in-progress'),
    aiOutcome: labels.includes('s/agent-review-incomplete') ? 'incomplete'
      : labels.includes('s/agent-changes-requested') ? 'changes'
      : labels.includes('s/agent-approved') ? 'approved'
      : (labels.includes('s/agent-reviewed') || lastAi) ? 'reviewed' : null,
    aiReviewSha: lastAi?.commit?.oid || '',
    aiReviewUrl: lastAi?.url || '',
    maintainerChanges: changes.map(r => r.author.login),
    maintainerApprovals: approvals.map(r => r.author.login),
  };
}

/**
 * Pure state machine. Returns the step list, the current step and who has
 * to act next.
 */
function computeJourney(f, saved = {}) {
  const you = `🙋 You (@${f.author})`;
  const maint = '👩‍💻 A maintainer';
  const team = '🤝 The MAUI team';
  const ciBot = '⚙️ CI';
  const agent = '🤖 The MAUI agent';
  const isFix = f.mode === 'fix';
  const updatedSinceAi = !!(f.aiReviewSha && f.headSha && f.aiReviewSha !== f.headSha);
  const readyAfterAi = !!(saved.readySha && saved.readySha === f.headSha);

  const steps = [
    { key: 'submitted', name: 'Submitted', status: 'done', note: `Opened by @${f.author}` },
    { key: 'ci', name: 'Build & tests (CI)' },
    { key: 'review', name: 'AI review' },
    { key: 'improve', name: 'Improvements' },
    { key: 'maintainer', name: 'Maintainer review' },
    { key: 'merge', name: 'Merge' },
  ];
  const s = Object.fromEntries(steps.map(x => [x.key, x]));

  // CI
  const cs = f.ciSummary || { total: 0, done: 0, failing: [] };
  if (f.ci === 'SUCCESS') Object.assign(s.ci, { status: 'done', note: `Passed on \`${short(f.headSha)}\` (${cs.total} checks)` });
  else if (f.ci === 'FAILURE') {
    const names = cs.failing.slice(0, 3).map(x => x.url ? `[${x.name}](${x.url})` : x.name).join(', ');
    const more = cs.failing.length > 3 ? ` and ${cs.failing.length - 3} more` : '';
    Object.assign(s.ci, { status: 'blocked', note: `${cs.failing.length} failing on \`${short(f.headSha)}\`: ${names}${more}` });
  }
  else if (f.ci === 'PENDING') Object.assign(s.ci, { status: 'active', note: `Running (${cs.done} of ${cs.total} checks done)` });
  else Object.assign(s.ci, { status: 'pending', note: 'Not started yet' });

  // AI review
  if (f.aiInProgress) Object.assign(s.review, { status: 'active', note: 'Running now' });
  else if (f.aiOutcome) {
    const outcome = { approved: 'No changes needed', changes: 'Changes suggested', incomplete: 'Could not finish', reviewed: 'Done' }[f.aiOutcome];
    const link = f.aiReviewUrl ? `[${outcome}](${f.aiReviewUrl})` : outcome;
    Object.assign(s.review, { status: f.aiOutcome === 'incomplete' ? 'blocked' : 'done', note: f.aiReviewSha ? `${link} on \`${short(f.aiReviewSha)}\`` : link });
  } else Object.assign(s.review, { status: 'pending', note: 'A maintainer starts it with `/review`' });

  // Improvements
  if (f.aiOutcome === 'approved') Object.assign(s.improve, { status: 'skipped', note: 'Nothing to improve' });
  else if (f.aiOutcome === 'changes' || f.aiOutcome === 'reviewed') {
    if (updatedSinceAi || readyAfterAi) Object.assign(s.improve, { status: 'done', note: readyAfterAi && !updatedSinceAi ? 'Author marked as ready' : `Updated since the AI review (\`${short(f.headSha)}\`)` });
    else Object.assign(s.improve, { status: 'active', note: isFix ? 'The MAUI team applies the findings on your branch' : 'You address the findings' });
  } else Object.assign(s.improve, { status: 'pending', note: isFix ? 'The MAUI team may push fixes to your branch' : 'You address the AI review findings' });

  // Maintainer review
  if (f.maintainerChanges.length) Object.assign(s.maintainer, { status: 'blocked', note: `Changes requested by ${f.maintainerChanges.map(l => '@' + l).join(', ')}` });
  else if (f.maintainerApprovals.length) Object.assign(s.maintainer, { status: 'done', note: `Approved by ${f.maintainerApprovals.map(l => '@' + l).join(', ')}` });
  else Object.assign(s.maintainer, { status: 'pending', note: 'A human has the final say' });

  // Merge
  const mergeHow = f.baseRef === 'net11'
    ? 'A maintainer retargets to the active release branch and merges after CI passes there'
    : 'A maintainer merges';
  if (f.state === 'MERGED') Object.assign(s.merge, { status: 'done', note: 'Merged 🎉' });
  else Object.assign(s.merge, { status: 'pending', note: mergeHow });

  const result = { steps, current: null, key: '', who: '', what: '', tone: 'info' };

  if (f.state === 'MERGED') {
    for (const st of steps) if (st.status !== 'skipped') st.status = 'done';
    return Object.assign(result, { key: 'merged', who: '🎉 Nobody, it is merged!', what: 'Thank you for contributing to .NET MAUI.', tone: 'ok' });
  }
  if (f.state === 'CLOSED') {
    return Object.assign(result, { key: 'closed', who: '—', what: 'This pull request was closed without merging.', tone: 'info' });
  }
  if (f.isDraft) {
    return Object.assign(result, { current: 'submitted', key: 'draft', who: you, what: 'This PR is a draft. Click **Ready for review** when you want it reviewed.', tone: 'warn' });
  }

  // Maintainer feedback wins over the automated steps.
  if (f.maintainerChanges.length) {
    result.current = 'maintainer';
    if (isFix) return Object.assign(result, { key: 'maint-changes-fix', who: team, what: 'A maintainer asked for changes. You opted in, so the MAUI team may apply them on your branch. Reply in the review thread if you disagree.', tone: 'warn' });
    return Object.assign(result, { key: 'maint-changes', who: you, what: 'A maintainer asked for changes. Push an update or reply in the review thread. Comment `/agent fix` to let the MAUI team make the change for you.', tone: 'warn' });
  }

  if (s.ci.status === 'blocked') {
    return Object.assign(result, { current: 'ci', key: 'ci-failed', who: you, what: 'CI is failing. Check the failing checks below. If the failure looks unrelated to your change, say so in a comment and a maintainer will rerun it.', tone: 'warn' });
  }

  if (s.review.status === 'active') {
    return Object.assign(result, { current: 'review', key: 'ai-running', who: agent, what: 'The AI review is running. No action needed from you.', tone: 'info' });
  }

  if (s.improve.status === 'active') {
    result.current = 'improve';
    if (isFix) return Object.assign(result, { key: 'improve-fix', who: team, what: 'The AI review suggested changes. You opted in, so the MAUI team will apply them on your branch. No action needed from you.', tone: 'info' });
    return Object.assign(result, { key: 'improve-you', who: you, what: 'The AI review suggested changes. Address them and push, or comment `/agent fix` to let the MAUI team finish it for you. Comment `/ready` if you think nothing else is needed.', tone: 'warn' });
  }

  if (f.maintainerApprovals.length) {
    if (s.ci.status !== 'done') {
      return Object.assign(result, { current: 'ci', key: 'approved-ci', who: ciBot, what: 'Approved! Waiting for CI to pass before merge.', tone: 'ok' });
    }
    return Object.assign(result, { current: 'merge', key: 'approved', who: maint, what: `Approved! ${mergeHow}.`, tone: 'ok' });
  }

  if (s.ci.status === 'pending') {
    return Object.assign(result, { current: 'ci', key: 'ci-not-started', who: maint, what: 'CI has not started. For security, a maintainer starts CI on community PRs. No action needed from you.', tone: 'info' });
  }

  if (s.ci.status === 'active') {
    return Object.assign(result, { current: 'ci', key: 'ci-running', who: ciBot, what: 'Builds and tests are running. No action needed from you.', tone: 'info' });
  }

  if (s.review.status === 'pending') {
    return Object.assign(result, { current: 'review', key: 'ai-pending', who: maint, what: 'A maintainer will start the AI review. No action needed from you.', tone: 'info' });
  }

  if (s.review.status === 'blocked') {
    return Object.assign(result, { current: 'review', key: 'ai-incomplete', who: maint, what: 'The AI review could not finish. A maintainer will retry it or review manually. No action needed from you.', tone: 'info' });
  }

  return Object.assign(result, { current: 'maintainer', key: 'maint-review', who: maint, what: 'All automated steps are done. A maintainer will review. No action needed from you.', tone: 'ok' });
}

const TITLES = {
  'merged': 'Merged', 'closed': 'Closed', 'draft': 'Draft', 'maint-changes-fix': 'Maintainer asked for changes',
  'maint-changes': 'Maintainer asked for changes', 'ci-failed': 'CI failing', 'ai-running': 'AI review started',
  'improve-fix': 'AI review suggested changes', 'improve-you': 'AI review suggested changes', 'approved-ci': 'Approved, CI pending',
  'approved': 'Approved', 'ci-not-started': 'CI not started', 'ai-pending': 'Waiting for AI review',
  'ci-running': 'CI running', 'ai-incomplete': 'AI review incomplete', 'maint-review': 'Ready for maintainer review',
};

const ICON = { done: '✅', active: '⏳', blocked: '⚠️', pending: '⬜', skipped: '➖' };

function render(f, journey, saved) {
  const lines = [MARKER, '### 🧭 Where is this PR?', ''];
  lines.push(`> **Waiting on: ${journey.who}**`);
  lines.push(`> ${journey.what}`);
  lines.push('');

  if (f.mode === 'fix' && f.isCrossRepository && !f.maintainerCanModify) {
    lines.push('> [!WARNING]');
    lines.push('> You chose **Review and improve automatically**, but **Allow edits by maintainers** is off, so nobody can push to your branch. Turn it on in the PR sidebar.');
    lines.push('');
  }

  const visible = journey.steps.filter(st => st.status !== 'skipped');
  const doneCount = visible.filter(st => st.status === 'done').length;
  const total = visible.length;
  const bar = '▰'.repeat(doneCount) + '▱'.repeat(Math.max(0, total - doneCount));
  lines.push(`\`${bar}\` ${doneCount} of ${total} steps done · Mode: **${MODE_LABEL[f.mode]}**`);
  lines.push('');
  lines.push('| | Step | Status |');
  lines.push('|---|---|---|');
  for (const st of journey.steps) {
    const isCurrent = st.key === journey.current;
    const icon = isCurrent && st.status !== 'blocked' ? '👉' : ICON[st.status];
    const name = isCurrent ? `**${st.name}**` : (st.status === 'skipped' ? `~${st.name}~` : st.name);
    lines.push(`| ${icon} | ${name} | ${st.note || ''} |`);
  }
  lines.push('');

  const history = saved.history || [];
  if (history.length) {
    lines.push('<details><summary>Activity</summary>');
    lines.push('');
    for (const h of [...history].reverse()) lines.push(`- \`${h.at.slice(0, 16).replace('T', ' ')} UTC\` ${h.text}`);
    lines.push('');
    lines.push('</details>');
  }
  lines.push('<details><summary>Commands</summary>');
  lines.push('');
  lines.push('| Comment | Who | What it does |');
  lines.push('|---|---|---|');
  lines.push('| `/agent fix` | PR author | Let the MAUI agent and maintainers push fixes for review findings to this branch |');
  lines.push('| `/agent stop` | PR author or maintainer | Switch to review only. You make the changes yourself |');
  lines.push('| `/ready` | PR author or maintainer | Mark the AI review findings as addressed, so the PR moves to maintainer review |');
  lines.push('');
  lines.push('</details>');
  lines.push('');
  lines.push(`<sub>This comment updates automatically. [How this works](${DOCS_URL})</sub>`);
  lines.push(encodeState(saved));
  return lines.join('\n');
}

/** Returns a new saved state with a history entry appended when the step changed. */
function advanceSaved(saved, journey, now) {
  const next = { ...saved, history: [...(saved.history || [])] };
  if (next.key !== journey.key) {
    next.key = journey.key;
    const who = journey.who.replace(/\s*\(@[^)]*\)/, '');
    next.history.push({ at: now, text: journey.key === 'merged' || journey.key === 'closed' ? TITLES[journey.key] : `${TITLES[journey.key] || journey.key} · waiting on ${who}` });
    if (next.history.length > MAX_HISTORY) next.history = next.history.slice(-MAX_HISTORY);
  }
  return next;
}

const PR_FIELDS = `
  number state merged isDraft body baseRefName isCrossRepository maintainerCanModify
  author { login }
  labels(first: 50) { nodes { name } }
  commits(last: 1) { nodes { commit { oid } } }
  reviews(last: 50) { nodes { author { login } state url commit { oid } } }
  latestOpinionatedReviews(first: 30) { nodes { author { login } authorAssociation state } }
`;

async function fetchPr(github, owner, repo, number) {
  const q = `query($owner:String!,$repo:String!,$number:Int!){repository(owner:$owner,name:$repo){pullRequest(number:$number){${PR_FIELDS}}}}`;
  const r = await github.graphql(q, { owner, repo, number });
  return r.repository.pullRequest;
}

async function fetchCi(github, owner, repo, sha) {
  if (!sha) return summarizeCi([], []);
  const runs = await github.paginate(github.rest.checks.listForRef, { owner, repo, ref: sha, filter: 'latest', per_page: 100 });
  const { data } = await github.rest.repos.getCombinedStatusForRef({ owner, repo, ref: sha, per_page: 100 });
  return summarizeCi(runs, data.statuses || []);
}

async function findTracker(github, owner, repo, number) {
  const comments = await github.paginate(github.rest.issues.listComments, { owner, repo, issue_number: number, per_page: 100 });
  return comments.find(c => c.user?.type === 'Bot' && String(c.body || '').startsWith(MARKER)) || null;
}

async function syncLabel(github, core, owner, repo, f) {
  const has = f.labels.includes(FIX_LABEL);
  try {
    if (f.mode === 'fix' && !has) await github.rest.issues.addLabels({ owner, repo, issue_number: f.number, labels: [FIX_LABEL] });
    if (f.mode !== 'fix' && has) await github.rest.issues.removeLabel({ owner, repo, issue_number: f.number, name: FIX_LABEL });
  } catch (e) {
    core.warning(`Could not sync ${FIX_LABEL} on #${f.number}: ${e.message}`);
  }
}

/** Creates or updates the tracker for one PR. */
async function updatePr({ github, core, owner, repo, number, create = true, mutateSaved }) {
  const pr = await fetchPr(github, owner, repo, number);
  if (!pr) return;
  const state = pr.merged ? 'MERGED' : pr.state;

  // Trackers are only created for open PRs; closed or merged PRs only refresh an existing one.
  const existing = await findTracker(github, owner, repo, number);
  if (!existing && (!create || state !== 'OPEN')) return;

  const ci = state === 'OPEN' ? await fetchCi(github, owner, repo, pr.commits?.nodes?.[0]?.commit?.oid) : undefined;
  const f = toFacts(pr, ci);
  if (f.state === 'OPEN') await syncLabel(github, core, owner, repo, f);

  let saved = existing ? decodeState(existing.body) : {};
  if (mutateSaved) saved = mutateSaved(saved, f);
  const journey = computeJourney(f, saved);
  saved = advanceSaved(saved, journey, new Date().toISOString());
  const body = render(f, journey, saved);

  if (!existing) {
    await github.rest.issues.createComment({ owner, repo, issue_number: number, body });
    core.info(`#${number}: created tracker (${journey.key})`);
  } else if (existing.body !== body) {
    await github.rest.issues.updateComment({ owner, repo, comment_id: existing.id, body });
    core.info(`#${number}: updated tracker (${journey.key})`);
  } else {
    core.info(`#${number}: unchanged (${journey.key})`);
  }
}

/** Parses a command from the first line of a comment. */
function parseCommand(body) {
  const first = String(body || '').trim().split(/\r?\n/)[0].trim().toLowerCase();
  let m = /^\/agent\s+(fix|stop)\s*$/.exec(first);
  if (m) return { name: m[1] === 'fix' ? 'fix' : 'suggest', kind: 'mode' };
  if (/^\/ready\s*$/.test(first)) return { kind: 'ready' };
  return null;
}

async function handleComment({ github, core, context }) {
  const { owner, repo } = context.repo;
  const payload = context.payload;
  const number = payload.issue.number;
  const cmd = parseCommand(payload.comment.body);
  if (!cmd) {
    if (payload.comment.user?.type !== 'Bot') await updatePr({ github, core, owner, repo, number });
    return;
  }

  const actor = payload.comment.user.login;
  const prAuthor = payload.issue.user.login;
  let isMaintainer = false;
  try {
    const p = await github.rest.repos.getCollaboratorPermissionLevel({ owner, repo, username: actor });
    isMaintainer = ['admin', 'maintain', 'write'].includes(p.data.permission);
  } catch { /* not a collaborator */ }

  const isAuthor = actor.toLowerCase() === prAuthor.toLowerCase();
  const allowed = cmd.kind === 'ready' || cmd.name === 'suggest' ? (isAuthor || isMaintainer) : isAuthor;
  const react = content => github.rest.reactions.createForIssueComment({ owner, repo, comment_id: payload.comment.id, content }).catch(() => {});

  if (!allowed) {
    core.info(`@${actor} is not allowed to run this command on #${number}`);
    await react('confused');
    return;
  }

  if (cmd.kind === 'mode') {
    const { data: pr } = await github.rest.pulls.get({ owner, repo, pull_number: number });
    const body = setMode(pr.body, cmd.name);
    if (body !== (pr.body || '')) await github.rest.pulls.update({ owner, repo, pull_number: number, body });
    await updatePr({ github, core, owner, repo, number });
  } else {
    await updatePr({ github, core, owner, repo, number, mutateSaved: (saved, f) => ({ ...saved, readySha: f.headSha }) });
  }
  await react('+1');
}

/** Refreshes existing trackers on all open PRs (catches CI and review changes). */
async function sweep({ github, core, context }) {
  const { owner, repo } = context.repo;
  const prs = await github.paginate(github.rest.pulls.list, { owner, repo, state: 'open', per_page: 100 });
  core.info(`Sweeping ${prs.length} open PRs`);
  for (const pr of prs) {
    try {
      await updatePr({ github, core, owner, repo, number: pr.number, create: false });
    } catch (e) {
      core.warning(`#${pr.number}: ${e.message}`);
    }
  }
}

async function main({ github, core, context }) {
  const { owner, repo } = context.repo;
  const ev = context.eventName;
  if (ev === 'issue_comment') return handleComment({ github, core, context });
  if (ev === 'schedule') return sweep({ github, core, context });
  if (ev === 'workflow_dispatch') {
    const n = Number(context.payload.inputs?.pr_number || 0);
    return n > 0 ? updatePr({ github, core, owner, repo, number: n }) : sweep({ github, core, context });
  }
  const number = context.payload.pull_request?.number;
  if (number) return updatePr({ github, core, owner, repo, number });
}

module.exports = { main, summarizeCi, parseMode, setMode, parseCommand, toFacts, computeJourney, render, advanceSaved, decodeState, encodeState, MARKER };
