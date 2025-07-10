using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [HideInInspector]
    public float hp, maxHp, attackPower;
    public GameObject hpBar;
    private CountDownManager cdm;
    void Start()
    {
        cdm = GameObject.Find("CountDownManager").GetComponent<CountDownManager>();
        hp = 100;
        maxHp = 100;
        attackPower = 30;
    }

    // Update is called once per frame
    void Update()
    {
        hpBar.GetComponent<Image>().fillAmount = hp / maxHp;
        if (hp <= 0)
        {
            cdm.gameOver = true;
        }
    }
}
