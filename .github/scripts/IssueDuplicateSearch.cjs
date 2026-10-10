const crypto = require('node:crypto');
const fs = require('node:fs/promises');
const http = require('node:http');
const path = require('node:path');
const { spawn } = require('node:child_process');
const { setTimeout: sleep } = require('node:timers/promises');

const repository = 'dotnet/maui';
const port = 8766;
const maxQueries = 8;
const maxRequests = 16;
const maxResponseBytes = 2 * 1024 * 1024;
const requestIntervalMs = 7000;
const maxWaitMs = 120000;
const maxTotalWaitMs = 180000;

function assert(condition, message) {
    if (!condition) throw new Error(message);
}

async function readJson(stream, maximum) {
    assert(stream, 'Expected a JSON response body.');
    const chunks = [];
    let size = 0;
    for await (const chunk of stream) {
        size += chunk.length;
        assert(size <= maximum, 'Duplicate-search JSON exceeds its size limit.');
        chunks.push(Buffer.from(chunk));
    }
    return JSON.parse(Buffer.concat(chunks).toString('utf8'));
}

function retryTime(headers, attempt) {
    const now = Date.now();
    const deadlines = [];
    const retryAfter = headers.get('retry-after');
    if (retryAfter !== null) {
        const deadline = /^\d+$/.test(retryAfter)
            ? now + Number(retryAfter) * 1000
            : Date.parse(retryAfter);
        assert(Number.isFinite(deadline), 'GitHub returned an invalid Retry-After header.');
        deadlines.push(deadline + 2000);
    }
    if (headers.get('x-ratelimit-remaining') === '0') {
        const reset = headers.get('x-ratelimit-reset');
        assert(reset !== null && /^\d+$/.test(reset), 'GitHub rate-limit reset is missing.');
        deadlines.push(Number(reset) * 1000 + 2000);
    }
    return deadlines.length ? Math.max(now + 1000, ...deadlines) : now + 60000 * 2 ** attempt;
}

