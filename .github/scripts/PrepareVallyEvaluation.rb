#!/usr/bin/env ruby

require "find"
require "open3"
require "pathname"
require "tmpdir"
require "yaml"

BASE_REF = "8935253083b24930f9a6f153f72b5ac196d1ae59"
FORBIDDEN_ENVIRONMENT_KEYS = %w[commands env mcpServers].freeze
FORBIDDEN_GRADERS = %w[program run-command].freeze
FORBIDDEN_DESTINATION_COMPONENTS = %w[.git .hg .svn].freeze
REQUIRED_SKILL_INVOCATION_SPECS = %w[
  .github/skills/ci-fix/tests/eval.ownership.vally.yaml
  .github/skills/code-review/tests/eval.trim-aot.vally.yaml
  .github/skills/try-fix/tests/eval.restore.vally.yaml
  .github/skills/try-fix/tests/eval.vally.yaml
].freeze
PARAM_PLACEHOLDER_PATTERN = /\$\{[A-Za-z_]\w*(?:=[^}]*)?\}/
PERSISTENT_GIT_IDENTITY_PATTERN =
  /(?:\A|[;&|\r\n])\s*git(?:\s+(?:(?:-C|-c)\s+\S+|--\S+))*\s+config(?:\s+--\S+)*\s+user\.(?:name|email)\b/im

FIXTURES = {
  "eval.restore.vally.yaml" => [
    {
      marker: "restores-synthetic-fix-without-raw-git",
      fallback_ref: "709ea3d7c75a03fa011956941fd9ca7c631e9d24",
      message: "Synthetic WebView lifecycle restoration attempt",
      files: ["src/Core/src/Handlers/WebView/WebViewHandler.Android.cs"]
    }
  ],
  "eval.vally.yaml" => [
    {
      marker: "happy-path-distinct-alternative-fix",
      fallback_ref: "709ea3d7c75a03fa011956941fd9ca7c631e9d24",
      message: "Synthetic WebView lifecycle fix",
      files: ["src/Core/src/Handlers/WebView/WebViewHandler.Android.cs"]
    },
    {
      marker: "regression-no-success-without-running-test",
      fallback_ref: "45662d6e08ae7fc7f6db0314d00111e34b5f99b1",
      message: "Synthetic Editor layout fix",
      files: ["src/Core/src/Handlers/Editor/EditorHandler.iOS.cs"]
    },
    {
      marker: "edge-case-second-attempt-avoids-prior-approach",
      fallback_ref: "2b1326a2098b26419ebe421455b04d081cfd3a25",
      message: "Synthetic Shell toolbar fix",
      files: ["src/Controls/src/Core/Compatibility/Handlers/Shell/Android/ShellToolbarTracker.cs"]
    },
    {
      marker: "edge-case-exhausted-iterations-documented-fail",
      fallback_ref: "5beb719c5c378c37b5b6e2f37fba4cdacadcaeae",
      message: "Synthetic CarouselView overlap fix",
      files: ["src/Controls/src/Core/Handlers/Items/Android/MauiCarouselRecyclerView.cs"]
    },
    {
      marker: "regression-no-repeated-root-cause-disguised",
      fallback_ref: "d1a7b782d363f34f72527284fdf8d76acdcc91ce",
      message: "Synthetic CollectionView template update fix",
      files: ["src/Controls/src/Core/Handlers/Items/StructuredItemsViewHandler.Android.cs"]
    },
    {
      marker: "regression-verify-correct-platform-code-path",
      fallback_ref: "7fede996393862cd695ac70323a82bb10c75a652",
      message: "Synthetic NavigationPage disconnect fix",
      files: ["src/Controls/src/Core/Compatibility/Handlers/NavigationPage/iOS/NavigationRenderer.cs"]
    }
  ]
}.freeze

def fail!(message)
  warn "Vally evaluation safety check failed: #{message}"
  exit 1
end

def run_git(repo_root, *args, env: {}, input: nil, strip: true)
  stdout, stderr, status = Open3.capture3(env, "git", "-C", repo_root, *args, stdin_data: input)
  raise "git #{args.join(' ')} failed: #{stderr}" unless status.success?

  strip ? stdout.strip : stdout
