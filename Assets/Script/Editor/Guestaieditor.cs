//#if UNITY_EDITOR
//using UnityEditor;
//using UnityEngine;

//[CustomEditor(typeof(GuestAI), true)]
//public class GuestAIEditor : Editor
//{
//    // foldout แยกกันทุก section — ไม่ซ้อนกัน
//    private bool _foldSetup = true;
//    private bool _foldHide = true;
//    private bool _foldPool = true;
//    private bool _foldSub = true;
//    private bool _foldOther = false; // ปิดเริ่มต้น — เปิดเมื่อต้องการ

//    static readonly Color C1 = new Color(0.22f, 0.24f, 0.28f);
//    static readonly Color C2 = new Color(0.16f, 0.20f, 0.32f);
//    static readonly Color CR = new Color(0.12f, 0.22f, 0.14f);
//    static readonly Color C_Other = new Color(0.20f, 0.20f, 0.20f); // เทาเข้ม

//    static readonly Color C3_Wolf = new Color(0.28f, 0.16f, 0.10f);
//    static readonly Color C3_Franken = new Color(0.10f, 0.20f, 0.30f);
//    static readonly Color C3_Mummy = new Color(0.24f, 0.20f, 0.08f);
//    static readonly Color C3_Witch = new Color(0.20f, 0.10f, 0.28f);

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();
//        GuestAI g = (GuestAI)target;

//        DrawHeader(g);
//        EditorGUILayout.Space(6);

//        DrawSection(C1, () => DrawSetup());
//        EditorGUILayout.Space(3);
//        DrawSection(C1, () => DrawHide());
//        EditorGUILayout.Space(3);
//        DrawSection(C1, () => DrawPool(g));
//        EditorGUILayout.Space(3);
//        DrawSubclass(g);
//        EditorGUILayout.Space(3);
//        DrawSection(C_Other, () => DrawOther());

//        if (Application.isPlaying)
//        {
//            EditorGUILayout.Space(3);
//            DrawSection(CR, () => DrawRuntime(g));
//            Repaint();
//        }

//        serializedObject.ApplyModifiedProperties();
//    }

//    // ── Colored box ──────────────────────────────

//    void DrawSection(Color bg, System.Action draw)
//    {
//        Color prev = GUI.backgroundColor;
//        GUI.backgroundColor = bg;
//        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
//        {
//            GUI.backgroundColor = prev;
//            draw();
//        }
//        GUI.backgroundColor = prev;
//    }

//    // ── Header ───────────────────────────────────

//    void DrawHeader(GuestAI g)
//    {
//        string n = g.GetType().Name.Replace("Guest", "");
//        string tag = n switch
//        {
//            "Ghost" => "👻  Ghost       |  2 req  |  decay -0.5",
//            "Vampire" => "🧛  Vampire     |  3 req  |  decay -1.0",
//            "Witch" => "🧙  Witch+Cat   |  2+2 req|  decay -0.5",
//            "Werewolf" => "🐺  Werewolf    |  3 req  |  decay -1.0  |  Anger Stack",
//            "Franken" => "⚡  Franken     |  2 req  |  decay -0.5  |  Sleepwalk",
//            "Mummy" => "🪦  Mummy       |  3 req  |  decay -1.0  |  Cloth forced",
//            "Reaper" => "💀  Reaper      |  3 req  |  decay -0.5  |  Large Room",
//            _ => "🏨  GuestAI",
//        };
//        EditorGUILayout.HelpBox(tag, MessageType.None);
//    }

//    // ── Setup ────────────────────────────────────

//    void DrawSetup()
//    {
//        _foldSetup = EditorGUILayout.Foldout(_foldSetup, "Setup", true, EditorStyles.foldoutHeader);
//        if (!_foldSetup) return;
//        EditorGUI.indentLevel++;
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("serviceCount"), new GUIContent("Service Count"));
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("heart"), new GUIContent("Heart (max)"));
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("decaysHit"), new GUIContent("Decay Per Tick"));
//        EditorGUI.indentLevel--;
//    }

