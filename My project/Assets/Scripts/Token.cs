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
            // Cr�ez l'objet dans la sc�ne actuelle
            

            // Marquez l'objet pour qu'il ne soit pas d�truit lors du chargement de la nouvelle sc�ne
            DontDestroyOnLoad(objectToTransfer);
            DontDestroyOnLoad(GameManager);

            // Chargez la nouvelle sc�ne de mani�re asynchrone
            SceneManager.LoadSceneAsync(sceneName).completed += (AsyncOperation op) =>
            {
                Debug.Log("ahh");
                // Une fois la nouvelle sc�ne charg�e, d�placez l'objet dans la nouvelle sc�ne
                SceneManager.MoveGameObjectToScene(objectToTransfer, SceneManager.GetSceneByName(sceneName));
                SceneManager.MoveGameObjectToScene(GameManager, SceneManager.GetSceneByName(sceneName));
            };
        }
        
    }
}
