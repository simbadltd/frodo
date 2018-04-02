using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Frodo.Core;

namespace Frodo.Tests.Utils
{
    public static class UserMock
    {
        public const string TestTaskAlias = "TEST_ALIAS";

        public const string TestTaskId = "TEST1";

        public const string AnalysisActivityAlias = "anl";

        public const string DevelopmentActivityAlias = "dev";

        public const string TestingActivityAlias = "tst";

        public const string CodeReviewActivityAlias = "cr";

        public static User Mock(this User user)
        {
            user.TaskIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {TestTaskAlias, TestTaskId}
            };

            user.ActivityMap = new Dictionary<string, Activity>
            {
                {AnalysisActivityAlias, Activity.Analysis},
                {DevelopmentActivityAlias, Activity.Development},
                {TestingActivityAlias, Activity.Testing},
                {CodeReviewActivityAlias, Activity.CodeReview},
            };

            return user;
        }
    }
}