//    // ── Hide On Check-in ─────────────────────────

//    void DrawHide()
//    {
//        _foldHide = EditorGUILayout.Foldout(_foldHide, "Hide On Check-in", true, EditorStyles.foldoutHeader);
//        if (!_foldHide) return;
//        EditorGUI.indentLevel++;
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("hideScaleDuration"), new GUIContent("Duration (s)"));
//        EditorGUI.indentLevel--;
//    }

//    // ── Service Pool ─────────────────────────────

//    void DrawPool(GuestAI g)
//    {
//        _foldPool = EditorGUILayout.Foldout(_foldPool, "Service Pool", true, EditorStyles.foldoutHeader);
//        if (!_foldPool) return;
//        EditorGUI.indentLevel++;
//        bool hasLuggage = g.serviceRequest_All.Exists(
//            i => i != null && i.requiredForService == ServiceRequestType.DeliveryLuggage);
//        if (!hasLuggage && g.serviceRequest_All.Count > 0)
//            EditorGUILayout.HelpBox("ไม่พบ Luggage ใน Pool", MessageType.Warning);
//        EditorGUILayout.PropertyField(serializedObject.FindProperty("serviceRequest_All"), new GUIContent("Items"), true);
//        EditorGUI.indentLevel--;
//    }

//    // ── Subclass ─────────────────────────────────

//    void DrawSubclass(GuestAI g)
//    {
//        switch (g)
//        {
//            case WerewolfGuest:
//                DrawSection(C2, () =>
//                {
//                    _foldSub = EditorGUILayout.Foldout(_foldSub, "Anger Stack", true, EditorStyles.foldoutHeader);
//                    if (!_foldSub) return;
//                    EditorGUILayout.Space(2);
//                    DrawSection(C3_Wolf, () =>
//                    {
//                        EditorGUI.indentLevel++;
//                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAngerBars"), new GUIContent("Bars"));
//                        EditorGUILayout.PropertyField(serializedObject.FindProperty("barDrainInterval"), new GUIContent("Drain (s)"));
//                        EditorGUI.indentLevel--;
//                    });
//                });
//                break;

//            case FrankenGuest:
//                DrawSection(C2, () =>
//                {
//                    _foldSub = EditorGUILayout.Foldout(_foldSub, "Sleepwalk", true, EditorStyles.foldoutHeader);
//                    if (!_foldSub) return;
//                    EditorGUILayout.Space(2);
//                    DrawSection(C3_Franken, () =>
//                    {
//                        EditorGUI.indentLevel++;
//                        EditorGUILayout.PropertyField(serializedObject.FindProperty("sleepwalkTarget"), new GUIContent("Walk Target"));
//                        EditorGUILayout.PropertyField(serializedObject.FindProperty("heartLossPerSecond"), new GUIContent("Heart Loss /s"));
//                        EditorGUI.indentLevel--;
//                    });
//                });
//                break;

//            case MummyGuest:
//                DrawSection(C2, () =>
//                {
//                    _foldSub = EditorGUILayout.Foldout(_foldSub, "Cloth Event", true, EditorStyles.foldoutHeader);
//                    if (!_foldSub) return;
//                    EditorGUILayout.Space(2);
//                    DrawSection(C3_Mummy, () =>
//                    {
//                        EditorGUI.indentLevel++;
//                        EditorGUILayout.PropertyField(serializedObject.FindProperty("clothItem"), new GUIContent("Cloth Item"));
//                        EditorGUI.indentLevel--;
//                    });
//                });
//                break;

