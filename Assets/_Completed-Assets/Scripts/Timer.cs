using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Timer : MonoBehaviour {
    public float timeLimit;
    public GameObject player;
    public Text gameOver;
    public Text timerText;
    public ParticleSystem fx;
    // Use this for initialization
    void Start () {

       
        
	}
	
	// Update is called once per frame
	void Update () {
    timeLimit -= Time.deltaTime;
        timerText.text = "Timer: " + timeLimit;
    if (timeLimit <= 0) {
            Destroy(player);
                fx.Play();
            gameOver.text = "YOU DIED SHAQ!!!";
        }
	}
}
