using System.Collections.Generic;

namespace Analyses.Analysis.Monotone.DetectionOfSignsAnalysis
{
	/// <summary>
	/// Helper class containing -, 0, + booleans with methods for unioning.
	/// </summary>
	public class Signs
	{
		public bool Negative { get; }
		public bool Zero { get; }
		public bool Positive { get; }

		public Signs() { }
		public Signs(bool negative, bool zero, bool positive)
		{
			Negative = negative;
			Zero = zero;
			Positive = positive;
		}

		public Signs Union(Signs other)
			=> new Signs(Negative || other.Negative, Zero || other.Zero, Positive || other.Positive);

		public List<Signs> ArithmeticPlus(Signs other)
		{
			List<Signs> result = new List<Signs>();
			if (Negative && other.Negative) result.Add(new Signs(true, false, false));
			if (Zero && other.Negative) result.Add(new Signs(true, false, false));
			if (Positive && other.Negative) result.Add(new Signs(true, true, true));
			if (Negative && other.Zero) result.Add(new Signs(true, false, false));
			if (Zero && other.Zero) result.Add(new Signs(false, true, false));
			if (Positive && other.Zero) result.Add(new Signs(false, false, true));
			if (Negative && other.Positive) result.Add(new Signs(true, true, true));
			if (Zero && other.Positive) result.Add(new Signs(false, false, true));
			if (Positive && other.Positive) result.Add(new Signs(false, false, true));
			return result;
		}

		public List<Signs> ArithmeticMinus(Signs other)
		{
			List<Signs> result = new List<Signs>();
			if (Negative && other.Negative) result.Add(new Signs(true, true, true));
			if (Zero && other.Negative) result.Add(new Signs(false, false, true));
			if (Positive && other.Negative) result.Add(new Signs(false, false, true));
			if (Negative && other.Zero) result.Add(new Signs(true, false, false));
			if (Zero && other.Zero) result.Add(new Signs(false, true, false));
			if (Positive && other.Zero) result.Add(new Signs(false, false, true));
			if (Negative && other.Positive) result.Add(new Signs(true, false, false));
			if (Zero && other.Positive) result.Add(new Signs(true, false, false));
			if (Positive && other.Positive) result.Add(new Signs(true, true, true));
			return result;
		}

		public List<Signs> ArithmeticMultiply(Signs other)
		{
			List<Signs> result = new List<Signs>();
			if (Negative && other.Negative) result.Add(new Signs(false, false, true));
			if (Zero && other.Negative) result.Add(new Signs(false, true, false));
			if (Positive && other.Negative) result.Add(new Signs(true, false, false));
			if (Negative && other.Zero) result.Add(new Signs(false, true, false));
			if (Zero && other.Zero) result.Add(new Signs(false, true, false));
			if (Positive && other.Zero) result.Add(new Signs(false, true, false));
			if (Negative && other.Positive) result.Add(new Signs(true, false, false));
			if (Zero && other.Positive) result.Add(new Signs(false, true, false));
			if (Positive && other.Positive) result.Add(new Signs(false, false, true));
			return result;
		}

		public override string ToString()
		{
			List<char> signs = new List<char>();
			if (Negative) signs.Add('-');
			if (Zero) signs.Add('0');
			if (Positive) signs.Add('+');
			return signs.Count > 0 ? $"{{ { string.Join(", ", signs) } }}" : "{ }";
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Signs)) return false;
			return GetHashCode() == (obj as Signs).GetHashCode();
		}
	}
}
