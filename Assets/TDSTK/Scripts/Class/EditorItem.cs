using UnityEngine;
using System.Collections;

using TDSTK;

namespace TDSTK{
	
	[System.Serializable]
	public class Item{
		public int ID=-1;
		public string name;
		public Sprite icon;
		
		public Item(int id=-1, string n="", Sprite ic=null){
			ID=id;	name=n;	icon=ic;
		}
	}
	
}
