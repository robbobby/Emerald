using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCollder :  MonoBehaviour
{
    public GameSceneManager GameScene
    {
        get { return GameManager.GameScene; }
    }
  

    public void OnTriggerEnter(Collider Player)
    {
        if (Player.CompareTag("Player"))
        {
            GameScene.BossHp.SetActive(true);
        }
    }
    public void OnTriggerExit(Collider Player)
    {
        if (Player.CompareTag("Player"))
        {          
            GameScene.BossHp.SetActive(false);
        }
    }



}
