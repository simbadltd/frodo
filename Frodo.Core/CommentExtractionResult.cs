namespace Frodo.Core
{
    public sealed class CommentExtractionResult
    {
        public string TaskId { get; set; }

        public Activity Activity { get; set; }

        public string Content { get; set; }
    }
}