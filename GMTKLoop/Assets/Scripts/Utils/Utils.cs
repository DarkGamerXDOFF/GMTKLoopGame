using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{


    #region Position calculation
    public static List<Vector3> GetPositionListAround(Vector3 startMovePosition, float[] ringDistanceArray, int[] ringPostionCountArray)
    {
        List<Vector3> positionList = new List<Vector3>();
        positionList.Add(startMovePosition);
        for (int i = 0; i < ringDistanceArray.Length; i++)
        {
            positionList.AddRange(GetPositionListAround(startMovePosition, ringDistanceArray[i], ringPostionCountArray[i]));
        }

        return positionList;
    }

    public static List<Vector3> GetPositionListAround(Vector3 startMovePosition, float distance, int positionsCount)
    {
        List<Vector3> positionsList = new List<Vector3>();

        Vector3 axis = Vector3.up; // Default ring plane axis
        for (int i = 0; i < positionsCount; i++)
        {
            float angle = i * (360f / positionsCount);
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            Vector3 dir = ApplyRotationToVector(new Vector3(1, 0, 0), angle);
            Vector3 position = startMovePosition + dir * distance;
            positionsList.Add(position);
        }

        return positionsList;
    }

    public static Vector3 ApplyRotationToVector(Vector2 vec, float angle) => Quaternion.Euler(0, angle, 0) * vec;

    public static int CalculateRingAmount(int unitCount, int baseUnitCount)
    {
        if (unitCount <= 1)
            return 0; // No rings needed if there's only one unit

        int ringCount = 0;
        int remainingUnits = unitCount - 1; // Subtracting the center unit
        int unitsInRing = baseUnitCount; // The first ring starts with 6 units

        while (remainingUnits > 0)
        {
            ringCount++;
            remainingUnits -= unitsInRing;
            unitsInRing += baseUnitCount; // Each new ring has 6 more units than the previous one
        }

        return ringCount;
    }

    public static void CalcRingSizeAndAmount(int ringCount, out float[] ringDistances, out int[] unitsPerRing, int baseUnitCount, float baseRingDistance)
    {
        ringDistances = new float[ringCount];
        unitsPerRing = new int[ringCount];

        for (int i = 0; i < ringCount; i++)
        {
            ringDistances[i] = baseRingDistance * (i + 1);
            unitsPerRing[i] = baseUnitCount * (i + 1);
        }
    }
    #endregion


}
