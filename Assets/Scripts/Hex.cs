using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hex : MonoBehaviour
{
    public int CoordX { get; set; }
    public int CoordY { get; set; }
    public float CoordValueX { get; set; }
    public float CoordValueY { get; set; }
    public int ColorCode { get; set; }
    public bool HasBomb { get; set; }

    //for debugging
    public int x;
    public int y;
    public int colorCode;


    private void Start()
    {
        x = this.CoordX;
        y = this.CoordY;
    }

    private void Update()
    {
        x = this.CoordX;
        y = this.CoordY;
        colorCode = this.ColorCode;
    }
}
