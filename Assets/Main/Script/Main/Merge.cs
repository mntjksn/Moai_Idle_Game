using UnityEngine;
using UnityEngine.SceneManagement;

public class Merge : MonoBehaviour
{
    public chbool cb;
    public Vector3 objPosition1;

    private Transform chp;
    private Transform canvas;
    private GameData data;

    private Camera cam;
    private bool isSceneLoading = false;

    // ★ 추가: 로드 중인지 아닌지 확인하는 플래그
    [HideInInspector] public bool IsLoadingData = false;

    private void Awake()
    {
        if (FindObjectsOfType<Merge>().Length == 1)
            DontDestroyOnLoad(gameObject);
        else
        {
            Destroy(gameObject);
            return;
        }

        chp = GameObject.Find("chp")?.transform;
        canvas = GameObject.Find("Canvas2")?.transform;
        cam = Camera.main;

        data = SaveManager.Load();
    }

    public void itemCreate(int num)
    {
        int childMax = data.settings.childMax;
        int upCh = data.upgrades.count;
        int currentChildren = GetActiveChildCount();
        int listMax = CharacterManager.Instance.GetCount();

        bool needSave = false;
        var item = CharacterManager.Instance.GetItem(num);

        // 엔딩 조건
        if (num >= listMax)
        {
            if (!isSceneLoading)
            {
                isSceneLoading = true;
                data.missions.mission_2_value++;
                SaveManager.Save(data);
                SceneManager.LoadScene("End");
            }
            return;
        }

        // 동일 레벨 소환
        if (num == upCh && currentChildren < childMax)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-2.2f, 2.2f),
                Random.Range(-3.5f, 2.0f),
                0f
            );

            SpawnItem(num, randomPos);
            needSave = true;
        }
        else if (num < listMax && num != upCh)
        {
            Vector3 mouse = Input.mousePosition;
            mouse.z = 10f;

            Vector3 worldPos = cam != null ? cam.ScreenToWorldPoint(mouse) : Vector3.zero;

            SpawnItem(num, worldPos);

            // ★ 여기! 로드 중일 때는 mission_4 증가 안 함
            if (!IsLoadingData)
            {
                if (data.background.merge_check <= 65000)
                    data.background.merge_check++;

                data.missions.mission_4_value++;
            }

            needSave = true;
        }

        // 도감 최초 해금
        if (!item.spawncheck)
        {
            item.spawncheck = true;
            data.missions.mission_2_value++;
            data.missions.mission_2_tic++;
            data.upgrades.chprefab = num;

            if (item.panel != null && canvas != null)
                Instantiate(item.panel, Vector3.zero, Quaternion.identity, canvas);

            needSave = true;
        }

        if (needSave)
            SaveManager.Save(data);
    }

    private int GetActiveChildCount()
    {
        int count = 0;
        int max = chp.childCount;

        for (int i = 0; i < max; i++)
            if (chp.GetChild(i).gameObject.activeSelf)
                count++;

        return count;
    }

    private void SpawnItem(int num, Vector3 pos)
    {
        GameObject go = ObjectPool.Instance.SpawnFromPool(num, pos, Quaternion.identity);

        if (go != null)
            go.transform.SetParent(chp, false);
    }

    public void SpawnItemDirect(int num, Vector3 pos)
    {
        GameObject go = ObjectPool.Instance.SpawnFromPool(num, pos, Quaternion.identity);
        if (go != null)
            go.transform.SetParent(chp, false);
    }
}