//————————————————————————————————————————————
//  OutlineEx.cs
//
//  Created by Chiyu Ren on 2018/9/12 23:03:51
//  Modify by zhenmu on 2019/3/26
//————————————————————————————————————————————
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;



namespace TooSimpleFramework.UI
{
    /// <summary>
    /// UGUI描边
    /// </summary>
    public class OutlineEx : BaseMeshEffect
    {
        public Color OutlineColor = Color.white;
        [Range(0, 8)]
        public int OutlineWidth = 0;
        public Shader Shader;

        public void SetColor(Color color)
        {
            OutlineColor = color;

            if (_material != null)
            {
                Destroy(_material);
                _material = null;
            }

            if (CheckShader())
            {
                this.SetParams();
                this._Refresh();
            }
        }

        private static List<UIVertex> m_VetexList = new List<UIVertex>();
        private Material _material;

        protected override void Awake()
        {
            base.Awake();

            if (CheckShader())
            {
                this.SetShaderChannels();
                this.SetParams();
                this._Refresh();
            }
        }


        bool CheckShader()
        {
            if (!enabled) return false;
            if (base.graphic == null)
            {
                Debug.Log("No Graphic Component !");
                return false;
            }

#if UNITY_ANDROID && UNITY_EDITOR
            Shader = Shader.Find("TSF Shaders/UI/OutlineEx");
#else
            if (Shader == null)
                Shader = Shader.Find("TSF Shaders/UI/OutlineEx");
#endif

            if (_material == null || _material.shader.name != Shader.name)
            {
                _material = new Material(Shader);
                base.graphic.material = _material;

                //Debug.Log("no material");
            }
            else
                _material = base.graphic.material;

            return true;
        }

        void SetParams()
        {
            if (base.graphic.material != null)
            {
                base.graphic.material.SetColor("_OutlineColor", OutlineColor);
                base.graphic.material.SetFloat("_OutlineWidth", OutlineWidth);
            }

        }

        void SetShaderChannels()
        {
            if (base.graphic.canvas)
            {
                var v1 = base.graphic.canvas.additionalShaderChannels;
                var v2 = AdditionalCanvasShaderChannels.TexCoord1;
                if ((v1 & v2) != v2)
                {
                    base.graphic.canvas.additionalShaderChannels |= v2;
                }
                v2 = AdditionalCanvasShaderChannels.TexCoord2;
                if ((v1 & v2) != v2)
                {
                    base.graphic.canvas.additionalShaderChannels |= v2;
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (CheckShader())
            {
                this.SetParams();
                this._Refresh();
            }
        }
#endif


        private void _Refresh()
        {
            base.graphic.SetVerticesDirty();
        }


        public override void ModifyMesh(VertexHelper vh)
        {
            vh.GetUIVertexStream(m_VetexList);

            this._ProcessVertices();

            vh.Clear();
            vh.AddUIVertexTriangleStream(m_VetexList);
        }


        private void _ProcessVertices()
        {
            for (int i = 0, count = m_VetexList.Count - 3; i <= count; i += 3)
            {
                var v1 = m_VetexList[i];
                var v2 = m_VetexList[i + 1];
                var v3 = m_VetexList[i + 2];
                // 计算原顶点坐标中心点
                //
                var minX = _Min(v1.position.x, v2.position.x, v3.position.x);
                var minY = _Min(v1.position.y, v2.position.y, v3.position.y);
                var maxX = _Max(v1.position.x, v2.position.x, v3.position.x);
                var maxY = _Max(v1.position.y, v2.position.y, v3.position.y);
                var posCenter = new Vector2(minX + maxX, minY + maxY) * 0.5f;
                // 计算原始顶点坐标和UV的方向
                //
                Vector2 triX, triY, uvX, uvY;
                Vector2 pos1 = v1.position;
                Vector2 pos2 = v2.position;
                Vector2 pos3 = v3.position;
                if (Mathf.Abs(Vector2.Dot((pos2 - pos1).normalized, Vector2.right))
                    > Mathf.Abs(Vector2.Dot((pos3 - pos2).normalized, Vector2.right)))
                {
                    triX = pos2 - pos1;
                    triY = pos3 - pos2;
                    uvX = v2.uv0 - v1.uv0;
                    uvY = v3.uv0 - v2.uv0;
                }
                else
                {
                    triX = pos3 - pos2;
                    triY = pos2 - pos1;
                    uvX = v3.uv0 - v2.uv0;
                    uvY = v2.uv0 - v1.uv0;
                }
                // 计算原始UV框
                var uvMin = _Min(v1.uv0, v2.uv0, v3.uv0);
                var uvMax = _Max(v1.uv0, v2.uv0, v3.uv0);
                //OutlineColor 和 OutlineWidth 也传入，避免出现不同的材质球
                //var col_rg = new Vector2(OutlineColor.r, OutlineColor.g);       //描边颜色 用uv3 和 tangent的 zw传递
                //var col_ba = new Vector4(0, 0, OutlineColor.b, OutlineColor.a);
                //var normal = new Vector3(0, 0, OutlineWidth);                   //描边的宽度 用normal的z传递

                // 为每个顶点设置新的Position和UV，并传入原始UV框
                v1 = _SetNewPosAndUV(v1, this.OutlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax);
                v2 = _SetNewPosAndUV(v2, this.OutlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax);
                v3 = _SetNewPosAndUV(v3, this.OutlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax);

                // 应用设置后的UIVertex
                //
                m_VetexList[i] = v1;
                m_VetexList[i + 1] = v2;
                m_VetexList[i + 2] = v3;
            }
        }


        private static UIVertex _SetNewPosAndUV(UIVertex pVertex, int pOutLineWidth,
            Vector2 pPosCenter,
            Vector2 pTriangleX, Vector2 pTriangleY,
            Vector2 pUVX, Vector2 pUVY,
            Vector2 pUVOriginMin, Vector2 pUVOriginMax)
        {
            // Position
            var pos = pVertex.position;
            var posXOffset = pos.x > pPosCenter.x ? pOutLineWidth : -pOutLineWidth;
            var posYOffset = pos.y > pPosCenter.y ? pOutLineWidth : -pOutLineWidth;
            pos.x += posXOffset;
            pos.y += posYOffset;
            pVertex.position = pos;
            // UV
            Vector2 uv = pVertex.uv0;
            uv += pUVX / pTriangleX.magnitude * posXOffset * (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1);
            uv += pUVY / pTriangleY.magnitude * posYOffset * (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1);
            pVertex.uv0 = uv;

            pVertex.uv1 = pUVOriginMin;     //uv1 uv2 可用  tangent  normal 在缩放情况 会有问题
            pVertex.uv2 = pUVOriginMax;

            return pVertex;
        }


        private static float _Min(float pA, float pB, float pC)
        {
            return Mathf.Min(Mathf.Min(pA, pB), pC);
        }


        private static float _Max(float pA, float pB, float pC)
        {
            return Mathf.Max(Mathf.Max(pA, pB), pC);
        }


        private static Vector2 _Min(Vector2 pA, Vector2 pB, Vector2 pC)
        {
            return new Vector2(_Min(pA.x, pB.x, pC.x), _Min(pA.y, pB.y, pC.y));
        }


        private static Vector2 _Max(Vector2 pA, Vector2 pB, Vector2 pC)
        {
            return new Vector2(_Max(pA.x, pB.x, pC.x), _Max(pA.y, pB.y, pC.y));
        }
    }
}