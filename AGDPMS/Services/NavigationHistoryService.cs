using System.Collections.Concurrent;
using Microsoft.AspNetCore.Components;

namespace AGDPMS.Services;

public sealed class NavigationHistoryService
{
    private readonly LinkedList<string> _history = new();
    private NavigationManager? _navigationManager;
    private readonly object _lock = new();

    public void Attach(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void Push(string uri)
    {
        lock (_lock)
        {
            if (_history.Last?.Value == uri)
            {
                return;
            }
            _history.AddLast(uri);
            // Bound history size
            if (_history.Count > 100)
            {
                _history.RemoveFirst();
            }
        }
    }

    public bool TryNavigateBack()
    {
        lock (_lock)
        {
            if (_navigationManager is null)
            {
                return false;
            }

            if (_history.Count <= 1)
            {
                return false;
            }

            // Remove current
            _history.RemoveLast();
            var previous = _history.Last?.Value;
            if (string.IsNullOrEmpty(previous))
            {
                return false;
            }

            _navigationManager.NavigateTo(previous);
            return true;
        }
    }
}


