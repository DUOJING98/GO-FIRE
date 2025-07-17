using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] Image frontBar;
    [SerializeField] Image backBar;


    [SerializeField] float maxHP = 100f;
    [SerializeField] float currentHP = 100f;
    [SerializeField] float targetHP = 100f;


    [SerializeField] float followSpeed = 1f;


    public void setHP(float hp)
    {
        targetHP = Mathf.Clamp(hp,0,maxHP);

        frontBar.fillAmount = targetHP/maxHP;
    }

    private void Update()
    {
        if(backBar.fillAmount>frontBar.fillAmount)
        {
            backBar.fillAmount = Mathf.MoveTowards(backBar.fillAmount,frontBar.fillAmount,followSpeed*Time.deltaTime);
        }
        else
        {
            backBar.fillAmount=frontBar.fillAmount;
        }
    }

}
