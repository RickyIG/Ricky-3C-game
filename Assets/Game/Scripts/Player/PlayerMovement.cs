using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using UnityEditor;

public class PlayerMovement : MonoBehaviour
{
    public Action OnGameOver;
    public Action OnYouWin;
    [SerializeField]
    private float _walkSpeed;
    [SerializeField]
    private float _sprintSpeed;
    [SerializeField]
    private float _crouchSpeed;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private float _walkSprintTransition;
    [SerializeField]
    private InputManager _input;
    [SerializeField]
    private float _rotationSmoothTime = 0.1f;
    [SerializeField]
    private Transform _groundDetector;
    [SerializeField]
    private float _detectorRadius;
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private LayerMask _gameOverLayer;
    [SerializeField]
    private LayerMask _youWinLayer;
    [SerializeField]
    private Vector3 _upperStepOffset;
    [SerializeField]
    private float _stepCheckerDistance;
    [SerializeField]
    private float _stepForce;
    [SerializeField]
    private Transform _climbDetector;
    [SerializeField]
    private float _climbCheckDistance;
    [SerializeField]
    private LayerMask _climbableLayer;
    [SerializeField]
    private Vector3 _climbOffset;
    [SerializeField]
    private float _climbSpeed;
    [SerializeField]
    private float _climbSprintSpeed;
    [SerializeField]
    private float _climbSprintTransition;
    [SerializeField]
    private Transform _cameraTransform;
    [SerializeField]
    private CameraManager _cameraManager;
    [SerializeField]
    private float _glideSpeed;
    [SerializeField]
    private float _airDrag;
    [SerializeField]
    private Vector3 _glideRotationSpeed;
    [SerializeField]
    private float _minGlideRotationX;
    [SerializeField]
    private float _maxGlideRotationX;
    [SerializeField]
    private float _resetComboInterval;
    [SerializeField]
    private Transform _hitDetector;
    [SerializeField]
    private float _hitDetectorRadius;
    [SerializeField]
    private LayerMask _hitLayer;
    [SerializeField]
    private LayerMask _hitRespawnableLayer;
    [SerializeField]
    private Transform _aboveDetector;
    [SerializeField] 
    private float _respawnDelay = 5.0f;
    [SerializeField]
    private GameObject _respawnPrefab;
    [SerializeField]
    private PlayerAudioManager _playerAudioManager;