function createSearchClient({ token, issueNumber, contextHash, runId, sha }) {
    assert(typeof token === 'string' && token.length > 0, 'A read-only search token is required.');
    assert(Number.isSafeInteger(issueNumber) && issueNumber > 0, 'Invalid search target.');
    assert(/^[a-f0-9]{64}$/.test(contextHash), 'Invalid prepared search context hash.');
    assert(Number.isSafeInteger(runId) && runId > 0, 'Invalid search run identity.');
    assert(/^[a-f0-9]{40}$/.test(sha), 'Invalid search revision.');
    const status = {
        version: 1,
        repository,
        issueNumber,
        contextHash,
        runId,
        sha,
        searchMode: 'lexical',
        queries: 0,
        completedQueries: 0,
        requests: 0,
        pending: 0,
        waitedMs: 0,
        failure: null,
    };
    let nextRequestAt = 0;
    let queue = Promise.resolve();

    async function get(url) {
        const response = await fetch(url, {
            headers: {
                Accept: 'application/vnd.github+json',
                Authorization: `Bearer ${token}`,
                'User-Agent': 'maui-issue-duplicate-search',
                'X-GitHub-Api-Version': '2022-11-28',
            },
            redirect: 'error',
            signal: AbortSignal.timeout(15000),
        });
        if (
            response.status === 429 ||
            (response.status === 403 &&
                (response.headers.get('x-ratelimit-remaining') === '0' ||
                    response.headers.has('retry-after')))
        ) {
            await response.body?.cancel();
            return { response, data: null };
        }
        return {
            response,
            data: await readJson(response.body, maxResponseBytes),
        };
    }

    async function assertPublicRepository() {
        const { response, data } = await get(`https://api.github.com/repos/${repository}`);
        assert(
            response.ok,
            `Cannot verify public duplicate-search repository: HTTP ${response.status}.`,
        );
        assert(
            data.full_name === repository && data.private === false && data.visibility === 'public',
            'Duplicate searches require public dotnet/maui.',
        );
    }

    async function search(query) {
        assert(
            status.failure === null,
            'This search session is incomplete; no more requests are permitted.',
        );
        await assertPublicRepository();
        for (let attempt = 0; attempt < 3; attempt++) {
            const wait = Math.max(0, nextRequestAt - Date.now());
            assert(
                wait <= maxWaitMs && status.waitedMs + wait <= maxTotalWaitMs,
                'GitHub search reset exceeds the bounded wait budget; investigation is incomplete.',
            );
            if (wait) {
                status.waitedMs += wait;
                console.info(
                    `Duplicate search waiting ${Math.ceil(wait / 1000)} seconds for pacing or GitHub reset.`,
                );
                await sleep(wait);
            }
            assert(
                status.requests < maxRequests,
                'The duplicate-search HTTP request budget is exhausted.',
            );
            status.requests++;
            nextRequestAt = Date.now() + requestIntervalMs;
            const url = new URL('https://api.github.com/search/issues');
            url.searchParams.set('q', query);
            url.searchParams.set('per_page', '20');
            url.searchParams.set('page', '1');
            const { response, data } = await get(url);
            if (response.ok) {
                assert(
                    Number.isSafeInteger(data.total_count) &&
                        data.total_count >= 0 &&
                        data.incomplete_results === false &&
                        Array.isArray(data.items) &&
                        data.items.length <= 20,
                    'GitHub returned incomplete or invalid issue search results.',
                );
                for (const issue of data.items) {
                    assert(
                        Number.isSafeInteger(issue.number) &&
                            issue.number > 0 &&
                            !issue.pull_request &&
                            issue.repository_url === `https://api.github.com/repos/${repository}` &&
                            issue.html_url ===
                                `https://github.com/${repository}/issues/${issue.number}` &&
                            typeof issue.title === 'string' &&
                            ['open', 'closed'].includes(issue.state),
                        'GitHub search returned an unexpected issue or repository.',
                    );
                }
                if (response.headers.get('x-ratelimit-remaining') === '0') {
                    nextRequestAt = Math.max(nextRequestAt, retryTime(response.headers, attempt));
                }
                status.completedQueries++;
                return {
                    searchMode: 'lexical',
                    query,
                    totalCount: data.total_count,
                    truncated: data.total_count > data.items.length,
                    queriesRemaining: maxQueries - status.queries,
                    candidates: data.items
                        .filter((issue) => issue.number !== issueNumber)
                        .map((issue) => ({
                            issueNumber: issue.number,
                            title: issue.title,
                            state: issue.state,
                            url: issue.html_url,
                        })),
                };
            }
            const limited =
                response.status === 429 ||
                (response.status === 403 &&
                    (response.headers.get('x-ratelimit-remaining') === '0' ||
                        response.headers.has('retry-after') ||
                        /rate limit/i.test(data?.message ?? '')));
            assert(limited, `GitHub issue search failed: HTTP ${response.status}.`);
            assert(attempt < 2, 'GitHub issue search remains rate-limited after bounded retries.');
            nextRequestAt = Math.max(nextRequestAt, retryTime(response.headers, attempt));
        }
        throw new Error('Duplicate search did not complete.');
    }

    return {
        assertPublicRepository,
        getStatus: () => ({ ...status }),
        search(args) {
            try {
                assert(
                    args &&
                        typeof args === 'object' &&
                        Object.keys(args).length === 1 &&
                        Array.isArray(args.terms) &&
                        args.terms.length >= 1 &&
                        args.terms.length <= 4 &&
                        args.terms.every(
                            (term) =>
                                typeof term === 'string' &&
                                term.length >= 1 &&
                                term.length <= 80 &&
                                /^[\p{L}\p{N}_][\p{L}\p{N}_ .+/-]*$/u.test(term),
                        ),
                    'Provide one to four literal keyword/phrase strings, without GitHub qualifiers.',
                );
                assert(
                    status.queries < maxQueries,
                    'The eight-query duplicate-search budget is exhausted.',
                );
                const query = `repo:${repository} is:issue ${args.terms.map((term) => `"${term}"`).join(' ')}`;
                assert(
                    query.length <= 256,
                    'Duplicate-search keywords exceed the GitHub query limit.',
                );
                status.queries++;
                status.pending++;
                const result = queue
                    .then(() => search(query))
                    .catch((error) => {
                        status.failure ??= error.message;
                        throw error;
                    })
                    .finally(() => {
                        status.pending--;
                    });
                // A failed call must reach its caller without poisoning the queue's bookkeeping.
                queue = result.then(
                    () => undefined,
                    () => undefined,
                );
                return result;
            } catch (error) {
                status.failure ??= error.message;
                throw error;
            }
        },
    };
}

