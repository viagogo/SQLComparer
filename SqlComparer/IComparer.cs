using System.Collections.Generic;
using SqlComparer.Model;

namespace SqlComparer
{
    public interface IComparer
    {
        IEnumerable<ComparisonResult> Compare(CompareOptions compareOptions, params ComparedEntity[] entities);
    }
}
