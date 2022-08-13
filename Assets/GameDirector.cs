using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    // ゲームモード
    enum MODE
    {
        NONE,
        NORMAL,
        RESULT,
    }
    
    MODE mode;
    MODE nextMode;

    // フィールド
    const int FIELD_SIZE_X = 8;
    const int FIELD_SIZE_Y = 8;

    // 状態
    enum FIELD
    {
        NONE,
        EMPTY,
        UNIT,
    }

    GameObject[,] fieldData;

    List<int[]> searchDirection = new List<int[]>()
    {
        new int[2]{ 0, +1}, // 上
        new int[2]{+1, +1}, // 右上
        new int[2]{ +1, 0}, // 右
        new int[2]{ +1, -1}, // 右下
        new int[2]{ 0, -1}, // 下
        new int[2]{ -1, -1}, // 左下
        new int[2]{ -1, 0}, // 左
        new int[2]{ -1, +1}, // 左上
    };

    // プレイヤー
    const int PLAYER_MAX = 2;
    int nowTurn;
    GamePlayer[] player;

    // タイマー
    bool isStop;
    float waitTimer;

    // テキスト
    GameObject txtInfo;
    string oldTxtInfo;

    // Start is called before the first frame update
    void Start()
    {
        initMatch();
    }

    // Update is called once per frame
    void Update()
    {
		if (isWait())
		{
            return;
		}

        if(MODE.NORMAL == mode)
		{
            normalMode();
		}
        else if(MODE.RESULT == mode)
		{

		}


        if(MODE.NONE != nextMode)
		{
            initMode(nextMode);
		}
    }

    // フィールドの初期化
    void initMatch()
    {
        txtInfo = GameObject.Find("TxtInfo");

        // フィールド
        GameObject field = GameObject.Find("Field");
        field.transform.localScale = new Vector3(FIELD_SIZE_X, 1, FIELD_SIZE_Y);

        // ---------------
        // プレイヤー設定
        // ---------------
        player = new GamePlayer[2];
        for (int i = 0; i < PLAYER_MAX; i++)
        {
            player[i] = new GamePlayer();
            player[i].IsPlayer = true;
            player[i].UnitType = UnitController.TYPE_WHITE;
        }

        // 相手の設定
        player[1].IsPlayer = false;
        player[1].UnitType = UnitController.TYPE_BLACK;

        nowTurn = 0;

        // ---------------
        // フィールド情報
        // ---------------
        fieldData = new GameObject[FIELD_SIZE_X, FIELD_SIZE_Y];

        // 初期配置（真ん中に配置）
        int[,] initFieldData = new int[FIELD_SIZE_X, FIELD_SIZE_Y];

        initFieldData[FIELD_SIZE_X / 2 - 1, FIELD_SIZE_Y / 2 - 1] = UnitController.TYPE_WHITE;
        initFieldData[FIELD_SIZE_X / 2, FIELD_SIZE_Y / 2 - 1] = UnitController.TYPE_BLACK;

        initFieldData[FIELD_SIZE_X / 2 - 1, FIELD_SIZE_Y / 2] = UnitController.TYPE_BLACK;
        initFieldData[FIELD_SIZE_X / 2, FIELD_SIZE_Y / 2] = UnitController.TYPE_WHITE;


        // 当たり判定の設置
        for (int i = 0; i < FIELD_SIZE_X; i++)
        {
            for (int j = 0; j < FIELD_SIZE_Y; j++)
            {
                // ワールド座標に変換
                float x = i - (FIELD_SIZE_X / 2 - 0.5f);
                float y = j - (FIELD_SIZE_Y / 2 - 0.5f);

                GameObject prefab = (GameObject)Resources.Load("BoxCollider");
                GameObject obj = Instantiate(prefab, new Vector3(x, 0, y), Quaternion.identity);

                // タイルを設置
                if ((i + j) % 2 == 0)
                {
                    Instantiate((GameObject)Resources.Load("Field1"), new Vector3(x, 0.01f, y), Quaternion.identity);
                }

                // 初期ユニット配置
                if (1 > initFieldData[i, j]) continue;

                setUnit(initFieldData[i, j], i, j);

            }
        }

        nowTurn = -1;
        initMode(MODE.NORMAL);
    }

    // ユニットデータを作成
    void setUnit(int type, int x, int y)
    {
        // 配列オーバー or なにか置かれている場合は処理をしない
        if (FIELD.EMPTY != getFieldData(x, y))
        {
            return;
        }

        // ゲームオブジェクト作成
        float posx = x - (FIELD_SIZE_X / 2 - 0.5f);
        float posy = y - (FIELD_SIZE_Y / 2 - 0.5f);

        GameObject prefab = (GameObject)Resources.Load("Unit");
        GameObject obj = Instantiate(prefab, new Vector3(posx, 0, posy), Quaternion.identity);

        obj.GetComponent<UnitController>().Reverse(type);

        // ひっくり返す
        float wait = 0;
        foreach (var v in getReverseUnitsAll(type, x, y))
		{
            GameObject unit = fieldData[v[0], v[1]];
            wait = unit.GetComponent<UnitController>().Reverse(type);
		}

        waitTimer += wait;

        fieldData[x, y] = obj;
    }

    // フィールドデータの状態を返す
    FIELD getFieldData(int x, int y)
    {
        // 配列オーバーのチェック
        if (x < 0 || y < 0 || fieldData.GetLength(0) <= x || fieldData.GetLength(1) <= y)
        {
            return FIELD.NONE;
        }

        // なにかすでに置かれている
        if (null != fieldData[x, y])
        {
            return FIELD.UNIT;
        }

        return FIELD.EMPTY;
    }

    // ターン切替
    void turnChange()
	{
        nowTurn++;

        if(PLAYER_MAX <= nowTurn)
		{
            nowTurn = 0;
		}
	}

    // モードの初期化
    void initMode(MODE next) 
	{
        mode = next;
        nextMode = MODE.NONE;

        if (MODE.NORMAL == mode)
        {
            turnChange();

            // 置けなかったら
            if (!searchEmptyField(player[nowTurn].UnitType))
            {
                turnChange();
            }

			// CPUなら少しウェイト
			if (!player[nowTurn].IsPlayer)
			{
                waitTimer += 2.0f;
			}

            // インフォ
            string playername = player[nowTurn].GetPlayerName();
            txtInfo.GetComponent<Text>().text = playername + "の番です";
        }
        else if (MODE.RESULT == mode)
        {
            string playername = player[0].GetPlayerName();
            int maxcount = 0;

            // ユニットが多い人を探す
            foreach (var v in player)
            {
                int type = v.UnitType;
                for (int i = 0; i < FIELD_SIZE_X; i++)
                {
                    for (int j = 0; j < FIELD_SIZE_Y; j++)
                    {
                        if (null == fieldData[i, j]) continue;

                        if (type == fieldData[i, j].GetComponent<UnitController>().UnitType)
                        {
                            v.UnitCount++;
                        }
                    }
                }

                if (maxcount < v.UnitCount)
                {
                    maxcount = v.UnitCount;
                    playername = v.GetPlayerName();
                }
            }

            txtInfo.GetComponent<Text>().text = playername + "の勝ちです！";
        }

	}

    // 通常モード処理
    void normalMode()
	{
        // 勝敗チェック
        if( !searchEmptyField(UnitController.TYPE_WHITE) && !searchEmptyField(UnitController.TYPE_BLACK))
		{
            nextMode = MODE.RESULT;
            return;
		}


		// CPUの処理
		if (!player[nowTurn].IsPlayer)
		{
            int type = player[nowTurn].UnitType;
            int max = 0;
            int mx = -1, my=-1;

            for (int i = 0; i < FIELD_SIZE_X; i++)
            {
                for (int j = 0; j < FIELD_SIZE_Y; j++)
                {
                    if (FIELD.EMPTY != getFieldData(i, j)) continue;

                    int count = getReverseUnitsAll(type, i, j).Count;
                    if (max < count)
                    {
                        max = count;
                        mx = i;
                        my = j;
                    }
                }
            }

            if(0 < max)
			{
                setUnit(type, mx, my);
                nextMode = MODE.NORMAL;
			}

            return;
        }


        // プレイヤーの処理
        if (Input.GetMouseButtonUp(0))
        {
            int x = -1, y = -1;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                if (null != hit.collider.gameObject)
                {
                    Vector3 pos = hit.collider.gameObject.transform.position;
                    // 配列になおす
                    x = (int)(pos.x + (FIELD_SIZE_X / 2 - 0.5f));
                    y = (int)(pos.z + (FIELD_SIZE_Y / 2 - 0.5f));
                }
            }

            // 置けたら置く
            if( 0 < getReverseUnitsAll(player[nowTurn].UnitType,x,y).Count)
			{
                setUnit(player[nowTurn].UnitType, x, y);
                nextMode = MODE.NORMAL;
            }
        }
    }

    // 敵対するタイプかどうかを返す
    bool isVsType(int type, int x, int y)
	{
        if(FIELD.UNIT != getFieldData(x, y))
		{
            return false;
		}

        GameObject unit = fieldData[x, y];

        int t = unit.GetComponent<UnitController>().UnitType;

        if(0<t && type != t)
		{
            return true;
		}

        return false;
	}

    // ひっくり返せる配列を返す
    List<int[]> getReverseUnits(int type, int x, int y, int vx, int vy)
	{
        int count = 0;

        List<int[]> ret = new List<int[]>();
        List<int[]> none = new List<int[]>();

		while (true)
		{
            // 調べる場所
            x += vx;
            y += vy;

            // ユニットがなければ終了
            if (FIELD.UNIT != getFieldData(x, y)) break;

            // 1枚目は別のタイプじゃないとだめ
            if( 0 == count)
			{
                if(isVsType(type, x, y))
				{
                    ret.Add(new int[] { x, y });
				}
				else
				{
                    return none;
				}
			}
			else
			{
				if (isVsType(type, x, y))
				{
                    ret.Add(new int[] { x, y });
				}
                // 自分と同じタイプを見つけた
				else
				{
                    return ret;
				}
			}

            count++;
		}

        return none;
    }

    // ひっくり返せる配列を返す（全方向）
    List<int[]> getReverseUnitsAll(int type, int x, int y)
	{
        List<int[]> ret = new List<int[]>();

        // その場所に置けるのかどうか
        if(FIELD.EMPTY != getFieldData(x, y))
		{
            return ret;
		}

        foreach(int[] dir in searchDirection)
		{
            int vx = dir[0], vy = dir[1];

            foreach(var v in getReverseUnits(type,x,y,vx,vy))
			{
                ret.Add(v);
			}
		}

        return ret;
    }

    // ユニットを置ける場所を探す
    bool searchEmptyField(int type)
	{
        bool ret = false;

        for(int i = 0; i < FIELD_SIZE_X; i++)
		{
            for (int j = 0; j < FIELD_SIZE_Y; j++)
			{
                // なにか置いてある
                if (FIELD.UNIT == getFieldData(i, j)) continue;

                // 空いてる場所
                if( 0 < getReverseUnitsAll(type,i,j).Count)
				{
                    ret = true;
				}
			}
        }

        return ret;
	}

    // ウェイトの処理
    bool isWait()
	{
        bool ret = false;

		// メニュー等
		if (isStop)
		{
            ret = true;
		}

        // タイマー
        if( 0 < waitTimer)
		{
            waitTimer -= Time.deltaTime;
            ret = true;
		}

        return ret;
	}

    // リスタートボタン
    public void Restart()
	{
        SceneManager.LoadScene("SampleScene");
	}

    // ポーズボタン
	public void Pause()
	{
        isStop = !isStop;

		if (isStop)
		{
            oldTxtInfo = txtInfo.GetComponent<Text>().text;
            txtInfo.GetComponent<Text>().text = "休憩中";
        }
		else
		{
            txtInfo.GetComponent<Text>().text = oldTxtInfo;
        }

        // 長考できないように盤面を隠す
        for (int i = 0; i < FIELD_SIZE_X; i++)
        {
            for (int j = 0; j < FIELD_SIZE_Y; j++)
            {
                // なにか置いてある
                if (null == fieldData[i, j]) continue;
                fieldData[i, j].SetActive( !isStop );
            }
        }
    }
}
