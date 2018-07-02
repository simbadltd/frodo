using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        private static readonly Regex[] Patterns =
        {
            new Regex("^(?<task>\\S*)\\/(?<activity>\\S*)\\=(?<time>\\S*) (?<comment>.*)", RegexOptions.IgnoreCase),
            new Regex("^(?<task>\\S*)\\/(?<activity>\\S*) (?<comment>.*)", RegexOptions.IgnoreCase),
            new Regex("^(?<task>\\S*)\\=(?<time>\\S*) (?<comment>.*)", RegexOptions.IgnoreCase),
            new Regex("^(?<task>\\S*)\\/(?<activity>\\S*)\\=(?<time>\\S*)", RegexOptions.IgnoreCase),
            new Regex("^(?<task>\\S*)\\/(?<activity>\\S*)", RegexOptions.IgnoreCase),
            new Regex("^(?<task>\\S*)\\=(?<time>\\S*)", RegexOptions.IgnoreCase),
            new Regex("^(?<task>\\S*) (?<comment>.*)", RegexOptions.IgnoreCase),
            new Regex("^(?<task>\\S*)", RegexOptions.IgnoreCase),
        };

        public ICollection<CommentExtractionResult> Extract(User user, string input)
        {
            var result = new List<CommentExtractionResult>();
            var sanitizedInput = input.Trim();
            var rawTasks = sanitizedInput.Split(MultipleTasksDelimiter);

            foreach (var rawTask in rawTasks)
            {
                var match = FindFirstSuitablePattern(rawTask);
                var isMatchNotFound = match == null;
                if (isMatchNotFound) continue;

                var taskId = match.Groups["task"].Value;
                var activity = MapActivity(user, match.Groups["activity"].Value);
                var time = match.Groups["time"].Value;
                var comment = match.Groups["comment"].Value;

                if (string.IsNullOrWhiteSpace(taskId))
                {
                    throw new InvalidOperationException("TaskId cannot be empty.");
                }

                var redefinedTime = ParseTaskTime(time);

                var entry = new CommentExtractionResult
                {
                    TaskId = taskId,
                    Activity = activity,
                    Comment = comment,
                    RedefinedTime = redefinedTime
                };

                result.Add(entry);
            }

            FillCommonCommentIfNeeded(result);

            return result;
        }

        private static Match FindFirstSuitablePattern(string input)
        {
            foreach (var regex in Patterns)
            {
                if (regex.IsMatch(input) == false) continue;

                return regex.Match(input);
            }

            return null;
        }

        private static TaskTime ParseTaskTime(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

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

        private static void FillCommonCommentIfNeeded(List<CommentExtractionResult> result)
        {
            var lastRecord = result.LastOrDefault();
            if (lastRecord == null) return;

            // [kk] Common comment should be comment of the last record
            // e.g.: TEST1;TEST2;TEST3 Common_Comment
            var commonComment = lastRecord.Comment;
            foreach (var record in result)
            {
                if (string.IsNullOrEmpty(record.Comment) == false) continue;

                record.Comment = commonComment;
            }
        }
    }
}