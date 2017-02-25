using UnityEngine;
using System.Collections;
using UnityEditor;

public class SpriteToolGameObject : MonoBehaviour {

	//For Sprite Slicer
	public int numberOfSprite = 82;
	public int width = 32;
	public int height = 32;
	public int pixelPerUnit = 1;
	public Texture2D texture2D;
	public TextureImporterFormat textureFormat = TextureImporterFormat.AutomaticTruecolor; //Change this attritbute depends on your sprite

	//For GameObject prefab and Animation
	public GameObject sampleGameObject; //Generate new GameObject prefab and Animation based on this sample GameObject
	public RuntimeAnimatorController runtimeAnimator; //RuntiemAnimatorController of the sample GameObject, this is a reference to overide for to-be generated GameObject

}
