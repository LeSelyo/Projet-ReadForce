using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ModSliderControl2 : MonoBehaviour
{
    [SerializeField]
    private float sliderMin = 1;
    [SerializeField]
    private float sliderMax = 100;

    public float Value { get; private set; } = 50;
    public Slider ChildSlider2 { get; private set; }
    public Action OnSliderValueChanged { get; set; }

    private InputField valueInput2 { get; set; }
    // Start is called before the first frame update


    void Start()
    {
        ChildSlider2 = transform.Find("Slider2").transform.GetComponent<Slider>();
        ChildSlider2.minValue = sliderMin;
        ChildSlider2.maxValue = sliderMax;
        ChildSlider2.value = Value;
        ChildSlider2.onValueChanged.AddListener(delegate {
            Value = (int)ChildSlider2.value;
            OnSliderValueChanged.Invoke();
        });

        valueInput2 = transform.Find("ValueInput2").transform.GetComponent<InputField>();
        valueInput2.onValueChanged.AddListener(delegate {
            Value = float.Parse(valueInput2.text);
        });

    }

    void updateControls()
    {
        valueInput2.text = Value.ToString();
        ChildSlider2.value = Value;
    }

    // Update is called once per frame
    void Update()
    {
        updateControls();
    }
}
