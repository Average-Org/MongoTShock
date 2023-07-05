using Microsoft.Xna.Framework;
using MongoDB.Entities;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent.NetModules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Net;
using Entity = MongoDB.Entities.Entity;

namespace TShockAPI
{
	public class PlayerData : Entity
	{
		public int Account { get; set; }
		public int Health { get; set; } = TShock.ServerSideCharacterConfig.Settings.StartingHealth;
		public int MaxHealth { get; set; } = TShock.ServerSideCharacterConfig.Settings.StartingHealth;
		public int Mana { get; set; } = TShock.ServerSideCharacterConfig.Settings.StartingMana;
		public int MaxMana { get; set; } = TShock.ServerSideCharacterConfig.Settings.StartingMana;
		public bool Exists { get; set; }
		public NetItem[] Inventory { get; set; } = new NetItem[NetItem.MaxInventory];
		public int? ExtraSlot { get; set; }
		public int SpawnX { get; set; } = -1;
		public int SpawnY { get; set; } = -1;
		public int? SkinVariant { get; set; }
		public int? Hair { get; set; }
		public byte HairDye { get; set; }
		public Color? HairColor { get; set; }
		public Color? PantsColor { get; set; }
		public Color? ShirtColor { get; set; }
		public Color? UnderShirtColor { get; set; }
		public Color? ShoeColor { get; set; }
		public bool[] HideVisuals { get; set; }
		public Color? SkinColor { get; set; }
		public Color? EyeColor { get; set; }
		public int QuestsCompleted { get; set; }
		public int UsingBiomeTorches { get; set; }
		public int HappyFunTorchTime { get; set; }
		public int UnlockedBiomeTorches { get; set; }
		public int CurrentLoadoutIndex { get; set; }
		public int AteArtisanBread { get; set; }
		public int UsedAegisCrystal { get; set; }
		public int UsedAegisFruit { get; set; }
		public int UsedArcaneCrystal { get; set; }
		public int UsedGalaxyPearl { get; set; }
		public int UsedGummyWorm { get; set; }
		public int UsedAmbrosia { get; set; }
		public int UnlockedSuperCart { get; set; }
		public int EnabledSuperCart { get; set; }

		public PlayerData(TSPlayer player)
		{
			for (int i = 0; i < NetItem.MaxInventory; i++)
			{
				Inventory[i] = new NetItem();

				var item = TShock.ServerSideCharacterConfig.Settings.StartingInventory.ElementAtOrDefault(i);

				if (item is not null)
					AdjustSlot(i, item);

				this.SaveAsync();
			}
		}

		public PlayerData() { }

		/// <summary>
		/// Stores an item at the specific storage slot
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="netID"></param>
		/// <param name="prefix"></param>
		/// <param name="stack"></param>
		[Obsolete("StoreSlot is deprecated, please use AdjustSlot instead.")]
		public void StoreSlot(int slot, int netID, byte prefix, int stack)
		{
			if (slot > (Inventory.Length - 1)) return; //if the slot is out of range then dont save

			Inventory[slot] = new NetItem(netID, stack, prefix);
		}

		public void AdjustSlot(int slot, NetItem item)
		{
			if (slot > (Inventory.Length - 1)) return; //if the slot is out of range then dont save

			Inventory[slot] = item;
		}

