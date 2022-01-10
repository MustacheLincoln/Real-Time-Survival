using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("rangedWeaponEquipped", "roundChambered", "pistolAmmo", "rifleAmmo", "meleeAttackCooldown", "meleeWeaponEquipped", "rangedWeapons", "meleeWeapons", "items", "itemSelected", "caloriesInInventory", "millilitersInInventory")]
	public class ES3UserType_Player : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Player() : base(typeof(Player)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (Player)obj;
			
			writer.WritePropertyByRef("rangedWeaponEquipped", instance.rangedWeaponEquipped);
			writer.WritePrivateField("roundChambered", instance);
			writer.WriteProperty("pistolAmmo", instance.pistolAmmo, ES3Type_int.Instance);
			writer.WriteProperty("rifleAmmo", instance.rifleAmmo, ES3Type_int.Instance);
			writer.WritePrivateField("meleeAttackCooldown", instance);
			writer.WritePropertyByRef("meleeWeaponEquipped", instance.meleeWeaponEquipped);
			writer.WriteProperty("items", instance.items, ES3Internal.ES3TypeMgr.GetES3Type(typeof(System.Collections.Generic.List<Item>)));
			writer.WritePropertyByRef("itemSelected", instance.itemSelected);
			writer.WriteProperty("caloriesInInventory", instance.caloriesInInventory, ES3Type_float.Instance);
			writer.WriteProperty("millilitersInInventory", instance.millilitersInInventory, ES3Type_float.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (Player)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "rangedWeaponEquipped":
						instance.rangedWeaponEquipped = reader.Read<RangedWeapon>();
						break;
					case "roundChambered":
					reader.SetPrivateField("roundChambered", reader.Read<System.Boolean>(), instance);
					break;
					case "pistolAmmo":
						instance.pistolAmmo = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "rifleAmmo":
						instance.rifleAmmo = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "meleeAttackCooldown":
					reader.SetPrivateField("meleeAttackCooldown", reader.Read<System.Single>(), instance);
					break;
					case "meleeWeaponEquipped":
						instance.meleeWeaponEquipped = reader.Read<MeleeWeapon>();
						break;
					case "items":
						instance.items = reader.Read<System.Collections.Generic.List<Item>>();
						break;
					case "itemSelected":
						instance.itemSelected = reader.Read<Item>();
						break;
					case "caloriesInInventory":
						instance.caloriesInInventory = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "millilitersInInventory":
						instance.millilitersInInventory = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_PlayerArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_PlayerArray() : base(typeof(Player[]), ES3UserType_Player.Instance)
		{
			Instance = this;
		}
	}
}