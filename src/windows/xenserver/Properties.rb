#Paths
MSBUILD_DIR = "#{ENV["SystemRoot"]}\\Microsoft.Net\\Framework\\v3.5\\"
SOURCE_DIR = "src"
UNITTESTS_RESULTS_DIR = "unittestresults"
RELEASE_DIR = "release"
AGENT_SERVICE_DIR = "src\\Rackspace.Cloud.Server.Agent.Service\\bin\\Debug"
UPDATE_SERVICE_DIR = "src\\Rackspace.Cloud.Server.Agent.UpdaterService\\bin\\Debug"
WXS_PATH = "src\\Rackspace.Cloud.Server.Agent.Deployer\\Product.wxs"

#General Config
SERVICE = "Rackspace.Cloud.Server.Agent.Service"
RELEASE_PACKAGE_NAME = "release.zip"

#Tools
NUNIT_EXE = "lib\\nunit\\nunit-console.exe"
CANDLE_EXE = "C:\\Program Files\\Windows Installer XML v3\\bin\\candle.exe"
LIGHT_EXE = "C:\\Program Files\\Windows Installer XML v3\\bin\\light.exe"


#Assembly Attributes
RELEASE_VERSION = "1.0"
RELEASE_BUILD_NUMBER = ENV['BUILD_NUMBER'] ||= "28" + ".0"
PRODUCT = "Rackspace Cloud Server Agent"
COPYRIGHT = "Copyright (c) 2009 2010, Rackspace Cloud.  All Rights Reserved";
COMPANY = "Rackspace Cloud"
DESCRIPTION = "C#.NET Agent for Windows Virtual Machines"
