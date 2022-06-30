using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class TankShooting : MonoBehaviour
    {
        private const string FIRE_BUTTON = "Fire1";
        private const string HOMING_MISSILE_BUTTON = "Fire2";

        public Rigidbody shell;
        public Transform fireTransform;
        public Slider aimSlider;
        public AudioSource shootingAudio;
        public AudioClip chargingClip;
        public AudioClip fireClip;
        public float minLaunchForce = 15f;
        public float maxLaunchForce = 30f;
        public float maxChargeTime = 0.75f;

        public float homingMissileInstantiateOffset = 4;

        private PhotonView photonView;

        private float currentLaunchForce;
        private float chargeSpeed;
        private bool isChargingMissile;

        private void OnEnable()
        {
            currentLaunchForce = minLaunchForce;
            aimSlider.value = minLaunchForce;
        }

        private void Start()
        {
            chargeSpeed = (maxLaunchForce - minLaunchForce) / maxChargeTime;
            photonView = GetComponent<PhotonView>();
        }

        private void Update()
        {
            UpdateMissileCharge();

            if (!photonView.IsMine) return;

            TryFireMissile();
            TryFireHomingMissile();
        }

        private void UpdateMissileCharge()
        {
            aimSlider.value = minLaunchForce;

            if (!isChargingMissile) return;

            currentLaunchForce += chargeSpeed * Time.deltaTime;
            aimSlider.value = currentLaunchForce;
        }

        private void TryFireHomingMissile()
        {
            if (!Input.GetButtonDown(HOMING_MISSILE_BUTTON)) return;

            if (!GetClickPosition(out var clickPos)) return;

            Collider[] colliders = Physics.OverlapSphere(clickPos, 5, LayerMask.GetMask("Players"));

            foreach (var tankCollider in colliders)
            {
                if (tankCollider.gameObject == gameObject) continue;

                var direction = (tankCollider.transform.position - transform.position).normalized;

                var position = transform.position + direction * homingMissileInstantiateOffset + Vector3.up;
                object[] data = { tankCollider.GetComponent<PhotonView>().ViewID };

                PhotonNetwork.Instantiate(
                    nameof(HomingMissile),
                    position,
                    Quaternion.LookRotation(transform.forward),
                    0,
                    data);
            }
        }

        private bool GetClickPosition(out Vector3 clickPos)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            var gotHit = Physics.Raycast(ray, out var hit, 1000, LayerMask.GetMask("Default"));

            clickPos = gotHit ? hit.point : Vector3.zero;

            return gotHit;
        }

        private void TryFireMissile()
        {
            if (currentLaunchForce >= maxLaunchForce && isChargingMissile)
            {
                currentLaunchForce = maxLaunchForce;
                FireMissile();
            }
            else if (Input.GetButtonDown(FIRE_BUTTON))
            {
                photonView.RPC(nameof(StartChargingMissile), RpcTarget.All);
            }
            else if (Input.GetButtonUp(FIRE_BUTTON) && isChargingMissile)
            {
                FireMissile();
            }
        }

        [PunRPC]
        private void StartChargingMissile()
        {
            isChargingMissile = true;
            currentLaunchForce = minLaunchForce;

            shootingAudio.clip = chargingClip;
            shootingAudio.Play();
        }


        private void FireMissile()
        {
            photonView.RPC(
                nameof(FireMissile),
                RpcTarget.All,
                fireTransform.position,
                fireTransform.rotation,
                currentLaunchForce * fireTransform.forward);
        }

        [PunRPC]
        private void FireMissile(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            isChargingMissile = false;
            currentLaunchForce = minLaunchForce;

            Rigidbody shellInstance = Instantiate(shell, position, rotation);
            shellInstance.velocity = velocity;

            shootingAudio.clip = fireClip;
            shootingAudio.Play();
        }
    }
}
