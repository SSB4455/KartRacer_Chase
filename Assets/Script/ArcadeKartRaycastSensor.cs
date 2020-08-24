/*
SSBB4455 2020-08-01
*/
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ArcadeKartRaycastSensor : MonoBehaviour
{
	Transform trans;
	internal Transform Trans { get {
			if (!trans)
			{
				trans = transform;
			}
			return trans;
		}
	}

#region Senses
	[Tooltip("How far should the agent shoot raycasts to detect the world?")]
	public float raycastDistance;

	[Tooltip("Which layers the raycasts will detect.")]
	public LayerMask mask;

	[Tooltip("Where the car edge?")]
	public float carEdge = 0.03f;
#endregion


#region ShowRaycastMesh
	public Mesh mesh;
	public Material material;
#endregion
	

#region Debugging
	[Header("Debug Option")]
	[Tooltip("Should we visualize the rays that the agent draws?")]
	public bool ShowRaycasts;
#endregion

	RaycastHit hitInfo;
	float hitDistance;



	public void Update()
	{
		if (ShowRaycasts)
		{
			Debug.DrawRay(Trans.position, Trans.forward * raycastDistance, Color.green);
			Debug.DrawRay(Trans.position, Trans.forward * carEdge, Color.gray);
			if (hitInfo.distance != 0 && hitInfo.distance < raycastDistance)
			{
				Debug.DrawRay(Trans.position, Trans.forward * hitInfo.distance, Color.red);
			}
			//Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, 0);
		}
	}

	public List<float> GetObservationV1()
	{
		bool hit = Physics.Raycast(Trans.position, Trans.forward, out hitInfo, raycastDistance, mask, QueryTriggerInteraction.Ignore);
		hitDistance = hit ? hitInfo.distance : raycastDistance;
		List<float> sensorRaycastInfoList = new List<float>();

		// 检测器指向角度
		sensorRaycastInfoList.AddRange(new float[3] { 
			Trans.eulerAngles.normalized.x, Trans.eulerAngles.normalized.y, Trans.eulerAngles.normalized.z });
		// 碰撞距离
		sensorRaycastInfoList.Add(hitDistance / raycastDistance);
		// 车身边缘
		sensorRaycastInfoList.Add(carEdge / raycastDistance);
		// 检测到的碰撞类型(墙壁0.1f 对方车辆0.2f 己方车辆0.3f)
		sensorRaycastInfoList.Add(0.1f);

		return sensorRaycastInfoList;
	}

}