using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSwitcher : MonoBehaviour {

	public enum ImageType {
		WIN, LOSE
	}

	public GameObject apple;
	public Sprite loseImg;
	public Sprite winImg;
	private SpriteRenderer image;

	void Start() {
		image = GetComponent<SpriteRenderer> ();
	}

	void Update() {
		if (apple.activeSelf) {
			changeImage (ImageType.LOSE);
		} else {
			changeImage (ImageType.WIN);
		}
	}

	void changeImage(ImageType imageType) {
		switch (imageType) {
			case ImageType.WIN:
				image.sprite = winImg;
				break;
			case ImageType.LOSE:
				image.sprite = loseImg;
				break;
		}
	}
}
