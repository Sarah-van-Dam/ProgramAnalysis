using Analyses;
using Analyses.Algorithms;
using Analyses.Analysis;
using Analyses.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using Analyses.Analysis.BitVector;
using Analyses.Analysis.Distributive;
using Analyses.Analysis.Monotone.DetectionOfSignsAnalysis;
using Analyses.Analysis.BitVector.LiveVariablesAnalysis;
using Analyses.Analysis.BitVector.ReachingDefinitionsAnalysis;

namespace Benchmark
{
	public class Benchmark
	{
		private const char HorizontalBorder = '-';
		private const char VerticalBorder = '|';
		private const char CornerBorder = '+';

		private const byte LeftColumnWidth = 18;
		private const byte ResultColumnWidth = 14;

		/// <summary>
		/// Outputs a text-table with analyses and their basic actions needed for each worklist implementation.
		/// </summary>
		/// <param name="programGraph">The program to perform the benchmark on.</param>
		public void Perform(ProgramGraph programGraph)
		{
			var divider = GetDivider();
			Debug.Print(divider);
			Debug.Print(HorizontalBorder + " Analysis ".PadRight(LeftColumnWidth) + HorizontalBorder +
				string.Join(HorizontalBorder, Enum.GetNames(typeof(WorklistImplementation)).Select(name => $" {name} ".PadRight(ResultColumnWidth))));
			Debug.Print(divider);
			StringBuilder rowBuilder = new StringBuilder();
			foreach (AnalysisImplementation analysisImplementation in Enum.GetValues(typeof(AnalysisImplementation)))
			{
				rowBuilder.Append(HorizontalBorder + (' ' + Enum.GetName(typeof(AnalysisImplementation), analysisImplementation) + ' ').PadRight(LeftColumnWidth) +
					HorizontalBorder);
				foreach (WorklistImplementation worklistImplementation in Enum.GetValues(typeof(WorklistImplementation)))
				{
					Analysis analysis = GetAnalysis(analysisImplementation, programGraph, worklistImplementation);
					analysis.Analyse();
					rowBuilder.Append($" {analysis._worklistAlgorithm.BasicActionsNeeded} ".PadRight(ResultColumnWidth) + HorizontalBorder);
					Debug.Print(rowBuilder.ToString());
					rowBuilder.Clear();
				}
			}
			Debug.Print(divider);
		}
		
		/// <summary>
		/// Internal use to get a divider line, ex. +-------+-------+
		/// </summary>
		/// <returns></returns>
		private string GetDivider()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(CornerBorder + new string(HorizontalBorder, LeftColumnWidth));
			var columnsCount = Enum.GetValues(typeof(Direction)).Length;
			var columnAppend = new string(HorizontalBorder, ResultColumnWidth) + CornerBorder;
			for (var column = 0; column < columnsCount; column++)
				builder.Append(columnAppend);

			return builder.ToString();
		}

		/// <summary>
		/// Helper to create an instance of a given Analysis object given the analysis implementation, program and worklist implementation.
		/// </summary>
		/// <param name="analysisImpl"></param>
		/// <param name="programGraph"></param>
		/// <param name="worklistImplementation"></param>
		/// <returns></returns>
		private Analysis GetAnalysis(AnalysisImplementation analysisImpl, ProgramGraph programGraph, WorklistImplementation worklistImplementation)
		{
			switch (analysisImpl)
			{
				case AnalysisImplementation.DangerousVariables:
					return new DangerousVariables(programGraph, worklistImplementation);
				case AnalysisImplementation.DetectionOfSigns:
					return new DetectionOfSigns(programGraph, worklistImplementation);
				case AnalysisImplementation.FaintVariables:
					return new FaintVariables(programGraph, worklistImplementation);
				case AnalysisImplementation.LiveVariables:
					return new LiveVariables(programGraph, worklistImplementation);
				case AnalysisImplementation.ReachingDefinitions:
					return new ReachingDefinitions(programGraph, worklistImplementation);
				default:
					throw new ArgumentException("The specified analysis is not yet listed in this method.", "analysisImpl");
			}
		}

	}
}