end

def inside?(path, root)
  path == root || path.start_with?("#{root}#{File::SEPARATOR}")
end

def validate_no_checkout_symlinks!(relative_path, repo_root, location)
  current = repo_root
  Pathname.new(relative_path).each_filename do |part|
    current = File.join(current, part)
    break unless File.exist?(current) || File.symlink?(current)

    fail!("#{location} traverses checkout symlink: #{current}") if File.symlink?(current)
  end
end

def validate_no_nested_symlinks!(path, location)
  return unless File.directory?(path)

  Find.find(path) do |entry|
    fail!("#{location} contains symlink: #{entry}") if File.symlink?(entry)
  end
end

def validate_relative_path!(value, base, root, location)
  fail!("#{location} must be a relative path") if Pathname.new(value).absolute?

  current = File.realpath(base)
  Pathname.new(value).each_filename do |part|
    if part == ".."
      current = File.dirname(current)
      fail!("#{location} escapes #{root}") unless inside?(current, root)
      next
    end
    next if part == "."

    candidate = File.join(current, part)
    fail!("#{location} traverses checkout symlink: #{candidate}") if File.symlink?(candidate)
    begin
      current = File.realpath(candidate)
    rescue Errno::ENOENT
      fail!("#{location} does not exist: #{candidate}")
    end
    fail!("#{location} escapes #{root}") unless inside?(current, root)
  end
  current
end

def validate_environment!(environment, spec_path, skill_root, repo_root, location)
  return unless environment
  fail!("#{location} must be a mapping") unless environment.is_a?(Hash)

  Array(environment["commands"]).each_with_index do |command, index|
    next unless command.is_a?(String) && command.match?(PERSISTENT_GIT_IDENTITY_PATTERN)

    fail!("#{location}.commands[#{index}] persistently writes Git identity; use per-command git -c instead")
  end

  forbidden = FORBIDDEN_ENVIRONMENT_KEYS & environment.keys
  fail!("#{location} uses forbidden key(s): #{forbidden.join(', ')}") unless forbidden.empty?

  Array(environment["skills"]).each_with_index do |skill, index|
    fail!("#{location}.skills[#{index}] must be a string") unless skill.is_a?(String)
    validate_relative_path!(skill, File.dirname(spec_path), skill_root, "#{location}.skills[#{index}]")
  end

  Array(environment["files"]).each_with_index do |file, index|
    fail!("#{location}.files[#{index}] must be a mapping") unless file.is_a?(Hash)
    src = file["src"]
    dest = file["dest"]
    fail!("#{location}.files[#{index}].src must be a string") unless src.is_a?(String)
    fail!("#{location}.files[#{index}].dest must be a string") unless dest.is_a?(String)
    src_location = "#{location}.files[#{index}].src"
    resolved_src = validate_relative_path!(src, File.dirname(spec_path), skill_root, src_location)
    relative_src = Pathname.new(resolved_src).relative_path_from(Pathname.new(skill_root)).to_s
    validate_no_checkout_symlinks!(relative_src, skill_root, src_location)
    validate_no_nested_symlinks!(resolved_src, src_location)
    destination_parts = Pathname.new(dest).each_filename.to_a
    fail!("#{location}.files[#{index}].dest must be a non-empty relative path") if dest.empty? || Pathname.new(dest).absolute?
    fail!("#{location}.files[#{index}].dest must not traverse parent directories") if destination_parts.include?("..")
    forbidden_component = destination_parts.find { |part| FORBIDDEN_DESTINATION_COMPONENTS.include?(part.downcase) }
    fail!("#{location}.files[#{index}].dest targets forbidden VCS metadata: #{forbidden_component}") if forbidden_component
    validate_no_checkout_symlinks!(dest, repo_root, "#{location}.files[#{index}].dest")
  end

  git = environment["git"]
  return unless git
  fail!("#{location}.git must be a mapping") unless git.is_a?(Hash)
  fail!("#{location}.git.type must be worktree") unless git["type"] == "worktree"
  fail!("#{location}.git.source must be .") unless git["source"] == "."
  fail!("#{location}.git.ref must be a full commit SHA") unless git["ref"].is_a?(String) && git["ref"].match?(/\A[0-9a-f]{40}\z/)
