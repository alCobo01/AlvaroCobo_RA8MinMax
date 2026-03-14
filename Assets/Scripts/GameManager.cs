using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject token1, token2;
    
    private float _distance = 1.1f;
    private int _length = 3;
    private int[,] _gameMatrix; //0 not chosen, 1 player, 2 enemy
    private Vector2[,] _gameMatrixPos;
    private Vector3 _mousePos;
    private void Awake()
    {
        _gameMatrix = new int[_length, _length];
        _gameMatrixPos = new Vector2[_length, _length];

        for (int i = 0; i < _length; i++) //fila
            for (int j = 0; j < _length; j++) //columna
                _gameMatrix[i, j] = 0;

        float iniX = -1f;
        float iniY = -1f;
        for (int i = 0; i < _length; i++) //fila
        {
            for (int j = 0; j < _length; j++) //columna
            {
                _gameMatrixPos[i, j] = new Vector2(iniX * 2f * _distance, -iniY * 2f * _distance);
                iniY++;
            }
            iniY = -1f;
            iniX++;
        }


    }

    private void Update()
    {
        _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos.z = 0f;

        if (Input.GetMouseButtonUp(0))
        {
            Collider2D hit = Physics2D.OverlapPoint(_mousePos);
            if (hit != null)
            {
                var pos = CalculatePosition();
                if (pos.Item2)
                {
                    Instantiate(token1, pos.Item1, Quaternion.identity);
                    // GameMatrix
                    if (EvaluateWin() != 0) return;
                    EnemyResponse();
                    EvaluateWin();
                }
            }
        }
        
    }
    /// <summary>
    /// Torna la posici� de la fitxa que tries en funci� de la posici� del click
    /// </summary>
    /// <returns>
    /// Una tupla que contiene:
    /// - Un booleano que indica si la operaci�n fue exitosa.
    /// - Un Vector3 que representa una posici�n en el espacio 3D.
    /// </returns>
    private (Vector3, bool) CalculatePosition() //3 x 3, es podria fer recursiu per� no em dona la gana
    {
        Vector3 position = Vector3.zero;
        int[] booleanPos = new int[2];
        bool dev = false;
        int x = 0, y = 0;
        booleanPos[1] = 1; //columna
        if (_mousePos.x > _distance) //X
        {
            position.x = 2 * _distance;
            booleanPos[1] = 2;
        }
        else if (_mousePos.x < -_distance)
        {
            position.x = -2 * _distance;
            booleanPos[1] = 0;
        }

        booleanPos[0] = 1; //fila
        if (_mousePos.y > _distance) //Y
        {
            position.y = 2 * _distance;
            booleanPos[0] = 0;
        }
        else if (_mousePos.y < -_distance)
        {
            position.y = -2 * _distance;
            booleanPos[0] = 2;
        }
        if (_gameMatrix[booleanPos[0], booleanPos[1]] == 0)
        {
            _gameMatrix[booleanPos[0], booleanPos[1]] = 1;
            dev = true;
        }
        return (position, dev);
    }

    private bool CalculatePositionEnemy(int row, int column)
    {
        bool dev = false;
        if (_gameMatrix[row, column] == 0)
        {
            _gameMatrix[row, column] = 2;
            dev = true;
        }
        return dev;
    }
    private void ShowMatrix() //fa un debug log de la matriu
    {
        string matrix = "";
        for (var i = 0; i < _length; i++)
        {
            for (var j = 0; j < _length; j++)
            {
                matrix += _gameMatrix[i, j] + " ";
            }
            matrix += "\n";
        }
        Debug.Log(matrix);
    }
    
    private void ShowMatrixPos() //fa un debug log de la matriu
    {
        string matrix = "";
        for (int i = 0; i < _length; i++)
        {
            for (int j = 0; j < _length; j++)
            {
                matrix += _gameMatrixPos[i, j] + " ";
            }
            matrix += "\n";
        }
        Debug.Log(matrix);
    }

    //EL VOSTRE EXERCICI COMEN�A AQUI
    private Vector3 CalculateEnemyPosition(int row, int col)
    {
        float x = (col - 1) * 2f * _distance;
        float y = (1 - row) * 2f * _distance;
        return new Vector3(x, y, 0);
    }
    private void EnemyResponse()
    {
        // Comprovar si el tauler esta ple abans que la IA jugui
        if (IsBoardFull(_gameMatrix)) return;

        // Copiar el tauler actual com a estat arrel
        int[,] currentMatrix = (int[,])_gameMatrix.Clone();

        // Crear node arrel: Team=1 (maximizer/IA), alpha=-infinit, beta=+infinit
        Node root = new Node(null, 1, -100, 100, -1, -1, currentMatrix);

        // Executar MinMax amb poda alpha-beta
        MinMax(root);

        // Obtenir la millor jugada
        if (root.BestChild != null)
        {
            int bestRow = root.BestChild.X;
            int bestCol = root.BestChild.Y;

            Debug.Log("IA tria: [" + bestRow + "," + bestCol + "]");

            // Actualitzar la matriu del joc
            if (CalculatePositionEnemy(bestRow, bestCol))
            {
                // Obtenir posicio mon i instanciar la fitxa
                Vector3 enemyPos = CalculateEnemyPosition(bestRow, bestCol);
                Instantiate(token2, enemyPos, Quaternion.identity);
            }
        }
    }
    private int EvaluateWin()
    {
        int lose = 0; //0 game not ended, 1 wins player, 2 wins enemy
        int winPlayer = 0;
        int winAI = 0;
        int counter = 0;
        while (counter <3 && lose == 0)
        {
            for (int j = 0; j < _length; j++)
            {
                winPlayer += _gameMatrix[counter, j] == 1 ? 1 : 0;
                winAI += _gameMatrix[counter, j] == 2 ? 1 : 0;
            }
            counter++;
            lose = winPlayer == 3 ? 1 : lose;
            lose = winAI == 3 ? 2 : lose;
            winPlayer = 0;
            winAI = 0;
        }
        counter = 0;
        while (counter <3 && lose == 0)
        {
            for (int j = 0; j < _length; j++)
            {
                winPlayer += _gameMatrix[j, counter] == 1 ? 1 : 0;
                winAI += _gameMatrix[j, counter] == 2 ? 1 : 0;
            }
            counter++;
            lose = winPlayer == 3 ? 1 : lose;
            lose = winAI == 3 ? 2 : lose;
            winPlayer = 0;
            winAI = 0;
        }
        counter = 0;
        if (lose == 0)
        {
            for (int j = 0; j < _length; j++)
            {
                winPlayer += _gameMatrix[j, j] == 1 ? 1 : 0;
                winAI += _gameMatrix[j, j] == 2 ? 1 : 0;
            }
            lose = winPlayer == 3 ? 1 : lose;
            lose = winAI == 3 ? 2 : lose;
            winPlayer = 0;
            winAI = 0;
        }
        if (lose == 0)
        {
            for (int j = 0; j < _length; j++)
            {
                winPlayer += _gameMatrix[2 - j, j] == 1 ? 1 : 0;
                winAI += _gameMatrix[2 - j, j] == 2 ? 1 : 0;
            }
            lose = winPlayer == 3 ? 1 : lose;
            lose = winAI == 3 ? 2 : lose;
            counter = 0;
            winPlayer = 0;
            winAI = 0;
        }
        if (lose == 1)
            Debug.Log("Player wins");
        else if (lose == 2)
            Debug.Log("Enemy wins");
        else if (IsBoardFull(_gameMatrix))
            Debug.Log("Draw!");
        return lose;
    }

    /// <summary>
    /// Algorisme MinMax amb poda Alpha-Beta.
    /// Team 1 = maximizer (IA, valor 2 a la matriu del joc)
    /// Team -1 = minimizer (jugador, valor 1 a la matriu del joc)
    /// </summary>
    private void MinMax(Node node)
    {
        // Avaluar si es un estat terminal
        int result = EvaluateMatrix(node.MatrixNode);

        // Cas base: node terminal o tauler ple
        if (result != 0 || IsBoardFull(node.MatrixNode))
        {
            if (result == 2)        // guanya enemic (IA)
                node.Value = 1;
            else if (result == 1)   // guanya jugador
                node.Value = -1;
            else
                node.Value = 0;     // empat
            return;
        }

        // Generar fills per cada casella buida
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (node.MatrixNode[i, j] == 0)
                {
                    // Crear copia del tauler
                    int[,] childMatrix = (int[,])node.MatrixNode.Clone();

                    // Col·locar fitxa segons el torn
                    childMatrix[i, j] = node.Team == 1 ? 2 : 1;

                    // Crear node fill amb l'equip contrari
                    Node child = new Node(node, -node.Team, node.Alpha, node.Beta, i, j, childMatrix);

                    // Crida recursiva
                    MinMax(child);

                    // Propagacio de valors
                    if (node.Team == 1) // Maximizer (IA)
                    {
                        if (child.Value > node.Value)
                        {
                            node.Value = child.Value;
                            node.BestChild = child;
                        }
                        node.Alpha = Mathf.Max(node.Alpha, node.Value);
                    }
                    else // Minimizer (jugador)
                    {
                        if (child.Value < node.Value)
                        {
                            node.Value = child.Value;
                            node.BestChild = child;
                        }
                        node.Beta = Mathf.Min(node.Beta, node.Value);
                    }

                    node.NodeChildren.Push(child);

                    // Poda alpha-beta: si alpha >= beta, podar branques restants
                    if (node.Alpha >= node.Beta)
                    {
                        node.Pruned = true;
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Avalua una matriu de joc arbitraria per determinar si hi ha un guanyador.
    /// Retorna: 0 = sense guanyador, 1 = guanya jugador, 2 = guanya IA
    /// </summary>
    private int EvaluateMatrix(int[,] matrix)
    {
        for (int i = 0; i < 3; i++)
        {
            // Comprovar files
            if (matrix[i, 0] != 0 && matrix[i, 0] == matrix[i, 1] && matrix[i, 1] == matrix[i, 2])
                return matrix[i, 0];
            // Comprovar columnes
            if (matrix[0, i] != 0 && matrix[0, i] == matrix[1, i] && matrix[1, i] == matrix[2, i])
                return matrix[0, i];
        }
        // Diagonal principal
        if (matrix[0, 0] != 0 && matrix[0, 0] == matrix[1, 1] && matrix[1, 1] == matrix[2, 2])
            return matrix[0, 0];
        // Diagonal secundaria
        if (matrix[2, 0] != 0 && matrix[2, 0] == matrix[1, 1] && matrix[1, 1] == matrix[0, 2])
            return matrix[2, 0];

        return 0;
    }

    /// <summary>
    /// Comprova si totes les caselles del tauler estan ocupades.
    /// </summary>
    private bool IsBoardFull(int[,] matrix)
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (matrix[i, j] == 0)
                    return false;
        return true;
    }
    
}