		public void CopyCharacter(TSPlayer player)
		{
			Health = player.TPlayer.statLife > 0 ? player.TPlayer.statLife : 1;
			MaxHealth = player.TPlayer.statLifeMax;
			Mana = player.TPlayer.statMana;
			MaxMana = player.TPlayer.statManaMax;
			if (player.sX > 0 && player.sY > 0)
			{
				SpawnX = player.sX;
				SpawnY = player.sY;
			}
			else
			{
				SpawnX = player.TPlayer.SpawnX;
				SpawnY = player.TPlayer.SpawnY;
			}
			ExtraSlot = player.TPlayer.extraAccessory ? 1 : 0;
			SkinVariant = player.TPlayer.skinVariant;
			Hair = player.TPlayer.hair;
			HairDye = player.TPlayer.hairDye;
			HairColor = player.TPlayer.hairColor;
			PantsColor = player.TPlayer.pantsColor;
			ShirtColor = player.TPlayer.shirtColor;
			UnderShirtColor = player.TPlayer.underShirtColor;
			ShoeColor = player.TPlayer.shoeColor;
			HideVisuals = player.TPlayer.hideVisibleAccessory;
			SkinColor = player.TPlayer.skinColor;
			EyeColor = player.TPlayer.eyeColor;
			QuestsCompleted = player.TPlayer.anglerQuestsFinished;
			UsingBiomeTorches = player.TPlayer.UsingBiomeTorches ? 1 : 0;
			HappyFunTorchTime = player.TPlayer.happyFunTorchTime ? 1 : 0;
			UnlockedBiomeTorches = player.TPlayer.unlockedBiomeTorches ? 1 : 0;
			CurrentLoadoutIndex = player.TPlayer.CurrentLoadoutIndex;

			AteArtisanBread = player.TPlayer.ateArtisanBread ? 1 : 0;
			UsedAegisCrystal = player.TPlayer.usedAegisCrystal ? 1 : 0;
			UsedAegisFruit = player.TPlayer.usedAegisFruit ? 1 : 0;
			UsedArcaneCrystal = player.TPlayer.usedArcaneCrystal ? 1 : 0;
			UsedGalaxyPearl = player.TPlayer.usedGalaxyPearl ? 1 : 0;
			UsedGummyWorm = player.TPlayer.usedGummyWorm ? 1 : 0;
			UsedAmbrosia = player.TPlayer.usedAmbrosia ? 1 : 0;
			UnlockedSuperCart = player.TPlayer.unlockedSuperCart ? 1 : 0;
			EnabledSuperCart = player.TPlayer.enabledSuperCart ? 1 : 0;

			Item[] inventory = player.TPlayer.inventory;
			Item[] armor = player.TPlayer.armor;
			Item[] dye = player.TPlayer.dye;
			Item[] miscEqups = player.TPlayer.miscEquips;
			Item[] miscDyes = player.TPlayer.miscDyes;
			Item[] piggy = player.TPlayer.bank.item;
			Item[] safe = player.TPlayer.bank2.item;
			Item[] forge = player.TPlayer.bank3.item;
			Item[] voidVault = player.TPlayer.bank4.item;
			Item trash = player.TPlayer.trashItem;
			Item[] loadout1Armor = player.TPlayer.Loadouts[0].Armor;
			Item[] loadout1Dye = player.TPlayer.Loadouts[0].Dye;
			Item[] loadout2Armor = player.TPlayer.Loadouts[1].Armor;
			Item[] loadout2Dye = player.TPlayer.Loadouts[1].Dye;
			Item[] loadout3Armor = player.TPlayer.Loadouts[2].Armor;
			Item[] loadout3Dye = player.TPlayer.Loadouts[2].Dye;

			for (int i = 0; i < NetItem.MaxInventory; i++)
			{
				switch (i)
				{
					case int n when n < NetItem.InventoryIndex.Item2:
						{
							Inventory[i] = (NetItem)inventory[i];
							break;
						}
					case int n when n < NetItem.ArmorIndex.Item2:
						{
							var index = i - NetItem.ArmorIndex.Item1;
							Inventory[i] = (NetItem)armor[index];
							break;
						}
					case int n when n < NetItem.DyeIndex.Item2:
						{
							var index = i - NetItem.DyeIndex.Item1;
							Inventory[i] = (NetItem)dye[index];
							break;
						}
					case int n when n < NetItem.MiscEquipIndex.Item2:
						{
							var index = i - NetItem.MiscEquipIndex.Item1;
							Inventory[i] = (NetItem)miscEqups[index];
							break;
						}
					case int n when n < NetItem.MiscDyeIndex.Item2:
						{
							var index = i - NetItem.MiscDyeIndex.Item1;
							Inventory[i] = (NetItem)miscDyes[index];
							break;
						}
					case int n when n < NetItem.PiggyIndex.Item2:
						{
							var index = i - NetItem.PiggyIndex.Item1;
							Inventory[i] = (NetItem)piggy[index];
							break;
						}
					case int n when n < NetItem.SafeIndex.Item2:
						{
							var index = i - NetItem.SafeIndex.Item1;
							Inventory[i] = (NetItem)safe[index];
							break;
						}
					case int n when n < NetItem.TrashIndex.Item2:
						{
							Inventory[i] = (NetItem)trash;
							break;
						}
					case int n when n < NetItem.ForgeIndex.Item2:
						{
							var index = i - NetItem.ForgeIndex.Item1;
							Inventory[i] = (NetItem)forge[index];
							break;
						}
					case int n when n < NetItem.VoidIndex.Item2:
						{
							var index = i - NetItem.VoidIndex.Item1;
							Inventory[i] = (NetItem)voidVault[index];
							break;
						}
					case int n when n < NetItem.Loadout1Armor.Item2:
						{
							var index = i - NetItem.Loadout1Armor.Item1;
							Inventory[i] = (NetItem)loadout1Armor[index];
							break;
						}
					case int n when n < NetItem.Loadout1Dye.Item2:
						{
							var index = i - NetItem.Loadout1Dye.Item1;
							Inventory[i] = (NetItem)loadout1Dye[index];
							break;
						}
					case int n when n < NetItem.Loadout2Armor.Item2:
						{
							var index = i - NetItem.Loadout2Armor.Item1;
							Inventory[i] = (NetItem)loadout2Armor[index];
							break;
						}
					case int n when n < NetItem.Loadout2Dye.Item2:
						{
							var index = i - NetItem.Loadout2Dye.Item1;
							Inventory[i] = (NetItem)loadout2Dye[index];
							break;
						}
					case int n when n < NetItem.Loadout3Armor.Item2:
						{
							var index = i - NetItem.Loadout3Armor.Item1;
							Inventory[i] = (NetItem)loadout3Armor[index];
							break;
						}
					case int n when n < NetItem.Loadout3Dye.Item2:
						{
							var index = i - NetItem.Loadout3Dye.Item1;
							Inventory[i] = (NetItem)loadout3Dye[index];
							break;
						}
				}
			}

		}

