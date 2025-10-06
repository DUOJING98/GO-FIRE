using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

/// <summary>
/// プレイヤー入力（新Input System）を受け取り、
/// 発砲/待機などのポーズ切替と GameManager への通知を担当するコンポーネント。
/// </summary>
/// <remarks>
/// - 生成済みの InputSystem_Actions を用いて、プレイヤー1/2の入力マップを個別に有効化。
/// - 入力発生時は GameManager.PlayerPressed(...) に集約してゲーム進行側へ委譲する。
/// - 表示（Sprite切替）と入力処理の責務を明確に分離。
/// </remarks>
public class Push : MonoBehaviour
{
    [Header("Identity / Link")]
    [Tooltip("プレイヤー識別用の名前。\"P1\" または \"P2\" を想定。")]
    public string playerName = " ";

    [Tooltip("ゲーム全体の進行を管理する GameManager への参照。")]
    public GameManager manager;

    private SpriteRenderer spriteRenderer;

    [Header("Pose Sprites")]
    [SerializeField] Sprite standSprite;     // 待機（スタンド）ポーズ用スプライト
    [SerializeField] Sprite fireStandSprite; // 発砲（スタンド）ポーズ用スプライト
    // [SerializeField] Sprite fireCrouchSprite; // しゃがみ発砲（拡張用）
    // [SerializeField] Sprite fireJumpSprite;   // ジャンプ発砲（拡張用）

    // [SerializeField] GameObject readyPrefab; // 準備表示の演出（未使用）
    // private GameObject readyText;

    private InputSystem_Actions action; // 自動生成されたInput Actionsのラッパ

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        action = new InputSystem_Actions();

        // プレイヤーIDに応じて適切なアクションに購読
        if (playerName == "P1")
        {
            action.Player1.Fire.performed += OnFire;
        }
        else if (playerName == "P2")
        {
            action.Player2.Fire.performed += OnFire;
        }
        else
        {
            // 想定外のIDに対する安全策（デバッグ支援）
            Debug.LogWarning($"[Push] 未知の playerName: {playerName}. 'P1' または 'P2' を想定しています。");
        }
    }

    /// <summary>
    /// 発砲ポーズへ切替（演出）。スプライトが未設定の場合は何もしない。
    /// </summary>
    public void SetFirePose()
    {
        if (spriteRenderer != null && fireStandSprite != null)
            spriteRenderer.sprite = fireStandSprite;
    }

    /// <summary>
    /// 待機（スタンド）ポーズへ切替（演出）。
    /// </summary>
    public void SetStandPose()
    {
        if (spriteRenderer != null && standSprite != null)
            spriteRenderer.sprite = standSprite;
    }

    /// <summary>
    /// ラウンド開始時の表示初期化（将来拡張に備え、メソッドとして分離）。
    /// </summary>
    public void ResetRound()
    {
        SetStandPose();
    }

    private void OnEnable()
    {
        // 対応するプレイヤーの入力マップのみ有効化
        if (playerName == "P1") action.Player1.Enable();
        else if (playerName == "P2") action.Player2.Enable();
    }

    private void OnDisable()
    {
        // 無効化でイベントの重複購読や無効入力を防ぐ
        if (playerName == "P1") action.Player1.Disable();
        else if (playerName == "P2") action.Player2.Disable();
    }

    /// <summary>
    /// 発砲入力を受け取ったときのハンドラ（新Input Systemのコールバック）。
    /// 準備フェーズ・ラウンド中いずれも GameManager に委譲し、単一入口で処理する。
    /// </summary>
    private void OnFire(InputAction.CallbackContext context)
    {
        if (manager == null)
        {
            Debug.LogError("[Push] GameManager 参照が未設定のため、入力を処理できません。");
            return;
        }

        // 準備フェーズ中：押下＝準備完了として扱う（GameManager側で一元処理）
        if (manager.isWaitingForReady)
        {
            Debug.Log("waiting ready state");
            manager.PlayerPressed(playerName, true);
            return;
        }

        // 入力不可フェーズは無視（CDMとGMで同期）
        if (!manager.CDM.canInput)
        {
            return;
        }

        // ラウンド中の正規入力：GameManager へ通知（正誤判定はCDM/GM側で実施）
        Debug.Log("can input state");
        manager.PlayerPressed(playerName, true);

        // 以降の拡張（例：連打防止、入力バッファ、長押し対応など）はここに追加
    }

    // public void ClearReady()
    // {
    //     if (readyText != null)
    //     {
    //         Destroy(readyText);
    //         readyText = null;
    //     }
    // }
}
