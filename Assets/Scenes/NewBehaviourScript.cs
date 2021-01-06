/*
SSBB4455 2020-05-02
*/
using System;
using System.Collections;
using System.Collections.Generic;
using com.unity.mgobe;
using com.unity.mgobe.src.Util;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
	const string MyRoomType = "GroupCar";

	public InputField inputFiled;


	PlayerInfo myPlayerInfo;
	List<RoomInfo> roomList;
	List<string> userIdList;

	List<Action> actionList = new List<Action>();
	static object onFrameLock = new object();

	public PlayerInfoPara playerInfoPara
	{
		get => new PlayerInfoPara
		{
			Name = myPlayerInfo.Name,
			CustomPlayerStatus = myPlayerInfo.CustomPlayerStatus,
			CustomProfile = myPlayerInfo.CustomProfile
		};
	}



	// Start is called before the first frame update
	void Start()
	{
		initSDK();

	}

	public void initSDK()
	{
		Global.OpenId = "openiD_" + SystemInfo.deviceUniqueIdentifier;       //全服不能重复的玩家标识Id
		Global.GameId = "obg-m0gzsv4k";// 替换为控制台上的“游戏ID”
		Global.SecretKey = "9b82efdecb1503df24d9fa46c4db7300392c1730";// 替换为控制台上的“游戏key”
		Global.Server = "m0gzsv4k.wxlagame.com";

		GameInfoPara gameInfo = new GameInfoPara
		{
			GameId = Global.GameId,
			OpenId = Global.OpenId,
			SecretKey = Global.SecretKey
		};
		ConfigPara config = new ConfigPara
		{
			Url = Global.Server,
			ReconnectMaxTimes = 5,
			ReconnectInterval = 4000,
			ResendInterval = 2000,
			ResendTimeout = 20000,
			IsAutoRequestFrame = false,
		};
		Listener.Init(gameInfo, config, (ResponseEvent eve) =>
		{
			if (eve.Code == ErrCode.EcOk)
			{
				Debug.Log("Listener.init ecOk");
				Global.Room = new Room(null);
				Listener.Add(Global.Room);
				RefreshRoomList();
				string playerId = GamePlayerInfo.GetInfo().Id;
				myPlayerInfo = new PlayerInfo();
				myPlayerInfo.Id = playerId;
				Debug.LogAssertion("GamePlayerInfoId = " + myPlayerInfo.Id);
				myPlayerInfo.Name = playerId;
				myPlayerInfo.TeamId = "0";
				myPlayerInfo.CustomPlayerStatus = 0;
				myPlayerInfo.CustomProfile = "";
			}
			// 初始化广播回调事件
			this.initBroadcast();

		});
		Debug.Log("initSDK finish");
	}

	public void CreateRoom()
	{
		CreateRoomPara para = new CreateRoomPara
		{
			RoomName = "Room" + UnityEngine.Random.Range(0, 1000),
			MaxPlayers = 12,
			RoomType = MyRoomType,
			CustomProperties = "0",
			IsPrivate = false,
			PlayerInfo = this.playerInfoPara
		};
		CreateRoom(para);
	}

	public void CreateRoom(CreateRoomPara para)
	{
		Global.Room.CreateRoom(para, eve =>
		{
			RefreshRoomList();
		});
	}

	public void RefreshRoomList()
	{
		GetRoomListPara para = new GetRoomListPara
		{
			PageNo = 1,
			PageSize = 20,
			RoomType = MyRoomType
		};
		Room.GetRoomList(para, (eve) =>
		{
			if (eve.Code == ErrCode.EcOk)
			{
				try
				{
					var rsp = (GetRoomListRsp)eve.Data;
					var rlist = new List<RoomInfo>();
					foreach (var item in rsp.RoomList)
					{
						rlist.Add(new RoomInfo(item));
						LogoutRoomInfo(item);
					}
					roomList = rlist;
				}
				catch (System.Exception e)
				{

					Debug.LogError(e);
				}
			}
			else
			{
				Debug.LogError("Get room list error: " + eve.Code);
			}
		});
	}

	public void JoinRoom()
	{
		Debug.Log("join room");
		int roomIdx = 0;
		RoomInfo roomInfo = roomList[roomIdx];
		if (roomInfo.MaxPlayers == (ulong)roomInfo.PlayerList.Count)
		{
			Debug.LogError("roomInfo.maxPlayers == roomInfo.playerList.Count");
			return;
		}

		Global.Room.InitRoom(roomInfo);
		Debug.Log(roomInfo.Name + "\t" + roomInfo.Id);
		JoinRoomPara para = new JoinRoomPara
		{
			PlayerInfo = this.playerInfoPara,
		};
		Global.Room.JoinRoom(para, (eve) =>
		{
			Debug.Log("joinRoom" + eve.Code);
			if (eve.Code == ErrCode.EcOk)
			{
				var rsp = (JoinRoomRsp)eve.Data;
				LogoutRoomInfo(rsp.RoomInfo);
				Debug.Log("join room ecOk" + "\t\t" + eve.Data);
			}
		});

	}

	public void LeaveRoom()
	{
		Global.Room.LeaveRoom(eve =>
		{
			RefreshRoomList();
		});
	}

    /// <summary>
    /// 不太好用
    /// </summary>
	public void GetMyRoomInfo()
	{
		Room.GetMyRoom((eve) =>
		{
			if (eve.Code == ErrCode.EcOk)
			{
				// 设置房间信息到 room 实例
				Debug.Log("玩家已在房间内：" + (eve.Data as RoomInfo).Name);
				return;
			}

			if (eve.Code == 20011)
			{
				Debug.Log("玩家不在房间内");
				return;
			}

			Debug.Log("调用失败");
		});
	}

	public void GetRoomPlayerInfo()
	{
		RoomInfo roomInfo = Global.Room.RoomInfo;
		LogoutRoomInfo(roomInfo);

		if (roomInfo != null)
		{
			userIdList = new List<string>();
			for (int i = 0; i < roomInfo.PlayerList.Count; i++)
			{
				userIdList.Add(roomInfo.PlayerList[i].Id);
			}
			if (userIdList.Count > 1)
			{
				userIdList.RemoveAt(0);
			}
		}
	}

	public void LogoutRoomInfo(RoomInfo roomInfo)
	{
		if (roomInfo != null)
		{
			string roomInfoStr = "room\t" + roomInfo.Id + "\t" + roomInfo.Name + "\t" + roomInfo.PlayerList.Count + "/" + roomInfo.MaxPlayers + "\n";
			for (int i = 0; i < roomInfo.PlayerList.Count; i++)
			{
				roomInfoStr += i + ". id = " + roomInfo.PlayerList[i].Id + "\t\tname = " + roomInfo.PlayerList[i].Name + "\n";
			}
			Debug.Log(roomInfoStr);
		}
	}

	public void SendToAll(string msg)
	{
		SendToClientPara para = new SendToClientPara
        {
            RecvPlayerList = new List<string>(),
			//Msg = Time.realtimeSinceStartup.ToString(),
			Msg = msg,
			RecvType = RecvType.RoomAll
        };
		Global.Room.SendToClient(para, (eve) =>
		{
			Debug.Log("SendToAll " + eve.Msg);
		});
	}

	public void SendToSomeOne()
	{
		SendToClient(userIdList);
	}

	public void SendToClient(List<string> playerList)
	{
		SendToClientPara para = new SendToClientPara
		{
			RecvPlayerList = playerList,
			Msg = Time.realtimeSinceStartup.ToString(),
			RecvType = RecvType.RoomSome
		};

		Global.Room.SendToClient(para, (eve) =>
		{
			Debug.Log("sendToClient " + eve.Msg);
		});
	}
	
    // Update is called once per frame
	void Update()
	{
		if (actionList.Count != 0)
		{
			lock (onFrameLock)
			{
				foreach (var item in actionList)
				{
					if (item != null) item();
				}
				actionList.Clear();
			}
		}
	}

	void initBroadcast()
	{
		Global.Room.OnRecvFrame = eve =>
		{
			//RecvFrameBst bst = (RecvFrameBst)eve.data;
			//AddAction(() => this.OnFrame(bst.frame));
			Debug.Log("onRecvFromClient" + eve.Data);
		};

		Global.Room.OnChangeRoom = eve =>
		{
			RefreshRoomList();
		};

		// 广播：房间有新玩家加入
		Global.Room.OnJoinRoom += (aa) => Debug.Log("新玩家加入" + aa.Data);

		// 广播：房间有玩家退出
		Global.Room.OnLeaveRoom += (aa) => Debug.Log("玩家退出" + aa.Data);

		// 广播：接收到玩家信息
		Global.Room.OnRecvFromClient += (aa) => Debug.Log("onRecvFromClient" + aa.Data);

		Room.OnMatch = eve =>
		{
			RefreshRoomList();
			Debugger.Log("on match!");
		};

		Room.OnCancelMatch = eve =>
		{
			RefreshRoomList();
			Debugger.Log("on cancel match! ");
		};
	}

	private void AddAction(Action cb)
	{
		lock (onFrameLock)
		{
			actionList.Add(cb);
		}
	}

	private void OnDestroy()
	{
        LeaveRoom();
		Global.UnInit();
	}
}