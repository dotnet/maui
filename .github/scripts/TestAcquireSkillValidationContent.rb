#!/usr/bin/env ruby

require "minitest/autorun"
require_relative "AcquireSkillValidationContent"

class TestAcquireSkillValidationContent < Minitest::Test
  Content = SkillValidationContent
  WORKFLOW = File.expand_path("../workflows/skill-validation.yml", __dir__)

  # Serve the GitHub Git-data response shape from local objects, never a network
  # endpoint. Fork commits are not fetched into the trusted fixture repository.
  class FixtureApi < Content::GitHubData
    attr_reader :requests
    attr_accessor :mutate

    def initialize(repo_root)
      super("fixture-fork/maui", "local-test-token")
      @repo_root = repo_root
      @requests = []
    end

    def json(resource)
      @requests << resource
      kind, ref = resource.split("/", 2)
      sha, query = ref.split("?", 2)
      Content.sha!(sha)
      document = case kind
                 when "commits"
                   {
                     "sha" => Content.git(@repo_root, "rev-parse", "#{sha}^{commit}"),
                     "tree" => { "sha" => Content.git(@repo_root, "rev-parse", "#{sha}^{tree}") }
                   }
                 when "trees"
                   args = ["ls-tree", "-l", "-z"]
                   args += ["-r", "-t"] if query == "recursive=1"
                   entries = Content.git(@repo_root, *args, sha, strip: false).split("\0").map do |record|
                     metadata, path = record.split("\t", 2)
                     mode, type, object, size = metadata.split
                     { "path" => path, "mode" => mode, "type" => type, "sha" => object, "size" => Integer(size, exception: false) }
                   end
                   { "sha" => sha, "truncated" => false, "tree" => entries }
                 when "blobs"
                   data = Content.git(@repo_root, "cat-file", "blob", sha, strip: false)
                   { "sha" => sha, "size" => data.bytesize, "encoding" => "base64", "content" => Base64.strict_encode64(data) }
                 else
                   raise "Unexpected Git-data request: #{resource}"
                 end
      @mutate&.call(resource, document)
      document
    end
  end

  class Response
    attr_reader :code

    def initialize(chunks, code: "200", headers: {})
      @chunks = chunks
      @code = code
      @headers = headers
    end

    def [](name)
      @headers[name]
    end

    def read_body
      @chunks.each { |chunk| yield chunk }
    end
  end

  def setup
    @root = Dir.mktmpdir("skill-validation-data-test-")
    @trusted = File.join(@root, "trusted")
    @fork = File.join(@root, "fork")
    FileUtils.mkdir_p(@trusted)
    git(@trusted, "init", "--quiet")
    write(@trusted, ".github/skills/demo/SKILL.md", "# Trusted instructions\n")
    write(@trusted, ".github/skills/demo/reference.md", "Trusted reference\n")
    write(@trusted, ".github/skills/demo/scripts/helper.ps1", "Write-Output 'trusted script'\n")
    write(@trusted, ".github/skills/demo/tests/eval.vally.yaml", "name: fixture\nstimuli: []\n")
    write(@trusted, ".github/agents/demo.agent.md", "# Trusted agent\n")
    write(@trusted, ".github/plugin.json", JSON.dump("name" => "trusted", "skills" => ["./skills/"]))
    write(@trusted, ".github/scripts/PrepareVallyEvaluation.rb", "raise 'trusted script sentinel'\n")
    write(@trusted, ".github/copilot/settings.json", "{}\n")
    @base = commit(@trusted, "trusted fixture")
    git(@trusted, "clone", "--quiet", "--no-hardlinks", "--", @trusted, @fork)
    @api = FixtureApi.new(@fork)
  end

  def teardown
    FileUtils.remove_entry(@root)
  end

  def test_normal_fork_content_stays_data_and_preserves_trusted_tools
    write(@fork, ".github/skills/demo/SKILL.md", "# Candidate instructions\n")
    write(@fork, ".github/skills/new-skill/SKILL.md", "# New skill\n")
    write(@fork, ".github/agents/new.agent.md", "# Candidate agent\n")
    write(@fork, ".github/plugin.json", JSON.dump("hooks" => { "command" => "must-not-run" }))
    write(@fork, ".github/scripts/PrepareVallyEvaluation.rb", "raise 'candidate script must never run'\n")
    write(@fork, ".npmrc", "registry=https://unused.invalid\n")
    write(@fork, "src/unrelated.rb", "raise 'unrelated code must never run'\n")
    head = commit(@fork, "candidate fixture")

    result = Content.acquire(@trusted, @api, head, @base)

    assert_equal "# Candidate instructions\n", read(@trusted, ".github/skills/demo/SKILL.md")
    assert_equal "# New skill\n", read(@trusted, ".github/skills/new-skill/SKILL.md")
    assert_equal "raise 'trusted script sentinel'\n", read(@trusted, ".github/scripts/PrepareVallyEvaluation.rb")
    assert_equal "# Trusted agent\n", read(@trusted, ".github/agents/demo.agent.md")
    refute File.exist?(File.join(@trusted, ".github/agents/new.agent.md"))
    assert_equal "trusted", JSON.parse(read(@trusted, ".github/plugin.json")).fetch("name")
    refute File.exist?(File.join(@trusted, ".npmrc"))
    refute File.exist?(File.join(@trusted, "src"))
    assert_equal @base, git(@trusted, "rev-parse", "HEAD")
    assert_equal "#{head} missing", Content.git(@trusted, "cat-file", "--batch-check", input: "#{head}\n")
    assert_equal 2, result.fetch("skill_count")
    assert_equal 2, result.fetch("agent_count")
    assert_equal "demo,new-skill", result.fetch("changed_skills")
    assert_equal true, result.fetch("harness_changed")
    refute @api.requests.any? { |request| request.include?("archive") || request.include?("refs/") }
    unwanted = git(@fork, "rev-parse", "#{head}:src/unrelated.rb")
    refute_includes @api.requests, "blobs/#{unwanted}"
    script = git(@fork, "rev-parse", "#{head}:.github/scripts/PrepareVallyEvaluation.rb")
    refute_includes @api.requests, "blobs/#{script}"
    snapshot_paths = Content.local_files(@trusted, result.fetch("tree_sha")).keys
    assert snapshot_paths.all? { |path| Content::DATA_ROOTS.any? { |root| path == root || path.start_with?("#{root}/") } }
  end

  def test_reuses_only_exact_trusted_blobs_without_remote_blob_requests
    result = Content.acquire(@trusted, @api, @base, @base)

    assert_equal "", result.fetch("changed_skills")
    assert_equal false, result.fetch("harness_changed")
    refute @api.requests.any? { |request| request.start_with?("blobs/") }
  end

  def test_head_does_not_follow_a_moved_branch
    write(@fork, ".github/skills/demo/SKILL.md", "# Gated head\n")
    head = commit(@fork, "gated head")
    write(@fork, ".github/skills/demo/SKILL.md", "# Later head\n")
    commit(@fork, "moved head")

    Content.acquire(@trusted, @api, head, @base)

    assert_equal "# Gated head\n", read(@trusted, ".github/skills/demo/SKILL.md")
    assert_equal "commits/#{head}", @api.requests.first
  end

  def test_rejects_unbound_repository_and_mutable_or_injected_refs
    ["https://github.com/fork/maui", "fork/maui/extra", "fork/..", "fork/maui?ref=x", "fork/maui\nother"].each do |repository|
      assert_raises(Content::Error) { Content::GitHubData.new(repository, "local-token") }
    end
    ["main", "HEAD", "refs/pull/1/head", "-c", "a" * 39, "#{"a" * 40}\n", "$(echo bad)"].each do |ref|
      assert_raises(Content::Error) { @api.commit_tree(ref) }
    end
    assert_empty @api.requests
  end

  def test_uses_the_bound_repository_endpoint_and_never_response_urls
    requests = []
    response = Response.new(['{"sha":"ignored","url":"https://unused.invalid"}'])
    http = Object.new
    http.define_singleton_method(:max_retries=) { |_value| }
    http.define_singleton_method(:request) { |request, &block| requests << request; block.call(response) }
    start = lambda do |host, port, **options, &block|
      assert_equal "api.github.com", host
      assert_equal 443, port
      assert_equal true, options.fetch(:use_ssl)
      block.call(http)
    end
    Net::HTTP.stub(:start, start) do
      Content::GitHubData.new("fixture-fork/maui", "local-token").json("commits/#{@base}")
    end
    assert_equal ["/repos/fixture-fork/maui/git/commits/#{@base}"], requests.map(&:path)
    assert_equal "identity", requests.first["Accept-Encoding"]
    assert_equal "Bearer local-token", requests.first["Authorization"]
  end

  def test_rejects_commit_and_tree_hash_mismatches_before_installing
    [
      ->(resource, document) { document["sha"] = "0" * 40 if resource.start_with?("commits/") },
      ->(resource, document) { document["tree"].first["sha"] = "0" * 40 if resource.start_with?("trees/") }
    ].each do |mutate|
      api = FixtureApi.new(@fork)
      api.mutate = mutate
      assert_raises(Content::Error) { Content.acquire(@trusted, api, @base, @base) }
      assert_equal "# Trusted instructions\n", read(@trusted, ".github/skills/demo/SKILL.md")
    end
  end

  def test_rejects_blob_hash_size_and_encoding_mismatches_before_installing
    write(@fork, ".github/skills/demo/SKILL.md", "# New content\n")
    head = commit(@fork, "new content")
    mutations = [
      ->(document) { document["content"] = Base64.strict_encode64("x" * document["size"]) },
      ->(document) { document["size"] += 1 },
      ->(document) { document["sha"] = "0" * 40 },
      ->(document) { document["encoding"] = "utf-8" },
      ->(document) { document["content"] = "not base64!" }
    ]
    mutations.each do |mutate|
      api = FixtureApi.new(@fork)
      api.mutate = ->(resource, document) { mutate.call(document) if resource.start_with?("blobs/") }
      assert_raises(Content::Error) { Content.acquire(@trusted, api, head, @base) }
      assert_equal "# Trusted instructions\n", read(@trusted, ".github/skills/demo/SKILL.md")
    end
  end

  def test_rejects_symlink_and_gitlink_objects_without_checking_them_out
    %w[120000 160000].each do |mode|
      object = mode == "120000" ? git(@fork, "hash-object", "-w", "--stdin", input: "../../scripts") : @base
      git(@fork, "update-index", "--add", "--cacheinfo", mode, object, ".github/skills/demo/link")
      head = commit_index(@fork, "link fixture")
      assert_raises(Content::Error) { Content.acquire(@trusted, FixtureApi.new(@fork), head, @base) }
      refute File.exist?(File.join(@trusted, ".github/skills/demo/link"))
    end
  end

  def test_rejects_repository_control_changes_and_deletions
    write(@fork, ".github/copilot/settings.json", "{\"disableAllHooks\":false}\n")
    changed = commit(@fork, "changed control")
    File.unlink(File.join(@fork, ".github/copilot/settings.json"))
    deleted = commit(@fork, "deleted control")
    [changed, deleted].each do |head|
      error = assert_raises(Content::Error) { Content.acquire(@trusted, FixtureApi.new(@fork), head, @base) }
      assert_includes error.message, "repository controls differ"
      assert_equal "{}\n", read(@trusted, ".github/copilot/settings.json")
    end
  end

  def test_rejects_repository_control_parent_symlink
    git(@fork, "update-index", "--force-remove", ".github/copilot/settings.json")
    link = git(@fork, "hash-object", "-w", "--stdin", input: "../../outside")
    git(@fork, "update-index", "--add", "--cacheinfo", "120000", link, ".github/copilot")
    head = commit_index(@fork, "control link")

    assert_raises(Content::Error) { Content.acquire(@trusted, @api, head, @base) }
    assert_equal "{}\n", read(@trusted, ".github/copilot/settings.json")
  end

  def test_rejects_auto_loaded_configuration_and_parameter_sidecars
    %w[
      .github/skills/demo/.vally.yaml
      .github/skills/demo/tests/.vally.yaml
      .github/skills/demo/tests/eval.vally.params.yaml
      .github/skills/demo/.mcp.json
      .github/skills/demo/.copilot/extensions/tool.mjs
      .github/skills/demo/.github/hooks/pre-tool.json
      .github/skills/demo/tests/fixtures/src/.github/hooks/pre-tool.json
      .github/skills/demo/tests/fixtures/src/.github/copilot/settings.json
      .github/skills/demo/plugin.json
      .github/skills/demo/node_modules/tool/index.js
    ].each do |path|
      error = assert_raises(Content::Error) { Content.validate_files!(path => entry(path)) }
      assert_includes error.message, "Auto-loaded configuration"
    end
  end

  def test_rejects_paths_that_can_escape_alias_inject_or_become_options
    %W[
      ../outside /absolute C:/outside .github\\skills\\demo\\file
      .github/skills/../file .github/skills/demo/../../file
      .github/skills/demo/.git/config .github/skills/demo/.GIT/config
      .github/skills/demo/file:stream .github/skills/demo/\u0000file
      .github/skills/demo/file\n::error::injected .github/skills/demo[other]/SKILL.md
      .github/scripts/PrepareVallyEvaluation.rb .github/skills/-option/SKILL.md
      .github/skills .github/plugin.json/child
    ].each do |path|
      assert_raises(Content::Error, path.inspect) { Content.validate_files!(path => entry(path)) }
    end
    first = ".github/skills/demo/SKILL.md"
    second = ".github/skills/demo/skill.md"
    assert_raises(Content::Error) { Content.validate_files!(first => entry(first), second => entry(second)) }
    assert_raises(Content::Error) { Content.validate_files!(first => entry(first), "#{first}/child" => entry("#{first}/child")) }
  end

  def test_rejects_truncated_duplicate_and_malformed_trees
    blob = entry("file")
    sha = Content.tree_hash([blob])
    valid = { "sha" => sha, "truncated" => false, "tree" => [blob] }
    assert_equal ["file"], Content.verify_tree!(valid, sha, recursive: true).keys
    [
      valid.merge("truncated" => true),
      valid.merge("truncated" => nil),
      valid.merge("tree" => [blob, blob]),
      valid.merge("tree" => [blob.merge("path" => "../outside")]),
      valid.merge("tree" => [blob.merge("path" => "dir/file")]),
      valid.merge("tree" => [blob.merge("mode" => "010000")]),
      valid.merge("tree" => [blob.merge("type" => "tree")]),
      valid.merge("tree" => [blob] * (Content::MAX_TREE_ENTRIES + 1))
    ].each do |document|
      assert_raises(Content::Error) { Content.verify_tree!(document, sha, recursive: true) }
    end
  end

  def test_enforces_file_count_file_size_total_size_and_path_bounds
    files = (0...Content::MAX_FILES).to_h do |index|
      path = ".github/skills/demo/tests/file-#{index}.txt"
      [path, entry(path)]
    end
    Content.validate_files!(files)
    extra = ".github/skills/demo/tests/extra.txt"
    assert_raises(Content::Error) { Content.validate_files!(files.merge(extra => entry(extra))) }
    path = ".github/skills/demo/tests/data.bin"
    Content.validate_files!(path => entry(path, size: Content::MAX_FILE_BYTES))
    [-1, nil, "10", 1.5, Content::MAX_FILE_BYTES + 1].each do |size|
      assert_raises(Content::Error) { Content.validate_files!(path => entry(path, size: size)) }
    end
    large = (0...16).to_h do |index|
      name = ".github/skills/demo/tests/large-#{index}.bin"
      [name, entry(name, size: Content::MAX_FILE_BYTES)]
    end
    Content.validate_files!(large)
    assert_raises(Content::Error) { Content.validate_files!(large.merge(extra => entry(extra))) }
    assert_raises(Content::Error) { Content.path!("a/" * 25 + "file") }
    assert_raises(Content::Error) { Content.path!("a" * 513) }
  end

  def test_rejects_oversized_content_before_blob_requests
    write(@fork, ".github/skills/demo/tests/huge.bin", "a" * (Content::MAX_FILE_BYTES + 1))
    head = commit(@fork, "oversized input")

    assert_raises(Content::Error) { Content.acquire(@trusted, @api, head, @base) }
    refute @api.requests.any? { |request| request.start_with?("blobs/") }
    assert_equal "# Trusted instructions\n", read(@trusted, ".github/skills/demo/SKILL.md")
  end

  def test_response_limits_errors_and_archives_fail_closed
    body = '{"v":"' + "a" * (Content::MAX_RESPONSE_BYTES - 8) + '"}'
    assert_equal Content::MAX_RESPONSE_BYTES, body.bytesize
    assert_equal Content::MAX_RESPONSE_BYTES - 8, Content::GitHubData.read_response(Response.new([body])).fetch("v").bytesize
    [
      Response.new([body, " "]),
      Response.new(["{}"], headers: { "content-length" => (Content::MAX_RESPONSE_BYTES + 1).to_s }),
      Response.new(["{}"], headers: { "content-length" => "3" }),
      Response.new(["{}"], headers: { "content-encoding" => "gzip" }),
      Response.new(["not-json"]),
      Response.new(["PK\x03\x04archive"]),
      Response.new(["[" * 33 + "0" + "]" * 33])
    ].each do |response|
      assert_raises(Content::Error) { Content::GitHubData.read_response(response) }
    end
    %w[301 302 307 401 403 404 429 500].each do |status|
      assert_raises(Content::Error) { Content::GitHubData.read_response(Response.new(["private error body"], code: status)) }
    end
  end

  def test_round_trips_binary_data_and_honors_deletions_without_executable_modes
    data = "\x00\x1a\xff\r\nbinary\n".b
    write(@fork, ".github/skills/demo/tests/data.bin", data)
    File.unlink(File.join(@fork, ".github/skills/demo/reference.md"))
    git(@fork, "add", ".")
    git(@fork, "update-index", "--chmod=+x", ".github/skills/demo/scripts/helper.ps1")
    head = commit_index(@fork, "binary and deleted data")

    result = Content.acquire(@trusted, @api, head, @base)
    assert_equal data, read(@trusted, ".github/skills/demo/tests/data.bin")
    refute File.exist?(File.join(@trusted, ".github/skills/demo/reference.md"))
    assert_equal 0, File.stat(File.join(@trusted, ".github/skills/demo/scripts/helper.ps1")).mode & 0o111
    write(@trusted, ".github/skills/demo/tests/data.bin", "changed")
    Content.restore(@trusted, result.fetch("tree_sha"))
    assert_equal data, read(@trusted, ".github/skills/demo/tests/data.bin")
    assert_equal @base, git(@trusted, "rev-parse", "HEAD")
  end

  def test_baseline_and_restore_keep_candidate_specs_and_handle_added_deleted_markdown
    write(@fork, ".github/skills/demo/SKILL.md", "# Candidate\n")
    write(@fork, ".github/skills/demo/new.md", "New reference\n")
    File.unlink(File.join(@fork, ".github/skills/demo/reference.md"))
    head = commit(@fork, "candidate instructions")
    result = Content.acquire(@trusted, @api, head, @base)
    tree = result.fetch("tree_sha")
    write(@trusted, ".github/skills/demo/tests/eval.vally.yaml", "prepared fixture spec\n")

    assert Content.baseline(@trusted, tree, @base, ".github/skills/demo/tests")
    assert_equal "# Trusted instructions\n", read(@trusted, ".github/skills/demo/SKILL.md")
    assert_equal "Trusted reference\n", read(@trusted, ".github/skills/demo/reference.md")
    refute File.exist?(File.join(@trusted, ".github/skills/demo/new.md"))
    assert_equal "prepared fixture spec\n", read(@trusted, ".github/skills/demo/tests/eval.vally.yaml")

    Content.restore(@trusted, tree)
    assert_equal "# Candidate\n", read(@trusted, ".github/skills/demo/SKILL.md")
    assert_equal "New reference\n", read(@trusted, ".github/skills/demo/new.md")
    refute File.exist?(File.join(@trusted, ".github/skills/demo/reference.md"))
    assert_equal "name: fixture\nstimuli: []\n", read(@trusted, ".github/skills/demo/tests/eval.vally.yaml")
  end

  def test_baseline_skips_unchanged_new_and_mixed_script_changes
    unchanged = Content.acquire(@trusted, @api, @base, @base).fetch("tree_sha")
    refute Content.baseline(@trusted, unchanged, @base, ".github/skills/demo/tests")
    write(@fork, ".github/skills/demo/SKILL.md", "# Candidate\n")
    write(@fork, ".github/skills/demo/scripts/helper.ps1", "Write-Output 'changed script'\n")
    write(@fork, ".github/skills/new-skill/SKILL.md", "# New skill\n")
    head = commit(@fork, "mixed candidate")
    tree = Content.acquire(@trusted, FixtureApi.new(@fork), head, @base).fetch("tree_sha")
    refute Content.baseline(@trusted, tree, @base, ".github/skills/demo/tests")
    refute Content.baseline(@trusted, tree, @base, ".github/skills/new-skill/tests")
    assert_equal "# Candidate\n", read(@trusted, ".github/skills/demo/SKILL.md")
  end

  def test_restore_rejects_an_unfiltered_commit_or_tree
    assert_raises(Content::Error) { Content.restore(@trusted, @base) }
    tree = git(@trusted, "rev-parse", "#{@base}^{tree}")
    assert_raises(Content::Error) { Content.restore(@trusted, tree) }
    assert_equal "# Trusted instructions\n", read(@trusted, ".github/skills/demo/SKILL.md")
  end

  def test_existing_hardlinks_are_not_overwritten_or_removed
    sentinel = File.join(@root, "sentinel")
    File.binwrite(sentinel, "protected")
    File.link(sentinel, File.join(@trusted, ".github/skills/demo/hardlink"))

    assert_raises(Content::Error) { Content.acquire(@trusted, @api, @base, @base) }
    assert_equal "protected", File.binread(sentinel)
    assert File.exist?(File.join(@trusted, ".github/skills/demo/hardlink"))
  end

  def test_cli_acquires_the_gated_head_without_refetching_available_trusted_history
    write(@trusted, ".github/scripts/PrepareVallyEvaluation.rb", "raise 'newer trusted workflow sentinel'\n")
    workflow_sha = commit(@trusted, "newer workflow revision")
    write(@fork, ".github/skills/demo/SKILL.md", "# Gated candidate\n")
    head = commit(@fork, "gated candidate")
    output = File.join(@root, "github-output")
    env = {
      "TRUSTED_REPOSITORY" => "trusted/maui", "TRUSTED_SHA" => workflow_sha,
      "TRUSTED_BASE_SHA" => @base, "HEAD_REPOSITORY" => "fixture-fork/maui",
      "HEAD_SHA" => head, "GH_TOKEN" => "local-token", "GITHUB_OUTPUT" => output,
      "POLICY_DIRECTORY" => File.join(@root, "policy")
    }
    original_git = Content.method(:git)
    git_without_fetch = lambda do |*args, **options|
      refute_includes args, "fetch"
      original_git.call(*args, **options)
    end
    api_factory = lambda do |repository, token|
      assert_equal "fixture-fork/maui", repository
      assert_equal "local-token", token
      @api
    end
    capture_io do
      Content.stub(:git, git_without_fetch) do
        Content::GitHubData.stub(:new, api_factory) do
          Content.main(["acquire", @trusted], env)
        end
      end
    end

    fields = File.readlines(output, chomp: true).to_h { |line| line.split("=", 2) }
    assert_equal "demo", fields.fetch("changed_skills")
    assert_equal "1", fields.fetch("skill_count")
    assert_equal "# Gated candidate\n", read(@trusted, ".github/skills/demo/SKILL.md")
    assert_equal workflow_sha, git(@trusted, "rev-parse", "HEAD")
    refute File.exist?(File.join(@trusted, ".git/shallow"))
  end

  def test_cli_rejects_an_untrusted_checkout_before_requesting_content
    assert_raises(Content::Error) { Content.main(["acquire"]) }
    assert_raises(Content::Error) { Content.main(["acquire", @trusted], "TRUSTED_SHA" => "a" * 40) }
    assert_empty @api.requests
  end

  def test_workflow_checkouts_and_controls_remain_trusted
    workflow = YAML.load_file(WORKFLOW)
    jobs = workflow.fetch("jobs")
    %w[static-check discover-eval evaluate hermeticity-gate].each do |name|
      job = jobs.fetch(name)
      assert_equal({ "contents" => "read" }, job.fetch("permissions"))
      checkout = job.fetch("steps").find { |step| step["uses"] == "actions/checkout@v4" }
      assert_equal "${{ github.repository }}", checkout.fetch("with").fetch("repository")
      assert_equal "${{ github.workflow_sha }}", checkout.fetch("with").fetch("ref")
      assert_equal false, checkout.fetch("with").fetch("persist-credentials")
      refute checkout.fetch("with").key?("allow-unsafe-pr-checkout")
      acquisition = job.fetch("steps").find { |step| step["id"] == "content" }
      assert_equal "ruby .github/scripts/AcquireSkillValidationContent.rb acquire \"$GITHUB_WORKSPACE\"", acquisition.fetch("run")
      assert_equal "${{ github.token }}", acquisition.fetch("env").fetch("GH_TOKEN")
      assert_equal "${{ github.workflow_sha }}", acquisition.fetch("env").fetch("TRUSTED_SHA")
      assert_equal "${{ runner.temp }}/skill-validation-policy", acquisition.fetch("env").fetch("POLICY_DIRECTORY")
    end
    assert_includes jobs.fetch("discover-eval").fetch("if"), "is_contributor == 'true'"
    assert_includes jobs.fetch("slash-gate").fetch("steps").first.fetch("run"), '"$PERMISSION" != "write"'
    baseline = jobs.fetch("evaluate").fetch("steps").find { |step| step["id"] == "eval-baseline" }
    assert_includes baseline.fetch("if"), "contains(github.event.comment.body, '--baseline')"
    assert_includes baseline.fetch("run"), 'trap restore EXIT'
    refute_match(/git .*?(?:checkout|restore|reset)/, baseline.fetch("run"))
    assert_includes jobs.fetch("evaluate").fetch("steps").find { |step| step["name"] == "Ensure fixture history is available" }.fetch("run"),
      "--list-fixture-refs"
  end

  def test_policy_validation_uses_candidate_bytes_without_replacing_trusted_workflows
    Content::POLICY_PATHS.each { |path| write(@trusted, path, "trusted policy\n") }
    trusted = commit(@trusted, "trusted policies")
    Content::POLICY_PATHS.each { |path| write(@fork, path, "invalid candidate policy\n") }
    head = commit(@fork, "candidate policies")
    destination = File.join(@root, "policy")

    result = Content.acquire(@trusted, @api, head, trusted, policy_directory: destination)

    Content::POLICY_PATHS.each do |path|
      assert_equal "invalid candidate policy\n", read(destination, path)
      assert_equal "trusted policy\n", read(@trusted, path)
    end
    assert result.fetch("harness_changed")
    assert_equal trusted, git(@trusted, "rev-parse", "HEAD")
    assert_raises(Content::Error) { Content.acquire(@trusted, @api, head, trusted, policy_directory: destination) }
    assert_raises(Content::Error) { Content.acquire(@trusted, @api, head, trusted, policy_directory: File.join(@trusted, "policy")) }
  end

  def test_missing_candidate_policy_does_not_fall_back_to_trusted_files
    Content::POLICY_PATHS.each { |path| write(@trusted, path, "trusted policy\n") }
    trusted = commit(@trusted, "trusted policies")
    destination = File.join(@root, "policy")

    Content.acquire(@trusted, @api, @base, trusted, policy_directory: destination)

    Content::POLICY_PATHS.each do |path|
      refute File.exist?(File.join(destination, path))
      assert_equal "trusted policy\n", read(@trusted, path)
    end
  end

  private

  def entry(path, size: 1)
    { "path" => path, "mode" => "100644", "type" => "blob", "sha" => "a" * 40, "size" => size }
  end

  def git(repo, *args, input: nil)
    Content.git(
      repo, "-c", "core.autocrlf=false", "-c", "core.hooksPath=#{@root}/no-hooks",
      "-c", "commit.gpgsign=false", *args, input: input
    )
  end

  def commit(repo, message)
    git(repo, "add", ".")
    commit_index(repo, message)
  end

  def commit_index(repo, message)
    git(repo, "-c", "user.name=Vally Test", "-c", "user.email=vally-test@example.invalid", "commit", "--quiet", "-m", message)
    git(repo, "rev-parse", "HEAD")
  end

  def write(repo, path, content)
    full_path = File.join(repo, path)
    FileUtils.mkdir_p(File.dirname(full_path))
    File.binwrite(full_path, content)
  end

  def read(repo, path)
    File.binread(File.join(repo, path))
  end
end
