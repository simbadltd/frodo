using System.Linq;
using Frodo.Core;
using Frodo.Tests.Utils;
using NUnit.Framework;

namespace Frodo.Tests.Core
{
    [TestFixture]
    public class DefaultTaskCommentParsingLogicTests
    {
        private const string DefaultComment = "Default Task Comment";

        private static object[] _parseLogicCases =
        {
            new object[]
            {
                $"{UserMock.TestTaskAlias}",
                UserMock.TestTaskAlias,
                Activity.Other,
                string.Empty
            },
            new object[]
            {
                $"{UserMock.TestTaskAlias}/{UserMock.AnalysisActivityAlias}",
                UserMock.TestTaskAlias,
                Activity.Analysis,
                string.Empty
            },
            new object[]
            {
                $"{UserMock.TestTaskAlias} {DefaultComment}",
                UserMock.TestTaskAlias,
                Activity.Other,
                DefaultComment
            },
            new object[]
            {
                $"{UserMock.TestTaskAlias}/{UserMock.AnalysisActivityAlias} {DefaultComment}",
                UserMock.TestTaskAlias,
                Activity.Analysis,
                DefaultComment
            },
            new object[]
            {
                $"{UserMock.TestTaskAlias}/{UserMock.AnalysisActivityAlias}=40h {DefaultComment}",
                UserMock.TestTaskAlias,
                Activity.Analysis,
                DefaultComment
            },
            new object[]
            {
                $"{UserMock.TestTaskAlias}=40h {DefaultComment}",
                UserMock.TestTaskAlias,
                Activity.Other,
                DefaultComment
            },
            new object[]
            {
                $"{UserMock.TestTaskAlias}=40h",
                UserMock.TestTaskAlias,
                Activity.Other,
                string.Empty
            },
        };

        private DefaultTaskCommentParsingLogic _target;

        private User _user;

        [SetUp]
        public void Setup()
        {
            _target = new DefaultTaskCommentParsingLogic();
            _user = new User().Mock();
        }

        [Test]
        [TestCaseSource(nameof(_parseLogicCases))]
        public void ShouldParseComment(string input, string expectedTaskId, Activity expectedActivity, string expectedComment)
        {
            // Act
            var result = _target.Extract(_user, input);

            // Assert
            Assert.That(result.Count == 1);

            var resultEntry = result.Single();
            Assert.AreEqual(expectedTaskId, resultEntry.TaskId);
            Assert.AreEqual(expectedActivity, resultEntry.Activity);
            Assert.AreEqual(expectedComment, resultEntry.Comment);
        }

        [Test]
        [TestCase("TEST1;TEST2")]
        [TestCase("TEST1;TEST2 Comment")]
        public void ShouldParseMultipleTasks(string input)
        {
            // Act
            var result = _target.Extract(_user, input).ToArray();

            // Assert
            Assert.That(result.Length == 2);

            Assert.AreEqual(result[0].TaskId, "TEST1");
            Assert.AreEqual(result[1].TaskId, "TEST2");
        }

        [Test]
        public void ShouldParseSingleCommentForMultipleTasks()
        {
            // Act
            var result = _target.Extract(_user, "TEST1;TEST2;TEST3 Comment").ToArray();

            // Assert
            Assert.That(result.Length == 3);

            Assert.AreEqual(result[0].TaskId, "TEST1");
            Assert.AreEqual(result[1].TaskId, "TEST2");
            Assert.AreEqual(result[2].TaskId, "TEST3");

            Assert.AreEqual(result[0].Comment, "Comment");
            Assert.AreEqual(result[1].Comment, "Comment");
            Assert.AreEqual(result[2].Comment, "Comment");
        }

        [Test]
        [TestCase("TASK1=60m", TaskTimeType.Minutes, 60D)]
        [TestCase("TASK1=60m Comment", TaskTimeType.Minutes, 60D)]
        [TestCase("TASK3=1h", TaskTimeType.Minutes, 60D)]
        [TestCase("TASK3=1h Comment", TaskTimeType.Minutes, 60D)]
        [TestCase("TASK5=85%", TaskTimeType.Percentage, 85D)]
        [TestCase("TASK5=85% Comment", TaskTimeType.Percentage, 85D)]
        public void ShouldRedefineTimeForSingleTask(string input, TaskTimeType taskTimeType, double value)
        {
            // Act
            var result = _target.Extract(_user, input);

            // Assert
            Assert.That(result.Count == 1);

            var resultEntry = result.Single();
            Assert.NotNull(resultEntry.RedefinedTime);
            Assert.AreEqual(resultEntry.RedefinedTime.Type, taskTimeType);
            Assert.AreEqual(resultEntry.RedefinedTime.Value, value);
        }

        [Test]
        [TestCase("TASK1=60m;TASK2=100m", TaskTimeType.Minutes, 60D, 100D)]
        [TestCase("TASK1=60m;TASK2=100m Comment", TaskTimeType.Minutes, 60D, 100D)]
        [TestCase("TASK3=1h;TASK4=2h", TaskTimeType.Minutes, 60D, 120D)]
        [TestCase("TASK3=1h;TASK4=2h Comment", TaskTimeType.Minutes, 60D, 120D)]
        [TestCase("TASK5=85%;TASK6=15%", TaskTimeType.Percentage, 85D, 15D)]
        [TestCase("TASK5=85%;TASK6=15% Comment", TaskTimeType.Percentage, 85D, 15D)]
        public void ShouldRedefineTimeForMultipleTasks(string input, TaskTimeType taskTimeType, double value1, double value2)
        {
            // Act
            var result = _target.Extract(_user, input).ToArray();

            // Assert
            Assert.That(result.Length == 2);

            Assert.NotNull(result[0].RedefinedTime);
            Assert.AreEqual(result[0].RedefinedTime.Type, taskTimeType);
            Assert.AreEqual(result[0].RedefinedTime.Value, value1);

            Assert.NotNull(result[1].RedefinedTime);
            Assert.AreEqual(result[1].RedefinedTime.Type, taskTimeType);
            Assert.AreEqual(result[1].RedefinedTime.Value, value2);
        }
    }
}