function assertDiscovery(status, { issueNumber, contextHash, runId, sha }) {
    assert(
        status.version === 1 &&
            status.repository === repository &&
            status.issueNumber === issueNumber &&
            status.contextHash === contextHash &&
            status.runId === Number(runId) &&
            status.sha === sha &&
            status.searchMode === 'lexical',
        'Trusted discovery evidence does not match this issue and workflow run.',
    );
    assert(
        status.failure === null &&
            status.pending === 0 &&
            Number.isInteger(status.queries) &&
            status.queries >= 1 &&
            status.queries <= maxQueries &&
            status.completedQueries === status.queries &&
            Number.isInteger(status.requests) &&
            status.requests >= status.queries &&
            status.requests <= maxRequests &&
            Number.isFinite(status.waitedMs) &&
            status.waitedMs >= 0 &&
            status.waitedMs <= maxTotalWaitMs,
        'Duplicate discovery is incomplete; refusing a report or completed no-match outcome.',
    );
}

async function serve(options) {
    const client = createSearchClient(options);
    await client.assertPublicRepository();
    assert(
        /^[a-f0-9]{64}$/.test(options.apiKey),
        'A fresh local MCP authentication key is required.',
    );
    const runtime = path.join(process.env.RUNNER_TEMP, 'gh-aw', 'actions');
    require(path.join(runtime, 'shim.cjs'));
    const { MCPServer, MCPHTTPTransport } = require(path.join(runtime, 'mcp_http_transport.cjs'));
    const server = new MCPServer(
        { name: 'duplicate-search', version: '1.0.0' },
        { logDir: path.join(process.env.RUNNER_TEMP, 'issue-duplicate-search', 'mcp') },
    );
    server.tool(
        'search_issues',
        'Read-only lexical search of public dotnet/maui issues. Use 1-4 literal keywords/phrases, combined with AND. Scope, 20 results, one page, eight queries and rate-aware retries are enforced. Read full candidate reports with github.issue_read; these are unscored leads.',
        {
            type: 'object',
            additionalProperties: false,
            required: ['terms'],
            properties: {
                terms: {
                    type: 'array',
                    minItems: 1,
                    maxItems: 4,
                    items: { type: 'string', minLength: 1, maxLength: 80 },
                },
            },
        },
        async (args) => ({
            content: [
                {
                    type: 'text',
                    text: JSON.stringify(await client.search(args)),
                },
            ],
        }),
    );
    const transport = new MCPHTTPTransport({ enableJsonResponse: true });
    await server.connect(transport);
    const httpServer = http.createServer(async (request, response) => {
        const supplied = Buffer.from(request.headers.authorization ?? '');
        const expected = Buffer.from(options.apiKey);
        if (supplied.length !== expected.length || !crypto.timingSafeEqual(supplied, expected)) {
            response.writeHead(401).end();
            return;
        }
        if (request.method === 'GET' && ['/health', '/status'].includes(request.url)) {
            response.writeHead(200, { 'Content-Type': 'application/json' });
            response.end(
                JSON.stringify(
                    request.url === '/status'
                        ? client.getStatus()
                        : { status: 'ok', server: 'duplicate-search' },
                ),
            );
            return;
        }
        if (request.method !== 'POST' || request.url !== '/') {
            response.writeHead(405).end();
            return;
        }
        try {
            const body = await readJson(request, 16384);
            await transport.handleRequest(request, response, body);
        } catch (error) {
            console.error(`Duplicate-search MCP request failed: ${error.message}`);
            if (!response.headersSent) response.writeHead(400).end();
        }
    });
    await new Promise((resolve, reject) => {
        httpServer.once('error', reject);
        httpServer.listen(options.port ?? port, options.host ?? '0.0.0.0', resolve);
    });
    process.once('SIGTERM', () => {
        httpServer.close(() => process.exit(0));
        setTimeout(() => process.exit(0), 1000).unref();
    });
    return httpServer;
}

