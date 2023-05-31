using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.TapTapAim;
using UnityEngine;
using UnityEngine.SceneManagement;
using HitCircle = Assets.Scripts.TapTapAim.HitCircle;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
    public class MouseCursor : MonoBehaviour
    {
         // Assurez-vous de définir votre curseur ici dans l'inspecteur Unity
        [SerializeField] private PlayerInput playerInput = null;
        public float Speed = 0.6f;
        public static float Sensitivity = 1;
        float t;
        Vector3 startPosition;
        Vector3 target;
        double timeToReachTarget;
        private TapTapAimSetup tapTapAimSetup;
        private IInteractable OnObject { get; set; }
        public bool IsGame { get; set; }
        private float Radius { get; set; }
        private IInteractable currentTarget { get; set; }
        private InputAction activateAction { get; set; }
        private InputAction activateAction2 { get; set; }

        public PlayerInput PlayerInput => playerInput;

        void Start()
        {

            Cursor.lockState = CursorLockMode.Confined;
            activateAction = PlayerInput.actions.FindAction("Activate");
            activateAction2 = PlayerInput.actions.FindAction("Activate2");
            Cursor.visible = false;
            OnObject = null;
            startPosition = new Vector3(0,0, 0);
            IsGame = SceneManager.GetActiveScene().name == "TapTapAim";
            if (IsGame)
            {
                tapTapAimSetup = GameObject.Find("Tracker").transform.GetComponent<TapTapAimSetup>();
                Radius = transform.GetComponent<CircleCollider2D>().radius;
            }
            Debug.Log(IsGame);


        }

        // Update is called once per frame
        void Update()
        {
            if (IsGame)
            {
                if (!(tapTapAimSetup.Tracker.NextObjToHit < tapTapAimSetup.ObjectInteractQueue.Count()))
                    return;
                var nextObj = ((MonoBehaviour)tapTapAimSetup.ObjectInteractQueue[tapTapAimSetup.Tracker.NextObjToHit]).transform;
                //Debug.Log("Next object to hit: " + ((IHittable)tapTapAimSetup.ObjectInteractQueue[tapTapAimSetup.Tracker.NextObjToHit]).InteractionID);
                if (tapTapAimSetup.isAutoPlay)
                {
                    var objTarget = nextObj.GetComponent<IInteractable>();
                    if (currentTarget != objTarget)
                    {
                        Debug.LogWarning($"Cursor target = HitId:{objTarget.InteractionID}");
                        currentTarget = objTarget;
                    }

                    //pos.y += 0.1f;
                    if (nextObj.transform.GetComponent<IHoldable>() == null)
                        SetDestination(nextObj.position, ((nextObj.GetComponent<IInteractable>().PerfectInteractionTimeInMs - tapTapAimSetup.Tracker.GetTimeInMs()) / 1000) * Speed);
                    else
                    {
                        SetDestination(nextObj.position, 0);
                    }
                    t += Time.deltaTime / (float)timeToReachTarget;
                    transform.position = Vector3.Lerp(startPosition, target, t);
                }
                /*else
                {
                    Vector2 mouseMovement = PlayerInput.actions["Mouse"].ReadValue<Vector2>();

                    float mouseX = mouseMovement.x * Sensitivity * Time.deltaTime;
                    float mouseY = mouseMovement.y * Sensitivity * Time.deltaTime;

                    Vector3 newPosition = new Vector3(mouseX, mouseY, 0f);
                    transform.position += newPosition;
                    Debug.Log(transform.position);
                }*/
                else
                {
                    Vector2 mouseMovement = PlayerInput.actions["Mouse"].ReadValue<Vector2>();

                    // Multipliez par la sensibilité pour contrôler la vitesse de mouvement du curseur
                    float mouseX = mouseMovement.x * Sensitivity * Time.deltaTime;
                    float mouseY = mouseMovement.y * Sensitivity * Time.deltaTime;

                    Vector3 newPosition = new Vector3(mouseX, mouseY, 0f);
                    transform.position += newPosition;
                    Debug.Log(transform.position);
                }

                RayCast();
            }
            else
            {
                transform.position = Input.mousePosition;
                //Debug.Log(transform.position);
            }
            transform.Rotate(Vector3.forward * -1000 * Time.deltaTime);

            //Debug.DrawLine(transform.position, transform.forward);
            //Debug.DrawRay(transform.position,transform.forward);

        }

        public void RayCast()
        {
            try
            {
                var hits = Physics2D.CircleCastAll(transform.position, Radius, transform.forward, 5);
                if (IsInteractable(hits, out var interactable))
                {
                    if (OnObject != interactable)
                    {
                        Debug.LogWarning($"ontop of {interactable.InteractionID} {((MonoBehaviour)interactable).name}");
                        OnObject = interactable;
                    }

                    if (tapTapAimSetup.isAutoPlay)
                    {

                        switch (interactable)
                        {
                            case HitCircle hitCircle:
                                if (!hitCircle.IsHitAttempted && hitCircle.IsInAutoPlayHitBound(tapTapAimSetup.Tracker.GetTimeInMs()))
                                {
                                    Debug.LogWarning("Try hit: " + hitCircle.name);

                                    hitCircle.TryInteract();
                                    GameObject.FindWithTag("TapCounter").GetComponent<TapTicker>().IncrementButton(1);
                                }
                                break;

                            case SliderHitCircle sliderHitCircle:
                                {

                                    if (!sliderHitCircle.IsHitAttempted && sliderHitCircle.IsInAutoPlayHitBound(tapTapAimSetup.Tracker.GetTimeInMs()))
                                    {
                                        Debug.LogWarning("Try hit: " + sliderHitCircle.name);

                                        sliderHitCircle.TryInteract();
                                        GameObject.FindWithTag("TapCounter").GetComponent<TapTicker>().IncrementButton(1);
                                    }

                                    break;
                                }
                            case SliderPositionRing sliderPositionRing:
                                {
                                    if (sliderPositionRing.IsInInteractionBound(tapTapAimSetup.Tracker.GetTimeInMs()))
                                    {
                                        Debug.LogWarning("Try interact: " + sliderPositionRing.name);
                                        sliderPositionRing.TryInteract();
                                    }

                                    break;
                                }
                        }

                    }
                    else
                    {
                        if (activateAction.triggered || activateAction2.triggered)
                        {
                            switch (interactable)
                            {
                                case HitCircle hitCircle:
                                    if (!hitCircle.IsHitAttempted && hitCircle.IsInAutoPlayHitBound(tapTapAimSetup.Tracker.GetTimeInMs()))
                                    {
                                        Debug.LogWarning("Try hit: " + hitCircle.name);

                                        hitCircle.TryInteract();
                                        GameObject.FindWithTag("TapCounter").GetComponent<TapTicker>().IncrementButton(1);
                                    }
                                    break;

                                case SliderHitCircle sliderHitCircle:
                                    {
                                        if (!sliderHitCircle.IsHitAttempted && sliderHitCircle.IsInAutoPlayHitBound(tapTapAimSetup.Tracker.GetTimeInMs()))
                                        {
                                            Debug.LogWarning("Try hit: " + sliderHitCircle.name);

                                            sliderHitCircle.TryInteract();
                                            GameObject.FindWithTag("TapCounter").GetComponent<TapTicker>().IncrementButton(1);
                                        }

                                        break;
                                    }
                                case SliderPositionRing sliderPositionRing:
                                    {
                                        if (sliderPositionRing.IsInInteractionBound(tapTapAimSetup.Tracker.GetTimeInMs()))
                                        {
                                            Debug.LogWarning("Try interact: " + sliderPositionRing.name);
                                            sliderPositionRing.TryInteract();
                                            //GameObject.FindWithTag("TapCounter").GetComponent<TapTicker>().IncrementButton(1);
                                        }

                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }


        }

        private bool IsInteractable(RaycastHit2D[] array, out IInteractable interactableObject)
        {
            var interactables = new List<IInteractable>();
            foreach (var raycastHit2D in array)
            {


                if (raycastHit2D.transform.GetComponent<IHitCircle>() != null)
                    interactables.Add(raycastHit2D.transform.GetComponent<IHitCircle>());
                else if (raycastHit2D.transform.GetComponent<ISliderHitCircle>() != null)
                    interactables.Add(raycastHit2D.transform.GetComponent<ISliderHitCircle>());
                else if (tapTapAimSetup.interactWithSliderPositionRing && raycastHit2D.transform.GetComponent<ISliderPositionRing>() != null)
                    interactables.Add(raycastHit2D.transform.GetComponent<ISliderPositionRing>());

            }
            if (!interactables.Any())
            {

                interactableObject = null;
                return false;
            }


            int minId = interactables.Select(h => h.InteractionID).ToList().Min();
            interactableObject = interactables.Single(h => h.InteractionID == minId);

            if (interactableObject.GetType() == typeof(HitCircle))
            {
                return true;
            }
            else if (interactableObject.GetType() == typeof(SliderHitCircle))
            {
                return true;
            }
            else if (interactableObject.GetType() == typeof(SliderPositionRing))
            {
                return true;
            }

            interactableObject = null;
            return false;
        }

        public void SetDestination(Vector3 destination, double time)
        {
            t = 0;
            startPosition = transform.position;
            timeToReachTarget = time;
            target = destination;
        }
    }
}
