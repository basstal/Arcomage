using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
// using GamePlay;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
// using NOAH.Core;
// using NOAH.Render;
// using Object = System.Object;
// using NOAH.Debug;
using System.Globalization;
using System.Text.RegularExpressions;
// using Localization;

namespace NOAH.UI
{

    public static class UIUtility
    {

        
        public static void ModifyParticleColor(ParticleSystem ps, Color color)
        {
            ParticleSystem.MainModule mainModule = ps.main;
            Color startColor = mainModule.startColor.color;
            startColor.r = color.r;
            startColor.g = color.g;
            startColor.b = color.b;
            startColor.a = color.a;
            mainModule.startColor = startColor;
        }
        
        public static Color GetColor(string htmlColor)
        {
            // if (!htmlColor.StartsWithOrdinal("#"))
            // {
            //     htmlColor = "#" + htmlColor;
            // }

            if (ColorUtility.TryParseHtmlString(htmlColor, out Color color))
                return color;
            return new Color(1, 1, 1, 1);
        }

        // public unsafe static Color GetColor32(int color32)
        // {
        //     return *(Color32*)&color32;
        // }

        public static Color GetColor(int r, int g, int b, float a = 1)
        {
            return new Color((float)r / 255, (float)g / 255, (float)b / 255, a);
        }

        public static string GetColor(Color c)
        {
            return ColorUtility.ToHtmlStringRGBA(c);
        }

        public static void SetEventHandler(UnityEvent @event, UnityAction callback)
        {
            if (@event != null)
            {
                @event.RemoveAllListeners();
                if (callback != null) @event.AddListener(callback);
            }
        }

        public static IEnumerator CaptureScreen()
        {
            yield return new WaitForEndOfFrame();
            GameObject obj = GameObject.Find("Plane");
            if (obj != null)
            {
                obj.transform.localScale = new Vector3(Screen.width * 1.0f / Screen.height, 1, 1);
                int width = Screen.width;
                int height = Screen.height;
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();
                obj.GetComponent<MeshRenderer>().material.mainTexture = tex;
            }
        }

        public static Vector2 WorldToUIPos(Camera worldCamera, Vector3 worldPos, RectTransform targetTransform)
        {
            Vector2 uiPosition = Vector2.zero;
            //var worldCamera = Camera.main;// CameraBase.Instance?.MainCamera;
            // var uiCamera = UIManager.Instance?.MainCamera;
            // if (worldCamera != null && uiCamera != null)
            // {
            //     RectTransformUtility.ScreenPointToLocalPointInRectangle(targetTransform, worldCamera.WorldToScreenPoint(worldPos), uiCamera, out uiPosition);
            // }
            return uiPosition;
        }
        
        public static Vector2 WorldToUIPos(Camera worldCamera,Camera uiCamera, Vector3 worldPos, RectTransform targetTransform)
        {
            Vector2 uiPosition = Vector2.zero;
            if (worldCamera != null && uiCamera != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(targetTransform, worldCamera.WorldToScreenPoint(worldPos), uiCamera, out uiPosition);
            }
            return uiPosition;
        }

        public static Vector3 UIPosToWorld(Camera worldCamera, RectTransform targetTransform, float z = 0)
        {
            Vector3 worldPos = Vector3.zero;
            // var uiCamera = UIManager.Instance?.MainCamera;
            // if (worldCamera != null && uiCamera != null)
            // {
            //     Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, targetTransform.position);
            //     screenPos.z = z - worldCamera.transform.position.z;
            //     worldPos = worldCamera.ScreenToWorldPoint(screenPos);
            // }
            return worldPos;
        }

        public static Vector3 ViewPortToUIPos(Vector3 vPos, RectTransform target)
        {
            Vector2 localPos = Vector2.zero;
            // var uiCamera = UIManager.Instance?.MainCamera;
            // if (uiCamera != null)
            // {
            //     Vector3 screenPos = uiCamera.ViewportToScreenPoint(vPos);
            //     RectTransformUtility.ScreenPointToLocalPointInRectangle(target, screenPos, uiCamera, out localPos);
            // }
            return localPos;
        }


        public static Type[] CSharpCallLua = new Type[]
        {
            typeof(DOGetter<float>), typeof(DOSetter<float>),
            typeof(DOGetter<Color>), typeof(DOSetter<Color>),
        };

        public static Tween DOTweenFloat(DOGetter<float> getter, DOSetter<float> setter, float endValue, float duration)
        {
            return DOTween.To(getter, setter, endValue, duration);
        }

        public static Tween DOTweenColor(DOGetter<Color> getter, DOSetter<Color> setter, Color endValue, float duration)
        {
            return DOTween.To(getter, setter, endValue, duration);
        }

        public static Tween DoTweenPosition(DOGetter<Vector3> getter, DOSetter<Vector3> setter, Vector3 endValue, float duration)
        {
            return DOTween.To(getter, setter, endValue, duration);
        }


        public static Tween DoTweenPosition(GameObject target, Vector3 endValue, float duration)
        {
            return DOTween.To(() => target.transform.position,
            (val) => target.transform.position = val, endValue, duration);
        }

