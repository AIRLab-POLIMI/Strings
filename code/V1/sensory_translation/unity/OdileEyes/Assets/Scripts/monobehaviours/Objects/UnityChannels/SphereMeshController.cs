using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Renderer))]
public class SphereMeshController : MonoBehaviour
{
    private new Renderer renderer; // new hides the parent <renderer> property.
    private Material material;
    Color emissionColor;
    
    [NonSerialized] public float targetBrightness;
    [NonSerialized] public float targetScale;
    
    private float _currentScale;
    private float _currentBrightness;
    
    private void Start()
    {
        // Gets access to the renderer and material components as we need to
        // modify them during runtime.
        renderer = GetComponent<Renderer>();
        material = renderer.material;
        
        // Gets the initial emission colour of the material, as we have to store
        // the information before turning off the light.
        emissionColor = material.GetColor("_EmissionColor");
        
        // Enables emission for the material, and make the material use
        // realtime emission.
        material.EnableKeyword("_EMISSION");
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        
        // disable it
        gameObject.SetActive(false);
    }

    public void Init(float initScale, float initBrightness)
    {
        _currentScale = initScale;
        targetScale = initScale;
        SetScale(initScale);
        
        _currentBrightness = initBrightness;
        targetBrightness = initBrightness;
        
        Debug.Log(" -----------INIT!");
        SetBrightness(initBrightness);
    }

    public void OnInputChanged(float newTargetBrightness, float newTargetScale)
    {
        targetBrightness = newTargetBrightness;
        targetScale = newTargetScale;
    }

    private void Update()
    {
        if (Mathf.Abs(_currentScale - targetScale) > 0.01f)
        {
            _currentScale = Mathf.Lerp(_currentScale, targetScale, 0.1f);
            SetScale(_currentScale);
        }
        
        if (Mathf.Abs(_currentBrightness - targetBrightness) > 0.05f)
        {
            Debug.Log(" -----------SB!!");
            _currentBrightness = Mathf.Lerp(_currentBrightness, targetBrightness, 0.1f);
            SetBrightness(_currentBrightness);
        }
    }

    private void SetBrightness(float intensity)
    {
        Debug.Log(material.color);
        
        // Update the emission color and intensity of the material.
        material.SetColor("_EmissionColor", emissionColor * intensity);

        // Makes the renderer update the emission and albedo maps of our material.
        RendererExtensions.UpdateGIMaterials(renderer);

        // Inform Unity's GI system to recalculate GI based on the new emission map.
        DynamicGI.SetEmissive(renderer, emissionColor * intensity);
        DynamicGI.UpdateEnvironment();
    }
    
    private void SetScale(float newScale) => 
        transform.localScale = Vector3.one * newScale;
}
