using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlComparer.Model;

namespace SqlComparer.Parsing
{
    internal class ProcedureObjectNameVisitor : TSqlFragmentVisitor
    {
        private readonly ObjectIdentifier _objectIdentifier;

        public ProcedureObjectNameVisitor(ObjectIdentifier objectIdentifier)
        {
            _objectIdentifier = objectIdentifier;
        }

        public override void Visit(CreateProcedureStatement createProcedureStatement)
        {
            ExtractSchemaAndProcedureNameIdentifiers(createProcedureStatement.ProcedureReference);
        }

        public override void Visit(AlterProcedureStatement alterProcedureStatement)
        {
            ExtractSchemaAndProcedureNameIdentifiers(alterProcedureStatement.ProcedureReference);
        }

        private void ExtractSchemaAndProcedureNameIdentifiers(ProcedureReference procedureReference)
        {
            _objectIdentifier.Schema = procedureReference.Name.SchemaIdentifier?.Value;
            _objectIdentifier.Name = procedureReference.Name.BaseIdentifier.Value;
        }
    }
}
