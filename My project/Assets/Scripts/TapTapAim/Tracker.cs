using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.TapTapAim;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Debug = UnityEngine.Debug;
using System.Collections;

namespace Assets.Scripts.TapTapAim
{
    public class Tracker : MonoBehaviour, ITracker
    {

        [SerializeField] private GameObject ScoreFinal, BestCombo;
        [SerializeField] private GameObject ZoneScore,ZoneCombo;
        private int nextObjectID { get; set; }
        private bool SkippedToStart;
        public TapTapAimSetup TapTapAimSetup { get; set; }
        public int Score { get; private set; }
        public List<HitScore> HitHistory { get; set; } = new List<HitScore>();
        private float HealthDrain { get; } = 5;
        private float HealthDamage { get; } = 20;
        public float HealthAddedPerHit { get; } = 7;

        
        public float HitAccuracy { get; private set; }
        public List<TimeSpan> BreakPeriodQueue { get; private set; } = new List<TimeSpan>();
        public double StartOffsetMs { get; set; }
        public int NextObjToHit { get; set; } = 0;
        public int NextObjToActivateID { get; set; }
        public int Combo { get; private set; }
        public float Health { get; private set; } = 100;
        public bool IsGameReady { get; set; }
        public bool GameFinished { get; private set; }
        private Stopwatch Stopwatch { get; } = new Stopwatch();
        public bool UseMusicTimeline { get; set; }
        private void Start()
        {


        }
        public void SetGameReady()
        {
            if(!UseMusicTimeline)
            Stopwatch.Start();

            IsGameReady = true;
        }

