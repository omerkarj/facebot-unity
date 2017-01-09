using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public Transform m_walkTarget;
	public float walkSpeed;

	public bool IsWalking { get; set; }
	private Animator m_animator;

	// Use this for initialization
	void Start () {
		m_animator = GetComponent<Animator> ();
		IsWalking = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (IsWalking && m_walkTarget != null) {
			m_animator.SetBool ("isWalking", true);

			float step = walkSpeed * Time.deltaTime;
			transform.position = Vector2.MoveTowards (transform.position, m_walkTarget.position, step);
		} 
		else {
			m_animator.SetBool ("isWalking", false);

		}
	}

	public void Play() {
		IsWalking = true;
	}

	public void Stop() {
		IsWalking = false;
	}

	public void Chant() {
		GetComponent<AudioSource> ().Play ();
	}
}
