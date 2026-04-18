using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StoryVideoPlayButtonBinder : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private StoryVideoController targetVideoController;

    private void Reset()
    {
        playButton = GetComponent<Button>();
    }

    private void Awake()
    {
        if (playButton == null)
        {
            playButton = GetComponent<Button>();
        }

        if (targetVideoController == null)
        {
            targetVideoController = FindAnyObjectByType<StoryVideoController>();
        }
    }

    private void OnEnable()
    {
        if (playButton == null) return;
        playButton.onClick.RemoveListener(OnPlayClicked);
        playButton.onClick.AddListener(OnPlayClicked);
    }

    private void OnDisable()
    {
        if (playButton == null) return;
        playButton.onClick.RemoveListener(OnPlayClicked);
    }

    private void OnPlayClicked()
    {
        if (targetVideoController == null) return;
        targetVideoController.PlayStoryVideo();
    }

    public void EditorAssignReferences(Button button, StoryVideoController controller)
    {
        playButton = button;
        targetVideoController = controller;
    }
}
