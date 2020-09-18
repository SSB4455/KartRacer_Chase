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
		public string circuitName;
		public Transform trackTransform;
		[SerializeField] private bool smoothRoute = true;
		[SerializeField] private int substeps = 10;

		[SerializeField] Transform[] wayCheckPoints;
		internal Transform[] WayCheckPoints { get { return wayCheckPoints; } set { wayCheckPoints = value; } }
		private RoutePoint[] inWayPoints;
		internal int InwayPointsCount { get { return inWayPoints.Length; } }
		[SerializeField] int[] optimizedIndices;
		internal float CircuitLength { get; private set; }



		// Use this for initialization
		private void Awake()
		{
			if (WayCheckPoints?.Length > 1)
			{
				CachePositionsAndDistances();
			}
		}

		Vector3 cacheLastPosition;
		RoutePoint cacheRoutePoint;
		public RoutePoint GetRoutePoint(Vector3 position)
		{
			RoutePoint result = new RoutePoint();
			if (cacheLastPosition == position)
			{
				return cacheRoutePoint;
			}

			cacheLastPosition = position;
			if (smoothRoute)
			{
				//First make a very rough sample of the from-to region 
				int steps = (wayCheckPoints.Length - 1) * 6; //Sampling six points per segment is enough to find the closest point range
				int step = InwayPointsCount / steps;
				if (step < 1) step = 1;
				float minDist = (position - inWayPoints[0].position).sqrMagnitude;
				int toIndex = InwayPointsCount - 1;
				int checkFrom = 0;
				int checkTo = toIndex;

				//Find the closest point range which will be checked in detail later
				for (int i = 0; i <= toIndex; i += step)
				{
					if (i > toIndex) i = toIndex;
					float dist = (position - inWayPoints[i].position).sqrMagnitude;
					if (dist < minDist)
					{
						minDist = dist;
						checkFrom = Mathf.Max(i - step, 0);
						checkTo = Mathf.Min(i + step, InwayPointsCount - 1);
					}
					if (i == toIndex) break;
				}
				minDist = (position - inWayPoints[checkFrom].position).sqrMagnitude;

				int index = checkFrom;
				for (int i = checkFrom; i < checkTo; i++)
				{
					float dist = (position - inWayPoints[i].position).sqrMagnitude;
					if (dist < minDist)
					{
						minDist = dist;
						index = i;
					}
				}
				//Project the point on the line between the two closest samples
				int backIndex = Mathf.Max(index - 1, 0);
				int frontIndex = Mathf.Min(index + 1, InwayPointsCount - 1);
				Vector3 back = LinearAlgebraUtility.ProjectOnLine(inWayPoints[backIndex].position, inWayPoints[index].position, position);
				Vector3 front = LinearAlgebraUtility.ProjectOnLine(inWayPoints[index].position, inWayPoints[frontIndex].position, position);
				float backLength = (inWayPoints[index].position - inWayPoints[backIndex].position).magnitude;
				float frontLength = (inWayPoints[index].position - inWayPoints[frontIndex].position).magnitude;
				float backProjectDist = (back - inWayPoints[backIndex].position).magnitude;
				float frontProjectDist = (front - inWayPoints[frontIndex].position).magnitude;
				if (backIndex < index && index < frontIndex)
				{
					if ((position - back).sqrMagnitude < (position - front).sqrMagnitude)
					{
						RoutePoint.Lerp(inWayPoints[backIndex], inWayPoints[index], backProjectDist / backLength, out result);
					} else {
						RoutePoint.Lerp(inWayPoints[frontIndex], inWayPoints[index], frontProjectDist / frontLength, out result);
					}
				}
				else if (backIndex < index)
				{
					RoutePoint.Lerp(inWayPoints[backIndex], inWayPoints[index], backProjectDist / backLength, out result);
				}
				else
				{
					RoutePoint.Lerp(inWayPoints[frontIndex], inWayPoints[index], frontProjectDist / frontLength, out result);
				}

				if (InwayPointsCount > 1 && result.percent < inWayPoints[1].percent) //Handle looped splines
				{
					Vector3 projected = LinearAlgebraUtility.ProjectOnLine(inWayPoints[InwayPointsCount - 1].position, inWayPoints[InwayPointsCount - 2].position, position);
					if ((position - projected).sqrMagnitude < (position - result.position).sqrMagnitude)
					{
						float lerp = LinearAlgebraUtility.InverseLerp(inWayPoints[InwayPointsCount - 1].position, inWayPoints[InwayPointsCount - 2].position, projected);
						RoutePoint.Lerp(inWayPoints[InwayPointsCount - 1], inWayPoints[InwayPointsCount - 2], lerp, out result);
					}
				}
			}

			cacheRoutePoint = result;
			return result;
		}

		public RoutePoint GetRoutePointByProgress(float progress)
		{
			//Evaluate
			RoutePoint result;
			
			int index;
			float lerp;
			GetSamplingValues(progress, out index, out lerp);
			if (lerp > 0) 
			{
				result = RoutePoint.Lerp(inWayPoints[index], inWayPoints[index + 1], lerp);
			} else {
				result = inWayPoints[index];
			}

			return result;
		}

		public void GetSamplingValues(float percent, out int inWayPointsIndex, out float lerp)
		{
			lerp = 0;
			percent = Mathf.Abs(percent);
			percent = percent - (int)percent;
			float indexValue = percent * (optimizedIndices.Length - 1);
			int index = (int)indexValue;
			inWayPointsIndex = optimizedIndices[index];
			float lerpPercent = 0;
			if (index < optimizedIndices.Length - 1)
			{
				//Percent 0-1 between the sampleIndex and the next sampleIndex
				float indexLerp = indexValue - index;
				float sampleIndexPercent = (float)index / (optimizedIndices.Length - 1);
				float nextSampleIndexPercent = (float)(index + 1) / (optimizedIndices.Length - 1);
				//Percent 0-1 of the sample between the sampleIndices' percents
				lerpPercent = Mathf.Lerp(sampleIndexPercent, nextSampleIndexPercent, indexLerp);
			}
			if (inWayPointsIndex < InwayPointsCount - 1)
			{
				lerp = Mathf.InverseLerp(inWayPoints[inWayPointsIndex].percent, inWayPoints[inWayPointsIndex + 1].percent, lerpPercent);
			}
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
			List<Transform> wayCheckPoint2List = new List<Transform>();
			for (int i = 0; i < WayCheckPoints?.Length; i++)
			{
				if (WayCheckPoints[i] != null)
				{
					wayCheckPoint2List.Add(WayCheckPoints[i]);
				}
			}
			WayCheckPoints = wayCheckPoint2List.ToArray();
			int checkPointNum = wayCheckPoint2List.Count;

			// transfer the position of each point and distances between points to arrays for
			// speed of lookup at runtime
			List<RoutePoint> inWayPointList = new List<RoutePoint>();
			for (int i = 0; i < wayCheckPoint2List.Count; ++i)
			{
				// get indices for the surrounding four points
				// two points(1, 2) are required by the linear lerp function
				// four points are required by the catmull-rom function
				int p0n = ((i - 1) + checkPointNum) % checkPointNum;
				int p1n = i % checkPointNum;
				int p2n = (i + 1) % checkPointNum;
				int p3n = (i + 2) % checkPointNum;

				Vector3 p0 = wayCheckPoint2List[p0n].position;
				Vector3 p1 = wayCheckPoint2List[p1n].position;
				Vector3 p2 = wayCheckPoint2List[p2n].position;
				Vector3 p3 = wayCheckPoint2List[p3n].position;
				Vector3 inWayPoint;
				for (int j = 0; j < substeps; j++)
				{
					float distProgress = (float)j / substeps;

					if (smoothRoute)
					{
						// smooth catmull-rom calculation between the two relevant points
						inWayPoint = CatmullRom(p0, p1, p2, p3, distProgress);
					} else {
						// simple linear lerp between the two points:
						inWayPoint = Vector3.Lerp(wayCheckPoint2List[p1n].position, wayCheckPoint2List[p2n].position, distProgress);
					}
					inWayPointList.Add(new RoutePoint(inWayPoint, Vector3.forward));
				}
			}
			CircuitLength = 0;
			for (int i = 0; i < inWayPointList.Count; i++)
			{
				inWayPointList[i] = new RoutePoint(inWayPointList[i].position,
					(inWayPointList[(i + 1) % inWayPointList.Count].position - inWayPointList[i].position).normalized,
					(float)i / inWayPointList.Count);
				CircuitLength += (inWayPointList[i].position - inWayPointList[(i + 1) % inWayPointList.Count].position).magnitude;
			}
			inWayPoints = inWayPointList.ToArray();

			//cache optimized index percent
			if (optimizedIndices == null || optimizedIndices.Length != InwayPointsCount) 
			{
				optimizedIndices = new int[InwayPointsCount];
			}
			optimizedIndices[0] = 0;
			optimizedIndices[optimizedIndices.Length - 1] = InwayPointsCount - 1;
			for (int i = 1; i < InwayPointsCount - 1; i++)
			{
				optimizedIndices[i] = 0;
				float samplePercent = (float)i / (InwayPointsCount - 1);
				for (int j = 0; j < InwayPointsCount; j++)
				{
					if (inWayPoints[j].percent > samplePercent)
					{
						break;
					}
					optimizedIndices[i] = j;
				}
			}
			if (optimizedIndices.Length > 1)
			{
				optimizedIndices[optimizedIndices.Length - 1] = InwayPointsCount - 1;
			}
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
			Vector3 prev = inWayPoints?.Length > 0 ? inWayPoints[0].position : Vector3.zero;
			for (int i = 1; i < inWayPoints?.Length; i++)
			{
				Vector3 next = inWayPoints[i].position;
				Gizmos.DrawLine(prev, next);
				Gizmos.DrawWireSphere(prev, 0.5f);
				Gizmos.DrawLine(prev, prev + inWayPoints[i - 1].direction * 3);
				prev = next;
			}
		
			for (int n = 0; n < WayCheckPoints?.Length; ++n)
			{
				Gizmos.DrawWireSphere(WayCheckPoints[(n + 1) % WayCheckPoints.Length].position, 1);
			}
		}


		public struct RoutePoint
		{
			public Vector3 position;
			public Vector3 direction;
			public float percent;



			public RoutePoint(Vector3 position, Vector3 direction, float percent = 0)
			{
				this.position = position;
				this.direction = direction;
				this.percent = percent;
			}

			public void SetDirection(Vector3 direction)
			{
				this.direction = direction;
			}

			public void SetPercent(float percent)
			{
				this.percent = percent;
			}

			public static RoutePoint Lerp(RoutePoint a, RoutePoint b, float lerp)
			{
				RoutePoint result = new RoutePoint();
				Lerp(a, b, lerp, out result);
				return result;
			}

			public static void Lerp(RoutePoint a, RoutePoint b, float lerp, out RoutePoint result)
			{
				result.position = Vector3.Lerp(a.position, b.position, lerp);
				result.direction = Vector3.Lerp(a.direction, b.direction, lerp);
				result.percent = Mathf.Lerp(a.percent, b.percent, lerp);
			}

			// public void CopyFrom(RoutePoint input)
			// {
			// 	this.position = input.position;
			// 	this.direction = input.direction;
			// 	this.percent = input.percent;
			// }
		}
	}
}

