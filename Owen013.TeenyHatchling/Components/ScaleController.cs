using UnityEngine;

namespace SmolHatchling.Components;

public class ScaleController : MonoBehaviour
{
    public static ScaleController Instance { get; private set; }
    public Vector3 TargetScale { get; private set; }
    public float AnimSpeed { get; private set; }

    private PlayerBody _playerBody;
    private PlayerCameraController _cameraController;
    private PlayerAnimController _animController;
    private PlayerScreamingController _npcPlayer;
    private PlayerCloneController _cloneController;
    private EyeMirrorController _mirrorController;
    private ShipCockpitController _cockpitController;
    private ShipLogController _logController;
    private OWAudioSource[] _audioSources;
    private CapsuleCollider _playerCollider;
    private CapsuleCollider _detectorCollider;
    private CapsuleShape _detectorShape;
    private GameObject _playerThruster;
    private GameObject _playerMarshmallowStick;
    private Vector3 _currentScale;
    private Vector3 _scaleVelocity;
    private Vector3 _colliderScale;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _playerBody = GetComponent<PlayerBody>();
        _cameraController = Locator.GetPlayerCameraController();
        _animController = GetComponentInChildren<PlayerAnimController>();
        _npcPlayer = FindObjectOfType<PlayerScreamingController>();
        _cloneController = FindObjectOfType<PlayerCloneController>();
        _mirrorController = FindObjectOfType<EyeMirrorController>();
        _cockpitController = FindObjectOfType<ShipCockpitController>();
        _logController = FindObjectOfType<ShipLogController>();
        _playerCollider = GetComponent<CapsuleCollider>();
        _detectorCollider = transform.Find("PlayerDetector").GetComponent<CapsuleCollider>();
        _detectorShape = GetComponentInChildren<CapsuleShape>();
        _playerThruster = transform.Find("PlayerVFX").gameObject;
        _playerMarshmallowStick = transform.Find("RoastingSystem").transform.Find("Stick_Root").gameObject;

        PlayerAudioController audioController = GetComponentInChildren<PlayerAudioController>();
        PlayerBreathingAudio breathingAudio = GetComponentInChildren<PlayerBreathingAudio>();
        Instance._audioSources = new OWAudioSource[]
        {
            audioController._oneShotSleepingAtCampfireSource,
            audioController._oneShotSource,
            breathingAudio._asphyxiationSource,
            breathingAudio._breathingLowOxygenSource,
            breathingAudio._breathingSource,
            breathingAudio._drowningSource
        };

        Config.OnConfigure += UpdateTargetScale;
        Instance.UpdateTargetScale();
        Instance.SnapSize();
    }

    private void OnDestroy()
    {
        Config.OnConfigure -= UpdateTargetScale;
    }

    private void Update()
    {
        if (_currentScale != TargetScale)
        {
            _currentScale = Vector3.SmoothDamp(_currentScale, TargetScale, ref _scaleVelocity, 0.25f);
            UpdatePlayerScale();
        }
    }

    private void UpdateTargetScale()
    {
        TargetScale = new Vector3(Config.PlayerRadius, Config.PlayerHeight, Config.PlayerRadius);
    }

    private void SnapSize()
    {
        _currentScale = TargetScale;
        UpdatePlayerScale();
    }

    private void UpdatePlayerScale()
    {
        // Change playermodel size and animation speed
        _animController.transform.localScale = _currentScale / 10;
        _animController.transform.localPosition = new Vector3(0, -1.03f, -0.2f * _currentScale.z);
        _playerThruster.transform.localScale = _currentScale;
        _playerThruster.transform.localPosition = new Vector3(0, -1 + _currentScale.y, 0);

        // Move camera and marshmallow stick root down to match new player height
        _cameraController._origLocalPosition = new Vector3(0f, -1 + 1.8496f * _currentScale.y, 0.15f * _currentScale.z);
        _cameraController.transform.localPosition = _cameraController._origLocalPosition;
        _playerMarshmallowStick.transform.localPosition = new Vector3(0.25f, -1.8496f + 1.8496f * _currentScale.y, 0.08f - 0.15f + 0.15f * _currentScale.z);

        // Change collider height and radius, and move colliders down so they touch the ground
        if (Config.ColliderMode == "Height & Radius") _colliderScale = TargetScale;
        else if (Config.ColliderMode == "Height Only") _colliderScale = new Vector3(1, TargetScale.y, 1);
        else _colliderScale = Vector3.one;
        float height = 2 * _colliderScale.y;
        float radius = Mathf.Min(_colliderScale.z / 2, height / 2);
        Vector3 center = new Vector3(0, _colliderScale.y - 1, 0);
        _playerCollider.height = _detectorCollider.height = _detectorShape.height = height;
        _playerCollider.radius = _detectorCollider.radius = _detectorShape.radius = radius;
        _playerCollider.center = _detectorCollider.center = _detectorShape.center = _playerBody._centerOfMass = _playerCollider.center = _detectorCollider.center = _playerBody._activeRigidbody.centerOfMass = center;

        // Change pitch if enabled
        float pitch = Config.IsPitchChangeEnabled ? 0.5f * Mathf.Pow(_currentScale.y, -1) + 0.5f : 1f;
        foreach (OWAudioSource audio in _audioSources) audio.pitch = pitch;

        // Change size of Ash Twin Project player clone if it exists
        if (_npcPlayer != null)
        {
            _npcPlayer.GetComponentInChildren<Animator>().gameObject.transform.localScale = _currentScale / 10;
            _npcPlayer.transform.Find("NPC_Player_FocalPoint").localPosition = new Vector3(-0.093f, 0.991f * TargetScale.y, 0.102f);
        }

        UpdateCockpitAttachPoint();
        UpdateLogAttachPoint();
        UpdateEyeCloneScale();
        UpdateMirrorCloneScale();

        AnimSpeed = Mathf.Pow(TargetScale.z, -1);
        _animController._animator.speed = AnimSpeed;
        if (_cloneController != null) _cloneController._playerVisuals.GetComponent<PlayerAnimController>()._animator.speed = AnimSpeed;
        if (_mirrorController != null) _mirrorController._mirrorPlayer.GetComponentInChildren<PlayerAnimController>()._animator.speed = AnimSpeed;
    }

    private void UpdateCockpitAttachPoint()
    {
        if (_cockpitController == null) return;
        _cockpitController._origAttachPointLocalPos = new Vector3(0, 2.1849f - 1.8496f * _currentScale.y, 4.2307f + 0.15f - 0.15f * _currentScale.z);
    }

    private void UpdateLogAttachPoint()
    {
        if (_logController == null) return;
        _logController._attachPoint._attachOffset = new Vector3(0f, 1.8496f - 1.8496f * _currentScale.y, 0.15f - 0.15f * _currentScale.z);
    }

    private void UpdateEyeCloneScale()
    {
        if (_cloneController == null) return;
        _cloneController._playerVisuals.transform.localScale = _currentScale / 10;
        _cloneController._signal._owAudioSource.pitch = _audioSources[0].pitch;
    }

    private void UpdateMirrorCloneScale()
    {
        // Update mirror player scale if it exists
        if (_mirrorController == null) return;
        _mirrorController._mirrorPlayer.transform.Find("Traveller_HEA_Player_v2 (2)").localScale = _currentScale / 10;
    }
}