using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;
using Analyses.Graph;
using Analyses.Helpers;
using Action = Analyses.Analysis.Actions;

[assembly: InternalsVisibleTo("Analyses.Test")]
namespace Analyses.Analysis.BitVector
{
    public abstract class BitVectorFramework<T> : Analysis
    {
        protected Operator JoinOperator;
        protected Direction Direction;
        protected Dictionary<Node, Constraints<T>> FinalConstraintsForNodes;
        internal Dictionary<Node, Constraints<T>> Constraints
        {
            get => FinalConstraintsForNodes;
        }
        internal Dictionary<Node, AnalysisResult<T>> AnalysisResult = new Dictionary<Node, AnalysisResult<T>>();

        protected BitVectorFramework(ProgramGraph programGraph)
        {
            _program = programGraph;
            FinalConstraintsForNodes = new Dictionary<Node, Constraints<T>>();
        }
        public abstract void Kill(Edge edge, Constraints<T> constraints);

        public abstract void Generate(Edge edge, Constraints<T> constraints);

        public override void Analyse()
        {
            this.SolveConstraints();
        }

        private KeyValuePair<Node, Constraints<T>> GetNextNode(string toNodeName)
        {
            return this.Constraints
                    .FirstOrDefault(x => x.Key.Name == toNodeName);
        }

        private AnalysisResult<T> ConstructConstraintForStartNode(KeyValuePair<Node, Constraints<T>> startNodeConstraint)
        {
            AnalysisResult<T> result = new AnalysisResult<T>();
            foreach (var hashSet in (startNodeConstraint.Value.Values))
            {
                foreach (var triple in hashSet)
                {
                    result.Add(triple);
                }
            }
            return result;
        }

        public abstract void ApplyKillSet(Edge edge, Constraints<T> constraintSet, AnalysisResult<T> result);

        private void ApplyGenSet(
            Edge edge,
            Constraints<T> constraintSet,
            AnalysisResult<T> result)
        {
            this.Generate(edge, constraintSet);
            foreach (var hashSet in constraintSet.Values)
            {
                result.UnionWith(hashSet);
            }
        }

        private void StoreConstraintSet(Node key, AnalysisResult<T> constraintSet)
        {
            Console.WriteLine($"Storing constraint set {constraintSet.AllToString()} on node {key.Name}");
            if (this.AnalysisResult.Keys.Contains(key))
            {
                AnalysisResult<T> valueToChange = null;
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
        /// Locates q_start and construct its constraints;
        /// Afterwards for each node traverses every edge and constructs additional constraints
        /// </summary>
        private void SolveConstraints()
        {
            String nextNode = "q_start";
            var startNodeConstraint =
                this.Constraints
                    .FirstOrDefault(x => x.Key.Name == nextNode);
            var currentConstraintSet = this.ConstructConstraintForStartNode(startNodeConstraint);
            AnalysisResult.Add(startNodeConstraint.Key, currentConstraintSet);

            //TODO(mta): change this when nodes are numbered in order
            foreach (Edge edge in this._program.Edges.Where( x => x.FromNode.Name == nextNode))
            {
                nextNode = edge.ToNode.Name;
                this.GenerateConstraintsForEdge(edge, nextNode, currentConstraintSet);
            }
            this.DebugPrint(this.AnalysisResult);
        }

        private void GenerateConstraintsForEdge(Edge edge, string nextNode, AnalysisResult<T> currentConstraintSet)
        {
            var newConstraintSet = new AnalysisResult<T>(currentConstraintSet);//create a copy *i hope*
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

        public abstract void DebugPrint(Dictionary<Node, AnalysisResult<T>> analysisResult);
    }
}