using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.TapTapAim;
using Assets.TapTapAim;
using Assets.TapTapAim.LineUtility;
using UnityEngine;
using System.Globalization;

namespace Assets.Scripts.TapTapAim
{
    public class TapTapAimSetup : MonoBehaviour, ITapTapAimSetup
    {
        public static double visibleStartOffsetMs = 400;
        private bool showSliders { get; } = true;
        private bool showLinerSlider { get; } = true;
        private bool showQuadraticSlider { get; } = true;
        private bool showCircles { get; } = true;
        public bool isAutoPlay { get; } = false;

        public Transform HitCircleTransform;
        public Transform CircleTransform;
        public Transform HitSliderTransform;
        public Transform Slider;
        public Transform SliderPositionRing;
        public Transform SliderHitCircleTransform;



        public bool interactWithSliderPositionRing { get; } = true;

        /// <summary>
        /// set a window of how innaccurate a hit can be to still be count as perfect
        /// </summary>
        public static int AccuracyLaybackMs { get; } = 100;
        public Transform PlayArea { get; set; }
        public List<IInteractable> ObjectInteractQueue { get; } = new List<IInteractable>();
        public List<IQueuable> ObjActivationQueue { get; } = new List<IQueuable>();
        public ITracker Tracker { get; set; }

        private bool Ready { get; set; }
        /// <summary>
        /// offset for start of map to give player some time to get ready
        /// </summary>
        public double OffsetForMapStartMs { get; private set; } = 2000;
        public bool AddOffset { get; set; }

        public AudioSource MusicSource { get; set; }

        public AudioSource HitSource { get; set; }
        private int PrevGroupID { get; set; } = -1;
        private int GroupIDCount { get; set; } = 0;

        private int InteractionID { get; set; } = -1;


        void Start()
        {
            PlayArea = GameObject.Find("PlayArea").transform;
            HitSource = GameObject.Find("HitSource").GetComponent<AudioSource>();
            MusicSource = GameObject.Find("MusicSource").GetComponent<AudioSource>();
            MusicSource.clip = GameStartParameters.MapJson.audioClip;
            MusicSource.pitch = GameStartParameters.PlayBackSpeed;
            Tracker = GameObject.Find("Tracker").GetComponent<Tracker>();
            Tracker.TapTapAimSetup = this;

            if (double.Parse(GameStartParameters.MapJson.map[0].Split(',')[2], CultureInfo.InvariantCulture) <
                OffsetForMapStartMs + visibleStartOffsetMs)
            {
                AddOffset = true;
                Tracker.StartOffsetMs = OffsetForMapStartMs;
            }
            else
            {
                Tracker.UseMusicTimeline = true;
            }

            InstantiateObjects();
        }

        private void AddInteractable(IInteractable interactableObject)
        {
            interactableObject.InteractionID = InteractionID += 1;
            ObjectInteractQueue.Add(interactableObject);
        }

        void InstantiateObjects()
        {

            for (var index = 0; index < GameStartParameters.MapJson.map.Count; index++)
            {

                var hitObject = GameStartParameters.MapJson.map[index].Split(',');
                if (hitObject.Length == 7)
                {
                    //spinner
                }
                else if (hitObject.Length == 6)
                {
                    //circle
                    if (showCircles)
                    {
                        var circle = CreateHitCircle(index, hitObject);
                        AddInteractable(circle);
                        ObjActivationQueue.Add(circle);

                        circle.name = ObjActivationQueue.Count - 1 + "-HitCircle";
                        circle.QueueID = ObjActivationQueue.Count - 1;
                    }
                }
                else
                {
                    //slider
                    if (showSliders)
                    {
                        var slider = CreateHitSlider(index, hitObject);
                        if (slider != null)
                        {
                            //ObjectInteractQueue.Add(slider);
                            ObjActivationQueue.Add(slider);
                            slider.QueueID = ((SliderHitCircle)slider.InitialHitCircle).QueueID = ObjActivationQueue.Count - 1;
                            AddInteractable(slider.InitialHitCircle);
                            if (interactWithSliderPositionRing)
                                AddInteractable(slider.SliderPositionRing);

                            slider.name = ObjActivationQueue.Count - 1 + "-HitSlider";
                        }
                    }
                }
            }

            for (int i = 0; i < ObjActivationQueue.Count; i++)
            {
                ObjActivationQueue[i].QueueID = i;
            }

            var count = 0;
            for (int i = ObjActivationQueue.Count - 1; i >= 0; i--)
            {
                ((MonoBehaviour)ObjActivationQueue[i]).transform.SetSiblingIndex(count);
                count++;
            }
            Tracker.SetGameReady();
        }