end

def validate_git_destination!(repo_root, ref, dest, location)
  prefix = []
  Pathname.new(dest).each_filename do |part|
    prefix << part
    path = prefix.join("/")
    entry = run_git(repo_root, "ls-tree", ref, "--", path)
    next if entry.empty?

    mode = entry.split.first
    fail!("#{location} traverses symlink at #{ref}:#{path}") if mode == "120000"
  end
end

def validate_effective_git_destinations!(document, repo_root)
  parent_environment = document["environment"] || {}
  contexts = [["environment", parent_environment]]
  Array(document["stimuli"]).each_with_index do |stimulus, index|
    child_environment = stimulus["environment"] || {}
    effective_environment = parent_environment.merge(child_environment)
    effective_environment["files"] = Array(parent_environment["files"]) + Array(child_environment["files"])
    contexts << ["stimuli[#{index}].environment", effective_environment]
  end

  contexts.each do |location, environment|
    git = environment["git"]
    files = Array(environment["files"])
    next unless git && !files.empty?

    ref = git.fetch("ref")
    begin
      run_git(repo_root, "cat-file", "-e", "#{ref}^{commit}")
    rescue RuntimeError => e
      fail!("#{location}.git.ref is unavailable for destination validation: #{e.message}")
    end
    files.each_with_index do |file, index|
      validate_git_destination!(repo_root, ref, file.fetch("dest"), "#{location}.files[#{index}].dest")
    end
  end
end

def validate_graders!(graders, location)
  Array(graders).each_with_index do |grader, index|
    next unless grader.is_a?(Hash)
    grader_type = grader["type"]
    if grader_type == "skill-invocation" && grader.key?("name")
      fail!("#{location}[#{index}] must not rename the skill-invocation grader")
    end
    next unless FORBIDDEN_GRADERS.include?(grader_type)

    fail!("#{location}[#{index}] uses forbidden grader #{grader_type}")
  end
end

def validate_spec!(spec_path, skill_root, repo_root, inspect_git_refs:)
  fail!("#{spec_path} uses Vally parameter placeholders; trusted validation requires structurally static specs") if File.read(spec_path).match?(PARAM_PLACEHOLDER_PATTERN)

  begin
    document = YAML.safe_load_file(spec_path, permitted_classes: [], permitted_symbols: [], aliases: false)
  rescue Psych::AliasesNotEnabled
    fail!("#{spec_path} uses YAML aliases; trusted validation requires alias-free specs")
  end
  fail!("#{spec_path} must contain a mapping") unless document.is_a?(Hash)

  validate_environment!(document["environment"], spec_path, skill_root, repo_root, "environment")
  validate_graders!(document["graders"], "graders")
  relative_spec_path = Pathname.new(spec_path).relative_path_from(Pathname.new(repo_root)).to_s
  require_skill_invocation = REQUIRED_SKILL_INVOCATION_SPECS.include?(relative_spec_path)

  Array(document["stimuli"]).each_with_index do |stimulus, index|
    fail!("stimuli[#{index}] must be a mapping") unless stimulus.is_a?(Hash)
    validate_environment!(stimulus["environment"], spec_path, skill_root, repo_root, "stimuli[#{index}].environment")
    graders = Array(stimulus["graders"])
    validate_graders!(graders, "stimuli[#{index}].graders")
    if require_skill_invocation && graders.none? { |grader| grader.is_a?(Hash) && grader["type"] == "skill-invocation" }
      fail!("stimuli[#{index}].graders must include skill-invocation for #{relative_spec_path}")
    end
  end
  validate_effective_git_destinations!(document, repo_root) if inspect_git_refs
end

def fixture_files(fixture)
  fixture[:files].is_a?(Hash) ? fixture[:files].keys : fixture[:files]
end

