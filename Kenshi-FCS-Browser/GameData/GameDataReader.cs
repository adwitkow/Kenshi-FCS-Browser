﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kenshi_FCS_Browser
{
	public class GameDataReader : IDisposable
	{
		private byte[] stringByteBuffer = new byte[4096];
		private readonly BinaryReader reader;
		private readonly string fileName;

		public GameDataReader(string fileName)
		{
			this.fileName = fileName;

			var memoryStream = new MemoryStream();
			try
			{
				using Stream stream = File.OpenRead(fileName);
				stream.CopyTo(memoryStream);
			}
			catch (IOException ex)
			{
				// TODO: Handle me
				throw ex;
			}

			memoryStream.Position = 0;

			// TODO: Figure out a way to handle the disposable fields gracefully.
			reader = new BinaryReader(memoryStream);
		}

		public GameData Load(GameData data)
		{
			GameData result;
			if (data == null)
			{
				result = new GameData();
			}
			else
			{
				result = data;
			}

			try
			{
				int num = reader.ReadInt32(); // TODO: Figure out what this is

				// header
				var version = reader.ReadInt32();
				var author = ReadString();
				var description = ReadString();
				var dependencies = new List<string>(ReadString().Split(new char[] { ',' }));
				var referenced = new List<string>(ReadString().Split(new char[] { ',' }));
				// header end

				reader.ReadInt32();
				int itemCount = reader.ReadInt32();
				for (int i = 0; i < itemCount; i++)
				{
					reader.ReadInt32();
					ItemType itemType = (ItemType)reader.ReadInt32();
					int itemId = reader.ReadInt32();
					string str = ReadString();
					string stringId;
					if (num >= 7)
					{
						stringId = ReadString();
					}
					else
					{
						stringId = string.Concat(itemId.ToString(), "-", fileName);
					}

					var item = result.GetItem(stringId);
					bool flag1 = item == null;

					if (item == null)
					{
						item = new GameDataItem(itemType, stringId);
						result.AddItem(item);
					}

					bool flag2 = LoadItem(item, str, ModMode.BASE, num, fileName, flag1);
					if (item.cachedState == GameDataState.REMOVED)
					{
						result.items.Remove(item.StringId);
					}
				}
				return result;
			}
			catch (EndOfStreamException ex)
			{
				// TODO: Handle me
				throw ex;
			}
			catch (IOException ex)
			{
				// TODO: Handle me
				throw ex;
			}
		}

		private bool LoadItem(GameDataItem item, string name, ModMode mode, int fileVersion, string filename, bool newItem)
		{
			int num;
			int num1;
			string str;

			item.baseName = name;
			item.Name = name;
			item.lockedName = name;
			item.modName = name;

			SortedList<string, object> strs = item.GetListForModMode(mode);

			bool flag = false;

			if (fileVersion < 14 && item.ItemType == ItemType.CONSTANTS)
			{
				strs = new SortedList<string, object>();
			}

			Dictionary<string, bool> strs1 = null;

			if (fileVersion >= 15)
			{
				reader.ReadInt32();
			}
			else if (fileVersion >= 11)
			{
				num = reader.ReadInt32();
				if (num > 0 && filename != "gamedata.base")
				{
					strs1 = new Dictionary<string, bool>();
					for (int i = 0; i < num; i++)
					{
						strs1[ReadString()] = reader.ReadBoolean();
					}
				}
			}
			num = reader.ReadInt32();
			for (int j = 0; j < num; j++)
			{
				string str1 = ReadString();
				bool flag1 = reader.ReadBoolean();
				if (item.IsTagged(strs1, str1))
				{
					strs[str1] = flag1;
				}
			}
			num = reader.ReadInt32();
			for (int k = 0; k < num; k++)
			{
				string str2 = ReadString();
				float single = reader.ReadSingle();
				if (item.IsTagged(strs1, str2))
				{
					strs[str2] = single;
				}
			}
			num = reader.ReadInt32();
			for (int l = 0; l < num; l++)
			{
				string str3 = ReadString();
				int num2 = reader.ReadInt32();
				if (item.IsTagged(strs1, str3))
				{
					strs[str3] = num2;
				}
			}
			if (fileVersion > 8)
			{
				num = reader.ReadInt32();
				for (int m = 0; m < num; m++)
				{
					string str4 = ReadString();
					SimpleVector3d _vec = new SimpleVector3d()
					{
						x = reader.ReadSingle(),
						y = reader.ReadSingle(),
						z = reader.ReadSingle()
					};
					if (item.IsTagged(strs1, str4))
					{
						strs[str4] = _vec;
					}
				}
				num = reader.ReadInt32();
				for (int n = 0; n < num; n++)
				{
					string str5 = ReadString();
					Quat _quat = new Quat()
					{
						x = reader.ReadSingle(),
						y = reader.ReadSingle(),
						z = reader.ReadSingle(),
						w = reader.ReadSingle()
					};
					if (item.IsTagged(strs1, str5))
					{
						strs[str5] = _quat;
					}
				}
			}
			num = reader.ReadInt32();
			for (int o = 0; o < num; o++)
			{
				string str6 = ReadString();
				string str7 = ReadString();
				if ((!strs.ContainsKey(str6) || strs[str6] is string) && item.IsTagged(strs1, str6))
				{
					strs[str6] = str7;
				}
			}
			num = reader.ReadInt32();
			for (int p = 0; p < num; p++)
			{
				string str8 = ReadString();
				ReadString();
				if (item.IsTagged(strs1, str8))
				{
					// GameDataFile
				}
			}
			num = reader.ReadInt32();
			for (int q = 0; q < num; q++)
			{
				string str10 = ReadString();
				int num3 = reader.ReadInt32();
				for (int r = 0; r < num3; r++)
				{
					if (fileVersion >= 8)
					{
						string str11 = ReadString();
						TripleInt tripleInt = new TripleInt(0, 0, 0)
						{
							x = reader.ReadInt32()
						};
						if (fileVersion >= 10)
						{
							tripleInt.y = reader.ReadInt32();
							tripleInt.z = reader.ReadInt32();
						}
						if (strs1 == null || strs1.ContainsKey(string.Concat("-ref-", str11)))
						{
							bool flag2 = (strs1 == null || strs1[string.Concat("-ref-", str11)] ? tripleInt.z == 2147483647 : true);
							Reference reference = item.GetReference(str10, str11);
							if (!flag2 || reference != null)
							{
								if (reference == null)
								{
									reference = new Reference(str11, null);
									item.References[str10].Add(reference);
								}
								else if (flag2)
								{
									if (mode != ModMode.BASE)
									{
										item.RemoveReference(str10, str11);
									}
									else
									{
										if (reference.item != null)
										{
											reference.item.RemoveRef(item);
										}
										item.References[str10].Remove(reference);
									}
									tripleInt = Reference.Removed;
								}
								if (mode == ModMode.ACTIVE && item.ItemType == ItemType.DIALOGUE_PACKAGE && tripleInt.y == 100)
								{
									tripleInt.y = 0;
								}
								if (mode == ModMode.BASE)
								{
									reference.original = tripleInt;
								}
								else if (mode == ModMode.ACTIVE)
								{
									reference.mod = tripleInt;
								}
								else if (mode == ModMode.LOCKED)
								{
									reference.locked = tripleInt;
								}
							}
						}
					}
					else
					{
						reader.ReadInt64();
					}
				}
			}
			num = reader.ReadInt32();
			for (int s = 0; s < num; s++)
			{
				if (fileVersion < 15)
				{
					num1 = reader.ReadInt32();
					str = string.Concat(num1.ToString(), "-", filename);
				}
				else
				{
					str = ReadString();
				}
				string str12 = str;
				GameDataInstance instance = item.GetInstance(str12) ?? new GameDataInstance();
				instance["ref"] = (fileVersion < 8 ? "" : ReadString());
				instance["x"] = reader.ReadSingle();
				instance["y"] = reader.ReadSingle();
				instance["z"] = reader.ReadSingle();
				instance["qw"] = reader.ReadSingle();
				instance["qx"] = reader.ReadSingle();
				instance["qy"] = reader.ReadSingle();
				instance["qz"] = reader.ReadSingle();
				if (fileVersion > 6)
				{
					int num4 = reader.ReadInt32();
					if (fileVersion >= 15)
					{
						for (int t = 0; t < num4; t++)
						{
							instance.AddReference("states", ReadString());
						}
					}
					else
					{
						for (int u = 0; u < num4; u++)
						{
							num1 = reader.ReadInt32();
							instance.AddReference("states", string.Concat(num1.ToString(), "-", filename, "-INGAME"));
						}
					}
				}
				if (mode != ModMode.ACTIVE)
				{
					instance.Flatten();
				}
				if (!item.Instances.ContainsKey(str12))
				{
					item.Instances.Add(str12, instance);
				}
				if (string.IsNullOrEmpty((string)instance.Data["ref"]))
				{
					item.Instances[str12].Flatten();
				}
			}
			if (item.ContainsKey("REMOVED"))
			{
				if ((bool)item.Data["REMOVED"])
				{
					item.cachedState = GameDataState.REMOVED;
				}
				strs.Remove("REMOVED");
				item.ModData.Remove("REMOVED");
				item.LockedData.Remove("REMOVED");
				item.RemoveRefTargets();
			}
			else if (mode == ModMode.BASE)
			{
				item.cachedState = GameDataState.ORIGINAL;
			}
			else if (!newItem || mode != ModMode.LOCKED)
			{
				item.RefreshState();
			}
			if (mode == ModMode.ACTIVE && item.baseName != null)
			{
				foreach (KeyValuePair<string, object> datum in item.Data)
				{
					if (!item.ModData.ContainsKey(datum.Key) || !item.ModData[datum.Key].Equals(datum.Value))
					{
						continue;
					}
					item.ModData.Remove(datum.Key);
				}
				foreach (KeyValuePair<string, List<Reference>> keyValuePair in item.References)
				{
					foreach (Reference value in keyValuePair.Value)
					{
						if (value.original == null || !value.original.Equals(value.mod))
						{
							continue;
						}
						value.mod = null;
					}
				}
			}
			return !flag;
		}

		private string ReadString()
		{
			int num = reader.ReadInt32();

			if (num <= 0)
			{
				return string.Empty;
			}

			if (num > stringByteBuffer.Length)
			{
				Array.Resize(ref stringByteBuffer, stringByteBuffer.Length * 2);
			}

			reader.Read(stringByteBuffer, 0, num);

			return Encoding.UTF8.GetString(stringByteBuffer, 0, num);
		}

		public void Dispose()
		{
			reader.BaseStream.Dispose();
			reader.Dispose();
		}
	}
}
