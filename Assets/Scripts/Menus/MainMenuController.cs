using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Tanks
{
    public class MainMenuController : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private SettingsController settingsPopup;

        private Action pendingAction;

        private void Start()
        {
            if (!PhotonNetwork.IsConnectedAndReady)
                PhotonNetwork.ConnectUsingSettings();

            playButton.onClick.AddListener(() => OnConnectionDependentActionClicked(JoinRandomRoom));
            lobbyButton.onClick.AddListener(() => OnConnectionDependentActionClicked(GoToLobbyList));
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);

            settingsPopup.gameObject.SetActive(false);
            settingsPopup.Setup();

            if (!PlayerPrefs.HasKey("PlayerName"))
                PlayerPrefs.SetString("PlayerName", "Player #" + Random.Range(0, 9999));
        }

        private void OnConnectionDependentActionClicked(Action action)
        {
            if (pendingAction != null) return;
            pendingAction = action;

            LoadingGraphics.Enable();

            if (PhotonNetwork.IsConnectedAndReady)
                action();
        }

        private void OnSettingsButtonClicked()
        {
            settingsPopup.gameObject.SetActive(true);
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to Master");
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");

            pendingAction?.Invoke();
        }

        public override void OnJoinedRoom()
        {
            SceneManager.LoadScene("RoomLobby");
        }

        public void JoinRandomRoom()
        {
            RoomOptions roomOptions = new RoomOptions { IsOpen = true, MaxPlayers = 4 };
            PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: roomOptions);
        }

        private void GoToLobbyList()
        {
            SceneManager.LoadSceneAsync("LobbyList");
        }
    }
}