/*
SSBB4455 2020-08-14
*/
using UnityEngine;

/// <summary>
/// 赛事观察员 获取车辆在赛道内的信息
/// </summary>

public interface ICircuitRacingObserver
{
	void Reset();
	float GetMatchProgress();
	float GetLoopProgress();
	bool MatchFinish();
	int GetCircuitTime();
	float GetCircuitLength();
	Vector3 GetStartPointPosition();
	Quaternion GetStartPointRotation();
	Vector3 GetCircuitWayDirection(float circuitProgress);
	int GetRank();
	int GetLoopCount();
}
