using UnityEngine;

public class RouletteHandler : MonoBehaviour
{
    public void StartRoulette()
    {
        Debug.Log("Roulette started!");
    }

    public void SpinRoulette()
    {
        Debug.Log("Roulette spinning...");
        int result = Random.Range(0, 37);
        Debug.Log("Roulette result: " + result);
    }
}
