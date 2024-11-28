using DG.Tweening;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// ������ڵ�ͼ��ͨ�������ͣ�鿴��Ϣ�Ĺ�����
/// </summary>
public class MouseHoverManager : MonoBehaviour
{
    // ����Tilemaps
    private Tilemap _groundTilemap;
    private Tilemap _wallTilemap;
    private Tilemap _furnitureTilemap;
    private Tilemap _glassTilemap;
    private Tilemap _itemTilemap;

    // ���������жϵĸ���Tile
    public Tile DoorTileHorizontal;
    public Tile DoorTileVertical;
    public Tile BrokenGlassTile;

    /// <summary>
    /// �����ͣ��ʾ��UI
    /// </summary>
    private GameObject _mouseHoverPanel;
    /// <summary>
    /// ��ͣ��ʾUI��������λ�õ�ƫ��
    /// </summary>
    private Vector3 _panelBias;
    /// <summary>
    /// ��ͣ��ʾUI�µ��ı�UI
    /// </summary>
    private TextMeshProUGUI _panelText;

    /// <summary>
    /// ��¼��һ�������ͣλ�õ�tile����
    /// </summary>
    private Vector3Int _lastCellPosition;

    /// <summary>
    /// �����ͣ��ʾ��Э��
    /// </summary>
    private Coroutine _showCoroutine;

    /// <summary>
    /// ����ҽ�������tile�ϵ���ͣ��ʾUI���ݵ�ʱ��
    /// </summary>
    private float _showDelay = 0.6f;

    /// <summary>
    /// �Ƿ�����ͣʱ��ʾUI��ָʾ����������ҿ���
    /// </summary>
    private bool _isShowUI = false;

    /// <summary>
    /// ��ʾ����л���ͣ��ʾ״̬��CD��ֻ��ֵΪfalseʱ�л���������Ч
    /// </summary>
    private bool _isSwitchCD = false;

    /// <summary>
    /// ������ʾ�����ͣ�Ƿ�򿪵���Ϣ��Э��
    /// </summary>
    private Coroutine _displayCoroutine;

    /// <summary>
    /// ��¼����Ҵ���������ʽUI֮ǰ�����ͣ��ʾ��״̬����������ҹر���������ʽUI��ָ�֮ǰ������
    /// </summary>
    private bool _isShowUIBefore;


