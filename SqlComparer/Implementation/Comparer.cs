using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlComparer.Model;

namespace SqlComparer.Implementation
{
    public class Comparer : IComparer
    {
        public IEnumerable<ComparisonResult> Compare(CompareOptions compareOptions, params ComparedEntity[] entities)
        {
            for (var i = 0; i < entities.Length - 1; i++)
            {
                yield return Compare(compareOptions, entities[i], entities[i + 1]);
            }
        }

        private ComparisonResult Compare(CompareOptions compareOptions, ComparedEntity entity1, ComparedEntity entity2)
        {
            var comparisonResult = new ComparisonResult
            {
                LeftEntity = entity1,
                RightEntity = entity2,
                CompareOptions = compareOptions
            };

            var entity1Tokens = entity1.Tree.ScriptTokenStream.AsEnumerable();
            var entity2Tokens = entity2.Tree.ScriptTokenStream.AsEnumerable();

            if (compareOptions.IgnoreWhitespace)
            {
                entity1Tokens = entity1Tokens.Where(x => x.TokenType != TSqlTokenType.WhiteSpace);
                entity2Tokens = entity2Tokens.Where(x => x.TokenType != TSqlTokenType.WhiteSpace);
            }

            if (compareOptions.IgnoreEndOfFile)
            {
                entity1Tokens = entity1Tokens.Where(x => x.TokenType != TSqlTokenType.EndOfFile);
                entity2Tokens = entity2Tokens.Where(x => x.TokenType != TSqlTokenType.EndOfFile);
            }

            // Important: only windows newlines so far. Add others too
            // Newlines are just whitespace but with the newline characters
            if (compareOptions.IgnoreNewLines)
            {
                entity1Tokens = entity1Tokens.Where(x => x.TokenType != TSqlTokenType.WhiteSpace && x.Text != "\r\n");
                entity2Tokens = entity2Tokens.Where(x => x.TokenType != TSqlTokenType.WhiteSpace && x.Text != "\r\n");
            }

            var areEqual = AreEqual(
                entity1Tokens, 
                entity2Tokens,
                comparisonResult.LinesDifferingInLeftEntity, 
                comparisonResult.LinesDifferingInRightEntity,
                compareOptions);

            comparisonResult.Outcome = areEqual ? ComparisonOutcome.Equal : ComparisonOutcome.Different;

            // Re-using the original ScriptTokenStream so we don't use the filtered stream based on compareOptions
            var entity1LineByLine = entity1.Tree.ScriptTokenStream.GroupBy(x => x.Line);
            foreach (var line in entity1LineByLine)
            {
                comparisonResult.LeftSourceTreeByLine.Add(line.Key, string.Join(string.Empty, line.Select(x => x.Text)));
            }

            var entity2LineByLine = entity2.Tree.ScriptTokenStream.GroupBy(x => x.Line);
            foreach (var line in entity2LineByLine)
            {
                comparisonResult.RightSourceTreeByLine.Add(line.Key, string.Join(string.Empty, line.Select(x => x.Text)));
            }

            // Done after the actual line-by-line check to make sure it doesn't get overwritten
            if (string.IsNullOrWhiteSpace(entity1.Representation) || string.IsNullOrWhiteSpace(entity2.Representation))
            {
                comparisonResult.Outcome = ComparisonOutcome.Missing;
            }
            
            return comparisonResult;
        }

        private bool AreEqual(
            IEnumerable<TSqlParserToken> entity1Tokens, 
            IEnumerable<TSqlParserToken> entity2Tokens, 
            HashSet<int> lineNumbersLeft, 
            HashSet<int> lineNumbersRight,
            CompareOptions compareOptions)
        {
            var entity1TokensGroupedByLine = entity1Tokens.GroupBy(x => x.Line).ToList();
            var entity2TokensGroupedByLine = entity2Tokens.GroupBy(x => x.Line).ToList();

            // Get lines
            var entity1Lines = new List<string>(entity1TokensGroupedByLine.Count);
            var entity2Lines = new List<string>(entity2TokensGroupedByLine.Count);
            var entity1LineMap = new Dictionary<int, int>(entity1TokensGroupedByLine.Count);
            var entity2LineMap = new Dictionary<int, int>(entity2TokensGroupedByLine.Count);

            var index = 0;
            foreach (var group in entity1TokensGroupedByLine.OrderBy(g => g.Key))
            {
                entity1Lines.Add(string.Join(string.Empty, group.Select(x => x.Text)).Replace(Environment.NewLine, string.Empty));
                entity1LineMap.Add(index, group.Key);
                index++;
            }
            index = 0;
            foreach (var group in entity2TokensGroupedByLine.OrderBy(g => g.Key))
            {
                entity2Lines.Add(string.Join(string.Empty, group.Select(x => x.Text)).Replace(Environment.NewLine, string.Empty));
                entity2LineMap.Add(index, group.Key);
                index++;
            }

            var results = Diff.DiffText(string.Join(Environment.NewLine, entity1Lines.ToArray()), string.Join(Environment.NewLine, entity2Lines.ToArray()),true,true,true);

            if(results.Count() == 0)
            {
                // The same
                return true;
            }

            if(entity1Lines.Count == 0 || entity2Lines.Count == 0)
            {
                // Missing
                return false;
            }

            // Build line number diffs
            foreach(var diffResult in results)
            {
                for(var i = diffResult.StartA; i < diffResult.StartA + diffResult.deletedA; i++)
                {
                    lineNumbersLeft.Add(entity1LineMap[i]);
                }
                for (var i = diffResult.StartB; i < diffResult.StartB + diffResult.insertedB; i++)
                {
                    lineNumbersRight.Add(entity2LineMap[i]);
                }
            }

            return false;
        }
    }
}