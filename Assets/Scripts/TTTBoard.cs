namespace TeodorVecerdi {
    using UnityEngine;
    using UnityEngine.UI;

    public class TTTBoard : MonoBehaviour {
        public Transform CellsParent;
        public Image Blur;
        public Sprite PlayerXSprite;
        public Sprite PlayerOSprite;
        public Material BlurMaterial;

        public Image[,] Cells;
        public bool Finished;

        public void SetAvailable(bool available) {
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    if (Cells[i, j].sprite.name == "Square") {
                        var _ = Cells[i, j].color;
                        Cells[i, j].color = available ? new Color(_.r, _.g, _.b, 100f / 255f) : new Color(_.r, _.g, _.b, 0f);
                    }
                    else {
                        Cells[i, j].color = Color.white;
                    }
                }
            }
        }

        private void Start() {
            Cells = new Image[3, 3];
            for (int i = 0; i < CellsParent.childCount; i++) {
                GameObject ch = CellsParent.GetChild(i).gameObject;
                var _ = ch.name.Split(',');
                Cells[int.Parse(_[1]), int.Parse(_[0])] = ch.GetComponent<Image>();
            }

            var mat = new Material(BlurMaterial.shader);
            mat.CopyPropertiesFromMaterial(BlurMaterial);
            Blur.material = mat;
        }

        public void SetMove(int x, int y, int player) {
            Cells[x, y].sprite = player == -1 ? PlayerXSprite : PlayerOSprite;
            Cells[x, y].color = Color.white;
        }

        public void SetWin(int player) {
            Finished = true;
            Blur.gameObject.SetActive(true);
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    Cells[i, j].gameObject.SetActive(false);
                }
            }

            if (player == -2) {
                Blur.material.SetColor("_Color", Color.white);
                return;
            }

            Blur.material.SetColor("_Color", player == 1 ? new Color(1, 0.58f, 0.58f) : new Color(0.58f, 0.58f, 1f));
        }

        public RectTransform GetRectTransform() => (RectTransform) transform;
    }
}