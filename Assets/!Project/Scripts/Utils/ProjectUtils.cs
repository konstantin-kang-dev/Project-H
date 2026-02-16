using System;
using System.Collections;
using UnityEngine;

using Random = UnityEngine.Random;
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

        Vector3 right = Vector3.Cross(direction, Vector3.up);
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(direction, Vector3.right);
        right.Normalize();

        Vector3 up = Vector3.Cross(right, direction);

        float randomX = Random.Range(-width * 0.5f, width * 0.5f);
        float randomY = Random.Range(-height * 0.5f, height * 0.5f);

        return origin + direction * distance + right * randomX + up * randomY;
    }
    public static int GetRandomExcluding(int a, int b, int exc)
    {
        int result = Random.Range(a, b - 1);
        return result >= exc ? result + 1 : result;
    }

    public static int GetNextIndex(int startIndex, int length, bool goForward)
    {
        int index = startIndex;
        if (goForward)
        {
            if (index >= length)
            {
                index = 0;
            }
            else
            {
                index++;
            }
        }
        else
        {
            if (index == 0)
            {
                index = length;
            }
            else
            {
                index--;
            }
        }

        return index;
    }

    public static int GetPercentOfValue(int value, int percent)
    {
        return value * percent / 100;
    }

    public static Quaternion GetFlatYLookRotation(Vector3 posA, Vector3 posB)
    {
        posA.y = 0;
        posB.y = 0;
        return Quaternion.LookRotation(posB - posA);
    }
    public static Vector3 GetPositionAtDistance(Vector3 targetPos, Vector3 origin, float distance)
    {
        Vector3 direction = (targetPos - origin).normalized;
        return origin + direction * distance;
    }
}