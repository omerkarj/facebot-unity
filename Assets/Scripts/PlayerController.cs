using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public GameObject ui;
	public GameObject apple;
	public Transform m_walkTarget;
	public float walkSpeed;

	public bool StartWalk { get; set; }
	public bool IsWalking { get; set; }
	public bool GameOver { get; set; }

	private Animator m_animator;
	private Vector2 startPosition;
	private bool resetPosition;
	private bool preventLose;

	// Use this for initialization
	void Start () {
		preventLose = false;
		GameOver = false;
		m_animator = GetComponent<Animator> ();
		IsWalking = false;
		resetPosition = false;
		startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (resetPosition) {
			transform.position = startPosition;
			apple.SetActive (true);
			ui.SetActive (false);
			resetPosition = false;
			preventLose = true;
		}

		if (StartWalk) {
			preventLose = false;
			StartCoroutine (waitUntilLose ());
			IsWalking = true;
			StartWalk = false;
		}

		if (IsWalking && m_walkTarget != null) {
			m_animator.SetBool ("isWalking", true);

			float step = walkSpeed * Time.deltaTime;
			transform.position = Vector2.MoveTowards (transform.position, m_walkTarget.position, step);
		} 
		else {
			m_animator.SetBool ("isWalking", false);
		}

		if (GameOver) {
			ui.SetActive (true);
			GameOver = false;
		}
	}

	public void Play() {
		StartWalk = true;
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

	private IEnumerator waitUntilLose() {
		yield return new WaitForSeconds (10);
		if (!preventLose) {
			Stop ();
			GameOver = true;
		}
	}
}
