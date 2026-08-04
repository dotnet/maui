#!/usr/bin/env ruby

require "fileutils"
require "minitest/autorun"
require "open3"
require "tmpdir"
require "yaml"

PREPARER = File.realpath(ARGV.shift || File.join(__dir__, "PrepareVallyEvaluation.rb"))
RESTORE_SPEC = ARGV.empty? ? nil : File.realpath(ARGV.shift)

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

  def test_rejects_skill_path_escape
    write_spec("environment" => { "skills" => ["../.."] })

    _stdout, stderr, status = run_validator

    refute status.success?
    assert_includes stderr, "escapes"
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

  private

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

  def write_spec(document)
    File.write(File.join(@tests_path, "eval.vally.yaml"), YAML.dump(document))
  end

  def write_raw_spec(content)
    File.write(File.join(@tests_path, "eval.vally.yaml"), content)
  end

  def write_fixture(name)
    File.write(File.join(@tests_path, name), "fixture")
  end

  def run_validator
    Open3.capture3(
      "ruby",
      PREPARER,
      @repo_root,
      ".github/skills/test-skill/tests",
      "--validate-only"
    )
  end
end