        private static VertexHelper s_vertexHelper = new VertexHelper();
        private static Action<Graphic, VertexHelper> s_OnPopulateMeshDelegate;

        private static Action<Graphic, VertexHelper> OnPopulateMeshDelegate
        {
            get
            {
                if (s_OnPopulateMeshDelegate == null)
                {
                    var methodInfo = typeof(Graphic).GetMethod("OnPopulateMesh", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(VertexHelper) }, null);
                    s_OnPopulateMeshDelegate = methodInfo.CreateDelegate(typeof(Action<Graphic, VertexHelper>)) as Action<Graphic, VertexHelper>;
                }
                return s_OnPopulateMeshDelegate;
            }
        }

        public static void SetString(TextMeshProUGUI tmp, string key)
        {
            // tmp.text = LocalizationManager.Instance.GetString(key);
        }

        public static Mesh BakeUIMesh(GameObject root)
        {
            var graphic = root.GetComponentInChildren<Graphic>();
            var mesh = new Mesh();
            mesh.Clear();

            float l = 0, r = 0, b = 0, t = 0;
            OnPopulateMeshDelegate(graphic, s_vertexHelper);
            s_vertexHelper.FillMesh(mesh);

            //手算bounds
            for (int k = 0; k < mesh.vertexCount; k++)
            {
                var pos = mesh.vertices[k];
                if (k == 0)
                {
                    l = r = pos.x;
                    b = t = pos.y;
                }
                else
                {
                    l = Math.Min(l, pos.x);
                    r = Math.Max(r, pos.x);
                    b = Math.Min(b, pos.y);
                    t = Math.Max(t, pos.y);
                }
            }
            /*
            var bakedMesh = new Mesh();
            bakedMesh.Clear();
            bakedMesh.SetVertices(vertices);
            bakedMesh.SetTriangles(triangles, 0);
            bakedMesh.SetUVs(0, uvs0);
            bakedMesh.SetUVs(1, uvs1);
            bakedMesh.RecalculateBounds();*/

            var uvs1 = new List<Vector4>(mesh.vertexCount);
            float w = r - l;
            float h = t - b;
            for (int i = 0; i < mesh.vertexCount; i++)
            {
                Vector2 uv = Vector2.zero;
                var pos = mesh.vertices[i];
                if (w > 0) uv.x = (pos.x - l) / w;
                if (h > 0) uv.y = (pos.y - b) / h;
                uvs1.Add(uv);
            }
            mesh.SetUVs(1, uvs1);
            mesh.SetUVs(2, new Vector2[0]);
            mesh.SetUVs(3, new Vector2[0]);
            return mesh;
        }

        //缓存数字字符串，int转string每次都会构造一个新的字符串。
        public static Dictionary<int, string> CacheNumber = new Dictionary<int, string>(500);
        const int MAX_CHACHE_COUNT = 1024;
        public static string GetCacheString(int a)
        {
            if (CacheNumber.TryGetValue(a, out var s))
            {
                return s;
            }
            if (CacheNumber.Count >= MAX_CHACHE_COUNT)
            {
                for (var removed = 0; removed < MAX_CHACHE_COUNT / 2; removed++)
                {
                    foreach (var key in CacheNumber.Keys)
                    {
                        CacheNumber.Remove(key);
                        break;
                    }
                }
                // Debug.LogTool.LogWarning("Performance", $"Cached numbers are more than {MAX_CHACHE_COUNT}. Remove half of them.");
            }

            s = a.ToString();
            CacheNumber[a] = s;
            return s;
        }

        public static void SetGrayLerp(Graphic img, float lerp)
        {
            var material = img.GetInstanceMaterial();
            if (material)
            {
                if (!material.IsKeywordEnabled("_ENABLEGRAYSCALE_ON"))
                {
                    material.EnableKeyword("_ENABLEGRAYSCALE_ON");
                }
                material.SetFloat("_GrayscaleLerp", lerp);
            }
        }
        
        public static void SetGrayLerpSoftMask(Graphic img, float lerp)
        {
            // var cmpt = img.transform.GetComponent<SoftMasking.SoftMaskable>();
            // if(cmpt == null) return;
            var material = img.materialForRendering;
            if (material)
            {
                if (!material.IsKeywordEnabled("_ENABLEGRAYSCALE_ON"))
                {
                    material.EnableKeyword("_ENABLEGRAYSCALE_ON");
                }
                material.SetFloat("_GrayscaleLerp", lerp);
                img.SetAllDirty();
            }
        }

        public static void SetTintColor(Graphic img, Color col, float intensity)
        {
            var material = img.GetInstanceMaterial();
            if (material == null) return;
            // TintColor need _ENABLEGRAYSCALE_ON keywords???
            // if (!material.IsKeywordEnabled("_ENABLEGRAYSCALE_ON")) {
            //     material.EnableKeyword("_ENABLEGRAYSCALE_ON");
            // }
            var factor = Mathf.Pow(2, intensity);
            var color = new UnityEngine.Color(col.r * factor, col.g * factor, col.b * factor);
            // material.SetColor(ShaderID.TintColor, color);
        }
        
