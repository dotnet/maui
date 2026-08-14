#!/usr/bin/env ruby

require "find"
require "open3"
require "pathname"
require "tmpdir"
require "yaml"

BASE_REF = "8935253083b24930f9a6f153f72b5ac196d1ae59"
FIXTURE_COMMIT_ENV = {
  "GIT_AUTHOR_DATE" => "2000-01-01T00:00:00Z",
  "GIT_COMMITTER_DATE" => "2000-01-01T00:00:00Z"
}.freeze
FORBIDDEN_ENVIRONMENT_KEYS = %w[commands env mcpServers].freeze
FORBIDDEN_GRADERS = %w[program run-command].freeze
FORBIDDEN_DESTINATION_COMPONENTS = %w[.git .hg .svn .mock-executor].freeze
APPROVED_FILE_DESTINATION_PREFIXES = [
  %w[review-input],
  [".github", "pr-review"]
].freeze
REPOSITORY_CONTROL_PATHS = %w[
  .mcp.json
  .github/hooks
  .github/mcp.json
  .github/copilot/settings.json
  .github/copilot/settings.local.json
  .claude/settings.json
  .claude/settings.local.json
].freeze
REQUIRED_SKILL_INVOCATION_SPECS = %w[
  .github/skills/agentic-labeler/tests/eval.vally.yaml
  .github/skills/ci-fix/tests/eval.ownership.vally.yaml
  .github/skills/code-review/tests/eval.producer-trace.vally.yaml
  .github/skills/code-review/tests/eval.trim-aot.vally.yaml
  .github/skills/try-fix/tests/eval.restore.vally.yaml
  .github/skills/try-fix/tests/eval.vally.yaml
  .github/skills/verify-tests-fail-without-fix/tests/eval.protocol.vally.yaml
  .github/skills/verify-tests-fail-without-fix/tests/eval.vally.yaml
].freeze
PER_GRADER_MUST_PASS_SPECS = %w[
  .github/skills/verify-tests-fail-without-fix/tests/eval.protocol.vally.yaml
].freeze
DISALLOWED_SKILL_INVOCATION_STIMULI = {
  ".github/skills/try-fix/tests/eval.vally.yaml" => %w[
    negative-trigger-documentation-question
  ],
  ".github/skills/verify-tests-fail-without-fix/tests/eval.vally.yaml" => %w[
    negative-trigger-general-test-question
  ]
}.freeze
REQUIRED_SKILL_PATTERNS = {
  ".github/skills/code-review/tests/eval.producer-trace.vally.yaml" => {
    "producer-trace workflow step" => /^### Step 1\.5: Trace External Output Contracts \(Always Active\)$/,
    "External Output Contract output section" => /^### External Output Contract$/
  },
  ".github/skills/try-fix/tests/eval.restore.vally.yaml" => {
    "mandatory baseline step" => /^### Step 2: Establish Baseline \(MANDATORY\)$/,
    "mandatory restore step" => /^### Step 9: Restore Working Directory \(MANDATORY .+\)$/,
    "trusted restore command" => /EstablishBrokenBaseline\.ps1 -Restore/,
    "conditional no-state restore completion" => /No baseline state found.+Restored False/m,
    "raw-git cleanup prohibition" => /not `git checkout`, `git restore`, or `git reset`/,
    "injected skill preservation" => /an evaluator-loaded `try-fix\/` directory may remain visible in\s+`git status --short`; do not delete it\./m
  }
}.freeze
REQUIRED_STIMULUS_PROMPT_PATTERNS = {
  ".github/skills/try-fix/tests/eval.restore.vally.yaml" => {
    "restores-synthetic-fix-without-raw-git" => {
      "pre-existing untracked path preservation" =>
        /The evaluator injects a read-only `try-fix\/` skill directory.+must remain.+Do not remove or modify it/m
    }
  }
}.freeze
MANDATORY_SPEC_PATHS = (
  REQUIRED_SKILL_INVOCATION_SPECS +
  PER_GRADER_MUST_PASS_SPECS +
  REQUIRED_SKILL_PATTERNS.keys +
  REQUIRED_STIMULUS_PROMPT_PATTERNS.keys +
  DISALLOWED_SKILL_INVOCATION_STIMULI.keys
).uniq.freeze
PARAM_PLACEHOLDER_PATTERN = /\$\{[A-Za-z_]\w*(?:=[^}]*)?\}/
PERSISTENT_GIT_IDENTITY_PATTERN =
  /(?:\A|[;&|\r\n])\s*git(?:\s+(?:(?:-C|-c)\s+\S+|--\S+))*\s+config(?:\s+--\S+)*\s+user\.(?:name|email)\b/im