    private void Start()
    {
        // ��ȡ���е�tilemap
        _groundTilemap = transform.GetChild(0).Find("GroundTilemap").GetComponent<Tilemap>();
        _wallTilemap = transform.GetChild(0).Find("WallTilemap").GetComponent<Tilemap>();
        _furnitureTilemap = transform.GetChild(0).Find("FurnitureTilemap").GetComponent<Tilemap>();
        _glassTilemap = transform.GetChild(0).Find("GlassTilemap").GetComponent<Tilemap>();
        _itemTilemap = transform.GetChild(0).Find("ItemTilemap").GetComponent<Tilemap>();

        // ��ʼ����ͣ��ʾUI��صĶ���
        _mouseHoverPanel = UIManager.Instance.MainCanvas.transform.Find("MouseHoverPanel").gameObject;
        _panelBias = new Vector3(120,-60);
        _panelText = _mouseHoverPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void OnDestroy()
    {
        if(_showCoroutine != null)
        {
            StopCoroutine(_showCoroutine);
        }    
    }

    private void Update()
    {
        if (UIManager.Instance.GetActiveUINumber == 0)  // ��������������ʽUIʱ����ֹ�����ͣ���ܣ����米��UI����Դ��UI��ս������UI�ȣ�
        {   
            if (Input.GetKeyDown(KeyCode.C) && !_isSwitchCD)    // ���̰�C�л��Ƿ����������ͣ���ܵ�״̬
            {
                _isSwitchCD = true;
                ReverseShowUIStatus();
                _isSwitchCD = false;
            }
            if (_isShowUIBefore)
            {
                _isShowUIBefore = false;
                _isSwitchCD = true;
                ReverseShowUIStatus(false);
                _isSwitchCD = false;
            }
        }
        else
        {
            // ����������ʽUI����ʱ���ر������ͣ����
            if (_isShowUI)
            {
                _isShowUIBefore = true;
                ReverseShowUIStatus(false);
            }
        }
    }

    /// <summary>
    /// ����������ͣ��ʾ��Э��
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowUI()
    {
        while (true)
        {
            // ��ȡ���λ�úͶ�Ӧ��cellPosition
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int targetCellPos = _groundTilemap.WorldToCell(mousePos);

            if (targetCellPos == _lastCellPosition) // ������λ�����ڵ�tile���ϴμ��ʱ��ͬ����ֱ��ʹչʾ�����ݸ������λ���ƶ��������ټ��������ж�
            {
                yield return new WaitForSeconds(0.05f); // �������ļ��Ϊ0.05s
                _mouseHoverPanel.GetComponent<RectTransform>().position = Input.mousePosition + _panelBias;
                continue;
            }

            // ������λ�����ڵ�tile���ϴμ��ʱ��ͬ���������ؾɵ�UI�������ٽ��и���
            _mouseHoverPanel.SetActive(false);
            _lastCellPosition = targetCellPos;

            // �����ͣ�ڿɼ���Χ�⣬��ֱ�ӷ���
            if (!GridMoveController.Instance.IsInBoundary(targetCellPos))
            {
                continue;
            }

            // ����ҽ�������tile�ϵ���ͣ��ʾUI�����Ǵ���ʱ�ӵģ��������ø�ʱ�ӽ���Ѱ·�ж�
            GridMoveController.Instance.JudgeReachable(targetCellPos);
            yield return new WaitForSeconds(_showDelay);

            bool isTileOccupied = false;
            string descriptionText = "";
            // �ж���ͣ������tile��
            if (_wallTilemap.HasTile(targetCellPos))    // WallTilemap������ǽ���ź��������͵��ϰ��
            {
                isTileOccupied = true;
                descriptionText = IsDoorTile(targetCellPos) ? "�ţ�����  <sprite=1>�Ҽ��򿪻�ر�\n" : "�˴��޷�����\n"; // TextMeshPro�ĸ��ı����ܣ����Բ���ͼƬ
            }
            else if (_furnitureTilemap.HasTile(targetCellPos))  // FurnitureTilemap
            {
                isTileOccupied = true;
                descriptionText = "�˴��޷�����\n";
            }
            else if (_groundTilemap.HasTile(targetCellPos)) // GroundTilemap�������͵�tile�ڵ�ͼ��Χ�ڵ�ÿһ�񶼴���
            {
                isTileOccupied = true;
                if (GridMoveController.Instance.IsMovable)  // ����ƶ�ʱ������ͣ��ʾ
                {
                    // ��ȡ֮ǰ����õ�Ѱ·���
                    bool isReachable = GridMoveController.Instance.IsReachable;
                    string requiredActionPoint = GridMoveController.Instance.RequiredActionPoint().ToString("0.0");
                    descriptionText = isReachable ? $"<sprite=0>����ƶ������� {requiredActionPoint}AP\n" : "�˴��޷�����\n";
                }
            }

            // �����ж��Ƿ���ͣ�ڲ�����
            if (_glassTilemap.HasTile(targetCellPos) && !IsGlassBroken(targetCellPos))  // GlassTilemap����������Ĳ���
            {
                descriptionText += "�������ƶ�������ײ��\n";
            }

            // �ж��Ƿ���ͣ����Դ����
            bool resourcePointExists = ResourcePointController.ResourcePointDictionary.ContainsKey(targetCellPos);
            if (resourcePointExists)
            {
                descriptionText += "��Դ�㣺����  <sprite=1>�Ҽ��鿴��Դ\n";
            }

            // �ж��Ƿ���ͣ����Ʒ�ϣ�ʹ�����߼�ⷽ��
            RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
            bool isFirst = true;
            if (hits.Length > 0)
            {
                foreach (var item in hits)
                {
                    if (item.collider.GetComponent<Item>() != null) // Ҫ����ײ�������Item��
                    {
                        if (isFirst)    // ����ʾ��Ʒ�б�ǰ��ʾ��Ʒ��ʰȡ��Ϣ
                        {
                            descriptionText += "����  <sprite=1>�Ҽ�ʰȡ��Ʒ\n";
                            isFirst = false;
                        }
                        descriptionText += $"��Ʒ��{item.collider.GetComponent<Item>().ItemData.ItemName}\n";
                    }
                }
            }

            // ������ͣ��ʾUI���ı����ݡ�λ�ã�������꣩���ɼ�״̬
            _panelText.text = descriptionText;
            if (isTileOccupied && descriptionText != "")
            {
                _mouseHoverPanel.GetComponent<RectTransform>().position = Input.mousePosition + _panelBias;
                _mouseHoverPanel.SetActive(true);
            }
        }
    }

    /// <summary>
    /// �ж�tile�Ƿ�Ϊ��
    /// </summary>
    /// <param name="position">��Ҫ�����жϵ�tilePosition</param>
    /// <returns>����true���ʾ�жϵ�tile����</returns>
    private bool IsDoorTile(Vector3Int position)
    {
        TileBase tile = _wallTilemap.GetTile(position);
        return tile == DoorTileHorizontal || tile == DoorTileVertical;
    }

    /// <summary>
    /// �ж�tile�Ƿ�Ϊ����Ĳ���
    /// </summary>
    /// <param name="position">��Ҫ�����жϵ�tilePosition</param>
    /// <returns>����true���ʾ�жϵ�tile������Ĳ���</returns>
    private bool IsGlassBroken(Vector3Int position)
    {
        TileBase tile = _glassTilemap.GetTile(position);
        return tile == BrokenGlassTile;
    }

    /// <summary>
    /// �л��Ƿ����������ͣ���ܵ�״̬
    /// </summary>
    private void ReverseShowUIStatus(bool isDisplayText=true)
    {
        string displayText;
        _isShowUI = !_isShowUI;
        if (_isShowUI)  // ���������ͣ����
        {
            _showCoroutine = StartCoroutine(ShowUI());
            displayText = "�����ͣ��ʾ�ѿ�";
        }
        else  // �ر������ͣ����
        {
            if (_showCoroutine != null)
            {
                StopCoroutine(_showCoroutine);
            }
            _mouseHoverPanel.SetActive(false);
            _lastCellPosition = Vector3Int.back;
            displayText = "�����ͣ��ʾ�ѹ�";
        }
        // ��ʾUI����ʾ��ҵ�ǰ�����ͣ�����Ƿ�����
        if(isDisplayText)
        {
            UIManager.Instance.DisplayHoverStatusPanel(displayText);
        }
    }

}
