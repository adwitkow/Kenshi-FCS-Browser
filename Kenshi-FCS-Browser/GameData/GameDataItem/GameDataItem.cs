using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kenshi_FCS_Browser
{
    public class GameDataItem
    {
		public int id;
		public GameDataState cachedState;
		public string baseName;
		public string modName;
		public string lockedName;
		public string mod;

		public SortedList<string, object> Data { get; private set; } = new SortedList<string, object>();
		public SortedList<string, object> ModData { get; private set; } = new SortedList<string, object>();
		public SortedList<string, object> LockedData { get; private set; } = new SortedList<string, object>();
		public SortedList<string, List<Reference>> References { get; private set; } = new SortedList<string, List<Reference>>();
		public SortedList<string, ArrayList> Removed { get; private set; } = new SortedList<string, ArrayList>();
		public SortedList<string, GameDataInstance> Instances { get; private set; } = new SortedList<string, GameDataInstance>();

		public object this[string s]
		{
			get
			{
				if (this.LockedData.ContainsKey(s))
				{
					return this.LockedData[s];
				}
				if (!this.ModData.ContainsKey(s))
				{
					return this.Data[s];
				}
				return this.ModData[s];
			}
			set
			{
				if (!this.Data.ContainsKey(s) || !value.Equals(this.Data[s]))
				{
					this.ModData[s] = value;
				}
				else if (this.ModData.ContainsKey(s))
				{
					this.ModData.Remove(s);
				}
				this.RefreshState();
			}
		}
		public string StringId { get; private set; }
        public ItemType ItemType { get; private set; }
		public string Name
		{
			get
			{
				if (this.lockedName != null)
				{
					return this.lockedName;
				}
				if (this.modName == null)
				{
					return this.baseName;
				}
				return this.modName;
			}
			set
			{
				string str;
				if (this.baseName == value)
				{
					str = null;
				}
				else
				{
					str = value;
				}
				this.modName = str;
				this.RefreshState();
			}
		}
		public int RefCount { get => References.Sum(pair => pair.Value.Count); }

		public GameDataItem(ItemType itemType, string itemId)
        {
            this.ItemType = itemType;
            this.StringId = itemId;
		}

		public SortedList<string, object> GetListForModMode(ModMode mode)
		{
			if (mode == ModMode.BASE)
			{
				return Data;
			}
			else if (mode == ModMode.ACTIVE)
			{
				return ModData;
			}
			else if (mode == ModMode.LOCKED)
			{
				return LockedData;
			}
			else
			{
				return null;
			}
		}

		public bool IsTagged(Dictionary<string, bool> tags, string key)
		{
			if (tags == null)
			{
				return true;
			}
			if (!tags.ContainsKey(key))
			{
				return false;
			}
			return tags[key];
		}

		public void Flatten()
		{
			this.baseName = this.Name;
			this.modName = null;
			foreach (KeyValuePair<string, object> modDatum in this.ModData)
			{
				this.Data[modDatum.Key] = modDatum.Value;
			}
			this.ModData.Clear();
			foreach (KeyValuePair<string, List<Reference>> reference in this.References)
			{
				foreach (Reference value in reference.Value)
				{
					if (value.mod == null)
					{
						continue;
					}
					value.original = new TripleInt(value.mod);
					value.mod = null;
				}
			}
			this.Removed.Clear();
			List<string> strs = new List<string>();
			foreach (KeyValuePair<string, GameDataInstance> instance in this.Instances)
			{
				if (instance.Value.cachedState != GameDataState.REMOVED)
				{
					instance.Value.Flatten();
				}
				else
				{
					strs.Add(instance.Key);
				}
			}
			foreach (string str in strs)
			{
				this.Instances.Remove(str);
			}
		}

		public void RemoveRefTargets()
		{
			foreach (string str in this.ReferenceLists())
			{
				foreach (Reference item in this.References[str])
				{
					if (item.item != null)
					{
						item.item.RemoveRef(this);
					}
					item.item = null;
				}
			}
			foreach (GameDataInstance value in this.Instances.Values)
			{
				if (value.resolvedRef != null)
				{
					value.resolvedRef.RemoveRef(this);
				}
				if (value.resolvedStates == null)
				{
					continue;
				}
				foreach (GameDataItem resolvedState in value.resolvedStates)
				{
					if (resolvedState == null)
					{
						continue;
					}
					resolvedState.RemoveRef(this);
				}
			}
		}

		public void RefreshState()
		{
			if (this.cachedState != GameDataState.LOCKED)
			{
				this.cachedState = GameDataState.UNKNOWN;
			}
		}

		public bool ContainsKey(string key)
		{
			if (this.ModData.ContainsKey(key) || this.Data.ContainsKey(key))
			{
				return true;
			}
			return this.LockedData.ContainsKey(key);
		}

		public void Remove(string key)
		{
			this.ModData.Remove(key);
			if (this.baseName == null)
			{
				this.Data.Remove(key);
			}
		}

		public Reference GetReference(string section, string id)
		{
			Reference reference;
			if (!this.References.ContainsKey(section))
			{
				this.References.Add(section, new List<Reference>());
			}
			IEnumerator enumerator = this.References[section].GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Reference current = (Reference)enumerator.Current;
					if (current.itemID != id)
					{
						continue;
					}
					reference = current;
					return reference;
				}
				return null;
			}
			finally
			{
				if (enumerator is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}

		public Reference AddReference(string section, string id, int? x = null, int? y = null, int? z = null)
		{
			Reference reference = this.GetReference(section, id);
			if (reference == null)
			{
				reference = this.GetRemovedReference(section, id);
				if (reference == null)
				{
					reference = new Reference(id, null);
				}
				else
				{
					this.Removed[section].Remove(reference);
				}
				this.References[section].Add(reference);

				if (x.HasValue)
				{
					reference.mod.x = x.Value;
				}
				if (y.HasValue)
				{
					reference.mod.y = y.Value;
				}
				if (z.HasValue)
				{
					reference.mod.z = z.Value;
				}
				if (reference.original != null && reference.original.Equals(reference.mod))
				{
					reference.mod = null;
				}
				this.RefreshState();
			}
			return reference;
		}

		private Reference GetRemovedReference(string section, string id)
		{
			Reference reference;
			if (this.Removed.ContainsKey(section))
			{
				IEnumerator enumerator = this.Removed[section].GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Reference current = (Reference)enumerator.Current;
						if (current.itemID != id)
						{
							continue;
						}
						reference = current;
						return reference;
					}
					return null;
				}
				finally
				{
					if (enumerator is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}
			return null;
		}

		public IEnumerable<string> ReferenceLists()
		{
			foreach (KeyValuePair<string, List<Reference>> reference in this.References)
			{
				yield return reference.Key;
			}
		}

		public GameDataInstance GetInstance(string id)
		{
			if (!this.Instances.ContainsKey(id))
			{
				return null;
			}
			return this.Instances[id];
		}

		public void RemoveRef(GameDataItem from)
		{
			bool flag = false;
			foreach (string str in from.ReferenceLists())
			{
				foreach (object item in from.References[str])
				{
					if (((Reference)item).item != this)
					{
						continue;
					}
					if (!flag)
					{
						flag = true;
					}
					else
					{
						return;
					}
				}
			}
			foreach (GameDataInstance value in from.Instances.Values)
			{
				if (value.resolvedRef == this)
				{
					if (!flag)
					{
						flag = true;
					}
					else
					{
						return;
					}
				}
				if (value.resolvedStates == null)
				{
					continue;
				}
				foreach (object resolvedState in value.resolvedStates)
				{
					if ((GameDataItem)resolvedState != this)
					{
						continue;
					}
					if (!flag)
					{
						flag = true;
					}
					else
					{
						return;
					}
				}
			}
		}

		private void RemoveReference(string section, Reference r)
		{
			if (r == null)
			{
				return;
			}
			if (r.item != null)
			{
				r.item.RemoveRef(this);
			}
			if (r.original != null)
			{
				r.item = null;
				r.mod = Reference.Removed;
				if (!this.Removed.ContainsKey(section))
				{
					this.Removed.Add(section, new ArrayList());
				}
				this.Removed[section].Add(r);
			}
			this.References[section].Remove(r);
			this.RefreshState();
		}

		public void RemoveReference(string section, string id)
		{
			this.RemoveReference(section, this.GetReference(section, id));
		}
	}
}