        private void Update()
        {
            if (!GameFinished)
            {
                if (!IsGameReady)
                    return;
                try
                {
                    if (NextObjToActivateID < TapTapAimSetup.ObjActivationQueue.Count)
                        IterateObjectQueue();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                try
                {
                    //if (NextObjToHit < TapTapAimSetup.ObjectInteractQueue.Count)
                    //IterateInteractionQueue();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                if (IsGameReady && UseMusicTimeline && !GameFinished)
                {
                    if (!TapTapAimSetup.MusicSource.isPlaying)
                        TapTapAimSetup.MusicSource.Play();
                    HandleHealth();
                }
                else
                {
                    GetTimeInMs();
                }
                CalculateAccuracy();
            }
            else
            {
                GameObject EndScreen = GameObject.FindGameObjectWithTag("EndScreen");

                if (!EndScreen.active)
                {
                    EndScreenDisplay();
                }
                else
                    return;


            }



           
            if (Input.GetKey(KeyCode.Escape))
            {
                Debug.Log("ahh");
                if( GameObject.Find("Token") != null)
                {
                    StartCoroutine(LoadSceneAndCallFunctionCoroutine());/*
                    GameManager otherScript = GameObject.Find("GameManager").GetComponent<GameManager>();
                    if (otherScript != null)
                    {
                        otherScript.Disconnected();
                    }
                    SceneManager.LoadScene("multijoueurs");*/

                }
                
                else
                    SceneManager.LoadScene("MapSelect");
            }
            else if (Input.GetKey(KeyCode.Space))
                if ((TapTapAimSetup.MusicSource.time * 1000) - 5000 <
                    TapTapAimSetup.ObjectInteractQueue[0].Visibility.VisibleStartStartTimeInMs && !SkippedToStart)
                {
                    SkippedToStart = true;
                    TapTapAimSetup.MusicSource.time = (float)TapTapAimSetup.ObjectInteractQueue[0].Visibility.VisibleStartStartTimeInMs - 2000f;
                }

        }
        private IEnumerator LoadSceneAndCallFunctionCoroutine()
        {
            // Charge la sc�ne de mani�re asynchrone
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("multijoueurs");

            // Attendez que la sc�ne soit compl�tement charg�e
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // La nouvelle sc�ne est charg�e, recherchez le script et appelez la fonction
            GameManager otherScript = GameObject.Find("GameManager").GetComponent<GameManager>();
            if (otherScript != null)
            {
                otherScript.Disconnected();
            }
        }
        public class Message
        {
            public string text;
            public TMP_Text textObject;
        }

        private void EndScreenDisplay()
        {
            GameObject EndScreen = GameObject.FindGameObjectWithTag("EndScreen");
            GameObject ScreenGame = GameObject.FindGameObjectWithTag("ScreenGame");
            ScreenGame.SetActive(false);
            EndScreen.SetActive(true);
            Message Scores = new Message();
            Message Combot = new Message();


            Scores.text = "Best Score " + ": " + Score;
            GameObject Scorest = Instantiate(ScoreFinal, ZoneScore.transform);
            Scores.textObject = Scorest.GetComponent<TMP_Text>();
            Scores.textObject.text = Scores.text;

            Combot.text = "Best Combo " + ": " + Combo;
            GameObject Combost = Instantiate(ScoreFinal, ZoneCombo.transform);
            Combot.textObject = Combost.GetComponent<TMP_Text>();
            Combot.textObject.text = Combot.text;
        }
        private void CalculateAccuracy()
        {
            try
            {
                float sum = 0;
                var count = 0;

                foreach (var hit in HitHistory)
                {
                    sum += hit.accuracy;
                    count++;
                }

                HitAccuracy = sum / count;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        private void IterateObjectQueue()
        {
            //if (Stopwatch.Elapsed + TimeSpan.FromMilliseconds(500) >= ((IObject)TapTapAimSetup.ObjectInteractQueue[nextObjectID]).VisibleStartStart)
            //{
            //    ((MonoBehaviour)TapTapAimSetup.ObjectInteractQueue[nextObjectID]).gameObject.SetActive(true);
            //    nextObjectID++;
            //}
            if (GetTimeInMs() + (100 * TapTapAimSetup.MusicSource.pitch) >= TapTapAimSetup.ObjActivationQueue[NextObjToActivateID].Visibility.VisibleStartStartTimeInMs)
            {
                ((MonoBehaviour)(TapTapAimSetup).ObjActivationQueue[NextObjToActivateID]).gameObject.SetActive(true);
                NextObjToActivateID++;

            }
            if (nextObjectID == (TapTapAimSetup).ObjActivationQueue.Count && nextObjectID == TapTapAimSetup.ObjectInteractQueue.Count)
                GameFinished = true;
        }

        
        /// <summary>
        /// Time in Ms
        /// Can be negative if offset is applied
        /// </summary>
        /// <returns></returns>
        public double GetTimeInMs()
        {
            if (UseMusicTimeline)
            {
                return TapTapAimSetup.MusicSource.time * 1000;
            }

            if (Stopwatch.Elapsed.TotalMilliseconds - StartOffsetMs >= 0)
            {
                UseMusicTimeline = true;
                Stopwatch.Stop();
                return TapTapAimSetup.MusicSource.time * 1000;
            }
            else
            {
                return Stopwatch.Elapsed.TotalMilliseconds - StartOffsetMs;
            }

        }
        public void IterateInteractionQueue(int? thisId = null)
        {
            if (thisId != null)
            {
                NextObjToHit = (int)thisId + 1;
                //Debug.Log($"set nextHitObj to:{NextObjToHit}");
                return;
            }
            //if (Stopwatch.Elapsed >= (TapTapAimSetup.ObjectInteractQueue[NextObjToHit]).PerfectInteractionTime + TimeSpan.FromMilliseconds(TapTapAimSetup.AccuracyLaybackMs))
            //{
            //    NextObjToHit++;
            //    Debug.Log($"iterate hitQueue to:{NextObjToHit}");
            //}
        }



        private void HandleHealth()
        {
            Health -= Time.deltaTime * HealthDrain;

            if (Health <= 0)
            {
                // GameFinished = true;
            }
            else if (Health > 100)
            {
                Health = 100;
            }
        }

        public void RecordEvent(bool hit, HitScore hitScore = null)
        {
            if (hit)
            {
                Combo++;
                if (hitScore != null)
                {
                    HitHistory.Add(hitScore);
                    Score += hitScore.score * (Combo + 1);

                    Health += HealthAddedPerHit;
                }
            }
            else
            {
                Combo = 0;
                Health -= HealthDamage;
            }
        }


    }
    public class HitScore
    {
        public float accuracy;
        public int id;
        public int score;
    }
}