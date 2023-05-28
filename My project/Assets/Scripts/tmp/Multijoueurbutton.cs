using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Multijoueurbutton : MonoBehaviour
{
	// Use this for initialization
	void Start () {
		
	}

    public void Play()
    {
        SceneManager.LoadScene("multijoueurs");
    }
    // Update is called once per frame
}
