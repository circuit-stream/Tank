using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tanks
{
    public class RoomLobbyController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button closeButton;

        [SerializeField] private PlayerLobbyEntry playerLobbyEntryPrefab;
        [SerializeField] private RectTransform entriesHolder;

        private Dictionary<Player, PlayerLobbyEntry> lobbyEntries;

        private bool IsEveryPlayerReady => lobbyEntries.Values.ToList().TrueForAll(entry => entry.IsPlayerReady);

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AddLobbyEntry(newPlayer);
            UpdateStartButton();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Destroy(lobbyEntries[otherPlayer].gameObject);
            lobbyEntries.Remove(otherPlayer);

            UpdateStartButton();
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            UpdateStartButton();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            lobbyEntries[targetPlayer].UpdateVisuals();

            UpdateStartButton();
        }

        private void AddLobbyEntry(Player player)
        {
            var entry = Instantiate(playerLobbyEntryPrefab, entriesHolder);
            entry.Setup(player);
            lobbyEntries.Add(player, entry);
        }

        private void Start()
        {
            LoadingGraphics.Disable();
            PhotonNetwork.AutomaticallySyncScene = true;
            DestroyHolderChildren();

            closeButton.onClick.AddListener(OnCloseButtonClicked);
            startButton.onClick.AddListener(OnStartButtonClicked);
            startButton.gameObject.SetActive(false);

            lobbyEntries = new Dictionary<Player, PlayerLobbyEntry>(PhotonNetwork.CurrentRoom.MaxPlayers);
            foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
                AddLobbyEntry(player);
        }

        private void UpdateStartButton()
        {
            startButton.gameObject.SetActive(PhotonNetwork.IsMasterClient && IsEveryPlayerReady);
        }

        private void OnStartButtonClicked()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("Trying to start game while not being the MasterClient");
                return;
            }

            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.LoadLevel("Gameplay");
        }

        private void OnCloseButtonClicked()
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("MainMenu");
        }

        private void DestroyHolderChildren()
        {
            for (var i = entriesHolder.childCount - 1; i >= 0; i--) {
                Destroy(entriesHolder.GetChild(i).gameObject);
            }
        }
    }
}