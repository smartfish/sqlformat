using System;

namespace Laan.SQL.Parser
{
    public class CreateTableStatementParser : TableStatementParser
    {
        private const string NOT = "NOT";
        private const string NULL = "NULL";
        private const string ASC = "ASC";
        private const string DESC = "DESC";
        private const string IDENTITY = "IDENTITY";
        private const string COLLATE = "COLLATE";
        private const string DEFAULT = "DEFAULT";

        CreateTableStatement _statement;

        internal CreateTableStatementParser( Tokenizer tokenizer ) : base( tokenizer )
        {
        }

        public override IStatement Execute()
        {
            _statement = new CreateTableStatement();

            _statement.TableName = GetTableName();

            Tokenizer.ExpectToken( Constants.OPEN_BRACKET );
            do
            {
                if ( Tokenizer.TokenEquals( CONSTRAINT ) )
                    ProcessPrimaryKeyConstraint();
                else
                    ProcessFieldDefinition();

            } while ( Tokenizer.TokenEquals( Constants.COMMA ) );

            Tokenizer.ExpectToken( Constants.CLOSE_BRACKET );

            return _statement;
        }

        private SqlType ProcessType()
        {
            string identifier = GetIdentifier();
            SqlType result = new SqlType( identifier );

            if ( Tokenizer.TokenEquals( Constants.OPEN_BRACKET ) )
            {
                string token = CurrentToken;
                ReadNextToken();
                result.Length = Int32.Parse( token );

                if ( Tokenizer.TokenEquals( Constants.COMMA ) )
                {
                    result.Scale = Int32.Parse( CurrentToken );
                    ReadNextToken();
                }

                Tokenizer.ExpectToken( Constants.CLOSE_BRACKET );
            }
            return result;
        }

        private void ProcessPrimaryKeyConstraint()
        {
            // this is the name of the constraint - not currenly used!
            string identifier = GetIdentifier();
            string orderBy = "";

            Tokenizer.ExpectTokens( new[] { PRIMARY, KEY, CLUSTERED, Constants.OPEN_BRACKET } );

            string keyFieldName = GetIdentifier();

            FieldDefinition keyField = _statement.Fields.FindByName( keyFieldName );
            if ( keyField != null )
                keyField.IsPrimaryKey = true;

            string token = CurrentToken;
            if ( Tokenizer.TokenEquals( ASC ) || Tokenizer.TokenEquals( DESC ) )
                orderBy = token;

            Tokenizer.ExpectToken( Constants.CLOSE_BRACKET );
        }

        private void ProcessFieldDefinition()
        {
            Nullability nullability = Nullability.Nullable;
            bool isPrimaryKey = false;
            Identity identity = null;

            string fieldName = GetIdentifier();
            SqlType type = ProcessType();

            if ( Tokenizer.TokenEquals( IDENTITY ) )
            {
                identity = ProcessIdentity();
            }

            if ( Tokenizer.TokenEquals( COLLATE ) )
            {
                type.Collation = CurrentToken;
                ReadNextToken();
            }

            if ( Tokenizer.TokenEquals( NULL ) )
            {
                nullability = Nullability.Nullable;
            }

            if ( Tokenizer.TokenEquals( NOT ) )
            {
                Tokenizer.ExpectToken( NULL );
                nullability = Nullability.NotNullable;
            }

            if ( Tokenizer.TokenEquals( IDENTITY ) )
            {
                identity = ProcessIdentity();
            }

            if ( Tokenizer.TokenEquals( PRIMARY ) )
            {
                Tokenizer.ExpectToken( KEY );
                nullability = Nullability.NotNullable;
                isPrimaryKey = true;
            }

            if ( Tokenizer.TokenEquals( CONSTRAINT ) )
            {
                // TODO: process column constraint
                string name = GetIdentifier();
                Tokenizer.ExpectToken( DEFAULT );
                Expression expression = ProcessExpression();
                string defaultValue = expression.Value;
            }

            if ( Tokenizer.TokenEquals( DEFAULT ) )
            {
                // TODO: process column constraint
                Expression expression = ProcessExpression();
                string defaultValue = expression.Value;
            }
            _statement.Fields.Add(
                new FieldDefinition()
                {
                    Name = fieldName,
                    Type = type,
                    Nullability = nullability,
                    IsPrimaryKey = isPrimaryKey,
                    Identity = identity
                }
            );
        }

        private Identity ProcessIdentity()
        {
            Identity result = new Identity();

            Tokenizer.ExpectToken( Constants.OPEN_BRACKET );

            result.Start = Int32.Parse( CurrentToken );
            ReadNextToken();

            Tokenizer.ExpectToken( Constants.COMMA );

            result.Increment = Int32.Parse( CurrentToken );
            ReadNextToken();

            Tokenizer.ExpectToken( Constants.CLOSE_BRACKET );

            return result;
        }
    }
}