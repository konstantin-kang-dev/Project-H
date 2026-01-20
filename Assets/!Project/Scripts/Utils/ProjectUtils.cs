using System.Collections;
using UnityEngine;

public static class ProjectUtils
{
    public static Vector3 RandomPositionInCone(Vector3 direction, float distance, float coneAngle)
    {
        direction.Normalize();

        float angle = Random.Range(0f, coneAngle);
        float rotation = Random.Range(0f, 360f);

        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
        if (perpendicular.sqrMagnitude < 0.001f)
            perpendicular = Vector3.Cross(direction, Vector3.right);

        perpendicular.Normalize();

        perpendicular = Quaternion.AngleAxis(rotation, direction) * perpendicular;

        Vector3 coneDirection = Quaternion.AngleAxis(angle, perpendicular) * direction;

        return coneDirection * distance;
    }

    public static Vector3 RandomPositionInRectangle(Vector3 origin, Vector3 direction, float distance, float width, float height)
    {
        direction.Normalize();

        // Создаём right вектор
        Vector3 right = Vector3.Cross(direction, Vector3.up);
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(direction, Vector3.right);
        right.Normalize();

        // Создаём up вектор
        Vector3 up = Vector3.Cross(right, direction);

        // Случайные смещения
        float randomX = Random.Range(-width * 0.5f, width * 0.5f);
        float randomY = Random.Range(-height * 0.5f, height * 0.5f);

        // Позиция = origin + центр прямоугольника + смещения
        return origin + direction * distance + right * randomX + up * randomY;
    }
}