using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TeodorVecerdi {
    public class GameManager : MonoBehaviourPunCallbacks {
        public Player PlayerPrefab;
        [HideInInspector] public Player LocalPlayer;

        private void Awake() {
            if (!PhotonNetwork.IsConnected) {
                SceneManager.LoadScene("Menu");
            }
        }

        private void Start() {
            LocalPlayer = PhotonNetwork.Instantiate(PlayerPrefab.gameObject.name, Vector3.zero, Quaternion.identity, 0).GetComponent<Player>();
            LocalPlayer.Initialize();
        }

        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player target, Hashtable changedProps) {
            base.OnPlayerPropertiesUpdate(target, changedProps);
            foreach (var prop in changedProps) {
                Debug.Log($"Property {prop.Key} was changed by player {target.NickName}/{target.UserId} to value {prop.Value}");
            }
        }
    }
}