using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kenshi_FCS_Browser
{
    public class GameData
    {
		public readonly Dictionary<string, GameDataItem> items;

		public GameData()
		{
			items = new Dictionary<string, GameDataItem>();
		}

		public void AddItem(GameDataItem item)
		{
			items.Add(item.StringId, item);
		}

		public GameDataItem GetItem(string id)
		{
			if (items.TryGetValue(id, out var item))
			{
				return item;
			}
			return null;
		}

		public GameDataItem[] GetItemsOfType(ItemType type)
		{
			return items.Values.Where(item => item.ItemType == type).ToArray();
		}
	}
}
