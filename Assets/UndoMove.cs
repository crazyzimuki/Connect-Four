using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoMove : MonoBehaviour
{
    public Arrow game;

    public void OnClick()
    {
        if (!game.ended)
        {
            game.UndoMove(game.board, game.moves); // Undo computer's move
            if (game.moves.Count > 0) game.UndoMove(game.board, game.moves); // Undo player's move, if exists
        }
    }
}
