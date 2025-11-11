using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Play : MonoBehaviour
{
    public GameObject menu;
    public Arrow game;

    public void OnClick()
    {
        menu.SetActive(false);
        game.canPlay = true;
        return;
    }
}
