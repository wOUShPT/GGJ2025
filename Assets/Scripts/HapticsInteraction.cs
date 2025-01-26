using System.Collections;
using AstralShift.QTI.Interactions;
using AstralShift.QTI.Interactors;
using UnityEngine;

public class HapticsInteraction : Interaction
{
    [SerializeField] 
    private long durationInMs;
    [SerializeField] 
    private int amplitude = 10;
    private Coroutine _vibrateCoroutine;
    public override void Interact(IInteractor interactor)
    {
        RDG.Vibration.Vibrate(durationInMs, amplitude, true);
        //Vibration.VibrateAndroid(durationInMs);
        base.Interact(interactor);
        OnEnd();
    }

    private void Vibrate(float duration)
    {
        if (!enabled || !gameObject.activeSelf)
        {
            return;
        }
        
        if (_vibrateCoroutine != null)
        {
            StopCoroutine(_vibrateCoroutine);
        }
        
        _vibrateCoroutine = StartCoroutine(VibrateCoroutine(duration));
    }
    
    private IEnumerator VibrateCoroutine(float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            Handheld.Vibrate();
            yield return null;
        }
    
        _vibrateCoroutine = null;
    }
}