def create_fixture_commit(repo_root, fixture)
  Dir.mktmpdir("vally-fixture-") do |temp_root|
    index_path = File.join(temp_root, "index")
    index_env = { "GIT_INDEX_FILE" => index_path }
    run_git(repo_root, "read-tree", BASE_REF, env: index_env)

    fixture_files(fixture).each do |path|
      content = run_git(repo_root, "show", "#{BASE_REF}:#{path}", strip: false)
      transformed = if fixture[:files].is_a?(Hash)
                      fixture[:files].fetch(path).call(content)
                    else
                      "#{content}\n// Vally fixture: committed candidate fix.\n"
                    end
      blob = run_git(repo_root, "hash-object", "-w", "--stdin", input: transformed)
      mode = run_git(repo_root, "ls-tree", BASE_REF, "--", path).split.first
      raise "Could not determine mode for #{path}" unless mode

      run_git(repo_root, "update-index", "--add", "--cacheinfo", mode, blob, path, env: index_env)
    end

    tree = run_git(repo_root, "write-tree", env: index_env)
    head = run_git(
      repo_root,
      "-c", "user.name=Vally Fixture",
      "-c", "user.email=vally-fixture@example.invalid",
      "commit-tree", tree, "-p", BASE_REF, "-m", fixture[:message]
    )

    parent = run_git(repo_root, "rev-parse", "#{head}^")
    changed = run_git(repo_root, "diff", "--name-only", "#{head}^", head).split("\n").sort
    raise "Fixture #{fixture[:marker]} has unexpected parent #{parent}" unless parent == BASE_REF
    raise "Fixture #{fixture[:marker]} changed #{changed.inspect}" unless changed == fixture_files(fixture).sort

    head
  end
end

def patch_fixture_ref!(spec_path, fixture, fixture_head)
  marker = fixture.fetch(:marker)
  fallback_ref = fixture.fetch(:fallback_ref)
  content = File.read(spec_path)
  pattern = /^(\s*ref:\s*)([0-9a-f]{40})(\s+#\s*fixture:\s*#{Regexp.escape(marker)}\s*)$/
  matches = content.scan(pattern)
  raise "Expected one fixture marker #{marker} in #{spec_path}, found #{matches.length}" unless matches.length == 1
  raise "Fixture #{marker} fallback ref changed" unless matches.first[1] == fallback_ref

  File.write(spec_path, content.sub(pattern, "\\1#{fixture_head}\\3"))
end

repo_root = File.realpath(File.expand_path(ARGV.fetch(0)))
requested_tests_path = File.expand_path(ARGV.fetch(1), repo_root)
validate_only = ARGV.include?("--validate-only")
skills_root = File.realpath(File.join(repo_root, ".github", "skills"))
fail!("tests path does not exist: #{requested_tests_path}") unless Dir.exist?(requested_tests_path)
tests_path = File.realpath(requested_tests_path)
fail!("tests path escapes .github/skills") unless inside?(tests_path, skills_root)

spec_paths = Dir.glob(File.join(tests_path, "*.vally.yaml")).sort
fail!("no Vally specs found under #{tests_path}") if spec_paths.empty?
skill_root = File.realpath(File.dirname(tests_path))
spec_paths.each do |spec_path|
  real_spec_path = File.realpath(spec_path)
  fail!("spec path escapes #{tests_path}: #{spec_path}") unless inside?(real_spec_path, tests_path)
  validate_spec!(real_spec_path, skill_root, repo_root, inspect_git_refs: !validate_only)
end

if !validate_only && File.basename(skill_root) == "try-fix"
  FIXTURES.each do |spec_name, fixtures|
    spec_path = File.join(tests_path, spec_name)
    fail!("missing fixture spec #{spec_path}") unless File.file?(spec_path)

    fixtures.each do |fixture|
      fixture_head = create_fixture_commit(repo_root, fixture)
      patch_fixture_ref!(spec_path, fixture, fixture_head)
      puts "Prepared #{fixture[:marker]} at #{fixture_head}"
    end
  end

  spec_paths.each { |spec_path| validate_spec!(spec_path, skill_root, repo_root, inspect_git_refs: true) }
end

puts "Validated #{spec_paths.length} Vally eval spec(s) under #{tests_path}"
