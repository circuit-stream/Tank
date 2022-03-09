using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Tanks
{
    public class TankManager : MonoBehaviour
    {
        private Player player;
        private TeamConfig teamConfig;
        private PhotonView photonView;
        private TankMovement tankMovement;
        private TankShooting tankShooting;
        private TankHealth tankHealth;
        private GameObject canvasGameObject;

        public string ColoredPlayerName => $"<color=#{ColorUtility.ToHtmlStringRGB(teamConfig.color)}>{player.NickName}</color>";
        public int Wins { get; set; }

        [PunRPC]
        private void OnHit(float explosionForce, Vector3 explosionSource, float explosionRadius, float damage)
        {
            tankMovement.GotHit(explosionForce, explosionSource, explosionRadius);
            tankHealth.TakeDamage(damage);
        }

        public void Awake()
        {
            SetupComponents();

            player = photonView.Owner;
            teamConfig = FindObjectOfType<GameManager>().RegisterTank(this, (int)player.CustomProperties["Team"]);

            SetupRenderers();
        }

        private void SetupComponents()
        {
            photonView = GetComponent<PhotonView>();
            tankShooting = GetComponent<TankShooting>();
            tankHealth = GetComponent<TankHealth>();
            tankMovement = GetComponent<TankMovement>();
            canvasGameObject = GetComponentInChildren<Canvas>().gameObject;
        }

        private void SetupRenderers()
        {
            MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

            foreach (var meshRenderer in renderers)
                meshRenderer.material.color = teamConfig.color;
        }

        public void DisableControl()
        {
            tankMovement.enabled = false;
            tankShooting.enabled = false;

            canvasGameObject.SetActive(false);
        }

        public void EnableControl()
        {
            tankMovement.enabled = true;
            tankShooting.enabled = true;

            canvasGameObject.SetActive(true);
        }

        public void Reset()
        {
            transform.position = teamConfig.spawnPoint.position;
            transform.rotation = teamConfig.spawnPoint.rotation;

            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}