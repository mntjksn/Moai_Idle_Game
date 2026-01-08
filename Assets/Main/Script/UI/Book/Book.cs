using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Book : MonoBehaviour
{
    public int index_book;

    public TextMeshProUGUI book_name;
    public GameObject image;
    public GameObject Book_prefab;

    private Button btn;
    private bool isUnlocked = false;
    private Image imgComp;
    private Transform canvas2;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(but_event);

        Input.multiTouchEnabled = false;

        imgComp = image.GetComponent<Image>();
        image.SetActive(false);

        canvas2 = GameObject.Find("Canvas2")?.transform;
    }

    private void Start()
    {
        TryUnlock();   // 시작할 때 한 번
    }

    private void Update()
    {
        // 아직 안 열린 애들만 가볍게 체크
        if (!isUnlocked)
            TryUnlock();
    }

    private void TryUnlock()
    {
        var character = CharacterManager.Instance?.GetItem(index_book);
        if (character == null || !character.spawncheck)
            return;

        isUnlocked = true;

        // 이미지 세팅
        if (character.itemimg != null)
        {
            imgComp.sprite = character.itemimg;
            image.SetActive(true);
        }

        // 이름 세팅
        if (index_book == 0)
            book_name.text = character.name;
        else
            book_name.text = $"{index_book}. {character.name}";
    }

    public void but_event()
    {
        if (!isUnlocked) return;

        GameData data = SaveManager.Load();
        data.upgrades.booknum = index_book;
        SaveManager.Save(data);

        if (canvas2 != null)
            Instantiate(Book_prefab, Vector3.zero, Quaternion.identity, canvas2);
    }
}