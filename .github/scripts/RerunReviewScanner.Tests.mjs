#!/usr/bin/env node

import assert from 'node:assert/strict';
import fs from 'node:fs';
import path from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

const readyLabel = 's/agent-ready-for-rerun';
const inProgressLabel = 's/agent-review-in-progress';
const declinedLabel = {
  name: 's/agent-rerun-declined',
  description: 'AI rerun scanner declined the current PR state; new author activity is required',
  color: 'D4C5F9',
};

const scriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const workflowPath = path.join(scriptDirectory, '../workflows/rerun-review-scanner.md');
const workflowLockPath = path.join(scriptDirectory, '../workflows/rerun-review-scanner.lock.yml');

function extractIndentedScript(filePath, startMarker, endMarker) {
  const contents = fs.readFileSync(filePath, 'utf8');
  const start = contents.indexOf(startMarker);
  const end = endMarker ? contents.indexOf(endMarker, start) : contents.length;
  assert.notEqual(start, -1, `scanner script start marker was not found in ${filePath}`);
  assert.notEqual(end, -1, `scanner script end marker was not found in ${filePath}`);

  const indentation = startMarker.indexOf('const');
  return contents
    .slice(start, end)
    .split('\n')
    .map(line => line.startsWith(' '.repeat(indentation)) ? line.slice(indentation) : line)
    .join('\n')
    .trimEnd();
}

function getGeneratedScannerScript() {
  const sourceScript = extractIndentedScript(
    workflowPath,
    "              const fs = require('fs');",
    '\n\n---\n',
  );
  const generatedScript = extractIndentedScript(
    workflowLockPath,
    "            const fs = require('fs');",
  );
  assert.equal(
    generatedScript,
    sourceScript,
    'rerun-review-scanner.lock.yml is stale; run strict gh-aw compilation',
  );
  return generatedScript;
}

function createAction(prNumber, overrides = {}) {
  return {
    prNumber,
    decision: 'skip',
    headSha: String(prNumber).padStart(40, '0'),
    activityCheckpoint: 1780219800000 + prNumber,
    activityKey: String(prNumber).repeat(64).slice(0, 64),
    rerunCommentId: 0,
    ...overrides,
  };
}

function httpError(status, message) {
  return Object.assign(new Error(message), { status });
}

async function executeGeneratedScanner({
  actions,
  labelSnapshots = {},
  hooks = {},
}) {
  const events = [];
  const messages = {
    errors: [],
    failures: [],
    info: [],
    warnings: [],
  };
  const labelReadCounts = new Map();
  const headByPR = new Map(actions.map(action => [action.prNumber, action.headSha]));
  const existingDeclinedLabel = {
    data: {
      description: declinedLabel.description,
      color: declinedLabel.color,
    },
  };

  const virtualFs = {
    existsSync: candidatePath => candidatePath === 'actions.json',
    readFileSync: candidatePath => {
      assert.equal(candidatePath, 'actions.json');
      return JSON.stringify(actions);
    },
  };
  const requireStub = moduleName => {
    assert.equal(moduleName, 'fs');
    return virtualFs;
  };
  const processStub = {
    env: {
      DRY_RUN: 'false',
      RERUN_ACTIONS_PATH: 'actions.json',
    },
  };
  const core = {
    error: message => messages.errors.push(String(message)),
    info: message => messages.info.push(String(message)),
    setFailed: message => messages.failures.push(String(message)),
    warning: message => messages.warnings.push(String(message)),
  };

  const github = {
    graphql: async (_query, variables) => {
      events.push(`graphql:${variables.number}`);
      if (hooks.graphql) {
        return hooks.graphql(variables);
      }
      return {
        repository: {
          pullRequest: {
            comments: {
              nodes: [],
              pageInfo: {
                hasPreviousPage: false,
                startCursor: null,
              },
            },
          },
        },
      };
    },
    rest: {
      actions: {
        createWorkflowDispatch: async args => {
          events.push(`dispatch:${args.inputs.pr_number}`);
        },
      },
      issues: {
        addLabels: async args => {
          events.push(`addLabels:${args.issue_number}`);
          if (hooks.addLabels) {
            return hooks.addLabels(args);
          }
        },
        createComment: async args => {
          events.push(`createComment:${args.issue_number}`);
          if (hooks.createComment) {
            return hooks.createComment(args);
          }
        },
        createLabel: async args => {
          events.push('createLabel');
          if (hooks.createLabel) {
            return hooks.createLabel(args);
          }
        },
        get: async args => {
          const prNumber = args.issue_number;
          const snapshots = labelSnapshots[prNumber] ?? [[readyLabel], [readyLabel]];
          const readIndex = labelReadCounts.get(prNumber) ?? 0;
          labelReadCounts.set(prNumber, readIndex + 1);
          const names = snapshots[Math.min(readIndex, snapshots.length - 1)];
          events.push(`getLabels:${prNumber}:${readIndex}`);
          return { data: { labels: names.map(name => ({ name })) } };
        },
        getLabel: async args => {
          events.push('getLabel');
          if (hooks.getLabel) {
            return hooks.getLabel(args);
          }
          return existingDeclinedLabel;
        },
        removeLabel: async args => {
          events.push(`removeLabel:${args.issue_number}:${args.name}`);
          if (hooks.removeLabel) {
            return hooks.removeLabel(args);
          }
        },
        updateLabel: async args => {
          events.push('updateLabel');
          if (hooks.updateLabel) {
            return hooks.updateLabel(args);
          }
        },
      },
      pulls: {
        get: async args => {
          events.push(`getPull:${args.pull_number}`);
          if (hooks.getPull) {
            return hooks.getPull(args);
          }
          return {
            data: {
              state: 'open',
              draft: false,
              head: { sha: headByPR.get(args.pull_number) },
            },
          };
        },
      },
      reactions: {
        createForIssueComment: async args => {
          events.push(`reaction:${args.comment_id}`);
        },
      },
    },
  };

  const AsyncFunction = Object.getPrototypeOf(async function () {}).constructor;
  const generatedScript = getGeneratedScannerScript();
  const run = new AsyncFunction('require', 'process', 'context', 'core', 'github', generatedScript);
  await run(requireStub, processStub, { repo: { owner: 'dotnet', repo: 'maui' } }, core, github);

  return { events, messages };
}