        public static void SetTintColor(Renderer renderer, Color col, float intensity)
        {
            var material = renderer.sharedMaterial;
            if (material == null) return;
            // TintColor need _ENABLEGRAYSCALE_ON keywords???
            // if (!material.IsKeywordEnabled("_ENABLEGRAYSCALE_ON")) {
            //     material.EnableKeyword("_ENABLEGRAYSCALE_ON");
            // }
            var factor = Mathf.Pow(2, intensity);
            var color = new UnityEngine.Color(col.r * factor, col.g * factor, col.b * factor,col.a);
            // material.SetColor(ShaderID.TintColor, color);
        }
        public static void ClearCacheString()
        {
            CacheNumber.Clear();
        }

        //播放动画的特殊需求：控制动态加载的特效
        // Animator 会保持对对象的持续控制，导致代码无法对控制对象进行任何修改
        // Animation 则在首次触发阶段会绑定控制对象，并且无法变更，而我们的特效又是回收制的，所以只能在每次播放时，奢侈地实例化一个clip，播完再销毁
        public static void PlayAnimationInstance(Animation anim, string animName, int templateCount)
        {
            if (anim == null) return;
            if (String.IsNullOrEmpty(animName)) return;

            if (anim.GetClipCount() > templateCount)
            {
                var cloneChip = anim.GetClip("Clone");
                anim.RemoveClip("Clone");
                // Util.Destroy(cloneChip);
            }

            var clipTemplate = anim.GetClip(animName);
            var tempClip = UnityEngine.Object.Instantiate(clipTemplate);
            anim.AddClip(tempClip, "Clone");
            anim.Play("Clone", PlayMode.StopAll);
        }
        
        public static float PlayAllTween(Transform root, string id, bool restart = false)
        {
            var tanims = root.GetComponentsInChildren<DOTweenAnimation>();
            bool noIdLimit = string.IsNullOrEmpty(id);
            var maxTime = 0f;
            foreach (var tanim in tanims)
            {
                if (noIdLimit || tanim.id == id)
                {
                    if (restart)
                    {
                        tanim.tween.Restart();
                    }
                    else
                    {
                        tanim.tween.Play();
                    }
                    maxTime = Math.Max(maxTime,tanim.delay + tanim.duration);
                }
            }
            return maxTime;
        }

        public static void PauseAllTween(Transform root, string id)
        {
            var tanims = root.GetComponentsInChildren<DOTweenAnimation>();
            bool noIdLimit = string.IsNullOrEmpty(id);
            foreach (var tanim in tanims)
            {
                if (noIdLimit || tanim.id == id)
                {
                    tanim.tween.Pause();
                }
            }
        }
        
        public static void StopAllTween(Transform root, string id, bool finish = false)
        {
            var tanims = root.GetComponentsInChildren<DOTweenAnimation>();
            bool noIdLimit = string.IsNullOrEmpty(id);
            foreach (var tanim in tanims)
            {
                if (noIdLimit || tanim.id == id)
                {
                    if (tanim.loops == 1)
                    {
                        tanim.tween.Complete();
                    }
                    else
                    {
                        tanim.tween.Goto(tanim.duration);
                    }
                }
            }

        }
        
        public static void RewindAllTween(Transform root, string id)
        {
            var tanims = root.GetComponentsInChildren<DOTweenAnimation>();
            bool noIdLimit = string.IsNullOrEmpty(id);
            foreach (var tanim in tanims)
            {
                if (noIdLimit || tanim.id == id)
                {
                    tanim.tween.Goto(0);
                }
            }
        }
        
        
        public static float FindTweenValueAtTime(DOTweenAnimation tween,float value, int index = 1)
        {
            if (tween == null) return -1f;
            var curve = tween.easeCurve;
            if (curve == null) return -1f;
            var count = 0;
            var start = 0;
            var finish = curve.keys.Length - 1;
            if (index < 0)
            {
                start = finish;
                finish = 0;
            }
        
            for (var i = start; i <= finish;)
            {
                var keyFrame = curve.keys[i];
                var val = Mathf.Lerp(tween.fromValueFloat, tween.endValueFloat, keyFrame.value);
                if(Mathf.Approximately(val,value))
                {
                    count++;
                    if (count == Math.Abs(index))
                    {
                        return keyFrame.time * tween.duration + tween.delay;
                    }
                }
                if (index < 0) i--;
                else i++;
            }
            return -1f;
        }

        // public static string CurLanguage
        // {
        //     get { return SettingsManager.Instance.MiscSettings.Current.Language; }
        // }

        public static bool TryGetCurFontScale(string textStyle, out float scale){
            scale = 1f;
            // var styleList = NOAH.UI.UIManager.Instance?.UIStyleSheet?.GetStyleTiers(textStyle);
            // var styleSettings = SettingsManager.Instance?.MiscSettings?.Current?.StyleSettings;
            // if (!styleSettings.TryGetValue(textStyle, out var index) || index >= styleList.Count) return false;
            // var styleStr = styleList[index].styleOpeningDefinition;
            // if (!float.TryParse(Regex.Replace(styleStr, @"[^\d]", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out scale)) return false;
            scale /= 100f;
            return true;
        }
    }
}
