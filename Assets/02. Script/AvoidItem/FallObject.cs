using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class FallObject : MonoBehaviour
{
    CharacterMove characterMove;
    public GameObject prefab;             // 생성할 오브젝트
    public static float spawnDistance = 10f;     // 카메라로부터의 거리
    public List<GameObject> objectPool = new List<GameObject>(); // 오브젝트 풀

    public bool isOut = false; // 아웃 여부
    float spawnInterval = 0.3f; // 생성 간격

    int poolSize = 50;
    private bool isEventSubscribed = false; // 이벤트 구독 상태 체크

    private void Awake()
    {
        characterMove = FindObjectOfType<CharacterMove>();
    }

    private void Start()
    {
        // 오브젝트 풀 초기화
        InitializeObjectPool();

        // 이벤트 구독 (Start에서 한 번 더 확실히)
        SubscribeToEvents();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()        //시작 시 이벤트 구독
    {
        if (!isEventSubscribed)     //이벤트가 구독되지 않은 경우에만 구독
        {
            GameManager.GameStartEvent += GameStart;
            GameManager.OnTimerFinishedEvent += StopSpawn;

            if (characterMove != null)
            {
                GameManager.GameStartEvent += characterMove.StartMovement;
                GameManager.OnTimerFinishedEvent += characterMove.StopMovement;
            }

            isEventSubscribed = true;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (isEventSubscribed)
        {
            GameManager.GameStartEvent -= GameStart;
            GameManager.OnTimerFinishedEvent -= StopSpawn;

            if (characterMove != null)
            {
                GameManager.GameStartEvent -= characterMove.StartMovement;
                GameManager.OnTimerFinishedEvent -= characterMove.StopMovement;
            }

            isEventSubscribed = false;
        }
    }

    private void InitializeObjectPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(prefab, new Vector3(0, 0, spawnDistance), Quaternion.identity);
            obj.transform.SetParent(transform);
            objectPool.Add(obj);
            obj.SetActive(false);
        }
    }

    private void RandomPosition(int index)
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            return;
        }

        Vector3 randomPos = cam.ViewportToWorldPoint(new Vector3(Random.Range(0f, 1f), 1.1f, spawnDistance));
        objectPool[index].transform.position = randomPos;
    }

    public void Spawn()
    {
        // 게임이 종료된 상태면 스폰하지 않음
        if (isOut) return;

        List<int> inactiveIndices = new List<int>();

        for (int i = 0; i < objectPool.Count; i++)
        {
            if (!objectPool[i].activeInHierarchy)
                inactiveIndices.Add(i);
        }

        if (inactiveIndices.Count > 0)
        {
            int randomIndex = inactiveIndices[Random.Range(0, inactiveIndices.Count)];
            RandomPosition(randomIndex);
            objectPool[randomIndex].SetActive(true);
            objectPool[randomIndex].GetComponent<Rigidbody>().velocity = Vector3.down * Random.Range(1f, 10f);
            StartCoroutine(DisableAfterTime(objectPool[randomIndex], 3f));
        }
        else
        {
            Debug.LogWarning("모든 풀 사용 중");
        }
    }

    public void StopSpawn()
    {
        isOut = true;
        StopAllCoroutines();

        // 모든 활성화된 오브젝트들을 비활성화
        foreach (var obj in objectPool)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                obj.SetActive(false);
            }
        }
    }

    public void GameStart()
    {
        isOut = false;
        StartCoroutine(SpawnLoop());
    }

    public IEnumerator SpawnLoop()
    {
        while (!isOut)
        {
            Spawn();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void DeSpawnNow(GameObject obj)
    {
        if (objectPool.Contains(obj) && obj.activeInHierarchy)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            obj.SetActive(false);
        }
    }

    IEnumerator DisableAfterTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (obj != null && obj.activeInHierarchy && !isOut)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            obj.SetActive(false);
        }
    }
}