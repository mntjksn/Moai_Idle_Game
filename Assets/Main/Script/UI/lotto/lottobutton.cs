using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class lottobutton : MonoBehaviour
{
    public GameObject result_panel;
    public TextMeshProUGUI title, res, sub;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        Input.multiTouchEnabled = false;
    }

    public void text()
    {
        result_panel.SetActive(false);
        GetComponent<Button>().interactable = true;
    }

    public void lotto_btn()
    {
        GameData data = SaveManager.Load();

        if (data.currency.ticket < 1)
        {
            AppearTextManager.Instance.Show("Æ¼ÄÏÀÌ ºÎÁ·ÇÕ´Ï´Ù!");
            return;
        }

        if (Setting.IsSFXOn())
            audioSource.Play();

        data.currency.ticket--;

        if (data.background.lotto_check <= 1500)
            data.background.lotto_check++;

        SaveManager.Save(data);

        PlayLotto(data);

        GetComponent<Button>().interactable = false;
        Invoke(nameof(text), 1f);
    }

    private void PlayLotto(GameData data)
    {
        data.missions.mission_6_value++;

        // °³º° È®·ü
        float p1 = data.lottos.lotto_1_value;
        float p2 = data.lottos.lotto_2_value;
        float p3 = data.lottos.lotto_3_value;
        float p4 = data.lottos.lotto_4_value;
        float p5 = data.lottos.lotto_5_value;

        float total = p1 + p2 + p3 + p4 + p5;
        float rand = Random.Range(0f, total);

        string t, r, s;

        if (rand < p1)
        {
            data.currency.token += data.lottos.lotto_1_reward;
            t = "***** 1µî ´çÃ· *****";
            r = $"ÅäÅ« {data.lottos.lotto_1_reward}°³";
            s = "Â¦Â¦Â¦Â¦Â¦Â¦Â¦Â¦Â¦Â¦";
        }
        else if (rand < p1 + p2)
        {
            data.currency.token += data.lottos.lotto_2_reward;
            t = "*** 2µî ´çÃ· ***";
            r = $"ÅäÅ« {data.lottos.lotto_2_reward}°³";
            s = "¾Æ½¬¿îµ¥~";
        }
        else if (rand < p1 + p2 + p3)
        {
            data.currency.dia += data.lottos.lotto_3_reward;
            t = "** 3µî ´çÃ· **";
            r = $"´ÙÀÌ¾Æ {data.lottos.lotto_3_reward}°³";
            s = "¸¸Á·ÇÏ½Ã³ª¿ä?";
        }
        else if (rand < p1 + p2 + p3 + p4)
        {
            data.currency.gold += data.lottos.lotto_4_reward;
            t = "* 4µî ´çÃ· *";
            r = $"µ¹¸æÀÌ {data.lottos.lotto_4_reward:N0}°³";
            s = "Æ¼²ø ¸ð¾Æ ÅÂ»ê";
        }
        else
        {
            t = "5µî ´çÃ·";
            r = $"²Î";
            s = "ÇÑ ¹ø ´õ µµÀü?!";
        }

        if (data.currency.gold > 2147483600)
            data.currency.gold = 2147483600;

        SaveManager.Save(data);

        result_panel.SetActive(true);
        title.text = t;
        res.text = r;
        sub.text = s;
    }
}