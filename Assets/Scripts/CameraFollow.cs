using UnityEngine;

namespace GameJam
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("跟隨設定")]
        public Transform target;
        public Vector3 offset = new Vector3(0, 0, -10f);
        public float followSpeed = 2f;
        public float rotationSpeed = 2f;

        [Header("平滑設定")]
        public bool useSmoothing = true;
        public AnimationCurve smoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("邊界限制")]
        public bool useBounds = false;
        public Vector2 minBounds = new Vector2(-10, -10);
        public Vector2 maxBounds = new Vector2(10, 10);

        [Header("預測移動")]
        public bool predictMovement = false;
        public float predictionDistance = 2f;

        private Vector3 velocity = Vector3.zero;
        private Camera cam;

        void Start()
        {
            cam = GetComponent<Camera>();

            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 targetPosition = GetTargetPosition();

            if (useSmoothing)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }

            if (useBounds)
            {
                Vector3 pos = transform.position;
                pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
                pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
                transform.position = pos;
            }
        }

        Vector3 GetTargetPosition()
        {
            Vector3 targetPos = target.position + offset;

            if (predictMovement)
            {
                Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
                if (targetRb != null)
                {
                    Vector3 prediction = targetRb.linearVelocity.normalized * predictionDistance;
                    targetPos += prediction;
                }
            }

            return targetPos;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, transform.position.z);
                Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
                Gizmos.DrawWireCube(center, size);
            }

            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, target.position + offset);

                if (predictMovement)
                {
                    Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
                    if (targetRb != null)
                    {
                        Gizmos.color = Color.blue;
                        Vector3 prediction = targetRb.linearVelocity.normalized * predictionDistance;
                        Gizmos.DrawLine(target.position, target.position + prediction);
                    }
                }
            }
        }
    }
}