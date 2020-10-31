using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBuilder : MonoBehaviour
{
    [SerializeField]
    private GameObject _hexObject;

    [SerializeField]
    public GameObject HexesParent;

    [SerializeField]
    public int GridHeight;

    [SerializeField]
    public int GridWidth;

    [SerializeField]
    private GameObject _hexIntersectionPoint;

    [SerializeField]
    private GameObject _intersectionPointsParent;

    [SerializeField]
    private List<Color> _colors;

    public static GameBuilder Instance { get; private set; }
    public Dictionary<Vector2, Vector2> HexPositions;
    public Dictionary<Vector2, Hex> Hexes;
    public int BombScore;

    private GameController _gameContoller;
    private float _hexOffsetX = 3.5f;
    private float _hexOffsetY = 4f;
    private bool _hasBuild;
    private bool _setted;
    private bool _bombSetted; 

    private void Awake()
    {
        PlayerPrefs.SetInt("Score", 0);
        PlayerPrefs.SetInt("HasBomb", 0);
        PlayerPrefs.SetInt("BombCounter", 0);
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        _gameContoller = GameObject.FindObjectOfType<GameController>();
        Hexes = new Dictionary<Vector2, Hex>();
        HexPositions = new Dictionary<Vector2, Vector2>();
        BuildGridMap();
        HexIntersectionBuilder();
    }

    private void Start()
    {
        while (_gameContoller.DestroyableHexes(Hexes).Count > 0)
        {
            DestroyMatchedHexes();
            RebuildGridMap();
        }

        _setted = true;
    }

    private void Update()
    {
        for (int i = 0; i < HexesParent.transform.childCount; i++)
        {
            var hex = HexesParent.transform.GetChild(i).GetComponent<Hex>();
            Hexes[new Vector2(hex.CoordX, hex.CoordY)] = hex;
            Hexes[new Vector2(hex.CoordX, hex.CoordY)].GetComponent<SpriteRenderer>().color = _colors[hex.ColorCode];
            Hexes[new Vector2(hex.CoordX, hex.CoordY)].transform.GetChild(0).gameObject.SetActive(hex.HasBomb);
        }
    }

    public void DestroyMatchedHexes()
    {
        if (PlayerPrefs.GetInt("HasBomb") == 1)
        {
            var bombCounter = PlayerPrefs.GetInt("BombCounter");
            bombCounter++;
            PlayerPrefs.SetInt("BombCounter", bombCounter);
            Debug.Log(bombCounter);
        }

        var destroyableHexes = _gameContoller.DestroyableHexes(Hexes);

        foreach (var hex in destroyableHexes)
        {
            for (int i = 0; i < HexesParent.transform.childCount; i++)
            {
                var hex0 = HexesParent.transform.GetChild(i).GetComponent<Hex>();

                if (hex0 == hex)
                {
                    if (hex0.HasBomb)
                    {
                        PlayerPrefs.SetInt("BombCounter", 0);
                        PlayerPrefs.SetInt("HasBomb", 0);
                    }

                    Hexes[new Vector2(hex.CoordX, hex.CoordY)] = null;
                    Destroy(hex0.gameObject);

                    if (_setted)
                    {
                        var score = PlayerPrefs.GetInt("Score");
                        score += 5;
                        PlayerPrefs.SetInt("Score", score);
                    }
                }
            }
        }

        _hasBuild = false;
    }


    public void RebuildGridMap()
    {
        _hasBuild = true;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (y != GridHeight - 1)
                {
                    var hexCoord = new Vector2(x, y);

                    if (Hexes[hexCoord] == null)
                    {
                        for (int y0 = y + 1; y0 < GridHeight; y0++)
                        {
                            if (Hexes[new Vector2(x, y0)] != null)
                            {
                                Hexes[hexCoord] = Hexes[new Vector2(x, y0)];
                                Hexes[hexCoord].CoordY = y;
                                Hexes[hexCoord].HasBomb = Hexes[new Vector2(x, y0)].HasBomb;
                                Hexes[hexCoord].colorCode = Hexes[new Vector2(x, y0)].colorCode;
                                Hexes[hexCoord].GetComponent<SpriteRenderer>().color = _colors[Hexes[hexCoord].ColorCode];
                                Hexes[hexCoord].y = y;
                                Hexes[new Vector2(x, y0)] = null;
                                y--;
                                break;
                            }
                        }
                    }
                }
            }
        }

        foreach (var hex in Hexes)
        {
            if (hex.Value != null)
            {
                hex.Value.CoordValueX = HexPositions[new Vector2(hex.Value.CoordX, hex.Value.CoordY)].x;
                hex.Value.CoordValueY = HexPositions[new Vector2(hex.Value.CoordX, hex.Value.CoordY)].y;

                for (int i = 0; i < HexesParent.transform.childCount; i++)
                {
                    var hex0 = HexesParent.transform.GetChild(i).GetComponent<Hex>();

                    if (hex0.CoordX == hex.Value.CoordX && hex0.CoordY == hex.Value.CoordY)
                        hex0.transform.position = HexPositions[new Vector2(hex.Value.CoordX, hex.Value.CoordY)];
                }
            }
        }

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (Hexes[new Vector2(x, y)] == null)
                {
                    var hexPos = HexPositions[new Vector2(x, y)];
                    var hexObj = Instantiate(_hexObject, hexPos, Quaternion.identity, HexesParent.transform);
                    var hex = hexObj.GetComponent<Hex>();
                    Hexes[new Vector2(x, y)] = hex;
                    Hexes[new Vector2(x, y)].CoordX = x;
                    Hexes[new Vector2(x, y)].CoordY = y;
                    Hexes[new Vector2(x, y)].CoordValueX = hexPos.x;
                    Hexes[new Vector2(x, y)].CoordValueX = hexPos.y;
                    Hexes[new Vector2(x, y)].ColorCode = Random.Range(0, _colors.Count());

                    if (PlayerPrefs.GetInt("HasBomb") == 0 && PlayerPrefs.GetInt("Score") >= BombScore)
                        Hexes[new Vector2(x, y)].HasBomb = true;
                    else
                        Hexes[new Vector2(x, y)].HasBomb = false;

                    if (Hexes[new Vector2(x, y)].HasBomb)
                    {
                        Hexes[new Vector2(x, y)].gameObject.transform.GetChild(0).gameObject.SetActive(true);
                        PlayerPrefs.SetInt("HasBomb", 1);
                    }

                    Hexes[new Vector2(x, y)].GetComponent<SpriteRenderer>().color = _colors[Hexes[new Vector2(x, y)].ColorCode];
                }
            }
        }
        //Debug.Log(str);
    }

    private void BuildGridMap()
    {
        float initialX = GridWidth % 2 == 0 ? (GridWidth / 2 * -_hexOffsetX) + _hexOffsetX / 2f : (Mathf.Floor(GridWidth / 2f)) * -_hexOffsetX;
        float initialY = (Mathf.Floor(GridHeight / 2f)) * -_hexOffsetY;
        float xPos = initialX;
        float yPos = initialY;

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                var hexObj = Instantiate(_hexObject, new Vector2(xPos, yPos), Quaternion.identity, HexesParent.transform);
                InitHex(hexObj.GetComponent<Hex>(), x, y, xPos, yPos, Random.Range(0, _colors.Count));
                HexPositions.Add(new Vector2(x, y), new Vector2(xPos, yPos));
                Hexes.Add(new Vector2(x, y), hexObj.GetComponent<Hex>());

                yPos += _hexOffsetY;
            }

            if ((x + 1) % 2 == 0)
                yPos = initialY;
            else
                yPos = initialY + 2;

            xPos += _hexOffsetX;
        }
    }

    private void HexIntersectionBuilder()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                if (x != GridWidth - 1)
                {
                    if (x % 2 == 0)
                    {
                        if (y == 0)
                        {
                            var origin = FindIntersectionCoordinate(Hexes[new Vector2(x, y)], Hexes[new Vector2(x, y + 1)], Hexes[new Vector2(x + 1, y)]);
                            var hexIntersectionPoint = Instantiate(_hexIntersectionPoint, origin, Quaternion.identity, _intersectionPointsParent.transform).GetComponent<HexIntersectionPoint>();
                            InitHexIntersectionPoint(hexIntersectionPoint, 180f);
                        }
                        else if (y != GridHeight - 1)
                        {
                            var origin = FindIntersectionCoordinate(Hexes[new Vector2(x, y)], Hexes[new Vector2(x + 1, y - 1)], Hexes[new Vector2(x + 1, y)]);
                            var hexIntersectionPoint = Instantiate(_hexIntersectionPoint, origin, Quaternion.identity, _intersectionPointsParent.transform).GetComponent<HexIntersectionPoint>();
                            InitHexIntersectionPoint(hexIntersectionPoint, 0f);

                            var origin0 = FindIntersectionCoordinate(Hexes[new Vector2(x, y)], Hexes[new Vector2(x, y + 1)], Hexes[new Vector2(x + 1, y)]);
                            var hexIntersectionPoint0 = Instantiate(_hexIntersectionPoint, origin0, Quaternion.identity, _intersectionPointsParent.transform).GetComponent<HexIntersectionPoint>();
                            InitHexIntersectionPoint(hexIntersectionPoint0, 180f);
                        }
                        else
                        {
                            var origin = FindIntersectionCoordinate(Hexes[new Vector2(x, y)], Hexes[new Vector2(x + 1, y - 1)], Hexes[new Vector2(x + 1, y)]);
                            var hexIntersectionPoint = Instantiate(_hexIntersectionPoint, origin, Quaternion.identity, _intersectionPointsParent.transform).GetComponent<HexIntersectionPoint>();
                            InitHexIntersectionPoint(hexIntersectionPoint, 0f);
                        }
                    }
                    else
                    {
                        if (y != GridHeight - 1)
                        {
                            var origin = FindIntersectionCoordinate(Hexes[new Vector2(x, y)], Hexes[new Vector2(x + 1, y)], Hexes[new Vector2(x + 1, y + 1)]);
                            var hexIntersectionPoint = Instantiate(_hexIntersectionPoint, origin, Quaternion.identity, _intersectionPointsParent.transform).GetComponent<HexIntersectionPoint>();
                            InitHexIntersectionPoint(hexIntersectionPoint, 0f);

                            var origin0 = FindIntersectionCoordinate(Hexes[new Vector2(x, y)], Hexes[new Vector2(x, y + 1)], Hexes[new Vector2(x + 1, y + 1)]);
                            var hexIntersectionPoint0 = Instantiate(_hexIntersectionPoint, origin0, Quaternion.identity, _intersectionPointsParent.transform).GetComponent<HexIntersectionPoint>();
                            InitHexIntersectionPoint(hexIntersectionPoint0, 180f);
                        }
                    }
                }
            }
        }
    }

    private Vector2 FindIntersectionCoordinate(Hex hex1, Hex hex2, Hex hex3)
    {
        var totalX = hex1.CoordValueX + hex2.CoordValueX + hex3.CoordValueX;
        var totalY = hex1.CoordValueY + hex2.CoordValueY + hex3.CoordValueY;

        return new Vector2(totalX / 3, totalY / 3);
    }

    private void InitHex(Hex hex, int coordX, int coordY, float coordValueX, float coordValueY, int colorCode)
    {
        hex.CoordX = coordX;
        hex.CoordY = coordY;
        hex.CoordValueX = coordValueX;
        hex.CoordValueY = coordValueY;
        hex.ColorCode = colorCode;
        hex.GetComponent<SpriteRenderer>().color = _colors[colorCode];

    }

    private void InitHexIntersectionPoint(HexIntersectionPoint hexIntersection, float rotationZValue)
    {
        hexIntersection.RotationZValue = rotationZValue;
    }
}