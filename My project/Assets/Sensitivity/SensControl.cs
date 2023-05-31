using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ModSliderControl2))]
public class SensControl : MonoBehaviour
{
    private ModSliderControl2 ModSliderControl2;
    // Start is called before the first frame update
    void Start()
    {
        ModSliderControl2 = transform.GetComponent<ModSliderControl2>();
        ModSliderControl2.OnSliderValueChanged += OnChange;
    }

    private void OnChange()
    {
        MouseCursor.Sensitivity = ModSliderControl2.Value;
        GameStartParameters.Sensitivity = ModSliderControl2.Value;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
