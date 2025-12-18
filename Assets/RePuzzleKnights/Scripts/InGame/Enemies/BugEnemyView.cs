using UnityEngine;
using UnityEngine.UI;

namespace RePuzzleKnights.Scripts.InGame.Enemies
{
    /// <summary>
    /// 敵のステータスUIを表示するViewクラス
    /// </summary>
    public class BugEnemyStatusView : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Canvas canvas;

        /// <summary>
        /// 初期化時に最大HPを設定する
        /// </summary>
        public void SetMaxHp(float maxHp)
        {
            if (hpSlider == null) 
                return;

            hpSlider.maxValue = maxHp;
            hpSlider.value = maxHp;

            // 復活などを考慮して表示状態をリセット
            if (canvas != null) 
                canvas.enabled = true;
            else 
                gameObject.SetActive(true);
        }

        /// <summary>
        /// 現在のHPを更新する
        /// </summary>
        public void UpdateSlider(float hp)
        {
            if (hpSlider == null) 
                return;

            hpSlider.value = hp;

            // HPが0以下になったら非表示にする
            if (hp <= 0.0f)
            {
                if (canvas != null)
                    canvas.enabled = false;
                else
                    gameObject.SetActive(false);
            }
        }
    }
}