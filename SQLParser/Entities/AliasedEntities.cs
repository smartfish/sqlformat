using System.Collections.Generic;
using System;

namespace Laan.SQL.Parser
{
    public class AliasedEntity : Expression
    {
        public AliasedEntity()
        {
            Alias = new Alias();
        }

        public Alias Alias { get; set; }
    }

    public enum AliasType { Implicit, As, Equals }

    public class Alias : Expression
    {
        public Alias()
        {
            Type = AliasType.Implicit;
        }

        public string Name { get; set; }
        public AliasType Type { get; set; }

        public override string Value
        {
            get
            {
                string format = "";
                switch ( Type )
                {
                    case AliasType.Implicit:
                        format = Name.Length > 0 ? String.Format( " {0}", Name ) : "";
                        break;

                    case AliasType.Equals:
                        format = Name.Length > 0 ? String.Format( " AS {0}", Name ) : "";
                        break;

                    case AliasType.As:
                        format = String.Format( " AS {0}", Name );
                        break;
                }
                return format;
            }
            protected set { base.Value = value; }
        }
    }

}
