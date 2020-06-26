﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

//몬스터 유한 상태 머신
public class EnemyFSM1 : MonoBehaviour
{
    //몬스터 상태 ENUM
    enum EnemyState
    {
        IDLE,
        MOVE,
        ATTACK,
        RETURN,
        DAMAGED,
        DIE,
    }

    //애니메이션을 제어하기 위한 애니메이터 컴포넌트
    Animator anim;

    EnemyState state; //몬스터 상태 변수

    //유용한 기능
    #region "IDLE 상태에 필요한 변수들"
    #endregion

    #region "MOVE 상태에 필요한 변수들"
    #endregion

    #region "ATTACK 상태에 필요한 변수들"
    #endregion

    #region "RETURN 상태에 필요한 변수들"
    #endregion

    #region "DAMAGED 상태에 필요한 변수들"
    #endregion

    #region "DIE 상태에 필요한 변수들"
    #endregion

    #region "IDLE 상태에 필요한 변수들"
    #endregion

    //필요한 변수들
    public float findRange = 15f;   //플레이어를 찾는 범위
    public float attackRange = 2f;  //공격 가능 범위
    public float moveRange = 30f;
    bool canAttack = true;
    Vector3 startPoint;             //몬스터 시작 위치
    //Quaternion startRotation;       //몬스터 시작 회전 값
    Transform player;               //플레이어를 찾기 위해
    CharacterController cc;
    NavMeshAgent agent;

    public EnemyHitBox HitBox;

    //몬스터 일반 변수
    int hp = 100;
    int att = 5;
    float speed = 0.5f;
    float offset;
    //공격 딜레이
    float attTime = 2f; //2초에 한번 공격
    float timer = 0f;   //타이머

    // Start is called before the first frame update
    void Start()
    {
        //몬스터 상태 초기화
        state = EnemyState.IDLE;
        //시작지점 저장
        startPoint = transform.position;
        //startRotation = transform.rotation;
        //플레이어 트랜스폼
        player = GameObject.Find("Player").transform;
        //캐릭터 컨트롤러 컴포넌트
        cc = GetComponent<CharacterController>();
        //애니메이터 컴포넌트
        anim = GetComponentInChildren<Animator>();
        //애니메이션 오프셋 랜덤 지정
        anim.SetFloat("Offset", UnityEngine.Random.Range(0.0f, 1.0f));
        //랜덤 값 지정
        transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
        agent = GetComponent<NavMeshAgent>();
    }

    private void FixedUpdate()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        //OnDrawGizmos();
        //상태에 따른 행동 처리
        switch (state)
        {
            case EnemyState.IDLE:
                Idle();
                break;
            case EnemyState.MOVE:
                Move();
                break;
            case EnemyState.ATTACK:
                Attack();
                break;
        }
    }
    
    public void StartHit()
    {
        CannotAttack();
        state = EnemyState.DAMAGED;
        print(state);
    }

    public void EndHit()
    {
        CanAttack();
        state = EnemyState.MOVE;
        print("좀비상태:이동");
    }


    private void Idle()
    {
        if(Vector3.Distance(transform.position , player.position) < findRange)
        {
            state = EnemyState.MOVE;
            print("상태 전환 : Idle -> Move");

            //애니메이션
            anim.SetTrigger("Move");
        }
    }

    private void Move()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackRange)//공격 범위 안에 들어옴
        {
            state = EnemyState.ATTACK;
            print("상태 전환 : Move -> Attack");
            timer = 2f;
        }
        else
        {
            agent.SetDestination(player.transform.position);
        }
    }

    private void Attack()
    {
        if (PlayerInput.Instance.state == PlayerInput.PlayerState.DIE) return;
        if (Vector3.Distance(transform.position, player.position) > attackRange)//현재 상태를 무브로 전환하기 (재추격)
        {
            state = EnemyState.MOVE;
            print("상태 전환 : Attack -> Move");
            anim.SetTrigger("Move");
            //타이머 초기화
            timer = 0f;
        }
        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            timer += Time.deltaTime;
            if (timer > attTime)
            {
                //일정 시간마다 플레이어를 공겨하기
                anim.SetTrigger("Attack");
                print("공격");
                //플레이어의 필요한 스크립트 컴포넌트를 가져와서 대미지를 주면 된다.
                //player.GetComponent<PlayerMove>().hitDamage(power);
                //타이머 초기화
                timer = 0f;
            }
        }
    }

    //플레이어 쪽에서 충돌 감지를 할 수 있으니 이 함수는 퍼블릭으로 만들자.
    public void HitDamage(int value)
    {
        //예외처리
        //피격 상태 이거나, 죽은 상태 일 때는 대미지 중첩으로 주지 않는다.
        if (state != EnemyState.DAMAGED && state != EnemyState.DIE)
        {
            //체력깎기
            print("좀비 공격당함");
            hp -= value;

            //몬스터의 체력이 1 이상이면 피격 상태
            if (hp > 0)
            {
                anim.SetTrigger("Damaged");
            }
            //0 이하이면 죽음 상태
            else
            {
                state = EnemyState.DIE;
                print("상태 전환 : AnyState -> Die");
                anim.SetTrigger("Die");
                Die();
            }
        }
    }

    public void HitStateSwitch()
    {
        if (state != EnemyState.DAMAGED)
        {
            state = EnemyState.DAMAGED;
        }
        else state = EnemyState.MOVE;
    }

    private void Die()
    {
        print("적 사망!");
        //진행 죽인 모든 코루틴은 정지한다
        StopAllCoroutines();
        StartCoroutine(DieProc());
    }

    IEnumerator DieProc()
    {
        //캐릭터 컨트롤러 비활성화
        cc.enabled = false;
        agent.enabled = false;
        //2초 후에 자기 자신을 제거한다.
        yield return new WaitForSeconds(2.0f);
        print("적 사망!");
    }

    private void OnDrawGizmos()
    {
        //공격 가능 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        //플레이어 찾을 수 있는 범위
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, findRange);

        //이동 가능 최대 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(startPoint, moveRange);
    }

    public void CanAttack()
    {
        HitBox.canAttack = true;
    }

    public void CannotAttack()
    {
        HitBox.canAttack = false;
    }
}
