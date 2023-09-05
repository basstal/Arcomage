using System.Collections;
using System.Collections.Generic;
using Arcomage.GameScripts.Utils;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using NOAH;
// using NOAH.Criware;

public class ToggleSlider : MonoBehaviour 
{
    public delegate void OnValueChanged(bool flag);
    public OnValueChanged OnValueChangedCallback;

    public  bool isOn;

    public Color onColorBg;
    public Color offColorBg;

    public Color onColorHandle;
    public Color offColorHandle;

    public Image toggleBgImage;
    public Image toggleHandleImage;

    public RectTransform toggle;

    public GameObject handle;
    private RectTransform handleTransform;

    private float handleSize;
    private float onPosX;
    private float offPosX;

    [OnValueChanged("Awake")]
    public float handleOffset;

    public GameObject onIcon;
    public GameObject offIcon;


    public float speed;
    static float t = 0.0f;

    private bool switching = false;

    void Awake()
    {
        handleTransform = handle.GetComponent<RectTransform>();
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleSize = handleRect.sizeDelta.x;
        float toggleSizeX = toggle.sizeDelta.x;
        onPosX = (toggleSizeX / 2) - (handleSize / 2) - handleOffset;
        offPosX = onPosX * -1;
    }

    void OnEnable()
    {
        toggleBgImage.color = isOn? onColorBg : offColorBg;
        toggleHandleImage.color = isOn? onColorHandle: offColorHandle;
        handleTransform.anchoredPosition = new Vector3(isOn ? onPosX : offPosX, 0f, 0f);
        float alpha = isOn ? 1 : 0;
        if (onIcon)
        {
            onIcon.SetActive(isOn);
            Transparency(onIcon, alpha, alpha);
        }

        if (offIcon)
        {
            offIcon.SetActive(!isOn);
            Transparency(offIcon, 1 - alpha, 1 - alpha);
        }
        
    }
        
    void Update()
    {
        if (switching)
        {
            Toggle(isOn);
        }
    }

    public void Switching()
    {    
        switching = true;
         // if (TryGetComponent<CriwareAudioAssist>(out var criAudioObject))
         // {
         //     criAudioObject.PlaySound("Click");
         // }
    }
        
    public void Toggle(bool toggleStatus)
    {
        if ((onIcon && !onIcon.activeSelf) || (offIcon && !offIcon.activeSelf))
        {
            if (onIcon)
            {
                onIcon.SetActive(true);
            }
            if (offIcon)
            {
                offIcon.SetActive(true);
            }
        }
        
        if (toggleStatus)
        {
            toggleBgImage.color = SmoothColor(onColorBg, offColorBg);
            toggleHandleImage.color = SmoothColor(onColorHandle, offColorHandle);
    
            Transparency (onIcon, 1f, 0f);
            Transparency (offIcon, 0f, 1f);

            handleTransform.anchoredPosition = SmoothMove(handle, onPosX, offPosX);
        }
        else 
        {
            toggleBgImage.color = SmoothColor(offColorBg, onColorBg);
            toggleHandleImage.color = SmoothColor(offColorHandle, onColorHandle);

            Transparency (onIcon, 0f, 1f);
            Transparency (offIcon, 1f, 0f);
            
            handleTransform.anchoredPosition = SmoothMove(handle, offPosX, onPosX);
        }
    }

    Vector3 SmoothMove(GameObject toggleHandle, float startPosX, float endPosX)
    {        
        Vector3 position = new Vector3 (Mathf.Lerp(startPosX, endPosX, t += speed * Time.unscaledDeltaTime), 0f, 0f);
        StopSwitching();
        return position;
    }

    Color SmoothColor(Color startCol, Color endCol)
    {
        Color resultCol;
        resultCol = Color.Lerp(startCol, endCol, t += speed * Time.unscaledDeltaTime);
        return resultCol;
    }

    void Transparency (GameObject alphaObj, float startAlpha, float endAlpha)
    {
        if (alphaObj)
        {
            float value = Mathf.Lerp(startAlpha, endAlpha, t += speed * Time.unscaledDeltaTime);
            Graphic graphic = alphaObj.GetComponent<Graphic>();
            if(graphic)
            {
                graphic.SetAlpha(value);   
            }
            else
            {
                CanvasGroup cg = alphaObj.GetComponent<CanvasGroup>();
                if(cg)
                {
                    cg.alpha = value;
                }
            }
        }
    }

    void StopSwitching()
    {
        if (t > 1.0f)
        {
            switching = false;

            t = 0.0f;
            switch(isOn)
            {
            case true:
                isOn = false;
                OnValueChangedCallback(isOn);
                break;

            case false:
                isOn = true;
                OnValueChangedCallback(isOn);
                break;
            }
        }
    }
}
