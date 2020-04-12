using System;
using System.Collections.Generic;
using System.Text;

namespace Kenshi_FCS_Browser
{
	public class TripleInt
	{
		public int v0;

		public int v1;

		public int v2;

		public TripleInt(TripleInt v)
		{
			this.v0 = v.v0;
			this.v1 = v.v1;
			this.v2 = v.v2;
		}

		public TripleInt(int i0 = 0, int i1 = 0, int i2 = 0)
		{
			this.v0 = i0;
			this.v1 = i1;
			this.v2 = i2;
		}

		public bool Equals(TripleInt b)
		{
			if (b == null || this.v0 != b.v0 || this.v1 != b.v1)
			{
				return false;
			}
			return this.v2 == b.v2;
		}
	}
}
