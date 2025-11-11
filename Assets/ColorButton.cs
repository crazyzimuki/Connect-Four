using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorButton : MonoBehaviour
{
    public Arrow game;
    public bool isPink;

    public void OnClick()
    {
        game.Pink = isPink;
    }
}
