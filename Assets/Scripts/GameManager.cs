using AstralShift.QTI.Helpers;
using System;
using System.Collections;
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
        animator = counterText.GetComponent<Animator>();
        ResetCounter();
        counterText.gameObject.SetActive(false);
    }

    public GameObject game;
    public TextMeshProUGUI counterText;
    private int _counter;
    private Animator animator;
    public Animator logoAnimator;
    public int Counter => _counter;

    private const string CounterPrefsKey = "Bomble/Counter";

    public void StartGame()
    {
        StartCoroutine(StarGameCoroutine());
    }

    private IEnumerator StarGameCoroutine()
    {
        logoAnimator.Play("INTROopen");
        yield return new WaitForSeconds(1f);
        game.SetActive(true);
        counterText.gameObject.SetActive(true);
    }

    public void IncreaseCounter(int value)
    {
        _counter += value;
        PlayerPrefs.SetInt(CounterPrefsKey, _counter);
        if (value > 0)
        {
            animator.Play("increase");
        }
        else
        {
            animator.Play("decrease");
        }
        UpdateCounter();
    }

    public void ResetCounter()
    {
        _counter = PlayerPrefs.GetInt(CounterPrefsKey, 0);
        UpdateCounter();
    }

    private void UpdateCounter()
    {
        counterText.text = _counter.ToString();
    }
}
