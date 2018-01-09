using System;

using DotGet.Core.Commands;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

using Moq;
using Xunit;

namespace DotGet.Core.Tests.Commands
{
    public class InstallCommandTests
    {
        private Mock<ILogger> _mLogger;
        private Mock<Resolver> _mResolver;

        public InstallCommandTests()
        {
            _mLogger = new Mock<ILogger>();
            _mResolver = new Mock<Resolver>();
        }

        [Fact]
        public void TestNoResolverReturnsFalse()
        {
            InstallCommand installCommand = new InstallCommand("/", _mLogger.Object);
            Assert.False(installCommand.Execute());
        }

        [Fact]
        public void TestAlreadyInstalledReturnsTrue()
        {
            _mResolver.Setup(r => r.CheckInstalled()).Returns(true);
            InstallCommand installCommand = new InstallCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.True(installCommand.Execute());
        }

        [Fact]
        public void TestNoExistsReturnsFalse()
        {
            _mResolver.Setup(r => r.Exists()).Returns(false);
            InstallCommand installCommand = new InstallCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.False(installCommand.Execute());
        }

        [Fact]
        public void TestNoResolveReturnsFalse()
        {
            _mResolver.Setup(r => r.Exists()).Returns(true);
            _mResolver.Setup(r => r.Resolve()).Returns(false);
            InstallCommand installCommand = new InstallCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.False(installCommand.Execute());
        }

        [Fact]
        public void TestResolveReturnsTrue()
        {
            _mResolver.Setup(r => r.Exists()).Returns(true);
            _mResolver.Setup(r => r.Resolve()).Returns(true);
            InstallCommand installCommand = new InstallCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.True(installCommand.Execute());
        }
    }
}
