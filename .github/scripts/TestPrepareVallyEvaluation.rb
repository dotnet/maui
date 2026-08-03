#!/usr/bin/env ruby

require "fileutils"
require "minitest/autorun"
require "open3"
require "tmpdir"
require "yaml"

PREPARER = File.realpath(ARGV.shift || File.join(__dir__, "PrepareVallyEvaluation.rb"))

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
