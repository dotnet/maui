#!/usr/bin/env ruby

require "open3"
require "pathname"
require "tmpdir"
require "yaml"

BASE_REF = "8935253083b24930f9a6f153f72b5ac196d1ae59"
FORBIDDEN_ENVIRONMENT_KEYS = %w[commands env mcpServers].freeze
FORBIDDEN_GRADERS = %w[program run-command].freeze
FORBIDDEN_DESTINATION_COMPONENTS = %w[.git .hg .svn].freeze

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

def validate_relative_path!(value, base, root, location)
  fail!("#{location} must be a relative path") if Pathname.new(value).absolute?

  resolved = File.expand_path(value, base)
  begin
    real_path = File.realpath(resolved)
  rescue Errno::ENOENT
    fail!("#{location} does not exist: #{resolved}")
  end
  fail!("#{location} escapes #{root}") unless inside?(real_path, root)
end

def validate_environment!(environment, spec_path, skill_root, location)
  return unless environment
  fail!("#{location} must be a mapping") unless environment.is_a?(Hash)

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
    validate_relative_path!(src, File.dirname(spec_path), skill_root, "#{location}.files[#{index}].src")
    destination_parts = Pathname.new(dest).each_filename.to_a
    fail!("#{location}.files[#{index}].dest must be a non-empty relative path") if dest.empty? || Pathname.new(dest).absolute?
    fail!("#{location}.files[#{index}].dest must not traverse parent directories") if destination_parts.include?("..")
    forbidden_component = destination_parts.find { |part| FORBIDDEN_DESTINATION_COMPONENTS.include?(part.downcase) }
    fail!("#{location}.files[#{index}].dest targets forbidden VCS metadata: #{forbidden_component}") if forbidden_component
  end

  git = environment["git"]
  return unless git
  fail!("#{location}.git must be a mapping") unless git.is_a?(Hash)
  fail!("#{location}.git.type must be worktree") unless git["type"] == "worktree"
  fail!("#{location}.git.source must be .") unless git["source"] == "."
  fail!("#{location}.git.ref must be a full commit SHA") unless git["ref"].is_a?(String) && git["ref"].match?(/\A[0-9a-f]{40}\z/)
end

def validate_graders!(graders, location)
  Array(graders).each_with_index do |grader, index|
    next unless grader.is_a?(Hash)
    next unless FORBIDDEN_GRADERS.include?(grader["type"])

    fail!("#{location}[#{index}] uses forbidden grader #{grader['type']}")
  end
end

def validate_spec!(spec_path, skill_root)
  document = YAML.safe_load_file(spec_path, permitted_classes: [], permitted_symbols: [], aliases: true)
  fail!("#{spec_path} must contain a mapping") unless document.is_a?(Hash)

  validate_environment!(document["environment"], spec_path, skill_root, "environment")
  validate_graders!(document["graders"], "graders")

  Array(document["stimuli"]).each_with_index do |stimulus, index|
    fail!("stimuli[#{index}] must be a mapping") unless stimulus.is_a?(Hash)
    validate_environment!(stimulus["environment"], spec_path, skill_root, "stimuli[#{index}].environment")
    validate_graders!(stimulus["graders"], "stimuli[#{index}].graders")
  end
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
  validate_spec!(real_spec_path, skill_root)
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

  spec_paths.each { |spec_path| validate_spec!(spec_path, skill_root) }
end

puts "Validated #{spec_paths.length} Vally eval spec(s) under #{tests_path}"
