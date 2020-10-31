using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameObject HexFrameObj;

    [SerializeField]
    private TextMeshProUGUI ScoreText;

    [SerializeField]
    private GameObject GameOverPanel;

    private Vector3 _hexFrameObjLastPos;
    private Transform _hexFrameObjInitialTransform;
    private bool _dragging;
    private Vector3 _firstMousePos;
    private Vector3 _lastMousePos;
    private Quaternion _targetRot;
    private int _roundMultiplier = 2;
    private bool _waitForCalculate;
    private float _selectedIntersectionPointRot;

    private void Start()
    {
        _hexFrameObjLastPos = HexFrameObj.transform.position;
        _hexFrameObjInitialTransform = HexFrameObj.transform;
        _targetRot = Quaternion.Euler(0f, 0f, 0f);
    }

    private void Update()
    {
        ScoreText.text = PlayerPrefs.GetInt("Score").ToString();

        if (PlayerPrefs.GetInt("BombCounter") == 5)
        {
            GameOverPanel.SetActive(true);
        }

        if (_hexFrameObjLastPos != HexFrameObj.transform.position)
        {
            var o = FindSelectedIntersectionPoint();
            _selectedIntersectionPointRot = o.GetComponent<HexIntersectionPoint>().RotationZValue;

            if (o != null)
            {
                SetInitialParent();
                var nearestHexes = FindIntersectionsNearestHexes(o);
                var hexesParent = GameBuilder.Instance.HexesParent.transform;

                foreach (var hex in nearestHexes)
                {
                    for (int i = 0; i < hexesParent.childCount; i++)
                    {
                        if (hexesParent.transform.GetChild(i).GetComponent<Hex>().CoordX == hex.GetComponent<Hex>().CoordX && hexesParent.transform.GetChild(i).GetComponent<Hex>().CoordY == hex.GetComponent<Hex>().CoordY)
                        {
                            hexesParent.transform.GetChild(i).parent = HexFrameObj.transform;
                        }
                    }
                }
            }

            _hexFrameObjLastPos = HexFrameObj.transform.position;
        }

        if (Vector3.Distance(_firstMousePos, _lastMousePos) > 10)
        {
            if (_selectedIntersectionPointRot == 0f)
            {
                RotateHexFrameType1();
            }
            else
            {
                RotateHexFrameType2();
            }
        }
    }

    private void SetInitialParent()
    {
        if (HexFrameObj.transform.childCount > 1)
        {
            var childCount = HexFrameObj.transform.childCount;
            for (int i = 1; i < childCount; i++)
            {
                var hex = HexFrameObj.transform.GetChild(1).GetComponent<Hex>();
                hex.transform.parent = GameBuilder.Instance.HexesParent.transform;
                hex.gameObject.transform.position = GameBuilder.Instance.HexPositions[new Vector2(hex.CoordX, hex.CoordY)];
            }
        }
    }

    private void ResetMove()
    {
        while (DestroyableHexes(GameBuilder.Instance.Hexes).Count > 0)
        {
            SetInitialParent();
            _roundMultiplier = 2;
            _firstMousePos = new Vector3(0, 0, 0);
            _lastMousePos = new Vector3(0, 0, 0);
            HexFrameObj.transform.GetChild(0).gameObject.SetActive(false);
            GameBuilder.Instance.DestroyMatchedHexes();
            GameBuilder.Instance.RebuildGridMap();
        }
    }

    private void RotateHexFrameType1()
    {
        Vector3 difference = _lastMousePos - HexFrameObj.transform.position;
        difference.Normalize();
        var atan2 = Mathf.Atan2(_firstMousePos.x, _firstMousePos.y);

        if (Mathf.Atan2(_lastMousePos.x, _lastMousePos.y) > atan2) //Clokwise Rotate
        {
            HexFrameObj.transform.rotation = Quaternion.RotateTowards(HexFrameObj.transform.rotation, Quaternion.Euler(0f, 0f, -60 * _roundMultiplier), 500 * Time.deltaTime);

            if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, -120f).eulerAngles && _roundMultiplier == 2 && !_waitForCalculate)
            {
                RotateColorsClockWise(1);
                StartCoroutine(RotateHexFrame());
            }
            else if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, -240f).eulerAngles && _roundMultiplier == 4 && !_waitForCalculate)
            {
                RotateColorsClockWise(1);
                StartCoroutine(RotateHexFrame());
            }
            else if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, -360f).eulerAngles && _roundMultiplier == 6 && !_waitForCalculate)
            {
                RotateColorsClockWise(1);
                StartCoroutine(RotateHexFrame());
            }
        }
        else //Count Clockwise
        {
            HexFrameObj.transform.rotation = Quaternion.RotateTowards(HexFrameObj.transform.rotation, Quaternion.Euler(0f, 0f, 60 * _roundMultiplier), 500 * Time.deltaTime);

            if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, 120f).eulerAngles && _roundMultiplier == 2 && !_waitForCalculate)
            {
                RotateColorsCountClockWise(1);
                StartCoroutine(RotateHexFrame());
            }
            else if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, 240f).eulerAngles && _roundMultiplier == 4 && !_waitForCalculate)
            {
                RotateColorsCountClockWise(1);
                StartCoroutine(RotateHexFrame());
            }
            else if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, 360f).eulerAngles && _roundMultiplier == 6 && !_waitForCalculate)
            {
                RotateColorsCountClockWise(1);
                StartCoroutine(RotateHexFrame());
            }
        }
    }

    private void RotateHexFrameType2()
    {
        Vector3 difference = _lastMousePos - HexFrameObj.transform.position;
        difference.Normalize();
        var atan2 = Mathf.Atan2(_firstMousePos.x, _firstMousePos.y);

        if (Mathf.Atan2(_lastMousePos.x, _lastMousePos.y) > atan2) //Clokwise Rotate
        {
            HexFrameObj.transform.rotation = Quaternion.RotateTowards(HexFrameObj.transform.rotation, Quaternion.Euler(0f, 0f, -180f + (-60 * _roundMultiplier)), 500 * Time.deltaTime);

            if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, 60f).eulerAngles && _roundMultiplier == 2 && !_waitForCalculate)
            {
                RotateColorsClockWise(2);
                StartCoroutine(RotateHexFrame());
            }
            else if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, -60f).eulerAngles && _roundMultiplier == 4 && !_waitForCalculate)
            {
                RotateColorsClockWise(2);
                StartCoroutine(RotateHexFrame());
            }
            else if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, 180f).eulerAngles && _roundMultiplier == 6 && !_waitForCalculate)
            {
                RotateColorsClockWise(2);
                StartCoroutine(RotateHexFrame());
            }
        }
        else //Count Clockwise
        {
            HexFrameObj.transform.rotation = Quaternion.RotateTowards(HexFrameObj.transform.rotation, Quaternion.Euler(0f, 0f, -180f + 60 * _roundMultiplier), 500 * Time.deltaTime);

            if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, -60f).eulerAngles && _roundMultiplier == 2 && !_waitForCalculate)
            {
                RotateColorsCountClockWise(2);
                StartCoroutine(RotateHexFrame());
            }
            else if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, 60f).eulerAngles && _roundMultiplier == 4 && !_waitForCalculate)
            {
                RotateColorsCountClockWise(2);
                StartCoroutine(RotateHexFrame());
            }
            else if (HexFrameObj.transform.rotation.eulerAngles == Quaternion.Euler(0f, 0f, -180f).eulerAngles && _roundMultiplier == 6 && !_waitForCalculate)
            {
                RotateColorsCountClockWise(2);
                StartCoroutine(RotateHexFrame());
            }
        }
    }

    private void RotateColorsCountClockWise(int type)
    {
        var hexes = GameBuilder.Instance.Hexes;
        var hex1 = new Hex();
        var hex2 = new Hex();
        var hex3 = new Hex();

        var selectedHexes = new List<Hex>();
        for (int i = 1; i < 4; i++)
            selectedHexes.Add(HexFrameObj.transform.GetChild(i).GetComponent<Hex>());

        if (type == 1)
        {
            hex1 = selectedHexes.OrderBy(s => s.CoordX).First();
            hex2 = selectedHexes.OrderByDescending(s => s.CoordX).OrderBy(k => k.CoordY).First();
            hex3 = selectedHexes.OrderByDescending(s => s.CoordX).OrderByDescending(k => k.CoordY).First();
        }
        else
        {
            hex1 = selectedHexes.OrderBy(s => s.CoordX).OrderBy(k => k.CoordY).First();
            hex2 = selectedHexes.OrderByDescending(s => s.CoordX).First();
            hex3 = selectedHexes.OrderByDescending(s => s.CoordY).OrderBy(k => k.CoordX).First();
        }

        var temp = hex1.ColorCode;
        hexes[new Vector2(hex1.CoordX, hex1.CoordY)].ColorCode = hex3.ColorCode;
        hexes[new Vector2(hex3.CoordX, hex3.CoordY)].ColorCode = hex2.ColorCode;
        hexes[new Vector2(hex2.CoordX, hex2.CoordY)].ColorCode = temp;
    }

    private void RotateColorsClockWise(int type)
    {
        var hexes = GameBuilder.Instance.Hexes;
        var hex1 = new Hex();
        var hex2 = new Hex();
        var hex3 = new Hex();

        var selectedHexes = new List<Hex>();
        for (int i = 1; i < 4; i++)
            selectedHexes.Add(HexFrameObj.transform.GetChild(i).GetComponent<Hex>());

        if (type == 1)
        {
            hex1 = selectedHexes.OrderBy(s => s.CoordX).First();
            hex2 = selectedHexes.OrderByDescending(s => s.CoordX).OrderBy(k => k.CoordY).First();
            hex3 = selectedHexes.OrderByDescending(s => s.CoordX).OrderByDescending(k => k.CoordY).First();
        }
        else
        {
            hex1 = selectedHexes.OrderBy(s => s.CoordX).OrderBy(k => k.CoordY).First();
            hex2 = selectedHexes.OrderByDescending(s => s.CoordX).First();
            hex3 = selectedHexes.OrderByDescending(s => s.CoordY).OrderBy(k => k.CoordX).First();
        }

        var temp = hex1.ColorCode;
        hexes[new Vector2(hex1.CoordX, hex1.CoordY)].ColorCode = hex2.ColorCode;
        hexes[new Vector2(hex2.CoordX, hex2.CoordY)].ColorCode = hex3.ColorCode;
        hexes[new Vector2(hex3.CoordX, hex3.CoordY)].ColorCode = temp;
    }

    IEnumerator RotateHexFrame()
    {
        _waitForCalculate = true;
        yield return new WaitForSeconds(0.1f);
        _roundMultiplier += 2;
        ResetMove();
        _waitForCalculate = false;

        if (_roundMultiplier == 8)
        {
            _roundMultiplier = 2;
            _firstMousePos = new Vector3(0, 0, 0);
            _lastMousePos = new Vector3(0, 0, 0);
        }
    }

    void OnMouseDown()
    {
        _firstMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _lastMousePos = _firstMousePos;
    }

    void OnMouseUp()
    {
        _lastMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public bool FindHex(Hex hex)
    {
        var hexesParent = GameObject.Find("Hexagons").transform;

        for (int i = 0; i < hexesParent.childCount; i++)
        {
            var hexObj = hexesParent.GetChild(i);

            if (hexObj.GetComponent<Hex>().CoordX == hex.CoordX && hexObj.GetComponent<Hex>().CoordY == hex.CoordY)
                return true;
        }

        return false;
    }

    public GameObject FindSelectedIntersectionPoint()
    {
        var intersectionParent = GameObject.Find("HexIntersections").transform;

        for (int i = 0; i < intersectionParent.childCount; i++)
        {
            if (HexFrameObj.transform.position == intersectionParent.transform.GetChild(i).position)
                return intersectionParent.transform.GetChild(i).gameObject;
        }

        return null;
    }

    public List<GameObject> FindIntersectionsNearestHexes(GameObject selectedIntersectionPoint)
    {
        var hexesParent = GameObject.Find("Hexagons").transform;
        var hexes = new List<GameObject>();

        for (int i = 0; i < hexesParent.childCount; i++)
        {
            var hexObj = hexesParent.transform.GetChild(i);
            var distance = Vector2.Distance(hexObj.position, selectedIntersectionPoint.transform.position);

            if (distance < 3f)
                hexes.Add(hexObj.gameObject);
        }

        return hexes;
    }

    public List<Hex> DestroyableHexes(Dictionary<Vector2, Hex> hexes)
    {
        var gridWidth = GameBuilder.Instance.GridWidth;
        var gridHeight = GameBuilder.Instance.GridHeight;
        List<Hex> destroyableHexes = new List<Hex>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (x != gridWidth - 1)
                {
                    if (x % 2 == 0)
                    {
                        if (y == 0)
                        {
                            var hex1 = hexes[new Vector2(x, y)];
                            var hex2 = hexes[new Vector2(x, y + 1)];
                            var hex3 = hexes[new Vector2(x + 1, y)];

                            if ((hex1 != null && hex2 != null && hex3 != null) && hex1.ColorCode == hex2.ColorCode && hex1.ColorCode == hex3.ColorCode)
                            {
                                var colorCode = hex1.ColorCode;
                                destroyableHexes.Add(hex1);
                                destroyableHexes.Add(hex2);
                                destroyableHexes.Add(hex3);
                                if (hexes[new Vector2(x + 1, y + 1)].ColorCode == colorCode)
                                {

                                }
                            }
                        }
                        else if (y != gridHeight - 1)
                        {
                            var hex1 = hexes[new Vector2(x, y)];
                            var hex2 = hexes[new Vector2(x + 1, y - 1)];
                            var hex3 = hexes[new Vector2(x + 1, y)];

                            if ((hex1 != null && hex2 != null && hex3 != null) && hex1.ColorCode == hex2.ColorCode && hex1.ColorCode == hex3.ColorCode)
                            {
                                destroyableHexes.Add(hex1);
                                destroyableHexes.Add(hex2);
                                destroyableHexes.Add(hex3);
                            }

                            var hex1a = hexes[new Vector2(x, y)];
                            var hex2a = hexes[new Vector2(x, y + 1)];
                            var hex3a = hexes[new Vector2(x + 1, y)];

                            if ((hex1a != null && hex2a != null && hex3a != null) && hex1a.ColorCode == hex2a.ColorCode && hex1a.ColorCode == hex3a.ColorCode)
                            {
                                destroyableHexes.Add(hex1a);
                                destroyableHexes.Add(hex2a);
                                destroyableHexes.Add(hex3a);
                            }
                        }
                        else
                        {
                            var hex1 = hexes[new Vector2(x, y)];
                            var hex2 = hexes[new Vector2(x + 1, y - 1)];
                            var hex3 = hexes[new Vector2(x + 1, y)];

                            if ((hex1 != null && hex2 != null && hex3 != null) && hex1.ColorCode == hex2.ColorCode && hex1.ColorCode == hex3.ColorCode)
                            {
                                destroyableHexes.Add(hex1);
                                destroyableHexes.Add(hex2);
                                destroyableHexes.Add(hex3);
                            }
                        }
                    }
                    else
                    {
                        if (y != gridHeight - 1)
                        {
                            var hex1 = hexes[new Vector2(x, y)];
                            var hex2 = hexes[new Vector2(x + 1, y)];
                            var hex3 = hexes[new Vector2(x + 1, y + 1)];

                            if ((hex1 != null && hex2 != null && hex3 != null) && hex1.ColorCode == hex2.ColorCode && hex1.ColorCode == hex3.ColorCode)
                            {
                                destroyableHexes.Add(hex1);
                                destroyableHexes.Add(hex2);
                                destroyableHexes.Add(hex3);
                            }

                            var hex1a = hexes[new Vector2(x, y)];
                            var hex2a = hexes[new Vector2(x, y + 1)];
                            var hex3a = hexes[new Vector2(x + 1, y + 1)];

                            if ((hex1a != null && hex2a != null && hex3a != null) && hex1a.ColorCode == hex2a.ColorCode && hex1a.ColorCode == hex3a.ColorCode)
                            {
                                destroyableHexes.Add(hex1a);
                                destroyableHexes.Add(hex2a);
                                destroyableHexes.Add(hex3a);
                            }
                        }
                    }
                }
            }
        }

        return destroyableHexes.ToList();
    }

    public void OnPlayAgainClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("PlayScene");
    }
}
