using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UnitController : MonoBehaviour
{
    public const int TYPE_WHITE = 1;
    public const int TYPE_BLACK = 2;

    // 自分のタイプ
    public int UnitType;

    Vector3 firstPosition;

    private void Awake()
	{
        firstPosition = transform.position;
    }

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
 
    }

    public float Reverse(int type, bool anim = true)
	{
        float angle = 0;
        float ret = 0.5f;

        if(TYPE_WHITE == type)
		{
		}
        else if(TYPE_BLACK == type)
		{
            angle = 180;
		}

        // 前回のアニメーションのリセット
        this.transform.DOKill();
        transform.position = firstPosition;

        transform.DOLocalJump(
            transform.position, // 終了地点
            1,  // ジャンプする力
            1,  // ジャンプ回数
            ret // アニメーション時間
        );

		if (anim)
		{
            this.transform.DORotate(new Vector3(angle, 0, 0), ret);
        }
		else
		{
            this.transform.eulerAngles = new Vector3(angle, 0, 0);
		}

        UnitType = type;

        return ret;
	}
}
