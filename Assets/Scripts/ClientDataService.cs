using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class ClientDataService : MonoBehaviour {

	public byte[] encodedScreenshot = Encoding.ASCII.GetBytes("IEND");

	private DrawLineService drawLineService;
	private PlayerController playerController;
	public readonly List<Vector2> CLEAR_LINES = new List<Vector2> (1);

	void Start() 
	{
		drawLineService = GetComponent<DrawLineService> ();
		playerController = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();

		CLEAR_LINES.Add (Vector2.zero);
	}

	public void DrawCompleteLineFromString(string pointString)
	{
		List<Vector2> pointsList = new List<Vector2> ();
		Match m = new Regex (@"\((\d+), (\d+)\)").Match (pointString);

		while (m.Success) {
			int x = int.Parse(m.Groups[1].ToString());
			int y = int.Parse(m.Groups[2].ToString());
			pointsList.Add(new Vector2(x, y));
			m = m.NextMatch ();
		}

		if (pointsList.Count > 0) {
			drawLineService.AddToLineCreationQueue(pointsList);
		}
	}

	public void ClearLines()
	{
		drawLineService.AddToLineCreationQueue (CLEAR_LINES);
	}

	public void Play() {
		playerController.Play ();
	}

	public void Stop() {
		playerController.Stop ();
	}

	public void Reset() {
		playerController.ResetPosition ();
	}

	public byte[] GetCurrentFrameAsByteArray () {
		return encodedScreenshot;
	}
}
