using System;

namespace Glosso.Services;

public class AuthStateService
{
    public event Action OnChange;
    private bool isAuthenticated;

    public bool IsAuthenticated
    {
        get => isAuthenticated;
        set
        {
            isAuthenticated = value;
            NotifyStateChanged();
        }
    }
    private void NotifyStateChanged() => OnChange?.Invoke();
}