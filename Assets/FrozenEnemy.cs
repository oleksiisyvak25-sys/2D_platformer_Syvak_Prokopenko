using UnityEditor;
using UnityEngine;

public class FrozenEnemy : MonoBehaviour
{
    [SerializeField]
    private GameObject _unfrozenEnemy;

    private bool _isUnfrozen = false;

    public void Unfreeze()
    {   if (!_isUnfrozen)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            Instantiate(_unfrozenEnemy, transform.position, transform.rotation);
            _isUnfrozen = true;
            Destroy(gameObject);
        }    
    }    

}
