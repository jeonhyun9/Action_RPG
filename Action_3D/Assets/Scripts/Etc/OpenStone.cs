﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class OpenStone : MonoBehaviour
{
    public GameObject fire;
    public GameObject floatingText;
    public GameObject fireEffect;
    public GameObject door;
    public GameObject doorSound;
    private TextMeshProUGUI textMeshPro;
    private byte alpha;
    private Color targetColor;
    private bool isFloating = false;
    private float floatingTimer;
    private bool doorOpenTrigger = false;
    private bool doorCloseTrigger = false;
    // Update is called once per frame
    private void Start()
    {
        targetColor = new Color(255, 133, 0, 0);
        textMeshPro = floatingText.GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        if (PlayerInput.Instance.isMeetBoss && doorSound.activeSelf == false)
        {
            doorCloseTrigger = true;
        }

        if (Vector3.Distance(transform.position, PlayerInput.Instance.transform.position) < 2f
            && !fire.activeSelf)
        {
            PlayerInput.Instance.isFireLit = true;
        }
        else PlayerInput.Instance.isFireLit = false;
        //플레이어가 캐스팅 중이면 실행
        if(PlayerInput.Instance.state == PlayerInput.PlayerState.END_CASTING && 
            Vector3.Distance(transform.position,PlayerInput.Instance.transform.position) < 2f)
        {
            if (!fire.activeSelf)
            {
                doorOpenTrigger = true;
                PlayerInput.Instance.PlusFireCap();
                PlayerInput.Instance.mpPotionCap = 1;
                PlayerInput.Instance.ManaRefill();
                GameObject effect = Instantiate(fireEffect, this.transform);
                effect.transform.position += new Vector3(0, 0.1f, 0);
                Destroy(effect, 1f);
                fire.SetActive(true);
                floatingText.SetActive(true);
            }
        }

        //텍스트 메쉬 프로의 투명도 증가
        if (fire.activeSelf && alpha + 1 < 255 && isFloating == false)
        {
            alpha += 1;
            targetColor = new Color32(255, 133, 0, alpha);
            textMeshPro.color = targetColor;
        }
        //글씨가 완전히 나타난 후 3초를 센다.
        else if(fire.activeSelf)
        {
            if (floatingTimer < 3f)
            {
                floatingTimer += Time.deltaTime;
            }
            else isFloating = true;
        }

        //3초가 지난 후 부터 투명도가 다시 감소하고, 텍스트 메쉬 프로 비활성화
        if(isFloating && alpha - 1 > 0)
        {
            alpha -= 1;
            if (alpha == 0) floatingText.SetActive(false);
            targetColor = new Color32(255, 133, 0, alpha);
            textMeshPro.color = targetColor;
        }

        //문 닫기
        if (doorOpenTrigger)
        {
            if(doorSound.activeSelf==false)
            {
                doorSound.SetActive(true);
            }
            door.transform.eulerAngles += new Vector3(0, 0.25f, 0);
            if(door.transform.eulerAngles.y > 140)
            {
                doorOpenTrigger = false;
                doorSound.SetActive(false);
            }
        }

        //문 열기 (보스 조우시 문이 닫힘)
        if (doorCloseTrigger)
        {
            //문 닫는 로직 중지
            doorOpenTrigger = false;
            if (doorSound.activeSelf == false)
            {
                doorSound.SetActive(true);
            }
            door.transform.eulerAngles -= new Vector3(0, 0.25f, 0);
            if (door.transform.eulerAngles.y <= 0)
            {
                doorCloseTrigger = false;
            }
        }
    }

}
