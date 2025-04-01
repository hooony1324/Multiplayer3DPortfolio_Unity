using UnityEngine;
using R3;
using static Define;

public class GameManager : MonoBehaviour
{
    public ReactiveProperty<EGameState> LocalGameState { get; private set; } = new(EGameState.Menu);

    public void SetGameState(EGameState state)
    {
        var isLeavingLobby = (state == EGameState.Menu || state == EGameState.JoinMenu) &&
        LocalGameState.Value == EGameState.Lobby;
        LocalGameState.Value = state;

        Debug.Log($"Switching Game State to : {Managers.Game.LocalGameState}");

        if (isLeavingLobby)
            Managers.Lobby.LeaveLobby();
    }

}