namespace UnityStandardAssets.Utility.Inspector
{
#if UNITY_EDITOR
	[CanEditMultipleObjects, CustomEditor(typeof(WaypointCircuit))]
	public class WaypointCircuitDrawer : Editor
	{
		private float lineHeight = 18;
		private float spacing = 4;
		WaypointCircuit circuit;



		private void OnEnable() {
			circuit = (WaypointCircuit)target;
		}

		public override void OnInspectorGUI()
		{
			if (circuit.WayCheckPoints == null)
			{
				//circuit.WayCheckPoints = new Transform[1];
			}
			DrawDefaultInspector();
			serializedObject.Update();

			float x = 3;
			float y = 5;
			float inspectorWidth = 45;

			// Draw label


			// Don't make child fields be indented
			var indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;



			var items = serializedObject.FindProperty("WayCheckPoints");
			var titles = new string[] { "Transform", "", "", "" };
			var props = new string[] { "transform", "^", "v", "-" };
			var widths = new float[] { .7f, .1f, .1f, .1f };
			float lineHeight = 18;
			bool changedLength = false;

			// 查找floatArray属性
			var elements = this.serializedObject.FindProperty("trackTransform");

			// 属性元素可见，控件展开状态
			if (EditorGUILayout.PropertyField(elements))
			{
				// 缩进一级
				EditorGUI.indentLevel++;
				// 设置元素个数
				elements.arraySize = EditorGUILayout.DelayedIntField("Size", elements.arraySize);
				// 绘制元素
				for (int i = 0, size = elements.arraySize; i < size; i++)
				{
					// 检索属性数组元素
					var element = elements.GetArrayElementAtIndex(i);
					EditorGUILayout.PropertyField(element);
				}
				// 重置缩进
				EditorGUI.indentLevel--;
			}
			// 分隔符
			EditorGUILayout.Separator();
			// 缩进一级
			EditorGUI.indentLevel++;
			for (int i = 0; i < circuit.WayCheckPoints?.Length; i++)
			{
				//水平布局
				EditorGUILayout.BeginHorizontal();

				circuit.WayCheckPoints[i] = (Transform)EditorGUILayout.ObjectField(i + ".", circuit.WayCheckPoints[i], typeof(Transform), true);
				if (GUILayout.Button("+", GUILayout.Width(20)))
				{

				}
				if (GUILayout.Button("-", GUILayout.Width(20)))
				{

				}
				GUILayout.EndHorizontal();
			}
			// 重置缩进
			EditorGUI.indentLevel--;

			

			y += lineHeight + spacing;
			

			// add all button
			var addAllButtonRect = new Rect(x, y, inspectorWidth, lineHeight);
			if (GUILayout.Button("Assign using all child objects"))
			{
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
			if (GUILayout.Button("Assign using all Track Trans child objects"))
			{
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
			if (GUILayout.Button("Auto Rename numerically from this order"))
			{
				int n = 0;
				foreach (Transform child in circuit.WayCheckPoints)
				{
					child.name = "Waypoint " + (n++).ToString("000");
				}
			}

			// 分隔符
			EditorGUILayout.Separator();


			serializedObject.ApplyModifiedProperties();

			//当Inspector 面板发生变化时保存数据
			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
			}
		}


		public float GetPropertyHeight(SerializedProperty property, GUIContent label)
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
