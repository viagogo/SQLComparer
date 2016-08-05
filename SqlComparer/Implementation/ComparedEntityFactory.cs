using SqlComparer.Model;
using SqlComparer.Parsing;
using System;
using NLog;

namespace SqlComparer.Implementation
{
    public class ComparedEntityFactory : IComparedEntityFactory
    {
        private readonly ITSqlFragmentFactory _fragmentFactory;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ComparedEntityFactory(ITSqlFragmentFactory fragmentFactory)
        {
            _fragmentFactory = fragmentFactory;
        }

        public ComparedEntity Create(string representation)
        {
            if(representation == null)
            {
                Logger.Error($"{nameof(representation)} is null");
                throw new ArgumentNullException(nameof(representation));
            }

            var parsedFragment = _fragmentFactory.CreateFragment(representation);
            var result = new ComparedEntity
            {
                Tree = parsedFragment.Fragment,
                ParseErrors = parsedFragment.ParseErrors,
                Representation = representation
            };

            var symbolVisitor = new ProcedureObjectNameVisitor(result.Identifier);
            result.Tree.Accept(symbolVisitor);

            return result;
        }
    }
}
