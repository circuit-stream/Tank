using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class TankShooting : MonoBehaviour
    {
        private const string FIRE_BUTTON = "Fire1";
        private const string HOMING_MISSILE_BUTTON = "Fire2";
        private const string AIR_STRIKE_BUTTON = "Fire3";

        public Rigidbody shell;
        public Transform fireTransform;
        public Slider aimSlider;
        public AudioSource shootingAudio;
        public AudioClip chargingClip;
        public AudioClip fireClip;
        public float minLaunchForce = 15f;
        public float maxLaunchForce = 30f;
        public float maxChargeTime = 0.75f;

        public GameObject airStrikePrefab;

        public float homingMissileInstantiateOffset = 4;

        private PhotonView photonView;

        private float currentLaunchForce;
        private float chargeSpeed;
        private bool fired;

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
            if (!photonView.IsMine) return;

            TryFireMissile();
            TryFireHomingMissile();
            TryFireAirStrike();
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

        private void TryFireAirStrike()
        {
            if (!Input.GetButtonDown(AIR_STRIKE_BUTTON)) return;
            if (!GetClickPosition(out var clickPos)) return;

            photonView.RPC("FireAirStrike", RpcTarget.All, clickPos + Vector3.up * .001f);
        }

        private void TryFireMissile()
        {
            aimSlider.value = minLaunchForce;

            if (currentLaunchForce >= maxLaunchForce && !fired)
            {
                currentLaunchForce = maxLaunchForce;
                FireMissile();
            }
            else if (Input.GetButtonDown(FIRE_BUTTON))
            {
                fired = false;
                currentLaunchForce = minLaunchForce;

                shootingAudio.clip = chargingClip;
                shootingAudio.Play();
            }
            else if (Input.GetButton(FIRE_BUTTON) && !fired)
            {
                currentLaunchForce += chargeSpeed * Time.deltaTime;

                aimSlider.value = currentLaunchForce;
            }
            else if (Input.GetButtonUp(FIRE_BUTTON) && !fired)
            {
                FireMissile();
            }
        }

        private void FireMissile()
        {
            fired = true;

            photonView.RPC(
                nameof(FireMissile),
                RpcTarget.All,
                fireTransform.position,
                fireTransform.rotation,
                currentLaunchForce * fireTransform.forward);

            currentLaunchForce = minLaunchForce;
        }

        [PunRPC]
        private void FireMissile(Vector3 position, Quaternion rotation, Vector3 velocity)
        {
            Rigidbody shellInstance = Instantiate(shell, position, rotation);
            shellInstance.velocity = velocity;

            shootingAudio.clip = fireClip;
            shootingAudio.Play();
        }

        [PunRPC]
        private void FireAirStrike(Vector3 position)
        {
            Instantiate(airStrikePrefab, position, Quaternion.identity);

            shootingAudio.clip = fireClip;
            shootingAudio.Play();
        }
    }
}
