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

  def test_rejects_forbidden_environment_key
    write_spec("environment" => { "mcpServers" => {} })

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "uses forbidden key(s): mcpServers"
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

  def test_trusted_fixture_source_ref_requires_exact_resolved_sha
    initialize_git_repo
    write_repo_file("README.md", "fixture\n")
    trusted = commit_all("trusted")

    assert_equal trusted, trusted_fixture_source_ref(@repo_root, { "TRUSTED_SHA" => trusted })
    assert_raises(SystemExit) { trusted_fixture_source_ref(@repo_root, {}) }
    assert_raises(SystemExit) { trusted_fixture_source_ref(@repo_root, { "TRUSTED_SHA" => "HEAD" }) }
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

  def run_validator(tests_path = @tests_path)
    Open3.capture3(
      "ruby",
      PREPARER,
      @repo_root,
      Pathname.new(tests_path).relative_path_from(Pathname.new(@repo_root)).to_s,
      "--validate-only"
    )
  end
end
