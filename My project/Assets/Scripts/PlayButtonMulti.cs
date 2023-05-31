using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButtonMulti : MonoBehaviour {

    public Token sceneLoader;
    // Use this for initialization
    void Start () {
		
	}

    public void Play()
    {
       
        sceneLoader.LoadSceneWithObject("TapTapAim");
    }
    // Update is called once per frame
}
