using System;
using System.Linq;
using System.Text;

using Laan.Sql.Parser.Entities;

namespace Laan.Sql.Formatter
{
    public class CreateViewStatementFormatter : StatementFormatter<CreateViewStatement>, IStatementFormatter
    {
        public CreateViewStatementFormatter(IIndentable indentable, StringBuilder sql, CreateViewStatement statement)
            : base(indentable, sql, statement)
        {
        }

        public void Execute()
        {
            _sql.AppendFormat("CREATE VIEW {0}\n", _statement.Name);
            _sql.AppendLine("AS");

            if (_statement.SelectBlock != null)
            {
                var formatter = new SelectStatementFormatter(this, _sql, _statement.SelectBlock);
                formatter.Execute();
            }
        }
    }
}
