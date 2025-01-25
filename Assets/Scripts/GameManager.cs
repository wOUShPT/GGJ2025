using System;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        ResetCounter();
    }

    public TextMeshProUGUI counterText;
    private int _counter;
    public int Counter => _counter;


    public void IncreaseCounter(int value)
    {
        _counter += value;
        UpdateCounter();
    }

    public void ResetCounter()
    {
        _counter = 0;
        UpdateCounter();
    }

    private void UpdateCounter()
    {
        counterText.text = _counter.ToString();
    }
}
