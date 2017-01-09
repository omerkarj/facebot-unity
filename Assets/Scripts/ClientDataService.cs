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

//	public void DrawLineToPointFromString(string pointString)
//	{
//		Vector2 point = null;
//		Match m = new Regex (@"\((\d+),(\d+)\)").Match (pointString);
//
//		if (m.Success) {
//			int x = int.Parse(m.Groups[1].ToString());
//			int y = int.Parse(m.Groups[2].ToString());
//			point = new Vector2 (x, y);
//		}
//
//		if (point != null) {
//			DrawLine drawLineScript = DrawLineService.Lines [0].GetComponent<DrawLine> ();
//			drawLineScript.DrawSegmentToPoint (point);
//			Debug.Log ("new point: " + point);
//		}
//	}

	void Start() 
	{
		drawLineService = GetComponent<DrawLineService> ();
		playerController = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerController> ();
	}

	public void DrawCompleteLineFromString(string pointString)
	{
		List<Vector2> pointsList = new List<Vector2> ();
		Match m = new Regex (@"\((\d+), (\d+)\)").Match (pointString);

		while (m.Success) {
			int x = int.Parse(m.Groups[1].ToString());
			int y = int.Parse(m.Groups[2].ToString());
			pointsList.Add(new Vector2(x, y));
			Debug.Log (string.Format("Added {0},{1} to points list", x, y));
			m = m.NextMatch ();
		}

		if (pointsList.Count > 0) {
			drawLineService.AddToLineCreationQueue(pointsList);
		}
	}

	public void ClearLines()
	{
		drawLineService.ClearLines ();
	}

	public void Play() {
		playerController.Play ();
	}

	public void Stop() {
		playerController.Stop ();
	}

	public byte[] GetCurrentFrameAsString () {
		return encodedScreenshot;
	}
}
