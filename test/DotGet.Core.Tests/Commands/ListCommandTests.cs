using System;

using DotGet.Core.Commands;
using DotGet.Core.Logging;
using DotGet.Core.Resolvers;

using Moq;
using Xunit;

namespace DotGet.Core.Tests.Commands
{
    public class ListCommandTests
    {
        private Mock<ILogger> _mLogger;
        private Mock<Resolver> _mResolver;

        public ListCommandTests()
        {
            _mLogger = new Mock<ILogger>();
            _mResolver = new Mock<Resolver>();
        }

        [Fact]
        public void TestNoToolInstalled()
        {
            _mResolver.Setup(r => r.GetInstalled()).Returns(new string[] { });
            ListCommand listCommand = new ListCommand(new[] { _mResolver.Object }, _mLogger.Object);
            Assert.True(listCommand.Execute());
            _mLogger.Verify(l => l.LogInformation("No .NET Core tool installed"));
        }

        [Fact]
        public void TestToolsInstalled()
        {
           _mResolver.Setup(r => r.GetInstalled()).Returns(new string[] { "" });
            ListCommand listCommand = new ListCommand(new[] { _mResolver.Object }, _mLogger.Object);
            Assert.True(listCommand.Execute());
        }
    }
}
