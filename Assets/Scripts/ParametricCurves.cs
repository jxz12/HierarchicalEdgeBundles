using UnityEngine;
using System.Collections.Generic;

// based on https://www.cs.helsinki.fi/group/goa/mallinnus/curves/curves.html for variable names

namespace ParametricCurves
{
    public class CubicParametricCurve
    {
        // simple cubic curve takes a start, end, midpoint, and midtangent as parameters
        public static List<Vector3> CubicGradient(Vector3 P1, Vector3 P2, Vector3 P2Gradient, Vector3 P3, int numSegments) {
            float[] M_c = {-4f,  0,  -4f,  4f,
                            8f, -4f,  6f, -4f,
                           -5f,  4f, -2f,  1f,
                            1f,  0,   0,   0};

            float[] G_x = new float[4] { P1.x, P2.x, P2Gradient.x, P3.x };
            float[] G_y = new float[4] { P1.y, P2.y, P2Gradient.y, P3.y };
            float[] G_z = new float[4] { P1.z, P2.z, P2Gradient.z, P3.z };

            List<Vector3> segmentPoints = new List<Vector3>();
            for (int i = 0; i <= numSegments; i++) {
                float t = (float)i / numSegments;

                segmentPoints.Add( new Vector3(TMG(t, M_c, G_x), TMG(t, M_c, G_y), TMG(t, M_c, G_z)) );
            }
            return segmentPoints;
        }

         
        public static List<Vector3> Bezier(Vector3 P1, Vector3 P2, Vector3 P3, Vector3 P4, int numSegments) {
            float[] M_b = {-1f,  3f, -3f, 1f,
                            3f, -6f,  3f, 0,
                           -3f,  3f,  0,  0,
                            1f,  0,   0,  0};

            float[] G_x = new float[4] { P1.x, P2.x, P3.x, P4.x };
            float[] G_y = new float[4] { P1.y, P2.y, P3.y, P4.y };
            float[] G_z = new float[4] { P1.z, P2.z, P3.z, P4.z };

            List<Vector3> segmentPoints = new List<Vector3>();
            for (int i = 0; i <= numSegments; i++) {
                float t = (float)i / numSegments;

                segmentPoints.Add( new Vector3(TMG(t, M_b, G_x), TMG(t, M_b, G_y), TMG(t, M_b, G_z)) );
            }
            return segmentPoints;
        }

        public static List<Vector3> BSpline(Vector3 P1, Vector3 P2, Vector3 P3, Vector3 P4, int numSegments, bool includeLastPoint=true) {
            float[] M_bspline = {-1f/6f, 3f/6f, -3f/6f, 1f/6f,
                                  3f/6f, -6f/6f, 3f/6f, 0,
                                 -3f/6f,  0,     3f/6f, 0,
                                  1f/6f,  4f/6f, 1f/6f, 0};

            float[] G_x = new float[4] { P1.x, P2.x, P3.x, P4.x };
            float[] G_y = new float[4] { P1.y, P2.y, P3.y, P4.y };
            float[] G_z = new float[4] { P1.z, P2.z, P3.z, P4.z };

            List<Vector3> segmentPoints = new List<Vector3>();

            int n = includeLastPoint ? numSegments : numSegments-1;
            for (int i = 0; i <= n; i++) {
                float t = (float)i / numSegments;

                segmentPoints.Add(new Vector3(TMGUnrolled(t, M_bspline, G_x),
                                              TMGUnrolled(t, M_bspline, G_y),
                                              TMGUnrolled(t, M_bspline, G_z)));
            }
            return segmentPoints;
        }

        public static List<Vector3> PiecewiseBspline(List<Vector3> P, int segmentsPerSpline) {
            if (P.Count < 3)
                throw new System.Exception("not enough points to make a curved spline");

            List<Vector3> segmentPoints = new List<Vector3>();

            segmentPoints.AddRange(BSpline(P[0], P[0], P[0], P[1], segmentsPerSpline, false));
            segmentPoints.AddRange(BSpline(P[0], P[0], P[1], P[2], segmentsPerSpline, false));

            for (int i = 0; i < P.Count - 3; i++) {
                segmentPoints.AddRange(BSpline(P[i], P[i+1], P[i+2], P[i+3], segmentsPerSpline, false));
            }
            int n = P.Count - 1;
            segmentPoints.AddRange(BSpline(P[n-2], P[n-1], P[n], P[n], segmentsPerSpline, false));
            segmentPoints.AddRange(BSpline(P[n-1], P[n], P[n], P[n], segmentsPerSpline, true));

            return segmentPoints;
        }

        static float TMG(float t, float[] M, float[] G) {
            // do M*G first
            float[] MG = new float[4] { 0, 0, 0, 0 };
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 4; j++) {
                    MG[i] += M[(i * 4) + j] * G[j];
                }
            }

            float[] T = new float[4] { t * t * t, t * t, t, 1f };
            float result = 0;
            for (int i = 0; i < 4; i++) {
                result += T[i] * MG[i];
            }
            return result;
        }
        
        static float TMGUnrolled(float t, float[] M, float[] G) {
            float[] MG = new float[4];
            MG[0] = M[0]*G[0] + M[1]*G[1] + M[2]*G[2] +M[3]*G[3];
            MG[1] = M[4]*G[0] + M[5]*G[1] + M[6]*G[2] +M[7]*G[3];
            MG[2] = M[8]*G[0] + M[9]*G[1] + M[10]*G[2] +M[11]*G[3];
            MG[3] = M[12]*G[0] + M[13]*G[1] + M[14]*G[2] +M[15]*G[3];

            float[] T = new float[4] {t*t*t, t*t, t, 1f};
            return T[0]*MG[0] + T[1]*MG[1] + T[2]*MG[2] + T[3]*MG[3];
        }
    }
}