        private HitSlider CreateHitSlider(int index, string[] hitObject)
        {

            var format = new SliderFormat(hitObject);
            if ((!showQuadraticSlider && format.type == SliderType.BezierCurve))
                return null; // not implemented yet


            var instance = Instantiate(HitSliderTransform, PlayArea).GetComponent<HitSlider>();
            instance.TripMs = (float)format.tripMs;


            instance.TapTapAimSetup = this;
            var transform = instance.transform.GetComponent<RectTransform>();
            transform.anchoredPosition = new Vector3(0, 0, 0);
            transform.sizeDelta = new Vector3(0, 0, -0.1f);
            transform.anchorMin = new Vector2(0f, 0f);
            transform.anchorMax = new Vector2(0f, 0f);

            instance.transform.localScale = new Vector2(1f, 1f);

            if (format.group == PrevGroupID)
            {
                instance.Number = GroupIDCount += 1;
            }
            else
            {
                PrevGroupID = format.group;
                instance.Number = GroupIDCount = 1;
            }

            var circleFormat = new CircleFormat
            {
                x = format.x,
                y = format.y,
                timeInMs = format.timeInMs,
                @group = format.group
            };

            var sliderHitcircleInstance = CreateSliderHitCircle(circleFormat);
            sliderHitcircleInstance.transform.SetParent(instance.transform);
            sliderHitcircleInstance.name = "SliderHitCircle";

            if (!float.IsNaN(format.points[0].x) && !float.IsNaN(format.points[0].y))
            {
                sliderHitcircleInstance.transform.localPosition = format.points[0];
            }
            else
            {
                Debug.LogWarning("Invalid slider position detected. Skipping...");
            }


            var sliderPositionRingInstance = Instantiate(SliderPositionRing, instance.transform).GetComponent<SliderPositionRing>();
            sliderPositionRingInstance.GetComponent<RectTransform>().position = sliderHitcircleInstance.GetComponent<RectTransform>().position;

            sliderPositionRingInstance.name = "SliderPositionRing";
            sliderPositionRingInstance.TapTapAimSetup = this;
            Slider sliderInstance = NewSliderInstance(format, instance, sliderPositionRingInstance);

            instance.SetUp(
                sliderHitcircleInstance,
                sliderInstance,
                sliderPositionRingInstance,
                GetPerfectTime(format),
                format.sliderTrips, this);

            return instance;

        }

        private Slider NewSliderInstance(SliderFormat format, HitSlider instance, SliderPositionRing sliderPositionRingInstance)
        {
            var sliderInstance = Instantiate(Slider, transform).GetComponent<Slider>();
            sliderInstance.transform.SetParent(instance.transform);
            sliderInstance.LineRenderer = instance.GetComponent<LineRenderer>();
            sliderInstance.SliderPositionRing = sliderPositionRingInstance;
            sliderInstance.Points = format.points;
            sliderInstance.SliderType = format.type;
            sliderInstance.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
            sliderInstance.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            sliderInstance.GetComponent<RectTransform>().position = new Vector3(0, 0, 0);
            sliderInstance.DrawSlider();
            return sliderInstance;
        }

