using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Frodo.Common;

namespace Frodo.Core
{
    public sealed class DefaultTaskCommentParsingLogic : ITaskCommentParsingLogic
    {
        private const char MultipleTasksDelimiter = ';';

        private const char TaskInformationDelimiter = '=';

        public ICollection<CommentExtractionResult> Extract(User user, string input)
        {
            var result = new List<CommentExtractionResult>();
            if (user.TaskPatterns == null)
            {
                return result;
            }

            var match = FindFirstSuitablePattern(user, input);
            var isMatchNotFound = match == null;
            if (isMatchNotFound)
            {
                return result;
            }

            var activity = MapActivity(user, match.Groups["activity"].Value);
            var content = match.Groups["content"].Value;
            var taskIds = match.Groups["task"].Value.Split(MultipleTasksDelimiter);

            foreach (var rawTaskId in taskIds)
            {
                var entry = ParseComment(rawTaskId, activity, content);
                result.Add(entry);
            }

            return result;
        }

        private static CommentExtractionResult ParseComment(string rawTaskId, Activity activity, string content)
        {
            if (string.IsNullOrWhiteSpace(rawTaskId))
            {
                throw new InvalidOperationException("TaskId cannot be empty.");
            }

            var taskInformationParts = rawTaskId.Split(TaskInformationDelimiter);

            Debug.Assert(taskInformationParts.Length > 0);
            var taskId = taskInformationParts[0];
            var redefinedTime = taskInformationParts.Length > 1 ? ParseTaskTime(taskInformationParts[1]) : null;

            var entry = new CommentExtractionResult
            {
                TaskId = taskId,
                Activity = activity,
                Comment = content,
                RedefinedTime = redefinedTime
            };
            return entry;
        }

        private static Match FindFirstSuitablePattern(User user, string input)
        {
            var sanitizedInput = input.Trim();

            foreach (var taskPattern in user.TaskPatterns)
            {
                var regex = new Regex(taskPattern, RegexOptions.IgnoreCase);
                if (regex.IsMatch(sanitizedInput) == false) continue;

                return regex.Match(sanitizedInput);
            }

            return null;
        }

        private static TaskTime ParseTaskTime(string raw)
        {
            var sanitizedRaw = raw.Replace(" ", string.Empty).ToLowerInvariant();

            if (sanitizedRaw.EndsWith("%"))
            {
                return new TaskTime
                {
                    Type = TaskTimeType.Percentage,
                    Value = sanitizedRaw.ReplaceAlphaCharacters(string.Empty).ToDouble(),
                };
            }

            if (sanitizedRaw.EndsWith("h"))
            {
                return new TaskTime
                {
                    Type = TaskTimeType.Minutes,
                    Value =
                        sanitizedRaw.ReplaceAlphaCharacters(string.Empty).ToDouble() *
                        60D, // +converting hours to minutes
                };
            }

            if (sanitizedRaw.EndsWith("m"))
            {
                return new TaskTime
                {
                    Type = TaskTimeType.Minutes,
                    Value = sanitizedRaw.ReplaceAlphaCharacters(string.Empty).ToDouble(),
                };
            }

            throw new ArgumentException($"Invalid string for task time: <{raw}>");
        }

        private static Activity MapActivity(User user, string rawActivity)
        {
            if (user.ActivityMap == null) return Activity.Other;

            return user.ActivityMap.ContainsKey(rawActivity) ? user.ActivityMap[rawActivity] : Activity.Other;
        }
    }
}