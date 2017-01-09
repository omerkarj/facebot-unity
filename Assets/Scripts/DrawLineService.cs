using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DrawLineService : MonoBehaviour {

	private List<GameObject> m_Lines = new List<GameObject> ();
	public List<GameObject> Lines { 
		get {
			return m_Lines;
		} 
	}

	private Queue<List<Vector2>> lineCreationQueue = new Queue<List<Vector2>> ();
	public GameObject m_LineParentPrefab;

	private const string LINE_TAG = "Line";

	void Update()
	{
		List<Vector2> pointsList = null;

		lock (lineCreationQueue) 
		{
			if (lineCreationQueue.Count > 0) {
				pointsList = lineCreationQueue.Dequeue ();
			}
		}

		if (pointsList != null) {
			List<Vector2> pointsInWorldList = new List<Vector2> ();
			foreach (Vector2 point in pointsList) 
			{
				Vector2 pointInWorld = convertToWorldPoint (point);
				pointsInWorldList.Add (pointInWorld);
			}

			CreateNewLine (pointsInWorldList);
		}
	}

	public void AddToLineCreationQueue (List<Vector2> pointsList)
	{
		lock (lineCreationQueue) 
		{
			lineCreationQueue.Enqueue(pointsList);
			Debug.Log ("Added line to creation queue");
		}
	}

	public void CreateNewLine (List<Vector2> pointsList)
	{
		// Create line renderer component and set its properties
		GameObject lineParent = Instantiate(Resources.Load("Prefabs/Line")) as GameObject;
		lineParent.transform.parent = gameObject.transform;
		Lines.Add (lineParent);

		DrawLine drawLineScript = lineParent.GetComponent<DrawLine> ();
		drawLineScript.DrawCompleteStroke (pointsList);
	}

	public void ClearLines ()
	{
		GameObject[] lines = GameObject.FindGameObjectsWithTag (LINE_TAG);
		foreach (GameObject line in lines) {
			GameObject.Destroy(line);
		}
	}

	private Vector2 convertToWorldPoint (Vector2 screenPoint) 
	{
		return Camera.main.ScreenToWorldPoint(new Vector2(screenPoint.y, 480 - screenPoint.x));
	}
}
