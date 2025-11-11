using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Difficulty : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public TextMeshProUGUI txt;
    public Arrow game;
    private Slider slider;
    private bool isDragging = false;
    private bool allowValueChange = true;
    public TMP_Text levelText;
    private int level;

    private void Start()
    {
        level = 1;
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        levelText.text = "Level: " + level.ToString();
    }

    public void ChangeText(int num)
    {
        txt.text = num.ToString();
        levelText.text = "Level: " + num.ToString();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Check if pointer is within slider bounds
        if (IsPointerOverSlider(eventData))
        {
            EnableValueChange();  // Allow value change
            isDragging = true;  // Allow slider to react
            slider.OnPointerDown(eventData);  // Trigger slider default behavior
        }
        else
        {
            DisableValueChange();  // Disable value change
            isDragging = false; // Prevent any slider reaction
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            slider.OnDrag(eventData); // Allow dragging if pointer started over the slider
        }
    }

    private bool IsPointerOverSlider(PointerEventData eventData)
    {
        // Check if the pointer is over the slider's RectTransform
        return RectTransformUtility.RectangleContainsScreenPoint(
            slider.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera);
    }

    public void OnSliderValueChanged(float value)
    {
        if (!allowValueChange)
            return;
        else
        {
            int val = Mathf.RoundToInt(value);
            game.Difficulty = val;
            ChangeText(val-1);
        }
    }

    public void EnableValueChange()
    {
        allowValueChange = true;
    }

    public void DisableValueChange()
    {
        allowValueChange = false;
    }
}