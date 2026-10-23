const crypto = require("node:crypto");
const fs = require("node:fs");
const path = require("node:path");

const outputTypes = new Set([
  "create_pull_request",
  "push_to_pull_request_branch",
  "update_pull_request",
  "add_comment",
  "mark_pull_request_as_ready_for_review",
  "add_labels",
  "noop",
  "report_incomplete",
]);
const nonPrTypes = new Set(["create_pull_request", "noop", "report_incomplete"]);

function parseArguments(argumentsList, allowedNames) {
  const values = {};
  for (let index = 0; index < argumentsList.length; index += 2) {
    const name = argumentsList[index].replace(/^--?/, "").toLowerCase();
    if (!argumentsList[index].startsWith("-") || !allowedNames.includes(name)) {
      throw new Error(`Unknown argument: ${argumentsList[index]}`);
    }
    if (Object.hasOwn(values, name) || argumentsList[index + 1] === undefined) {
      throw new Error(`Argument '${name}' must have exactly one value.`);
    }
    values[name] = argumentsList[index + 1];
  }
  return values;
}

function boundedInteger(value, minimum, maximum, name) {
  const number = Number(value);
  if (!/^[0-9]+$/.test(String(value)) || !Number.isSafeInteger(number) ||
      number < minimum || number > maximum) {
    throw new Error(`${name} must be an integer in ${minimum}..${maximum}.`);
  }
  return number;
}

function registerExpectation(type, pullRequestNumber, outputDirectory) {
  if (!outputTypes.has(type)) {
    throw new Error(`Unsupported safe output type: ${type}`);
  }
  const pr = pullRequestNumber === undefined
    ? null
    : boundedInteger(pullRequestNumber, 1, 2147483647, "PullRequestNumber");
  if (!nonPrTypes.has(type) && pr === null) {
    throw new Error(`PullRequestNumber is required for safe output type '${type}'.`);
  }
  if (nonPrTypes.has(type) && pr !== null) {
    throw new Error(`PullRequestNumber must be omitted for safe output type '${type}'.`);
  }

  const directory = outputDirectory ?? "/tmp/gh-aw/agent/ci-fix-output-expectations";
  if (!directory) {
    throw new Error("OutputDirectory must not be empty.");
  }
  fs.mkdirSync(directory, { recursive: true });
  const file = path.join(directory, `${crypto.randomUUID().replaceAll("-", "")}.json`);
  fs.writeFileSync(file, `${JSON.stringify({
    type,
    pullRequestNumber: pr,
    registeredAt: new Date().toISOString(),
  })}\n`, { encoding: "utf8", flag: "wx", mode: 0o600 });
  return file;
}

if (require.main === module) {
  const values = parseArguments(process.argv.slice(2), [
    "type", "pullrequestnumber", "outputdirectory",
  ]);
  console.log(registerExpectation(values.type, values.pullrequestnumber, values.outputdirectory));
}

module.exports = { boundedInteger, parseArguments, registerExpectation };
