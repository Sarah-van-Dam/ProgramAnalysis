using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        protected Dictionary<Node, IConstraints> FinalConstraintsForNodes;
        internal Dictionary<Node, IConstraints> Constraints
        {
            get => FinalConstraintsForNodes;
        }
        internal Dictionary<Node, AnalysisResult<T>> AnalysisResult = new Dictionary<Node, AnalysisResult<T>>();

        protected BitVectorFramework(ProgramGraph programGraph)
        {
            _program = programGraph;
            FinalConstraintsForNodes = new Dictionary<Node, IConstraints>();
        }
        public abstract void Kill(Edge edge, IConstraints constraints);

        public abstract void Generate(Edge edge, IConstraints constraints);

        public override void Analyse()
        {
            this.SolveConstraints();
        }

        private KeyValuePair<Node, IConstraints> GetNextNode(string toNodeName)
        {
            return this.Constraints
                    .FirstOrDefault(x => x.Key.Name == toNodeName);
        }

        public abstract AnalysisResult<T> ConstructConstraintForStartNode(KeyValuePair<Node, IConstraints> startNodeConstraint);

        public abstract void ApplyKillSet(Edge edge, IConstraints constraintSet, AnalysisResult<T> result);

        public abstract void ApplyGenSet(Edge edge, IConstraints constraintSet, AnalysisResult<T> result);

        public abstract void StoreConstraintSet(Node key, AnalysisResult<T> constraintSet);

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
            AnalysisResult.DebugPrint();
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
    }
}