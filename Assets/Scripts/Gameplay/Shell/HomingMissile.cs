using Photon.Pun;
using UnityEngine;

namespace Tanks
{
    public class HomingMissile : MonoBehaviour, IPunInstantiateMagicCallback
    {
        [SerializeField] private Rigidbody missileBody;
        [SerializeField] private float speed = 12;
        [SerializeField] private PhotonView view;
        [SerializeField] private ShellExplosion shellExplosion;

        private Rigidbody target;
        private int targetViewId;

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] instantiationData = info.photonView.InstantiationData;

            targetViewId = (int)instantiationData[0];
            target = PhotonView.Find(targetViewId).GetComponent<Rigidbody>();

            if (view.IsMine)
                view.TransferOwnership(PhotonNetwork.MasterClient);
        }

        private void FixedUpdate()
        {
            if (!view.IsMine) return;

            var direction = (target.position - transform.position).normalized;
            direction.y = 0;
            transform.forward = direction;

            Vector3 movement = direction * speed * Time.deltaTime;
            missileBody.MovePosition(missileBody.position + movement);
        }
    }
}