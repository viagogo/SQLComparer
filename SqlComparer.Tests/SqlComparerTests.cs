using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlComparer.Implementation;
using SqlComparer.Model;
using SqlComparer.Parsing;

namespace SqlComparer.Tests
{
    [TestClass]
    public class SqlComparerTests
    {
        private readonly IComparer _comparer = new Comparer();
        private readonly IComparedEntityFactory _entityFactory = new ComparedEntityFactory(new TSqlFragmentFactory());
        private readonly CompareOptions _compareOptions = new CompareOptions();

        [TestMethod]
        public void SqlComparer_TwoEntities_OneResult()
        {
            var proc1 = GetEntity("Schema1", "Name");
            var proc2 = GetEntity("Schema2", "Name");

            var comparisonResult = _comparer.Compare(_compareOptions, proc1, proc2).ToArray();

            Assert.AreEqual(comparisonResult.Length, 1);
        }

        [TestMethod]
        public void SqlComparer_TwoEntities_DifferentSchemas()
        {
            var proc1 = GetEntity("Schema1", "Name");
            var proc2 = GetEntity("Schema2", "Name");

            var comparisonResult = _comparer.Compare(_compareOptions, proc1, proc2).ToArray();

            Assert.AreEqual(comparisonResult.Single().Outcome, ComparisonOutcome.Different);
        }

        [TestMethod]
        public void SqlComparer_TwoEntities_DifferentNames()
        {
            var proc1 = GetEntity("Schema1", "Name1");
            var proc2 = GetEntity("Schema1", "Name2");

            var comparisonResult = _comparer.Compare(_compareOptions, proc1, proc2).ToArray();

            Assert.AreEqual(comparisonResult.Single().Outcome, ComparisonOutcome.Different);
        }

        [TestMethod]
        public void SqlComparer_TwoEntities_EntirelyIdenticalContent()
        {
            var proc1 = GetEntity("Schema1", "Name");
            var proc2 = GetEntity("Schema1", "Name");

            var comparisonResult = _comparer.Compare(_compareOptions, proc1, proc2).ToArray();

            Assert.AreEqual(comparisonResult.Single().Outcome, ComparisonOutcome.Equal);
            Assert.IsFalse(comparisonResult.Single().LeftEntity.HasParseErrors);
            Assert.IsFalse(comparisonResult.Single().RightEntity.HasParseErrors);
        }

        [TestMethod]
        public void SqlComparer_TwoEntities_IdenticalContent_DifferentWhitespace()
        {
            var proc1 = GetEntity("Schema1", "Name");
            var proc2 = GetEntity("Schema1", "Name", Environment.NewLine);

            _compareOptions.IgnoreWhitespace = true;
            _compareOptions.IgnoreEndOfFile = true;
            _compareOptions.IgnoreNewLines = true;
            var comparisonResult = _comparer.Compare(_compareOptions, proc1, proc2).ToArray();

            Assert.AreEqual(comparisonResult.Single().Outcome, ComparisonOutcome.Equal);
        }

        [TestMethod]
        public void SqlComparer_TwoEntities_DifferentContent()
        {
            var proc1 =
                _entityFactory.Create(
$@"create procedure Schema1.name
as

select * from Users");

            var proc2 =
                _entityFactory.Create(
$@"create procedure Schema1.name
as

select * from Others");

            var comparisonResult = _comparer.Compare(_compareOptions, proc1, proc2).ToArray();
            Assert.AreEqual(comparisonResult.Single().Outcome, ComparisonOutcome.Different);
            Assert.IsFalse(comparisonResult.Single().LeftEntity.HasParseErrors);
            Assert.IsFalse(comparisonResult.Single().RightEntity.HasParseErrors);
            Assert.IsTrue(comparisonResult.Single().LinesDifferingInLeftEntity.Contains(4));
            Assert.IsTrue(comparisonResult.Single().LinesDifferingInRightEntity.Contains(4));
        }

        [TestMethod]
        public void SqlComparer_TwoEntities_DifferentContent_WithExtraLinesInProc2()
        {
            var proc1 = _entityFactory.Create(
$@"create procedure Schema1.name
as

select * from Users");


            var proc2 = _entityFactory.Create(
$@"create procedure Schema1.name
as

select * from Others




");

            var comparisonResult = _comparer.Compare(_compareOptions, proc1, proc2).ToArray();
            Assert.AreEqual(comparisonResult.Single().Outcome, ComparisonOutcome.Different);
            Assert.IsFalse(comparisonResult.Single().LeftEntity.HasParseErrors);
            Assert.IsFalse(comparisonResult.Single().RightEntity.HasParseErrors);
            Assert.IsTrue(comparisonResult.Single().LinesDifferingInLeftEntity.Contains(4));
            Assert.IsTrue(comparisonResult.Single().LinesDifferingInRightEntity.Contains(5));
            Assert.IsTrue(comparisonResult.Single().LinesDifferingInRightEntity.Contains(6));
            Assert.IsTrue(comparisonResult.Single().LinesDifferingInRightEntity.Contains(7));
            Assert.IsTrue(comparisonResult.Single().LinesDifferingInRightEntity.Contains(8));
        }

        [TestMethod]
        public void SqlComparer_WithParseErrors()
        {
            var proc1 = GetEntity("Schema1", "Name");

            var proc2 = _entityFactory.Create(
$@"create procedure Schema1.Name
as

select * fr
");

            var comparisonResult = _comparer.Compare(_compareOptions, proc1, proc2).ToArray();

            Assert.AreEqual(comparisonResult.Single().Outcome, ComparisonOutcome.Different);
            Assert.IsFalse(comparisonResult.Single().LeftEntity.HasParseErrors);
            Assert.IsTrue(comparisonResult.Single().RightEntity.HasParseErrors);
            // TODO compare exact parse error?
        }

        private ComparedEntity GetEntity(string schema, string name, string whitespace = "")
        {
            return
                _entityFactory.Create(
$@"create procedure {schema}.{name}
as

select * from Users{whitespace}");
        }
    }
}