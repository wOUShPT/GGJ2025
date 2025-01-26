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
    public int Counter => _counter;

    public void StartGame()
    {
        StartCoroutine(StarGameCoroutine());
    }

    private IEnumerator StarGameCoroutine()
    {
        yield return new WaitForSeconds(1f);    
    }

    public void IncreaseCounter(int value)
    {
        _counter += value;
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
        _counter = 0;
        UpdateCounter();
    }

    private void UpdateCounter()
    {
        counterText.text = _counter.ToString();
    }
}
