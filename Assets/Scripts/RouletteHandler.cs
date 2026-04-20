using UnityEngine;
using UnityEngine.UI;

public class RouletteHandler : MonoBehaviour
{
    public void StartRoulette()
    {
        Debug.Log("Рулетка начата!");
        SpinRoulette();
    }

    public void SpinRoulette()
    {
        Debug.Log("Рулетка крутиться...");
        int result = Random.Range(0, 37);
        Debug.Log("Результат рулетки: " + result);
    }
}
