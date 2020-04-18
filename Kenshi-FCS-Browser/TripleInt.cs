using System;
using System.Collections.Generic;
using System.Text;

namespace Kenshi_FCS_Browser
{
	public class TripleInt
	{
		public int x;

		public int y;

		public int z;

		public TripleInt(TripleInt value)
		{
			this.x = value.x;
			this.y = value.y;
			this.z = value.z;
		}

		public TripleInt(int x = 0, int y = 0, int z = 0)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public bool Equals(TripleInt b)
		{
			if (b == null || this.x != b.x || this.y != b.y)
			{
				return false;
			}
			return this.z == b.z;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			return Equals((TripleInt)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(this.x, this.y, this.z);
		}

		public override string ToString()
		{
			return $"({x}, {y}, {z})";
		}
	}
}
