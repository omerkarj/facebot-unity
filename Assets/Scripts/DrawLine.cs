using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class DrawLine : MonoBehaviour
{
	public UnityEvent onLineDrawnEvent;

	public float m_lineWidth;
	public PhysicMaterial m_physicMaterial;

	private LineRenderer m_lineRenderer;
	private List<Vector2> m_pointsList;

	private const string COLLIDER_TAG = "LineCollider";


	void Awake ()
	{
		m_pointsList = new List<Vector2>();

		// Create line renderer component and set its properties
		m_lineRenderer = gameObject.AddComponent<LineRenderer>();
		m_lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		m_lineRenderer.numPositions = 2;
		m_lineRenderer.startWidth = m_lineWidth;
		m_lineRenderer.endWidth = m_lineWidth;
		m_lineRenderer.startColor = Color.green;
		m_lineRenderer.endColor = Color.green;
		m_lineRenderer.useWorldSpace = true; 
	}
		
	void Update ()
	{
	}

	public void DrawCompleteStroke(List<Vector2> pointsList)
	{
		m_pointsList.Clear ();

		foreach (Vector2 point in pointsList) 
		{
			DrawSegmentToPoint (point);
		}	

		Debug.Log ("new line segment drawn");
	}

	public void DrawSegmentToPoint(Vector2 newPoint) 
	{
		m_pointsList.Add(newPoint);
		m_lineRenderer.numPositions = m_pointsList.Count;
		m_lineRenderer.SetPosition(m_pointsList.Count - 1, (Vector2) m_pointsList[m_pointsList.Count - 1]);

		if (m_pointsList.Count > 1)
		{
			CreateCollider();

			// invoke all listeners for the line draw event
			onLineDrawnEvent.Invoke();
		}
	}
		
	// Creates a collider for the last segment of the line
	private void CreateCollider()
	{
		Vector2 point1 = m_pointsList[m_pointsList.Count - 2];
		Vector2 point2 = m_pointsList[m_pointsList.Count - 1];

		GameObject obj = new GameObject(COLLIDER_TAG);
		obj.transform.position = (point1 + point2) / 2;
		obj.transform.right = (point2 - point1).normalized;

		BoxCollider2D boxCollider = obj.AddComponent<BoxCollider2D>();
		boxCollider.size = new Vector2((point2 - point1).magnitude, m_lineWidth);

		obj.tag = COLLIDER_TAG;
		obj.transform.parent = this.transform;
	}
}