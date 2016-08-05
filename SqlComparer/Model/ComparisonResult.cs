using System.Collections.Generic;

namespace SqlComparer.Model
{
    public class ComparisonResult
    {
        public ComparedEntity LeftEntity { get; set; }
        public ComparedEntity RightEntity { get; set; }

        public CompareOptions CompareOptions { get; set; }

        public ComparisonOutcome Outcome { get; set; }

        public Dictionary<int, string> LeftSourceTreeByLine = new Dictionary<int, string>();
        public Dictionary<int, string> RightSourceTreeByLine = new Dictionary<int, string>();

        public HashSet<int> LinesDifferingInLeftEntity { get; set; } = new HashSet<int>();
        public HashSet<int> LinesDifferingInRightEntity { get; set; } = new HashSet<int>();
    }
}
