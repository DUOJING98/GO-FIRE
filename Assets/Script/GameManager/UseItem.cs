using UnityEngine;

public class UseItem : MonoBehaviour
{
    float hp, attack;
    public GameObject player;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //item1 increase attack
    void UseItem1()
    {
        attack += 25;
    }
    //item2 increase hp
    void UseItem2()
    {
        hp += 25;
    }
    //item3 shi dui fang gong ji li jian ban
    void UseItem3()
    {
        
    }
    //item4 ruo dui fang shi yong item2, shi qi hp -20
    void UseItem4()
    {
        
    }
    //item5 shi shuang fang gong ji li xiang deng
    void UseItem5()
    {
        attack += 5;
    }
}
