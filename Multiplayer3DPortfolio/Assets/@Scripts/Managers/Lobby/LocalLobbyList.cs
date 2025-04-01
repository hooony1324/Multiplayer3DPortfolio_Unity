using R3;
using System;
using System.Collections.Generic;

public enum ELobbyQueryState
{
    Empty,
    Fetching,
    Error,
    Fetched
}

public class LocalLobbyList
{
    public ReactiveProperty<ELobbyQueryState> QueryState = new ReactiveProperty<ELobbyQueryState>(ELobbyQueryState.Empty);
    public ReactiveProperty<Dictionary<string, LocalLobby>> CurrentLobbies = new ReactiveProperty<Dictionary<string, LocalLobby>>(new Dictionary<string, LocalLobby>());

    public void Clear()
    {
        CurrentLobbies.Value.Clear();
        QueryState.Value = ELobbyQueryState.Fetched;
    }
}