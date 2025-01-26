using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class MovementBehaviour : MonoBehaviour
{
    [SerializeField] 
    private Transform _transform;
    [SerializeField]
    private Vector3 direction;
    [SerializeField]
    private Vector2 speedRange;
    private float _speed;

    private void Start()
    {
        _speed = Random.Range(speedRange.x, speedRange.y);
    }

    private void Update()
    {
        _transform.position += direction * (_speed * Time.deltaTime);
    }
}
