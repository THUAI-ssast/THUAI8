using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapUIManager : MonoBehaviour
{
    public static MapUIManager Instance;

    private GameObject _mapPanel;
    private GameObject _smallMapMask;
    private GameObject _bigMapPanel;
    private GameObject _playerPositionImage;
    private GameObject _currentMask;
    private GameObject _nextSafeArea;

    private readonly Vector2 _playerPositionImageBias = new(0.65f, -5);

    private readonly float _lengthPerTile = 316.56f / 143;

    public bool IsDisplayBigMap = false;

    private Vector2 _originalPos;

    private float xBound;
    private float yBound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }
    }
    void Start()
    {
        _mapPanel = UIManager.Instance.MainCanvas.transform.Find("MapPanel").gameObject;
        _smallMapMask = _mapPanel.transform.Find("SmallMapMask").gameObject;
        _bigMapPanel = _mapPanel.transform.Find("SmallMapMask/BigMapImage").gameObject;
        _playerPositionImage = _bigMapPanel.transform.Find("PlayerPositionImage").gameObject;
        _currentMask = _bigMapPanel.transform.Find("SafeArea/CurrentMask").gameObject;
        _nextSafeArea = _bigMapPanel.transform.Find("NextSafeArea").gameObject; 

        xBound = (_bigMapPanel.GetComponent<RectTransform>().rect.width - _smallMapMask.GetComponent<RectTransform>().rect.width) / 2;
        yBound = (_bigMapPanel.GetComponent<RectTransform>().rect.height - _smallMapMask.GetComponent<RectTransform>().rect.height) / 2;
    }

    void Update()
    {
    }

    public void UpdatePlayerPositionImage(Vector3Int playerTilePos)
    {
        _playerPositionImage.GetComponent<RectTransform>().localPosition = TilePosToImagePos(playerTilePos);
        if(!IsDisplayBigMap)
        {
            Vector2 targetPos = -_playerPositionImage.transform.localPosition;
            if (targetPos.x < -xBound)
                targetPos.x = -xBound;
            else if (targetPos.x > xBound)
                targetPos.x = xBound;
            if (targetPos.y < -yBound)
                targetPos.y = -yBound;
            else if(targetPos.y > yBound)
                targetPos.y = yBound;
            _bigMapPanel.transform.localPosition = targetPos;
        }
    }

    public void UpdateSafeArea(int safeAreaLength, Vector2Int safeAreaOrigin, int nextSafeAreaLength, Vector2Int nextSafeAreaOrigin)
    {
        float currentLength = safeAreaLength * _lengthPerTile;
        Vector2 currentOrigin = TilePosToImagePos(safeAreaOrigin);
        float nextLength = nextSafeAreaLength * _lengthPerTile;
        Vector2 nextOrigin = TilePosToImagePos(nextSafeAreaOrigin);

        if(RoundManager.Instance.RoundCount > 1)
        {
            _currentMask.GetComponent<RectTransform>().localPosition = currentOrigin + new Vector2(currentLength, currentLength) / 2;
            _currentMask.GetComponent<RectTransform>().localScale = new Vector2(currentLength, currentLength);
        }
        if(nextSafeAreaLength != -1)
        {
            Debug.Log(nextSafeAreaLength);
            _nextSafeArea.GetComponent<RectTransform>().localPosition = nextOrigin + new Vector2(nextLength, nextLength) / 2;
            _nextSafeArea.GetComponent<RectTransform>().localScale = new Vector2(nextLength, nextLength);
        }
    }

    private Vector2 TilePosToImagePos(Vector3Int tilePos)
    {
        return _playerPositionImageBias + _lengthPerTile * new Vector2(tilePos.x, tilePos.y);
    }

    private Vector2 TilePosToImagePos(Vector2Int tilePos)
    {
        return _playerPositionImageBias + _lengthPerTile * new Vector2(tilePos.x, tilePos.y);
    }

    private Vector3Int ImagePosToTilePos(Vector2 imagePos)
    {
        Vector2 tilePos = (imagePos - _playerPositionImageBias) / _lengthPerTile;
        return new Vector3Int((int)Math.Round(tilePos.x), (int)Math.Round(tilePos.y), 0);
    }

    public void DisplayBigMap()
    {
        IsDisplayBigMap = true;
        _originalPos = _bigMapPanel.transform.localPosition;
        _bigMapPanel.SetActive(false);
        _bigMapPanel.transform.localPosition = Vector2.zero;
        _bigMapPanel.transform.localPosition -= _smallMapMask.transform.localPosition;
        UIManager.Instance.ReversePanel(_bigMapPanel);

        _smallMapMask.GetComponent<RectMask2D>().enabled = false;
        _smallMapMask.GetComponent<Image>().color = new Color(1,1,1,0);
    }

    public void DisplaySmallMap()
    {
        IsDisplayBigMap = false;
        UIManager.Instance.ReversePanel(_bigMapPanel);
        _bigMapPanel.transform.localPosition = _originalPos;
        _bigMapPanel.SetActive(true);

        _smallMapMask.GetComponent<RectMask2D>().enabled = true;
        _smallMapMask.GetComponent<Image>().color = new Color(1, 1, 1, 1);
    }
}
