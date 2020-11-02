/*
SSBB4455 2020-08-14
*/
using UnityEngine;

/// <summary>
/// 赛事观察员 获取车辆在赛道内的信息
/// </summary>

public interface ICircuitRacingObserver
{
	string GetCircuitName();
	void Reset();
	float GetMatchProgress();
	float GetLoopProgress();
	bool MatchFinish();
	/// <returns>返回当前圈的用时(毫秒)</returns>
	System.TimeSpan GetLoopTime();
	/// <returns>返回从开始到现在的用时(毫秒)</returns>
	System.TimeSpan GetMatchTime();
	float GetCircuitLength();
	Vector3 GetStartPointPosition(int rank = 0);
	Quaternion GetStartPointRotation(int rank = 0);
	Vector3 GetGuideLinePosition(float loopProgress);
	Vector3 GetGuideLineDirection(float loopProgress);
	int GetRank();
	int GetTotalLoopCount();
	int GetMaxFinishLoopCount();
}
