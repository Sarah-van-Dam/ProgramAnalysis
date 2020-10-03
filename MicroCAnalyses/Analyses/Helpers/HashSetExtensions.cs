using System;
using System.Collections.Generic;
using System.Text;
using Analyses.Analysis;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;

namespace Analyses.Helpers
{
    public static class HashSetExtensions
    {
        public static string AllToString(this HashSet<(string, string, string)> hashset)
        {
            lock (hashset)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in hashset)
                    sb.Append(item);
                return sb.ToString();
            }
        }

        public static void DebugPrint(this KeyValuePair<Node, Constraints<(string, string, string)>>  startNodeConstraint)
        {
            var rdConstraints = (startNodeConstraint.Value as ReachingDefinitionConstraints).VariableToPossibleAssignments;

            //debug print using string
            var str = $"RD({startNodeConstraint.Key.Name}) is a superset of " +
                      "{";
            foreach (var line in rdConstraints)
            {
                str = str + $"{line.Value.AllToString()},";
            }
            str = str.Remove(str.Length - 1) + "}";
            Console.WriteLine(str);
        }
    }
}