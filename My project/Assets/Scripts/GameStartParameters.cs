using UnityEngine;

namespace Assets.Scripts
{
    public class GameStartParameters : MonoBehaviour {

        public static StartMapJson MapJson = new StartMapJson();
        public static float PlayBackSpeed = 1;
        public static float Sensitivity = 5;
        // Use this for initialization

        public class StartMapJson : MapJson
        {
            public AudioClip audioClip;
         


        }
    }
}
