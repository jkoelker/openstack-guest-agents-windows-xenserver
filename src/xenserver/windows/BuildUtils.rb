def output_dir
	if ENV.keys.include?('CC_BUILD_ARTIFACTS')
		dir = ENV['CC_BUILD_ARTIFACTS']
	else
		dir = 'results'
	end
	Dir.mkdir dir unless File.exists?(dir)
	return dir
end

class ReleaseZipper
  def create(filename, root, installerBatchFile, excludes=/^$/)
    root = root + "/" if root[root.length - 1].chr != "/"
    Zip::ZipFile.open(filename, Zip::ZipFile::CREATE) do |zip|
      Find.find(root) do |path|
        next if path =~ excludes
        zip_path = path.gsub(root, '')
        zip.add(zip_path, path)
      end
      zip.add(installerBatchFile, File.join(Dir.pwd, installerBatchFile))
    end
  end
end

class AssemblyInfoBuilder
  attr_accessor :assemblyAttributes
  def initialize(sourcePath, assemblyAttributes = {})
    @sourcePath = sourcePath
    @assemblyAttributes = assemblyAttributes
  end
  
  def do
    assemblyInfoFiles = FileList["#{@sourcePath}/**/AssemblyInfo.cs"]
    assemblyInfoFiles.each do |filePath|
      puts "AssemblyInfo file found at: #{filePath}"
      assemblyTitleLine = IO.readlines(filePath).grep /AssemblyTitle/
      AssemblyFile.process(filePath, "w+") do |file|
        file.puts "using System.Reflection;"
        file.puts "using System.Runtime.InteropServices;"
        file.puts ""
        file.puts assemblyTitleLine
        file.puts "[assembly: AssemblyDescription(\"#{@assemblyAttributes[:description]}\")]"
        file.puts "[assembly: AssemblyConfiguration(\"\")]"
        file.puts "[assembly: AssemblyCompany(\"#{@assemblyAttributes[:company]}\")]"
        file.puts "[assembly: AssemblyProduct(\"#{@assemblyAttributes[:product]}\")]"
        file.puts "[assembly: AssemblyCopyright(\"#{@assemblyAttributes[:copyright]}\")]"
        file.puts "[assembly: AssemblyTrademark(\"\")]"
        file.puts "[assembly: AssemblyCulture(\"\")]"
        file.puts "[assembly: ComVisible(false)]"
        file.puts "[assembly: AssemblyVersion(\"#{@assemblyAttributes[:version]}.#{@assemblyAttributes[:buildNumber]}\")]"
        file.puts "[assembly: AssemblyFileVersion(\"#{@assemblyAttributes[:version]}.#{@assemblyAttributes[:buildNumber]}\")]"
      end
    end
  end
end
  
class AssemblyFile
  def AssemblyFile.process(*args)
    f = File.open(*args)
    yield f
    f.close()
  end
end
