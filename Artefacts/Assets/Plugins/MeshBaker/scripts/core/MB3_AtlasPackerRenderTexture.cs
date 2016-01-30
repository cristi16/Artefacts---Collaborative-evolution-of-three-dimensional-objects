using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using DigitalOpus.MB.Core;

public class MB_TextureCombinerRenderTexture{
	Material mat;
	RenderTexture _destinationTexture;
	Camera myCamera;
	int _padding;
	bool _doRenderAtlas = false;

	Rect[] rs;
	List<MB3_TextureCombiner.MB_TexSet> textureSets;
	int indexOfTexSetToRender;
	Texture2D targTex;
	
	public Texture2D DoRenderAtlas(GameObject gameObject, int width, int height, int padding, Rect[] rss, List<MB3_TextureCombiner.MB_TexSet> textureSetss, int indexOfTexSetToRenders){
		textureSets = textureSetss;
		indexOfTexSetToRender = indexOfTexSetToRenders;
		_padding = padding;
		rs = rss;
		Shader s = Shader.Find ("MeshBaker/AtlasShader");
		if (s == null){
			Debug.LogError ("Could not find shader 'MeshBaker/AtlasShader'. Try reimporting mesh baker");
			return null;
		}
		mat = new Material(s);
		
		_destinationTexture = new RenderTexture(height,width,24);
		_destinationTexture.filterMode = FilterMode.Point;
		
		myCamera = gameObject.GetComponent<Camera>();
		myCamera.orthographic = true;
		myCamera.orthographicSize = height >> 1;
		myCamera.targetTexture = _destinationTexture;
		myCamera.backgroundColor = Color.clear;       //todo get appropriate for atlas
		myCamera.clearFlags = CameraClearFlags.Color;
		
		Transform camTransform = myCamera.GetComponent<Transform>();
		camTransform.localPosition = new Vector3(width >> 1, height >> 1, 3);
		camTransform.localRotation = Quaternion.Euler(0, 180, 180);
		
		_doRenderAtlas = true;
		Debug.Log ("Calling MyCamera.Render");
		myCamera.Render();
		_doRenderAtlas = false;
		
		MB_Utility.Destroy(mat);
		//MB_Utility.Destroy(myCamera);
		MB_Utility.Destroy(_destinationTexture);
		
		Debug.Log ("Finished OnRenderAtlas ");

		Texture2D tempTex = targTex;
		targTex = null;
		return tempTex;
	}
	
	public void OnRenderObject(){
		Debug.Log ("FastRenderAtlas.OnRenderObject num rects=" + rs.Length);
		if (_doRenderAtlas){
			//assett rs must be same length as textureSets;
			for (int i = 0; i < rs.Length; i++){
				MB3_TextureCombiner.MeshBakerMaterialTexture texInfo = textureSets[i].ts[indexOfTexSetToRender];
//				texInfo.t = textureSets[i].ts[indexOfTexSetToRender].t;
//				texInfo.offset.x = 0f;
//				texInfo.offset.y = 0f;
//				texInfo.scale.x = 1f;
//				texInfo.scale.y = 1f;]
				Debug.Log ("Added " + texInfo.t + " to atlas w=" + texInfo.t.width + " h=" + texInfo.t.height + " offset=" + texInfo.offset + " scale=" + texInfo.scale + " rect=" + rs[i]);
				CopyScaledAndTiledToAtlas(texInfo,rs[i],_padding,false,_destinationTexture.width,false);
			}
			
			//Convert the render texture to a Texture2D
			Texture2D tempTexture;
			tempTexture = new Texture2D(_destinationTexture.width, _destinationTexture.height, TextureFormat.ARGB32, true);
			
			int xblocks = _destinationTexture.width / 512;
			int yblocks = _destinationTexture.height / 512;
			if (xblocks == 0 || yblocks == 0)
			{
				RenderTexture.active = _destinationTexture;
				tempTexture.ReadPixels(new Rect(0, 0, _destinationTexture.width, _destinationTexture.height), 0, 0, true);
				RenderTexture.active = null;
			} else {
				// figures that ReadPixels works differently on OpenGL and DirectX, someday this code will break because Unity fixes this bug!
				if (IsOpenGL()){
					for (int x = 0; x < xblocks; x++){
						for (int y = 0; y < yblocks; y++)
						{
							RenderTexture.active = _destinationTexture;
							tempTexture.ReadPixels(new Rect(x * 512, y * 512, 512, 512), x * 512, y * 512, true);
							RenderTexture.active = null;
						}
					}
				} else {
					for (int x = 0; x < xblocks; x++){
						for (int y = 0; y < yblocks; y++){
							RenderTexture.active = _destinationTexture;
							tempTexture.ReadPixels(new Rect(x * 512, _destinationTexture.height - 512 - y * 512, 512, 512), x * 512, y * 512, true);
							RenderTexture.active = null;
						}
					}
				}
			}
			tempTexture.Apply ();
			myCamera.targetTexture = null;
			RenderTexture.active = null;
			
			targTex = tempTexture;
			Debug.Log ("in Render");	
		}
	}
	
