using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoUpdatingPlugin
{
	internal class VersionData : IEquatable<VersionData>, IComparable<VersionData>
	{
		public static readonly VersionData ZERO = new VersionData();

		public List<int> numbers;

		public bool IsValidSemver { get; }
		public int Length => numbers.Count;

		public VersionData()
		{
			IsValidSemver = false;
			numbers = new List<int>(0);
		}

		public VersionData(string versionString)
		{
			if (string.IsNullOrWhiteSpace(versionString))
			{
				return;
			}

			versionString = versionString.Trim();

			if (IsValidVersionString(versionString))
			{
				MatchCollection matches = Regex.Matches(versionString, "\\d+");
				IsValidSemver = true;
				numbers = new List<int>(matches.Count);

				foreach (Match match in matches)
				{
					int parsedNumber = int.Parse(match.Value);
					numbers.Add(parsedNumber > 0 ? parsedNumber : 0);
				}
			}

			return;

		}

		public static bool IsValidVersionString(string versionString) => Regex.IsMatch(versionString, "^v?[0-9][\\d.-_]*[^\\s]*$");

		public int GetIndex(int index)
		{
			return numbers.Count > index ? numbers.ElementAt(index) : 0;
		}

		public override string ToString()
		{
			if (numbers.Count == 0)
			{
				return "0";
			}
			else
			{
				return this.ToString(numbers.Count);
			}
		}

		public string ToString(int depth)
		{
			StringBuilder s = new StringBuilder();
			for (int i = 0; i < depth - 1; i++)
			{
				s.Append(GetIndex(i));
				s.Append(".");
			}

			s.Append(GetIndex(depth - 1));
			return s.ToString();
		}

		public bool Equals(VersionData other) => CompareVersion(this, other) == 0;

		public override bool Equals(object obj) => Equals(obj as VersionData);

		public override int GetHashCode() => this.IsValidSemver ? 1 : 0;

		public static bool operator ==(VersionData left, VersionData right) => left.Equals(right);

		public static bool operator !=(VersionData left, VersionData right) => !left.Equals(right);

		public static bool operator <(VersionData left, VersionData right) => CompareVersion(left, right) == -1;

		public static bool operator >(VersionData left, VersionData right) => CompareVersion(left, right) == 1;

		public static bool operator <=(VersionData left, VersionData right) => CompareVersion(left, right) != 1;

		public static bool operator >=(VersionData left, VersionData right) => CompareVersion(left, right) != -1;

		public int CompareTo(VersionData other) => CompareVersion(this, other);

		/// <summary>Compares two mod versions</summary>
		/// <returns>
		/// -1 : right more recent (left less than right)<br/>
		/// 0 : identical (left equals right)<br/>
		/// 1 : left more recent (left greater than right)
		/// </returns>
		private static int CompareVersion(VersionData left, VersionData right)
		{
			if (left is null)
			{
				left = VersionData.ZERO;
			}

			if (right is null)
			{
				right = VersionData.ZERO;
			}

			if (left.IsValidSemver != right.IsValidSemver)
			{
				return left.IsValidSemver ? 1 : -1;
			}

			int compareLength = left.Length > right.Length ? left.Length : right.Length;
			for (int i = 0; i < compareLength; ++i)
			{
				int leftNumber = left.GetIndex(i);
				int rightNumber = right.GetIndex(i);

				if (leftNumber > rightNumber)
				{
					return 1;
				}

				if (leftNumber < rightNumber)
				{
					return -1;
				}
			}

			return 0;
		}
	}
}
