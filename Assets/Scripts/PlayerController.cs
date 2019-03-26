using Photon.Pun;
using TMPro;
using UnityEngine;

namespace TeodorVecerdi {
    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour {
        protected Player Player;
        private bool shouldUpdate;
        public GameObject LocalBoardPrefab;
        public GameObject GlobalBoardPrefab;

        private TTTBoard[,] uiLocalBoards;
        private Board[,] localBoards;
        private Board globalBoard;
        private Transform boardParent;
        private Transform uiGlobalBoard;
        private Vector3 screenOffset;
        private int currentPlayer = -1;
        private int availableBoards = BoardHelper.All;
        private TextMeshProUGUI CurrentPlayerText;

        private void Start() {
            Player = GetComponent<Player>();
            CurrentPlayerText = GameObject.Find("CurrentPlayerText").GetComponent<TextMeshProUGUI>();
            screenOffset = new Vector3(Screen.width / 2f - 300 * 1.5f, Screen.height / 2f - 300 * 1.5f);
            boardParent = GameObject.Find("BoardParent").transform;
            globalBoard = new Board();
            localBoards = new Board[3, 3];
            uiLocalBoards = new TTTBoard[3, 3];
            for (int x = 0; x < 3; x++) {
                for (int y = 0; y < 3; y++) {
                    uiLocalBoards[x, y] = Instantiate(LocalBoardPrefab, boardParent).GetComponent<TTTBoard>();
                    uiLocalBoards[x, y].GetRectTransform().localPosition = new Vector3((x - 1) * 300, (y - 1) * -300);
                    localBoards[x, y] = new Board();
                }
            }

            uiGlobalBoard = Instantiate(GlobalBoardPrefab, boardParent).transform;
        }
        private void FixedUpdate() {
            if(!Application.isFocused) return;
            Player.mousePosition = Input.mousePosition;
            if (Input.GetMouseButtonDown(0) && currentPlayer == Player.localPlayer && !globalBoard.Finished) {
                var convertedMousePosition = GetMousePosition();
                var actualMousePosition = convertedMousePosition - screenOffset;
                int globalIndexX = (int) actualMousePosition.x / 300;
                int globalIndexY = (int) actualMousePosition.y / 300;
                int localIndexX = (int) actualMousePosition.x / 100 % 3;
                int localIndexY = (int) actualMousePosition.y / 100 % 3;

                if (globalIndexX >= 0 && globalIndexX <= 2 && globalIndexY >= 0 && globalIndexY <= 2) {
                    if ((availableBoards & BoardHelper.Boards[globalIndexX * 3 + globalIndexY]) != 0) {
                        Player.photonView.RPC("TryMakeMoveRPC", RpcTarget.AllBuffered, globalIndexX, globalIndexY, localIndexX, localIndexY, currentPlayer);
                    }
                }
            }
        }

        public void TryMakeMoveLocal(int globalIndexX, int globalIndexY, int localIndexX, int localIndexY, int player) {
            (int code, string message) _ = localBoards[globalIndexX, globalIndexY].TryMakeMove(localIndexX, localIndexY, player);
            if (_.code != -2) {
                if (_.code != 0) {
                    uiLocalBoards[globalIndexX, globalIndexY].SetWin(player);
                    var __ = globalBoard.TryMakeMove(globalIndexX, globalIndexY, player);
                    if (globalBoard.Finished) {
                        for (int i = 0; i < 3; i++) {
                            for (int j = 0; j < 3; j++) {
                                if (!uiLocalBoards[i, j].Finished) uiLocalBoards[i, j].SetWin(-2);
                            }
                        }
                    }
                }

                uiLocalBoards[globalIndexX, globalIndexY].SetMove(localIndexX, localIndexY, player);
                availableBoards = localBoards[localIndexX, localIndexY].Finished ? BoardHelper.All : BoardHelper.Boards[localIndexX * 3 + localIndexY];
                currentPlayer *= -1;
                UpdateUI();
            }
        }


        // ReSharper disable once InconsistentNaming
        private void UpdateUI() {
            CurrentPlayerText.text = "Current player: " + (currentPlayer == -1 ? "X" : "O");
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    uiLocalBoards[i, j].SetAvailable((availableBoards & BoardHelper.Boards[i * 3 + j]) != 0);
                }
            }
        }

        private Vector3 GetMousePosition() => new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
    }
}