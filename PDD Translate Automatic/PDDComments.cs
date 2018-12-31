using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PDD_Translate_Automatic
{
    class PDDComment
    {
        private int start, finish;

        public PDDComment(int index, int length)
        {
            start = index;
            finish = index + length;
        }

        public bool IsOverlapping(int checkStart, int checkLength)
        {
            int checkFinish = checkStart + checkLength;
            return (checkStart > start && checkStart < finish) // Match starts inside comment
                || (checkFinish > start && checkFinish < finish) // Match ends inside comment
                || (start > checkStart && start < checkFinish); // Comment inside match (should probably be handled better than throwing the whole thing out)
        }
    }

    class PDDComments
    {
        private static readonly Regex ltxCommentRx = new Regex(";.+");
        private static readonly Regex xmlCommentRx = new Regex("<!--.+?-->", RegexOptions.Singleline);
        private static readonly Regex scriptCommentRx1 = new Regex(@"--(?!\[\[).+");
        private static readonly Regex scriptCommentRx2 = new Regex(@"--\[\[.+?\]\]", RegexOptions.Singleline);

        private string fileText;
        private List<PDDComment> comments = new List<PDDComment>();

        public PDDComments(string extension, string setFileText)
        {
            fileText = setFileText;
            if (extension == ".ltx")
                AddComments(ltxCommentRx);
            else if (extension == ".xml")
                AddComments(xmlCommentRx);
            else if (extension == ".script")
            {
                AddComments(scriptCommentRx1);
                AddComments(scriptCommentRx2);
            }
        }

        private void AddComments(Regex pattern)
        {
            foreach (Match match in pattern.Matches(fileText))
                comments.Add(new PDDComment(match.Index, match.Length));
        }

        public bool IsMatchCommented(Match match)
        {
            foreach (PDDComment comment in comments)
                if (comment.IsOverlapping(match.Index, match.Length))
                    return true;
            return false;
        }
    }
}
