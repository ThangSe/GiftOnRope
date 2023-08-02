using UnityEngine;
using UnityEngine.UI;

public class NewPopUp : IItem
{
    private GameObject _object;
    private float _speed = 100f;
    private float _disappearTimer;
    private float _disappearTimerMax = 1f;
    private float _disappearSpeed = 1f;
    private Color _defaultTextColor = Color.black;
    private Color _textColor;

    public NewPopUp(GameObject newObject)
    {
        _object = newObject;
    }

    public void Act()
    {
        _disappearTimer -= Time.deltaTime;
        if (_disappearTimer < 0)
        {
            Deactive();
        }
        else
        {
            _object.transform.position += Vector3.up * _speed * Time.deltaTime;
            _textColor.a -= _disappearSpeed * Time.deltaTime;
            _object.transform.GetComponent<Text>().color = _textColor;
        }
    }

    public void Active(Vector3 pos, Vector3 dir)
    {
        throw new System.NotImplementedException();
    }

    public void Active(Vector3 pos, Vector3 dir, string text)
    {
        _object.transform.GetComponent<Text>().color = _defaultTextColor;
        _object.transform.GetComponent<Text>().text = "+" + text + " Point";
        _textColor = _defaultTextColor;
        _disappearTimer = _disappearTimerMax;
        _object.transform.position = pos;
        _object.SetActive(true);
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