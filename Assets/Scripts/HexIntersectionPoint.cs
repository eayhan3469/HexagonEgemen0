using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexIntersectionPoint : MonoBehaviour
{
    public float RotationZValue { get; set; }

    public bool HasSelected { get; set; }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var selector = GameObject.Find("HexFrame");
            selector.transform.GetChild(0).gameObject.SetActive(true);
            selector.transform.position = this.gameObject.transform.position;
            selector.transform.rotation = Quaternion.Euler(0, 0, RotationZValue);
            this.HasSelected = true;
        }
    }
}
