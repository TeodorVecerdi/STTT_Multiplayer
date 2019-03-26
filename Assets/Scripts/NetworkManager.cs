using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TeodorVecerdi {
    public class NetworkManager : MonoBehaviourPunCallbacks {
        public TMP_InputField NicknameText;
        protected bool TriesToConnectToRoom;

        private void Start() {
            DontDestroyOnLoad(this);
            ConnectToMaster();
            TriesToConnectToRoom = false;
        }


        private void ConnectToMaster() {
            PhotonNetwork.OfflineMode = false;
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.GameVersion = "v1";
            PhotonNetwork.ConnectUsingSettings();
        }

        public override void OnConnectedToMaster() {
            base.OnConnectedToMaster();
            Debug.Log("Connected to Master!");
        }

        public void OnClickConnectToRoom() {
            if (!PhotonNetwork.IsConnected)
                return;

            TriesToConnectToRoom = true;
            PhotonNetwork.NickName = NicknameText.text;
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message) {
            base.OnJoinRandomFailed(returnCode, message);
            PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = 2});
        }

        public override void OnCreateRoomFailed(short returnCode, string message) {
            base.OnCreateRoomFailed(returnCode, message);
            Debug.Log(message);
            TriesToConnectToRoom = false;
        }

        public override void OnJoinedRoom() {
            base.OnJoinedRoom();
            TriesToConnectToRoom = false;
            PhotonNetwork.SetPlayerCustomProperties(new Hashtable {{"player", PhotonNetwork.CurrentRoom.PlayerCount == 1 ? -1 : 1}});
            Debug.Log("Master: " + PhotonNetwork.IsMasterClient + " | Players In Room: " + PhotonNetwork.CurrentRoom.PlayerCount + " | RoomName: " + PhotonNetwork.CurrentRoom.Name);
            if (PhotonNetwork.IsMasterClient && SceneManager.GetActiveScene().name != "Main")
                SceneManager.LoadScene("Main");
        }
    }
}