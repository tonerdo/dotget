using System;

using DotGet.Core.Commands;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

using Moq;
using Xunit;

namespace DotGet.Core.Tests.Commands
{
    public class UninstallCommandTests
    {
        private Mock<ILogger> _mLogger;
        private Mock<Resolver> _mResolver;

        public UninstallCommandTests()
        {
            _mLogger = new Mock<ILogger>();
            _mResolver = new Mock<Resolver>();
        }

        [Fact]
        public void TestNoResolverReturnsFalse()
        {
            UninstallCommand uninstallCommand = new UninstallCommand("/", _mLogger.Object);
            Assert.False(uninstallCommand.Execute());
        }

        [Fact]
        public void TestNotAlreadyInstalledReturnsFalse()
        {
            _mResolver.Setup(r => r.CheckInstalled()).Returns(false);
            UninstallCommand uninstallCommand = new UninstallCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.False(uninstallCommand.Execute());
        }

        [Fact]
        public void TestNoRemoveReturnsFalse()
        {
            _mResolver.Setup(r => r.CheckInstalled()).Returns(true);
            _mResolver.Setup(r => r.Remove()).Returns(false);
            UninstallCommand uninstallCommand = new UninstallCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.False(uninstallCommand.Execute());
        }

        [Fact]
        public void TestResolveReturnsTrue()
        {
            _mResolver.Setup(r => r.CheckInstalled()).Returns(true);
            _mResolver.Setup(r => r.Remove()).Returns(true);
            UninstallCommand uninstallCommand = new UninstallCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.True(uninstallCommand.Execute());
        }
    }
}
