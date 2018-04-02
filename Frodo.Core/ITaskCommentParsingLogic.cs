using System.Collections.Generic;

namespace Frodo.Core
{
    public interface ITaskCommentParsingLogic
    {
        ICollection<CommentExtractionResult> Extract(User user, string input);
    }
}