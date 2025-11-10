using UnityEngine;
using UnityEngine.Events;

namespace HandSurvivor
{
    public class ARPlaneCalibration : MonoBehaviour
    {
        [Header("Plane Settings")]
        [SerializeField] private GameObject planePrefab;
        [SerializeField] private Vector2 defaultPlaneSize = new Vector2(1f, 1f);
        [SerializeField] private float planeHeight = 0.01f;

        [Header("Controls")]
        [SerializeField] private float moveSpeed = 0.5f;
        [SerializeField] private float rotateSpeed = 45f;
        [SerializeField] private float scaleSpeed = 0.1f;

        [Header("Preview")]
        [SerializeField] private bool showWorldPreview = true;
        [SerializeField] private GameObject worldPreviewPrefab;

        [Header("Events")]
        public UnityEvent<Transform> OnPlaneLocked;

        private GameObject currentPlane;
        private GameObject worldPreview;
        private bool isLocked = false;
        private Transform planeTransform;

        public bool IsLocked => isLocked;
        public Transform PlaneTransform => planeTransform;
        public Bounds PlaneBounds
        {
            get
            {
                if (planeTransform == null) return new Bounds();
                Vector3 size = new Vector3(
                    planeTransform.localScale.x * defaultPlaneSize.x,
                    planeHeight,
                    planeTransform.localScale.z * defaultPlaneSize.y
                );
                return new Bounds(planeTransform.position, size);
            }
        }

        private void Start()
        {
            SpawnPlane();
        }

        private void SpawnPlane()
        {
            if (planePrefab != null)
            {
                currentPlane = Instantiate(planePrefab);
            }
            else
            {
                currentPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                currentPlane.name = "CalibrationPlane";
            }

            planeTransform = currentPlane.transform;
            planeTransform.localScale = new Vector3(defaultPlaneSize.x / 10f, 1f, defaultPlaneSize.y / 10f);
            planeTransform.position = Camera.main.transform.position + Camera.main.transform.forward * 1f;

            MeshRenderer renderer = currentPlane.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.3f, 0.8f, 0.3f, 0.5f);
                mat.SetFloat("_Surface", 1);
                mat.SetFloat("_Blend", 0);
                mat.renderQueue = 3000;
                renderer.material = mat;
            }

            if (showWorldPreview && worldPreviewPrefab != null)
            {
                worldPreview = Instantiate(worldPreviewPrefab, planeTransform);
                worldPreview.transform.localPosition = Vector3.zero;
            }
        }

        private void Update()
        {
            if (isLocked) return;

            HandlePlaneMovement();
        }

        private void HandlePlaneMovement()
        {
            if (OVRInput.Get(OVRInput.Button.PrimaryThumbstick))
            {
                Vector2 stick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
                Vector3 forward = Camera.main.transform.forward;
                Vector3 right = Camera.main.transform.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                planeTransform.position += (forward * stick.y + right * stick.x) * moveSpeed * Time.deltaTime;
            }

            if (OVRInput.Get(OVRInput.Button.SecondaryThumbstick))
            {
                Vector2 stick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

                planeTransform.Rotate(Vector3.up, stick.x * rotateSpeed * Time.deltaTime);

                float newScaleX = planeTransform.localScale.x + stick.y * scaleSpeed * Time.deltaTime;
                float newScaleZ = planeTransform.localScale.z + stick.y * scaleSpeed * Time.deltaTime;
                planeTransform.localScale = new Vector3(
                    Mathf.Max(0.1f, newScaleX),
                    1f,
                    Mathf.Max(0.1f, newScaleZ)
                );
            }

            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            {
                float delta = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
                planeTransform.position += Vector3.up * delta * moveSpeed * Time.deltaTime;
            }
            if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger))
            {
                float delta = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger);
                planeTransform.position += Vector3.down * delta * moveSpeed * Time.deltaTime;
            }
        }

        public void LockPlane()
        {
            if (isLocked) return;

            isLocked = true;

            if (currentPlane != null)
            {
                MeshRenderer renderer = currentPlane.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = false;
                }
            }

            OnPlaneLocked?.Invoke(planeTransform);
            Debug.Log($"[ARPlaneCalibration] Plane locked at position {planeTransform.position}");
        }

        public void UnlockPlane()
        {
            isLocked = false;

            if (currentPlane != null)
            {
                MeshRenderer renderer = currentPlane.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = true;
                }
            }
        }

        public void ResetPlane()
        {
            if (currentPlane != null)
            {
                Destroy(currentPlane);
            }
            if (worldPreview != null)
            {
                Destroy(worldPreview);
            }

            isLocked = false;
            SpawnPlane();
        }

        public void AdjustAspectRatio(float widthMultiplier, float depthMultiplier)
        {
            if (isLocked) return;

            planeTransform.localScale = new Vector3(
                defaultPlaneSize.x / 10f * widthMultiplier,
                1f,
                defaultPlaneSize.y / 10f * depthMultiplier
            );
        }
    }
}
