const fs = require("node:fs");
const path = require("node:path");

module.exports = async function freezeBuildEvidence() {
  const branch = process.env.CI_SCAN_BRANCH;
  const workflowSha = process.env.CI_SCAN_WORKFLOW_SHA;
  if (!["main", "net11.0"].includes(branch) || !/^[0-9a-f]{40}$/.test(workflowSha ?? "")) {
    throw new Error("Invalid scanner branch or immutable workflow SHA.");
  }
  const artifactRoot = path.join(process.env.RUNNER_TEMP, "ci-scan");
  const agentRoot = "/tmp/gh-aw/agent/trusted";
  const definitions = [
    { name: "maui-pr", definition_id: 302 },
    { name: "maui-pr-devicetests", definition_id: 314 },
    { name: "maui-pr-uitests", definition_id: 313 },
  ];
  const cutoff = Date.now() - 7 * 24 * 60 * 60 * 1000;
  const fetchResponse = async url => {
    const response = await fetch(url, { signal: AbortSignal.timeout(30000) });
    if (!response.ok) {
      throw new Error(`CI evidence request failed with HTTP ${response.status}.`);
    }
    return response;
  };
  const fetchJson = async url => (await fetchResponse(url)).json();
  const fetchText = async url => {
    const text = await (await fetchResponse(url)).text();
    if (text.length > 20_000_000) {
      throw new Error("CI log exceeded the 20 MB evidence limit.");
    }
    return text;
  };
  const writeEvidence = (relativePath, content) => {
    for (const root of [artifactRoot, agentRoot]) {
      const output = path.join(root, relativePath);
      fs.mkdirSync(path.dirname(output), { recursive: true });
      fs.writeFileSync(output, content);
    }
  };
  const pipelines = [];
  for (const definition of definitions) {
    const query = new URLSearchParams({
      definitions: String(definition.definition_id),
      branchName: `refs/heads/${branch}`,
      statusFilter: "completed",
      resultFilter: "succeeded,failed,partiallySucceeded",
      queryOrder: "finishTimeDescending",
      "$top": "1",
      "api-version": "7.1",
    });
    const builds = await fetchJson(
      `https://dev.azure.com/dnceng-public/public/_apis/build/builds?${query}`);
    if (!builds || !Array.isArray(builds.value)) {
      throw new Error(`Malformed build list for ${definition.name}.`);
    }
    const build = builds.value[0];
    if (!build) {
      pipelines.push({ ...definition, status: "skipped-no-recent-build" });
      continue;
    }
    const finishTime = Date.parse(build.finishTime);
    if (!Number.isFinite(finishTime)) {
      throw new Error(`Invalid finishTime for ${definition.name}.`);
    }
    if (finishTime < cutoff) {
      pipelines.push({ ...definition, status: "skipped-no-recent-build" });
      continue;
    }
    const buildId = Number(build.id);
    if (!Number.isSafeInteger(buildId) || buildId <= 0 ||
        Number(build.definition?.id) !== definition.definition_id ||
        build.sourceBranch !== `refs/heads/${branch}` || build.status !== "completed") {
      throw new Error(`Invalid build evidence for ${definition.name}.`);
    }
    const timeline = await fetchJson(
      `https://dev.azure.com/dnceng-public/public/_apis/build/builds/${buildId}/timeline?api-version=7.1`);
    if (!timeline || !Array.isArray(timeline.records)) {
      throw new Error(`Malformed timeline for ${definition.name} build ${buildId}.`);
    }
    const records = timeline.records;
    const children = new Map();
    for (const record of records) {
      if (!children.has(record.parentId)) {
        children.set(record.parentId, []);
      }
      children.get(record.parentId).push(record);
    }
    const requiredLogIds = new Set();
    const failedLeafLogIds = new Set();
    for (const record of records) {
      const logId = Number(record.log?.id);
      if (!Number.isSafeInteger(logId) || logId <= 0) {
        continue;
      }
      const hasFailedChild = (children.get(record.id) ?? []).some(child => child.result === "failed");
      const failedLeaf = record.result === "failed" && !hasFailedChild;
      const deviceSubmission = definition.definition_id === 314 && record.type === "Task" &&
        /^DeviceTests.+ \((?:Unix|Windows)\)$/.test(String(record.name ?? "")) &&
        record.result !== "skipped";
      if (failedLeaf || deviceSubmission) {
        requiredLogIds.add(logId);
      }
      if (failedLeaf) {
        failedLeafLogIds.add(logId);
      }
    }
    if (build.result !== "succeeded" && requiredLogIds.size === 0) {
      throw new Error(`No inspectable failure logs for ${definition.name}.`);
    }
    for (const logId of [...requiredLogIds].sort((a, b) => a - b)) {
      const azdoLog = await fetchText(
        `https://dev.azure.com/dnceng-public/public/_apis/build/builds/${buildId}/logs/${logId}?api-version=7.1`);
      const evidence = [`===== AzDO log ${buildId}/${logId} =====`, azdoLog];
      if (definition.definition_id === 314) {
        const jobIds = [...new Set(
          [...azdoLog.matchAll(/https:\/\/helix\.dot\.net\/api\/jobs\/([0-9a-f-]{36})\/workitems/ig)]
            .map(match => match[1].toLowerCase()))];
        for (const jobId of jobIds) {
          let workItems;
          for (let attempt = 1; attempt <= 6; attempt++) {
            const [details, items] = await Promise.all([
              fetchJson(`https://helix.dot.net/api/jobs/${jobId}/details?api-version=2019-06-17`),
              fetchJson(`https://helix.dot.net/api/jobs/${jobId}/workitems?api-version=2019-06-17`),
            ]);
            if (!Array.isArray(items)) {
              throw new Error(`Invalid work-item evidence for Helix job ${jobId}.`);
            }
            const counts = details?.WorkItems;
            const initialCount = Number(details?.InitialWorkItemCount);
            const finishedCount = Number(counts?.Finished);
            const pendingCounts = [counts?.Unscheduled, counts?.Waiting, counts?.Running].map(Number);
            const terminalItems = items.every(item => {
              const state = String(item.State ?? "").toLowerCase();
              const hasExitCode = item.ExitCode !== null && item.ExitCode !== undefined &&
                item.ExitCode !== "" && Number.isSafeInteger(Number(item.ExitCode));
              return (state === "finished" || state === "failed") && (state === "failed" || hasExitCode);
            });
            if (Number.isSafeInteger(initialCount) && initialCount >= 0 &&
                Number.isSafeInteger(finishedCount) && finishedCount >= initialCount && finishedCount > 0 &&
                pendingCounts.every(count => Number.isSafeInteger(count) && count === 0) &&
                details.Finished && items.length >= finishedCount && terminalItems) {
              workItems = items;
              break;
            }
            if (attempt < 6) {
              await new Promise(resolve => setTimeout(resolve, 5000));
            }
          }
          if (!workItems) {
            throw new Error(`Helix job ${jobId} has incomplete terminal work-item evidence.`);
          }
          for (const item of workItems) {
            const name = String(item.Name ?? "").trim();
            if (!name || name.length > 1000 || /[\r\n]/.test(name)) {
              throw new Error(`Invalid work-item name for Helix job ${jobId}.`);
            }
            const deadletter = String(item.ConsoleOutputUri ?? "")
              .toLowerCase().includes("helix-workitem-deadletter");
            if (String(item.State).toLowerCase() !== "failed" && Number(item.ExitCode) === 0 && !deadletter) {
              continue;
            }
            const consoleUrl = new URL(item.ConsoleOutputUri);
            if (consoleUrl.protocol !== "https:" ||
                (!deadletter && !consoleUrl.hostname.endsWith(".blob.core.windows.net"))) {
              throw new Error(`Invalid console URL for Helix job ${jobId}.`);
            }
            evidence.push(`===== Helix console ${jobId}/${name} =====`,
              deadletter
                ? `Helix work item ${name} was deadlettered: ${consoleUrl}`
                : await fetchText(consoleUrl.toString()));
            if (evidence.length > 402 || evidence.reduce((size, text) => size + text.length, 0) > 25_000_000) {
              throw new Error(`Evidence for ${definition.name} ${buildId}/${logId} exceeds its safety limit.`);
            }
            failedLeafLogIds.add(logId);
          }
        }
      }
      writeEvidence(`evidence/${definition.name}/${buildId}-${logId}.log`, evidence.join("\n"));
    }
    pipelines.push({
      ...definition,
      status: "scanned",
      build_id: buildId,
      source_version: build.sourceVersion,
      result: build.result,
      required_log_ids: [...requiredLogIds].sort((a, b) => a - b),
      failed_leaf_log_ids: [...failedLeafLogIds].sort((a, b) => a - b),
    });
  }
  writeEvidence("expected-builds.json", JSON.stringify({
    schema_version: 1,
    branch,
    trusted_publisher_ref: workflowSha,
    pipelines,
  }, null, 2));
};
