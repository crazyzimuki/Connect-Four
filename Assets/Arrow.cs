using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class Arrow : MonoBehaviour
{
    public char[,] board = new char[6, 7]{{ ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                                          { ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                                          { ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                                          { ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                                          { ' ', ' ', ' ', ' ', ' ', ' ', ' '},
                                          { ' ', ' ', ' ', ' ', ' ', ' ', ' '} };
    public LayerMask uiLayerMask;
    public bool canMakeMove;
    public bool Pink;
    public bool canPlay;
    public int Difficulty;
    public bool ended = false;
    static float minX = -4.3f;
    static float maxX = 4.27f;
    static float stepX;
    static Vector3 mouseWorldPosition; // Actual position of the mouse
    static Vector3 checkerWorldPosition; // Corrected position to spawn the checker in
    public GameObject checker;
    public Sprite yellow, pink;
    public TMP_Text UIText;
    public GameObject UIButton;
    public List<GameObject> Checkers;
    public List<int> moves;

    void Start()
    {
        canPlay = false;
        Difficulty = 2;
        UIButton.SetActive(false);
        stepX = (maxX - minX)/6;
        moves = new List<int>();
        Checkers = new List<GameObject>();
    }

    void Update()
    {
        int turn;
        if (Pink) turn = (moves.Count+2) % 2; // The +2 prevents the modulo-0 anomally
        else turn = ((moves.Count+2) % 2) + 1;

        if (!ended && (turn % 2 == 0) && canMakeMove && !IsPointerOverUI()) // Player move
        {
            // Handle player input if not over UI
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    PlayerMove(); // Make the player move
                }
            }
            else if (Input.GetMouseButtonDown(0)) // For mouse input
            {
                PlayerMove(); // Make the player move
            }
        }

        else if (!ended && canMakeMove) // Computer move
        {
            if (canPlay && (turn % 2) == 1)
            {
                canMakeMove = false; // Prevent further moves until this flag is reset
                (int, int) move = alphabeta(board, int.MinValue, int.MaxValue, Difficulty, false);
                Debug.Log(move.Item1);
                moves.Add(move.Item2); // Save move
                ModifyBoard(board, move.Item2, 'O');
                ConvertToCheckerPos();
                SpawnChecker(checker, yellow, pink, checkerWorldPosition);
                StartCoroutine(Cooldown()); // Reset the flag after a short delay
            }
        }

        if (TerminalState(board, 0) != null) // If game ended
        {
            int state = (int)TerminalState(board, 0);
            if (!Pink) { state *= -1; --state; } // Flip value based on player color
            if (state == (int.MaxValue - Difficulty)) { UIText.text = "Pink Won"; GameEnded(); }
            else if (state == (int.MinValue + Difficulty)) { UIText.text = "Yellow Won"; GameEnded();}
            else if (state == 0) { UIText.text = "Draw"; ; GameEnded(); }
            else { UIText.text = "Error"; ; GameEnded(); }
        }
    }

    public void PlayerMove()
    {
        if (canPlay)
        {
            canMakeMove = false;
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Get mouse position in the scene
            int move = ConvertInputToMove(mouseWorldPosition); // Save move

            if (IsLegal(board, move))
            {
                moves.Add(move);
                Debug.Log(move.ToString());
                ModifyBoard(board, move, 'X');
                ConvertToCheckerPos();
                SpawnChecker(checker, pink, yellow, checkerWorldPosition);
            }
            StartCoroutine(Cooldown()); // Reset the flag after a short delay
        }
        return;
    }

    // Helper function to check if the pointer is over a UI object
    public bool IsPointerOverUI()
    {
        // Check if the pointer is currently over a UI element
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        // Cast a ray from the camera to check if it hits a UI element
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, uiLayerMask))
        {
            // If it hits something in the UI layer, it's considered a UI interaction
            return true;
        }

        return false;
    }

    void GameEnded()
    {
        EnableRetry();
        ended = true;
    }

    void EnableRetry()
    {
        UIButton.SetActive(true);
    }

    public void UndoMove(char[,] board, List<int> moves)
    {
        for (int k = 0; k <= 5; k++)
        {
            if (board[k, moves[moves.Count-1]] != ' ')
            {
                board[k, moves[moves.Count-1]] = ' '; break;
            }
        }

        moves.RemoveAt(moves.Count - 1); // Delete last move from list
        Destroy(Checkers[Checkers.Count - 1]); // Destroy last checker placed
        Checkers.RemoveAt(Checkers.Count - 1); // Remove destroyed checker from list
        return;
    }

    public void UndoAIMove(char[,] board, List<int> moves)
    {
        for (int k = 0; k <= 5; k++)
        {
            if (board[k, moves[moves.Count - 1]] != ' ')
            {
                board[k, moves[moves.Count - 1]] = ' '; break;
            }
        }

        moves.RemoveAt(moves.Count - 1); // Delete last move from list
        return;
    }

    void ConvertToCheckerPos()
    {
        checkerWorldPosition = new Vector3(minX + stepX * moves[moves.Count - 1], 4f, 0f);
        return;
    }

    int ConvertInputToMove(Vector3 mouse)
    {
        float smallest = float.MaxValue;
        int ind = 0;

        // Round X value to closest valid position
        for (int i = 6; i>=0; i--)
        {
            float distance = Mathf.Abs(mouse.x - (minX + stepX * i));
            if (distance < smallest)
            {
                smallest = distance;
                ind = i;
            }
        }
        return ind;
    }

    public void ModifyBoard(char[,] board, int move, char XO)
    {
        for (int k = 5; k >= 0; k--)
        {
            if (board[k, move] == ' ')
            {
                board[k, move] = XO;
                return;
            }
        }
        return;
    }

    void SpawnChecker(GameObject checker, Sprite pink, Sprite yellow, Vector3 pos)
    {
         GameObject newChecker = Instantiate(checker, pos - new Vector3(0f, 0.5f, 0f), Quaternion.identity) as GameObject;
         if (Pink) { newChecker.GetComponent<SpriteRenderer>().sprite = pink; }
         else { newChecker.GetComponent<SpriteRenderer>().sprite = yellow; }
         Checkers.Add(newChecker);
    }

    static bool IsLegal(char[,] board, int move)
    {
        if (move >= 0 && move < 7)
            if (board[0, move] == ' ')
                return true;

        return false;
    }

    public int? TerminalState(char[,] board, int depth)
    {
        bool emptyspotfound = false;

        for (int k = 0; k < 6; k++)
        {
            for (int i = 0; i < 7; i++)
            {
                if (i < 4) // Horizontal
                {
                    if (board[k, i] == 'X' && board[k, i + 1] == 'X' && board[k, i + 2] == 'X' && board[k, i + 3] == 'X')
                    { return ((int.MaxValue - (Difficulty - depth))); }

                    if (board[k, i] == 'O' && board[k, i + 1] == 'O' && board[k, i + 2] == 'O' && board[k, i + 3] == 'O')
                    { return ((int.MinValue + (Difficulty - depth))); }
                }

                if (k < 3) // Vertical
                {
                    if (board[k, i] == 'X' && board[k + 1, i] == 'X' && board[k + 2, i] == 'X' && board[k + 3, i] == 'X')
                    { return ((int.MaxValue - (Difficulty+1 - depth))); }

                    if (board[k, i] == 'O' && board[k + 1, i] == 'O' && board[k + 2, i] == 'O' && board[k + 3, i] == 'O')
                    { return ((int.MinValue + (Difficulty - depth))); }
                }

                if (i < 4 && k < 3) // Down-Right diagonals
                {
                    if (board[k, i] == 'X' && board[k + 1, i + 1] == 'X' && board[k + 2, i + 2] == 'X' && board[k + 3, i + 3] == 'X')
                    { return ((int.MaxValue - (Difficulty - depth))); }

                    if (board[k, i] == 'O' && board[k + 1, i + 1] == 'O' && board[k + 2, i + 2] == 'O' && board[k + 3, i + 3] == 'O')
                    { return ((int.MinValue + (Difficulty - depth))); }
                }

                if (i > 2 && k < 3) // Down-Left diagonals
                {
                    if (board[k, i] == 'X' && board[k + 1, i - 1] == 'X' && board[k + 2, i - 2] == 'X' && board[k + 3, i - 3] == 'X')
                    { return ((int.MaxValue - (Difficulty - depth))); }

                    if (board[k, i] == 'O' && board[k + 1, i - 1] == 'O' && board[k + 2, i - 2] == 'O' && board[k + 3, i - 3] == 'O')
                    { return ((int.MinValue + (Difficulty - depth))); }
                }

                if (board[k, i] == ' ') emptyspotfound = true;
            }
        }

        if (!emptyspotfound) return 0;
        return null;
    }

    public (int score, int move) alphabeta(char[,] board, int alpha, int beta, int depth, bool maximize)
    {
        if (depth == 0)
        {
            return (NodeEvaluation(board, 0, maximize), 0);
        }

        if (TerminalState(board, depth) != null) return ((int)TerminalState(board, depth), 0);

        List<int> allmoves = new List<int>(LegalMoves());
        allmoves = MoveOrdering(allmoves);
        int value = 0, currentvalue = 0, bestmove = 0;

        if (maximize) // Maximizing player
        {
            value = int.MinValue;

            foreach (int i in allmoves)
            {
                // Create a copy of the board
                //char[,] boardCopy = (char[,])board.Clone();

                ModifyBoard(board, i, 'X'); // Make the move
                moves.Add(i);
                currentvalue = alphabeta(board, alpha, beta, depth - 1, false).score; // Evaluate the move
                UndoAIMove(board, moves);
                if (currentvalue > value)
                {
                    value = currentvalue;
                    bestmove = i;
                }
                if (alpha < value)
                {
                    alpha = value;
                }
                if (alpha >= beta)
                    break;
            }
            return (value, bestmove);
        }
        else // Minimizing player
        {
            value = int.MaxValue;

            foreach (int i in allmoves)
            {
                ModifyBoard(board, i, 'O'); // Make the move
                moves.Add(i);
                currentvalue = alphabeta(board, alpha, beta, depth - 1, true).score; // Evaluate the move
                UndoAIMove(board, moves);

                if (currentvalue < value)
                {
                    value = currentvalue;
                    bestmove = i;
                }
                if (beta > value)
                {
                    beta = value;
                }
                if (alpha >= beta)
                    break;
            }
            return (value, bestmove);
        }
    }

    public List<int> LegalMoves()
    {
        List<int> allmoves = new List<int>();

        for (int i = 0; i < 7; i++)
        {
            if (board[0, i] == ' ') allmoves.Add(i);
        }
        return allmoves;
    }

    public List<int> MoveOrdering(List<int> moves) // Sorts move list based on distance from center
    {
        List<int> SortedList = new List<int>();
        SortedList = moves.OrderBy(n => Mathf.Abs(n - 3)).ToList();
        return SortedList;
    }

    public int NodeEvaluation(char[,] board, int depth, bool maximize)
    {
        if (TerminalState(board, depth) != null) return (int)TerminalState(board, depth);

        int EvaluationX = 0, EvaluationO = 0;
        for (int k = 0; k < 6; k++)
            for (int i = 0; i < 7; i++)
            {
                //Center checkers are worth more

                if (board[k, i] == 'X')
                {
                    switch (i)
                    {
                        case 0: case 6: EvaluationX += 1; break;
                        case 1: case 5: EvaluationX += 2; break;
                        case 2: case 4: EvaluationX += 3; break;
                        case 3: EvaluationX += 5; break;
                    }
                }

                if (board[k, i] == 'O')
                {
                    switch (i)
                    {
                        case 0: case 6: EvaluationO += 1; break;
                        case 1: case 5: EvaluationO += 2; break;
                        case 2: case 4: EvaluationO += 3; break;
                        case 3: EvaluationO += 5; break;
                    }
                }

                //Open 3-streaks are worth the most

                if (i < 4) // Horizontal
                {
                    if (board[k, i] == 'X' && board[k, i + 1] == 'X' && board[k, i + 2] == 'X' && board[k, i + 3] == ' '
                        || board[k, i] == 'X' && board[k, i + 1] == 'X' && board[k, i + 2] == ' ' && board[k, i + 3] == 'X'
                        || board[k, i] == 'X' && board[k, i + 1] == ' ' && board[k, i + 2] == 'X' && board[k, i + 3] == 'X'
                        || board[k, i] == ' ' && board[k, i + 1] == 'X' && board[k, i + 2] == 'X' && board[k, i + 3] == 'X')
                    { EvaluationX += 20; }

                    if (board[k, i] == 'O' && board[k, i + 1] == 'O' && board[k, i + 2] == 'O' && board[k, i + 3] == ' '
                        || board[k, i] == 'O' && board[k, i + 1] == 'O' && board[k, i + 2] == ' ' && board[k, i + 3] == 'O'
                        || board[k, i] == 'O' && board[k, i + 1] == ' ' && board[k, i + 2] == 'O' && board[k, i + 3] == 'O'
                        || board[k, i] == ' ' && board[k, i + 1] == 'O' && board[k, i + 2] == 'O' && board[k, i + 3] == 'O')
                    { EvaluationO += 20; }
                }

                if (k < 3) // Vertical
                {
                    if (board[k, i] == ' ' && board[k + 1, i] == 'X' && board[k + 2, i] == 'X' && board[k + 3, i] == 'X')
                    { EvaluationX += 20; }

                    if (board[k, i] == ' ' && board[k + 1, i] == 'O' && board[k + 2, i] == 'O' && board[k + 3, i] == 'O')
                    { EvaluationO += 20; }
                }

                if (i < 4 && k < 3) // Down-Right Diagonals
                {
                    if (board[k, i] == 'X' && board[k + 1, i + 1] == 'X' && board[k + 2, i + 2] == 'X' && board[k + 3, i + 3] == ' '
                        || board[k, i] == 'X' && board[k + 1, i + 1] == 'X' && board[k + 2, i + 2] == ' ' && board[k + 3, i + 3] == 'X'
                        || board[k, i] == 'X' && board[k + 1, i + 1] == ' ' && board[k + 2, i + 2] == 'X' && board[k + 3, i + 3] == 'X'
                        || board[k, i] == ' ' && board[k + 1, i + 1] == 'X' && board[k + 2, i + 2] == 'X' && board[k + 3, i + 3] == 'X')
                    { EvaluationX += 20; }

                    if (board[k, i] == 'O' && board[k + 1, i + 1] == 'O' && board[k + 2, i + 2] == 'O' && board[k + 3, i + 3] == ' '
                        || board[k, i] == 'O' && board[k + 1, i + 1] == 'O' && board[k + 2, i + 2] == ' ' && board[k + 3, i + 3] == 'O'
                        || board[k, i] == 'O' && board[k + 1, i + 1] == ' ' && board[k + 2, i + 2] == 'O' && board[k + 3, i + 3] == 'O'
                        || board[k, i] == ' ' && board[k + 1, i + 1] == 'O' && board[k + 2, i + 2] == 'O' && board[k + 3, i + 3] == 'O')
                    { EvaluationO += 20; }
                }

                if (i > 2 && k < 3) // Down-Left Diagonals
                {
                    if (board[k, i] == 'X' && board[k + 1, i - 1] == 'X' && board[k + 2, i - 2] == 'X' && board[k + 3, i - 3] == ' '
                        || board[k, i] == 'X' && board[k + 1, i - 1] == 'X' && board[k + 2, i - 2] == ' ' && board[k + 3, i - 3] == 'X'
                        || board[k, i] == 'X' && board[k + 1, i - 1] == ' ' && board[k + 2, i - 2] == 'X' && board[k + 3, i - 3] == 'X'
                        || board[k, i] == ' ' && board[k + 1, i - 1] == 'X' && board[k + 2, i - 2] == 'X' && board[k + 3, i - 3] == 'X')
                    { EvaluationX += 20; }

                    if (board[k, i] == 'O' && board[k + 1, i - 1] == 'O' && board[k + 2, i - 2] == 'O' && board[k + 3, i - 3] == ' '
                        || board[k, i] == 'O' && board[k + 1, i - 1] == 'O' && board[k + 2, i - 2] == ' ' && board[k + 3, i - 3] == 'O'
                        || board[k, i] == 'O' && board[k + 1, i - 1] == ' ' && board[k + 2, i - 2] == 'O' && board[k + 3, i - 3] == 'O'
                        || board[k, i] == ' ' && board[k + 1, i - 1] == 'O' && board[k + 2, i - 2] == 'O' && board[k + 3, i - 3] == 'O')
                    { EvaluationO += 20; }
                }
            }
            return (EvaluationX - EvaluationO + (Mathf.RoundToInt(Random.Range(Difficulty-9, 9-Difficulty))*10));
    }

    // Coroutine to reset the flag after a delay
    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(.5f); // Adjust delay as needed
        canMakeMove = true;
    }
}