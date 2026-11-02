using UnityEngine.UI;
using UnityEngine;
public class FreezingSystem : MonoBehaviour
{
    [SerializeField]
    private Image _freezeBar;

    [SerializeField]
    private float _maxFreezeLevel;
    [SerializeField]
    private float _freezeRate;
    [SerializeField]
    private float _warmRate;
    private float _currentFreezeLevel = 0;
    private int _countActiveFires = 0;

    private PlayerHealth _playerHealth;
    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
    }
    private void Update()
    {
        Freezing();
        UpdateFrozingBar();
    }

    private void Freezing()
    {
        if (_countActiveFires == 0 && _currentFreezeLevel < _maxFreezeLevel)
        {
            _currentFreezeLevel = Mathf.Clamp(_currentFreezeLevel + _freezeRate * Time.deltaTime, 0, _maxFreezeLevel);
        }
        else if (_countActiveFires > 0 && _currentFreezeLevel > 0)
        {
            _currentFreezeLevel = Mathf.Clamp(_currentFreezeLevel - _warmRate * Time.deltaTime, 0, _maxFreezeLevel);
        }
        DieByFrozing();
    }

    private void DieByFrozing()
    {
        if (_currentFreezeLevel >= _maxFreezeLevel)
        {
            _playerHealth.Kill();
        }
    }

    public void AddCampFire(int campFire)
    {
        _countActiveFires = _countActiveFires + campFire;
    }

    private float FrozingProgress()
    {
        if (_maxFreezeLevel > 0)
        {
            return _currentFreezeLevel / _maxFreezeLevel;
        }

        Debug.Log("_maxFreezeLevel should be bigger than 0");
        return 0;
    }

    private void UpdateFrozingBar()
    {
        _freezeBar.fillAmount = FrozingProgress();
    }

    public void ResetFreezingSystem()
    {
        _currentFreezeLevel = 0;
        _countActiveFires = 0;   
    }
}