	private bool IsOpenGL(){
		var graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;
		return graphicsDeviceVersion.StartsWith("OpenGL");
	}
	
	private void CopyScaledAndTiledToAtlas(MB3_TextureCombiner.MeshBakerMaterialTexture source, Rect rec, int _padding, bool _fixOutOfBoundsUVs, int maxSize, bool isNormalMap){			
		Rect r = rec;
		r.x += _padding;
		r.y += _padding;
		r.width -= _padding * 2;
		r.height -= _padding * 2;
		
		//fill the _padding
		Rect srcPrTex = new Rect();
		Rect targPr = new Rect();
		
		srcPrTex.width = source.scale.x;
		srcPrTex.height = source.scale.y;
		srcPrTex.x = source.offset.x;
		srcPrTex.y = source.offset.y;
		if (_fixOutOfBoundsUVs){
			srcPrTex.width *= source.obUVscale.x;
			srcPrTex.height *= source.obUVscale.y;
			srcPrTex.x += source.obUVoffset.x;
			srcPrTex.y += source.obUVoffset.y;
		}
		Texture tex = source.t;
		
		//main texture
		TextureWrapMode oldTexWrapMode = tex.wrapMode;
		if (srcPrTex.width == 1f && srcPrTex.height == 1f && srcPrTex.x == 0f && srcPrTex.y == 0f){
			//fixes bug where there is a dark line at the edge of the texture
			tex.wrapMode = TextureWrapMode.Clamp;
		} else {
			tex.wrapMode = TextureWrapMode.Repeat;
		}
		Graphics.DrawTexture(r, tex, srcPrTex, 0, 0, 0, 0, mat);
		
		//TODO fill the _padding
		
		Rect srcPr = new Rect();
		
		//top margin
		srcPr.x = srcPrTex.x;
		srcPr.y = srcPrTex.y + 1f - 1f / tex.height;
		srcPr.width = srcPrTex.width;
		srcPr.height = 1f / tex.height;
		targPr.x = rec.x + _padding;
		targPr.y = rec.y;
		targPr.width = r.width;
		targPr.height = _padding;
		Graphics.DrawTexture(targPr, tex, srcPr, 0, 0, 0, 0, mat);
		
		//bot margin
		srcPr.x = srcPrTex.x;
		srcPr.y = srcPrTex.y;
		srcPr.width = srcPrTex.width;
		srcPr.height = 1f / tex.height;
		targPr.x = rec.x + _padding;
		targPr.y = rec.y + r.height + _padding;
		targPr.width = r.width;
		targPr.height = _padding;
		Graphics.DrawTexture(targPr, tex, srcPr, 0, 0, 0, 0, mat);
		
		//left margin
		srcPr.x = srcPrTex.x;
		srcPr.y = srcPrTex.y;
		srcPr.width = 1f / tex.width;
		srcPr.height = srcPrTex.height;
		targPr.x = rec.x;
		targPr.y = rec.y + _padding;
		targPr.width = _padding;
		targPr.height = r.height;
		Graphics.DrawTexture(targPr, tex, srcPr, 0, 0, 0, 0, mat);
		
		//right margin
		srcPr.x = srcPrTex.x + 1f - 1f / tex.width;
		srcPr.y = srcPrTex.y;
		srcPr.width = 1f / tex.width;
		srcPr.height = srcPrTex.height;
		targPr.x = rec.x + r.width + _padding;
		targPr.y = rec.y + _padding;
		targPr.width = _padding;
		targPr.height = r.height;
		Graphics.DrawTexture(targPr, tex, srcPr, 0, 0, 0, 0, mat);
		
		//top left corner
		srcPr.x = srcPrTex.x;
		srcPr.y = srcPrTex.y;
		srcPr.width = 1f / tex.width;
		srcPr.height = 1f / tex.height;
		targPr.x = rec.x; 
		targPr.y = rec.y;
		targPr.width = _padding;
		targPr.height = _padding;
		Graphics.DrawTexture(targPr, tex, srcPr, 0, 0, 0, 0, mat);
		
		tex.wrapMode = oldTexWrapMode;
	}
}

//TODO Test with a tex with a one pixel wide border and different color on each corner
[ExecuteInEditMode]
public class MB3_AtlasPackerRenderTexture : MonoBehaviour {
	MB_TextureCombinerRenderTexture fastRenderer;
	bool _doRenderAtlas = false;

	public int width;
	public int height;
	public int padding;
	public Rect[] rects;
	public Texture2D tex1;
	public List<MB3_TextureCombiner.MB_TexSet> textureSets;
	public int indexOfTexSetToRender;
	
	public Texture2D OnRenderAtlas(){
		fastRenderer = new MB_TextureCombinerRenderTexture();
		_doRenderAtlas = true;
		Texture2D atlas = fastRenderer.DoRenderAtlas(this.gameObject,width,height,padding,rects,textureSets,indexOfTexSetToRender);
		_doRenderAtlas = false;
		return atlas;
	}
	
	void OnRenderObject(){
		if (_doRenderAtlas){
			Debug.Log ("FastRender.OnRenderObject");
			fastRenderer.OnRenderObject();
			_doRenderAtlas = false;
		}
	}
}
