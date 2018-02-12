using System;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;
using System.Threading;

namespace Frodo.Core
{
    public class DefaultTaskCommentParsingLogic : ITaskCommentParsingLogic
    {
        public CommentExtractionResult Extract(User user, string input)
        {
            var result = new CommentExtractionResult();
            if (user.TaskPatterns == null) return result;

            var sanitizedInput = input.Trim();

            foreach (var taskPattern in user.TaskPatterns)
            {
                var regex = new Regex(taskPattern, RegexOptions.IgnoreCase);
                if (regex.IsMatch(sanitizedInput) == false) continue;

                var match = regex.Match(sanitizedInput);

                result.TaskId = match.Groups["task"].Value;

                if (string.IsNullOrWhiteSpace(result.TaskId))
                    throw new InvalidOperationException("TaskId cannot be empty.");

                result.Activity = MapActivity(user, match.Groups["activity"].Value);
                result.Content = match.Groups["content"].Value;
            }

            return result;
        }

        private static Activity MapActivity(User user, string rawActivity)
        {
            if (user.ActivityMap == null) return Activity.Other;

            return user.ActivityMap.ContainsKey(rawActivity) ? user.ActivityMap[rawActivity] : Activity.Other;
        }
    }
}