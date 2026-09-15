#!/usr/bin/env ruby

require "json"
require "minitest/autorun"

SKILL_VALIDATION_WORKFLOW = File.realpath(ARGV.fetch(0))
SKILL_EVALUATION_SOAK_WORKFLOW = File.realpath(ARGV.fetch(1))
ROLLBACK_FIXTURE = File.realpath(ARGV.fetch(2))

class TestSkillWorkflowModes < Minitest::Test
  def setup
    @validation = File.read(SKILL_VALIDATION_WORKFLOW)
    @soak = File.read(SKILL_EVALUATION_SOAK_WORKFLOW)
    @rollback = JSON.parse(File.read(ROLLBACK_FIXTURE))
  end

  def test_manual_execution_accepts_current_default_branch
    assert manual_source_valid?(
      event_name: "workflow_dispatch",
      ref: "refs/heads/main",
      ref_type: "branch",
      default_branch: "main",
      workflow_sha: "abc123",
      default_sha: "abc123"
    )
  end

  def test_manual_execution_rejects_non_default_branch
    refute manual_source_valid?(
      event_name: "workflow_dispatch",
      ref: "refs/heads/topic",
      ref_type: "branch",
      default_branch: "main",
      workflow_sha: "abc123",
      default_sha: "abc123"
    )
  end

  def test_manual_execution_rejects_tag
    refute manual_source_valid?(
      event_name: "workflow_dispatch",
      ref: "refs/tags/v1",
      ref_type: "tag",
      default_branch: "main",
      workflow_sha: "abc123",
      default_sha: "abc123"
    )
  end

  def test_manual_execution_rejects_missing_or_ambiguous_values
    base = {
      event_name: "workflow_dispatch",
      ref: "refs/heads/main",
      ref_type: "branch",
      default_branch: "main",
      workflow_sha: "abc123",
      default_sha: "abc123"
    }

    %i[ref ref_type default_branch workflow_sha default_sha].each do |key|
      refute manual_source_valid?(**base.merge(key => "")), key
    end
    refute manual_source_valid?(**base.merge(ref_type: "branch,tag"))
    refute manual_source_valid?(**base.merge(ref: "main"))
  end

  def test_manual_execution_rejects_workflow_sha_mismatch
    refute manual_source_valid?(
      event_name: "workflow_dispatch",
      ref: "refs/heads/main",
      ref_type: "branch",
      default_branch: "main",
      workflow_sha: "older",
      default_sha: "current"
    )
  end

  def test_both_workflows_apply_the_manual_source_gate
    [@validation, @soak].each do |workflow|
      assert_includes workflow, "EVENT_REF_TYPE: ${{ github.ref_type }}"
      assert_includes workflow, "WORKFLOW_SHA: ${{ github.workflow_sha }}"
      assert_includes workflow, "github.rest.repos.getCommit"
      assert_includes workflow, "eventRefType === 'branch'"
      assert_includes workflow, "eventRef === expectedRef"
      assert_includes workflow, "workflowSha === trustedDefaultSha"
      assert_includes workflow, "needs.execution-mode.outputs.manual_source_valid == 'true'"
    end
  end

  def test_live_jobs_use_the_dedicated_environment
    [@validation, @soak].each do |workflow|
      refute workflow.lines.any? { |line| line.match?(/^\s+environment:\s+copilot-pat-pool\s*$/) }
    end
    assert_equal 2, @validation.lines.count { |line| line.match?(/^\s+environment:\s+skill-evaluation-pat-pool\s*$/) }
    assert_equal 1, @soak.lines.count { |line| line.match?(/^\s+environment:\s+skill-evaluation-pat-pool\s*$/) }
  end

  def test_static_only_mode_gates_every_live_job
    [
      job_block(@validation, "evaluate"),
      job_block(@validation, "hermeticity-gate"),
      job_block(@soak, "code-review")
    ].each do |job|
      assert_includes job, "needs.execution-mode.outputs.static_only != 'true'"
      assert_includes job, "needs.execution-mode.outputs.manual_source_valid == 'true'"
      assert_includes job, "github.ref_type == 'branch'"
      assert_includes job, "github.workflow_sha == needs.execution-mode.outputs.trusted_default_sha"
    end
  end

  def test_static_only_success_uses_exact_status_and_check_label
    outcome = final_outcome(
      static_only: true,
      static_result: "success",
      discover_result: "success",
      has_entries: true,
      evaluation_result: "skipped",
      evaluation_passed: "na"
    )

    assert_equal "success", outcome.fetch(:conclusion)
    assert_equal "passed (static only)", outcome.fetch(:label)
    assert_operator @validation.scan("passed (static only)").length, :>=, 2
    assert_includes @soak, "title = 'passed (static only)'"
    assert_includes @soak, "description: title"
    assert_includes @soak, "output: {\n                title,"
  end

  def test_static_failure_remains_failed
    outcome = final_outcome(
      static_only: true,
      static_result: "failure",
      discover_result: "success",
      has_entries: true,
      evaluation_result: "skipped",
      evaluation_passed: "na"
    )

    assert_equal "failure", outcome.fetch(:conclusion)
    assert_equal "Static validation failed", outcome.fetch(:label)
  end

  def test_normal_mode_requires_evaluation_for_evaluatable_changes
    graph = live_job_graph(static_only: false, manual_source_valid: true, static_result: "success", has_entries: true)

    assert graph.fetch(:evaluate)
    assert graph.fetch(:hermeticity)
    assert graph.fetch(:soak)
  end

  def test_unexpected_evaluation_skip_is_failed
    outcome = final_outcome(
      static_only: false,
      static_result: "success",
      discover_result: "success",
      has_entries: true,
      evaluation_result: "skipped",
      evaluation_passed: "na"
    )

    assert_equal "failure", outcome.fetch(:conclusion)
    assert_equal "Evaluation unexpectedly skipped", outcome.fetch(:label)
    assert_includes @validation, 'DESC="Evaluation unexpectedly skipped"'
    assert_includes @validation, "title = 'Evaluation unexpectedly skipped'"
    assert_includes @soak, "title = 'Live evaluation unexpectedly skipped'"
  end

  def test_rollback_drill
    skills = discover_evaluatable_skills(@rollback.fetch("changed_files"))
    assert_equal [@rollback.fetch("skill")], skills

    static_graph = live_job_graph(
      static_only: true,
      manual_source_valid: true,
      static_result: @rollback.fetch("static_result"),
      has_entries: !skills.empty?
    )
    refute static_graph.fetch(:evaluate)
    refute static_graph.fetch(:hermeticity)
    refute static_graph.fetch(:soak)
    assert_equal(
      { conclusion: "success", label: "passed (static only)" },
      final_outcome(
        static_only: true,
        static_result: @rollback.fetch("static_result"),
        discover_result: "success",
        has_entries: !skills.empty?,
        evaluation_result: "skipped",
        evaluation_passed: "na"
      )
    )

    normal_graph = live_job_graph(
      static_only: false,
      manual_source_valid: true,
      static_result: @rollback.fetch("static_result"),
      has_entries: !skills.empty?
    )
    assert normal_graph.values.all?

    failed = final_outcome(
      static_only: true,
      static_result: @rollback.fetch("failing_static_result"),
      discover_result: "success",
      has_entries: !skills.empty?,
      evaluation_result: "skipped",
      evaluation_passed: "na"
    )
    assert_equal "failure", failed.fetch(:conclusion)
  end

  private

  def manual_source_valid?(event_name:, ref:, ref_type:, default_branch:, workflow_sha:, default_sha:)
    return true unless event_name == "workflow_dispatch"

    values = [ref, ref_type, default_branch, workflow_sha, default_sha]
    values.all? { |value| value.is_a?(String) && !value.empty? } &&
      ref_type == "branch" &&
      ref == "refs/heads/#{default_branch}" &&
      workflow_sha == default_sha
  end

  def live_job_graph(static_only:, manual_source_valid:, static_result:, has_entries:)
    base = !static_only && manual_source_valid && static_result == "success"
    {
      evaluate: base && has_entries,
      hermeticity: base && has_entries,
      soak: base
    }
  end

  def final_outcome(static_only:, static_result:, discover_result:, has_entries:, evaluation_result:, evaluation_passed:)
    return { conclusion: "failure", label: "Static validation failed" } unless static_result == "success"
    return { conclusion: "success", label: "passed (static only)" } if static_only
    return { conclusion: "failure", label: "Evaluation discovery failed" } unless discover_result == "success"
    return { conclusion: "success", label: "Skill validation passed (static only)" } unless has_entries
    return { conclusion: "success", label: "Skill validation passed" } if evaluation_passed == "true"
    return { conclusion: "failure", label: "LLM evaluation failed" } if evaluation_passed == "false"
    return { conclusion: "failure", label: "Evaluation unexpectedly skipped" } if evaluation_result == "skipped"

    { conclusion: "failure", label: "Evaluation incomplete" }
  end

  def discover_evaluatable_skills(changed_files)
    changed_files.filter_map do |path|
      match = path.match(%r{\A\.github/skills/([^/]+)/tests/eval[^/]*\.vally\.yaml\z})
      match && match[1]
    end.uniq.sort
  end

  def job_block(workflow, name)
    match = workflow.match(/^  #{Regexp.escape(name)}:\n.*?(?=^  [a-zA-Z0-9_-]+:\n|\z)/m)
    refute_nil match, "missing job #{name}"
    match[0]
  end
end
