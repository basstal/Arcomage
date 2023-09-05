using UnityEngine;
using System.Collections.Generic;

namespace NOAH.UI
{
    class Triangulator
    {
        private List<Vector2> mPoints = new List<Vector2>();

        public Triangulator(Vector2[] points)
        {
            mPoints = new List<Vector2>(points);
        }

        public int[] Triangulate()
        {
            List<int> indices = new List<int>();
            int n = mPoints.Count;
            if (n < 3) return indices.ToArray();
            int[] circle = new int[n];
            if (Area() < 0) //顺时针
            {
                for (int i = 0; i < n; i++)
                {
                    circle[i] = i;
                }
            }
            else
            {
                for (int i = 0; i < n; i++)
                {
                    circle[i] = n - i - 1;
                }
            }

            int remainPoint = circle.Length;
            int testIndex = 0;
            int noValidTriangleTimes = 0;
            while (remainPoint >= 3)
            {
                int second = testIndex + 1 < remainPoint ? testIndex + 1 : testIndex + 1 - remainPoint;
                int third = testIndex + 2 < remainPoint ? testIndex + 2 : testIndex + 2 - remainPoint;

                int aIndex = circle[testIndex];
                int bIndex = circle[second];
                int cIndex = circle[third];

                var A = mPoints[aIndex];
                var B = mPoints[bIndex];
                var C = mPoints[cIndex];

                var ac = C - A;
                var ab = B - A;
                bool isRightOrder = ab.x * ac.y - ab.y * ac.x < 0;

                if (!isRightOrder)
                {
                    noValidTriangleTimes++;
                    if (noValidTriangleTimes == remainPoint)
                        break;
                    testIndex++;
                    if (testIndex >= remainPoint)
                        testIndex = 0;
                    continue;
                }

                bool pureTriangle = true;
                for (int j = 0; j < remainPoint; j++)
                {
                    int dIndex = circle[j];
                    if (dIndex == aIndex || dIndex == bIndex || dIndex == cIndex)
                        continue;
                    bool inside = InsideTriangle(mPoints[aIndex], mPoints[bIndex], mPoints[cIndex], mPoints[dIndex]);
                    if (inside)
                    {
                        pureTriangle = false;
                        break;
                    }
                }

                if (!pureTriangle)
                {
                    noValidTriangleTimes++;
                    if (noValidTriangleTimes == remainPoint)
                        break;
                    testIndex++;
                    if (testIndex >= remainPoint)
                        testIndex = 0;
                    continue;
                }

                noValidTriangleTimes = 0;
                indices.Add(aIndex);
                indices.Add(bIndex);
                indices.Add(cIndex);
                remainPoint--;
                for (int j = second; j < remainPoint; j++) //remove second point in triangle 
                {
                    circle[j] = circle[j + 1];
                }

                testIndex--;
                if (testIndex < 0)
                    testIndex = 0;
            }

            return indices.ToArray();
        }

        private float Area()
        {
            int n = mPoints.Count;
            float A = 0.0f;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                Vector2 pval = mPoints[p];
                Vector2 qval = mPoints[q];
                A += pval.x * qval.y - qval.x * pval.y;
            }

            return (A * 0.5f);
        }

        private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            Vector2 ap = P - A;
            float compareValue = 0.01f;
            if (Mathf.Abs(ap.x) < compareValue && Mathf.Abs(ap.y) < compareValue)
                return false;
            Vector2 bp = P - B;
            if (Mathf.Abs(bp.x) < compareValue && Mathf.Abs(bp.y) < compareValue)
                return false;
            Vector2 cp = P - C;
            if (Mathf.Abs(bp.x) < compareValue && Mathf.Abs(cp.y) < compareValue)
                return false;


            float x1, y1, x2, y2, crossZ;
            x1 = B.x - A.x;
            y1 = B.y - A.y;
            x2 = ap.x;
            y2 = ap.y;
            crossZ = x1 * y2 - y1 * x2;
            if (crossZ >= 0.0f)
                return false;

            x1 = C.x - B.x;
            y1 = C.y - B.y;
            x2 = bp.x;
            y2 = bp.y;
            crossZ = x1 * y2 - y1 * x2;
            if (crossZ >= 0.0f)
                return false;

            x1 = A.x - C.x;
            y1 = A.y - C.y;
            x2 = cp.x;
            y2 = cp.y;
            crossZ = x1 * y2 - y1 * x2;
            return crossZ < 0.0f;
        }


        // public int[] Triangulate()
        // {
        //     List<int> indices = new List<int>();

        //     int n = mPoints.Count;
        //     if (n < 3) return indices.ToArray();

        //     int[] V = new int[n];
        //     if (Area() > 0)
        //     {
        //         for (int v = 0; v < n; v++)
        //             V[v] = v;
        //     }
        //     else
        //     {
        //         for (int v = 0; v < n; v++)
        //             V[v] = (n - 1) - v;
        //     }

        //     int nv = n;
        //     int count = 2 * nv;
        //     for (int m = 0, v = nv - 1; nv > 2;)
        //     {
        //         if ((count--) <= 0)
        //             return indices.ToArray();

        //         int u = v;
        //         if (nv <= u)
        //             u = 0;
        //         v = u + 1;
        //         if (nv <= v)
        //             v = 0;
        //         int w = v + 1;
        //         if (nv <= w)
        //             w = 0;

        //         if (IsEligibleTriangle(u, v, w, nv, V))
        //         {
        //             int a, b, c, s, t;
        //             a = V[u];
        //             b = V[v];
        //             c = V[w];
        //             indices.Add(a);
        //             indices.Add(b);
        //             indices.Add(c);
        //             m++;
        //             for (s = v, t = v + 1; t < nv; s++, t++)
        //                 V[s] = V[t];
        //             nv--;
        //             count = 2 * nv;
        //         }
        //     }

        //     indices.Reverse();
        //     return indices.ToArray();
        // }

        // private bool IsEligibleTriangle(int u, int v, int w, int n, int[] V)
        // {
        //     int p;
        //     Vector2 A = mPoints[V[u]];
        //     Vector2 B = mPoints[V[v]];
        //     Vector2 C = mPoints[V[w]];
        //     // testify: point order 
        //     float abCROSSac = (B.x - A.x) * (C.y - A.y) - (B.y - A.y) * (C.x - A.x);
        //     if (Mathf.Epsilon > abCROSSac)
        //         return false;
        //     // testify: is there a point inside the triangle  
        //     for (p = 0; p < n; p++)
        //     {
        //         if ((p == u) || (p == v) || (p == w))
        //             continue;
        //         Vector2 P = mPoints[V[p]];
        //         if (InsideTriangle(A, B, C, P))
        //             return false;
        //     }
        //     return true;
        // }
    }
}