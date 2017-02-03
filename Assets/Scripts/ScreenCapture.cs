using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// saves each game frame as and encoded PNG string instead of rendering it
public class ScreenCapture : MonoBehaviour {

	private Camera mainCamera;
	private RenderTexture textureTarget;
	private Texture2D screenShot;
	private int width = Screen.width;
	private int height = Screen.height;

	private ClientDataService clientDataService;

	void Start () {
		mainCamera = GetComponent<Camera>();
		textureTarget = new RenderTexture(width, height, 24);
		screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
		clientDataService = GameObject.FindGameObjectWithTag ("Server").GetComponent<ClientDataService> ();

		mainCamera.targetTexture = textureTarget;
		RenderTexture.active = textureTarget;
	}

	void OnPostRender()
	{
		screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

		byte[] encoded = screenShot.EncodeToPNG ();
		clientDataService.encodedScreenshot = Encoding.ASCII.GetBytes(Convert.ToBase64String(encoded) + "<EOF>");
	}

	void OnApplicationQuit()
	{
		mainCamera.targetTexture = null;
		RenderTexture.active = null;
		Destroy(textureTarget);
	}	
}
