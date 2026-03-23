using UnityEngine;

namespace ClawSurvivor.Systems
{
    /// <summary>
    /// 摄像机跟随 - 平滑跟随玩家，限制边界
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("跟随设置")]
        [Tooltip("跟随平滑速度")]
        public float smoothSpeed = 5f;
        [Tooltip("摄像机偏移")]
        public Vector3 offset = new Vector3(0, 0, -10f);

        [Header("边界限制")]
        [Tooltip("是否启用边界限制")]
        public bool useBounds = false;
        [Tooltip("X轴最小值")]
        public float boundMinX = -50f;
        [Tooltip("X轴最大值")]
        public float boundMaxX = 50f;
        [Tooltip("Y轴最小值")]
        public float boundMinY = -50f;
        [Tooltip("Y轴最大值")]
        public float boundMaxY = 50f;

        private Transform target;

        private void Start()
        {
            var player = FindObjectOfType<Player.PlayerController>();
            if (player != null)
                target = player.transform;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPos = target.position + offset;
            Vector3 smoothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);

            if (useBounds)
            {
                smoothedPos.x = Mathf.Clamp(smoothedPos.x, boundMinX, boundMaxX);
                smoothedPos.y = Mathf.Clamp(smoothedPos.y, boundMinY, boundMaxY);
            }

            transform.position = smoothedPos;
        }
    }
}
