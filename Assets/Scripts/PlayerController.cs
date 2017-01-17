using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public Transform m_walkTarget;
	public float walkSpeed;

	public bool IsWalking { get; set; }
	private Animator m_animator;
	private Vector2 startPosition;
	private bool resetPosition;

	// Use this for initialization
	void Start () {
		m_animator = GetComponent<Animator> ();
		IsWalking = false;
		resetPosition = false;
		startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (resetPosition) {
			transform.position = startPosition;
			resetPosition = false;
		}

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

	public void ResetPosition() {
		resetPosition = true;
	}

	public void Chant() {
		GetComponent<AudioSource> ().Play ();
	}
}
