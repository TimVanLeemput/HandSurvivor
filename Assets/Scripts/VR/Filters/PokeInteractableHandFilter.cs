using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;
using HandSurvivor.Utilities;
using MyBox;

[RequireComponent(typeof(PokeInteractable))]
public class PokeInteractableHandFilter : MonoBehaviour, IGameObjectFilter
{
   [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

         [SerializeField, ReadOnly] private PokeInteractable _pokeInteractable = null;
    [SerializeField] private HandFilterType _handFilterType = HandFilterType.Both;
    [SerializeField] private List<GameObject> _gameObjectWithPokeInteractorFilters = null;

    private readonly List<PokeInteractorFilter> _matchingFilters = new List<PokeInteractorFilter>();

    private void Awake()
    {
        _pokeInteractable = GetComponent<PokeInteractable>();
    }

    private void Start()
    {
        // Find and cache filters that match our desired hand filter type
        foreach (var go in _gameObjectWithPokeInteractorFilters)
        {
            var filter = go.GetComponent<PokeInteractorFilter>();
            if (filter == null)
                continue;

            if (_handFilterType == HandFilterType.Both ||
                filter.HandFilterType == _handFilterType)
            {
                _matchingFilters.Add(filter);
                if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                    Debug.Log($"[PokeInteractableHandFilter] Matched {filter.gameObject.name} (HandType: {filter.HandFilterType})", filter.gameObject);
            }
        }

        // Just register *this* filter
        _pokeInteractable.InjectOptionalInteractorFilters(new List<IGameObjectFilter> { this });
    }

    public bool Filter(GameObject go)
    {
        if (_matchingFilters.Count == 0)
            return true; // No filters, allow all

        // Try to detect if 'go' belongs to a known interactor filter
        foreach (var filter in _matchingFilters)
        {
            if (filter == null) continue;

            if (IsSameInteractor(go, filter.gameObject))
            {
                // Allow if hand type matches
                if (_handFilterType == HandFilterType.Both ||
                    filter.HandFilterType == _handFilterType)
                    return true;
                else
                {
                    if (showDebugLogs && HandSurvivor.DebugSystem.DebugLogManager.EnableAllDebugLogs)

                        Debug.Log($"[PokeInteractableHandFilter] Rejected {go.name} due to mismatched hand type.");
                    return false;
                }
            }
        }

        // Not part of any known interactor → reject if a hand type restriction exists
        return _handFilterType == HandFilterType.Both;
    }

    private bool IsSameInteractor(GameObject go, GameObject reference)
    {
        // Check if 'go' or its parents match the reference interactor object
        return go == reference || go.transform.IsChildOf(reference.transform);
    }
}
