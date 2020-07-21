using System.Text.RegularExpressions;

namespace Microsoft.CST.OAT
{
    public class RegexCapture : ClauseCapture
    {
        public RegexCapture(Clause clause, Match result, object? state1 = null, object? state2 = null) : base(clause, state1, state2)
        {
            Result = result;
        }

        public Match Result { get; }
    }
}
