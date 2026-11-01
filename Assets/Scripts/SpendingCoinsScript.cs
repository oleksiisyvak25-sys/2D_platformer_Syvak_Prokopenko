using UnityEngine;

public class SpendingCoinsScript : MonoBehaviour
{
    [SerializeField]
    private int _solution;

    private bool _isSolved = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<CollectingCoins>(out CollectingCoins collector) && !_isSolved)
        {
            Solve(collector); 
        }
    }
    private void Solve(CollectingCoins col)
    {
        if (col.NumberOfCoins >= _solution)
        {
            Destroy(gameObject);
            _isSolved = true;
            col.IncreaseNumberOfCoins(-_solution);
        }
    }
}
