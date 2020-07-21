using System.Collections.Generic;

namespace Microsoft.CST.OAT.Captures
{
    public class RuleCapture
    {
        public RuleCapture(Rule r, List<ClauseCapture> captures)
        {
            Rule = r;
            Captures = captures;
        }

        public Rule Rule { get; }
        public List<ClauseCapture> Captures { get; }
    }
}
