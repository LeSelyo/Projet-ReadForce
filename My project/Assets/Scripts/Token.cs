using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Token : MonoBehaviour
{
    public GameObject objectToTransfer;
    public GameObject GameManager;

    public void LoadSceneWithObject(string sceneName)
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            // Créez l'objet dans la scène actuelle
            

            // Marquez l'objet pour qu'il ne soit pas détruit lors du chargement de la nouvelle scène
            DontDestroyOnLoad(objectToTransfer);
            DontDestroyOnLoad(GameManager);

            // Chargez la nouvelle scène de manière asynchrone
            SceneManager.LoadSceneAsync(sceneName).completed += (AsyncOperation op) =>
            {
                Debug.Log("ahh");
                // Une fois la nouvelle scène chargée, déplacez l'objet dans la nouvelle scène
                SceneManager.MoveGameObjectToScene(objectToTransfer, SceneManager.GetSceneByName(sceneName));
                SceneManager.MoveGameObjectToScene(GameManager, SceneManager.GetSceneByName(sceneName));
            };
        }
        
    }
}
