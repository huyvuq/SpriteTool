/*
 * Created by Huy Vu
 * 2017 all rights reserved
 * 
 * Use this tool if: you are generating a bunch of 2d GameObject
 * with the same animations (with different sprites), and all of them have
 * the same generic sprite sheet style
 * 
 * THIS TOOL IS FREE FOR EVERYONE
 * Feel free to contribute fixes and requests
 * on my GitHub: https://github.com/huyvuq/SpriteTool/
 */
#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[CustomEditor(typeof(SpriteToolGameObject))]
public class SpriteTool : Editor {

	public SpriteToolGameObject spriteToolGO;


	void OnEnable(){
		spriteToolGO = target as SpriteToolGameObject;
	}


	public override void OnInspectorGUI(){

		EditorGUILayout.BeginVertical ();
		spriteToolGO.numberOfSprite = EditorGUILayout.IntField ("Number of Sprites:", spriteToolGO.numberOfSprite);
		spriteToolGO.width = EditorGUILayout.IntField ("Width:", spriteToolGO.width);
		spriteToolGO.height = EditorGUILayout.IntField ("Height:", spriteToolGO.height);
		spriteToolGO.pixelPerUnit = EditorGUILayout.IntField ("Pixels Per Unit:", spriteToolGO.pixelPerUnit);
		spriteToolGO.texture2D = (Texture2D)EditorGUILayout.ObjectField ("Sprite Sheet:", spriteToolGO.texture2D, typeof(Texture2D), false);
		spriteToolGO.sampleGameObject = (GameObject)EditorGUILayout.ObjectField ("Sample GameObject:", spriteToolGO.sampleGameObject, typeof(GameObject), true);
		if(spriteToolGO.sampleGameObject.GetComponent<Animator>().runtimeAnimatorController)
			spriteToolGO.runtimeAnimator = spriteToolGO.sampleGameObject.GetComponent<Animator>().runtimeAnimatorController;
	

		if (GUILayout.Button("Slice Sprites")){
			SliceSprites();
		}

		if (GUILayout.Button("Generate Animations")){
			ReplaceAnimationFrame(spriteToolGO.runtimeAnimator, spriteToolGO.texture2D);
		}

		if (GUILayout.Button("Generate Gameobject Prefab")){
			GenerateGameObjectPrefab(spriteToolGO.sampleGameObject, spriteToolGO.texture2D, 
				(RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/Animation/" + spriteToolGO.texture2D.name + "/" +
					spriteToolGO.texture2D.name + ".controller", typeof(RuntimeAnimatorController)));
		}

		EditorGUILayout.EndVertical ();
	}



	/*Sprite Slicer, use this method to quickly slice sprites 
	 * from a sprite sheets
	 */
	void SliceSprites()
	{

		//sprite slicing part
		Texture2D texture = spriteToolGO.texture2D;
		string path = AssetDatabase.GetAssetPath(texture);
		TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
		ti.isReadable = true;
		ti.spritePixelsPerUnit = spriteToolGO.pixelPerUnit;
		ti.spriteImportMode = SpriteImportMode.None; //reset import mode 
		ti.spriteImportMode = SpriteImportMode.Multiple;
		ti.textureFormat = TextureImporterFormat.AutomaticTruecolor;
		List<SpriteMetaData> newData = new List<SpriteMetaData>();

		int SliceWidth = spriteToolGO.width;
		int SliceHeight = spriteToolGO.height;
		int count = 0;
		for (int j = texture.height; j > 0; j -= SliceHeight)
		{		for (int i = 0; i < texture.width; i += SliceWidth)
			{

				if (count >= spriteToolGO.numberOfSprite) break;
				SpriteMetaData smd = new SpriteMetaData();
				smd.pivot = new Vector2(0.5f, 0.5f);
				smd.alignment = 9;
				count ++;
				smd.name = texture.name + "_" + count;
				smd.rect = new Rect(i, j - SliceHeight, SliceWidth, SliceHeight);

				newData.Add(smd);
			}
		}
		ti.spritesheet = newData.ToArray();
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
		Debug.Log("Done Slicing!");
	}

