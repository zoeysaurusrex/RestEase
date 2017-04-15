require 'json'

RESTEASE_DIR = 'src/RestEase'
TESTS_DIR = 'src/RestEaseUnitTests'

RESTEASE_CSPROJ = File.join(RESTEASE_DIR, 'RestEase.csproj')
NUGET_DIR = File.join(File.dirname(__FILE__), 'NuGet')

desc "Create NuGet package"
task :package do
  sh 'dotnet', 'pack', '--no-build', '--configuration=Release', "--output=\"#{NUGET_DIR}\"", '--include-symbols', RESTEASE_DIR
end

desc "Bump version number"
task :version, [:version] do |t, args|
  version = args[:version]

  content = IO.read(RESTEASE_CSPROJ)
  content[/<VersionPrefix>(.+?)<\/VersionPrefix>/, 1] = version
  File.open(RESTEASE_CSPROJ, 'w'){ |f| f.write(content) }
end

desc "Build the project for release"
task :build do
  sh 'dotnet', 'build', '--configuration=Release', RESTEASE_DIR
end

desc "Run tests"
task :test do
  Dir.chdir(TESTS_DIR) do
    sh 'dotnet', 'xunit'
  end
end
