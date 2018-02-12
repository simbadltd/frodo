namespace Frodo.Core
{
    public interface ITaskCommentParsingLogic
    {
        CommentExtractionResult Extract(User user, string input);
    }
}