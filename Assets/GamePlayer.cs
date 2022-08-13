using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayer
{
	public bool IsPlayer = true;
	public int UnitType;
	public int UnitCount;

	public string GetPlayerName()
	{
		string ret = "";

		string playername = "プレイヤー";
		string type = "（白）";

		if (!IsPlayer)
		{
			playername = "CPU";
		}

		if(UnitController.TYPE_BLACK == UnitType)
		{
			type = "（黒）";
		}

		ret = playername + type;

		return ret;
	}

}
