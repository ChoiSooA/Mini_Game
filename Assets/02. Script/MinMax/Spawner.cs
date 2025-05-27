using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Spawner : MonoBehaviour
{

    public GameObject objectPrefab;
    Transform spawnPoint;
    int maxObjects = 10;
    float radius = 1f;
    float objectScale = 0.5f;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    [HideInInspector] public int objectCount;

    private void Awake()
    {
        spawnPoint = transform;
    }

    public void SpawnObjects()      // 오브젝트를 생성하는 함수
    {
        objectCount = Random.Range(1, maxObjects);  // 1부터 maxObjects 사이의 랜덤한 개수로 오브젝트 생성

        for (int i = 0; i < objectCount; i++)   //objectCount만큼 반복하여 오브젝트 생성
        {
            GameObject gameobject = Instantiate(objectPrefab, spawnPoint.position, Quaternion.identity);
            gameobject.transform.localScale = new Vector3(objectScale, objectScale, objectScale);
            gameobject.transform.parent = spawnPoint;
            spawnedObjects.Add(gameobject);
        }
        ArrangeInCircle();      //생성된 오브젝트를 원형으로 배치
    }

    public void ClearObjects()      // 생성된 오브젝트를 모두 제거하는 함수
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            Destroy(spawnedObjects[i]);
        }
        spawnedObjects.Clear();
    }

    public void ArrangeInCircle()   // 원형으로 배치하는 함수
    {
        if (spawnedObjects.Count == 0) return;      //스폰된 오브젝트가 없으면 리턴

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            float angle = 360f / spawnedObjects.Count * i;  //360도를 오브젝트 개수로 나누어 각 오브젝트의 각도 계산
            float radian = angle * Mathf.Deg2Rad;       //Deg2Rad: 각도를 라디안으로 변환

            Vector3 offset = new Vector3(Mathf.Sin(radian), Mathf.Cos(radian), 0) * radius; //sin과 cos를 이용하여 원형으로 배치할 위치 계산

            spawnedObjects[i].transform.localPosition = offset;
        }
    }

}