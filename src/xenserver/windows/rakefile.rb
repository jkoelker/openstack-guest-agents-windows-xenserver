require 'fileutils'
require 'BuildUtils'
require "Properties"
require 'zip/zip'
require 'find'
require 'rake/clean'

CLEAN.include(UNITTESTS_RESULTS_DIR, output_dir, "src/**/bin", "src/**/obj")

task :default => ["clean", "build:all", "package", "finish"]

namespace :build do
  task :all => [:compile, :test]

  desc "creates output directories"
  task :create do
    FileUtils.mkdir UNITTESTS_RESULTS_DIR unless File.exists?(UNITTESTS_RESULTS_DIR)
    FileUtils.mkdir output_dir unless File.exists?(output_dir)
  end

  desc "version the assemblies"
  task :version do
    puts "writing AssemblyInfo files ..."
    asmInfoBuilder = AssemblyInfoBuilder.new(SOURCE_DIR,   
                                            {:version => RELEASE_VERSION,
                                            :buildNumber => RELEASE_BUILD_NUMBER,
                                            :product => PRODUCT, 
                                            :copyright => COPYRIGHT,
                                            :company => COMPANY,
                                            :description => DESCRIPTION})
    asmInfoBuilder.do
  end

  desc "code compile"
  task :compile => [:create, :version] do
	puts "BUILD REVISION: #{ENV['CC_BUILD_REVISION']}"
	puts "BUILD LABEL: #{ENV['CC_BUILD_LABEL']}"
	puts "BUILD ARTIFACTS: #{ENV['CC_BUILD_ARTIFACTS']}"
    puts "compiling solution(s) ..."
    solutions = FileList["#{SOURCE_DIR}/**/*.sln"]
    solutions.each do |solution|
      sh "#{MSBUILD_DIR}\\msbuild.exe /t:Clean;Rebuild /p:Configuration=Debug #{solution}"
    end
  end

  desc "runs tests"
  task :test => [:compile] do
    puts "running unit tests ..."
    tests = FileList["#{SOURCE_DIR}/**/*.Specs.dll"].exclude(/obj\//)
    sh "#{NUNIT_EXE} #{tests} /nologo /xml=#{UNITTESTS_RESULTS_DIR}/TestResults.xml"
  end
end

desc "create index.html file for custom artifacts"
task :create_artifact_index do
  INDEX_FILE_NAME = 'index.html'
  
  INDEX = <<-EOS
<html>
<body>
  <a href="#{output_dir}\\AgentService.zip">AgentService.zip</a><br/>
  <a href="#{output_dir}\\UpdateService.zip">UpdateService.zip</a>
</body>
</html>
  EOS

  File.open("#{output_dir}\\#{INDEX_FILE_NAME}", 'w') {|f| f.write(INDEX) }
end

desc "dumps release dll's into zip file(s)"
task :package do 
  puts "creating build packages ..."
  zipper = ReleaseZipper.new
  zipper.create("#{output_dir}\\AgentService.zip", AGENT_SERVICE_DIR, "installagentservice.bat")
  zipper.create("#{output_dir}\\UpdateService.zip", UPDATE_SERVICE_DIR, "installupdateservice.bat")
  Rake::Task["create_artifact_index"].execute
end

desc "performs any post complete functions"
task :finish do 
  puts "ALL DONE."
end