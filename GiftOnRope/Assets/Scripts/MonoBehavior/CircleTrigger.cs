using System;
using System.Collections;
using UnityEngine;

public class CircleTrigger : MonoBehaviour, IDelay
{
    public static event EventHandler OnPassThroughCircle;

    public static void ResetStaticData()
    {
        OnPassThroughCircle = null;
    }

    [SerializeField] private Camera _camera;
    [SerializeField] private float speed = 10f;
    private Vector3 _nextPosition = Vector3.zero;
    private bool _isPass;
    private float _offsetFromScreenZ = -10f;
    private float _distaneToTopScreen = 3f;
    private float _distanceToNextCircle = 6f;
    private float _minSpawnPosX = -1.6f;
    private float _maxSpawnPosX = 1.6f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(DelayTime(.15f));
    }

    public IEnumerator DelayTime(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        OnPassThroughCircle?.Invoke(this, EventArgs.Empty);
        _nextPosition = new Vector3(0, transform.position.y - _distaneToTopScreen, _offsetFromScreenZ);
        _isPass = true;
        transform.position = RandomPos();
    }

    private void Update()
    {
        if (IsPassCircle())
        {
            MoveToNextPos();
        }
        if (Mathf.RoundToInt(_camera.transform.position.y) == Mathf.RoundToInt(_nextPosition.y) && IsPassCircle())
        {
            _isPass = false;
        }        
    }

    private void MoveToNextPos()
    {
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _nextPosition, speed * Time.deltaTime);
    }

    private bool IsPassCircle()
    {
        return _isPass;
    }

    private Vector3 RandomPos()
    {
        return new Vector3(UnityEngine.Random.Range(_minSpawnPosX, _maxSpawnPosX), transform.position.y - _distanceToNextCircle);
    }
}