		public void RestoreCharacter(TSPlayer player)
		{
			player.TPlayer.statLife = Health;
			player.TPlayer.statLifeMax = MaxHealth;
			player.TPlayer.statMana = MaxMana;
			player.TPlayer.statManaMax = MaxMana;
			player.TPlayer.SpawnX = SpawnX;
			player.TPlayer.SpawnY = SpawnY;
			player.sX = SpawnX;
			player.sY = SpawnY;
			player.TPlayer.hairDye = HairDye;
			player.TPlayer.anglerQuestsFinished = QuestsCompleted;
			player.TPlayer.UsingBiomeTorches = UsingBiomeTorches == 1;
			player.TPlayer.happyFunTorchTime = HappyFunTorchTime == 1;
			player.TPlayer.unlockedBiomeTorches = UnlockedBiomeTorches == 1;
			player.TPlayer.CurrentLoadoutIndex = CurrentLoadoutIndex;
			player.TPlayer.ateArtisanBread = AteArtisanBread == 1;
			player.TPlayer.usedAegisCrystal = UsedAegisCrystal == 1;
			player.TPlayer.usedAegisFruit = UsedAegisFruit == 1;
			player.TPlayer.usedArcaneCrystal = UsedArcaneCrystal == 1;
			player.TPlayer.usedGalaxyPearl = UsedGalaxyPearl == 1;
			player.TPlayer.usedGummyWorm = UsedGummyWorm == 1;
			player.TPlayer.usedAmbrosia = UsedAmbrosia == 1;
			player.TPlayer.unlockedSuperCart = UnlockedSuperCart == 1;
			player.TPlayer.enabledSuperCart = EnabledSuperCart == 1;

			if (ExtraSlot != null)
				player.TPlayer.extraAccessory = ExtraSlot == 1 ? true : false;
			if (SkinVariant != null)
				player.TPlayer.skinVariant = (int)SkinVariant;
			if (Hair != null)
				player.TPlayer.hair = (int)Hair;
			if (HairColor != null)
				player.TPlayer.hairColor = (Color)HairColor;
			if (PantsColor != null)
				player.TPlayer.pantsColor = (Color)PantsColor;
			if (ShirtColor != null)
				player.TPlayer.shirtColor = (Color)ShirtColor;
			if (UnderShirtColor != null)
				player.TPlayer.underShirtColor = (Color)UnderShirtColor;
			if (ShoeColor != null)
				player.TPlayer.shoeColor = (Color)ShoeColor;
			if (SkinColor != null)
				player.TPlayer.skinColor = (Color)SkinColor;
			if (EyeColor != null)
				player.TPlayer.eyeColor = (Color)EyeColor;

			if (HideVisuals != null)
				player.TPlayer.hideVisibleAccessory = HideVisuals;
			else
				player.TPlayer.hideVisibleAccessory = new bool[player.TPlayer.hideVisibleAccessory.Length];


			if (HideVisuals is not null)
				player.TPlayer.hideVisibleAccessory = HideVisuals;
			else
				player.TPlayer.hideVisibleAccessory = new bool[player.TPlayer.hideVisibleAccessory.Length];

			for (int i = 0; i < NetItem.MaxInventory; i++)
			{
				if (i < NetItem.InventoryIndex.Item2)
				{
					//0-58
					player.TPlayer.inventory[i].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.inventory[i].netID != 0)
					{
						player.TPlayer.inventory[i].stack = Inventory[i].Stack;
						player.TPlayer.inventory[i].prefix = Inventory[i].PrefixId;
					}
				}
				else if (i < NetItem.ArmorIndex.Item2)
				{
					//59-78
					var index = i - NetItem.ArmorIndex.Item1;
					player.TPlayer.armor[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.armor[index].netID != 0)
					{
						player.TPlayer.armor[index].stack = Inventory[i].Stack;
						player.TPlayer.armor[index].prefix = (byte)Inventory[i].PrefixId;
					}
				}
				else if (i < NetItem.DyeIndex.Item2)
				{
					//79-88
					var index = i - NetItem.DyeIndex.Item1;
					player.TPlayer.dye[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.dye[index].netID != 0)
					{
						player.TPlayer.dye[index].stack = Inventory[i].Stack;
						player.TPlayer.dye[index].prefix = (byte)Inventory[i].PrefixId;
					}
				}
				else if (i < NetItem.MiscEquipIndex.Item2)
				{
					//89-93
					var index = i - NetItem.MiscEquipIndex.Item1;
					player.TPlayer.miscEquips[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.miscEquips[index].netID != 0)
					{
						player.TPlayer.miscEquips[index].stack = Inventory[i].Stack;
						player.TPlayer.miscEquips[index].prefix = (byte)Inventory[i].PrefixId;
					}
				}
				else if (i < NetItem.MiscDyeIndex.Item2)
				{
					//93-98
					var index = i - NetItem.MiscDyeIndex.Item1;
					player.TPlayer.miscDyes[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.miscDyes[index].netID != 0)
					{
						player.TPlayer.miscDyes[index].stack = Inventory[i].Stack;
						player.TPlayer.miscDyes[index].prefix = (byte)Inventory[i].PrefixId;
					}
				}
				else if (i < NetItem.PiggyIndex.Item2)
				{
					//98-138
					var index = i - NetItem.PiggyIndex.Item1;
					player.TPlayer.bank.item[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.bank.item[index].netID != 0)
					{
						player.TPlayer.bank.item[index].stack = Inventory[i].Stack;
						player.TPlayer.bank.item[index].prefix = (byte)Inventory[i].PrefixId;
					}
				}
				else if (i < NetItem.SafeIndex.Item2)
				{
					//138-178
					var index = i - NetItem.SafeIndex.Item1;
					player.TPlayer.bank2.item[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.bank2.item[index].netID != 0)
					{
						player.TPlayer.bank2.item[index].stack = Inventory[i].Stack;
						player.TPlayer.bank2.item[index].prefix = (byte)Inventory[i].PrefixId;
					}
				}
				else if (i < NetItem.TrashIndex.Item2)
				{
					//179-219
					var index = i - NetItem.TrashIndex.Item1;
					player.TPlayer.trashItem.netDefaults(Inventory[i].NetId);

					if (player.TPlayer.trashItem.netID != 0)
					{
						player.TPlayer.trashItem.stack = Inventory[i].Stack;
						player.TPlayer.trashItem.prefix = (byte)Inventory[i].PrefixId;
					}
				}
				else if (i < NetItem.ForgeIndex.Item2)
				{
					//220
					var index = i - NetItem.ForgeIndex.Item1;
					player.TPlayer.bank3.item[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.bank3.item[index].netID != 0)
					{
						player.TPlayer.bank3.item[index].stack = Inventory[i].Stack;
						player.TPlayer.bank3.item[index].Prefix((byte)Inventory[i].PrefixId);
					}
				}
				else if (i < NetItem.VoidIndex.Item2)
				{
					//260
					var index = i - NetItem.VoidIndex.Item1;
					player.TPlayer.bank4.item[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.bank4.item[index].netID != 0)
					{
						player.TPlayer.bank4.item[index].stack = Inventory[i].Stack;
						player.TPlayer.bank4.item[index].Prefix((byte)Inventory[i].PrefixId);
					}
				}
				else if (i < NetItem.Loadout1Armor.Item2)
				{
					var index = i - NetItem.Loadout1Armor.Item1;
					player.TPlayer.Loadouts[0].Armor[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.Loadouts[0].Armor[index].netID != 0)
					{
						player.TPlayer.Loadouts[0].Armor[index].stack = Inventory[i].Stack;
						player.TPlayer.Loadouts[0].Armor[index].Prefix((byte)Inventory[i].PrefixId);
					}
				}
				else if (i < NetItem.Loadout1Dye.Item2)
				{
					var index = i - NetItem.Loadout1Dye.Item1;
					player.TPlayer.Loadouts[0].Dye[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.Loadouts[0].Dye[index].netID != 0)
					{
						player.TPlayer.Loadouts[0].Dye[index].stack = Inventory[i].Stack;
						player.TPlayer.Loadouts[0].Dye[index].Prefix((byte)Inventory[i].PrefixId);
					}
				}
				else if (i < NetItem.Loadout2Armor.Item2)
				{
					var index = i - NetItem.Loadout2Armor.Item1;
					player.TPlayer.Loadouts[1].Armor[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.Loadouts[1].Armor[index].netID != 0)
					{
						player.TPlayer.Loadouts[1].Armor[index].stack = Inventory[i].Stack;
						player.TPlayer.Loadouts[1].Armor[index].Prefix((byte)Inventory[i].PrefixId);
					}
				}
				else if (i < NetItem.Loadout2Dye.Item2)
				{
					var index = i - NetItem.Loadout2Dye.Item1;
					player.TPlayer.Loadouts[1].Dye[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.Loadouts[1].Dye[index].netID != 0)
					{
						player.TPlayer.Loadouts[1].Dye[index].stack = Inventory[i].Stack;
						player.TPlayer.Loadouts[1].Dye[index].Prefix((byte)Inventory[i].PrefixId);
					}
				}
				else if (i < NetItem.Loadout3Armor.Item2)
				{
					var index = i - NetItem.Loadout3Armor.Item1;
					player.TPlayer.Loadouts[2].Armor[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.Loadouts[2].Armor[index].netID != 0)
					{
						player.TPlayer.Loadouts[2].Armor[index].stack = Inventory[i].Stack;
						player.TPlayer.Loadouts[2].Armor[index].Prefix((byte)Inventory[i].PrefixId);
					}
				}
				else if (i < NetItem.Loadout3Dye.Item2)
				{
					var index = i - NetItem.Loadout3Dye.Item1;
					player.TPlayer.Loadouts[2].Dye[index].netDefaults(Inventory[i].NetId);

					if (player.TPlayer.Loadouts[2].Dye[index].netID != 0)
					{
						player.TPlayer.Loadouts[2].Dye[index].stack = Inventory[i].Stack;
						player.TPlayer.Loadouts[2].Dye[index].Prefix((byte)Inventory[i].PrefixId);
					}
				}
			}

