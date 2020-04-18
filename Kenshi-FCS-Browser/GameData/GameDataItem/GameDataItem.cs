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

		public SortedList<string, object> data = new SortedList<string, object>();

		public SortedList<string, object> modData = new SortedList<string, object>();

		public SortedList<string, object> lockedData = new SortedList<string, object>();

		public SortedList<string, List<Reference>> references = new SortedList<string, List<Reference>>();

		public SortedList<string, ArrayList> removed = new SortedList<string, ArrayList>();

		public SortedList<string, GameDataInstance> instances = new SortedList<string, GameDataInstance>();


		public GameDataItem.Accessor<int> idata;

		public GameDataItem.Accessor<bool> bdata;

		public GameDataItem.Accessor<float> fdata;

		public GameDataItem.Accessor<string> sdata;

		public object this[string s]
		{
			get
			{
				if (this.lockedData.ContainsKey(s))
				{
					return this.lockedData[s];
				}
				if (!this.modData.ContainsKey(s))
				{
					return this.data[s];
				}
				return this.modData[s];
			}
			set
			{
				if (!this.data.ContainsKey(s) || !value.Equals(this.data[s]))
				{
					this.modData[s] = value;
				}
				else if (this.modData.ContainsKey(s))
				{
					this.modData.Remove(s);
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
		public int RefCount { get => references.Sum(pair => pair.Value.Count); }

		public GameDataItem(ItemType itemType, string itemId)
        {
            this.ItemType = itemType;
            this.StringId = itemId;
			this.SetupAccessors();
		}

		public SortedList<string, object> GetListForModMode(ModMode mode)
		{
			if (mode == ModMode.BASE)
			{
				return data;
			}
			else if (mode == ModMode.ACTIVE)
			{
				return modData;
			}
			else if (mode == ModMode.LOCKED)
			{
				return lockedData;
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
			foreach (KeyValuePair<string, object> modDatum in this.modData)
			{
				this.data[modDatum.Key] = modDatum.Value;
			}
			this.modData.Clear();
			foreach (KeyValuePair<string, List<Reference>> reference in this.references)
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
			this.removed.Clear();
			List<string> strs = new List<string>();
			foreach (KeyValuePair<string, GameDataInstance> instance in this.instances)
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
				this.instances.Remove(str);
			}
		}

		public void RemoveRefTargets()
		{
			foreach (string str in this.ReferenceLists())
			{
				foreach (Reference item in this.references[str])
				{
					if (item.item != null)
					{
						item.item.RemoveRef(this);
					}
					item.item = null;
				}
			}
			foreach (GameDataInstance value in this.instances.Values)
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
			if (this.modData.ContainsKey(key) || this.data.ContainsKey(key))
			{
				return true;
			}
			return this.lockedData.ContainsKey(key);
		}

		public void Remove(string key)
		{
			this.modData.Remove(key);
			if (this.baseName == null)
			{
				this.data.Remove(key);
			}
		}

		public Reference GetReference(string section, string id)
		{
			Reference reference;
			if (!this.references.ContainsKey(section))
			{
				this.references.Add(section, new List<Reference>());
			}
			IEnumerator enumerator = this.references[section].GetEnumerator();
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

		public Reference AddReference(string section, string id, int? v0 = null, int? v1 = null, int? v2 = null)
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
					this.removed[section].Remove(reference);
				}
				this.references[section].Add(reference);

				if (v0.HasValue)
				{
					reference.mod.v0 = v0.Value;
				}
				if (v1.HasValue)
				{
					reference.mod.v1 = v1.Value;
				}
				if (v2.HasValue)
				{
					reference.mod.v2 = v2.Value;
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
			if (this.removed.ContainsKey(section))
			{
				IEnumerator enumerator = this.removed[section].GetEnumerator();
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
			foreach (KeyValuePair<string, List<Reference>> reference in this.references)
			{
				yield return reference.Key;
			}
		}

		public GameDataInstance GetInstance(string id)
		{
			if (!this.instances.ContainsKey(id))
			{
				return null;
			}
			return this.instances[id];
		}

		public void RemoveRef(GameDataItem from)
		{
			bool flag = false;
			foreach (string str in from.ReferenceLists())
			{
				foreach (object item in from.references[str])
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
			foreach (GameDataInstance value in from.instances.Values)
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
				if (!this.removed.ContainsKey(section))
				{
					this.removed.Add(section, new ArrayList());
				}
				this.removed[section].Add(r);
			}
			this.references[section].Remove(r);
			this.RefreshState();
		}

		public void RemoveReference(string section, string id)
		{
			this.RemoveReference(section, this.GetReference(section, id));
		}

		private void SetupAccessors()
		{
			this.idata = new Accessor<int>(this);
			this.fdata = new Accessor<float>(this);
			this.bdata = new Accessor<bool>(this);
			this.sdata = new Accessor<string>(this);
		}

		public class Accessor<T>
		{
			private readonly GameDataItem item;

			public T this[string s]
			{
				get
				{
					return (T)this.item[s];
				}
				set
				{
					this.item[s] = value;
				}
			}

			public Accessor(GameDataItem me)
			{
				this.item = me;
			}

			public bool ContainsKey(string key)
			{
				if (!this.item.ContainsKey(key))
				{
					return false;
				}
				return this.item[key] is T;
			}

			public void Remove(string key)
			{
				if (this.ContainsKey(key))
				{
					this.item.Remove(key);
				}
			}
		}
	}
}
