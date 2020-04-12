using System;
using System.Collections.Generic;
using System.Text;

namespace Kenshi_FCS_Browser
{
	public class Quat
	{
		public float w = 1f;

		public float x;

		public float y;

		public float z;

		public Quat()
		{
		}

		public void @set(float qw, float qx, float qy, float qz)
		{
			this.w = qw;
			this.x = qx;
			this.y = qy;
			this.z = qz;
		}

		public void @set(Quat v)
		{
			this.w = v.w;
			this.x = v.x;
			this.y = v.y;
			this.z = v.z;
		}

		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3}", new object[] { this.w, this.x, this.y, this.z });
		}
	}
}
