using System;
using System.Collections.Generic;
using System.Text;

namespace Analyses.Analysis
{
	public enum AnalysisImplementation
	{
		LiveVariables,
		ReachingDefinitions,
		DangerousVariables,
		FaintVariables,
		DetectionOfSigns
	}
}