//            case WitchGuest:
//                DrawSection(C2, () =>
//                {
//                    _foldSub = EditorGUILayout.Foldout(_foldSub, "Cat", true, EditorStyles.foldoutHeader);
//                    if (!_foldSub) return;
//                    EditorGUILayout.Space(2);
//                    DrawSection(C3_Witch, () =>
//                    {
//                        EditorGUI.indentLevel++;
//                        EditorGUILayout.PropertyField(serializedObject.FindProperty("catServicePool"), new GUIContent("Cat Pool"), true);
//                        EditorGUILayout.PropertyField(serializedObject.FindProperty("catServiceCount"), new GUIContent("Cat Count"));
//                        EditorGUILayout.PropertyField(serializedObject.FindProperty("catServiceButton"), new GUIContent("Cat Button"));
//                        EditorGUI.indentLevel--;
//                    });
//                });
//                break;
//        }
//    }

//    // ── Other — field ที่เหลือ / เพิ่มใหม่ในอนาคต ──

//    // field ที่ draw ไปแล้วใน section อื่น — blacklist ไม่ให้ซ้ำ
//    static readonly string[] _drawnFields = new[]
//    {
//        "m_Script",
//        // Setup
//        "serviceCount", "heart", "decaysHit",
//        // Hide On Check-in
//        "hideOnCheckIn", "hideScaleDuration",
//        // Service Pool
//        "serviceRequest_All",
//        // Subclass — Werewolf
//        "maxAngerBars", "barDrainInterval",
//        // Subclass — Franken
//        "sleepwalkTarget", "heartLossPerSecond",
//        // Subclass — Mummy
//        "clothItem",
//        // Subclass — Witch
//        "catServicePool", "catServiceCount", "catServiceButton",
//        // Runtime (read-only, ไม่ต้อง draw ซ้ำ)
//        "guestPhase", "isDecaying", "isExit", "currentService",
//        "servicePoint", "rentNet",
//    };

//    void DrawOther()
//    {
//        _foldOther = EditorGUILayout.Foldout(_foldOther, "Other", true, EditorStyles.foldoutHeader);
//        if (!_foldOther) return;
//        EditorGUI.indentLevel++;
//        DrawPropertiesExcluding(serializedObject, _drawnFields);
//        EditorGUI.indentLevel--;
//    }

//    // ── Runtime ──────────────────────────────────

//    void DrawRuntime(GuestAI g)
//    {
//        EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);
//        EditorGUILayout.LabelField("Phase", g.guestPhase.ToString());

//        float ratio = Mathf.Clamp01(g.heart / 5f);
//        Color hc = ratio > 0.6f ? new Color(0.2f, 0.8f, 0.3f)
//                 : ratio > 0.3f ? new Color(0.9f, 0.7f, 0.1f)
//                 : new Color(0.9f, 0.2f, 0.2f);
//        DrawBar($"Heart  {g.heart:F1} / 5", ratio, hc);

//        string svc = g.currentService != null ? g.currentService.itemName : "—";
//        EditorGUILayout.LabelField("Service", svc);

//        if (g is WerewolfGuest wolf)
//            DrawBar($"Anger  {wolf.CurrentAngerBars} / {wolf.maxAngerBars}",
//                (float)wolf.CurrentAngerBars / wolf.maxAngerBars,
//                new Color(0.9f, 0.4f, 0.1f));

//        if (g is FrankenGuest fr && fr.IsSleepwalking)
//            EditorGUILayout.HelpBox("💤 Sleepwalking", MessageType.Warning);

//        if (g is WitchGuest wt && wt.catCurrentService != null)
//            EditorGUILayout.LabelField("Cat Service", wt.catCurrentService.itemName);
//    }

//    // ── Bar ──────────────────────────────────────

//    void DrawBar(string label, float ratio, Color col)
//    {
//        Rect r = GUILayoutUtility.GetRect(0, 16, GUILayout.ExpandWidth(true));
//        r.x += 16; r.width -= 20;
//        EditorGUI.DrawRect(r, new Color(0.1f, 0.1f, 0.1f));
//        Rect fill = r; fill.width = r.width * ratio;
//        EditorGUI.DrawRect(fill, col);
//        GUI.Label(r, "  " + label, EditorStyles.miniLabel);
//    }
//}
//#endif