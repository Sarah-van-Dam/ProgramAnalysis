using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Analyses;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using Action = Analyses.Analysis.Actions;

[assembly: InternalsVisibleTo("Analyses.Test")]
namespace Analyses.Analysis.BitVector
{
    public abstract class BitVectorFramework : Analysis
    {
        protected Operator JoinOperator;
        protected Direction Direction;
        protected Dictionary<Node, IConstraints> FinalConstraintsForNodes;
        internal Dictionary<Node, IConstraints> Constraints
        {
            get => FinalConstraintsForNodes;
        }
        protected Dictionary<Node, RdResult> AnalysisResult = new Dictionary<Node, RdResult>();

        protected BitVectorFramework(ProgramGraph programGraph)
        {
            _program = programGraph;
            FinalConstraintsForNodes = new Dictionary<Node, IConstraints>();
        }
        public abstract void Kill(Edge edge, IConstraints constraints);

        public abstract void Generate(Edge edge, IConstraints constraints);

        public override void Analyse()
        {
            this.ConstructConstraints();
        }

        private KeyValuePair<Node, IConstraints> GetNextNode(string toNodeName)
        {
            return this.Constraints
                    .FirstOrDefault(x => x.Key.Name == toNodeName);
        }
        private RdResult ConstructConstraintForStartNode(KeyValuePair<Node, IConstraints> startNodeConstraint)
        {
            RdResult result = new RdResult();
            foreach (var hashSet in (startNodeConstraint.Value as ReachingDefinitionConstraints).VariableToPossibleAssignments.Values)
            {
                foreach (var triple in hashSet)
                {
                    result.Add(triple);
                }
            }
            return result;
        }

        private void ApplyKillSet(Edge edge, IConstraints constraintSet, RdResult result)
        {
            this.Kill(edge, constraintSet);
            foreach (var hashSet in (constraintSet as ReachingDefinitionConstraints)
                .VariableToPossibleAssignments)
            {
                result.RemoveWhere(tuple => tuple.variable.Contains(hashSet.Key));
            }
        }

        private void ApplyGenSet(Edge edge, IConstraints constraintSet, RdResult result)
        {
            this.Generate(edge, constraintSet);
            foreach (var hashSet in (constraintSet as ReachingDefinitionConstraints)
                .VariableToPossibleAssignments.Values)
            {
                result.UnionWith(hashSet);
            }
        }

        private void StoreConstraintSet(Node key, RdResult constraintSet)
        {
            Console.WriteLine($"Storing constraint set {constraintSet.AllToString()} on node {key.Name}");
            if (AnalysisResult.Keys.Contains(key))
            {
                RdResult valueToChange = new RdResult();
                AnalysisResult.TryGetValue(key, out valueToChange);
                valueToChange.UnionWith(constraintSet);
                AnalysisResult[key] = valueToChange;
            }
            else
            {
                AnalysisResult.Add(key, constraintSet);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void ConstructConstraints()
        {
            //Locate q_start and construct its constraints
            String nextNode = "q_start";
            var startNodeConstraint =
                this.Constraints
                    .FirstOrDefault(x => x.Key.Name == nextNode);
            var currentConstraintSet = this.ConstructConstraintForStartNode(startNodeConstraint);
            AnalysisResult.Add(startNodeConstraint.Key, currentConstraintSet);

            //For every edge going out of each following node (//TODO: change this when nodes are numbered in order)
            foreach (Edge edge in this._program.Edges.Where( x => x.FromNode.Name == nextNode))
            {
                nextNode = edge.ToNode.Name;
                this.GenerateConstraintsForEdge(edge, nextNode, currentConstraintSet);
            }
            AnalysisResult.DebugPrint();
        }

        private void GenerateConstraintsForEdge(Edge edge, string nextNode, RdResult currentConstraintSet)
        {
            var newConstraintSet = new RdResult(currentConstraintSet);//create a copy *i hope*
            Console.WriteLine($"Traversing edge {edge}");
            var node = this.GetNextNode(nextNode);

            //the killRD
            this.ApplyKillSet(edge, node.Value, newConstraintSet);
            //Console.WriteLine($"After killRD: {newConstraintSet.AllToString()}");

            //the genRD
            this.ApplyGenSet(edge, node.Value, newConstraintSet);
            //Console.WriteLine($"After genRD: {newConstraintSet.AllToString()}");

            //store it
            this.StoreConstraintSet(node.Key, newConstraintSet);
            currentConstraintSet = newConstraintSet;
        }
    }
}

public class RdResult: HashSet<(string variable, string startNode, string endNode)>
{
    public RdResult() : base()
    {
    }

    public RdResult(RdResult currentConstraintSet): base(currentConstraintSet)
    {
    }
}

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

    public static void DebugPrint(this KeyValuePair<Node, IConstraints>  startNodeConstraint)
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

    public static void DebugPrint(this Dictionary<Node, RdResult> analysisResult)
    {
        foreach (var node in analysisResult)
        {
            Console.WriteLine(node.Key);
            Console.WriteLine(node.Value.AllToString());
        }
    }
}

