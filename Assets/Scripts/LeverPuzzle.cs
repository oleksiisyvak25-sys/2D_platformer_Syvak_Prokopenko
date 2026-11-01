using UnityEngine;
using UnityEngine.Events;

public class LeverPuzzle : MonoBehaviour
{
    [Header("Puzzle")]
    [SerializeField] private Lever[] levers;
    [SerializeField] private int[] solution;

    [Header("Result")]
    [SerializeField] private DoorController[] doors;
    [SerializeField] private bool lockLeversWhenSolved = true;
    [SerializeField] private bool closeDoorsIfBroken = false;
    [SerializeField] private UnityEvent onSolved;

    public bool IsSolved { get; private set; }

    private void Awake()
    {
        if (levers == null) return;

        foreach (var lever in levers)
        {
            if (lever != null) lever.Changed += OnLeverChanged;
        }
    }

    private void Start()
    {
        if (Matches())
        {
            Debug.LogWarning("LeverPuzzle: комбінація розв'язана вже на старті, змін Start State у важелів!", this);
        }
    }

    private void OnDestroy()
    {
        if (levers == null) return;

        foreach (var lever in levers)
        {
            if (lever != null) lever.Changed -= OnLeverChanged;
        }
    }

    private void OnValidate()
    {
        if (levers != null && (solution == null || solution.Length != levers.Length))
        {
            System.Array.Resize(ref solution, levers.Length);
        }
    }

    private void OnLeverChanged(Lever changed)
    {
        bool matches = Matches();

        if (matches && !IsSolved)
        {
            Solve();
        }
        else if (!matches && IsSolved && closeDoorsIfBroken && !lockLeversWhenSolved)
        {
            Unsolve();
        }
    }

    private bool Matches()
    {
        if (levers == null || solution == null || levers.Length == 0 || levers.Length != solution.Length)
        {
            return false;
        }

        for (int i = 0; i < levers.Length; i++)
        {
            if (levers[i] == null || levers[i].State != solution[i]) return false;
        }

        return true;
    }

    private void Solve()
    {
        IsSolved = true;

        if (doors != null)
        {
            foreach (var door in doors)
            {
                if (door != null) door.Open();
            }
        }

        if (lockLeversWhenSolved)
        {
            foreach (var lever in levers)
            {
                if (lever != null) lever.SetLocked(true);
            }
        }

        onSolved?.Invoke();
    }

    private void Unsolve()
    {
        IsSolved = false;

        if (doors == null) return;

        foreach (var door in doors)
        {
            if (door != null) door.Close();
        }
    }
}