	/*
	 * Use method to replace the sprites
	 * of the animations. 
	 * THE SPRITES NEED TO MATCH SAMPLE GAMEOBJECT SPRITES
	 * SLICE THE SPRITE SHEET BEFORE USING THIS
	 * */

	void ReplaceAnimationFrame(RuntimeAnimatorController runTimeAnimatorSource, Texture2D newTexture){
		string path = AssetDatabase.GetAssetPath(newTexture);
		string animControllerPath = "Assets/Animation/" + newTexture.name + "/";
		if(!Directory.Exists(animControllerPath))
		{    
			//if it doesn't, create it
			Directory.CreateDirectory(animControllerPath);
		}

		//animation part
		Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();

		//override animation controller
		RuntimeAnimatorController newAnimator = spriteToolGO.runtimeAnimator;
		AnimationClip animClipFromPath;
		AnimatorOverrideController overrideController = new AnimatorOverrideController();
		overrideController.runtimeAnimatorController = newAnimator;


		for (int i = 0; i < spriteToolGO.runtimeAnimator.animationClips.Length; i ++){
			AnimationClip animClip = new AnimationClip();
			animClip.frameRate = spriteToolGO.runtimeAnimator.animationClips[i].frameRate;
			AnimationUtility.SetAnimationClipSettings(animClip, AnimationUtility.GetAnimationClipSettings(spriteToolGO.runtimeAnimator.animationClips[i]));

			AnimationClip oldAnimClip = spriteToolGO.runtimeAnimator.animationClips[i];

			foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(oldAnimClip)){				
				ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(oldAnimClip, binding);
				string source2DTexturePath = AssetDatabase.GetAssetPath(keyframes[0].value);
				Sprite[] sourceSprites = AssetDatabase.LoadAllAssetsAtPath(source2DTexturePath).OfType<Sprite>().ToArray();


				for (int x = 0; x < keyframes.Length; x++){
					int tempIndex = getIndex(keyframes[x], sourceSprites);
					if (tempIndex >= 0){
						keyframes[x].value = sprites[tempIndex]; 
					}
				}

				AnimationUtility.SetObjectReferenceCurve(animClip, binding, keyframes);
				string animPath = animControllerPath + newTexture.name + "_" + oldAnimClip.name + ".anim";

				if(File.Exists(animPath))
				{    
					File.Delete(animPath);
				}

				AssetDatabase.CreateAsset(animClip, animPath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				animClipFromPath = (AnimationClip)AssetDatabase.LoadAssetAtPath(animPath, typeof(AnimationClip));
				overrideController[oldAnimClip.name] = animClipFromPath;
			}
		}
		newAnimator = overrideController;

		string animNewControllerPath = animControllerPath + newTexture.name+".controller";
		if(File.Exists(animNewControllerPath))
		{    
			File.Delete(animNewControllerPath);
		}


		AssetDatabase.CreateAsset(newAnimator, animNewControllerPath);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log("Done Generating Animations!");
	}

	/*Generate new GameObject prefab after
	 * createing the animation
	 */

	void GenerateGameObjectPrefab(GameObject go, Texture2D texture, RuntimeAnimatorController animationController){
		Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().ToArray();
		GameObject newGo = Instantiate(go);
		newGo.name = texture.name;

		if (newGo.GetComponent<SpriteRenderer>().sprite != null && sprites != null){
			newGo.GetComponent<SpriteRenderer>().sprite = sprites[0];
		}
		if (animationController != null){
			if (!newGo.GetComponent<Animator>()){
				newGo.AddComponent<Animator>();
			}
			newGo.GetComponent<Animator>().runtimeAnimatorController = animationController;
		}

		string newGameObjectPath = "Assets/Prefabs/" + texture.name +".prefab";

		Object prefab = PrefabUtility.CreateEmptyPrefab(newGameObjectPath);
		PrefabUtility.ReplacePrefab(newGo, prefab, ReplacePrefabOptions.ConnectToPrefab);

		Debug.Log("Done generating new prefab!");
	}

	int getIndex(ObjectReferenceKeyframe key, Sprite[] sprites){
		for (int i = 0; i < sprites.Length; i++){
			if (key.value.name == sprites[i].name){
				return i;
			}
		}
		return -1;
	}
}
#endif