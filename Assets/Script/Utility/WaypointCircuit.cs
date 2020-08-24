using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityStandardAssets.Utility
{
	public class WaypointCircuit : MonoBehaviour
	{
		public Transform trackTransform;
		[SerializeField] private bool smoothRoute = true;
		[SerializeField] private int substeps = 10;

		public Transform[] WayCheckPoints { get; set; }
		private Vector3[] inWayPoints;
		private Vector3[] inWayPointDirections;
		internal int InwayPointsCount { get; private set; }
		internal float CircuitLength { get; private set; }



		// Use this for initialization
		private void Awake()
		{
			if (WayCheckPoints.Length > 1)
			{
				CachePositionsAndDistances();
			}
		}

		public RoutePoint GetRoutePoint(Vector3 carPos)
		{
			// position and direction
			int inWayPointIndex = GetInWayPointIndex(carPos);
			return new RoutePoint(inWayPoints[inWayPointIndex], inWayPointDirections[inWayPointIndex]);
		}

		Vector3 cacheLastPosition;
		int cachePointIndex;
		public int GetInWayPointIndex(Vector3 position)
		{
			if (cacheLastPosition == position)
			{
				return cachePointIndex;
			}
			int inWayPointIndex = 0;
			if (smoothRoute)
			{
				float distance = -1, distance2 = -1;
				for (int i = 0; i < inWayPoints.Length - 1; i++)
				{
					distance2 = Vector3.Distance(inWayPoints[i], position);
					if (distance < 0 || distance2 < distance)
					{
						distance = distance2;
						inWayPointIndex = i;
					}
				}
			}

			cacheLastPosition = position;
			cachePointIndex = inWayPointIndex;
			return inWayPointIndex;
		}

		public float GetInWayProgress(Vector3 position)
		{
			float inWayPointIndex = GetInWayPointIndex(position);
			return inWayPointIndex / InwayPointsCount;
		}

		public RoutePoint GetRoutePointByProgress(float progress)
		{
			int inWayPointIndex = (int)(progress * InwayPointsCount) % InwayPointsCount;
			return new RoutePoint(inWayPoints[inWayPointIndex], inWayPointDirections[inWayPointIndex]);
		}

		private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
		{
			// comments are no use here... it's the catmull-rom equation.
			// Un-magic this, lord vector!
			return 0.5f *
				   ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i +
					(-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
		}

		private void CachePositionsAndDistances()
		{
			//去除空的点
			List<Transform> wayPoint2List = new List<Transform>();
			for (int i = 0; i < WayCheckPoints.Length; i++)
			{
				if (WayCheckPoints[i] != null)
				{
					wayPoint2List.Add(WayCheckPoints[i]);
				}
			}
			WayCheckPoints = wayPoint2List.ToArray();
			int checkPointNum = WayCheckPoints.Length;

			// transfer the position of each point and distances between points to arrays for
			// speed of lookup at runtime
			List<Vector3> inWayPointList = new List<Vector3>();
			List<Vector3> inWayPointDirectionList = new List<Vector3>();

			for (int i = 0; i < WayCheckPoints.Length; ++i)
			{
				var t1 = WayCheckPoints[(i) % WayCheckPoints.Length];
				var t2 = WayCheckPoints[(i + 1) % WayCheckPoints.Length];
				float distance = (t1.position - t2.position).magnitude;

				// get indices for the surrounding four points
				// two points(1, 2) are required by the linear lerp function
				// four points are required by the catmull-rom function
				int p0n = ((i - 2) + checkPointNum) % checkPointNum;
				int p1n = ((i - 1) + checkPointNum) % checkPointNum;
				int p2n = i % checkPointNum;
				int p3n = (i + 1) % checkPointNum;

				Vector3 p0 = WayCheckPoints[p0n].position;
				Vector3 p1 = WayCheckPoints[p1n].position;
				Vector3 p2 = WayCheckPoints[p2n].position;
				Vector3 p3 = WayCheckPoints[p3n].position;
				Vector3 inWayPoint;
				for (float dist = 0; dist < distance; dist += distance / substeps)
				{
					// found point numbers, now find interpolation value between the two middle points
					float distProgress = Mathf.InverseLerp(0, distance, dist);

					if (smoothRoute)
					{
						// smooth catmull-rom calculation between the two relevant points
						inWayPoint = CatmullRom(p0, p1, p2, p3, distProgress);
					} else {
						// simple linear lerp between the two points:
						inWayPoint = Vector3.Lerp(WayCheckPoints[p1n].position, WayCheckPoints[p2n].position, distProgress);
					}
					inWayPointList.Add(inWayPoint);
				}
			}
			inWayPointList.Add(inWayPointList[0]);
			CircuitLength = 0;
			for (int i = 0; i < inWayPointList.Count; i++)
			{
				inWayPointDirectionList.Add((inWayPointList[i] - inWayPointList[(i + 1) % inWayPointList.Count]).normalized);
				CircuitLength += Vector3.Distance(inWayPointList[i], inWayPointList[(i + 1) % inWayPointList.Count]);
			}
			inWayPoints = inWayPointList.ToArray();
			inWayPointDirections = inWayPointDirectionList.ToArray();
			InwayPointsCount = inWayPointList.Count;
		}


		private void OnDrawGizmos()
		{
			DrawGizmos(false);
		}

		private void OnDrawGizmosSelected()
		{
			DrawGizmos(true);
		}

		private void DrawGizmos(bool selected)
		{
			CachePositionsAndDistances();
			Gizmos.color = selected ? Color.yellow : new Color(1, 1, 0, 0.5f);
			Vector3 prev = inWayPoints[0];
			for (int i = 1; i < inWayPoints.Length; i++)
			{
				Vector3 next = inWayPoints[i];
				Gizmos.DrawLine(prev, next);
				Gizmos.DrawWireSphere(prev, 0.5f);
				prev = next;
			}
			Gizmos.DrawLine(prev, WayCheckPoints[0].position);
		
			for (int n = 0; n < WayCheckPoints?.Length; ++n)
			{
				Gizmos.DrawWireSphere(WayCheckPoints[(n + 1) % WayCheckPoints.Length].position, 1);
			}
		}


		public struct RoutePoint
		{
			public Vector3 position;
			public Vector3 direction;



			public RoutePoint(Vector3 position, Vector3 direction)
			{
				this.position = position;
				this.direction = direction;
			}
		}
	}
}

namespace UnityStandardAssets.Utility.Inspector
{
#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(WaypointCircuit))]
	public class WaypointListDrawer : PropertyDrawer
	{
		private float lineHeight = 18;
		private float spacing = 4;


		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			float x = position.x;
			float y = position.y;
			float inspectorWidth = position.width;

			// Draw label


			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var items = property.FindPropertyRelative("items");
			var titles = new string[] { "Transform", "", "", "" };
			var props = new string[] { "transform", "^", "v", "-" };
			var widths = new float[] { .7f, .1f, .1f, .1f };
			float lineHeight = 18;
			bool changedLength = false;
			if (items.arraySize > 0)
			{
				for (int i = -1; i < items.arraySize; ++i)
				{
					var item = items.GetArrayElementAtIndex(i > 0 ? i : 0);

					float rowX = x;
					for (int n = 0; n < props.Length; ++n)
					{
						float w = widths[n] * inspectorWidth;

						// Calculate rects
						Rect rect = new Rect(rowX, y, w, lineHeight);
						rowX += w;

						if (i == -1)
						{
							EditorGUI.LabelField(rect, titles[n]);
						}
						else
						{
							if (n == 0)
							{
								EditorGUI.ObjectField(rect, item.objectReferenceValue, typeof(Transform), true);
							}
							else
							{
								if (GUI.Button(rect, props[n]))
								{
									switch (props[n])
									{
										case "-":
											items.DeleteArrayElementAtIndex(i);
											items.DeleteArrayElementAtIndex(i);
											changedLength = true;
											break;
										case "v":
											if (i > 0)
											{
												items.MoveArrayElement(i, i + 1);
											}
											break;
										case "^":
											if (i < items.arraySize - 1)
											{
												items.MoveArrayElement(i, i - 1);
											}
											break;
									}
								}
							}
						}
					}

					y += lineHeight + spacing;
					if (changedLength)
					{
						break;
					}
				}
			}
			else
			{
				// add button
				var addButtonRect = new Rect((x + position.width) - widths[widths.Length - 1] * inspectorWidth, y,
											 widths[widths.Length - 1] * inspectorWidth, lineHeight);
				if (GUI.Button(addButtonRect, "+"))
				{
					items.InsertArrayElementAtIndex(items.arraySize);
				}

				y += lineHeight + spacing;
			}

			// add all button
			var addAllButtonRect = new Rect(x, y, inspectorWidth, lineHeight);
			if (GUI.Button(addAllButtonRect, "Assign using all child objects"))
			{
				var circuit = property.FindPropertyRelative("circuit").objectReferenceValue as WaypointCircuit;
				var children = new Transform[circuit.transform.childCount];
				int n = 0;
				foreach (Transform child in circuit.transform)
				{
					children[n++] = child;
				}
				Array.Sort(children, new TransforSiblingIndexComparer());
				circuit.WayCheckPoints = new Transform[children.Length];
				for (n = 0; n < children.Length; ++n)
				{
					circuit.WayCheckPoints[n] = children[n];
				}
			}
			y += lineHeight + spacing;

			// add all button
			var addAllOnTrackTransformButtonRect = new Rect(x, y, inspectorWidth, lineHeight);
			if (GUI.Button(addAllOnTrackTransformButtonRect, "Assign using all Track Trans child objects"))
			{
				var circuit = property.FindPropertyRelative("circuit").objectReferenceValue as WaypointCircuit;
				var children = new Transform[circuit.trackTransform.childCount];
				int n = 0;
				for (int i = 0; i < children.Length; i++)
				{
					children[i] = circuit.trackTransform.GetChild(i);
				}
				Array.Sort(children, new TransforSiblingIndexComparer());
				circuit.WayCheckPoints = new Transform[children.Length];
				for (n = 0; n < children.Length; ++n)
				{
					circuit.WayCheckPoints[n] = children[n];
				}
			}
			y += lineHeight + spacing;

			// rename all button
			var renameButtonRect = new Rect(x, y, inspectorWidth, lineHeight);
			if (GUI.Button(renameButtonRect, "Auto Rename numerically from this order"))
			{
				var circuit = property.FindPropertyRelative("circuit").objectReferenceValue as WaypointCircuit;
				int n = 0;
				foreach (Transform child in circuit.WayCheckPoints)
				{
					child.name = "Waypoint " + (n++).ToString("000");
				}
			}
			y += lineHeight + spacing;

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}


		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			SerializedProperty items = property.FindPropertyRelative("items");
			float lineAndSpace = lineHeight + spacing;
			return 60 + (items.arraySize * lineAndSpace) + lineAndSpace;
		}


		// comparer for check distances in ray cast hits
		public class TransformNameComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				return ((Transform)x).name.CompareTo(((Transform)y).name);
			}
		}

		// comparer for check distances in ray cast hits
		public class TransforSiblingIndexComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				return ((Transform)x).GetSiblingIndex().CompareTo(((Transform)y).GetSiblingIndex());
			}
		}
	}
#endif
}
