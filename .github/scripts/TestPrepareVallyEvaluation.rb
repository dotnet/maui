#!/usr/bin/env ruby

require "fileutils"
require "json"
require "minitest/autorun"
require "open3"
require "pathname"
require "tmpdir"
require "yaml"

PREPARER = File.realpath(ARGV.shift || File.join(__dir__, "PrepareVallyEvaluation.rb"))
RESTORE_SPEC = ARGV.empty? ? nil : File.realpath(ARGV.shift)
VERIFICATION_SPEC = ARGV.empty? ? nil : File.realpath(ARGV.shift)
SETUP_RUNTIME = ARGV.empty? ? nil : File.realpath(ARGV.shift)
TOKEN_SELECTOR = ARGV.empty? ? nil : File.realpath(ARGV.shift)
SKILL_VALIDATION_WORKFLOW = ARGV.empty? ? nil : File.realpath(ARGV.shift)
require PREPARER

class TestPrepareVallyEvaluation < Minitest::Test
  def setup
    @repo_root = Dir.mktmpdir("prepare-vally-evaluation-")
    @tests_path = File.join(@repo_root, ".github", "skills", "test-skill", "tests")
    FileUtils.mkdir_p(@tests_path)
  end

  def teardown
    FileUtils.remove_entry(@repo_root)
  end

  def test_accepts_spec_without_executable_environment
    write_spec("environment" => { "skills" => [".."] })

    _stdout, stderr, status = run_validator

    assert status.success?, stderr
  end

  def test_rejects_parameterized_environment_key
    write_raw_spec(<<~YAML)
      environment:
        ${KEY=commands}:
          - echo "$COPILOT_GITHUB_TOKEN"
    YAML

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "uses Vally parameter placeholders"
  end

  def test_rejects_yaml_aliases
    write_raw_spec(<<~YAML)
      shared: &shared
        skills: [".."]
      environment: *shared
    YAML

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "uses YAML aliases"
  end

  def test_rejects_yaml_merge_keys_before_semantic_validation
    write_raw_spec(<<~YAML)
      stimuli:
        - name: merge-key-bypass
          prompt: unsafe
          <<:
            prompt: safe
    YAML

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "uses a YAML merge key"
    assert_includes stderr, "trusted validation requires explicit mappings"
  end

  def test_rejects_explicitly_tagged_yaml_merge_keys
    write_raw_spec(<<~YAML)
      !!merge harmless:
        commands:
          - echo injected
      environment:
        skills: [".."]
    YAML

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "uses a YAML merge key"
    assert_includes stderr, "trusted validation requires explicit mappings"
  end

  def test_rejects_forbidden_environment_key
    write_spec("environment" => { "mcpServers" => {} })

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "uses forbidden key(s): mcpServers"
  end

  def test_rejects_mock_executor_override
    write_spec("defaults" => { "executor" => "mock" })

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "must not override the trusted copilot-sdk executor"
  end

  def test_rejects_executable_grader
    write_spec(
      "stimuli" => [
        {
          "name" => "executable-grader",
          "graders" => [{ "type" => "run-command", "config" => { "command" => "echo unsafe" } }]
        }
      ]
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "uses forbidden grader run-command"
  end

  def test_rejects_absolute_prompt_attachment
    write_spec(
      "stimuli" => [
        {
          "name" => "sensitive-attachment",
          "attachments" => ["/proc/self/environ"]
        }
      ]
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "stimuli[0].attachments is forbidden"
  end

  def test_rejects_relative_prompt_attachment
    write_fixture("attachment.txt")
    write_spec(
      "stimuli" => [
        {
          "name" => "fixture-attachment",
          "attachments" => ["attachment.txt"]
        }
      ]
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "stimuli[0].attachments is forbidden"
  end

  def test_rejects_named_skill_invocation_grader
    write_spec(
      "stimuli" => [
        {
          "name" => "renamed-skill-invocation",
          "graders" => [
            {
              "type" => "skill-invocation",
              "name" => "routing",
              "config" => { "required" => ["test-skill"] }
            }
          ]
        }
      ]
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "must not rename the skill-invocation grader"
  end

  def test_rejects_duplicate_skill_invocation_graders
    write_spec(
      "stimuli" => [
        {
          "name" => "duplicate-routing",
          "graders" => [
            { "type" => "skill-invocation", "config" => { "required" => ["test-skill"] } },
            { "type" => "skill-invocation", "config" => { "required" => ["test-skill"] } }
          ]
        }
      ]
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "contains multiple skill-invocation graders"
  end

  def test_rejects_top_level_graders_ignored_by_pinned_vally
    write_spec(
      "graders" => [
        { "type" => "skill-invocation", "config" => { "required" => ["test-skill"] } }
      ],
      "stimuli" => [
        {
          "name" => "duplicate-routing",
          "graders" => [
            { "type" => "skill-invocation", "config" => { "required" => ["test-skill"] } }
          ]
        }
      ]
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "uses top-level graders, which pinned Vally does not execute"
  end

  def test_requires_skill_invocation_grader_for_routing_specs
    routing_tests_path = File.join(@repo_root, ".github", "skills", "try-fix", "tests")
    FileUtils.mkdir_p(routing_tests_path)
    File.write(
      File.join(routing_tests_path, "eval.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "routing-regression",
            "graders" => [{ "type" => "output-contains", "config" => { "substring" => "done" } }]
          }
        ]
      )
    )

    _stdout, stderr, status = run_validator(routing_tests_path)

    refute status.success?
    assert_includes stderr, "must include required skill-invocation targeting try-fix"
  end

  def test_rejects_decorative_skill_invocation_grader_for_routing_specs
    routing_tests_path = File.join(@repo_root, ".github", "skills", "try-fix", "tests")
    FileUtils.mkdir_p(routing_tests_path)
    File.write(
      File.join(routing_tests_path, "eval.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "decorative-routing-grader",
            "graders" => [
              {
                "type" => "skill-invocation",
                "config" => { "required" => ["another-skill"] }
              }
            ]
          }
        ]
      )
    )

    _stdout, stderr, status = run_validator(routing_tests_path)

    refute status.success?
    assert_includes stderr, "must include required skill-invocation targeting try-fix"
  end

  def test_rejects_disallowed_grader_for_required_routing_scenario
    routing_tests_path = File.join(@repo_root, ".github", "skills", "try-fix", "tests")
    FileUtils.mkdir_p(routing_tests_path)
    File.write(
      File.join(routing_tests_path, "eval.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "routing-regression",
            "graders" => [
              {
                "type" => "skill-invocation",
                "config" => { "disallowed" => ["try-fix"] }
              }
            ]
          }
        ]
      )
    )

    _stdout, stderr, status = run_validator(routing_tests_path)

    refute status.success?
    assert_includes stderr, "must include required skill-invocation targeting try-fix"
  end

  def test_rejects_executor_restriction_for_mandatory_invocation_spec
    routing_tests_path = File.join(@repo_root, ".github", "skills", "try-fix", "tests")
    FileUtils.mkdir_p(routing_tests_path)
    File.write(
      File.join(routing_tests_path, "eval.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "routing-regression",
            "supported_executors" => ["unsupported-executor"],
            "graders" => [
              {
                "type" => "skill-invocation",
                "config" => { "required" => ["try-fix"] }
              }
            ]
          }
        ]
      )
    )

    _stdout, stderr, status = run_validator(routing_tests_path)

    refute status.success?
    assert_includes stderr, "must not restrict supported_executors"
  end

  def test_rejects_aggregate_scoring_for_per_grader_must_pass_spec
    protocol_tests_path = File.join(
      @repo_root,
      ".github",
      "skills",
      "verify-tests-fail-without-fix",
      "tests"
    )
    FileUtils.mkdir_p(protocol_tests_path)
    File.write(
      File.join(protocol_tests_path, "eval.protocol.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "must-pass-protocol",
            "graders" => [
              {
                "type" => "skill-invocation",
                "config" => { "required" => ["verify-tests-fail-without-fix"] }
              }
            ]
          }
        ],
        "scoring" => { "threshold" => 1.0 }
      )
    )

    _stdout, stderr, status = run_validator(protocol_tests_path)

    refute status.success?
    assert_includes stderr, "must use an empty scoring map so every grader must pass its declared threshold"
  end

  def test_requires_producer_trace_output_contract_in_skill_template
    code_review_root = File.join(@repo_root, ".github", "skills", "code-review")
    code_review_tests_path = File.join(code_review_root, "tests")
    FileUtils.mkdir_p(code_review_tests_path)
    File.write(File.join(code_review_root, "SKILL.md"), "### Step 1.5: Trace External Output Contracts (Always Active)\n")
    File.write(
      File.join(code_review_tests_path, "eval.producer-trace.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "producer-trace",
            "graders" => [
              {
                "type" => "skill-invocation",
                "config" => { "required" => ["code-review"] }
              }
            ]
          }
        ]
      )
    )

    _stdout, stderr, status = run_validator(code_review_tests_path)

    refute status.success?
    assert_includes stderr, "requires External Output Contract output section"
  end

  def test_requires_try_fix_restoration_contract_in_skill_template
    try_fix_root = File.join(@repo_root, ".github", "skills", "try-fix")
    try_fix_tests_path = File.join(try_fix_root, "tests")
    FileUtils.mkdir_p(try_fix_tests_path)
    File.write(File.join(try_fix_root, "SKILL.md"), "# Try Fix\n")
    File.write(
      File.join(try_fix_tests_path, "eval.restore.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "restore-protocol",
            "graders" => [
              {
                "type" => "skill-invocation",
                "config" => { "required" => ["try-fix"] }
              }
            ]
          }
        ]
      )
    )

    _stdout, stderr, status = run_validator(try_fix_tests_path)

    refute status.success?
    assert_includes stderr, "requires mandatory baseline step"
  end

  def test_requires_try_fix_to_preserve_injected_skill_directory
    try_fix_root = File.join(@repo_root, ".github", "skills", "try-fix")
    try_fix_tests_path = File.join(try_fix_root, "tests")
    FileUtils.mkdir_p(try_fix_tests_path)
    File.write(
      File.join(try_fix_root, "SKILL.md"),
      <<~MARKDOWN
        ### Step 2: Establish Baseline (MANDATORY)
        ### Step 9: Restore Working Directory (MANDATORY - always)
        EstablishBrokenBaseline.ps1 -Restore
        No baseline state found and Restored False
        not `git checkout`, `git restore`, or `git reset`
      MARKDOWN
    )
    File.write(
      File.join(try_fix_tests_path, "eval.restore.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "restore-protocol",
            "graders" => [
              {
                "type" => "skill-invocation",
                "config" => { "required" => ["try-fix"] }
              }
            ]
          }
        ]
      )
    )

    _stdout, stderr, status = run_validator(try_fix_tests_path)

    refute status.success?
    assert_includes stderr, "requires injected skill preservation"
  end

  def test_requires_restore_fixture_to_preserve_injected_skill_directory
    try_fix_root = File.join(@repo_root, ".github", "skills", "try-fix")
    try_fix_tests_path = File.join(try_fix_root, "tests")
    FileUtils.mkdir_p(try_fix_tests_path)
    File.write(
      File.join(try_fix_root, "SKILL.md"),
      <<~MARKDOWN
        ### Step 2: Establish Baseline (MANDATORY)
        ### Step 9: Restore Working Directory (MANDATORY - always)
        EstablishBrokenBaseline.ps1 -Restore
        No baseline state found and Restored False
        not `git checkout`, `git restore`, or `git reset`
        an evaluator-loaded `try-fix/` directory may remain visible in
        `git status --short`; do not delete it.
      MARKDOWN
    )
    File.write(
      File.join(try_fix_tests_path, "eval.restore.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "restores-synthetic-fix-without-raw-git",
            "prompt" => "Restore the target file.",
            "graders" => [
              {
                "type" => "skill-invocation",
                "config" => { "required" => ["try-fix"] }
              }
            ]
          }
        ]
      )
    )

    _stdout, stderr, status = run_validator(try_fix_tests_path)

    refute status.success?
    assert_includes stderr, "prompt requires pre-existing untracked path preservation"
  end

  def test_requires_restore_fixture_stimulus_name
    try_fix_root = File.join(@repo_root, ".github", "skills", "try-fix")
    try_fix_tests_path = File.join(try_fix_root, "tests")
    FileUtils.mkdir_p(try_fix_tests_path)
    File.write(
      File.join(try_fix_root, "SKILL.md"),
      <<~MARKDOWN
        ### Step 2: Establish Baseline (MANDATORY)
        ### Step 9: Restore Working Directory (MANDATORY - always)
        EstablishBrokenBaseline.ps1 -Restore
        No baseline state found and Restored False
        not `git checkout`, `git restore`, or `git reset`
        an evaluator-loaded `try-fix/` directory may remain visible in
        `git status --short`; do not delete it.
      MARKDOWN
    )
    File.write(
      File.join(try_fix_tests_path, "eval.restore.vally.yaml"),
      YAML.dump(
        "stimuli" => [
          {
            "name" => "renamed-restoration-scenario",
            "prompt" => "Restore the target file.",
            "graders" => [
              {
                "type" => "skill-invocation",
                "config" => { "required" => ["try-fix"] }
              }
            ]
          }
        ]
      )
    )

    _stdout, stderr, status = run_validator(try_fix_tests_path)

    refute status.success?
    assert_includes stderr, "is missing required stimulus name(s): restores-synthetic-fix-without-raw-git"
  end

  def test_rejects_required_name_decoy_separate_from_fixture_marker_owner
    try_fix_root = File.join(@repo_root, ".github", "skills", "try-fix")
    try_fix_tests_path = File.join(try_fix_root, "tests")
    FileUtils.mkdir_p(try_fix_tests_path)
    File.write(
      File.join(try_fix_root, "SKILL.md"),
      <<~MARKDOWN
        ### Step 2: Establish Baseline (MANDATORY)
        ### Step 9: Restore Working Directory (MANDATORY - always)
        EstablishBrokenBaseline.ps1 -Restore
        No baseline state found and Restored False
        not `git checkout`, `git restore`, or `git reset`
        an evaluator-loaded `try-fix/` directory may remain visible in
        `git status --short`; do not delete it.
      MARKDOWN
    )
    File.write(
      File.join(try_fix_tests_path, "eval.restore.vally.yaml"),
      <<~YAML
        stimuli:
          - name: restores-synthetic-fix-without-raw-git
            prompt: |
              The evaluator injects a read-only `try-fix/` skill directory that
              must remain exactly as found. Do not remove or modify it.
            graders:
              - type: skill-invocation
                config: { required: [try-fix] }
          - name: renamed-restoration-scenario
            prompt: Restore the target file.
            environment:
              git:
                type: worktree
                ref: 451bd73351c20b49a03e43cafe54a1dfb31e3771 # fixture: restores-synthetic-fix-without-raw-git
                source: .
            graders:
              - type: skill-invocation
                config: { required: [try-fix] }
      YAML
    )

    _stdout, stderr, status = run_validator(try_fix_tests_path)

    refute status.success?
    assert_includes stderr, 'fixture marker restores-synthetic-fix-without-raw-git belongs to stimulus "renamed-restoration-scenario"'
  end

  def test_rejects_duplicate_name_decoy_for_fixture_marker_owner
    try_fix_root = File.join(@repo_root, ".github", "skills", "try-fix")
    try_fix_tests_path = File.join(try_fix_root, "tests")
    FileUtils.mkdir_p(try_fix_tests_path)
    File.write(
      File.join(try_fix_root, "SKILL.md"),
      <<~MARKDOWN
        ### Step 2: Establish Baseline (MANDATORY)
        ### Step 9: Restore Working Directory (MANDATORY - always)
        EstablishBrokenBaseline.ps1 -Restore
        No baseline state found and Restored False
        not `git checkout`, `git restore`, or `git reset`
        an evaluator-loaded `try-fix/` directory may remain visible in
        `git status --short`; do not delete it.
      MARKDOWN
    )
    File.write(
      File.join(try_fix_tests_path, "eval.restore.vally.yaml"),
      <<~YAML
        stimuli:
          - name: restores-synthetic-fix-without-raw-git
            prompt: |
              The evaluator injects a read-only `try-fix/` skill directory that
              must remain exactly as found. Do not remove or modify it.
            graders:
              - type: skill-invocation
                config: { required: [try-fix] }
          - name: restores-synthetic-fix-without-raw-git
            name: renamed-restoration-scenario
            prompt: Restore the target file.
            environment:
              git:
                type: worktree
                ref: 451bd73351c20b49a03e43cafe54a1dfb31e3771 # fixture: restores-synthetic-fix-without-raw-git
                source: .
            graders:
              - type: skill-invocation
                config: { required: [try-fix] }
      YAML
    )

    _stdout, stderr, status = run_validator(try_fix_tests_path)

    refute status.success?
    assert_includes stderr, 'contains duplicate YAML mapping key "name"'
    assert_includes stderr, "first defined at line"
  end

  def test_rejects_semantically_duplicate_tagged_mapping_key
    write_raw_spec(<<~YAML)
      stimuli:
        - name: original-name
          !!binary bmFtZQ==: decoded-name
    YAML

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, 'contains duplicate YAML mapping key "name"'
  end

  def test_comment_posting_cannot_erase_evaluator_verdict
    skip "skill-validation workflow not supplied" unless SKILL_VALIDATION_WORKFLOW

    workflow = File.read(SKILL_VALIDATION_WORKFLOW)
    post_comment = workflow[/      - name: Post comment\n.*?(?=\n  # ={10,}\n  # REPORT STATUS)/m]

    refute_nil post_comment
    output_index = post_comment.index("fs.appendFileSync(outputPath, `eval_passed=")
    api_index = post_comment.index("const comments = await github.paginate(")
    refute_nil output_index
    refute_nil api_index
    assert_operator output_index, :<, api_index
    assert_match(/try \{\s+\/\/ Upsert comment.*?github\.paginate\(.*?\} catch \(err\) \{/m, post_comment)
    assert_match(/retries:\s+3/, post_comment)
  end

  def test_rejects_vcs_metadata_destination
    write_fixture("fixture.txt")
    write_spec(
      "environment" => {
        "files" => [{ "src" => "fixture.txt", "dest" => ".git/config" }]
      }
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "targets forbidden VCS metadata: .git"
  end

  def test_rejects_mock_executor_evidence_destination
    write_fixture("fixture.txt")
    write_spec(
      "environment" => {
        "files" => [{ "src" => "fixture.txt", "dest" => ".mock-executor/stimulus.json" }]
      }
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "targets forbidden VCS metadata: .mock-executor"
  end

  def test_rejects_repository_hook_destination
    write_fixture("fixture.txt")
    write_spec(
      "environment" => {
        "files" => [{ "src" => "fixture.txt", "dest" => ".github/hooks/pre-tool.json" }]
      }
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "targets forbidden repository hooks"
  end

  def test_rejects_normalized_repository_hook_destination
    write_fixture("fixture.txt")
    ["./.github/hooks/pre-tool.json", ".github/./hooks/pre-tool.json"].each do |dest|
      write_spec(
        "environment" => {
          "files" => [{ "src" => "fixture.txt", "dest" => dest }]
        }
      )

      _stdout, stderr, status = run_validator

      refute status.success?, dest
      assert_includes stderr, "targets forbidden repository hooks"
    end
  end

  def test_rejects_repository_copilot_and_claude_settings_destinations
    write_fixture("fixture.txt")
    [
      ".github/copilot/settings.json",
      ".github/copilot/settings.local.json",
      ".claude/settings.json",
      ".claude/settings.local.json"
    ].each do |dest|
      write_spec(
        "environment" => {
          "files" => [{ "src" => "fixture.txt", "dest" => dest }]
        }
      )

      _stdout, stderr, status = run_validator

      refute status.success?, dest
      assert_includes stderr, "must target an approved fixture root"
    end
  end

  def test_rejects_unapproved_file_overlay_destination
    write_fixture("fixture.txt")
    write_spec(
      "environment" => {
        "files" => [{ "src" => "fixture.txt", "dest" => "src/credential-probe.sh" }]
      }
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "must target an approved fixture root"
  end

  def test_accepts_approved_file_overlay_destinations
    write_fixture("fixture.txt")
    [
      "review-input/fixture.txt",
      ".github/pr-review/pr-preflight.md"
    ].each do |dest|
      write_spec(
        "environment" => {
          "files" => [{ "src" => "fixture.txt", "dest" => dest }]
        }
      )

      _stdout, stderr, status = run_validator

      assert status.success?, "#{dest}: #{stderr}"
    end
  end

  def test_rejects_candidate_repository_controls_changed_from_trusted_ref
    write_spec("environment" => { "skills" => [".."] })
    initialize_git_repo
    write_repo_file(".github/copilot/settings.json", "{}\n")
    trusted = commit_all("trusted")

    [
      [".github/copilot/settings.json", "{\"hooks\":{\"preToolUse\":[]}}\n"],
      [".github/copilot/settings.local.json", "{\"disableAllHooks\":false}\n"],
      [".claude/settings.json", "{\"hooks\":{\"preToolUse\":[]}}\n"],
      [".claude/settings.local.json", "{\"hooks\":{\"preToolUse\":[]}}\n"],
      [".github/hooks/pre-tool.json", "{\"version\":1,\"hooks\":{}}\n"],
      [".mcp.json", "{\"mcpServers\":{\"unsafe\":{\"command\":\"sh\"}}}\n"],
      [".github/mcp.json", "{\"mcpServers\":{\"unsafe\":{\"command\":\"sh\"}}}\n"]
    ].each do |path, content|
      write_repo_file(path, content)
      git("add", "-f", path)
      commit_all("candidate control")

      _stdout, stderr, status = run_validator(
        env: { "TRUSTED_BASE_SHA" => trusted },
        validate_only: false
      )

      refute status.success?, path
      assert_includes stderr, "candidate checkout contains untrusted repository control file(s): #{path}"
      git("reset", "--hard", trusted)
    end
  end

  def test_rejects_candidate_repository_controls_deleted_from_trusted_ref
    write_spec("environment" => { "skills" => [".."] })
    initialize_git_repo
    write_repo_file(".github/copilot/settings.json", "{}\n")
    trusted = commit_all("trusted")
    FileUtils.rm(File.join(@repo_root, ".github", "copilot", "settings.json"))
    commit_all("candidate deletes trusted control")

    _stdout, stderr, status = run_validator(
      env: { "TRUSTED_BASE_SHA" => trusted },
      validate_only: false
    )

    refute status.success?
    assert_includes stderr, "candidate checkout contains untrusted repository control file(s): .github/copilot/settings.json"
  end

  def test_rejects_fixture_ref_with_untrusted_repository_controls
    write_spec("environment" => { "skills" => [".."] })
    initialize_git_repo
    write_repo_file(".github/copilot/settings.json", "{}\n")
    trusted = commit_all("trusted")
    write_repo_file(".mcp.json", "{\"mcpServers\":{\"unsafe\":{\"command\":\"sh\"}}}\n")
    untrusted_fixture = commit_all("untrusted fixture")
    FileUtils.rm(File.join(@repo_root, ".mcp.json"))
    write_spec(
      "stimuli" => [
        {
          "name" => "untrusted-fixture",
          "environment" => {
            "git" => {
              "type" => "worktree",
              "source" => ".",
              "ref" => untrusted_fixture
            }
          }
        }
      ]
    )
    commit_all("candidate spec")

    _stdout, stderr, status = run_validator(
      env: { "TRUSTED_BASE_SHA" => trusted },
      validate_only: false
    )

    refute status.success?
    assert_includes stderr, "stimuli[0].environment.git.ref contains untrusted repository control file(s)"
  end

  def test_rejects_fixture_ref_with_deleted_repository_controls
    write_spec("environment" => { "skills" => [".."] })
    initialize_git_repo
    write_repo_file(".github/copilot/settings.json", "{}\n")
    trusted = commit_all("trusted")
    FileUtils.rm(File.join(@repo_root, ".github", "copilot", "settings.json"))
    fixture = commit_all("fixture deletes trusted control")
    write_spec(
      "stimuli" => [
        {
          "name" => "untrusted-fixture",
          "environment" => {
            "git" => {
              "type" => "worktree",
              "source" => ".",
              "ref" => fixture
            }
          }
        }
      ]
    )
    write_repo_file(".github/copilot/settings.json", "{}\n")
    commit_all("candidate spec")

    _stdout, stderr, status = run_validator(
      env: { "TRUSTED_BASE_SHA" => trusted },
      validate_only: false
    )

    refute status.success?
    assert_includes stderr, "stimuli[0].environment.git.ref contains untrusted repository control file(s): .github/copilot/settings.json"
  end

  def test_requires_trusted_repository_control_ref
    write_spec("environment" => { "skills" => [".."] })
    initialize_git_repo
    commit_all("candidate")

    _stdout, stderr, status = run_validator(allow_missing_trusted_control_ref: false)

    refute status.success?
    assert_includes stderr, "TRUSTED_BASE_SHA or TRUSTED_SHA is required to validate repository controls"
  end

  def test_lists_effective_executor_and_judge_models
    write_spec(
      "defaults" => {
        "model" => "gpt-5.6-sol",
        "judge_model" => "claude-opus-5"
      },
      "stimuli" => [
        {
          "name" => "effective-graders",
          "prompt" => "test",
          "graders" => [
            {
              "type" => "prompt",
              "config" => { "model" => "stimulus-prompt-judge" }
            },
            {
              "type" => "prompt",
              "config" => { "prompt" => "uses default judge" }
            },
            {
              "type" => "panel",
              "config" => {
                "models" => [
                  "panel-judge-a",
                  { "model" => "panel-judge-b", "weight" => 2 }
                ]
              }
            }
          ]
        }
      ]
    )
    initialize_git_repo
    commit_all("candidate")

    stdout, stderr, status = run_validator(list_models: true)

    assert status.success?, stderr
    assert_equal(
      %w[
        claude-opus-5
        gpt-5.6-sol
        panel-judge-a
        panel-judge-b
        stimulus-prompt-judge
      ],
      stdout.lines(chomp: true)
    )
  end

  def test_lists_models_from_legacy_config
    write_spec(
      "config" => {
        "model" => "legacy-executor",
        "judge_model" => "legacy-judge"
      },
      "stimuli" => [
        {
          "name" => "legacy",
          "graders" => [{ "type" => "prompt", "config" => { "prompt" => "judge" } }]
        }
      ]
    )
    initialize_git_repo
    commit_all("candidate")

    stdout, stderr, status = run_validator(list_models: true)

    assert status.success?, stderr
    assert_equal %w[legacy-executor legacy-judge], stdout.lines(chomp: true)
  end

  def test_model_listing_excludes_non_eval_specs
    write_spec(
      "defaults" => {
        "model" => "effective-executor",
        "judge_model" => "unused-judge"
      }
    )
    File.write(
      File.join(@tests_path, "soak.capability.vally.yaml"),
      YAML.dump("defaults" => { "model" => "phantom-executor", "judge_model" => "phantom-judge" })
    )
    initialize_git_repo
    commit_all("candidate")

    stdout, stderr, status = run_validator(list_models: true)

    assert status.success?, stderr
    assert_equal ["effective-executor"], stdout.lines(chomp: true)
  end

  def test_rejects_prompt_grader_without_effective_judge_model
    write_spec(
      "defaults" => { "model" => "executor" },
      "stimuli" => [
        {
          "name" => "implicit-fallback",
          "graders" => [{ "type" => "prompt", "config" => { "prompt" => "judge" } }]
        }
      ]
    )
    initialize_git_repo
    commit_all("candidate")

    _stdout, stderr, status = run_validator(list_models: true)

    refute status.success?
    assert_includes stderr, "must declare config.model or inherit an explicit judge_model"
  end

  def test_rejects_combined_legacy_config_and_defaults_for_model_listing
    write_spec(
      "config" => { "model" => "legacy-executor" },
      "defaults" => { "model" => "executor" }
    )
    initialize_git_repo
    commit_all("candidate")

    _stdout, stderr, status = run_validator(list_models: true)

    refute status.success?
    assert_includes stderr, "must not combine legacy config with defaults"
  end

  def test_rejects_stimulus_model_override_ignored_by_pinned_vally
    write_spec(
      "defaults" => { "model" => "executor" },
      "stimuli" => [{ "name" => "ignored-override", "model" => "phantom-executor" }]
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "uses unsupported model key(s): model"
  end

  def test_rejects_candidate_repository_control_directory_symlink
    write_spec("environment" => { "skills" => [".."] })
    initialize_git_repo
    write_repo_file(".github/copilot/settings.json", "{}\n")
    trusted = commit_all("trusted")
    FileUtils.remove_entry(File.join(@repo_root, ".github", "copilot"))
    write_repo_file("candidate-controls/settings.json", "{\"disableAllHooks\":false}\n")
    File.symlink("../../candidate-controls", File.join(@repo_root, ".github", "copilot"))
    git("add", "-f", ".github/copilot", "candidate-controls/settings.json")
    commit_all("candidate control symlink")

    _stdout, stderr, status = run_validator(env: { "TRUSTED_BASE_SHA" => trusted })

    refute status.success?
    assert_includes stderr, "candidate checkout traverses repository control symlink: .github/copilot"
  end

  def test_rejects_fixture_ref_with_repository_control_directory_symlink
    write_spec("environment" => { "skills" => [".."] })
    initialize_git_repo
    write_repo_file(".github/copilot/settings.json", "{}\n")
    trusted = commit_all("trusted")
    FileUtils.remove_entry(File.join(@repo_root, ".github", "copilot"))
    write_repo_file("candidate-controls/settings.json", "{\"disableAllHooks\":false}\n")
    File.symlink("../../candidate-controls", File.join(@repo_root, ".github", "copilot"))
    git("add", "-f", ".github/copilot", "candidate-controls/settings.json")
    untrusted_fixture = commit_all("untrusted fixture")
    FileUtils.remove_entry(File.join(@repo_root, ".github", "copilot"))
    write_repo_file(".github/copilot/settings.json", "{}\n")
    write_spec(
      "stimuli" => [
        {
          "name" => "untrusted-fixture",
          "environment" => {
            "git" => {
              "type" => "worktree",
              "source" => ".",
              "ref" => untrusted_fixture
            }
          }
        }
      ]
    )
    commit_all("candidate spec")

    _stdout, stderr, status = run_validator(
      env: { "TRUSTED_BASE_SHA" => trusted },
      validate_only: false
    )

    refute status.success?
    assert_includes stderr, "stimuli[0].environment.git.ref traverses repository control symlink: .github/copilot"
  end

  def test_rejects_symlinked_destination_component
    write_fixture("fixture.txt")
    FileUtils.mkdir_p(File.join(@repo_root, "outside"))
    File.symlink(File.join(@repo_root, "outside"), File.join(@repo_root, "escape"))
    write_spec(
      "environment" => {
        "files" => [{ "src" => "fixture.txt", "dest" => "escape/fixture.txt" }]
      }
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "traverses checkout symlink"
  end

  def test_rejects_nested_source_symlink_that_escapes_later_overlay
    fixture_dir = File.join(@tests_path, "fixture-dir")
    FileUtils.mkdir_p(fixture_dir)
    File.symlink("../../outside", File.join(fixture_dir, "escape"))
    write_fixture("payload.txt")
    write_spec(
      "environment" => {
        "files" => [
          { "src" => "fixture-dir", "dest" => "seed" },
          { "src" => "payload.txt", "dest" => "seed/escape/payload.txt" }
        ]
      }
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "contains symlink"
    assert_includes stderr, "fixture-dir/escape"
  end

  def test_rejects_skill_path_escape
    write_spec("environment" => { "skills" => ["../.."] })

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "escapes"
  end

  def test_rejects_symlinked_skills_root_that_escapes_checkout
    outside_root = Dir.mktmpdir("prepare-vally-skills-outside-")
    begin
      skills_root = File.join(@repo_root, ".github", "skills")
      FileUtils.remove_entry(skills_root)
      File.symlink(outside_root, skills_root)
      @tests_path = File.join(skills_root, "test-skill", "tests")
      FileUtils.mkdir_p(@tests_path)
      write_spec("environment" => { "skills" => [".."] })

      _stdout, stderr, status = run_validator

      refute status.success?
      assert_includes stderr, "skills root traverses checkout symlink"
    ensure
      FileUtils.remove_entry(outside_root)
    end
  end

  def test_rejects_symlinked_tests_scope_inside_skills
    actual_tests_path = File.join(@repo_root, ".github", "skills", "other-skill", "tests")
    FileUtils.mkdir_p(actual_tests_path)
    File.write(File.join(actual_tests_path, "eval.vally.yaml"), YAML.dump({}))
    FileUtils.remove_entry(@tests_path)
    File.symlink(actual_tests_path, @tests_path)

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "tests path traverses checkout symlink"
  end

  def test_mandatory_layout_rejects_symlinked_tests_scope
    actual_tests_path = File.join(@repo_root, ".github", "skills", "other-skill", "tests")
    labeler_tests_path = File.join(@repo_root, ".github", "skills", "agentic-labeler", "tests")
    FileUtils.mkdir_p(actual_tests_path)
    FileUtils.mkdir_p(File.dirname(labeler_tests_path))
    File.symlink(actual_tests_path, labeler_tests_path)

    _stdout, stderr, status = Open3.capture3(
      "ruby",
      PREPARER,
      @repo_root,
      "--validate-mandatory-layout"
    )

    refute status.success?
    assert_includes stderr, "mandatory tests path traverses checkout symlink"
  end

  def test_requires_mandatory_spec_filename
    labeler_tests_path = File.join(@repo_root, ".github", "skills", "agentic-labeler", "tests")
    FileUtils.mkdir_p(labeler_tests_path)
    File.write(File.join(labeler_tests_path, "eval.renamed.vally.yaml"), YAML.dump({}))

    _stdout, stderr, status = run_validator(labeler_tests_path)

    refute status.success?
    assert_includes stderr, "missing mandatory Vally spec .github/skills/agentic-labeler/tests/eval.vally.yaml"
  end

  def test_rejects_default_scope_git_name_write
    assert_rejects_persistent_identity('git config user.name "Vally Fixture"')
  end

  def test_rejects_local_git_email_write_with_git_directory
    assert_rejects_persistent_identity(
      'git -C "$GITHUB_WORKSPACE" config --local user.email "vally-fixture@example.invalid"'
    )
  end

  def test_rejects_chained_worktree_git_name_write
    assert_rejects_persistent_identity(
      'printf "prepare fixture" && git config --worktree user.name "Vally Fixture"'
    )
  end

  def test_rejects_multiline_git_name_write
    assert_rejects_persistent_identity(<<~SH)
      echo ok
      git config user.name "Vally Fixture"
    SH
  end

  def test_rejects_newline_separated_git_identity_write
    assert_rejects_persistent_identity(
      "printf 'prepare fixture'\r\n  git config user.email vally-fixture@example.invalid"
    )
  end

  def test_try_fix_raw_git_cleanup_matcher_boundaries
    skip "restore spec not provided" unless RESTORE_SPEC

    document = YAML.safe_load_file(RESTORE_SPEC, aliases: true)
    grader = document.fetch("stimuli").first.fetch("graders").find { |item| item["type"] == "tool-calls" }
    pattern = grader.fetch("config").fetch("disallowed").first.fetch("command")
    matcher = Regexp.new(pattern)

    disallowed_commands = [
      "git checkout -- file",
      "echo ok | git -C /tmp checkout file",
      "FOO=bar git clean -fd",
      "/usr/bin/git reset --hard",
      "sleep 1 & git reset --hard HEAD",
      "p=$(git reset --hard)",
      "(git restore file)",
      "{ git restore file; }",
      "`git clean -fdx`",
      "if true; then git reset --hard; fi",
      "sudo git stash",
      "sh -c 'git restore file'",
      "printf '%s\n' file | xargs git checkout --"
    ]
    allowed_commands = [
      "git rev-parse HEAD^",
      "git diff --quiet",
      "git status --short",
      "echo \"git reset --hard\"",
      "rg \"git restore\" file"
    ]

    assert_empty disallowed_commands.reject { |command| matcher.match?(command) }
    assert_empty allowed_commands.select { |command| matcher.match?(command) }
  end

  def test_verification_command_guard_matcher_boundaries
    skip "verification spec not provided" unless VERIFICATION_SPEC

    document = YAML.safe_load_file(VERIFICATION_SPEC, aliases: true)
    tool_graders = document.fetch("stimuli").flat_map do |stimulus|
      stimulus.fetch("graders").select { |item| item["type"] == "tool-calls" }
    end
    disallowed_git = [
      "git checkout -- file",
      "cd /repo\ngit restore --worktree .",
      "echo setup\n/usr/bin/git reset --hard",
      "git \\\nrestore --worktree .",
      "FOO=bar env git clean -fd",
      "env -C /repo git restore --worktree .",
      "FOO=bar env -C /repo git reset --hard",
      "timeout 60 git reset --hard",
      "timeout 60 bash -c \"git restore file\"",
      "pwsh -Command \"git restore --worktree .\"",
      "\"git\" restore --worktree .",
      "'/usr/bin/git' reset --hard",
      "sudo git stash",
      "sh -c 'git restore file'",
      "git apply --reverse fix.patch",
      "git apply -R fix.patch"
    ]
    allowed_git = [
      "git rev-parse HEAD^",
      "git diff --quiet",
      "git status --short"
    ]
    disallowed_dotnet = [
      "dotnet test project.csproj",
      "echo setup\ndotnet test project.csproj",
      "dotnet \\\ntest project.csproj",
      "/usr/local/bin/dotnet test project.csproj",
      "env FOO=bar dotnet --roll-forward LatestMajor test project.csproj",
      "env -C /repo dotnet test project.csproj",
      "timeout 60 dotnet test project.csproj",
      "env -C /repo bash -c \"dotnet test project.csproj\"",
      "eval \"dotnet test project.csproj\"",
      "\"dotnet\" test project.csproj",
      "bash -c 'dotnet test project.csproj'"
    ]
    allowed_dotnet = [
      "dotnet build project.csproj"
    ]

    tool_graders.each_with_index do |grader, index|
      patterns = grader.fetch("config").fetch("disallowed").map { |entry| entry.fetch("command") }
      git_patterns = patterns.select { |pattern| pattern.include?("checkout|clean|restore|reset|stash") }
      dotnet_patterns = patterns.select { |pattern| pattern.include?("dotnet") }
      refute_empty git_patterns, "grader #{index} has no Git cleanup guard"
      refute_empty dotnet_patterns, "grader #{index} has no raw dotnet test guard"
      assert_javascript_matcher_boundaries(git_patterns, disallowed_git, allowed_git)
      assert_javascript_matcher_boundaries(dotnet_patterns, disallowed_dotnet, allowed_dotnet)
    end
  end

  def test_executable_overlay_uses_trusted_ref_not_candidate_head
    scripts_root = ".github/skills/verify-tests-fail-without-fix/scripts"
    script_path = File.join(scripts_root, "verify-tests-fail.ps1")
    initialize_git_repo

    write_repo_file(script_path, "Write-Output 'base'\n")
    parent = commit_all("base")
    write_repo_file(script_path, "Write-Output 'trusted'\n")
    trusted = commit_all("trusted")
    write_repo_file(script_path, "Write-Output $env:COPILOT_GITHUB_TOKEN\n")
    commit_all("candidate")

    overlay = create_skill_overlay_commit(@repo_root, parent, trusted, scripts_root, "trusted overlay")

    assert_equal parent, git("rev-parse", "#{overlay}^")
    assert_equal "Write-Output 'trusted'\n", git("show", "#{overlay}:#{script_path}", strip: false)
    refute_includes git("show", "#{overlay}:#{script_path}", strip: false), "COPILOT_GITHUB_TOKEN"
  end

  def test_executable_overlay_removes_files_deleted_from_trusted_ref
    scripts_root = ".github/skills/verify-tests-fail-without-fix/scripts"
    retained_path = File.join(scripts_root, "verify-tests-fail.ps1")
    deleted_path = File.join(scripts_root, "deleted-helper.ps1")
    initialize_git_repo

    write_repo_file(retained_path, "Write-Output 'base'\n")
    write_repo_file(deleted_path, "Write-Output 'base helper'\n")
    parent = commit_all("base")
    FileUtils.rm(File.join(@repo_root, deleted_path))
    write_repo_file(retained_path, "Write-Output 'trusted'\n")
    trusted = commit_all("trusted deletion")
    write_repo_file(deleted_path, "Write-Output $env:COPILOT_GITHUB_TOKEN\n")
    commit_all("candidate restores deleted helper")

    overlay = create_skill_overlay_commit(@repo_root, parent, trusted, scripts_root, "trusted overlay")

    assert_equal "Write-Output 'trusted'\n", git("show", "#{overlay}:#{retained_path}", strip: false)
    assert_raises(RuntimeError) { git("show", "#{overlay}:#{deleted_path}", strip: false) }
  end

  def test_trusted_fixture_source_ref_requires_exact_resolved_sha
    initialize_git_repo
    write_repo_file("README.md", "fixture\n")
    trusted = commit_all("trusted")

    assert_equal trusted, trusted_fixture_source_ref(@repo_root, { "TRUSTED_SHA" => trusted })
    assert_raises(SystemExit) { trusted_fixture_source_ref(@repo_root, {}) }
    assert_raises(SystemExit) { trusted_fixture_source_ref(@repo_root, { "TRUSTED_SHA" => "HEAD" }) }
  end

  def test_runtime_setup_keeps_copilot_policy_root_read_only
    skip "runtime setup script not provided" unless SETUP_RUNTIME

    content = File.read(SETUP_RUNTIME)
    assert_includes content, 'sudo -n install -d -o root -g root -m 755 "$trusted_copilot_home"'
    refute_includes content, 'sudo -n install -d -o root -g root -m 1777 "$trusted_copilot_home"'
    assert_includes content, '"$trusted_copilot_home/config.json"'
    assert_includes content, '"$trusted_copilot_home/settings.json"'
    assert_includes content, 'sudo -n -u "$eval_user" /usr/bin/test -w "$protected_path"'
    refute_includes content, "\n\t--experimental \\\n"
  end

  def test_runtime_setup_limits_writable_copilot_state
    skip "runtime setup script not provided" unless SETUP_RUNTIME

    content = File.read(SETUP_RUNTIME)
    assert_includes content, 'sudo -n install -d -o "$eval_user" -g "$eval_user" -m 700'
    assert_includes content, '"$trusted_copilot_home/logs"'
    assert_includes content, '"$trusted_copilot_home/session-state"'
    assert_includes content, '"$trusted_copilot_home/session-store.db"'
    assert_includes content, '"$trusted_copilot_home/session-store.db-shm"'
    assert_includes content, '"$trusted_copilot_home/session-store.db-wal"'
    assert_includes content, 'sudo -n install -d -o root -g root -m 555'
    assert_includes content, '"$trusted_copilot_home/installed-plugins"'
    refute_includes content, '"$trusted_copilot_home/hooks"'
  end

  def test_token_selector_skips_pat_with_an_invalid_model_probe_response
    skip "token selector not provided" unless TOKEN_SELECTOR

    Dir.mktmpdir("select-vally-token-") do |root|
      fake_bin = File.join(root, "bin")
      FileUtils.mkdir_p(fake_bin)
      write_executable(
        File.join(fake_bin, "curl"),
        <<~'SH'
          #!/usr/bin/env bash
          header=$(cat)
          name="Author"
          name+="ization"
          case "$header" in
            "$name: Bearer partial-token"|"$name: Bearer complete-token")
              printf 200
              ;;
            *)
              printf 401
              ;;
          esac
        SH
      )
      write_executable(
        File.join(fake_bin, "timeout"),
        <<~'SH'
          #!/usr/bin/env bash
          shift
          exec "$@"
        SH
      )
      runner = File.join(root, "runner")
      write_executable(
        runner,
        <<~'SH'
          #!/usr/bin/env bash
          model=""
          while [ "$#" -gt 0 ]; do
            if [ "$1" = "--model" ]; then
              model=$2
              break
            fi
            shift
          done
          if [ "$COPILOT_GITHUB_TOKEN" = "partial-token" ] && [ "$model" = "claude-opus-5" ]; then
            printf WRONG_MODEL
            exit 0
          fi
          printf MODEL_OK
        SH
      )
      wrapper = File.join(root, "wrapper")
      runtime = File.join(root, "runtime")
      write_executable(wrapper, "#!/usr/bin/env bash\nexit 0\n")
      write_executable(runtime, "#!/usr/bin/env bash\nexit 0\n")
      output = File.join(root, "output")
      env = {
        "PATH" => "#{fake_bin}:#{ENV.fetch("PATH")}",
        "COPILOT_PAT_0" => "partial-token",
        "COPILOT_PAT_1" => "complete-token",
        "GITHUB_RUN_ID" => "0",
        "TOKEN_START_OFFSET" => "0"
      }

      _stdout, stderr, status = Open3.capture3(
        env,
        "bash",
        TOKEN_SELECTOR,
        output,
        runner,
        wrapper,
        runtime,
        root,
        File.join(root, "probe"),
        "gpt-5.6-sol",
        "claude-opus-5"
      )

      assert status.success?, stderr
      assert_equal "token=complete-token\n", File.read(output)
      refute_includes stderr, "partial-token"
      refute_includes stderr, "complete-token"
    end
  end

  def test_token_selector_labels_github_transport_failures
    skip "token selector not provided" unless TOKEN_SELECTOR

    content = File.read(TOKEN_SELECTOR)
    assert_match(/if ! status=\$\(.*?https:\/\/api\.github\.com\/user\s+\); then\s+status="transport-error"\s+fi/m, content)
    refute_match(/https:\/\/api\.github\.com\/user\s+\|\| true/, content)
    assert_includes content, "GitHub /user transport failed"
  end

  def test_token_selector_combines_run_attempt_and_matrix_entropy
    skip "token selector not provided" unless TOKEN_SELECTOR

    Dir.mktmpdir("select-vally-token-") do |root|
      fake_bin = File.join(root, "bin")
      FileUtils.mkdir_p(fake_bin)
      write_executable(
        File.join(fake_bin, "curl"),
        <<~'SH'
          #!/usr/bin/env bash
          cat >/dev/null
          printf 200
        SH
      )
      write_executable(
        File.join(fake_bin, "timeout"),
        <<~'SH'
          #!/usr/bin/env bash
          shift
          exec "$@"
        SH
      )
      runner = File.join(root, "runner")
      write_executable(runner, "#!/usr/bin/env bash\nprintf MODEL_OK\n")
      wrapper = File.join(root, "wrapper")
      runtime = File.join(root, "runtime")
      write_executable(wrapper, "#!/usr/bin/env bash\nexit 0\n")
      write_executable(runtime, "#!/usr/bin/env bash\nexit 0\n")
      token_env = (0..9).to_h { |index| ["COPILOT_PAT_#{index}", "token-#{index}"] }
      select = lambda do |run_id:, offset: 0, attempt: 1|
        output = File.join(root, "output-#{run_id}-#{offset}-#{attempt}")
        env = token_env.merge(
          "PATH" => "#{fake_bin}:#{ENV.fetch("PATH")}",
          "GITHUB_RUN_ID" => run_id.to_s,
          "GITHUB_RUN_ATTEMPT" => attempt.to_s,
          "TOKEN_START_OFFSET" => offset.to_s,
          "TOKEN_START_INDEX" => nil
        )
        _stdout, stderr, status = Open3.capture3(
          env,
          "bash",
          TOKEN_SELECTOR,
          output,
          runner,
          wrapper,
          runtime,
          root,
          File.join(root, "probe"),
          "gpt-5.6-sol"
        )
        assert status.success?, stderr
        File.read(output)
      end

      assert_equal "token=token-8\n", select.call(run_id: 18)
      assert_equal "token=token-9\n", select.call(run_id: 19)
      assert_equal "token=token-9\n", select.call(run_id: 18, offset: 1)
      assert_equal "token=token-9\n", select.call(run_id: 18, attempt: 2)
    end
  end

  def test_token_selector_rejects_unsafe_model_name
    skip "token selector not provided" unless TOKEN_SELECTOR

    Dir.mktmpdir("select-vally-token-") do |root|
      executable = File.join(root, "executable")
      write_executable(executable, "#!/usr/bin/env bash\nexit 0\n")
      output = File.join(root, "output")

      _stdout, stderr, status = Open3.capture3(
        { "COPILOT_PAT_0" => "token" },
        "bash",
        TOKEN_SELECTOR,
        output,
        executable,
        executable,
        executable,
        root,
        File.join(root, "probe"),
        "gpt-5.6-sol;echo-unsafe"
      )

      refute status.success?
      assert_includes stderr, "Invalid explicit model"
      refute File.exist?(output)
    end
  end

  private

  def initialize_git_repo
    git("init", "--quiet")
  end

  def write_repo_file(path, content)
    full_path = File.join(@repo_root, path)
    FileUtils.mkdir_p(File.dirname(full_path))
    File.write(full_path, content)
  end

  def write_executable(path, content)
    File.write(path, content)
    FileUtils.chmod(0o700, path)
  end

  def commit_all(message)
    git("add", ".")
    git("-c", "user.name=Vally Test", "-c", "user.email=vally-test@example.invalid", "commit", "--quiet", "-m", message)
    git("rev-parse", "HEAD")
  end

  def git(*args, strip: true)
    stdout, stderr, status = Open3.capture3("git", "-C", @repo_root, *args)
    raise "git #{args.join(' ')} failed: #{stderr}" unless status.success?

    strip ? stdout.strip : stdout
  end

  def assert_rejects_persistent_identity(command)
    write_spec(
      "stimuli" => [
        {
          "name" => "persistent-git-identity",
          "environment" => { "commands" => [command] }
        }
      ]
    )

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "persistently writes Git identity"
  end

  def assert_javascript_matcher_boundaries(patterns, disallowed, allowed)
    script = <<~'JS'
      const patterns = JSON.parse(process.argv[1]);
      const disallowed = JSON.parse(process.argv[2]);
      const allowed = JSON.parse(process.argv[3]);
      const matchers = patterns.map(pattern => new RegExp(pattern));
      const matches = command => matchers.some(matcher => matcher.test(command));
      const missed = disallowed.filter(command => !matches(command));
      const falsePositives = allowed.filter(command => matches(command));
      if (missed.length || falsePositives.length) {
        console.error(JSON.stringify({ missed, falsePositives }));
        process.exit(1);
      }
    JS
    _stdout, stderr, status = Open3.capture3(
      "node",
      "-e",
      script,
      JSON.dump(patterns),
      JSON.dump(disallowed),
      JSON.dump(allowed)
    )
    assert status.success?, stderr
  end

  def write_spec(document)
    File.write(File.join(@tests_path, "eval.vally.yaml"), YAML.dump(document))
  end

  def write_raw_spec(content)
    File.write(File.join(@tests_path, "eval.vally.yaml"), content)
  end

  def write_fixture(name)
    File.write(File.join(@tests_path, name), "fixture")
  end

  def run_validator(
    tests_path = @tests_path,
    env: {},
    validate_only: true,
    allow_missing_trusted_control_ref: true,
    list_models: false
  )
    validator_env = {
      "TRUSTED_BASE_SHA" => nil,
      "TRUSTED_SHA" => nil
    }.merge(env)
    args = [
      validator_env,
      "ruby",
      PREPARER,
      @repo_root,
      Pathname.new(tests_path).relative_path_from(Pathname.new(@repo_root)).to_s
    ]
    args << "--validate-only" if validate_only
    args << "--allow-missing-trusted-control-ref" if allow_missing_trusted_control_ref
    args << "--list-models" if list_models
    Open3.capture3(*args)
  end
end
