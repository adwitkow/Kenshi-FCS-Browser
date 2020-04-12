using System.Collections;

namespace Kenshi_FCS_Browser
{
    public class GameDataInstance : GameDataItem // Why is this called an instance?
    {
		public GameDataItem resolvedRef;

		public ArrayList resolvedStates;

		public GameDataInstance() : base(ItemType.NULL_ITEM, "")
		{
			base.Name = "";
		}
	}
}