VERIFY_FIX_TRANSFORM = lambda do |content|
  marker = "\t\tinternal static bool VallyFixtureIsFixed => true;\n\n"
  insertion = "\t{\n#{marker}"
  transformed = content.sub("\t{\n", insertion)
  raise "Could not add verify fixture marker to Brush" if transformed == content

  transformed
end

VERIFY_TEST_TRANSFORM = lambda do |content|
  test = [
    "\t\t[Fact]",
    "\t\tpublic void VallyFixtureDetectsFix()",
    "\t\t{",
    "\t\t\tvar property = typeof(Brush).GetProperty(",
    "\t\t\t\t\"VallyFixtureIsFixed\",",
    "\t\t\t\tSystem.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);",
    "\t\t\tAssert.NotNull(property);",
    "\t\t\tAssert.True((bool)property.GetValue(null));",
    "\t\t}"
  ].join("\n")
  transformed = content.sub(/\n\t}\n}\s*\z/, "\n\n#{test}\n\t}\n}\n")
  raise "Could not add verify fixture test" if transformed == content

  transformed
end

VERIFY_FULL_FILES = {
  "src/Controls/src/Core/Brush/Brush.cs" => VERIFY_FIX_TRANSFORM,
  "src/Controls/tests/Core.UnitTests/BrushTypeConverterUnitTests.cs" => VERIFY_TEST_TRANSFORM
}.freeze

VERIFY_FAILURE_ONLY_FILES = {
  "src/Controls/tests/Core.UnitTests/BrushTypeConverterUnitTests.cs" => VERIFY_TEST_TRANSFORM
}.freeze

