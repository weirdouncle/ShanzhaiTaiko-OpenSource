using UnityEngine;

public class NijiiroResultPaopaoScript : MonoBehaviour
{
    public GameObject[] Player1Result;
    public GameObject[] Player2Result;
    public Transform Player2Parent;
    public void SetPaopao(int result_index, bool player2)
    {
        if (player2)
            Instantiate(Player2Result[result_index], Player2Parent);
        else
            Instantiate(Player1Result[result_index], transform);
    }
}
