ABSOLUTE_PATH = File.expand_path(File.dirname(__FILE__))

COMPILE_TARGET = 'Release'
PRODUCT = "Rackspace Cloud Server Agent"
COPYRIGHT = "Copyright (c) 2009 2010 2011, Rackspace Cloud.  All Rights Reserved";
COMPANY = "Rackspace Cloud"
DESCRIPTION = "C#.NET Agent for Windows Virtual Machines"
CLR_VERSION = 'v3.5'
RELEASE_BUILD_NUMBER = "1.0.31.0"

#Paths
SLN_FILE = File.join(ABSOLUTE_PATH,'src','WindowsConfigurationAgent.sln')
AGENT_UNIT_TEST_DLL = File.join(ABSOLUTE_PATH,'src','Rackspace.Cloud.Server.Agent.Specs','bin',COMPILE_TARGET,'Rackspace.Cloud.Server.Agent.Specs.dll')
DIFFIEHELLMAN_UNIT_TEST_DLL = File.join(ABSOLUTE_PATH,'src','Rackspace.Cloud.Server.DiffieHellman.Specs','bin',COMPILE_TARGET,'Rackspace.Cloud.Server.DiffieHellman.Specs.dll')
AGENT_SERVICE_DIR = File.join(ABSOLUTE_PATH,'src','Rackspace.Cloud.Server.Agent.Service','bin','Release')
UPDATE_SERVICE_DIR = File.join(ABSOLUTE_PATH,'src','Rackspace.Cloud.Server.Agent.UpdaterService','bin','Release')
BUILDS_DIR = File.join(ABSOLUTE_PATH,'builds')

# NOTE(jkoelker) to build on Windows .NET
#NUNIT_CMD_EXE = File.join(ABSOLUTE_PATH,'lib','nunit','nunit-console.exe')
#FRAMEWORK_DIR = File.join(ENV['windir'].dup, 'Microsoft.NET', 'Framework', CLR_VERSION)
#MSBUILD_EXE = File.join(FRAMEWORK_DIR, 'msbuild.exe')
#NUNIT_OPTION_FLAG = '/'

# NOTE(jkoelker) to build on mono make sure nunit is installed and uncomment
MSBUILD_EXE = 'xbuild'
NUNIT_CMD_EXE = 'nunit-console'
NUNIT_OPTION_FLAG = '-'