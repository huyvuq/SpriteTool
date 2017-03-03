#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;

public class SpriteToolMenu {
	[MenuItem("SpriteTool/Create SpriteTool Object")]
	public static void CreateTileMap(){
		GameObject go = new GameObject ("SpriteTool");
		go.AddComponent<SprireMakerGO> ();
	}
}
#endif