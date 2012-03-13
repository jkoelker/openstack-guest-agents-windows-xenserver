using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rackspace.Cloud.Server.Agent.Configuration;
using Rackspace.Cloud.Server.Agent.Interfaces;
using Rhino.Mocks;

namespace Rackspace.Cloud.Server.Agent.Specs {
    [TestFixture]
    public class XenStoreSpec {

        private IXenStore xenStore;
        private IExecutableProcess executable;

        [SetUp]
        public void Setup()
        {
            executable = MockRepository.GenerateMock<IExecutableProcess>();
            xenStore = new XenStore(executable);
        }

        [Test]
        public void Should_call_executable_implementation_with_the_right_parameters_when_writing_to_xenstore()
        {
            xenStore.Write("name1", "value1");
            executable.AssertWasCalled(x => x.Run(Constants.XenClientPath, "write data/guest/name1 value1"));
        }

        [Test]
        public void Should_Ignore_MessageKeys_For_Which_The_File_Was_Removed()
        {
            var messageKey = Guid.NewGuid().ToString();
            executable.Expect(process => process.Run(Constants.XenClientPath, "dir " + Constants.WritableDataHostBase)).Return(ExecutableResult(messageKey));
            executable.Expect(
                process => process.Run(Constants.XenClientPath, string.Format("read data/host/{0}", messageKey))).Return(
                    ExecutableResult(string.Format("reading data/host/{0}: The system cannot find the file specified.", messageKey)));
            var commands = xenStore.GetCommands();
            Assert.IsTrue(commands.Count == 0);
        }

        private static ExecutableResult ExecutableResult(string messageKey)
        {
            return new ExecutableResult  { Output = new List<string>{messageKey} };
        }
    }
}
