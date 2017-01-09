using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleCollision : MonoBehaviour {

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.tag == "Player") {
			collision.gameObject.GetComponent<PlayerController> ().Chant ();
			Destroy (gameObject);
		}
	}
}