FIXTURES = {
  "try-fix" => {
    "eval.restore.vally.yaml" => [
      {
        marker: "restores-synthetic-fix-without-raw-git",
        fallback_ref: "451bd73351c20b49a03e43cafe54a1dfb31e3771",
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
  },
  "verify-tests-fail-without-fix" => {
    "eval.protocol.vally.yaml" => [
      {
        marker: "happy-path-full-verification-mode",
        fallback_ref: "643d58e3d1bbe9dcccb0b13571689633ff5bc52a",
        message: "Synthetic full test verification fixture",
        files: VERIFY_FULL_FILES
      },
      {
        marker: "happy-path-verify-failure-only-mode",
        fallback_ref: "7d5e02aace0ffd70fbc2559baa48cdf585bc24f5",
        message: "Synthetic failure-only test verification fixture",
        files: VERIFY_FAILURE_ONLY_FILES
      },
      {
        marker: "regression-no-manual-git-revert",
        fallback_ref: "643d58e3d1bbe9dcccb0b13571689633ff5bc52a",
        message: "Synthetic full test verification fixture",
        files: VERIFY_FULL_FILES
      },
      {
        marker: "edge-case-require-full-verification-with-fix-files",
        fallback_ref: "643d58e3d1bbe9dcccb0b13571689633ff5bc52a",
        message: "Synthetic full test verification fixture",
        files: VERIFY_FULL_FILES
      }
    ]
  }
}.freeze

def yaml_mapping_value_node(mapping, key)
  return unless mapping.is_a?(Psych::Nodes::Mapping)

  pair = mapping.children.each_slice(2).find do |key_node, _value_node|
    key_node.is_a?(Psych::Nodes::Scalar) && key_node.value == key
  end
  pair&.last
end

def validate_no_duplicate_mapping_keys!(node, relative_spec_path, scalar_visitor)
  if node.is_a?(Psych::Nodes::Mapping)
    key_lines = {}
    node.children.each_slice(2) do |key_node, value_node|
      if key_node.is_a?(Psych::Nodes::Scalar)
        key = scalar_visitor.accept(key_node)
        if key == "<<" || key_node.tag == "tag:yaml.org,2002:merge"
          fail!(
            "#{relative_spec_path} uses a YAML merge key at line #{key_node.start_line + 1}; " \
            "trusted validation requires explicit mappings"
          )
        end
        if key_lines.key?(key)
          fail!(
            "#{relative_spec_path} contains duplicate YAML mapping key #{key.inspect} at line " \
            "#{key_node.start_line + 1} (first defined at line #{key_lines.fetch(key) + 1})"
          )
        end
        key_lines[key] = key_node.start_line
      end

      validate_no_duplicate_mapping_keys!(key_node, relative_spec_path, scalar_visitor)
      validate_no_duplicate_mapping_keys!(value_node, relative_spec_path, scalar_visitor)
    end
  elsif node.respond_to?(:children)
    Array(node.children).each do |child|
      validate_no_duplicate_mapping_keys!(child, relative_spec_path, scalar_visitor)
    end
  end
end

def validate_fixture_marker_owners!(spec_path, relative_spec_path, syntax_tree)
  path_parts = Pathname.new(relative_spec_path).each_filename.to_a
  fixtures = FIXTURES.dig(path_parts[2], File.basename(relative_spec_path))
  return unless fixtures

  stimuli_node = yaml_mapping_value_node(syntax_tree.root, "stimuli")
  fail!("#{relative_spec_path} fixtures require a stimuli sequence") unless stimuli_node.is_a?(Psych::Nodes::Sequence)

  lines = File.readlines(spec_path)
  fixtures.each do |fixture|
    marker = fixture.fetch(:marker)
    marker_pattern = /#\s*fixture:\s*#{Regexp.escape(marker)}\s*$/
    marker_lines = lines.each_index.select { |line| lines[line].match?(marker_pattern) }
    unless marker_lines.length == 1
      fail!("#{relative_spec_path} requires exactly one fixture marker #{marker}")
    end

    marker_line = marker_lines.first
    owners = stimuli_node.children.filter_map do |stimulus_node|
      environment_node = yaml_mapping_value_node(stimulus_node, "environment")
      git_node = yaml_mapping_value_node(environment_node, "git")
      ref_node = yaml_mapping_value_node(git_node, "ref")
      next unless ref_node&.start_line == marker_line

      yaml_mapping_value_node(stimulus_node, "name")&.value
    end
    unless owners.length == 1
      fail!("#{relative_spec_path} fixture marker #{marker} must annotate its stimulus environment.git.ref")
    end
    unless owners.first == marker
      fail!("#{relative_spec_path} fixture marker #{marker} belongs to stimulus #{owners.first.inspect}, not the required stimulus")
    end
  end
end

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
    normalized_destination_parts = destination_parts.reject { |part| part == "." }
    normalized_destination_parts_downcase = normalized_destination_parts.map(&:downcase)
    forbidden_component = normalized_destination_parts.find { |part| FORBIDDEN_DESTINATION_COMPONENTS.include?(part.downcase) }
    fail!("#{location}.files[#{index}].dest targets forbidden VCS metadata: #{forbidden_component}") if forbidden_component
    if normalized_destination_parts_downcase.first(2) == [".github", "hooks"]
      fail!("#{location}.files[#{index}].dest targets forbidden repository hooks")
    end
    validate_no_checkout_symlinks!(dest, repo_root, "#{location}.files[#{index}].dest")
    approved_destination = APPROVED_FILE_DESTINATION_PREFIXES.any? do |prefix|
      normalized_destination_parts_downcase.first(prefix.length) == prefix
    end
    unless approved_destination
      fail!("#{location}.files[#{index}].dest must target an approved fixture root")
    end
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

def repository_control_entries(repo_root, ref)
  output = run_git(
    repo_root,
    "ls-tree", "-r", ref, "--", *REPOSITORY_CONTROL_PATHS,
    strip: false
  )
  output.lines.to_h do |line|
    metadata, path = line.chomp.split("\t", 2)
    [path, metadata]
  end
end

def validate_no_repository_control_symlinks!(repo_root, ref, location)
  prefixes = REPOSITORY_CONTROL_PATHS.flat_map do |path|
    parts = path.split("/")
    (1..parts.length).map { |length| parts.first(length).join("/") }
  end.uniq
  prefixes.each do |path|
    entry = run_git(repo_root, "ls-tree", ref, "--", path)
    next if entry.empty?

    mode = entry.split.first
    fail!("#{location} traverses repository control symlink: #{path}") if mode == "120000"
  end
end

def validate_repository_controls!(repo_root, ref, trusted_ref, location)
  return unless trusted_ref

  validate_no_repository_control_symlinks!(repo_root, ref, location)
  trusted_entries = repository_control_entries(repo_root, trusted_ref)
  candidate_entries = repository_control_entries(repo_root, ref)
  unsafe_paths = (trusted_entries.keys | candidate_entries.keys).filter_map do |path|
    path unless trusted_entries[path] == candidate_entries[path]
  end
  return if unsafe_paths.empty?

  fail!("#{location} contains untrusted repository control file(s): #{unsafe_paths.join(', ')}")
end

def trusted_repository_control_ref(repo_root, env = ENV, allow_missing: false)
  ref = env["TRUSTED_BASE_SHA"].to_s
  ref = env["TRUSTED_SHA"].to_s if ref.empty?
  if ref.empty?
    return nil if allow_missing

    fail!("TRUSTED_BASE_SHA or TRUSTED_SHA is required to validate repository controls")
  end

  fail!("trusted repository control ref must be a full commit SHA") unless ref.match?(/\A[0-9a-f]{40}\z/)
  resolved = run_git(repo_root, "rev-parse", "--verify", "#{ref}^{commit}")
  fail!("trusted repository control ref did not resolve exactly: #{ref}") unless resolved == ref

  ref
end

def validate_effective_git_destinations!(document, repo_root, trusted_control_ref)
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
    next unless git

    ref = git.fetch("ref")
    begin
      run_git(repo_root, "cat-file", "-e", "#{ref}^{commit}")
    rescue RuntimeError => e
      fail!("#{location}.git.ref is unavailable for destination validation: #{e.message}")
    end
    validate_repository_controls!(repo_root, ref, trusted_control_ref, "#{location}.git.ref")
    files.each_with_index do |file, index|
      validate_git_destination!(repo_root, ref, file.fetch("dest"), "#{location}.files[#{index}].dest")
    end
  end
end

def validate_graders!(graders, location)
  graders = Array(graders)
  skill_invocations = graders.count { |grader| grader.is_a?(Hash) && grader["type"] == "skill-invocation" }
  fail!("#{location} contains multiple skill-invocation graders") if skill_invocations > 1

  graders.each_with_index do |grader, index|
    next unless grader.is_a?(Hash)
    grader_type = grader["type"]
    if grader_type == "skill-invocation" && grader.key?("name")
      fail!("#{location}[#{index}] must not rename the skill-invocation grader")
    end
    next unless FORBIDDEN_GRADERS.include?(grader_type)

    fail!("#{location}[#{index}] uses forbidden grader #{grader_type}")
  end
end

def skill_invocation_targets?(grader, skill_name, polarity)
  return false unless grader.is_a?(Hash) && grader["type"] == "skill-invocation"

  config = grader["config"]
  return false unless config.is_a?(Hash)

  values = config[polarity]
  values.is_a?(Array) && values.include?(skill_name)
end

def validate_spec!(spec_path, relative_spec_path, skill_root, repo_root, inspect_git_refs:, trusted_control_ref:)
  fail!("#{spec_path} uses Vally parameter placeholders; trusted validation requires structurally static specs") if File.read(spec_path).match?(PARAM_PLACEHOLDER_PATTERN)

  syntax_tree = Psych.parse_file(spec_path)
  scalar_visitor = Psych::Visitors::NoAliasRuby.create(symbolize_names: false, freeze: false)
  validate_no_duplicate_mapping_keys!(syntax_tree, relative_spec_path, scalar_visitor)

  begin
    document = YAML.safe_load_file(spec_path, permitted_classes: [], permitted_symbols: [], aliases: false)
  rescue Psych::AliasesNotEnabled
    fail!("#{spec_path} uses YAML aliases; trusted validation requires alias-free specs")
  end
  fail!("#{spec_path} must contain a mapping") unless document.is_a?(Hash)
  %w[defaults config].each do |scope_name|
    scope = document[scope_name]
    if scope.is_a?(Hash) && scope.key?("executor") && scope["executor"] != "copilot-sdk"
      fail!("#{relative_spec_path} must not override the trusted copilot-sdk executor")
    end
  end
  if document.key?("graders")
    fail!("#{relative_spec_path} uses top-level graders, which pinned Vally does not execute")
  end

  validate_environment!(document["environment"], spec_path, skill_root, repo_root, "environment")
  require_skill_invocation = REQUIRED_SKILL_INVOCATION_SPECS.include?(relative_spec_path)
  if PER_GRADER_MUST_PASS_SPECS.include?(relative_spec_path) && document["scoring"] != {}
    fail!("#{relative_spec_path} must use an empty scoring map so every grader must pass its declared threshold")
  end
  skill_name = File.basename(skill_root)
  Array(REQUIRED_SKILL_PATTERNS[relative_spec_path]).each do |description, pattern|
    skill_path = File.join(skill_root, "SKILL.md")
    fail!("#{relative_spec_path} requires #{skill_path}") unless File.file?(skill_path)
    fail!("#{relative_spec_path} requires #{description} in #{skill_path}") unless File.read(skill_path).match?(pattern)
  end

  required_stimulus_prompts = REQUIRED_STIMULUS_PROMPT_PATTERNS.fetch(relative_spec_path, {})
  observed_required_stimuli = {}
  Array(document["stimuli"]).each_with_index do |stimulus, index|
    fail!("stimuli[#{index}] must be a mapping") unless stimulus.is_a?(Hash)
    stimulus_name = stimulus["name"]
    required_prompt_patterns = required_stimulus_prompts.fetch(stimulus_name, {})
    observed_required_stimuli[stimulus_name] = true if required_stimulus_prompts.key?(stimulus_name)
    required_prompt_patterns.each do |description, pattern|
      unless stimulus["prompt"].is_a?(String) && stimulus["prompt"].match?(pattern)
        fail!("#{relative_spec_path} stimuli[#{index}].prompt requires #{description}")
      end
    end
    unsupported_model_keys = stimulus.keys & %w[model judge_model]
    unless unsupported_model_keys.empty?
      fail!("stimuli[#{index}] uses unsupported model key(s): #{unsupported_model_keys.join(", ")}")
    end
    if stimulus.key?("attachments")
      fail!("stimuli[#{index}].attachments is forbidden in credentialed evaluation specs")
    end
    validate_environment!(stimulus["environment"], spec_path, skill_root, repo_root, "stimuli[#{index}].environment")
    graders = Array(stimulus["graders"])
    validate_graders!(graders, "stimuli[#{index}].graders")
    effective_invocations = graders.count do |grader|
      grader.is_a?(Hash) && grader["type"] == "skill-invocation"
    end
    if effective_invocations > 1
      fail!("stimuli[#{index}] has multiple effective skill-invocation graders")
    end
    if require_skill_invocation
      allowed_disallowed = Array(DISALLOWED_SKILL_INVOCATION_STIMULI[relative_spec_path])
      if allowed_disallowed.include?(stimulus["name"])
        unless graders.any? { |grader| skill_invocation_targets?(grader, skill_name, "disallowed") }
          fail!("stimuli[#{index}].graders must include disallowed skill-invocation targeting #{skill_name} for #{relative_spec_path}")
        end
      elsif graders.none? { |grader| skill_invocation_targets?(grader, skill_name, "required") }
        fail!("stimuli[#{index}].graders must include required skill-invocation targeting #{skill_name} for #{relative_spec_path}")
      end
      if stimulus.key?("supported_executors")
        fail!("stimuli[#{index}] must not restrict supported_executors in mandatory invocation spec #{relative_spec_path}")
      end
    end
  end
  missing_required_stimuli = required_stimulus_prompts.keys.reject { |name| observed_required_stimuli[name] }
  unless missing_required_stimuli.empty?
    fail!("#{relative_spec_path} is missing required stimulus name(s): #{missing_required_stimuli.join(", ")}")
  end
  validate_fixture_marker_owners!(spec_path, relative_spec_path, syntax_tree)
  if inspect_git_refs
    validate_effective_git_destinations!(document, repo_root, trusted_control_ref)
  end
  document
end

def effective_models(document, relative_spec_path)
  config = document["config"]
  defaults = document["defaults"]
  if config && defaults
    fail!("#{relative_spec_path} must not combine legacy config with defaults")
  end
  execution_defaults = defaults || config
  unless execution_defaults.is_a?(Hash)
    fail!("#{relative_spec_path} must declare defaults.model or legacy config.model")
  end

  executor_model = execution_defaults["model"]
  fail!("#{relative_spec_path} must declare an explicit executor model") unless executor_model

  models = [executor_model]
  default_judge_model = execution_defaults["judge_model"]
  Array(document["stimuli"]).each_with_index do |stimulus, stimulus_index|
    Array(stimulus["graders"]).each_with_index do |grader, grader_index|
      next unless grader.is_a?(Hash)

      case grader["type"]
      when "prompt"
        grader_config = grader["config"].is_a?(Hash) ? grader["config"] : {}
        judge_model = grader_config["model"] || default_judge_model
        unless judge_model
          fail!(
            "#{relative_spec_path} stimuli[#{stimulus_index}].graders[#{grader_index}] " \
            "must declare config.model or inherit an explicit judge_model"
          )
        end
        models << judge_model
      when "panel"
        grader_config = grader["config"]
        next unless grader_config.is_a?(Hash)

        Array(grader_config["models"]).each do |entry|
          models << (entry.is_a?(Hash) ? entry["model"] : entry)
        end
      end
    end
  end
  models
end

def fixture_files(fixture)
  fixture[:files].is_a?(Hash) ? fixture[:files].keys : fixture[:files]
end

def create_skill_overlay_commit(repo_root, parent_ref, source_ref, relative_root, message)
  Dir.mktmpdir("vally-skill-overlay-") do |temp_root|
    index_path = File.join(temp_root, "index")
    index_env = { "GIT_INDEX_FILE" => index_path }
    run_git(repo_root, "read-tree", parent_ref, env: index_env)

    parent_files = run_git(repo_root, "ls-tree", "-r", "--name-only", parent_ref, "--", relative_root).split("\n")
    source_files = run_git(repo_root, "ls-tree", "-r", "--name-only", source_ref, "--", relative_root).split("\n")
    (parent_files - source_files).each do |path|
      run_git(repo_root, "update-index", "--force-remove", "--", path, env: index_env)
    end
    source_files.each do |path|
      entry = run_git(repo_root, "ls-tree", source_ref, "--", path).split
      raise "Could not determine trusted tree entry for #{path}" unless entry.length >= 3

      run_git(repo_root, "update-index", "--add", "--cacheinfo", entry[0], entry[2], path, env: index_env)
    end

    tree = run_git(repo_root, "write-tree", env: index_env)
    run_git(
      repo_root,
      "-c", "user.name=Vally Fixture",
      "-c", "user.email=vally-fixture@example.invalid",
      "commit-tree", tree, "-p", parent_ref, "-m", message,
      env: FIXTURE_COMMIT_ENV
    )
  end
end

def trusted_fixture_source_ref(repo_root, env = ENV)
  # Fixture scripts execute in the credentialed Vally process, so source them
  # only from the immutable workflow revision rather than candidate HEAD.
  trusted_sha = env["TRUSTED_SHA"].to_s
  fail!("TRUSTED_SHA is required to materialize executable fixture scripts") unless trusted_sha.match?(/\A[0-9a-f]{40}\z/)

  resolved = run_git(repo_root, "rev-parse", "--verify", "#{trusted_sha}^{commit}")
  fail!("TRUSTED_SHA did not resolve exactly: #{trusted_sha}") unless resolved == trusted_sha

  trusted_sha
end

def validate_mandatory_layout!(repo_root)
  MANDATORY_SPEC_PATHS.each do |relative_spec_path|
    relative_tests_path = File.dirname(relative_spec_path)
    validate_no_checkout_symlinks!(relative_tests_path, repo_root, "mandatory tests path")
    tests_path = File.join(repo_root, relative_tests_path)
    fail!("missing mandatory tests path #{relative_tests_path}") unless Dir.exist?(tests_path)
    validate_no_checkout_symlinks!(relative_spec_path, repo_root, "mandatory spec")
    spec_path = File.join(repo_root, relative_spec_path)
    fail!("missing mandatory Vally spec #{relative_spec_path}") unless File.file?(spec_path)
  end
end

def create_fixture_commit(repo_root, fixture, parent_ref: BASE_REF)
  Dir.mktmpdir("vally-fixture-") do |temp_root|
    index_path = File.join(temp_root, "index")
    index_env = { "GIT_INDEX_FILE" => index_path }
    run_git(repo_root, "read-tree", parent_ref, env: index_env)

    fixture_files(fixture).each do |path|
      content = run_git(repo_root, "show", "#{parent_ref}:#{path}", strip: false)
      transformed = if fixture[:files].is_a?(Hash)
                      fixture[:files].fetch(path).call(content)
                    else
                      "#{content}\n// Vally fixture: committed candidate fix.\n"
                    end
      blob = run_git(repo_root, "hash-object", "-w", "--stdin", input: transformed)
      mode = run_git(repo_root, "ls-tree", parent_ref, "--", path).split.first
      raise "Could not determine mode for #{path}" unless mode

      run_git(repo_root, "update-index", "--add", "--cacheinfo", mode, blob, path, env: index_env)
    end

    tree = run_git(repo_root, "write-tree", env: index_env)
    head = run_git(
      repo_root,
      "-c", "user.name=Vally Fixture",
      "-c", "user.email=vally-fixture@example.invalid",
      "commit-tree", tree, "-p", parent_ref, "-m", fixture[:message],
      env: FIXTURE_COMMIT_ENV
    )

    parent = run_git(repo_root, "rev-parse", "#{head}^")
    changed = run_git(repo_root, "diff", "--name-only", "#{head}^", head).split("\n").sort
    raise "Fixture #{fixture[:marker]} has unexpected parent #{parent}" unless parent == parent_ref
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

def main(argv = ARGV)
  allow_missing_trusted_control_ref = argv.delete("--allow-missing-trusted-control-ref")
  list_models = argv.delete("--list-models")
  repo_root = File.realpath(File.expand_path(argv.fetch(0)))
  if argv.fetch(1) == "--validate-mandatory-layout"
    validate_mandatory_layout!(repo_root)
    puts "Validated #{MANDATORY_SPEC_PATHS.length} mandatory Vally spec path(s)"
    return
  end

  requested_tests_path = File.expand_path(argv.fetch(1), repo_root)
  validate_only = argv.include?("--validate-only")
  skills_root_relative = File.join(".github", "skills")
  validate_no_checkout_symlinks!(skills_root_relative, repo_root, "skills root")
  skills_root = File.realpath(File.join(repo_root, skills_root_relative))
  fail!("skills root escapes checkout: #{skills_root}") unless inside?(skills_root, repo_root)
  requested_tests_relative = Pathname.new(requested_tests_path).relative_path_from(Pathname.new(repo_root)).to_s
  tests_match = requested_tests_relative.match(%r{\A\.github/skills/([^/]+)/tests\z})
  fail!("tests path must use the lexical .github/skills/<skill>/tests scope") unless tests_match
  validate_no_checkout_symlinks!(requested_tests_relative, repo_root, "tests path")
  fail!("tests path does not exist: #{requested_tests_path}") unless Dir.exist?(requested_tests_path)
  tests_path = File.realpath(requested_tests_path)
  fail!("tests path escapes .github/skills") unless inside?(tests_path, skills_root)
  skill_name = tests_match[1]
  skill_root = File.realpath(File.join(skills_root, skill_name))
  trusted_control_ref = trusted_repository_control_ref(
    repo_root,
    allow_missing: allow_missing_trusted_control_ref
  )
  validate_repository_controls!(repo_root, "HEAD", trusted_control_ref, "candidate checkout")

  spec_paths = Dir.glob(File.join(tests_path, "*.vally.yaml")).sort
  fail!("no Vally specs found under #{tests_path}") if spec_paths.empty?
  spec_documents = spec_paths.to_h do |spec_path|
    relative_spec_path = Pathname.new(spec_path).relative_path_from(Pathname.new(repo_root)).to_s
    validate_no_checkout_symlinks!(relative_spec_path, repo_root, "spec path")
    real_spec_path = File.realpath(spec_path)
    fail!("spec path escapes #{tests_path}: #{spec_path}") unless inside?(real_spec_path, tests_path)
    document = validate_spec!(
      real_spec_path,
      relative_spec_path,
      skill_root,
      repo_root,
      inspect_git_refs: !validate_only,
      trusted_control_ref: trusted_control_ref
    )
    [spec_path, document]
  end
  mandatory_specs = MANDATORY_SPEC_PATHS.select do |relative_path|
    File.dirname(relative_path) == requested_tests_relative
  end
  mandatory_specs.each do |relative_path|
    mandatory_path = File.join(repo_root, relative_path)
    fail!("missing mandatory Vally spec #{relative_path}") unless File.file?(mandatory_path)
    validate_no_checkout_symlinks!(relative_path, repo_root, "mandatory spec")
  end
  if list_models
    effective_specs = spec_documents.select do |spec_path, _document|
      File.basename(spec_path).match?(/\Aeval.*\.vally\.yaml\z/)
    end
    fail!("no eval*.vally.yaml specs found under #{tests_path}") if effective_specs.empty?
    models = effective_specs.flat_map do |spec_path, document|
      relative_spec_path = Pathname.new(spec_path).relative_path_from(Pathname.new(repo_root)).to_s
      effective_models(document, relative_spec_path)
    end
    fail!("no explicit models found under #{tests_path}") if models.empty?
    models.each do |model|
      fail!("invalid explicit model under #{tests_path}: #{model.inspect}") unless model.is_a?(String) && model.match?(/\A[A-Za-z0-9._-]+\z/) && model != "auto"
    end
    models.uniq.sort.each { |model| puts model }
    return
  end

  skill_fixtures = FIXTURES[File.basename(skill_root)]
  if !validate_only && skill_fixtures
    fixture_parent = if File.basename(skill_root) == "verify-tests-fail-without-fix"
                       scripts_root = Pathname.new(File.join(skill_root, "scripts"))
                         .relative_path_from(Pathname.new(repo_root)).to_s
                       create_skill_overlay_commit(
                         repo_root,
                         BASE_REF,
                         trusted_fixture_source_ref(repo_root),
                         scripts_root,
                         "Synthetic verification skill baseline"
                       )
                     else
                       BASE_REF
                     end
    skill_fixtures.each do |spec_name, fixtures|
      spec_path = File.join(tests_path, spec_name)
      fail!("missing fixture spec #{spec_path}") unless File.file?(spec_path)

      fixtures.each do |fixture|
        fixture_head = create_fixture_commit(repo_root, fixture, parent_ref: fixture_parent)
        patch_fixture_ref!(spec_path, fixture, fixture_head)
        puts "Prepared #{fixture[:marker]} at #{fixture_head}"
      end
    end

    spec_paths.each do |spec_path|
      relative_spec_path = Pathname.new(spec_path).relative_path_from(Pathname.new(repo_root)).to_s
      validate_spec!(
        spec_path,
        relative_spec_path,
        skill_root,
        repo_root,
        inspect_git_refs: true,
        trusted_control_ref: trusted_control_ref
      )
    end
  end

  puts "Validated #{spec_paths.length} Vally eval spec(s) under #{tests_path}"
end

main if __FILE__ == $PROGRAM_NAME
