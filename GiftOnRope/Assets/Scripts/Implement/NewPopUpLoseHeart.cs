using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPopUpLoseHeart : IItem
{
    private GameObject _object;
    private float _speed = 100f;
    private float _disappearTimer;
    private float _disappearTimerMax = 1f;

    public NewPopUpLoseHeart(GameObject newObject)
    {
        _object = newObject;
    }
    public void Act()
    {
        _disappearTimer -= Time.deltaTime;
        Debug.Log("Act");
        if (_disappearTimer < 0)
        {
            Deactive();
        }
        else
        {
            _object.transform.position += Vector3.up * _speed * Time.deltaTime;
        }
    }

    public void Active(Vector3 pos, Vector3 dir)
    {
        _disappearTimer = _disappearTimerMax;
        _object.transform.position = pos;
        _object.SetActive(true);
    }

    public void Active(Vector3 pos, Vector3 dir, string text)
    {
        throw new System.NotImplementedException();
    }

    public bool CompareTagGO(string tag)
    {
        return _object.CompareTag(tag);
    }

    public void Deactive()
    {
        _object.SetActive(false);
    }

    public bool GetActiveState()
    {
        return _object.activeSelf;
    }

    public void Setting(Sprite newSprite, int index)
    {
        throw new System.NotImplementedException();
    }
}