        private double GetPerfectTime(Format format)
        {
            return format.timeInMs;
            //return format.timeInMs + (AddOffset ? OffsetForMapStartMs : 0);
        }

        private HitCircle CreateHitCircle(int index, string[] hitObject)
        {
            var instance = Instantiate(HitCircleTransform, PlayArea).GetComponent<HitCircle>();
            instance.TapTapAimSetup = this;

            var format = new CircleFormat(hitObject);
            if (format.group == PrevGroupID)
            {
                instance.GroupNumberShownOnCircle = GroupIDCount += 1;
            }
            else
            {
                PrevGroupID = format.group;
                instance.GroupNumberShownOnCircle = GroupIDCount = 1;
            }
            instance.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            instance.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

            instance.GetComponent<RectTransform>().anchoredPosition = new Vector3(format.x, format.y, 0);
            instance.transform.localScale = new Vector2(1f, 1f);

            instance.PerfectInteractionTimeInMs = GetPerfectTime(format);
            return instance;
        }

        private SliderHitCircle CreateSliderHitCircle(CircleFormat circleFormat)
        {
            var instance = Instantiate(SliderHitCircleTransform, PlayArea).GetComponent<SliderHitCircle>();
            instance.TapTapAimSetup = this;


            instance.name = "Hit Circle";
            instance.GroupNumberShownOnCircle = GroupIDCount;

            instance.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
            instance.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);

            instance.transform.GetComponent<RectTransform>().anchoredPosition = new Vector3(circleFormat.x, circleFormat.y, 0);
            instance.transform.localScale = new Vector2(1f, 1f);

            instance.PerfectInteractionTimeInMs = GetPerfectTime(circleFormat);
            return instance;
        }

        private class Format
        {
            public float x;
            public float y;
            public double timeInMs;
            public int group { get; set; }
        }

        private class SpinnerFormat : Format
        {
            public SpinnerFormat() { }

        }
        private class CircleFormat : Format
        {
            public CircleFormat() { }
            public CircleFormat(string[] split)
            {

                x = float.Parse(split[0]);
                y = float.Parse(split[1]);
                timeInMs = int.Parse(split[2]);
                if (split.Length > 3)
                    group = int.Parse(split[3]);
            }
        }
        private class SliderFormat : Format
        {

            private List<Vector3> vectors = new List<Vector3>();

            public int sliderTrips = 0;
            public double tripMs;
            public SliderType type;
            public SliderFormat() { }
            public List<Vector3> points = new List<Vector3>();
            public SliderFormat(string[] split)
            {
                x = float.Parse(split[0]);
                y = float.Parse(split[1]);
                timeInMs = int.Parse(split[2]);
                group = int.Parse(split[3]);
                var typeAndAnchorSplit = split[5].Split('|');


                var anchors = typeAndAnchorSplit.Skip(1).ToArray();
                foreach (var anchor in anchors)
                {
                    var xy = anchor.Split(':').Select(float.Parse).ToArray();
                    vectors.Add(new Vector2(xy[0], xy[1]));
                }
                switch (typeAndAnchorSplit[0])
                {
                    case "L":
                        type = SliderType.LinearLine;
                        points = LinearLine.GetPoints(new Vector2(x, y), vectors[0]);
                        break;
                    case "P":
                        {
                            type = SliderType.PerfectCurve;
                            var list = new List<Vector3>(vectors);
                            list.Insert(0, new Vector3(x, y, 0));

                            points = PerfectCurve.GetPoints(list);
                            break;
                        }
                    case "B":
                        {
                            type = SliderType.BezierCurve;
                            var list = new List<Vector3>(vectors);
                            list.Insert(0, new Vector3(x, y, 0));
                            points = BezierCurve.GetPoints(list);
                            break;
                        }
                }
                sliderTrips = int.Parse(split[6]);
                tripMs = double.Parse(split[7], CultureInfo.InvariantCulture);
            }

            private void SetPoints()
            {

            }
        }


    }
}