async function start({ core, issueNumber }) {
    const contextPath = path.join(
        process.env.RUNNER_TEMP,
        'issue-duplicate-context',
        'context.json',
    );
    const stat = await fs.lstat(contextPath);
    assert(
        stat.isFile() && !stat.isSymbolicLink() && stat.size <= 1024 * 1024,
        'Invalid prepared context file.',
    );
    const prepared = JSON.parse(await fs.readFile(contextPath, 'utf8'));
    assert(prepared.target.issueNumber === issueNumber, 'Prepared search target changed.');
    const apiKey = crypto.randomBytes(32).toString('hex');
    core.setSecret(apiKey);
    const directory = path.join(process.env.RUNNER_TEMP, 'issue-duplicate-search');
    await fs.mkdir(directory, { recursive: true });
    const log = await fs.open(path.join(directory, 'search.log'), 'a');
    const child = spawn(process.execPath, [__filename], {
        detached: true,
        stdio: ['ignore', log.fd, log.fd],
        env: {
            RUNNER_TEMP: process.env.RUNNER_TEMP,
            RUNNER_TRACKING_ID: process.env.RUNNER_TRACKING_ID,
            GITHUB_RUN_ID: process.env.GITHUB_RUN_ID,
            GITHUB_SHA: process.env.GITHUB_SHA,
            ISSUE_NUMBER: String(issueNumber),
            ISSUE_CONTEXT_HASH: prepared.contextHash,
            ISSUE_DUPLICATE_SEARCH_API_KEY: apiKey,
            GITHUB_SEARCH_TOKEN: process.env.GITHUB_SEARCH_TOKEN,
        },
    });
    let startupError;
    child.once('error', (error) => {
        startupError = error;
    });
    await log.close();
    child.unref();
    try {
        for (let attempt = 0; attempt < 60; attempt++) {
            assert(!startupError, 'The duplicate-search service could not be started.');
            assert(child.exitCode === null, 'The duplicate-search service exited during startup.');
            const ready = await fetch(`http://127.0.0.1:${port}/health`, {
                headers: { Authorization: apiKey },
                signal: AbortSignal.timeout(1000),
            }).catch((error) => {
                if (error.cause?.code !== 'ECONNREFUSED') throw error;
                return null;
            });
            if (ready?.ok) {
                assert(
                    (await ready.json()).server === 'duplicate-search',
                    'Unexpected local search service.',
                );
                core.setOutput('api_key', apiKey);
                core.setOutput('pid', String(child.pid));
                core.info('Repository-scoped, rate-aware duplicate search is ready.');
                return;
            }
            await sleep(500);
        }
        throw new Error('The duplicate-search service did not become ready within 30 seconds.');
    } catch (error) {
        child.kill('SIGTERM');
        throw error;
    }
}

async function collect({ core }) {
    const pid = Number(process.env.ISSUE_DUPLICATE_SEARCH_PID);
    assert(Number.isSafeInteger(pid) && pid > 0, 'Invalid trusted search service PID.');
    try {
        const response = await fetch(`http://127.0.0.1:${port}/status`, {
            headers: {
                Authorization: process.env.ISSUE_DUPLICATE_SEARCH_API_KEY,
            },
            signal: AbortSignal.timeout(5000),
        });
        assert(response.ok, `Cannot retain trusted discovery status: HTTP ${response.status}.`);
        const status = await readJson(response.body, 16384);
        const directory = path.join(process.env.RUNNER_TEMP, 'issue-duplicate-search');
        await fs.writeFile(path.join(directory, 'discovery.json'), JSON.stringify(status));
        core.info(
            `Retained trusted discovery: ${status.completedQueries}/${status.queries} queries, ${status.requests} HTTP requests, ${status.failure === null ? 'no failure' : 'incomplete'}.`,
        );
    } finally {
        process.kill(pid, 'SIGTERM');
    }
}

if (require.main === module) {
    serve({
        token: process.env.GITHUB_SEARCH_TOKEN,
        apiKey: process.env.ISSUE_DUPLICATE_SEARCH_API_KEY,
        issueNumber: Number(process.env.ISSUE_NUMBER),
        contextHash: process.env.ISSUE_CONTEXT_HASH,
        runId: Number(process.env.GITHUB_RUN_ID),
        sha: process.env.GITHUB_SHA,
    }).catch((error) => {
        console.error(`Duplicate-search startup failed: ${error.message}`);
        process.exitCode = 1;
    });
}

module.exports = { start, collect, serve, assertDiscovery };
