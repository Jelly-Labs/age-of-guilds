using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
	public class Commodity
	{
		public readonly string id;
		public readonly string name;

		private Commodity(string _id, string _name)
		{
			id = _id;
			name = _name;
		}

		public static readonly List<Commodity> AvailableCommodities = new()
		{
			new("fish", "Fish"),
			new("wood", "Wood"),
			new("metal", "Metal")
		};
	}

	public enum CommodityType {
		Fish,
		Wood,
		Metal
    }
}