#!/usr/bin/env ruby

require "base64"
require "digest"
require "fileutils"
require "json"
require "net/http"
require "timeout"
require_relative "PrepareVallyEvaluation"

module SkillValidationContent
  MAX_FILES = 1024
  MAX_FILE_BYTES = 2 * 1024 * 1024
  MAX_TOTAL_BYTES = 32 * 1024 * 1024
  MAX_TREE_ENTRIES = 4096
  MAX_RESPONSE_BYTES = 4 * 1024 * 1024
  SKILLS_ROOT = ".github/skills"
  POLICY_PATHS = %w[
    .github/workflows/skill-validation.yml
    .github/workflows/skill-evaluation-soak.yml
    .github/scripts/fixtures/skill-workflow-policy/rollback-drill.json
  ].freeze
  DATA_ROOTS = [SKILLS_ROOT, ".github/agents", ".github/plugin.json", *POLICY_PATHS].freeze
  HARNESS_PATHS = %w[
    .github/workflows/skill-validation.yml
    .github/scripts/AcquireSkillValidationContent.rb
    .github/scripts/TestAcquireSkillValidationContent.rb
    .github/scripts/PrepareVallyEvaluation.rb
    .github/scripts/TestPrepareVallyEvaluation.rb
    .github/scripts/TestSkillWorkflowModes.rb
    .github/scripts/SelectVallyToken.sh
    .github/scripts/SetupVallyRuntime.sh
    .github/workflows/skill-evaluation-soak.yml
    .github/scripts/fixtures/skill-workflow-policy/rollback-drill.json
  ].freeze
  GIT_MODES = {
    "040000" => "tree",
    "100644" => "blob",
    "100755" => "blob",
    "120000" => "blob",
    "160000" => "commit"
  }.freeze

  class Error < StandardError; end

  def self.sha!(value)
    raise Error, "Expected an immutable full Git SHA" unless value.is_a?(String) && value.match?(/\A[0-9a-f]{40}\z/)

    value
  end

  def self.repository!(value)
    unless value.is_a?(String) && value.match?(%r{\A[A-Za-z0-9][A-Za-z0-9-]{0,38}/[A-Za-z0-9_.-]{1,100}\z}) &&
        !%w[. ..].include?(value.split("/").last)
      raise Error, "Expected a GitHub owner/repository, not a URL or ref"
    end

    value
  end

  def self.path!(value)
    unless value.is_a?(String) && value.bytesize <= 512 && value.match?(%r{\A[A-Za-z0-9_.-]+(?:/[A-Za-z0-9_.-]+)*\z})
      raise Error, "Invalid data path: #{value.inspect}"
    end
    parts = value.split("/")
    if parts.length > 24 || parts.any? { |part| part.bytesize > 255 || %w[. .. .git .hg .svn].include?(part.downcase) }
      raise Error, "Unsafe data path: #{value.inspect}"
    end

    value
  end

  def self.skill_name!(value)
    unless value.is_a?(String) && value.match?(/\A[A-Za-z0-9][A-Za-z0-9_-]{0,63}\z/)
      raise Error, "Invalid skill name: #{value.inspect}"
    end

    value
  end

  def self.git(repo_root, *args, **options)
    run_git(repo_root, "--no-replace-objects", *args, binary: true, **options)
  end

  def self.tree_hash(entries)
    body = entries.sort_by { |entry| "#{entry.fetch("path")}#{entry["type"] == "tree" ? "/" : ""}".b }.map do |entry|
      "#{entry.fetch("mode").sub(/\A0+/, "")} #{entry.fetch("path")}\0".b +
        [entry.fetch("sha")].pack("H*")
    end.join.b
    Digest::SHA1.hexdigest("tree #{body.bytesize}\0".b + body)
  end

  def self.verify_tree!(document, sha, recursive:)
    unless document.is_a?(Hash) && document["sha"] == sha && document["truncated"] == false &&
        document["tree"].is_a?(Array) && document["tree"].length <= MAX_TREE_ENTRIES
      raise Error, "Missing, truncated, oversized, or mismatched Git tree"
    end

    entries = {}
    folded_paths = {}
    document.fetch("tree").each do |entry|
      raise Error, "Malformed Git tree entry" unless entry.is_a?(Hash)

      path = path!(entry["path"])
      raise Error, "Unexpected recursive tree entry" if !recursive && path.include?("/")
      raise Error, "Duplicate or case-aliased data path: #{path}" if folded_paths.key?(path.downcase)
      raise Error, "Invalid Git tree mode/type: #{path}" unless GIT_MODES[entry["mode"]] == entry["type"]

      sha!(entry["sha"])
      entries[path] = entry
      folded_paths[path.downcase] = true
    end
    children = Hash.new { |hash, key| hash[key] = [] }
    entries.each do |path, entry|
      parent = File.dirname(path)
      if parent != "." && entries.dig(parent, "type") != "tree"
        raise Error, "Missing or non-directory parent: #{path}"
      end
      children[parent] << entry.merge("path" => File.basename(path))
    end
    expected_trees = { "." => sha }
    if recursive
      entries.each { |path, entry| expected_trees[path] = entry.fetch("sha") if entry["type"] == "tree" }
    end
    expected_trees.each do |path, expected|
      raise Error, "Git tree object hash mismatch: #{path}" unless tree_hash(children[path]) == expected
    end
    entries
  end

  class GitHubData
    def initialize(repository, token)
      @repository = SkillValidationContent.repository!(repository)
      raise Error, "A read-only GitHub token is required" if token.to_s.empty?

      @token = token
      @trees = {}
    end

    def json(resource)
      uri = URI("https://api.github.com/repos/#{@repository}/git/#{resource}")
      request = Net::HTTP::Get.new(uri)
      request["Authorization"] = "Bearer #{@token}"
      request["Accept"] = "application/vnd.github+json"
      request["Accept-Encoding"] = "identity"
      request["X-GitHub-Api-Version"] = "2022-11-28"
      request["User-Agent"] = "maui-skill-validation"
      Net::HTTP.start(uri.host, uri.port, use_ssl: true, open_timeout: 10, read_timeout: 30) do |http|
        http.max_retries = 0
        http.request(request) { |response| return self.class.read_response(response) }
      end
    end

    def self.read_response(response)
      raise Error, "GitHub data request failed (HTTP #{response.code}); redirects are not followed" unless response.code == "200"
      unless [nil, "identity"].include?(response["content-encoding"])
        raise Error, "Compressed GitHub data responses are not accepted"
      end
      length = response["content-length"]
      if length && (!length.match?(/\A[0-9]+\z/) || length.to_i > MAX_RESPONSE_BYTES)
        raise Error, "Oversized GitHub data response"
      end
      body = +"".b
      response.read_body do |chunk|
        raise Error, "Oversized GitHub data response" if body.bytesize + chunk.bytesize > MAX_RESPONSE_BYTES

        body << chunk
      end
      raise Error, "Truncated GitHub data response" if length && body.bytesize != length.to_i

      JSON.parse(body, max_nesting: 32)
    rescue JSON::ParserError
      raise Error, "Malformed GitHub data JSON"
    end

    def commit_tree(sha)
      document = json("commits/#{SkillValidationContent.sha!(sha)}")
      unless document.is_a?(Hash) && document["sha"] == sha && document["tree"].is_a?(Hash)
        raise Error, "GitHub commit does not match the gated head SHA"
      end

      SkillValidationContent.sha!(document.dig("tree", "sha"))
    end

    def tree(sha, recursive: false)
      key = [SkillValidationContent.sha!(sha), recursive]
      @trees[key] ||= SkillValidationContent.verify_tree!(
        json("trees/#{sha}#{recursive ? "?recursive=1" : ""}"),
        sha,
        recursive: recursive
      )
    end

    def entry(root_tree, path)
      parts = SkillValidationContent.path!(path).split("/")
      current = root_tree
      parts.each_with_index do |part, index|
        entry = tree(current)[part]
        return nil unless entry
        return entry if index == parts.length - 1

        raise Error, "Data lookup traverses a link or non-directory: #{path}" unless entry["mode"] == "040000"

        current = entry.fetch("sha")
      end
    end

    def files(root_tree, path)
      entry = entry(root_tree, path)
      return {} unless entry
      return { path => entry.merge("path" => path) } unless entry["type"] == "tree"

      tree(entry.fetch("sha"), recursive: true).filter_map do |relative, child|
        next if child["type"] == "tree"

        full_path = "#{path}/#{relative}"
        [full_path, child.merge("path" => full_path)]
      end.to_h
    end

    def blob(entry)
      document = json("blobs/#{entry.fetch("sha")}")
      unless document.is_a?(Hash) && document["sha"] == entry["sha"] &&
          document["encoding"] == "base64" && document["size"] == entry["size"] && document["content"].is_a?(String)
        raise Error, "Malformed or mismatched Git blob"
      end
      begin
        content = Base64.strict_decode64(document.fetch("content").delete("\n"))
      rescue ArgumentError
        raise Error, "Malformed Git blob base64"
      end
      SkillValidationContent.verify_blob!(entry, content)
    end
  end

  def self.verify_blob!(entry, content)
    unless content.bytesize == entry.fetch("size") && content.bytesize <= MAX_FILE_BYTES &&
        Digest::SHA1.hexdigest("blob #{content.bytesize}\0".b + content.b) == entry.fetch("sha")
      raise Error, "Git blob size or object hash mismatch: #{entry.fetch("path")}"
    end

    content
  end

  def self.validate_files!(files)
    raise Error, "Too many data files" if files.length > MAX_FILES

    total = 0
    folded_paths = {}
    files.each do |path, entry|
      path!(path)
      unless path == ".github/plugin.json" || POLICY_PATHS.include?(path) ||
          DATA_ROOTS.first(2).any? { |root| path.start_with?("#{root}/") }
        raise Error, "File is outside the data allowlist: #{path}"
      end
      raise Error, "Duplicate or case-aliased data path: #{path}" if folded_paths.key?(path.downcase)
      raise Error, "Only regular Git blobs are accepted: #{path}" unless %w[100644 100755].include?(entry["mode"]) && entry["type"] == "blob"

      sha!(entry["sha"])
      size = entry["size"]
      raise Error, "Invalid or oversized data file: #{path}" unless size.is_a?(Integer) && size.between?(0, MAX_FILE_BYTES)

      total += size
      folded_paths[path.downcase] = true
      if path.start_with?("#{SKILLS_ROOT}/")
        parts = path.delete_prefix("#{SKILLS_ROOT}/").split("/")
        skill_name!(parts.first)
        raise Error, "A skill must be a directory: #{path}" if parts.length < 2

        # Vally walks upward for project config and auto-loads adjacent params.
        # Skill injection must not introduce repository/CLI configuration either.
        if parts.any? { |part| part.start_with?(".") && part != ".github" && part != ".gitattributes" } ||
            parts.any? { |part| %w[node_modules plugin.json package.json mcp.json].include?(part.downcase) } ||
            path.match?(/\.params\.ya?ml\z/i) ||
            REPOSITORY_CONTROL_PATHS.any? do |control|
              control_parts = control.split("/")
              parts.drop(1).map(&:downcase).each_cons(control_parts.length).include?(control_parts)
            end
          raise Error, "Auto-loaded configuration is not skill data: #{path}"
        end
      end
    end
    raise Error, "Data exceeds the total byte limit" if total > MAX_TOTAL_BYTES

    files.each_key do |path|
      parts = path.split("/")
      (1...parts.length).each do |length|
        raise Error, "A data file is also a parent directory: #{path}" if folded_paths.key?(parts.first(length).join("/").downcase)
      end
    end
  end

  def self.local_files(repo_root, ref, *paths)
    sha!(ref)
    output = git(repo_root, "ls-tree", "-r", "-l", "-z", ref, "--", *paths, strip: false)
    output.split("\0").to_h do |record|
      metadata, path = record.split("\t", 2)
      mode, type, sha, size = metadata.split
      [path, { "path" => path, "mode" => mode, "type" => type, "sha" => sha, "size" => Integer(size, exception: false) }]
    end
  end

  def self.validate_controls!(repo_root, api, root_tree, trusted_ref)
    expected = repository_control_entries(repo_root, trusted_ref)
    actual = REPOSITORY_CONTROL_PATHS.each_with_object({}) do |path, entries|
      api.files(root_tree, path).each do |name, entry|
        unless entry["type"] == "blob" && %w[100644 100755].include?(entry["mode"])
          raise Error, "Untrusted repository control link: #{name}"
        end
        entries[name] = "#{entry.fetch("mode")} #{entry.fetch("type")} #{entry.fetch("sha")}"
      end
    end
    raise Error, "Candidate repository controls differ from the trusted base" unless actual == expected
  end

  def self.safe_destination!(repo_root, path)
    path!(path)
    current = repo_root
    path.split("/").each do |part|
      current = File.join(current, part)
      next unless File.exist?(current) || File.symlink?(current)

      stat = File.lstat(current)
      if stat.symlink? || (!stat.directory? && (!stat.file? || stat.nlink != 1))
        raise Error, "Unsafe existing destination: #{current}"
      end
    end
    current
  end

  def self.install_skills(repo_root, files, contents)
    validate_files!(files)
    target = safe_destination!(repo_root, SKILLS_ROOT)
    if File.exist?(target)
      raise Error, "Skills destination must be a directory" unless File.directory?(target)

      Find.find(target) do |path|
        safe_destination!(repo_root, Pathname.new(path).relative_path_from(Pathname.new(repo_root)).to_s)
      end
      FileUtils.remove_entry(target)
    end
    FileUtils.mkdir_p(target, mode: 0o755)
    files.each do |path, entry|
      next unless path.start_with?("#{SKILLS_ROOT}/")

      destination = safe_destination!(repo_root, path)
      FileUtils.mkdir_p(File.dirname(destination), mode: 0o755)
      File.binwrite(destination, verify_blob!(entry, contents.fetch(entry.fetch("sha"))))
      File.chmod(0o644, destination)
    end
  end

  def self.snapshot(repo_root, files, contents)
    Dir.mktmpdir("skill-validation-index-") do |directory|
      env = { "GIT_INDEX_FILE" => File.join(directory, "index") }
      git(repo_root, "read-tree", "--empty", env: env)
      files.each do |path, entry|
        blob = git(repo_root, "hash-object", "-w", "--stdin", input: contents.fetch(entry.fetch("sha")))
        raise Error, "Snapshot blob changed" unless blob == entry.fetch("sha")

        git(repo_root, "update-index", "--add", "--cacheinfo", entry.fetch("mode"), blob, path, env: env)
      end
      git(repo_root, "write-tree", env: env)
    end
  end

  def self.local_blob(repo_root, entry)
    sha = entry.fetch("sha")
    metadata = git(
      repo_root, "cat-file", "--batch-check",
      env: { "GIT_NO_LAZY_FETCH" => "1" }, input: "#{sha}\n"
    )
    return nil if metadata == "#{sha} missing"

    unless metadata == "#{sha} blob #{entry.fetch("size")}"
      raise Error, "Local Git object does not match the bounded data entry"
    end

    verify_blob!(entry, git(repo_root, "cat-file", "blob", sha, strip: false))
  end

  def self.install_policy_data(repo_root, destination, files, contents)
    destination = File.expand_path(destination)
    if inside?(destination, repo_root) || File.exist?(destination) || File.symlink?(destination)
      raise Error, "Policy data requires a fresh directory outside the trusted checkout"
    end

    Dir.mkdir(destination, 0o700)
    POLICY_PATHS.each do |path|
      entry = files[path]
      next unless entry

      target = safe_destination!(destination, path)
      FileUtils.mkdir_p(File.dirname(target), mode: 0o700)
      File.binwrite(target, verify_blob!(entry, contents.fetch(entry.fetch("sha"))))
      File.chmod(0o600, target)
    end
  end

  def self.acquire(repo_root, api, head_sha, trusted_ref, policy_directory: nil)
    root_tree = api.commit_tree(sha!(head_sha))
    validate_controls!(repo_root, api, root_tree, trusted_ref)
    files = DATA_ROOTS.each_with_object({}) { |root, result| result.merge!(api.files(root_tree, root)) }
    validate_files!(files)
    contents = {}
    files.each_value do |entry|
      contents[entry.fetch("sha")] ||= local_blob(repo_root, entry) || api.blob(entry)
      verify_blob!(entry, contents.fetch(entry.fetch("sha")))
    end
    if files.key?(".github/plugin.json")
      plugin = JSON.parse(contents.fetch(files.fetch(".github/plugin.json").fetch("sha")), max_nesting: 32)
      raise Error, "Plugin metadata must be a JSON object" unless plugin.is_a?(Hash)
    end
    tree = snapshot(repo_root, files, contents)
    changed = git(repo_root, "diff", "--name-only", "-z", trusted_ref, tree, "--", SKILLS_ROOT, strip: false)
      .split("\0").map { |path| skill_name!(path.split("/")[2]) }.uniq.sort
    harness_changed = HARNESS_PATHS.any? do |path|
      candidate = api.entry(root_tree, path)
      trusted = local_files(repo_root, trusted_ref, path)[path]
      candidate&.values_at("mode", "sha") != trusted&.values_at("mode", "sha")
    end
    install_policy_data(repo_root, policy_directory, files, contents) if policy_directory
    install_skills(repo_root, files, contents)
    {
      "tree_sha" => tree,
      "skill_count" => files.keys.grep(%r{\A\.github/skills/}).map { |path| path.split("/")[2] }.uniq.length,
      "agent_count" => files.keys.count { |path| path.match?(%r{\A\.github/agents/[^/]+\.md\z}) },
      "changed_skills" => changed.join(","),
      "harness_changed" => harness_changed
    }
  end

  def self.restore(repo_root, tree)
    raise Error, "Expected a data-only snapshot tree" unless git(repo_root, "cat-file", "-t", sha!(tree)) == "tree"

    files = local_files(repo_root, tree)
    validate_files!(files)
    contents = files.values.to_h do |entry|
      [entry.fetch("sha"), verify_blob!(entry, git(repo_root, "cat-file", "blob", entry.fetch("sha"), strip: false))]
    end
    install_skills(repo_root, files, contents)
  end

  def self.baseline(repo_root, tree, base_sha, tests_path)
    match = tests_path.match(%r{\A\.github/skills/([^/]+)/tests\z})
    raise Error, "Invalid baseline tests scope" unless match

    skill_name!(match[1])
    root = tests_path.delete_suffix("/tests")
    candidate = local_files(repo_root, sha!(tree), root)
    base = local_files(repo_root, sha!(base_sha), root)
    validate_files!(candidate)
    validate_files!(base)
    unless base.key?("#{root}/SKILL.md")
      warn "No base SKILL.md; skipping baseline"
      return false
    end
    paths = (candidate.keys | base.keys).reject { |path| path.start_with?("#{root}/tests/") }
    changed = paths.select { |path| candidate[path]&.values_at("mode", "sha") != base[path]&.values_at("mode", "sha") }
    if changed.empty? || changed.any? { |path| !path.end_with?(".md") }
      warn "Instructions unchanged or non-Markdown assets changed; skipping baseline"
      return false
    end
    contents = paths.grep(/\.md\z/).to_h do |path|
      entry = base[path]
      content = entry && verify_blob!(entry, git(repo_root, "cat-file", "blob", entry.fetch("sha"), strip: false))
      safe_destination!(repo_root, path)
      [path, content]
    end
    contents.each do |path, content|
      destination = safe_destination!(repo_root, path)
      if content
        FileUtils.mkdir_p(File.dirname(destination), mode: 0o755)
        File.binwrite(destination, content)
        File.chmod(0o644, destination)
      elsif File.exist?(destination)
        File.unlink(destination)
      end
    end
    true
  end

  def self.main(argv, env = ENV)
    raise Error, "A command and trusted checkout path are required" unless argv.length >= 2

    command, root, *args = argv
    repo_root = File.realpath(root)
    case command
    when "acquire"
      raise Error, "acquire does not accept additional arguments" unless args.empty?

      trusted_sha = sha!(env.fetch("TRUSTED_SHA"))
      raise Error, "Checkout is not the trusted workflow revision" unless git(repo_root, "rev-parse", "HEAD") == trusted_sha

      base_sha = env["TRUSTED_BASE_SHA"].to_s
      unless base_sha.empty? || sha!(base_sha) == trusted_sha
        metadata = git(
          repo_root, "cat-file", "--batch-check",
          env: { "GIT_NO_LAZY_FETCH" => "1" }, input: "#{base_sha}\n"
        )
        if metadata == "#{base_sha} missing"
          repository = repository!(env.fetch("TRUSTED_REPOSITORY"))
          git(repo_root, "fetch", "--no-tags", "--depth=1", "https://github.com/#{repository}.git", base_sha)
        elsif !metadata.match?(/\A#{base_sha} commit [0-9]+\z/)
          raise Error, "The trusted base SHA is not a commit"
        end
      end
      trusted_ref = trusted_repository_control_ref(repo_root, env)
      api = GitHubData.new(env.fetch("HEAD_REPOSITORY"), env.fetch("GH_TOKEN"))
      outputs = Timeout.timeout(600) do
        acquire(repo_root, api, env.fetch("HEAD_SHA"), trusted_ref, policy_directory: env.fetch("POLICY_DIRECTORY"))
      end
      File.open(env.fetch("GITHUB_OUTPUT"), "a") do |file|
        outputs.each { |key, value| file.puts("#{key}=#{value}") }
      end
      puts "Acquired #{outputs.fetch("skill_count")} skill(s) as bounded Git data"
    when "restore"
      raise Error, "restore requires the snapshot tree SHA" unless args.length == 1

      restore(repo_root, args[0])
    when "baseline"
      raise Error, "baseline requires snapshot, base SHA, and tests path" unless args.length == 3

      puts baseline(repo_root, *args)
    else
      raise Error, "Usage: AcquireSkillValidationContent.rb <acquire|restore|baseline> <trusted-checkout> [args]"
    end
  end
end

if __FILE__ == $PROGRAM_NAME
  begin
    SkillValidationContent.main(ARGV)
  rescue SkillValidationContent::Error, JSON::ParserError, KeyError, Timeout::Error,
      SocketError, OpenSSL::SSL::SSLError, SystemCallError => error
    warn "Skill content acquisition failed: #{error.message.inspect}"
    exit 1
  end
end