			// Just like in MessageBuffer when the client receives a ContinueConnecting, let's sync the CurrentLoadoutIndex _before_ any of
			// the items.
			// This is sent to everyone BUT this player, and then ONLY this player. When using UUID login, it is too soon for the server to
			// broadcast packets to this client.
			NetMessage.SendData((int)PacketTypes.SyncLoadout, remoteClient: player.Index, number: player.Index, number2: player.TPlayer.CurrentLoadoutIndex);
			NetMessage.SendData((int)PacketTypes.SyncLoadout, ignoreClient: player.Index, number: player.Index, number2: player.TPlayer.CurrentLoadoutIndex);

			float slot = 0f;
			for (int k = 0; k < NetItem.InventorySlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].inventory[k].Name), player.Index, slot, (float)Main.player[player.Index].inventory[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.ArmorSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].armor[k].Name), player.Index, slot, (float)Main.player[player.Index].armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.DyeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].dye[k].Name), player.Index, slot, (float)Main.player[player.Index].dye[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.MiscEquipSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].miscEquips[k].Name), player.Index, slot, (float)Main.player[player.Index].miscEquips[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.MiscDyeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].miscDyes[k].Name), player.Index, slot, (float)Main.player[player.Index].miscDyes[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.PiggySlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.SafeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank2.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank2.item[k].prefix);
				slot++;
			}
			NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].trashItem.Name), player.Index, slot++, (float)Main.player[player.Index].trashItem.prefix);
			for (int k = 0; k < NetItem.ForgeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank3.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank3.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.VoidSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].bank4.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank4.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[0].Armor[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[0].Armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[0].Dye[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[0].Dye[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Armor[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[1].Armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Dye[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[1].Dye[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[2].Armor[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[2].Armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
			{
				NetMessage.SendData(5, -1, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Dye[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[2].Dye[k].prefix);
				slot++;
			}


			NetMessage.SendData(4, -1, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData(42, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData(16, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

			slot = 0f;
			for (int k = 0; k < NetItem.InventorySlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].inventory[k].Name), player.Index, slot, (float)Main.player[player.Index].inventory[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.ArmorSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].armor[k].Name), player.Index, slot, (float)Main.player[player.Index].armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.DyeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].dye[k].Name), player.Index, slot, (float)Main.player[player.Index].dye[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.MiscEquipSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].miscEquips[k].Name), player.Index, slot, (float)Main.player[player.Index].miscEquips[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.MiscDyeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].miscDyes[k].Name), player.Index, slot, (float)Main.player[player.Index].miscDyes[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.PiggySlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.SafeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank2.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank2.item[k].prefix);
				slot++;
			}
			NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].trashItem.Name), player.Index, slot++, (float)Main.player[player.Index].trashItem.prefix);
			for (int k = 0; k < NetItem.ForgeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank3.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank3.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.VoidSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].bank4.item[k].Name), player.Index, slot, (float)Main.player[player.Index].bank4.item[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[0].Armor[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[0].Armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[0].Dye[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[0].Dye[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Armor[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[1].Armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[1].Dye[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[1].Dye[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutArmorSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[2].Armor[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[2].Armor[k].prefix);
				slot++;
			}
			for (int k = 0; k < NetItem.LoadoutDyeSlots; k++)
			{
				NetMessage.SendData(5, player.Index, -1, NetworkText.FromLiteral(Main.player[player.Index].Loadouts[2].Dye[k].Name), player.Index, slot, (float)Main.player[player.Index].Loadouts[2].Dye[k].prefix);
				slot++;
			}



			NetMessage.SendData(4, player.Index, -1, NetworkText.FromLiteral(player.Name), player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData(42, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData(16, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

			for (int k = 0; k < Player.maxBuffs; k++)
			{
				player.TPlayer.buffType[k] = 0;
			}

			/*
			 * The following packets are sent twice because the server will not send a packet to a client
			 * if they have not spawned yet if the remoteclient is -1
			 * This is for when players login via uuid or serverpassword instead of via
			 * the login command.
			 */
			NetMessage.SendData(50, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
			NetMessage.SendData(50, player.Index, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);

			NetMessage.SendData(76, player.Index, -1, NetworkText.Empty, player.Index);
			NetMessage.SendData(76, -1, -1, NetworkText.Empty, player.Index);

			NetMessage.SendData(39, player.Index, -1, NetworkText.Empty, 400);

			if (Main.GameModeInfo.IsJourneyMode)
			{
				var sacrificedItems = TShock.ResearchDatastore.GetSacrificedItems();
				for (int i = 0; i < ItemID.Count; i++)
				{
					var amount = 0;
					if (sacrificedItems.ContainsKey(i))
					{
						amount = sacrificedItems[i];
					}

					var response = NetCreativeUnlocksModule.SerializeItemSacrifice(i, amount);
					NetManager.Instance.SendToClient(response, player.Index);
				}
			}
		}
	}
}
