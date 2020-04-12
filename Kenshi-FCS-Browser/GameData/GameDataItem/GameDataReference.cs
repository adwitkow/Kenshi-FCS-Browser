using System;
using System.Collections.Generic;
using System.Text;

namespace Kenshi_FCS_Browser
{
	public class Reference
	{
		private string stringId = "";

		public GameDataItem item;

		public TripleInt original;

		public TripleInt mod;

		public TripleInt locked;

		public static TripleInt Removed;

		public string itemID
		{
			get
			{
				if (this.item == null)
				{
					return this.stringId;
				}
				return this.item.StringId;
			}
		}

		public TripleInt Values
		{
			get
			{
				if (this.locked != null)
				{
					return this.locked;
				}
				if (this.mod == null)
				{
					return this.original;
				}
				return this.mod;
			}
		}

		static Reference()
		{
			Removed = new TripleInt(2147483647, 2147483647, 2147483647);
		}

		public Reference(string id, TripleInt value = null)
		{
			this.stringId = id;
			this.original = value;
		}

		public Reference(GameDataItem item, TripleInt value = null)
		{
			this.item = item;
			this.original = value;
		}
	}
}
