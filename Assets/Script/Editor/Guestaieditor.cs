#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GuestAI), true)]
public class GuestAIEditor : Editor
{
    bool _foldService = true;
    bool _foldEconomy = false;
    bool _foldUI = false;
    bool _foldVisual = false;

    static GUIStyle _headerStyle;
    static GUIStyle _badgeStyle;
    static GUIStyle _boxStyle;

    void InitStyles()
    {
        if (_headerStyle != null) return;

        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };

        _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        _boxStyle = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(8, 8, 6, 6) };
    }

    public override void OnInspectorGUI()
    {
        InitStyles();
        GuestAI guest = (GuestAI)target;
        serializedObject.Update();

        // ── Type badge ───────────────────────────
        string typeName = target.GetType().Name.Replace("Guest", "");
        Color typeColor = typeName switch
        {
            "Ghost" => new Color(0.5f, 0.8f, 1f),
            "Vampire" => new Color(0.6f, 0.2f, 0.8f),
            "Werewolf" => new Color(0.9f, 0.5f, 0.1f),
            "Mummy" => new Color(0.9f, 0.85f, 0.5f),
            "Franken" => new Color(0.3f, 0.9f, 0.5f),
            "Reaper" => new Color(0.5f, 0.5f, 0.5f),
            "Witch" => new Color(0.8f, 0.3f, 0.8f),
            _ => new Color(0.7f, 0.7f, 0.7f)
        };

        EditorGUILayout.Space(4);
        Rect typeRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(typeRect, typeColor * 0.7f);
        EditorGUI.LabelField(typeRect, target.GetType().Name, _headerStyle);
        EditorGUILayout.Space(4);

        // ── Heart bar (always visible) ───────────
        EditorGUILayout.BeginVertical(_boxStyle);

        float maxHeart = 5f;
        float ratio = Application.isPlaying ? Mathf.Clamp01(guest.heart / maxHeart) : 1f;
        Color barColor = ratio > 0.5f
            ? Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2f)
            : Color.Lerp(Color.red, Color.yellow, ratio * 2f);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Heart", GUILayout.Width(80));
        Rect barRect = GUILayoutUtility.GetRect(0, 16, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(barRect, new Color(0.2f, 0.2f, 0.2f));
        EditorGUI.DrawRect(new Rect(barRect.x, barRect.y, barRect.width * ratio, barRect.height), barColor);
        string heartText = Application.isPlaying ? $"{guest.heart:0.0} / {maxHeart}" : "edit in Prefab";
        EditorGUI.LabelField(barRect, heartText, _badgeStyle);
        EditorGUILayout.EndHorizontal();

        // Phase badge (play mode only)
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Phase", GUILayout.Width(80));
            Color phaseColor = guest.guestPhase switch
            {
                Guestphase.CheckingIn => new Color(0.3f, 0.7f, 1f),
                Guestphase.InRoom => new Color(0.3f, 0.9f, 0.4f),
                Guestphase.RequestingService => new Color(1f, 0.8f, 0.1f),
                Guestphase.CheckingOut => new Color(1f, 0.3f, 0.3f),
                _ => Color.gray
            };
            Rect phaseRect = GUILayoutUtility.GetRect(0, 18, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(phaseRect, phaseColor * 0.6f);
            EditorGUI.LabelField(phaseRect, guest.guestPhase.ToString(), _badgeStyle);
            EditorGUILayout.EndHorizontal();
        }

        // Decay
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Decay per tick", GUILayout.Width(108));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("decaysHit"), GUIContent.none);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);

        // ── Service ─────────────────────────────
        _foldService = DrawSection("Service", _foldService, () =>
        {
            DrawField("servicePool", "Pool (drag ItemSOs)");
            DrawField("serviceCount", "Items per stay");
            DrawField("deliveryPerSlot", "Deliveries per slot");

            if (Application.isPlaying && guest.currentService != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Current Request", GUILayout.Width(120));
                EditorGUILayout.LabelField(guest.currentService.itemName, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
            }
        });

        // ── Subclass-specific ────────────────────
        DrawSubclassSection(guest);

        // ── Economy ─────────────────────────────
        _foldEconomy = DrawSection("Economy", _foldEconomy, () =>
        {
            DrawField("roomPayment", "Room Payment");
            DrawField("tip", "Tip");
            DrawField("servicePayment", "Service Payment");
            if (Application.isPlaying)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.IntField("Total Income", guest.totalIncome);
                EditorGUI.EndDisabledGroup();
            }
        });

        // ── UI & Visual ─────────────────────────
        _foldUI = DrawSection("UI & Visual", _foldUI, () =>
        {
            DrawField("characterVisual", "Character Visual");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Guest UI (auto)", guest.guestUI, typeof(GuestUIController), true);
            EditorGUI.EndDisabledGroup();
        });

        serializedObject.ApplyModifiedProperties();

        if (Application.isPlaying) Repaint();
    }

    // ─────────────────────────────────────────────
    //  Subclass sections
    // ─────────────────────────────────────────────

    void DrawSubclassSection(GuestAI guest)
    {
        if (guest is WerewolfGuest wolf)
        {
            DrawSection("Werewolf — Anger Stack", true, () =>
            {
                DrawField("maxAngerBars", "Max Bars");
                DrawField("barDrainInterval", "Drain Interval (s)");

                if (Application.isPlaying)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Anger", GUILayout.Width(80));
                    Rect r = GUILayoutUtility.GetRect(0, 16, GUILayout.ExpandWidth(true));
                    float ratio = wolf.maxAngerBars > 0 ? (float)wolf.CurrentAngerBars / wolf.maxAngerBars : 0f;
                    EditorGUI.DrawRect(r, new Color(0.2f, 0.2f, 0.2f));
                    EditorGUI.DrawRect(new Rect(r.x, r.y, r.width * ratio, r.height), new Color(1f, 0.4f, 0f));
                    EditorGUI.LabelField(r, $"{wolf.CurrentAngerBars} / {wolf.maxAngerBars}", _badgeStyle);
                    EditorGUILayout.EndHorizontal();
                }
            }, true);
        }
        else if (guest is FrankenGuest franken)
        {
            DrawSection("Franken — Sleepwalk", true, () =>
            {
                DrawField("heartLossPerSecond", "Heart Loss / sec");
                DrawField("maxExtraDelay", "Max Extra Delay (s)");
                DrawField("sleepwalkPoints", "Sleepwalk Points");

                if (Application.isPlaying)
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Toggle("Is Sleepwalking", franken.IsSleepwalking);
                    EditorGUI.EndDisabledGroup();
                }
            }, true);
        }
        else if (guest is MummyGuest)
        {
            DrawSection("Mummy — Cloth Event", true, () =>
            {
                DrawField("clothItem", "Cloth ItemSO");
                EditorGUILayout.HelpBox("servicePool → Food/Drink/Soul เท่านั้น อย่าใส่ Cloth", MessageType.Info);
            }, true);
        }
        else if (guest is WitchGuest)
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.HelpBox("Witch: deliveryPerSlot = 2 (ส่งของ 2 ชิ้นต่อ 1 slot)\nตั้งค่าใน Service section ด้านบน", MessageType.Info);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        else if (guest is ReaperGuest)
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.HelpBox("Reaper: ต้องการ RoomType.Big เท่านั้น\nGuestRoomAssigner เช็กให้อัตโนมัติ", MessageType.Info);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
    }

    // ─────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────

    bool DrawSection(string title, bool foldout, System.Action content, bool alwaysOpen = false)
    {
        if (alwaysOpen)
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            content();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
            return true;
        }

        // ใช้ Foldout แทน BeginFoldoutHeaderGroup เพื่อป้องกัน nesting error
        EditorGUILayout.BeginVertical(_boxStyle);
        foldout = EditorGUILayout.Foldout(foldout, title, true, EditorStyles.foldoutHeader);
        if (foldout)
        {
            EditorGUI.indentLevel++;
            content();
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(1);
        return foldout;
    }

    void DrawField(string propName, string label)
    {
        var prop = serializedObject.FindProperty(propName);
        if (prop != null)
            EditorGUILayout.PropertyField(prop, new GUIContent(label));
    }
}
#endif