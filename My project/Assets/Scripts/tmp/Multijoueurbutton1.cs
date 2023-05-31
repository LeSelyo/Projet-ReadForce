using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Multijoueurbutton1 : MonoBehaviour
{
	// Use this for initialization
	void Start () {
		
	}

    public void Play()
    {
        SceneManager.LoadScene("multijoueurs1");
    }
    // Update is called once per frame
}
