using System;
using System.Collections.Generic;
using System.Text;

namespace Kenshi_FCS_Browser
{
	public enum GameDataState
	{
		UNKNOWN,
		INVALID,
		ORIGINAL,
		OWNED,
		MODIFIED,
		LOCKED,
		REMOVED,
		LOCKED_REMOVED
	}
}
