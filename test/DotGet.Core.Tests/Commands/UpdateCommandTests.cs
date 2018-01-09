using System;

using DotGet.Core.Commands;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

using Moq;
using Xunit;

namespace DotGet.Core.Tests.Commands
{
    public class UpdateCommandTests
    {
        private Mock<ILogger> _mLogger;
        private Mock<Resolver> _mResolver;

        public UpdateCommandTests()
        {
            _mLogger = new Mock<ILogger>();
            _mResolver = new Mock<Resolver>();
        }

        [Fact]
        public void TestNoResolverReturnsFalse()
        {
            UpdateCommand updateCommand = new UpdateCommand("/", _mLogger.Object);
            Assert.False(updateCommand.Execute());
        }

        [Fact]
        public void TestNotAlreadyInstalledReturnsFalse()
        {
            _mResolver.Setup(r => r.CheckInstalled()).Returns(false);
            UpdateCommand updateCommand = new UpdateCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.False(updateCommand.Execute());
        }

        [Fact]
        public void TestUpdatedReturnsTrue()
        {
            _mResolver.Setup(r => r.CheckInstalled()).Returns(true);
            _mResolver.Setup(r => r.CheckUpdated()).Returns(true);

            UpdateCommand updateCommand = new UpdateCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.True(updateCommand.Execute());
        }

        [Fact]
        public void TestNoResolveReturnsFalse()
        {
            _mResolver.Setup(r => r.CheckInstalled()).Returns(true);
            _mResolver.Setup(r => r.CheckUpdated()).Returns(false);
            _mResolver.Setup(r => r.Resolve()).Returns(false);
            UpdateCommand updateCommand = new UpdateCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.False(updateCommand.Execute());
        }

        [Fact]
        public void TestResolveReturnsTrue()
        {
            _mResolver.Setup(r => r.CheckInstalled()).Returns(true);
            _mResolver.Setup(r => r.CheckUpdated()).Returns(false);
            _mResolver.Setup(r => r.Resolve()).Returns(true);
            UpdateCommand updateCommand = new UpdateCommand(_mResolver.Object, "", _mLogger.Object);
            Assert.True(updateCommand.Execute());
        }
    }
}
