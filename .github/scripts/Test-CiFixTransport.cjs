const fs = require("node:fs");
const os = require("node:os");
const path = require("node:path");
const { spawnSync } = require("node:child_process");
const {
  boundedInteger,
  parseArguments,
  registerExpectation,
} = require("./Register-CiFixSafeOutputExpectation.cjs");

function git(argumentsList, allowedExitCodes = [0]) {
  const result = spawnSync("git", argumentsList, { encoding: "utf8", maxBuffer: 10485760 });
  if (result.error) {
    throw new Error(`Could not execute git: ${result.error.message}`, { cause: result.error });
  }
  if (!allowedExitCodes.includes(result.status)) {
    throw new Error(`git ${argumentsList[0]} failed (${result.status}): ${result.stderr.trim()}`);
  }
  return result;
}

const values = parseArguments(process.argv.slice(2), [
  "baseref", "maxfiles", "maxpatchbytes", "maxcommits",
  "expectedoutputtype", "pullrequestnumber", "expectationdirectory",
]);
const baseRef = values.baseref;
if (!baseRef || baseRef.startsWith("-")) {
  throw new Error("BaseRef must be a nonempty commit reference, not a git option.");
}
const maxFiles = boundedInteger(values.maxfiles ?? "20", 1, 100, "MaxFiles");
const maxPatchBytes = boundedInteger(values.maxpatchbytes ?? "262144", 1024, 10485760, "MaxPatchBytes");
const maxCommits = boundedInteger(values.maxcommits ?? "3", 1, 10, "MaxCommits");
const expectedOutputType = values.expectedoutputtype;
if (!["create_pull_request", "push_to_pull_request_branch"].includes(expectedOutputType)) {
  throw new Error("ExpectedOutputType must be create_pull_request or push_to_pull_request_branch.");
}
const pr = values.pullrequestnumber === undefined
  ? null
  : boundedInteger(values.pullrequestnumber, 1, 2147483647, "PullRequestNumber");
if (expectedOutputType === "push_to_pull_request_branch" && pr === null) {
  throw new Error("PullRequestNumber is required when advancing an existing PR.");
}
if (expectedOutputType === "create_pull_request" && pr !== null) {
  throw new Error("PullRequestNumber must be omitted when creating a PR.");
}

git(["rev-parse", "--verify", `${baseRef}^{commit}`]);
if (git(["merge-base", "--is-ancestor", baseRef, "HEAD"], [0, 1]).status !== 0) {
  throw new Error(`Transport rejected: '${baseRef}' is not an ancestor of HEAD (rebase/reset/base divergence).`);
}
const range = `${baseRef}..HEAD`;
const commitCount = Number(git(["rev-list", "--count", range, "--"]).stdout.trim());
if (!Number.isSafeInteger(commitCount) || commitCount < 1 || commitCount > maxCommits) {
  throw new Error(`Transport rejected: ${commitCount} new commits; expected 1..${maxCommits}.`);
}
if (git(["rev-list", "--count", "--merges", range, "--"]).stdout.trim() !== "0") {
  throw new Error("Transport rejected: merge commits are not append-only CI-fix attempts.");
}

const changedFiles = git(["diff", "--name-only", "-z", "--no-renames", range, "--"])
  .stdout.split("\0").filter(Boolean);
if (changedFiles.length < 1 || changedFiles.length > maxFiles) {
  throw new Error(`Transport rejected: ${changedFiles.length} changed files; expected 1..${maxFiles}.`);
}
const rejectedFiles = changedFiles.filter(file =>
  !/^(src\/(AI|Core|Controls|Essentials|BlazorWebView|TestUtils|Templates)\/|.+\/PublicAPI\.Unshipped\.txt$)/.test(file));
if (rejectedFiles.length > 0) {
  throw new Error(`Transport rejected: out-of-scope paths: ${rejectedFiles.join(", ")}`);
}

const temporaryDirectory = fs.mkdtempSync(path.join(os.tmpdir(), "ci-fix-transport-"));
const patchFile = path.join(temporaryDirectory, "transport.patch");
let patchBytes;
try {
  git(["diff", "--binary", "--no-ext-diff", "--no-renames", `--output=${patchFile}`, range, "--"]);
  patchBytes = fs.statSync(patchFile).size;
} finally {
  if (fs.existsSync(patchFile)) {
    fs.unlinkSync(patchFile);
  }
  fs.rmdirSync(temporaryDirectory);
}
if (patchBytes < 1 || patchBytes > maxPatchBytes) {
  throw new Error(`Transport rejected: ${patchBytes} patch bytes; expected 1..${maxPatchBytes}.`);
}

const head = git(["rev-parse", "HEAD"]).stdout.trim();
registerExpectation(expectedOutputType, values.pullrequestnumber, values.expectationdirectory);
console.log(JSON.stringify({
  baseRef,
  head,
  commitCount,
  changedFileCount: changedFiles.length,
  changedFiles,
  patchBytes,
  maxPatchBytes,
  expectedOutputType,
  pullRequestNumber: pr,
}, null, 2));
