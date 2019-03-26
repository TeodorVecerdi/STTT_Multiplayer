using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TeodorVecerdi {
    public class Player : MonoBehaviourPunCallbacks, IPunObservable {
        public GameObject PlayerMousePrefab;
        
        [HideInInspector] public string Nickname;
        [HideInInspector] public Color PlayerColor;
        [HideInInspector] public Vector3 mousePosition;
        [HideInInspector] public PlayerController playerController;
        [HideInInspector] public int localPlayer;

        private Vector3 offset = new Vector3(76f*2, -16f*2, 0f);
        private Transform PlayerMousePanel;
        private TextMeshProUGUI PlayerMousePanel_PlayerName;
        private Image PlayerMousePanel_Color;

        private void Awake() {
            playerController = GetComponent<PlayerController>();
            if (!photonView.IsMine && playerController != null) {
                Destroy(playerController);
            }

            PlayerMousePanel = Instantiate(PlayerMousePrefab, Vector3.zero, Quaternion.identity).transform;
            PlayerMousePanel.parent = GameObject.Find("Canvas").transform;
        }

        void Start() {
            PlayerMousePanel_PlayerName = PlayerMousePanel.Find("PlayerName").GetComponent<TextMeshProUGUI>();
            PlayerMousePanel_Color = PlayerMousePanel.Find("Color").GetComponent<Image>();
            photonView.RPC("UpdateDataRPC", RpcTarget.OthersBuffered, Nickname, PlayerColor.r, PlayerColor.g, PlayerColor.b, localPlayer);
        }

        void FixedUpdate() {
            if(!Application.isFocused) return;
            PlayerMousePanel.position = mousePosition + offset;
            UpdateData();
        }

        public void Initialize() {
            PlayerColor = Random.ColorHSV();
            Nickname = PhotonNetwork.NickName;
            localPlayer = PhotonNetwork.CurrentRoom.PlayerCount == 1 ? -1 : 1;
            if (string.IsNullOrEmpty(PhotonNetwork.NickName))
                Nickname = "Unnamed " + photonView.Owner.ActorNumber;
            UpdateData();
        }

        public void UpdateData() {
            PlayerMousePanel_Color.color = PlayerColor;
            PlayerMousePanel_PlayerName.text = $"({(localPlayer == -1 ? "X" : "O")}) {Nickname}";
        }

        [PunRPC]
        public void UpdateDataRPC(string nickname, float r, float g, float b, int playerType, PhotonMessageInfo info) {
            var player = info.photonView.gameObject.GetComponent<Player>();
            player.localPlayer = playerType;
            player.Nickname = nickname;
            player.PlayerColor = new Color(r, g, b);
            Debug.Log($"Received update from player {info.photonView.Owner.NickName}/{info.photonView.Owner.ActorNumber}");
        }

        [PunRPC]
        public void TryMakeMoveRPC(int globalIndexX, int globalIndexY, int localIndexX, int localIndexY, int currentPlayer, PhotonMessageInfo info) {
            var manager = GameObject.Find("GameManager").GetComponent<GameManager>();
            manager.LocalPlayer.playerController.TryMakeMoveLocal(globalIndexX, globalIndexY, localIndexX, localIndexY, currentPlayer);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            if (stream.IsWriting) {
                stream.SendNext(mousePosition.x);
                stream.SendNext(mousePosition.y);
                stream.SendNext(mousePosition.z);
            }
            else {
                mousePosition.x = (float) stream.ReceiveNext();
                mousePosition.y = (float) stream.ReceiveNext();
                mousePosition.z = (float) stream.ReceiveNext();
            }
        }
    }
}