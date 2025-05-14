namespace Treep.Utils {
    using UnityEngine;

    public static class Collider2DGizmoUtility {
        public static void GizmosDraw2DCollider(Collider2D collider) {
            if (collider == null || !collider.enabled) return;

            Gizmos.color = Color.green;

            if (collider is CircleCollider2D circle) {
                Collider2DGizmoUtility.DrawCircle(circle.transform.TransformPoint(circle.offset),
                    circle.radius * Collider2DGizmoUtility.GetMaxScale(circle.transform), 32);
            }
            else if (collider is BoxCollider2D box) {
                var matrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(box.transform.TransformPoint(box.offset), box.transform.rotation,
                    Vector3.Scale(box.size, box.transform.lossyScale));
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
                Gizmos.matrix = matrix;
            }
            else if (collider is CapsuleCollider2D capsule) {
                Collider2DGizmoUtility.DrawCapsule2D(capsule);
            }
            else if (collider is PolygonCollider2D poly) {
                for (var i = 0; i < poly.pathCount; i++) {
                    var path = poly.GetPath(i);
                    Collider2DGizmoUtility.DrawPath(poly.transform, poly.offset, path, true);
                }
            }
            else if (collider is EdgeCollider2D edge) {
                Collider2DGizmoUtility.DrawPath(edge.transform, edge.offset, edge.points, false);
            }
        }

        private static float GetMaxScale(Transform t) {
            var s = t.lossyScale;
            return Mathf.Max(s.x, s.y);
        }

        private static void DrawCircle(Vector3 center, float radius, int segments) {
            var angleStep = 360f / segments;
            var prevPoint = center + Quaternion.Euler(0, 0, 0) * Vector3.right * radius;
            for (var i = 1; i <= segments; i++) {
                var angle = i * angleStep;
                var newPoint = center + Quaternion.Euler(0, 0, angle) * Vector3.right * radius;
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        private static void DrawCapsule2D(CapsuleCollider2D capsule) {
            var tr = capsule.transform;
            var offset = capsule.offset;
            var scale = tr.lossyScale;
            var pos = tr.TransformPoint(offset);
            var dir = capsule.direction;
            var size = capsule.size;

            var radius = dir == CapsuleDirection2D.Vertical
                ? 0.5f * size.x * scale.x
                : 0.5f * size.y * scale.y;

            var height = dir == CapsuleDirection2D.Vertical
                ? size.y * scale.y
                : size.x * scale.x;

            var segments = 16;

            var up = dir == CapsuleDirection2D.Vertical ? Vector3.up : Vector3.right;
            var right = dir == CapsuleDirection2D.Vertical ? Vector3.right : Vector3.up;

            var topCenter = pos + up * (height / 2f - radius);
            var bottomCenter = pos - up * (height / 2f - radius);

            Collider2DGizmoUtility.DrawSemiCircle(topCenter, radius, segments, 0f);
            Collider2DGizmoUtility.DrawSemiCircle(bottomCenter, radius, segments, 180f);

            Gizmos.DrawLine(topCenter + right * radius, bottomCenter + right * radius);
            Gizmos.DrawLine(topCenter - right * radius, bottomCenter - right * radius);
        }

        private static void DrawSemiCircle(Vector3 center, float radius, int segments, float startAngle) {
            var angleStep = 180f / segments;
            var prevPoint = center + Quaternion.Euler(0, 0, startAngle) * Vector3.right * radius;
            for (var i = 1; i <= segments; i++) {
                var angle = startAngle + i * angleStep;
                var newPoint = center + Quaternion.Euler(0, 0, angle) * Vector3.right * radius;
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        private static void DrawPath(Transform tr, Vector2 offset, Vector2[] points, bool loop) {
            if (points.Length < 2) return;
            for (var i = 0; i < points.Length - 1; i++) {
                Gizmos.DrawLine(
                    tr.TransformPoint(offset + points[i]),
                    tr.TransformPoint(offset + points[i + 1])
                );
            }

            if (loop) {
                Gizmos.DrawLine(
                    tr.TransformPoint(offset + points[^1]),
                    tr.TransformPoint(offset + points[0])
                );
            }
        }
    }
}
