using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class PlayerLobbyEntry : MonoBehaviour
    {
        [SerializeField] private Button readyButton;
        [SerializeField] private GameObject readyText;
        [SerializeField] private Button waitingButton;
        [SerializeField] private GameObject waitingText;

        [SerializeField] private TMP_Text playerName;
        [SerializeField] private Button changeTeamButton;
        [SerializeField] private Image teamHolder;
        [SerializeField] private List<Sprite> teamBackgrounds;

        private Player player;

        public int PlayerTeam
        {
            get => player.CustomProperties.ContainsKey("Team") ? (int)player.CustomProperties["Team"] : 0;
            set
            {
                Hashtable hash = new Hashtable { { "Team", value } };
                player.SetCustomProperties(hash);
            }
        }

        public bool IsPlayerReady
        {
            get => player.CustomProperties.ContainsKey("IsReady") && (bool)player.CustomProperties["IsReady"];
            set
            {
                Hashtable hash = new Hashtable { { "IsReady", value } };
                player.SetCustomProperties(hash);
            }
        }

        private bool IsLocalPlayer => Equals(player, PhotonNetwork.LocalPlayer);

        public void Setup(Player entryPlayer)
        {
            player = entryPlayer;

            if (IsLocalPlayer)
            {
                PlayerTeam = GetAvailableTeam(0);
            }

            playerName.text = player.NickName;

            if (!IsLocalPlayer)
                Destroy(changeTeamButton);

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            teamHolder.sprite = teamBackgrounds[PlayerTeam];

            waitingText.SetActive(!IsPlayerReady);
            readyText.SetActive(IsPlayerReady);
        }

        private void Start()
        {
            Debug.Assert(PhotonNetwork.CurrentRoom.MaxPlayers == teamBackgrounds.Count,
                "Incorrect number of team backgrounds");

            waitingButton.onClick.AddListener(() => OnReadyButtonClick(true));
            readyButton.onClick.AddListener(() => OnReadyButtonClick(false));
            changeTeamButton.onClick.AddListener(OnChangeTeamButtonClicked);

            waitingButton.gameObject.SetActive(IsLocalPlayer);
            readyButton.gameObject.SetActive(false);
        }

        private int GetAvailableTeam(int desiredTeam)
        {
            while (true)
            {
                bool available = true;

                foreach (var currentPlayer in PhotonNetwork.PlayerList)
                {
                    if (currentPlayer == PhotonNetwork.LocalPlayer) continue;

                    int team = currentPlayer.CustomProperties.ContainsKey("Team") ?
                        (int) currentPlayer.CustomProperties["Team"] : 0;

                    if (team == desiredTeam)
                    {
                        available = false;
                        desiredTeam = (desiredTeam + 1) % PhotonNetwork.CurrentRoom.MaxPlayers;
                    }
                }

                if (available) return desiredTeam;
            }
        }

        private void OnChangeTeamButtonClicked()
        {
            PlayerTeam = GetAvailableTeam((PlayerTeam + 1) % PhotonNetwork.CurrentRoom.MaxPlayers);
        }

        private void OnReadyButtonClick(bool isReady)
        {
            waitingButton.gameObject.SetActive(!isReady);
            waitingText.SetActive(!isReady);
            readyButton.gameObject.SetActive(isReady);
            readyText.SetActive(isReady);

            IsPlayerReady = isReady;
        }
    }
}