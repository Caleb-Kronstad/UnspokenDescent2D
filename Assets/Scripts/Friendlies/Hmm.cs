using UnityEngine;

public class Hmm : MonoBehaviour
{
    [SerializeField] private float interact_range = 3.0f;
    [SerializeField] private AudioSource audio_source;
    [SerializeField] private GameObject prompt;

    private PlayerControls controls;
    private Transform player;
    private bool played = false;

    void Awake()
    {
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        GameObject player_obj = GameObject.FindGameObjectWithTag("Player");
        if (player_obj != null)
            player = player_obj.transform;

        if (prompt != null)
            prompt.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        bool in_range = Vector2.Distance(transform.position, player.position) <= interact_range;

        if (prompt != null)
            prompt.SetActive(in_range && !played);

        if (in_range && !played && controls.Land.Interact.triggered)
        {
            played = true;
            prompt.SetActive(false);
            audio_source.Play();
        }
    }
}
