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

    public void SpawnObjects()      // ������Ʈ�� �����ϴ� �Լ�
    {
        objectCount = Random.Range(1, maxObjects);  // 1���� maxObjects ������ ������ ������ ������Ʈ ����

        for (int i = 0; i < objectCount; i++)   //objectCount��ŭ �ݺ��Ͽ� ������Ʈ ����
        {
            GameObject gameobject = Instantiate(objectPrefab, spawnPoint.position, Quaternion.identity);
            gameobject.transform.localScale = new Vector3(objectScale, objectScale, objectScale);
            gameobject.transform.parent = spawnPoint;
            spawnedObjects.Add(gameobject);
        }
        ArrangeInCircle();      //������ ������Ʈ�� �������� ��ġ
    }

    public void ClearObjects()      // ������ ������Ʈ�� ��� �����ϴ� �Լ�
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            Destroy(spawnedObjects[i]);
        }
        spawnedObjects.Clear();
    }

    public void ArrangeInCircle()   // �������� ��ġ�ϴ� �Լ�
    {
        if (spawnedObjects.Count == 0) return;      //������ ������Ʈ�� ������ ����

        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            float angle = 360f / spawnedObjects.Count * i;  //360���� ������Ʈ ������ ������ �� ������Ʈ�� ���� ���
            float radian = angle * Mathf.Deg2Rad;       //Deg2Rad: ������ �������� ��ȯ

            Vector3 offset = new Vector3(Mathf.Sin(radian), Mathf.Cos(radian), 0) * radius; //sin�� cos�� �̿��Ͽ� �������� ��ġ�� ��ġ ���

            spawnedObjects[i].transform.localPosition = offset;
        }
    }

}