    private Rigidbody _rigidbody;
    private float _speed;
    private float _rotationSmoothVelocity;
    private bool _isGrounded;
    private PlayerStance _playerStance;
    private Animator _animator;
    private CapsuleCollider _collider;
    private bool _isPunching;
    private int _combo = 0;
    private Coroutine _resetCombo;

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        _speed = _walkSpeed;
        _playerStance = PlayerStance.Stand;
        HideAndLockCursor();
    }

    private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
        _input.OnJumpInput += Jump;
        _input.OnClimbInput += StartClimb;
        _input.OnCancelClimb += CancelClimb;
        _input.OnCrouchInput += Crouch;
        _input.OnGlideInput += StartGlide;
        _input.OnCancelGlide += CancelGlide;
        _input.OnPunchInput += Punch;
        _cameraManager.OnChangePerspective += ChangePerspective;
    }

    private void OnDestroy() 
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
        _input.OnCrouchInput -= Crouch;
        _input.OnGlideInput -= StartGlide;
        _input.OnCancelGlide -= CancelGlide;
        _input.OnPunchInput -= Punch;
        _cameraManager.OnChangePerspective -= ChangePerspective;
    }

    private void Update() 
    {
        CheckIsGrounded();
        CheckStep();
        CheckWall();
        Glide();
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
        bool isPlayerCrouch = _playerStance == PlayerStance.Crouch;
        bool isPlayerGliding = _playerStance == PlayerStance.Glide;

        if ((isPlayerStanding || isPlayerCrouch) && !_isPunching)
        {
            switch (_cameraManager.CameraState)
            {
                case CameraState.ThirdPerson:
                    if (axisDirection.magnitude >= 0.1)
                    {
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
                    }
                    break;

                case CameraState.FirstPerson:
                    transform.rotation = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
                    Vector3 verticalDirection = axisDirection.y * transform.forward;
                    Vector3 horizontalDirection = axisDirection.x * transform.right;
                    movementDirection = verticalDirection + horizontalDirection;
                    _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
                    break;
                
                default:
                    break;
            }
            Vector3 velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            _animator.SetFloat("Velocity", velocity.magnitude * axisDirection.magnitude);
            _animator.SetFloat("VelocityZ", velocity.magnitude * axisDirection.y);
            _animator.SetFloat("VelocityX", velocity.magnitude * axisDirection.x);
        }
        else if (isPlayerClimbing)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 vertical = axisDirection.y * transform.up;
            movementDirection = horizontal + vertical;
            // _rigidbody.AddForce(movementDirection * _speed * Time.deltaTime);
            _rigidbody.AddForce(movementDirection * Time.deltaTime * _climbSpeed);
            // Vector3 velocity = new Vector3(_rigidbody.velocity.x, _rigidbody.velocity.y, 0);

            Vector3 localVelocity = transform.InverseTransformDirection(_rigidbody.velocity);
            _animator.SetFloat("ClimbVelocityY", localVelocity.magnitude * axisDirection.y);
            _animator.SetFloat("ClimbVelocityX", localVelocity.magnitude * axisDirection.x);   
        }
        else if (isPlayerGliding)
        {
            Vector3 rotationDegree = transform.rotation.eulerAngles;
            rotationDegree.x += _glideRotationSpeed.x * axisDirection.y * Time.deltaTime;
            rotationDegree.x = Mathf.Clamp(rotationDegree.x, _minGlideRotationX, _maxGlideRotationX);
            rotationDegree.z += _glideRotationSpeed.z * axisDirection.x * Time.deltaTime;
            rotationDegree.y += _glideRotationSpeed.y * axisDirection.x * Time.deltaTime;
            transform.rotation = Quaternion.Euler(rotationDegree);
        }
    } 

    private void Sprint(bool isSprint)
    {
        if (isSprint)
        {
            if (_playerStance == PlayerStance.Climb && _speed < _climbSprintSpeed)
            {
                _speed += _climbSprintTransition * Time.deltaTime;
            }
            else
            if (_playerStance != PlayerStance.Climb && _speed < _sprintSpeed)
            {
                _speed += _walkSprintTransition * Time.deltaTime;
            }
            
        }
        else
        {
            if (_playerStance == PlayerStance.Climb && _speed > _climbSpeed)
            {
                _speed -= _climbSprintTransition * Time.deltaTime;
            }
            else
            if (_playerStance != PlayerStance.Climb && _speed > _walkSpeed)
            {
                _speed -= _walkSprintTransition * Time.deltaTime;
            }
        }
    }

    private void Jump()
    {
        bool canJump = _animator.GetCurrentAnimatorStateInfo(0).IsTag("CanJump");
        if (_isGrounded && !_isPunching && canJump) 
        {
            Vector3 jumpDirection = Vector3.up;
            _rigidbody.AddForce(jumpDirection * _jumpForce, ForceMode.Impulse);
            _animator.SetTrigger("Jump");
        }    
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);
        if (_isGrounded)
        {
            CancelGlide();
        }
        bool _isGameOver = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _gameOverLayer);
        if (_isGameOver)
        {
            OnGameOver();
        }
        bool _isYouWin = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _youWinLayer);
        if (_isYouWin)
        {
            OnYouWin();
        }
    }

    private void CheckStep()
    {
        bool isHitLowerStep = Physics.Raycast(_groundDetector.position, transform.forward, _stepCheckerDistance);
        bool isHitUpperStep = Physics.Raycast(_groundDetector.position + _upperStepOffset, transform.forward,
                                                _stepCheckerDistance);
        if (isHitLowerStep && !isHitUpperStep)
        {
            _rigidbody.AddForce(0, _stepForce * Time.deltaTime, 0);
        }
    }

    private void StartClimb()
    {
        bool isInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position,
                                                        transform.forward,
                                                        out RaycastHit hit,
                                                        _climbCheckDistance,
                                                        _climbableLayer);
        bool isNotClimbing = _playerStance != PlayerStance.Climb;
        if (isInFrontOfClimbingWall && _isGrounded && isNotClimbing)
        {
            _collider.center = Vector3.up * 1.3f;
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;
            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;
            _speed = _climbSpeed;
            // Mendapatkan titik terdekat antara Climbable dengan Player
            Vector3 closestPointFromClimbable = hit.collider.bounds.ClosestPoint(transform.position);
            // Menentukan arah Player dengan selisih antara titik terdekat dengan pemain
            Vector3 hitForward = closestPointFromClimbable - transform.position;
            // Membuat arah sumbu y menjadi 0, karena hanya perlu sumbu x dan z
            hitForward.y = 0;
            // Me-rotasi pemain berdasarkan arah pemain terhadap titik terdekat dari Climbable
            transform.rotation = Quaternion.LookRotation(hitForward);

            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(70);

            _animator.SetBool("IsClimbing", true);
        }
        StartCoroutine(CheckClimbingRoutine());
    }

    private IEnumerator CheckClimbingRoutine()
    {
        yield return new WaitForSeconds(2.0f);

        while (_playerStance == PlayerStance.Climb)
        {
            if (!CheckWall())
            {
                CancelClimb();
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private bool CheckWall()
    {
        bool isInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position,
                                                        transform.forward,
                                                        out RaycastHit hit,
                                                        _climbCheckDistance,
                                                        _climbableLayer);
        return isInFrontOfClimbingWall;
    }

    private void CancelClimb()
    {
        if (_playerStance == PlayerStance.Climb)
        {
            _collider.center = Vector3.up * 0.9f;
            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward;
            _speed = _walkSpeed;

            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(40);

            _animator.SetBool("IsClimbing", false);
        }
    }    

    private void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_groundDetector.position, _detectorRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(_groundDetector.position, transform.forward * _stepCheckerDistance);
        Gizmos.DrawRay(_groundDetector.TransformPoint(_upperStepOffset), transform.forward * _stepCheckerDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(_climbDetector.position, transform.forward * _climbCheckDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(_aboveDetector.position, transform.up * 1);
    }

    private void ChangePerspective()
    {
        _animator.SetTrigger("ChangePerspective");
    }

    private void Crouch()
    {
        if (_playerStance == PlayerStance.Stand)
        {
            _playerStance = PlayerStance.Crouch;
            _animator.SetBool("IsCrouch", true);
            _collider.height = 1.3f;
            _collider.center = Vector3.up * 0.66f;
            _speed = _crouchSpeed;
        }
        else if (_playerStance == PlayerStance.Crouch)
        {
            bool isForceToCrouch = Physics.Raycast(_aboveDetector.position, transform.up, 1);
            if (!isForceToCrouch)
            {
                _playerStance = PlayerStance.Stand;
                _animator.SetBool("IsCrouch", false);
                _collider.height = 1.8f;
                _collider.center = Vector3.up * 0.9f;
                _speed = _walkSpeed;
            }
        }
    }

    private void Glide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            Vector3 playerRotation = transform.rotation.eulerAngles;
            float lift = playerRotation.x;
            Vector3 upForce = transform.up * (lift + _airDrag);
            Vector3 forwardForce = transform.forward * _glideSpeed;
            Vector3 totalForce = upForce + forwardForce;
            _rigidbody.AddForce(totalForce * Time.deltaTime);
        }   
    }

    private void StartGlide()
    {
        if (_playerStance != PlayerStance.Glide && !_isGrounded)
        {
            _playerStance = PlayerStance.Glide;
            _animator.SetBool("IsGliding", true);
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _playerAudioManager.PlayGlideSfx();
        }
    }

    private void CancelGlide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsGliding", false);
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _playerAudioManager.StopGlideSfx();
        }
    }

    private void Punch()
    {
        if (!_isPunching && _playerStance == PlayerStance.Stand)
        {
            _isPunching = true;
            if (_combo < 3)
            {
                _combo += 1;
            }
            else
            {
                _combo = 1;
            }
            _animator.SetInteger("Combo", _combo);
            _animator.SetTrigger("Punch");
        }
    }

    private void EndPunch()
    {
        _isPunching = false;
        if (_resetCombo != null)
        {
            StopCoroutine(_resetCombo);
        }
        _resetCombo = StartCoroutine(ResetCombo());
    }

    private IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(_resetComboInterval);
        _combo = 0;
    }

    private void Hit()
    {
        Collider[] hitObjects = Physics.OverlapSphere(_hitDetector.position, _hitDetectorRadius, _hitLayer);
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].gameObject != null)
            {
                _playerAudioManager.PlayDestroyableSFX();
                ScoreManager.instance.AddScore();
                Destroy(hitObjects[i].gameObject);
            }
        }

        Collider[] hitRespawnableObjects = Physics.OverlapSphere(_hitDetector.position, _hitDetectorRadius, _hitRespawnableLayer);
        if (hitRespawnableObjects.Length > 0) 
        {
            for (int i = 0; i < hitRespawnableObjects.Length; i++)
            {
                if (hitRespawnableObjects[i] != null)
                {
                    _playerAudioManager.PlayRespawnableDestroyableSFX();
                    Destroy(hitRespawnableObjects[i].gameObject);
                    Debug.Log("Object destroyed!");
                    StartCoroutine(RespawnObject(hitRespawnableObjects[i].transform.position, hitRespawnableObjects[i].transform.rotation, _respawnDelay));
                }
            }
        }
    }

    private IEnumerator RespawnObject(Vector3 position, Quaternion rotation, float delay)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(_respawnPrefab, position, rotation);
    }

}
