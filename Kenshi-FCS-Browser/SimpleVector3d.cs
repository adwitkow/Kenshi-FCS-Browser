using System;
using System.Collections.Generic;
using System.Text;

namespace Kenshi_FCS_Browser
{
	public class SimpleVector3d
	{
		public float x;

		public float y;

		public float z;

		public SimpleVector3d()
		{
		}

		public void @Set(float a, float b, float c)
		{
			this.x = a;
			this.y = b;
			this.z = c;
		}

		public void @Set(SimpleVector3d v)
		{
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", this.x, this.y, this.z);
		}
	}
}