test('markDeclined recovers when the decline label is created concurrently', async () => {
  const action = createAction(1);
  let getLabelCalls = 0;
  const result = await executeGeneratedScanner({
    actions: [action],
    hooks: {
      createLabel: async () => {
        throw httpError(422, 'already exists');
      },
      getLabel: async () => {
        getLabelCalls += 1;
        if (getLabelCalls === 1) {
          throw httpError(404, 'not found');
        }
        return {
          data: {
            description: declinedLabel.description,
            color: declinedLabel.color,
          },
        };
      },
    },
  });

  assert.equal(getLabelCalls, 2);
  assert.ok(result.events.indexOf('createLabel') < result.events.indexOf('createComment:1'));
  assert.ok(result.events.indexOf('createComment:1') < result.events.indexOf('addLabels:1'));
  assert.ok(result.events.indexOf('addLabels:1') < result.events.indexOf(`removeLabel:1:${readyLabel}`));
  assert.deepEqual(result.messages.failures, []);
});

test('markDeclined revalidates labels before side effects and stops on an in-progress race', async () => {
  const result = await executeGeneratedScanner({
    actions: [createAction(2)],
    labelSnapshots: {
      2: [
        [readyLabel],
        [readyLabel, inProgressLabel],
      ],
    },
  });

  assert.ok(result.events.includes('getLabels:2:0'));
  assert.ok(result.events.includes('getLabels:2:1'));
  assert.ok(!result.events.includes('createComment:2'));
  assert.ok(!result.events.includes('addLabels:2'));
  assert.ok(!result.events.includes(`removeLabel:2:${readyLabel}`));
  assert.match(result.messages.info.join('\n'), /acquired s\/agent-review-in-progress/);
  assert.deepEqual(result.messages.failures, []);
});

test('markDeclined failures remain visible while unrelated PR actions continue', async () => {
  const result = await executeGeneratedScanner({
    actions: [createAction(3), createAction(4)],
    hooks: {
      addLabels: async args => {
        if (args.issue_number === 3) {
          throw httpError(503, 'service unavailable');
        }
      },
    },
  });

  assert.ok(result.events.includes('createComment:3'));
  assert.ok(!result.events.includes(`removeLabel:3:${readyLabel}`));
  assert.ok(result.events.includes('createComment:4'));
  assert.ok(result.events.includes(`removeLabel:4:${readyLabel}`));
  assert.match(result.messages.errors.join('\n'), /Failed to process PR #3/);
  assert.deepEqual(result.messages.failures, ['One or more rerun decisions failed to dispatch.']);
});

test('comment-less autonomous trigger skips when the PR becomes draft before dispatch', async () => {
  const action = createAction(5, {
    decision: 'trigger',
    platform: 'android',
    pipelineRef: 'main',
  });
  const result = await executeGeneratedScanner({
    actions: [action],
    hooks: {
      getPull: async () => ({
        data: {
          state: 'open',
          draft: true,
          head: { sha: action.headSha },
        },
      }),
    },
  });

  assert.ok(result.events.includes('getPull:5'));
  assert.ok(!result.events.includes('dispatch:5'));
  assert.ok(!result.events.includes(`removeLabel:5:${declinedLabel.name}`));
  assert.match(result.messages.info.join('\n'), /became draft before dispatch/);
  assert.deepEqual(result.messages.failures, []);
});

test('comment-less autonomous trigger skips when the head changes before dispatch', async () => {
  const action = createAction(6, {
    decision: 'trigger',
    platform: 'android',
    pipelineRef: 'main',
  });
  let liveReads = 0;
  const result = await executeGeneratedScanner({
    actions: [action],
    hooks: {
      getPull: async () => {
        liveReads += 1;
        return {
          data: {
            state: 'open',
            draft: false,
            head: { sha: liveReads === 1 ? action.headSha : 'f'.repeat(40) },
          },
        };
      },
    },
  });

  assert.equal(liveReads, 2);
  assert.ok(result.events.includes(`removeLabel:6:${declinedLabel.name}`));
  assert.ok(!result.events.includes('dispatch:6'));
  assert.match(result.messages.info.join('\n'), /head changed before dispatch/);
  assert.deepEqual(result.messages.failures, []);
});

test('comment-based trigger preserves its authenticated dispatch flow', async () => {
  const action = createAction(7, {
    decision: 'trigger',
    platform: 'ios',
    pipelineRef: 'main',
    rerunCommentId: 42,
  });
  const result = await executeGeneratedScanner({
    actions: [action],
    hooks: {
      getPull: async () => {
        throw new Error('comment-based triggers must not use autonomous revalidation');
      },
    },
  });

  assert.ok(!result.events.includes('getPull:7'));
  assert.ok(result.events.includes(`removeLabel:7:${declinedLabel.name}`));
  assert.ok(result.events.includes('dispatch:7'));
  assert.ok(result.events.includes('reaction:42'));
  assert.deepEqual(result.messages.failures, []);
});
