using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Mission : MonoBehaviour
{
    int quizIndex = 0;

    public GameObject buttonPrefab;         // ��ư ������
    public RectTransform buttonParent;      // Canvas ���� �� ������Ʈ
    private List<Button> dynamicButtons = new List<Button>();
    public TMP_Text questionText;
    public Button startButton;

    Spawner[] spawners;

    bool BigOrSmall;      // true = ���� �� ����, false = ū �� ����

    private void OnEnable()
    {
        GameManager.GameStartEvent += BeginMission;
        GameManager.OnTimerFinishedEvent += OnTimeout;
    }

    private void OnDisable()
    {
        GameManager.GameStartEvent -= BeginMission;
        GameManager.OnTimerFinishedEvent -= OnTimeout;
    }

    private void Start()
    {
        spawners = FindObjectsOfType<Spawner>();

        startButton.onClick.AddListener(() =>
        {
            GameManager.Instance.StartTimer(10f); // �ð� ���� ����
            startButton.gameObject.SetActive(false);
        });
    }

    void BeginMission()
    {
        GenerateButtonsForSpawners();
        quizIndex = 0;
        Question();
    }

    void Question()
    {
        BigOrSmall = Random.Range(0, 2) == 0;
        questionText.text = BigOrSmall ? "���� ���� ������" : "ū ���� ������";
        ResetSpawners();
    }

    void Compare(int index)
    {
        if (GameManager.Instance.IsTimedOut) return;

        Spawner selected = spawners[index];
        int selectedCount = selected.objectCount;

        int bestCount = selectedCount;

        foreach (var spawner in spawners)
        {
            int count = spawner.objectCount;
            if (BigOrSmall)
            {
                if (count < bestCount) bestCount = count;
            }
            else
            {
                if (count > bestCount) bestCount = count;
            }
        }

        bool isCorrect = (selectedCount == bestCount);
        questionText.text = isCorrect ? "<color=green>�����̿���!</color>" : "<color=red>Ʋ�Ⱦ��!</color>";

        if (isCorrect) GameManager.Instance.ScoreUp();

        StartCoroutine(NextQuiz());
    }

    IEnumerator NextQuiz()
    {
        quizIndex++;

        foreach (var btn in dynamicButtons)
            btn.interactable = false;

        yield return new WaitForSeconds(0.3f);

        foreach (var btn in dynamicButtons)
            btn.interactable = true;

        if (!GameManager.Instance.IsTimedOut)
        {
            Question();
        }
    }

    void OnTimeout()
    {
        foreach (var spawner in spawners)
            spawner.ClearObjects();

        foreach (var btn in dynamicButtons)
            btn.gameObject.SetActive(false);

        startButton.gameObject.SetActive(true);
        //questionText.text = $"���� �Ϸ�! {GameManager.Instance.CurrentScore}�� ����!";
        questionText.text = "";
    }

    void ResetSpawners()
    {
        bool hasDuplicate = true;

        while (hasDuplicate)
        {
            foreach (var spawner in spawners)
            {
                spawner.ClearObjects();
                spawner.SpawnObjects();
            }

            List<int> counts = new List<int>();
            foreach (var spawner in spawners)
            {
                counts.Add(spawner.objectCount);
            }

            hasDuplicate = HasDuplicate(counts);
        }
    }

    bool HasDuplicate(List<int> counts)
    {
        HashSet<int> unique = new HashSet<int>();

        foreach (int count in counts)
        {
            if (!unique.Add(count))
                return true;
        }

        return false;
    }

    void GenerateButtonsForSpawners()
    {
        foreach (var btn in dynamicButtons)
            Destroy(btn.gameObject);
        dynamicButtons.Clear();

        for (int i = 0; i < spawners.Length; i++)
        {
            GameObject newBtn = Instantiate(buttonPrefab, buttonParent);
            Button btn = newBtn.GetComponent<Button>();

            int capturedIndex = i;
            btn.onClick.AddListener(() => Compare(capturedIndex));

            dynamicButtons.Add(btn);

            Vector3 screenPos = Camera.main.WorldToScreenPoint(spawners[i].transform.position);
            newBtn.GetComponent<RectTransform>().position = screenPos;
        }
    }
}
