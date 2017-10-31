#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class SprireMakerGO : MonoBehaviour {

	public Texture2D texture2D;
	public int numberOfSprite = 82;
	public int width = 32;
	public int height = 32;
	public int pixelPerUnit = 1;

	public TextureImporterFormat textureFormat = TextureImporterFormat.AutomaticTruecolor;
	public RuntimeAnimatorController runtimeAnimator;
	public GameObject sampleGameObject;


	//The codes below are deprecated, do not use
	//This is our list we want to use to represent our class as an array.
	public List<AnimationSwap> animationSwap = new List<AnimationSwap>(1);

	void AddNew(){
		//Add a new index position to the end of our list
		animationSwap.Add(new AnimationSwap());
	}

	void Remove(int index){
		//Remove an index position from our list at a point in our list array
		animationSwap.RemoveAt(index);
	}
}

//deprecated, do not use
[System.Serializable]
public class AnimationSwap{
	public string animatioName;
	public int startFrame;
	public int endFrame;
	public int framePerSecond;
}
#endif